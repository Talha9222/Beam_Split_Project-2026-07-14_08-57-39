using BeamSplit.Data;
using BeamSplit.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BeamSplit.UI
{
    public class LosePanel : PanelBase
    {
        [SerializeField] private TMP_Text reasonText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button menuButton;

        private LevelData levelToRetry;

        public event System.Action OnMenuClicked;

        protected override void Awake()
        {
            base.Awake();

            if (retryButton != null)
            {
                retryButton.onClick.AddListener(Retry);
            }

            if (menuButton != null)
            {
                menuButton.onClick.AddListener(() => OnMenuClicked?.Invoke());
            }
        }

        public void Show(string reason, LevelData level)
        {
            levelToRetry = level;

            if (reasonText != null)
            {
                reasonText.text = reason;
            }

            Show();
        }

        private void Retry()
        {
            LevelProgress.PendingLevel = levelToRetry;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
