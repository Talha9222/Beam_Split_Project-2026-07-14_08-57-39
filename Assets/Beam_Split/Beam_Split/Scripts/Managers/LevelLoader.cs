using BeamSplit.Data;
using BeamSplit.Gameplay;
using BeamSplit.Gameplay.Economy;
using BeamSplit.Gameplay.Objective;
using BeamSplit.Gameplay.Powerups;
using BeamSplit.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeamSplit.Managers
{
    /// <summary>
    /// Single test level, no level-select. State machine:
    /// LoadingData -> ShowingObjectives -> ShowingTutorial -> Playing -> Won | Lost.
    /// See CLAUDE.md / plan Section 3 for the full sequencing rationale, especially the
    /// win/lose race-condition ordering inside PlacementController.PlaceCurrentMode.
    /// </summary>
    public class LevelLoader : MonoBehaviour
    {
        private enum LevelState { LoadingData, ShowingObjectives, ShowingTutorial, Playing, Won, Lost }

        [SerializeField] private LevelData currentLevel;
        [SerializeField] private LevelData[] levels;
        [SerializeField] private GridManager gridManager;
        [SerializeField] private BeamSimulator beamSimulator;
        [SerializeField] private Emitter emitterPrefab;
        [SerializeField] private Receiver receiverPrefab;
        [SerializeField] private GameObject wallPrefab;

        [SerializeField] private ObjectiveController objectiveController;
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private PlacementController placementController;
        [SerializeField] private PowerupController powerupController;
        [SerializeField] private HUD hud;
        [SerializeField] private PlacementTray placementTray;

        [SerializeField] private ObjectivePanel objectivePanel;
        [SerializeField] private TutorialPanel tutorialPanel;
        [SerializeField] private WinPanel winPanel;
        [SerializeField] private LosePanel losePanel;
        [SerializeField] private PauseMenu pauseMenu;
        [SerializeField] private CoinRewardEffect coinRewardEffect;

        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private LevelState state;

        private void Start()
        {
            state = LevelState.LoadingData;

            if (LevelProgress.PendingLevel != null)
            {
                currentLevel = LevelProgress.PendingLevel;
                LevelProgress.PendingLevel = null;
            }
            else if (levels != null && levels.Length > 0)
            {
                // No same-session hand-off (Retry/Next Level both set PendingLevel) - this is
                // a fresh scene entry (app launch or Main Menu -> Play), so resume from the
                // last level saved to PlayerPrefs instead of always restarting at Level 1.
                int savedIndex = SaveManager.Load().currentLevelIndex;
                if (savedIndex >= 0 && savedIndex < levels.Length)
                {
                    currentLevel = levels[savedIndex];
                }
            }

            if (currentLevel == null || gridManager == null || beamSimulator == null)
            {
                Debug.LogError("[LevelLoader] Missing required references; cannot load level.");
                return;
            }

            PersistCurrentLevel();

            gridManager.Configure(currentLevel.gridWidth, currentLevel.gridHeight);
            beamSimulator.Configure(gridManager, currentLevel);
            beamSimulator.OnAllReceiversActive += HandleWin;

            objectiveController.Configure(currentLevel);
            objectiveController.OnObjectiveFailed += HandleLoseFromObjective;

            placementController.SetInputEnabled(false);
            placementController.Configure(gridManager, beamSimulator, objectiveController);

            powerupController?.Configure(currentLevel);
            placementTray?.Configure(currentLevel);

            hud.Configure(currentLevel, economyManager, objectiveController);

            if (pauseMenu != null)
            {
                pauseMenu.IsLevelPlaying = () => IsPlaying;
                pauseMenu.OnMenuClicked += GoToMainMenu;
            }

            SpawnEmitters();
            SpawnReceivers();
            SpawnWalls();

            beamSimulator.Recalculate();

            state = LevelState.ShowingObjectives;
            objectivePanel.OnClosed += HandleObjectivesClosed;
            Debug.Log($"[LevelLoader] Showing objective panel with description: \"{currentLevel.objectiveDescription}\"");
            objectivePanel.Show(currentLevel.objectiveDescription);
        }

        private void OnDestroy()
        {
            if (beamSimulator != null)
            {
                beamSimulator.OnAllReceiversActive -= HandleWin;
            }

            if (objectiveController != null)
            {
                objectiveController.OnObjectiveFailed -= HandleLoseFromObjective;
            }

            if (objectivePanel != null)
            {
                objectivePanel.OnClosed -= HandleObjectivesClosed;
            }

            if (tutorialPanel != null)
            {
                tutorialPanel.OnClosed -= HandleTutorialClosed;
            }

            if (pauseMenu != null)
            {
                pauseMenu.OnMenuClicked -= GoToMainMenu;
            }
        }

        private void PersistCurrentLevel()
        {
            if (levels == null)
            {
                return;
            }

            int index = System.Array.IndexOf(levels, currentLevel);
            if (index < 0)
            {
                return;
            }

            var save = SaveManager.Load();
            save.currentLevelIndex = index;
            save.schemaVersion = 1;
            SaveManager.Save(save);
        }

        private void SpawnEmitters()
        {
            if (emitterPrefab == null)
            {
                Debug.LogError("[LevelLoader] SpawnEmitters: emitterPrefab is not assigned; skipping spawn.");
                return;
            }

            Debug.Log($"[LevelLoader] SpawnEmitters: spawning {currentLevel.emitters.Count} emitter(s).");
            foreach (var data in currentLevel.emitters)
            {
                var worldPos = gridManager.GetWorldPosition(data.gridPosition);
                var emitter = Instantiate(emitterPrefab, worldPos, Quaternion.identity);
                emitter.Configure(data.gridPosition, data.initialDirection);
                beamSimulator.RegisterEmitter(emitter);
                Debug.Log($"[LevelLoader] Spawned emitter at grid {data.gridPosition} -> world {worldPos}, instance={emitter.name}");
            }
        }

        private void SpawnReceivers()
        {
            if (receiverPrefab == null)
            {
                Debug.LogError("[LevelLoader] SpawnReceivers: receiverPrefab is not assigned; skipping spawn.");
                return;
            }

            Debug.Log($"[LevelLoader] SpawnReceivers: spawning {currentLevel.receivers.Count} receiver(s).");
            foreach (var data in currentLevel.receivers)
            {
                var worldPos = gridManager.GetWorldPosition(data.gridPosition);
                var receiver = Instantiate(receiverPrefab, worldPos, Quaternion.identity);
                receiver.Configure(data.gridPosition, data.requiredColor);
                beamSimulator.RegisterReceiver(receiver);
                Debug.Log($"[LevelLoader] Spawned receiver at grid {data.gridPosition} -> world {worldPos}, requiredColor={data.requiredColor}, instance={receiver.name}");
            }
        }

        private void SpawnWalls()
        {
            if (wallPrefab == null)
            {
                return;
            }

            foreach (var wallCell in currentLevel.walls)
            {
                Instantiate(wallPrefab, gridManager.GetWorldPosition(wallCell), Quaternion.identity);
            }
        }

        private void HandleObjectivesClosed()
        {
            objectivePanel.OnClosed -= HandleObjectivesClosed;
            state = LevelState.ShowingTutorial;
            tutorialPanel.OnClosed += HandleTutorialClosed;
            tutorialPanel.Show(currentLevel.tutorialText);
        }

        private void HandleTutorialClosed()
        {
            tutorialPanel.OnClosed -= HandleTutorialClosed;
            state = LevelState.Playing;
            placementController.SetInputEnabled(true);
            objectiveController.BeginRunning();
            hud.Show();
        }

        private void HandleWin()
        {
            if (state != LevelState.Playing)
            {
                return; // guard: ignore late-arriving events after already won/lost
            }

            state = LevelState.Won;
            placementController.SetInputEnabled(false);
            objectiveController.StopRunning();
            AudioManager.Instance?.PlayWin();

            int reward = currentLevel.coinReward;

            winPanel.OnNextLevelClicked += OnNextLevelClicked;
            winPanel.OnMenuClicked += GoToMainMenu;

            // Coin fly juice ticks the HUD counter up coin-by-coin instead of jumping
            // straight to the total; WinPanel only appears once every coin has landed.
            // Degrades to a single instant award + immediate WinPanel if not wired yet
            // (coinRewardEffect itself also degrades gracefully if coinImagePrefab is unset).
            if (coinRewardEffect != null)
            {
                StartCoroutine(coinRewardEffect.PlayCoinReward(reward, economyManager.Award, () => winPanel.Show(reward)));
            }
            else
            {
                economyManager.Award(reward);
                winPanel.Show(reward);
            }
        }

        private void HandleLoseFromObjective()
        {
            string reason = currentLevel.objectiveType == ObjectiveType.TimeLimit ? "Out of time" : "Out of moves";
            HandleLose(reason);
        }

        private void HandleLose(string reason)
        {
            if (state != LevelState.Playing)
            {
                return;
            }

            state = LevelState.Lost;
            placementController.SetInputEnabled(false);
            objectiveController.StopRunning();
            AudioManager.Instance?.PlayLose();

            losePanel.OnMenuClicked += GoToMainMenu;
            losePanel.Show(reason, currentLevel);
        }

        public bool IsPlaying => state == LevelState.Playing;

        private void GoToMainMenu()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }

        /// <summary>
        /// Shows a test interstitial (if AdsManager is present) before advancing; falls
        /// through immediately if not, so testing CoreSimTest.unity directly without going
        /// through SplashScreen first still works.
        /// </summary>
        private void OnNextLevelClicked()
        {
            var ads = AdsManager.Instance != null ? AdsManager.Instance : FindObjectOfType<AdsManager>();
            if (ads != null)
            {
                ads.ShowInterstitialAd(AdvanceToNextLevel);
            }
            else
            {
                AdvanceToNextLevel();
            }
        }

        /// <summary>
        /// Advances to the next entry in the levels array (by reloading this scene with
        /// LevelProgress.PendingLevel set - see Start()); goes to the main menu once past
        /// the last level.
        /// </summary>
        private void AdvanceToNextLevel()
        {
            int index = levels != null ? System.Array.IndexOf(levels, currentLevel) : -1;
            if (index >= 0 && index + 1 < levels.Length)
            {
                LevelProgress.PendingLevel = levels[index + 1];
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else
            {
                GoToMainMenu();
            }
        }
    }
}
