using System;

namespace TheLegends.Base.Ads
{
    public static partial class AdsCaller
    {
        #region NativeInterOpen

        private static readonly NativeAdFormatConfig NativeInterOpenConfig = new NativeAdFormatConfig
        {
            AdsType = AdsType.NativeInterOpen,
            LayoutPair = new NativeLayoutPair
            {
                Media = NativeName.Native_FullScreen_Media,
                NoMedia = NativeName.Native_FullScreen_No_Media
            },
            UseLoadingAnimation = false,
            ShowToastOnUnavailable = false,
            ShouldPreloadOnUnavailable = null,
            ShowAction = (order, pos, layout, onShow, onClose, onDismiss, onClick) =>
                AdsManager.Instance.ShowNativeInterOpen(order, pos, layout, onShow, onClose, onDismiss, onClick),
            HideAction = order => AdsManager.Instance.HideNativeInterOpen(order),
            LoadAction = order => AdsManager.Instance.LoadNativeInterOpen(order)
        };

        public static void LoadNativeInterOpen(PlacementOrder currentPlacement, PlacementOrder nextPlacement)
        {
            LoadDualPlacement(AdsType.NativeInterOpen, NativeInterOpenConfig.LoadAction, currentPlacement, nextPlacement);
        }

        public static void ShowNativeInterOpenLoop2(
            PlacementOrder currentPlacement,
            PlacementOrder nextPlacement,
            string position,
            Action onShow,
            Action onClose,
            NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
            NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            ShowLoop2Core(NativeInterOpenConfig, currentPlacement, nextPlacement, position, onShow, onClose, defaultCountdownConfig, metaCountdownConfig);
        }

        public static void ShowNativeInterOpenLoopMax(
            PlacementOrder currentPlacement,
            PlacementOrder nextPlacement,
            string position,
            Action onShow,
            Action onClose,
            NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
            NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            ShowLoopMaxCore(NativeInterOpenConfig, currentPlacement, nextPlacement, position, AdsManager.Instance.adsConfigs.maxNativeFullScreenLoadLoop, onShow, onClose, defaultCountdownConfig, metaCountdownConfig);
        }

        public static void ShowNativeInterOpenNoLoop(
            PlacementOrder placementOrder,
            string position,
            Action onShow,
            Action onClose,
            Action onAdDismissedFullScreenContent,
            NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
            NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            ShowNoLoopCore(NativeInterOpenConfig, placementOrder, position, onShow, onClose, onAdDismissedFullScreenContent, defaultCountdownConfig, metaCountdownConfig);
        }

        #endregion
    }
}
