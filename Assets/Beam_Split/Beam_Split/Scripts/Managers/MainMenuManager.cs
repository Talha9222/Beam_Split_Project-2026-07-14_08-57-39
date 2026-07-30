using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BeamSplit.Managers
{
    /// <summary>
    /// Scene-resident singleton for MainMenu.unity. Play loads the first playable level
    /// scene (CoreSimTest — no level-select scene exists yet, see CLAUDE.md "Build status");
    /// Quit calls Application.Quit(), which is a no-op in the Editor, so Play Mode must be
    /// stopped manually there.
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private string playSceneName = "CoreSimTest";

        private void Awake()
        {
            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }
        }

        private void OnPlayClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            SceneManager.LoadScene(playSceneName);
        }

        private void OnQuitClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
