using System.Collections;
using System.Collections.Generic;
#if USE_ADMOB
using GoogleMobileAds.Api;
#endif
using TheLegends.Base.Ads;
#if USE_APPSFLYER
using TheLegends.Base.AppsFlyer;
#endif
#if USE_FIREBASE
using TheLegends.Base.Firebase;
#endif
using UnityEngine;
using UnityEngine.UI;

public class DemoManager : MonoBehaviour
{
    public PlacementOrder order = PlacementOrder.One;
    public Button initBtn;
    public Button loadInterstitialBtn;
    public Button showInterstitialBtn;
    public Button loadRewardedBtn;
    public Button showRewardedBtn;
    public Button loadAppOpenBtn;
    public Button showAppOpenBtn;
    public Button loadBannerBtn;
    public Button showBannerBtn;
    public Button hideBannerBtn;
    public Button loadMrecBtn;
    public Button showMrecBtn;
    public Button hideMrecBtn;
    public Button loadNativeOverlayBtn;
    public Button showNativeOverlayBtn;
    public Button hideNativeOverlayBtn;
    public Button testBtn;
    public Dropdown MrecPosDropdown;
    public Button loadNativeBtn;
    public Button showNativeBtn;
    public Button hideNativeBtn;
    public Button loadNativeVideoPlatformBtn;
    public Button showNativeVideoPlatformBtn;
    public Button hideNativeVideoPlatformBtn;
    public Button loadNativeBannerPlatformBtn;
    public Button showNativeBannerPlatformBtn;
    public Button hideNativeBannerPlatformBtn;
    public Button adjustLayoutForNativeBannerBtn;
    public Button removeAdsBtn;

#if USE_ADMOB && USE_ADMOB_NATIVE_UNITY
    public AdmobNativeController nativeAdsMrec;
    public AdmobNativeController nativeAdsBanner;
#endif

#if USE_ADMOB
    public AdmobNativeAdvancedController nativeBannerAdvanced;
    public AdmobNativeAdvancedController nativeVideoAdvanced;
#endif


    public Button nativeOverlayCloseBtn;
    public GameObject nativeOverlayBG;

    public AdLayoutHelper adLayoutHelper;


    private void OnEnable()
    {
        initBtn.onClick.AddListener(InitAdsManager);
        loadInterstitialBtn.onClick.AddListener(LoadInterstitial);
        showInterstitialBtn.onClick.AddListener(ShowInterstitial);
        loadRewardedBtn.onClick.AddListener(Loadrewarded);
        showRewardedBtn.onClick.AddListener(ShowRewarded);
        loadAppOpenBtn.onClick.AddListener(LoadAppOpen);
        showAppOpenBtn.onClick.AddListener(ShowAppOpen);
        loadBannerBtn.onClick.AddListener(LoadBanner);
        showBannerBtn.onClick.AddListener(ShowBanner);
        hideBannerBtn.onClick.AddListener(HideBanner);
        loadMrecBtn.onClick.AddListener(LoadMrec);
        showMrecBtn.onClick.AddListener(ShowMrec);
        hideMrecBtn.onClick.AddListener(HideMrec);
        loadNativeBtn.onClick.AddListener(LoadNative);
        showNativeBtn.onClick.AddListener(ShowNative);
        hideNativeBtn.onClick.AddListener(HideNative);
        loadNativeVideoPlatformBtn.onClick.AddListener(LoadNativeVideoPlatform);
        showNativeVideoPlatformBtn.onClick.AddListener(ShowNativeVideoPlatform);
        hideNativeVideoPlatformBtn.onClick.AddListener(HideNativeVideoPlatform);
        loadNativeBannerPlatformBtn.onClick.AddListener(LoadNativeBannerPlatform);
        showNativeBannerPlatformBtn.onClick.AddListener(ShowNativeBannerPlatform);
        hideNativeBannerPlatformBtn.onClick.AddListener(HideNativeBannerPlatform);
        adjustLayoutForNativeBannerBtn.onClick.AddListener(AdjustLayoutForNativeBanner);
        removeAdsBtn.onClick.AddListener(RemoveAds);
    }


    private void InitAdsManager()
    {
        // AdsManager.Instance.Init();
        // FirebaseManager.Instance.Init();
        // AppsFlyerManager.Instance.Init();
        StartCoroutine(DoInit());
    }

