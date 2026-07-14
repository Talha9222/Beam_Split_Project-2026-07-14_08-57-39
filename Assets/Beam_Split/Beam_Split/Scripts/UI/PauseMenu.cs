using BeamSplit.Gameplay;
using BeamSplit.Gameplay.Objective;
using UnityEngine;
using UnityEngine.UI;

namespace BeamSplit.UI
{
    /// <summary>
    /// Pausing means ONLY blocking placement input and suspending the objective clock, NOT
    /// Time.timeScale = 0 — a global timescale change would also stall UI animations meant
    /// to keep running (e.g. toast fades).
    /// </summary>
    public class PauseMenu : PanelBase
    {
        [SerializeField] private PlacementController placementController;
        [SerializeField] private ObjectiveController objectiveController;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button menuButton;

        public event System.Action OnMenuClicked;

        /// <summary>Set by LevelLoader so Resume doesn't race a win/lose that happened while paused.</summary>
        public System.Func<bool> IsLevelPlaying;

        protected override void Awake()
        {
            base.Awake();

            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(Resume);
            }

            if (menuButton != null)
            {
                menuButton.onClick.AddListener(() => OnMenuClicked?.Invoke());
            }
        }

        public void Configure(PlacementController controller, ObjectiveController objController)
        {
            placementController = controller;
            objectiveController = objController;
        }

        public override void Show()
        {
            base.Show();
            placementController?.SetInputEnabled(false);
            objectiveController?.PauseRunning();
        }

        public void Resume()
        {
            Hide();
            if (IsLevelPlaying == null || IsLevelPlaying())
            {
                placementController?.SetInputEnabled(true);
                objectiveController?.ResumeRunning();
            }
        }
    }
}
