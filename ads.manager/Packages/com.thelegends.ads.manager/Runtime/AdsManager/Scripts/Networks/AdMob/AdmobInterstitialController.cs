#if USE_ADMOB
using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using TheLegends.Base.UI;
namespace TheLegends.Base.Ads
{
    public class AdmobInterstitialController : AdsPlacementBase
    {
        private PreloadConfiguration preloadConfiguration;
        private InterstitialAd _interstitialAd;

        protected Action OnClose;

        public override void LoadAds()
        {
#if USE_ADMOB
            InterstitialAdPreloader.Destroy(adsUnitID);

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

            // InterstitialAd.Load(adsUnitID.Trim(), request,
            //     (InterstitialAd ad, LoadAdError error) =>
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
            //             AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + "ad failed to load with error : " + error);
            //             OnInterLoadFailed(error);
            //             return;
            //         }

            //         if (ad == null)
            //         {
            //             AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + "Unexpected error: load event fired with null ad and null error.");
            //             OnInterLoadFailed(error);
            //             return;
            //         }

            //         AdsManager.Instance.Log($"{AdsNetworks}_{AdsType} " + "ad loaded with response : " + ad.GetResponseInfo());

            //         _interstitialAd = ad;

            //         OnAdsLoadAvailable();
            //     });

            InterstitialAdPreloader.Preload(
                adsUnitID,
                preloadConfiguration,
                OnAdPreloaded,
                OnAdFailedToPreload,
                OnAdsExhausted);
#else

#endif
        }

        public void ShowAds(string showPosition, Action OnClose = null)
        {
            this.OnClose = OnClose;
            base.ShowAds(showPosition);

#if USE_ADMOB
            if (IsReady && IsAvailable)
            {
                // _interstitialAd.OnAdClicked += OnInterClick;
                // _interstitialAd.OnAdPaid += OnInterPaid;
                // _interstitialAd.OnAdImpressionRecorded += OnInterImpression;
                // _interstitialAd.OnAdFullScreenContentClosed += OnInterClosed;
                // _interstitialAd.OnAdFullScreenContentFailed += OnInterShowFailed;
                // _interstitialAd.OnAdFullScreenContentOpened += OnInterShowSuccess;
                // _interstitialAd.Show();

                _interstitialAd = InterstitialAdPreloader.DequeueAd(adsUnitID);

                if (_interstitialAd != null)
                {
                    _interstitialAd.OnAdClicked += OnInterClick;
                    _interstitialAd.OnAdPaid += OnInterPaid;
                    _interstitialAd.OnAdImpressionRecorded += OnInterImpression;
                    _interstitialAd.OnAdFullScreenContentClosed += OnInterClosed;
                    _interstitialAd.OnAdFullScreenContentFailed += OnInterShowFailed;
                    _interstitialAd.OnAdFullScreenContentOpened += OnInterShowSuccess;
                    _interstitialAd.Show();
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
            return AdsType.Interstitial;
#else
            return AdsType.None;
#endif
        }

        public override bool IsAdsReady()
        {
#if USE_ADMOB
            // if (_interstitialAd != null)
            // {
            //     return _interstitialAd.CanShowAd();
            // }

            // return false;

            return InterstitialAdPreloader.IsAdAvailable(adsUnitID);
#else
            return false;
#endif
        }


        #region Internal

        private void OnInterClick()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnAdsClick();
            });
        }

        private void OnInterImpression()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnImpression();
            });
        }

        private void OnInterShowSuccess()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnAdsShowSuccess();
                AdsManager.Instance.OnFullScreenAdsShow();
            });
        }

        private void OnInterLoadFailed(AdError error)
        {
            var errorDescription = error?.GetMessage();
            // OnAdsLoadFailed(errorDescription);

            AdsManager.Instance.LogError($"{AdsNetworks.ToString()}_{AdsType.ToString()} " + "OnAdsLoadFailed " + adsUnitID + " Error: " + errorDescription);

            if (InterstitialAdPreloader.GetNumAdsAvailable(adsUnitID) == 0)
            {
                Status = AdsEvents.LoadNotAvailable;
            }
        }

        private void OnInterShowFailed(AdError error)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                var errorDescription = error?.GetMessage();
                OnAdsShowFailed(errorDescription);
            });
        }

        protected virtual void OnInterClosed()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                UILoadingController.Show(1f, () =>
                {
                    OnClose?.Invoke();
                    OnClose = null;

                    AdsManager.Instance.OnFullScreenAdsClosed();
                });

                _interstitialAd.OnAdClicked -= OnInterClick;
                _interstitialAd.OnAdPaid -= OnInterPaid;
                _interstitialAd.OnAdImpressionRecorded -= OnInterImpression;
                _interstitialAd.OnAdFullScreenContentClosed -= OnInterClosed;
                _interstitialAd.OnAdFullScreenContentFailed -= OnInterShowFailed;
                _interstitialAd.OnAdFullScreenContentOpened -= OnInterShowSuccess;

                if (_interstitialAd != null)
                {
                    _interstitialAd.Destroy();
                    _interstitialAd = null;
                }

                OnAdsClosed();
            });
        }

        private void OnInterPaid(AdValue value)
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
            OnInterLoadFailed(adError);
        }

        private void OnAdsExhausted(string preloadId)
        {
            AdsManager.Instance.LogWarning($"{AdsNetworks}_{AdsType} Preload ad configuration {preloadId} was exhausted");
            LoadAds();
        }

        protected override bool IsAdsAvailable()
        {
            return InterstitialAdPreloader.IsAdAvailable(adsUnitID) && AdsManager.Instance.IsCanShowAds;
        }


        #endregion




    }

}

#endif