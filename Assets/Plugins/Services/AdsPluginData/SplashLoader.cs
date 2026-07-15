using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class SplashLoader : MonoBehaviour
{
    public GameObject SceneLoading;
    public Image fillImage;
    public float timeToLoad = 0.8f;

    public bool splashScreen = true;
    private void Start()
    {
        if (splashScreen)
        {
            LoadScene_MM();
        }
    }

    private void LoadScene_MM()
    {
        fillImage.DOFillAmount(1, timeToLoad).OnComplete(() =>
        {
            SceneManager.LoadScene(1);
        });
    }

    public void LoadScene_Scene(int index)
    {
        SceneLoading.SetActive(true);
        fillImage.DOFillAmount(1, timeToLoad).OnComplete(() =>
        {
            SceneManager.LoadScene(index);
        });
    }
}
