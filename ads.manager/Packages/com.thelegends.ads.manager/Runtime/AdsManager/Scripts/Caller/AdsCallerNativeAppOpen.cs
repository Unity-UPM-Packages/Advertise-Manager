using System;

namespace TheLegends.Base.Ads
{
    public static partial class AdsCaller
    {
        #region NativeAppOpen

        private static readonly NativeAdFormatConfig NativeAppOpenConfig = new NativeAdFormatConfig
        {
            AdsType = AdsType.NativeAppOpen,
            LayoutPair = new NativeLayoutPair
            {
                Media1 = NativeName.Native_AppOpen_Media,
                NoMedia1 = NativeName.Native_AppOpen_No_Media,
                Media2 = null,
                NoMedia2 = null
            },
            UseLoadingAnimation = false,
            ShowToastOnUnavailable = false,
            ShouldPreloadOnUnavailable = () => AdsManager.Instance.SettingsAds.preloadSettings.nativeAds.preloadNativeAppOpen,
            ShowAction = (order, pos, layout, onShow, onClose, onDismiss, onClick) =>
                AdsManager.Instance.ShowNativeAppOpen(order, pos, layout, onShow, onClose, onDismiss, onClick),
            HideAction = order => AdsManager.Instance.HideNativeAppOpen(order),
            LoadAction = order => AdsManager.Instance.LoadNativeAppOpen(order)
        };

        public static void ShowNativeAppOpenLoop2(
            PlacementOrder currentPlacement,
            PlacementOrder nextPlacement,
            string position,
            Action onShow,
            Action onClose,
            NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
            NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            ShowLoop2Core(NativeAppOpenConfig, currentPlacement, nextPlacement, position, onShow, onClose, defaultCountdownConfig, metaCountdownConfig);
        }

        public static void ShowNativeAppOpenLoopMax(
            PlacementOrder currentPlacement,
            PlacementOrder nextPlacement,
            string position,
            Action onShow,
            Action onClose,
            NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
            NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            ShowLoopMaxCore(NativeAppOpenConfig, currentPlacement, nextPlacement, position, AdsManager.Instance.adsConfigs.maxNativeFullScreenLoadLoop, onShow, onClose, defaultCountdownConfig, metaCountdownConfig);
        }

        public static void ShowNativeAppOpenNoLoop(
            PlacementOrder placementOrder,
            string position,
            Action onShow,
            Action onClose,
            Action onAdDismissedFullScreenContent,
            NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
            NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            ShowNoLoopCore(NativeAppOpenConfig, placementOrder, position, onShow, onClose, onAdDismissedFullScreenContent, defaultCountdownConfig, metaCountdownConfig);
        }

        #endregion
    }
}