    private IEnumerator DoInit()
    {
        yield return AdsManager.Instance.DoInit();

#if USE_FIREBASE
        var defaultRemoteConfig = new Dictionary<string, object>
        {
            {"adInterOnComplete", AdsManager.Instance.adsConfigs.adInterOnComplete},
            {"adInterOnStart", AdsManager.Instance.adsConfigs.adInterOnStart},
            {"timePlayToShowAds", AdsManager.Instance.adsConfigs.timePlayToShowAds},
            {"adNativeBannerHeight", AdsManager.Instance.adsConfigs.adNativeBannerHeight},
            {"adTimeReload", AdsManager.Instance.adsConfigs.adTimeReload}
        };

        yield return FirebaseManager.Instance.DoInit(defaultRemoteConfig);

        FirebaseManager.Instance.FetchRemoteData(() =>
        {
            AdsManager.Instance.adsConfigs.adInterOnComplete = FirebaseManager.Instance.RemoteGetValueBoolean("adInterOnComplete", AdsManager.Instance.adsConfigs.adInterOnComplete);
            AdsManager.Instance.adsConfigs.adInterOnStart = FirebaseManager.Instance.RemoteGetValueBoolean("adInterOnStart", AdsManager.Instance.adsConfigs.adInterOnStart);
            AdsManager.Instance.adsConfigs.timePlayToShowAds = FirebaseManager.Instance.RemoteGetValueFloat("timePlayToShowAds", AdsManager.Instance.adsConfigs.timePlayToShowAds);
            AdsManager.Instance.adsConfigs.adNativeBannerHeight = FirebaseManager.Instance.RemoteGetValueFloat("adNativeBannerHeight", AdsManager.Instance.adsConfigs.adNativeBannerHeight);
            AdsManager.Instance.adsConfigs.adTimeReload = FirebaseManager.Instance.RemoteGetValueFloat("adTimeReload", AdsManager.Instance.adsConfigs.adTimeReload);
        }, null);
#endif

#if USE_APPSFLYER
        yield return AppsFlyerManager.Instance.DoInit();
#endif
    }

    private void LoadInterstitial()
    {
        AdsManager.Instance.LoadInterstitial(AdsType.Interstitial, order);
    }

    private void ShowInterstitial()
    {
        AdsManager.Instance.ShowInterstitial(AdsType.Interstitial, order, "Default", () =>
        {
            AdsManager.Instance.Log("Interstitial closed");
        });
    }

    private void Loadrewarded()
    {
        AdsManager.Instance.LoadRewarded(order);
    }

    private void ShowRewarded()
    {
        AdsManager.Instance.ShowRewarded(order, "Default", () =>
        {
            AdsManager.Instance.Log("Rewarded successfully");
        });

    }

    private void LoadAppOpen()
    {
        AdsManager.Instance.LoadAppOpen(order);
    }

    private void ShowAppOpen()
    {
        AdsManager.Instance.ShowAppOpen(order, "Default");
    }

    private void LoadBanner()
    {
        AdsManager.Instance.LoadBanner(order);
    }

    private void ShowBanner()
    {
        AdsManager.Instance.ShowBanner(order, "Default");
    }

    private void HideBanner()
    {
        AdsManager.Instance.HideBanner(order);
    }

    private void LoadMrec()
    {
        AdsManager.Instance.LoadMrec(AdsType.Mrec, order);
    }

    private void ShowMrec()
    {
        var mrecPos = (AdsPos)MrecPosDropdown.value;
        AdsManager.Instance.ShowMrec(AdsType.Mrec, order, mrecPos, new Vector2Int(0, 0), "Default");
    }

    private void HideMrec()
    {
        AdsManager.Instance.HideMrec(AdsType.Mrec, order);
    }

    public void LoadNative()
    {
#if USE_ADMOB && USE_ADMOB_NATIVE_UNITY
        nativeAdsMrec.LoadAds();
        nativeAdsBanner.LoadAds();
#endif
    }

    public void ShowNative()
    {
#if USE_ADMOB && USE_ADMOB_NATIVE_UNITY
        nativeAdsMrec.ShowAds();
        nativeAdsBanner.ShowAds("Default");
#endif
    }

    public void HideNative()
    {
#if USE_ADMOB && USE_ADMOB_NATIVE_UNITY
        nativeAdsMrec.HideAds();
        nativeAdsBanner.HideAds();
#endif
    }

    public void AAAAA()
    {
        AdsManager.Instance.GetAdsStatus(AdsType.NativeUnity, order);
    }

    public void LoadNativeVideoPlatform()
    {
#if USE_ADMOB
        nativeVideoAdvanced.LoadAds();
#endif
    }

    public void ShowNativeVideoPlatform()
    {
#if USE_ADMOB
        nativeVideoAdvanced.ShowAds(() =>
        {
            AdsManager.Instance.Log("NativeVideo show");
        }, () =>
        {
            AdsManager.Instance.Log("NativeVideo closed");
        }, () =>
        {
            AdsManager.Instance.Log("NativeVideo full screen content closed");
        });
#endif
    }

    public void HideNativeVideoPlatform()
    {
#if USE_ADMOB
        nativeVideoAdvanced.HideAds();
#endif
    }


    public void LoadNativeBannerPlatform()
    {
#if USE_ADMOB
        nativeBannerAdvanced.LoadAds();
#endif
    }

    public void ShowNativeBannerPlatform()
    {
#if USE_ADMOB
        nativeBannerAdvanced.ShowAds(() =>
        {
            AdsManager.Instance.Log("NativeBanner show");
        }, () =>
        {
            AdsManager.Instance.Log("NativeBanner closed");
        });
#endif
    }

    public void HideNativeBannerPlatform()
    {
#if USE_ADMOB
        nativeBannerAdvanced.HideAds();
#endif
    }

    public void AdjustLayoutForNativeBanner()
    {
        adLayoutHelper.AdjustLayoutForNativeBanner(60);
    }

    public void RemoveAds()
    {
        AdsManager.Instance.IsCanShowAds = false;
    }
}
