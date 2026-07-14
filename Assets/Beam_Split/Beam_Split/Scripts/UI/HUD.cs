using BeamSplit.Data;
using BeamSplit.Gameplay.Economy;
using BeamSplit.Gameplay.Objective;
using BeamSplit.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeamSplit.UI
{
    /// <summary>
    /// Always-visible during Play. Subscribes to EconomyManager/ObjectiveController events;
    /// thin wire-and-format component, no business logic.
    /// </summary>
    public class HUD : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text levelNumberText;
        [SerializeField] private TMP_Text coinsText;
        [SerializeField] private TMP_Text objectiveLiveValueText;
        [SerializeField] private Button pauseButton;
        [SerializeField] private PauseMenu pauseMenu;

        private EconomyManager economyManager;
        private ObjectiveController objectiveController;
        private ObjectiveType objectiveType;

        private void Awake()
        {
            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(OpenPause);
            }

            SetVisible(false);
        }

        public void Configure(LevelData level, EconomyManager economy, ObjectiveController objController)
        {
            Unsubscribe();

            economyManager = economy;
            objectiveController = objController;
            objectiveType = level.objectiveType;

            if (levelNumberText != null)
            {
                levelNumberText.text = $"Level {level.levelNumber}";
            }

            if (economyManager != null)
            {
                economyManager.OnCoinsChanged += HandleCoinsChanged;
                HandleCoinsChanged(economyManager.Coins);
            }

            if (objectiveController != null)
            {
                objectiveController.OnMovesChanged += HandleMovesChanged;
                objectiveController.OnTimeChanged += HandleTimeChanged;
            }
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            if (economyManager != null)
            {
                economyManager.OnCoinsChanged -= HandleCoinsChanged;
            }

            if (objectiveController != null)
            {
                objectiveController.OnMovesChanged -= HandleMovesChanged;
                objectiveController.OnTimeChanged -= HandleTimeChanged;
            }
        }

        public void Show()
        {
            SetVisible(true);
        }

        public void Hide()
        {
            SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        private void HandleCoinsChanged(int coins)
        {
            if (coinsText != null)
            {
                coinsText.text = coins.ToString();
            }
        }

        private void HandleMovesChanged(int moves)
        {
            if (objectiveType == ObjectiveType.MoveLimit && objectiveLiveValueText != null)
            {
                objectiveLiveValueText.text = $"Moves: {moves}";
            }
        }

        private void HandleTimeChanged(float seconds)
        {
            if (objectiveType == ObjectiveType.TimeLimit && objectiveLiveValueText != null)
            {
                objectiveLiveValueText.text = FormatTime(seconds);
            }
        }

        private static string FormatTime(float seconds)
        {
            int totalSeconds = Mathf.Max(0, Mathf.CeilToInt(seconds));
            int minutes = totalSeconds / 60;
            int secs = totalSeconds % 60;
            return $"{minutes:00}:{secs:00}";
        }

        private void OpenPause()
        {
            pauseMenu?.Show();
        }
    }
}
