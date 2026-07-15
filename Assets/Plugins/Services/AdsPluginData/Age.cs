using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Age : MonoBehaviour
{
    [SerializeField] Slider age;
    [SerializeField] Text ageText;
    private void Awake()
    {
    }
    private void Start()
    {
        Setup();
    }
    public void Setup()
    {
        if (PlayerPrefs.HasKey("UserAge"))
        {
            Time.timeScale = 1;
            AdsManager.Instance.InitializeAds();
            Destroy(this.gameObject);
        }
        else
        {
            this.gameObject.SetActive(true);
            Time.timeScale = 0;
        }
        age.onValueChanged.AddListener(delegate { SetText(); });
    }
    private void SetText() => ageText.text = age.value.ToString();

    public void Confirm()
    {
        if (age.value > 0)
        {
            Time.timeScale = 1;
            int userAge = (int)age.value;
            PlayerPrefs.SetInt("UserAge", userAge);
            AdsManager.Instance.InitializeAds();
            gameObject.SetActive(false);
        }
    }
}
