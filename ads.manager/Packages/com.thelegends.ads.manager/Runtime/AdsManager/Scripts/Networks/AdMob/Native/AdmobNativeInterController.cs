#if USE_ADMOB
using System;
using System.Collections;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using TheLegends.Base.Firebase;
using TheLegends.Base.UI;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class AdmobNativeInterController : AdmobNativePlatformController
    {
        public override AdsType GetAdsType()
        {
#if USE_ADMOB
            return AdsType.NativeInter;
#else
            return AdsType.None;
#endif
        }

        protected override void OnNativePlatformClosed()
        {
#if USE_ADMOB
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                UILoadingController.Show(1f, () =>
                {
                    OnClose?.Invoke();
                    AdsManager.Instance.OnFullScreenAdsClosed();
                });
                OnAdsClosed();
            });
#endif
        }

        protected override void OnNativePlatformShow()
        {
#if USE_ADMOB
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                OnAdsShowSuccess();
                OnShow?.Invoke();
                AdsManager.Instance.OnFullScreenAdsShow();
            });
#endif
        }
    }
}
#endif
