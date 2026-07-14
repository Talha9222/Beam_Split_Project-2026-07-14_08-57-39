using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeamSplit.UI
{
    public class WinPanel : PanelBase
    {
        [SerializeField] private TMP_Text coinsEarnedText;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button menuButton;

        public event System.Action OnNextLevelClicked;
        public event System.Action OnMenuClicked;

        protected override void Awake()
        {
            base.Awake();

            if (nextLevelButton != null)
            {
                nextLevelButton.onClick.AddListener(() => OnNextLevelClicked?.Invoke());
            }

            if (menuButton != null)
            {
                menuButton.onClick.AddListener(() => OnMenuClicked?.Invoke());
            }
        }

        public void Show(int coinsEarned)
        {
            if (coinsEarnedText != null)
            {
                coinsEarnedText.text = $"+{coinsEarned} coins";
            }

            Show();
        }
    }
}
