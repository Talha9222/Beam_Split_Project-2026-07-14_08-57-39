using GoogleMobileAds.Api;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AdmobManager : MonoBehaviour
{
    // public static AdsManager Instance;
    [Header("18+ AdIds")]
    public string bannarId18Plus;
    public string interstatialId18Plus;
    public string rewardedId18Plus;
    public string rewardedInterstatialId18Plus;
    public string appopenAdId18Plus;

    [Header("18 Below AdIds")]
    public string bannarId18Below;
    public string interstatialId18Below;
    public string rewardedId18Below;
    public string rewardedInterstatialId18Below;
    public string appopenAdId18Below;



    private string admobBannarUnitId;
    private string admobInterstitialadUnitId;
    private string admobRewardedAdUnitId;
    private string rewardedInterstitialAdUnitId;
    string appOpemUnitId;

    //Admob Reference
    BannerView admobBannarView;
    InterstitialAd _interstitialAd;
    RewardedAd _rewardedAd;
    RewardedInterstitialAd _rewardedInterstitialAd;
    private AppOpenAd appOpenAd;
    private Action _onInterstitialClosed; // store callback
    private Action _onRewardedClosed; // store callback

    public int childAge;
    /// <summary>
    /// Shows the app open ad.
    /// </summary>
    /// 

    //public static AdmobManager Instance;

    //private void Awake()
    //{
    //    if (Instance == null)
    //    {
    //        Instance = this;
    //        DontDestroyOnLoad(this);
    //    }
    //}
    //public void Start()
    //{

    //}


    public void InitializeGoogleMobileAds()
    {
        // Initialize the Mobile Ads SDK.
        MobileAds.Initialize((initStatus) =>
        {
            Dictionary<string, AdapterStatus> map = initStatus.getAdapterStatusMap();
            foreach (KeyValuePair<string, AdapterStatus> keyValuePair in map)
            {
                string className = keyValuePair.Key;
                AdapterStatus status = keyValuePair.Value;
                switch (status.InitializationState)
                {
                    case AdapterState.NotReady:
                        // The adapter initialization did not complete.
                        MonoBehaviour.print("Adapter: " + className + " not ready.");
                        break;
                    case AdapterState.Ready:
                        // The adapter was successfully initialized.
                        MonoBehaviour.print("Adapter: " + className + " is initialized.");
                        break;
                }
            }
        });


        if (PlayerPrefs.HasKey("UserAge"))
        {
            SetConfiguration();
        }
    }

    public void SetConfiguration()
    {
        Debug.Log("Configuration");
        if (PlayerPrefs.GetInt("UserAge") > childAge)
        {
            admobBannarUnitId = bannarId18Plus;
            admobInterstitialadUnitId = interstatialId18Plus;
            admobRewardedAdUnitId = rewardedId18Plus;
            appOpemUnitId = appopenAdId18Plus;
            //rewardedInterstitialAdUnitId = rewardedInterstatialId18Plus;
            InatalizeAds();
        }
        else
        {

            admobBannarUnitId = bannarId18Below;
            admobInterstitialadUnitId = interstatialId18Below;
            admobRewardedAdUnitId = rewardedId18Below;
            appOpemUnitId = appopenAdId18Below;
            // rewardedInterstitialAdUnitId = rewardedInterstatialId18Below;
            InatalizeAds();

        }
    }
    void InatalizeAds()
    {
        LoadAppOpenAd();
        AdmobCreateBannerView();
        AdmobLoadInterstitialAd();
        AdmobLoadRewardedAd();
    }
   
   
    public void AdmobCreateBannerView()
    {
        Debug.Log("Creating banner view");
        if (admobBannarView != null)
        {
            AdmobBannarDestroyAd();
        }
        admobBannarView = new BannerView(admobBannarUnitId, AdSize.Banner, AdPosition.Bottom);
        AdmobBannarLoadAd();
    }
    public void AdmobBannarLoadAd()
    {
        if (admobBannarView == null)
        {
            AdmobCreateBannerView();
        }
        var adRequest = new AdRequest(); ;
        admobBannarView.LoadAd(adRequest);
    }
    public bool IsAdmobBannarAvailable()
    {
        return admobBannarView != null;
    }
    public void AdmobBannarDestroyAd()
    {
        if (admobBannarView != null)
        {
            Debug.Log("Destroying banner view.");
            admobBannarView.Destroy();
            admobBannarView = null;
        }
    }



    public void AdmobLoadInterstitialAd()
    {
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }
        var adRequest = new AdRequest();
        InterstitialAd.Load(admobInterstitialadUnitId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("interstitial ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Interstitial ad loaded with response : "
                          + ad.GetResponseInfo());

                _interstitialAd = ad;
                AdmobInterstitialRegisterEventHandlers(ad);
            });
    }
    public void AdmobShowInterstitialAd(Action onClosed = null)
    {
        Debug.Log("show admob " + AdmobIsInterstitialAvailble());
        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            Debug.Log("Showing interstitial ad.");
            _onInterstitialClosed = onClosed; // save callback
            _interstitialAd.Show();
        }
        else
        {
            onClosed?.Invoke();
            Debug.LogError("Interstitial ad is not ready yet.");
            //AdmobLoadInterstitialAd();
        }
    }
    public bool AdmobIsInterstitialAvailble()
    {
        return _interstitialAd != null && _interstitialAd.CanShowAd();
    }
    private void AdmobInterstitialRegisterEventHandlers(InterstitialAd interstitialAd)
    {
        // Raised when the ad is estimated to have earned money.
        interstitialAd.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        interstitialAd.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Interstitial ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        interstitialAd.OnAdClicked += () =>
        {
            Debug.Log("Interstitial ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        interstitialAd.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Interstitial ad full screen content opened.");

        };
        // Raised when the ad closed full screen content.
        interstitialAd.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial ad full screen content closed.");
            _onInterstitialClosed?.Invoke();
            _onInterstitialClosed = null;
            AdmobLoadInterstitialAd();
        };
        // Raised when the ad failed to open full screen content.
        interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content " +
                           "with error : " + error);
            AdmobLoadInterstitialAd();
        };
    }


    public void AdmobLoadRewardedAd()
    {
        // Clean up the old ad before loading a new one.
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        Debug.Log("Loading the rewarded ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        RewardedAd.Load(admobRewardedAdUnitId, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Rewarded ad loaded with response : "
                          + ad.GetResponseInfo());

                _rewardedAd = ad;
                AdmobRewardedRegisterEventHandlers(ad);
            });
    }
    public void AdmobShowRewardedAd(Action onClosed = null)
    {
        const string rewardMsg =
            "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";

        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _onRewardedClosed = onClosed;

            _rewardedAd.Show((Reward reward) =>
            {
                // TODO: Reward the user.
                Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
            });
        }
        else
        {
            onClosed?.Invoke();
        }
    }
    public bool AdmobIsRewardedVideoAvailble()
    {
        return _rewardedAd != null && _rewardedAd.CanShowAd();
    }
    private void AdmobRewardedRegisterEventHandlers(RewardedAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad full screen content closed.");
            _onRewardedClosed?.Invoke();
            _onRewardedClosed = null;
            AdmobLoadRewardedAd();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content " +
                           "with error : " + error);
            AdmobLoadRewardedAd();
        };
    }


    public void AdmobLoadRewardedInterstitialAd()
    {
        // Clean up the old ad before loading a new one.
        if (_rewardedInterstitialAd != null)
        {
            _rewardedInterstitialAd.Destroy();
            _rewardedInterstitialAd = null;
        }

        Debug.Log("Loading the rewarded interstitial ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        // send the request to load the ad.
        RewardedInterstitialAd.Load(rewardedInterstitialAdUnitId, adRequest,
            (RewardedInterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("rewarded interstitial ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Rewarded interstitial ad loaded with response : "
                          + ad.GetResponseInfo());

                _rewardedInterstitialAd = ad;
                AdmobRegisterEventHandlers(ad);
            });
    }
    void AdmobShowRewardedInterstitialAd()
    {
        const string rewardMsg =
            "Rewarded interstitial ad rewarded the user. Type: {0}, amount: {1}.";

        if (_rewardedInterstitialAd != null && _rewardedInterstitialAd.CanShowAd())
        {
            _rewardedInterstitialAd.Show((Reward reward) =>
            {
                // TODO: Reward the user.
                Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
            });
        }
    }


    public bool AdmobIsRewardedInterstitiralAvailable()
    {
        return _rewardedInterstitialAd != null && _rewardedInterstitialAd.CanShowAd();
    }

    private void AdmobRegisterEventHandlers(RewardedInterstitialAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Rewarded interstitial ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded interstitial ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded interstitial ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded interstitial ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded interstitial ad full screen content closed.");
            AdmobLoadRewardedInterstitialAd();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded interstitial ad failed to open " +
                           "full screen content with error : " + error);
            AdmobLoadRewardedInterstitialAd();
        };
    }

    public void LoadAppOpenAd()
    {
        // Clean up the old ad before loading a new one.
        if (appOpenAd != null)
        {
            appOpenAd.Destroy();
            appOpenAd = null;
        }

        Debug.Log("Loading the app open ad.");

        // Create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        AppOpenAd.Load(appOpemUnitId, adRequest,
            (AppOpenAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("app open ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("App open ad loaded with response : "
                          + ad.GetResponseInfo());

                appOpenAd = ad;
                RegisterEventHandlers(ad);
                ShowAppOpenAd();
            });
    }

    private void RegisterEventHandlers(AppOpenAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("App open ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("App open ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("App open ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("App open ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("App open ad full screen content closed.");
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("App open ad failed to open full screen content " +
                           "with error : " + error);
        };
    }
    /// <summary>
    /// Shows the app open ad.
    /// </summary>
    public void ShowAppOpenAd()
    {
        if (appOpenAd != null && appOpenAd.CanShowAd())
        {
            Debug.Log("Showing app open ad.");
            appOpenAd.Show();
        }
        else
        {
            Debug.LogError("App open ad is not ready yet.");
        }
    }

}

