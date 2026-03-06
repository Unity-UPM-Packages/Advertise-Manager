#if USE_ADMOB
using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using TheLegends.Base.UI;

namespace TheLegends.Base.Ads
{
    public class AdmobInterstitialOpenController : AdsPlacementBase
	{
		private InterstitialAd _interstitialAd;

		protected Action OnClose;

		public override void LoadAds()
		{
#if USE_ADMOB
			if (!IsCanLoadAds())
			{
				return;
			}

			if (IsReady)
			{
				return;
			}

			base.LoadAds();
			AdRequest request = new AdRequest();

			InterstitialAd.Load(adsUnitID.Trim(), request,
				(InterstitialAd ad, LoadAdError error) =>
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
						AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + "ad failed to load with error : " + error);
						OnInterLoadFailed(error);
						return;
					}

					if (ad == null)
					{
						AdsManager.Instance.LogError($"{AdsNetworks}_{AdsType} " + "Unexpected error: load event fired with null ad and null error.");
						OnInterLoadFailed(error);
						return;
					}

					AdsManager.Instance.Log($"{AdsNetworks}_{AdsType} " + "ad loaded with response : " + ad.GetResponseInfo());

					_interstitialAd = ad;

					OnAdsLoadAvailable();
				});
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
				_interstitialAd.OnAdClicked += OnInterClick;
				_interstitialAd.OnAdPaid += OnInterPaid;
				_interstitialAd.OnAdImpressionRecorded += OnInterImpression;
				_interstitialAd.OnAdFullScreenContentClosed += OnInterClosed;
				_interstitialAd.OnAdFullScreenContentFailed += OnInterShowFailed;
				_interstitialAd.OnAdFullScreenContentOpened += OnInterShowSuccess;
				_interstitialAd.Show();
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
			return AdsType.InterOpen;
#else
            return AdsType.None;
#endif
		}

		public override bool IsAdsReady()
		{
#if USE_ADMOB
			if (_interstitialAd != null)
			{
				return _interstitialAd.CanShowAd();
			}

			return false;
#else
            return false;
#endif
		}


		#region Internal

		private void OnInterClick()
		{
			MobileAdsEventExecutor.ExecuteInUpdate(() =>
			{
				OnAdsClick();
			});
		}

		private void OnInterImpression()
		{
			MobileAdsEventExecutor.ExecuteInUpdate(() =>
			{
				OnImpression();
			});
		}

		private void OnInterShowSuccess()
		{
			MobileAdsEventExecutor.ExecuteInUpdate(() =>
			{
				OnAdsShowSuccess();
				AdsManager.Instance.OnFullScreenAdsShow();
			});
		}

		private void OnInterLoadFailed(AdError error)
		{
			var errorDescription = error?.GetMessage();
			OnAdsLoadFailed(errorDescription);
		}

		private void OnInterShowFailed(AdError error)
		{
			MobileAdsEventExecutor.ExecuteInUpdate(() =>
			{
				var errorDescription = error?.GetMessage();
				OnAdsShowFailed(errorDescription);
			});
		}

		protected virtual void OnInterClosed()
		{
			MobileAdsEventExecutor.ExecuteInUpdate(() =>
			{
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

				OnClose?.Invoke();
				OnClose = null;
				Status = AdsEvents.Close;

			});
		}

		private void OnInterPaid(AdValue value)
		{
			MobileAdsEventExecutor.ExecuteInUpdate(() =>
			{
				AdsManager.Instance.LogImpressionData(AdsNetworks, AdsType, adsUnitID, value);
			});

		}

		#endregion


        protected override void SetTimeOut()
        {
#if USE_ADMOB
            timeOut = AdsManager.Instance.adsConfigs.adInterOpenTimeOut;
#endif
        }
    }

}

#endif
