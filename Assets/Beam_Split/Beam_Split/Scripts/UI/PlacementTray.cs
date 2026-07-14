using BeamSplit.Data;
using BeamSplit.Gameplay;
using BeamSplit.Gameplay.Powerups;
using UnityEngine;
using UnityEngine.UI;

namespace BeamSplit.UI
{
    /// <summary>
    /// 5 mode buttons (Mirror/Red/Blue/Yellow/Erase) + 4 powerup buttons. Thin wiring only —
    /// business logic lives in PlacementController / PowerupController.
    /// </summary>
    public class PlacementTray : MonoBehaviour
    {
        [System.Serializable]
        public class ModeButtonEntry
        {
            public Button button;
            public GameObject selectedOutline;
            public PlacementController.PlacementMode mode;
        }

        [SerializeField] private PlacementController placementController;
        [SerializeField] private PowerupController powerupController;

        [SerializeField] private ModeButtonEntry[] modeButtons;

        [SerializeField] private Button addTimeButton;
        [SerializeField] private Button addMovesButton;
        [SerializeField] private Button hintButton;
        [SerializeField] private Button undoButton;

        [SerializeField] private GameObject addTimeButtonRoot;
        [SerializeField] private GameObject addMovesButtonRoot;

        private void Awake()
        {
            foreach (var entry in modeButtons)
            {
                if (entry.button == null)
                {
                    continue;
                }

                var mode = entry.mode;
                entry.button.onClick.AddListener(() => SelectMode(mode));
            }

            if (addTimeButton != null) addTimeButton.onClick.AddListener(() => powerupController.TryUseAddTime());
            if (addMovesButton != null) addMovesButton.onClick.AddListener(() => powerupController.TryUseAddMoves());
            if (hintButton != null) hintButton.onClick.AddListener(() => powerupController.TryUseHint());
            if (undoButton != null) undoButton.onClick.AddListener(() => powerupController.TryUseUndo());
        }

        /// <summary>Decided once at level start: +Time shows on timed levels, +Moves on the inverse.</summary>
        public void Configure(LevelData level)
        {
            bool isTimeLimit = level.objectiveType == ObjectiveType.TimeLimit;

            if (addTimeButtonRoot != null) addTimeButtonRoot.SetActive(isTimeLimit);
            if (addMovesButtonRoot != null) addMovesButtonRoot.SetActive(!isTimeLimit);

            RefreshSelectedOutline();
        }

        private void SelectMode(PlacementController.PlacementMode mode)
        {
            placementController.SetMode(mode);
            RefreshSelectedOutline();
        }

        private void RefreshSelectedOutline()
        {
            var current = placementController.CurrentMode;
            foreach (var entry in modeButtons)
            {
                if (entry.selectedOutline != null)
                {
                    entry.selectedOutline.SetActive(entry.mode == current);
                }
            }
        }

        private void Update()
        {
            // Cheap correctness check for keyboard-driven mode changes (Editor convenience);
            // avoids the tray's outline going stale when digit keys are used instead of taps.
            RefreshSelectedOutline();
        }
    }
}
