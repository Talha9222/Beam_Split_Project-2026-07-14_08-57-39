using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeamSplit.Managers
{
    /// <summary>
    /// Scene-resident singleton, static-Instance pattern like GridManager/EconomyManager,
    /// but additionally DontDestroyOnLoad since it must survive the MainMenu -> CoreSimTest
    /// scene load (and back). Placed in MainMenu.unity (the first scene loaded); guards
    /// against a duplicate spawning if MainMenu is reloaded later (e.g. via a Menu button)
    /// while one already persists.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Music")]
        [SerializeField] private AudioClip mainMenuMusic;
        [SerializeField] private AudioClip gameplayMusic;

        [Header("SFX")]
        [SerializeField] private AudioClip buttonClickClip;
        [SerializeField] private AudioClip placeClip;
        [SerializeField] private AudioClip eraseClip;
        [SerializeField] private AudioClip winClip;
        [SerializeField] private AudioClip loseClip;
        [SerializeField] private AudioClip coinClip;
        [SerializeField] private AudioClip powerupClip;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void Start()
        {
            PlayMusicForScene(SceneManager.GetActiveScene().name);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
                Instance = null;
            }
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            PlayMusicForScene(scene.name);
        }

        private void PlayMusicForScene(string sceneName)
        {
            PlayMusic(sceneName == "MainMenu" ? mainMenuMusic : gameplayMusic);
        }

        public void PlayMusic(AudioClip clip)
        {
            if (musicSource == null || clip == null || musicSource.clip == clip)
            {
                return;
            }

            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }

        public void PlaySfx(AudioClip clip)
        {
            if (sfxSource == null || clip == null)
            {
                return;
            }

            sfxSource.PlayOneShot(clip);
        }

        public void PlayButtonClick() => PlaySfx(buttonClickClip);
        public void PlayPlace() => PlaySfx(placeClip);
        public void PlayErase() => PlaySfx(eraseClip);
        public void PlayWin() => PlaySfx(winClip);
        public void PlayLose() => PlaySfx(loseClip);
        public void PlayCoin() => PlaySfx(coinClip);
        public void PlayPowerup() => PlaySfx(powerupClip);
    }
}
