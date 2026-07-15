using System;
using UnityEngine;
using UnityEngine.Rendering;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance;
    public AdmobManager admobManager;
    public bool adActiveOnChild = false;
    public int childAge=13;


    private float appLaunchTime;
    private bool isInitialized = false;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            appLaunchTime = Time.unscaledTime;
            DontDestroyOnLoad(this);
        }else
        {
            Destroy(gameObject);
        }

    }
    public void InitializeAds()
    {
        
        admobManager.childAge = PlayerPrefs.GetInt("UserAge");
        if ( adActiveOnChild || PlayerPrefs.GetInt("UserAge") > childAge )
        {
            admobManager.SetConfiguration();
            isInitialized = true;
        }
    }

    public void ShowBanner()
    {
        if (!adActiveOnChild && PlayerPrefs.GetInt("UserAge") < childAge) return;
        admobManager.AdmobBannarLoadAd();
    }

    public void ShowInterstitialAd(Action callBack)
    {
        if (!adActiveOnChild && PlayerPrefs.GetInt("UserAge") < childAge)
        {
            callBack();
        }
        else
        {
            admobManager.AdmobShowInterstitialAd(callBack);
        }
    }

    public void ShowRewardedAd(Action callBack1,Action CallBack2)
    {
        //if (!adActiveOnChild && PlayerPrefs.GetInt("UserAge") < childAge)
        //{
        //    callBack1();
        //}
        //else
        //{
        if (isInitialized)
        {
            if (admobManager.AdmobIsRewardedVideoAvailble())
            {
                admobManager.AdmobShowRewardedAd(callBack1);
            }
            else
            {
                CallBack2();
            }
        }else
        {
            CallBack2();
        }
        // }
    }


    public float GetAppLaunchTime()
    {
        return Time.unscaledTime -  appLaunchTime;
    }
}
