using System;
using System.Collections;
using GoogleMobileAds.Api;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class AdmobNativeAdvancedController : AdsPlacementBase
    {
        private AdmobNativeAdvanced _nativeAdvancedAd;
        public AdmobNativeAdvanced NativeAdvancedAd => _nativeAdvancedAd;
        private INativeAdvancedShowStrategy _showStratery;
        protected Action OnClose;
        protected Action OnShow;
        protected Action OnAdDismissedFullScreenContent;

        [SerializeField]
        private PlacementOrder _order = PlacementOrder.One;
        [SerializeField] private string showPosition;

        public override AdsMediation GetAdsMediation()
        {
#if USE_ADMOB
            return AdsMediation.Admob;
#else
            return AdsMediation.None;
#endif
        }
        public override AdsType GetAdsType()
        {
#if USE_ADMOB
            return AdsType.NativeAdvanced;
#else
            return AdsType.None;
#endif
        }

        public override bool IsAdsReady()
        {
#if USE_ADMOB
            return NativeAdvancedAd != null && NativeAdvancedAd.IsAdAvailable();
#else
            return false;
#endif
        }

        public string LayoutId => gameObject.GetInstanceID().ToString();

        private void Awake()
        {
            _showStratery = GetComponent<INativeAdvancedShowStrategy>();
            AdsManager.Instance.OnCanShowAdsChanged += OnCanShowAdsChanged;
        }

        private void Start()
        {
            position = showPosition;

            var platform = Application.platform;
            var isIOS = platform == RuntimePlatform.IPhonePlayer || platform == RuntimePlatform.OSXPlayer;
            var isTest = AdsManager.Instance.SettingsAds.isTest;

            var list = isTest
                ? (isIOS
                    ? AdsManager.Instance.SettingsAds.ADMOB_IOS_Test.nativeAdvancedIds
                    : AdsManager.Instance.SettingsAds.ADMOB_Android_Test.nativeAdvancedIds)
                : (isIOS
                    ? AdsManager.Instance.SettingsAds.ADMOB_IOS.nativeAdvancedIds
                    : AdsManager.Instance.SettingsAds.ADMOB_Android.nativeAdvancedIds);

            if (list.Count <= 0)
            {
                return;
            }

            var placementIndex = Mathf.Clamp((int)_order - 1, 0, list.Count - 1);
            placement = list[placementIndex];

            StartCoroutine(IEInit(placement, _order));
        }

        private IEnumerator IEInit(Placement placement, PlacementOrder order)
        {
            while (!AdsManager.Instance.IsInitialized())
            {
                yield return null;
            }

            Init(placement, _order);

            AdsManager.Instance.RegisterNativeAdvanced(this);

            NativeAdAssetManager.InitializeAndCache(this.gameObject);
            this.gameObject.SetActive(false);
        }

        private void OnCanShowAdsChanged(bool isCanShowAds)
        {
            if (!isCanShowAds)
            {
                HideAds();
            }
        }

        public override void LoadAds()
        {
#if USE_ADMOB
            if (!IsCanLoadAds())
            {
                return;
            }

            if (IsReady && Status == AdsEvents.LoadAvailable)
            {
                return;
            }

            // NativeAdvancedDestroy();

            base.LoadAds();

            // Unity flags already set in StoreConfigs (simple like AdmobNativeController)

            AdRequest request = new AdRequest();

            AdmobNativeAdvanced.Load(adsUnitID.Trim(), request, (native, error) =>
            {
                PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    if (_loadRequestId != _currentLoadRequestId)
                    {
                        // If the load request ID does not match, this callback is from a previous request
                        return;
                    }

                    StopHandleTimeout();

                    // if error is not null, the load request failed.
                    if (error != null)
                    {
                        AdsManager.Instance.LogError($"{AdsMediation}_{AdsType} " + "ad failed to load with error : " + error);
                        OnNativeAdvancedLoadFailed(error);
                        return;
                    }

                    if (native == null)
                    {
                        AdsManager.Instance.LogError($"{AdsMediation}_{AdsType} " + "Unexpected error: load event fired with null ad and null error.");
                        OnNativeAdvancedLoadFailed(error);
                        return;
                    }

                    var responseInfo = native.GetResponseInfo();
                    networkName = responseInfo.GetMediationAdapterClassName();

                    AdsManager.Instance.Log($"{AdsMediation}_{AdsType} " + "ad loaded with response : " + responseInfo);

                    if (NativeAdvancedAd != null)
                    {
                        NativeAdvancedDestroy();
                    }

                    _nativeAdvancedAd = native;

                    OnAdsLoadAvailable();

                    adsUnitIDIndex = 0;

                    _showStratery.OnAdsLoaded(this);


                });
            });
