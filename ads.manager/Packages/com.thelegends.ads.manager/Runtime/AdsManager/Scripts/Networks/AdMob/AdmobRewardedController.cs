#if USE_ADMOB

using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using TheLegends.Base.UI;

namespace TheLegends.Base.Ads
{
    public class AdmobRewardedController : AdsPlacementBase
    {
        private PreloadConfiguration preloadConfiguration;
        private RewardedAd _rewardedAd;
        private Action OnRewarded;

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
            return AdsType.Rewarded;
#else
            return AdsType.None;
#endif
        }

        public override bool IsAdsReady()
        {
#if USE_ADMOB
            // if (_rewardedAd != null)
            // {
            //     return _rewardedAd.CanShowAd();
            // }

            // return false;

            return RewardedAdPreloader.IsAdAvailable(adsUnitID);
#else
            return false;
#endif
        }

        public override void LoadAds()
        {
#if USE_ADMOB
            RewardedAdPreloader.Destroy(adsUnitID);

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

            // RewardedAd.Load(adsUnitID.Trim(), request,
            //     (RewardedAd ad, LoadAdError error) =>
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
            //             OnRewardedLoadFailed(error);
            //             return;
            //         }

            //         if (ad == null)
            //         {
            //             AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + "Unexpected error: load event fired with null ad and null error.");
            //             OnRewardedLoadFailed(error);
            //             return;
            //         }

            //         AdsManager.Instance.Log($"{AdsNetworks}_{AdsType} " + "ad loaded with response : " + ad.GetResponseInfo());

            //         _rewardedAd = ad;

            //         OnAdsLoadAvailable();
            //     });

            RewardedAdPreloader.Preload(
                adsUnitID,
                preloadConfiguration,
                OnAdPreloaded,
                OnAdFailedToPreload,
                OnAdsExhausted);
        }
#endif


        public void ShowAds(string showPosition, Action OnRewarded = null)
        {
            base.ShowAds(showPosition);
#if USE_ADMOB
            if (IsReady && IsAvailable)
            {
                // _rewardedAd.OnAdClicked += OnRewardClick;
                // _rewardedAd.OnAdPaid += OnRewardPaid;
                // _rewardedAd.OnAdImpressionRecorded += OnRewardImpression;
                // _rewardedAd.OnAdFullScreenContentClosed += OnRewardedClosed;
                // _rewardedAd.OnAdFullScreenContentFailed += OnRewardedShowFailed;
                // _rewardedAd.OnAdFullScreenContentOpened += OnRewardShowSuccess;
                // _rewardedAd.Show(reward =>
                // {
                //     if (reward != null)
                //     {
                //         AdsManager.Instance.Log($"{AdsNetworks}_{AdsType} " + $"{adsUnitID} " + "claimed");
                //         this.OnRewarded = OnRewarded;
                //     }
                // });

                _rewardedAd = RewardedAdPreloader.DequeueAd(adsUnitID);

                if (_rewardedAd != null)
                {
                    _rewardedAd.OnAdClicked += OnRewardClick;
                    _rewardedAd.OnAdPaid += OnRewardPaid;
                    _rewardedAd.OnAdImpressionRecorded += OnRewardImpression;
                    _rewardedAd.OnAdFullScreenContentClosed += OnRewardedClosed;
                    _rewardedAd.OnAdFullScreenContentFailed += OnRewardedShowFailed;
                    _rewardedAd.OnAdFullScreenContentOpened += OnRewardShowSuccess;
                    _rewardedAd.Show(reward =>
                    {
                        if (reward != null)
                        {
                            AdsManager.Instance.Log($"{AdsNetworks}_{AdsType} " + $"{adsUnitID} " + "claimed");
                            this.OnRewarded = OnRewarded;
                        }
                    });
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

        private void OnRewardClick()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnAdsClick();
            });
        }

        private void OnRewardPaid(AdValue value)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnAdsPaid(value);
            });
        }

        private void OnRewardImpression()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnImpression();
            });
        }

        private void OnRewardShowSuccess()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnAdsShowSuccess();
                AdsManager.Instance.OnFullScreenAdsShow();
            });
        }

        private void OnRewardedLoadFailed(AdError error)
        {
            var errorDescription = error?.GetMessage();
            // OnAdsLoadFailed(errorDescription);

            AdsManager.Instance.LogError($"{AdsNetworks.ToString()}_{AdsType.ToString()} " + "OnAdsLoadFailed " + adsUnitID + " Error: " + errorDescription);

            if (RewardedAdPreloader.GetNumAdsAvailable(adsUnitID) == 0)
            {
                Status = AdsEvents.LoadNotAvailable;
            }
            
        }

        private void OnRewardedShowFailed(AdError error)
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                var errorDescription = error?.GetMessage();
                OnAdsShowFailed(errorDescription);
            });
        }

        private void OnRewardedClosed()
        {
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                UILoadingController.Show(1f, () =>
                {
                    OnRewarded?.Invoke();
                    OnRewarded = null;

                    AdsManager.Instance.OnFullScreenAdsClosed();
                });

                _rewardedAd.OnAdClicked -= OnRewardClick;
                _rewardedAd.OnAdPaid -= OnRewardPaid;
                _rewardedAd.OnAdImpressionRecorded -= OnRewardImpression;
                _rewardedAd.OnAdFullScreenContentClosed -= OnRewardedClosed;
                _rewardedAd.OnAdFullScreenContentFailed -= OnRewardedShowFailed;
                _rewardedAd.OnAdFullScreenContentOpened -= OnRewardShowSuccess;

                if (_rewardedAd != null)
                {
                    _rewardedAd.Destroy();
                    _rewardedAd = null;
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
            OnRewardedLoadFailed(adError);
        }

        private void OnAdsExhausted(string preloadId)
        {
            AdsManager.Instance.LogWarning($"{AdsNetworks}_{AdsType} Preload ad configuration {preloadId} was exhausted");
            LoadAds();
        }

        protected override bool IsAdsAvailable()
        {
            return RewardedAdPreloader.IsAdAvailable(adsUnitID) && AdsManager.Instance.IsCanShowAds;
        }


        #endregion
    }
}

#endif