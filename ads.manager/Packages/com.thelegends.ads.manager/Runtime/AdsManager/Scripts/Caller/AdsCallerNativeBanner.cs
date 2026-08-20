using System;

namespace TheLegends.Base.Ads
{
    public static partial class AdsCaller
    {
        #region NativeBanner

        public static void ShowNativeBanner(PlacementOrder placementOrder, string position, Action onShow, Action onClose, Action OnAdDismissedFullScreenContent, Action OnClick)
        {
            string layoutName = NativeName.Native_Banner;

            AdsManager.Instance.ShowNativeBanner(placementOrder, position, layoutName, onShow, onClose, OnAdDismissedFullScreenContent, OnClick)
            .WithAutoReload(AdsManager.Instance.adsConfigs.nativeBannerTimeReload)
            .WithShowOnLoaded(true)
            .Execute();
        }

        #endregion
    }
}
