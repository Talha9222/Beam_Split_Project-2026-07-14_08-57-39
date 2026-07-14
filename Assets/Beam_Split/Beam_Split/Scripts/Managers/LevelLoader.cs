using BeamSplit.Data;
using BeamSplit.Gameplay;
using BeamSplit.Gameplay.Economy;
using BeamSplit.Gameplay.Objective;
using BeamSplit.Gameplay.Powerups;
using BeamSplit.UI;
using UnityEngine;

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

        private LevelState state;

        private void Start()
        {
            state = LevelState.LoadingData;

            if (currentLevel == null || gridManager == null || beamSimulator == null)
            {
                Debug.LogError("[LevelLoader] Missing required references; cannot load level.");
                return;
            }

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

            SpawnEmitters();
            SpawnReceivers();
            SpawnWalls();

            beamSimulator.Recalculate();

            state = LevelState.ShowingObjectives;
            objectivePanel.OnClosed += HandleObjectivesClosed;
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
        }

        private void SpawnEmitters()
        {
            if (emitterPrefab == null)
            {
                return;
            }

            foreach (var data in currentLevel.emitters)
            {
                var emitter = Instantiate(emitterPrefab, gridManager.GetWorldPosition(data.gridPosition), Quaternion.identity);
                emitter.Configure(data.gridPosition, data.initialDirection);
                beamSimulator.RegisterEmitter(emitter);
            }
        }

        private void SpawnReceivers()
        {
            if (receiverPrefab == null)
            {
                return;
            }

            foreach (var data in currentLevel.receivers)
            {
                var receiver = Instantiate(receiverPrefab, gridManager.GetWorldPosition(data.gridPosition), Quaternion.identity);
                receiver.Configure(data.gridPosition, data.requiredColor);
                beamSimulator.RegisterReceiver(receiver);
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

            int reward = currentLevel.coinReward;
            economyManager.Award(reward);

            winPanel.OnNextLevelClicked += OnNextLevelStub;
            winPanel.OnMenuClicked += OnMenuStub;
            winPanel.Show(reward);
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

            losePanel.OnMenuClicked += OnMenuStub;
            losePanel.Show(reason);
        }

        public bool IsPlaying => state == LevelState.Playing;

        private void OnMenuStub()
        {
            Debug.Log("[LevelLoader] Menu/Next Level not implemented - no menu/level-select scene exists yet (out of scope this phase).");
        }

        private void OnNextLevelStub()
        {
            Debug.Log("[LevelLoader] Menu/Next Level not implemented - no menu/level-select scene exists yet (out of scope this phase).");
        }
    }
}