#endif
        }

        public void ShowAds()
        {
            ShowAds(position);
        }

        public void ShowAds(Action OnShow = null, Action OnClose = null, Action OnAdDismissedFullScreenContent = null)
        {
            ShowAds(position, OnShow, OnClose, OnAdDismissedFullScreenContent);
        }

        public void ShowAds(string showPosition, Action OnShow = null, Action OnClose = null, Action OnAdDismissedFullScreenContent = null)
        {
#if USE_ADMOB

            position = showPosition;

            if (Status == AdsEvents.ShowSuccess)
            {
                AdsManager.Instance.LogError($"{AdsMediation}_{AdsType} " + "is showing --> return");
                return;
            }

            this.OnClose = OnClose;
            this.OnShow = OnShow;
            this.OnAdDismissedFullScreenContent = OnAdDismissedFullScreenContent;
            base.ShowAds(position);

            if (IsReady && IsAvailable)
            {
                OnAdsShowSuccess();
                RegisterAdEvents();

                CancelReloadAds();

                _showStratery.ExecuteShow(this);
            }
            else
            {
                AdsManager.Instance.LogWarning($"{AdsMediation}_{AdsType} " + "is not ready --> Load Ads");
                reloadCount = 0;
                LoadAds();
            }
#endif
        }

        public void HideAds()
        {
            OnShow = null;
            OnClose = null;
            OnAdDismissedFullScreenContent = null;

            NativeAdvancedDestroy();
            OnNativeAdvancedClosed();
        }

        public void DelayReloadAd(float time)
        {
            Invoke(nameof(LoadAds), time);
        }

        private void CancelReloadAds()
        {
            CancelInvoke(nameof(LoadAds));
        }


        #region Internal

        private void NativeAdvancedDestroy()
        {
#if USE_ADMOB
            try
            {
                if (NativeAdvancedAd != null)
                {
                    CancelReloadAds();

                    UnregisterAdEvents();
                    NativeAdvancedAd.Destroy();
                    _nativeAdvancedAd = null;
                }
            }
            catch (Exception ex)
            {
                AdsManager.Instance.LogException(ex);
            }
#endif
        }

        private void OnNativeAdvancedLoadFailed(LoadAdError error)
        {
#if USE_ADMOB
            var errorDescription = error?.GetMessage();
            OnAdsLoadFailed(errorDescription);

            _showStratery.OnAdsLoadFailed(this);
#endif
        }

        private void RegisterAdEvents()
        {
#if USE_ADMOB
            if (NativeAdvancedAd == null) return;

            NativeAdvancedAd.OnAdPaid += OnAdsPaid;
            NativeAdvancedAd.OnAdClicked += OnNativeAdvancedClick;
            NativeAdvancedAd.OnAdDidRecordImpression += OnNativeAdvancedImpression;
            NativeAdvancedAd.OnVideoStart += OnVideoStart;
            NativeAdvancedAd.OnVideoEnd += OnVideoEnd;
            NativeAdvancedAd.OnVideoMute += OnVideoMute;
            NativeAdvancedAd.OnVideoPlay += OnVideoPlay;
            NativeAdvancedAd.OnVideoPause += OnVideoPause;
            NativeAdvancedAd.OnAdClosed += OnNativeAdvancedClosed;
            NativeAdvancedAd.OnAdShow += OnNativeAdvancedShow;
            NativeAdvancedAd.OnAdShowedFullScreenContent += OnNativeAdvancedShowedFullScreenContent;
            NativeAdvancedAd.OnAdDismissedFullScreenContent += OnNativeAdvancedDismissedFullScreenContent;
#endif
        }

        private void UnregisterAdEvents()
        {
#if USE_ADMOB
            if (NativeAdvancedAd == null) return;

            NativeAdvancedAd.OnAdPaid -= OnAdsPaid;
            NativeAdvancedAd.OnAdClicked -= OnNativeAdvancedClick;
            NativeAdvancedAd.OnAdDidRecordImpression -= OnNativeAdvancedImpression;
            NativeAdvancedAd.OnVideoStart -= OnVideoStart;
            NativeAdvancedAd.OnVideoEnd -= OnVideoEnd;
            NativeAdvancedAd.OnVideoMute -= OnVideoMute;
            NativeAdvancedAd.OnVideoPlay -= OnVideoPlay;
            NativeAdvancedAd.OnVideoPause -= OnVideoPause;
            NativeAdvancedAd.OnAdClosed -= OnNativeAdvancedClosed;
            NativeAdvancedAd.OnAdShow -= OnNativeAdvancedShow;
            NativeAdvancedAd.OnAdShowedFullScreenContent -= OnNativeAdvancedShowedFullScreenContent;
            NativeAdvancedAd.OnAdDismissedFullScreenContent -= OnNativeAdvancedDismissedFullScreenContent;
#endif
        }

        private void OnAdsPaid(AdValue adValue)
        {
#if USE_ADMOB
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                AdsManager.Instance.LogImpressionData(AdsMediation, AdsType, adsUnitID, networkName, position, adValue);
            });
