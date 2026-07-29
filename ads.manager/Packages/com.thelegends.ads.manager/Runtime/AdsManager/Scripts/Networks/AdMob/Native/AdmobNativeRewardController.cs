#if USE_ADMOB
using TheLegends.Base.UI;

namespace TheLegends.Base.Ads
{
    public class AdmobNativeRewardController : AdmobNativePlatformController
    {
        public override AdsType GetAdsType()
        {
#if USE_ADMOB
            return AdsType.NativeReward;
#else
            return AdsType.None;
#endif
        }

        protected override void OnNativePlatformClosed()
        {
#if USE_ADMOB
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
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
            PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
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
