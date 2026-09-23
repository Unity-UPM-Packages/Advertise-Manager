using System;

namespace TheLegends.Base.Ads
{
    public static partial class AdsCaller
    {
        #region NativeInter

        private static readonly NativeAdFormatConfig NativeInterConfig = new NativeAdFormatConfig
        {
            AdsType = AdsType.NativeInter,
            LayoutPair = new NativeLayoutPair
            {
                Media = NativeName.Native_FullScreen_Media,
                NoMedia = NativeName.Native_FullScreen_No_Media
            },
            UseLoadingAnimation = true,
            ShowToastOnUnavailable = false,
            ShouldPreloadOnUnavailable = null,
            ShowAction = (order, pos, layout, onShow, onClose, onDismiss, onClick) =>
                AdsManager.Instance.ShowNativeInter(order, pos, layout, onShow, onClose, onDismiss, onClick),
            HideAction = order => AdsManager.Instance.HideNativeInter(order),
            LoadAction = order => AdsManager.Instance.LoadNativeInter(order)
        };

        public static void LoadNativeInter(PlacementOrder currentPlacement, PlacementOrder nextPlacement)
        {
            LoadDualPlacement(AdsType.NativeInter, NativeInterConfig.LoadAction, currentPlacement, nextPlacement);
        }

        public static void ShowNativeInterLoop2(
            PlacementOrder currentPlacement,
            PlacementOrder nextPlacement,
            string position,
            Action onShow,
            Action onClose,
            NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
            NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            ShowLoop2Core(NativeInterConfig, currentPlacement, nextPlacement, position, onShow, onClose, defaultCountdownConfig, metaCountdownConfig);
        }

        public static void ShowNativeInterLoopMax(
            PlacementOrder currentPlacement,
            PlacementOrder nextPlacement,
            string position,
            Action onShow,
            Action onClose,
            NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
            NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            ShowLoopMaxCore(NativeInterConfig, currentPlacement, nextPlacement, position, AdsManager.Instance.adsConfigs.maxNativeFullScreenLoadLoop, onShow, onClose, defaultCountdownConfig, metaCountdownConfig);
        }

        public static void ShowNativeInterNoLoop(
            PlacementOrder placementOrder,
            string position,
            Action onShow,
            Action onClose,
            Action onAdDismissedFullScreenContent,
            NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
            NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            ShowNoLoopCore(NativeInterConfig, placementOrder, position, onShow, onClose, onAdDismissedFullScreenContent, defaultCountdownConfig, metaCountdownConfig);
        }

        private static readonly NativeAdFormatConfig NativeHalfScreenConfig = new NativeAdFormatConfig
        {
            AdsType = AdsType.NativeInter,
            LayoutPair = new NativeLayoutPair
            {
                Media = NativeName.Native_HalfScreen_Media,
                NoMedia = NativeName.Native_HalfScreen_No_Media
            },
            UseLoadingAnimation = false,
            ShowToastOnUnavailable = false,
            ShouldPreloadOnUnavailable = null,
            ShowAction = (order, pos, layout, onShow, onClose, onDismiss, onClick) =>
                AdsManager.Instance.ShowNativeInter(order, pos, layout, onShow, onClose, onDismiss, onClick),
            HideAction = order => AdsManager.Instance.HideNativeInter(order),
            LoadAction = order => AdsManager.Instance.LoadNativeInter(order)
        };

        public static void ShowNativeInterHalfScreen(
            PlacementOrder placementOrder,
            string position,
            Action onShow,
            Action onClose,
            Action onAdDismissedFullScreenContent,
            NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
            NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            ShowNoLoopCore(NativeHalfScreenConfig, placementOrder, position, onShow, onClose, onAdDismissedFullScreenContent, defaultCountdownConfig, metaCountdownConfig);
        }

        #endregion
    }
}