#endif
        }

        private void OnNativeAdvancedClick()
        {
#if USE_ADMOB
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnAdsClick();
            });
#endif
        }

        private void OnNativeAdvancedImpression(object sender, EventArgs args)
        {
#if USE_ADMOB
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnImpression();
            });
#endif
        }

        private void OnVideoStart()
        {
#if USE_ADMOB
            AdsManager.Instance.Log($"{AdsMediation}_{AdsType} Video started");
#endif
        }

        private void OnVideoEnd()
        {
#if USE_ADMOB
            AdsManager.Instance.Log($"{AdsMediation}_{AdsType} Video ended");
#endif
        }

        private void OnVideoMute(object sender, bool isMuted)
        {
#if USE_ADMOB
            AdsManager.Instance.Log($"{AdsMediation}_{AdsType} Video mute state: {isMuted}");
#endif
        }

        private void OnVideoPlay()
        {
#if USE_ADMOB
            AdsManager.Instance.Log($"{AdsMediation}_{AdsType} Video playing");
#endif
        }

        private void OnVideoPause()
        {
#if USE_ADMOB
            AdsManager.Instance.Log($"{AdsMediation}_{AdsType} Video paused");
#endif
        }

        protected virtual void OnNativeAdvancedClosed()
        {
#if USE_ADMOB
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnAdsClosed();
                OnClose?.Invoke();
            });
#endif
        }

        protected virtual void OnNativeAdvancedShow()
        {
#if USE_ADMOB
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnAdsShowSuccess();
                OnShow?.Invoke();
            });
#endif
        }

        private void OnNativeAdvancedShowedFullScreenContent()
        {
#if USE_ADMOB
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {

            });
#endif
        }

        private void OnNativeAdvancedDismissedFullScreenContent()
        {
#if USE_ADMOB
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {

            });
#endif
        }

        #endregion



    }
}
