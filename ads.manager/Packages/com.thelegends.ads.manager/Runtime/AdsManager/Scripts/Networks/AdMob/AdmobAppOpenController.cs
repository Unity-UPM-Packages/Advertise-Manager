#if USE_ADMOB

using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using TheLegends.Base.UI;

namespace TheLegends.Base.Ads
{
    public class AdmobAppOpenController : AdsPlacementBase
    {
        private PreloadConfiguration preloadConfiguration;
        private AppOpenAd _appOpenAd;
        protected Action OnClose;

        public override AdsNetworks GetAdsNetworks()
        {
#if USE_ADMOB
            return AdsNetworks.Admob;
#else
        return AdsNetworks.None;
#endif
        }

        public override AdsType GetAdsType()
        {
#if USE_ADMOB
            return AdsType.AppOpen;
#else
        return AdsType.None;
#endif
        }

        public override bool IsAdsReady()
        {
#if USE_ADMOB
            // if (_appOpenAd != null)
            // {
            //     return _appOpenAd.CanShowAd();
            // }

            // return false;

            return AppOpenAdPreloader.IsAdAvailable(adsUnitID);
#else
        return false;
#endif
        }

        public override void LoadAds()
        {
#if USE_ADMOB
            AppOpenAdPreloader.Destroy(adsUnitID);

            if (!IsCanLoadAds())
            {
                return;
            }

            if (IsReady)
            {
                return;
            }

            preloadConfiguration = new PreloadConfiguration
            {
                AdUnitId = adsUnitID,
                Request = new AdRequest(),
                BufferSize = 2
            };

            Status = AdsEvents.LoadRequest;
            
            // AdRequest request = new AdRequest();

            // AppOpenAd.Load(adsUnitID.Trim(), request,
            //     (AppOpenAd ad, LoadAdError error) =>
            //     {
            //         if (_loadRequestId != _currentLoadRequestId)
            //         {
            //             // If the load request ID does not match, this callback is from a previous request
            //             return;
            //         }

            //         StopHandleTimeout();

            //         // if error is not null, the load request failed.
            //         if (error != null)
            //         {
            //             AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + "failed to load with error : " + error);
            //             OnAppOpenLoadFailed(error);
            //             return;
            //         }

            //         if (ad == null)
            //         {
            //             AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + "Unexpected error: load event fired with null ad and null error.");
            //             OnAppOpenLoadFailed(error);
            //             return;
            //         }

            //         AdsManager.Instance.Log($"{AdsNetworks}_{AdsType} " + "ad loaded with response : " + ad.GetResponseInfo());

            //         _appOpenAd = ad;

            //         OnAdsLoadAvailable();

            //     });

            AppOpenAdPreloader.Preload(
                adsUnitID,
                preloadConfiguration,
                OnAdPreloaded,
                OnAdFailedToPreload,
                OnAdsExhausted);
        }
#endif

        public void ShowAds(string showPosition, Action OnClose = null)
        {
            this.OnClose = OnClose;
            base.ShowAds(showPosition);
#if USE_ADMOB
            if (IsReady && IsAvailable)
            {
                // _appOpenAd.OnAdClicked += OnAppOpenClick;
                // _appOpenAd.OnAdPaid += OnAdsPaid;
                // _appOpenAd.OnAdImpressionRecorded += OnAppOpenImpression;
                // _appOpenAd.OnAdFullScreenContentClosed += OnAppOpenClosed;
                // _appOpenAd.OnAdFullScreenContentFailed += OnAppOpenShowFailed;
                // _appOpenAd.OnAdFullScreenContentOpened += OnAppOpenShowSuccess;
                // _appOpenAd.Show();

                _appOpenAd = AppOpenAdPreloader.DequeueAd(adsUnitID);

                if (_appOpenAd != null)
                {
                    _appOpenAd.OnAdClicked += OnAppOpenClick;
                    _appOpenAd.OnAdPaid += OnAdsPaid;
                    _appOpenAd.OnAdImpressionRecorded += OnAppOpenImpression;
                    _appOpenAd.OnAdFullScreenContentClosed += OnAppOpenClosed;
                    _appOpenAd.OnAdFullScreenContentFailed += OnAppOpenShowFailed;
                    _appOpenAd.OnAdFullScreenContentOpened += OnAppOpenShowSuccess;
                    _appOpenAd.Show();
                }
            }
            else
            {
                AdsManager.Instance.LogWarning($"{AdsNetworks}_{AdsType} " + "is not ready --> Load Ads");
                reloadCount = 0;
                LoadAds();
            }
#endif
        }


        #region Internal

        private void OnAppOpenClick()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnAdsClick();
            });
        }

        private void OnAppOpenShowSuccess()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnAdsShowSuccess();
                AdsManager.Instance.OnFullScreenAdsShow();
            });
        }

        private void OnAppOpenImpression()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnImpression();
            });
        }

        private void OnAppOpenLoadFailed(AdError error)
        {
            var errorDescription = error?.GetMessage();
            // OnAdsLoadFailed(errorDescription);

            AdsManager.Instance.LogError($"{AdsNetworks.ToString()}_{AdsType.ToString()} " + "OnAdsLoadFailed " + adsUnitID + " Error: " + errorDescription);

            if (AppOpenAdPreloader.GetNumAdsAvailable(adsUnitID) == 0)
            {
                Status = AdsEvents.LoadNotAvailable;
            }
        }

        private void OnAppOpenShowFailed(AdError error)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                var errorDescription = error?.GetMessage();
                OnAdsShowFailed(errorDescription);
            });
        }

        private void OnAppOpenClosed()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                UILoadingController.Show(1f, () =>
                {
                    OnClose?.Invoke();
                    OnClose = null;

                    AdsManager.Instance.OnFullScreenAdsClosed();
                });

                _appOpenAd.OnAdClicked -= OnAppOpenClick;
                _appOpenAd.OnAdPaid -= OnAdsPaid;
                _appOpenAd.OnAdImpressionRecorded -= OnAppOpenImpression;
                _appOpenAd.OnAdFullScreenContentClosed -= OnAppOpenClosed;
                _appOpenAd.OnAdFullScreenContentFailed -= OnAppOpenShowFailed;
                _appOpenAd.OnAdFullScreenContentOpened -= OnAppOpenShowSuccess;

                if (_appOpenAd != null)
                {
                    _appOpenAd.Destroy();
                    _appOpenAd = null;
                }

                OnAdsClosed();
            });
        }

        private void OnAdsPaid(AdValue value)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                AdsManager.Instance.LogImpressionData(AdsNetworks, AdsType, adsUnitID, value);
            });
        }

        public override void OnAdsClosed()
        {
            Status = AdsEvents.Close;
        }


        #endregion

        #region Preload Callbacks
        private void OnAdPreloaded(string preloadId, ResponseInfo responseInfo)
        {
            OnAdsLoadAvailable();
        }

        private void OnAdFailedToPreload(string preloadId, AdError adError)
        {
            AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} ad failed to load with error : {adError}");
            OnAppOpenLoadFailed(adError);
        }

        private void OnAdsExhausted(string preloadId)
        {
            AdsManager.Instance.LogWarning($"{AdsNetworks}_{AdsType} Preload ad configuration {preloadId} was exhausted");
            LoadAds();
        }

        protected override bool IsAdsAvailable()
        {
            return AppOpenAdPreloader.IsAdAvailable(adsUnitID) && AdsManager.Instance.IsCanShowAds;
        }


        #endregion
    }
}

#endif