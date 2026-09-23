using System;

namespace TheLegends.Base.Ads
{
    public static partial class AdsCaller
    {
        #region NativeReward

        private static readonly NativeAdFormatConfig NativeRewardConfig = new NativeAdFormatConfig
        {
            AdsType = AdsType.NativeReward,
            LayoutPair = new NativeLayoutPair
            {
                Media = NativeName.Native_FullScreen_Media,
                NoMedia = NativeName.Native_FullScreen_No_Media
            },
            UseLoadingAnimation = true,
            ShowToastOnUnavailable = true,
            ShouldPreloadOnUnavailable = () => AdsManager.Instance.SettingsAds.preloadSettings.nativeAds.preloadNativeReward,
            ShowAction = (order, pos, layout, onShow, onClose, onDismiss, onClick) =>
                AdsManager.Instance.ShowNativeReward(order, pos, layout, onShow, onClose, onDismiss, onClick),
            HideAction = order => AdsManager.Instance.HideNativeReward(order),
            LoadAction = order => AdsManager.Instance.LoadNativeReward(order)
        };

        public static void LoadNativeReward(PlacementOrder currentPlacement, PlacementOrder nextPlacement)
        {
            LoadDualPlacement(AdsType.NativeReward, NativeRewardConfig.LoadAction, currentPlacement, nextPlacement);
        }

        private static void WrapRewardCallbacks(Action onShow, Action onClose, out Action wrappedOnShow, out Action wrappedOnClose)
        {
            bool isAdShowed = false;
            wrappedOnShow = () =>
            {
                isAdShowed = true;
                onShow?.Invoke();
            };
            wrappedOnClose = () =>
            {
                if (isAdShowed)
                {
                    onClose?.Invoke();
                }
            };
        }

        public static void ShowNativeRewardLoop2(
            PlacementOrder currentPlacement,
            PlacementOrder nextPlacement,
            string position,
            Action onShow,
            Action onClose,
            NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
            NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            WrapRewardCallbacks(onShow, onClose, out var wrappedOnShow, out var wrappedOnClose);
            ShowLoop2Core(NativeRewardConfig, currentPlacement, nextPlacement, position, wrappedOnShow, wrappedOnClose, defaultCountdownConfig, metaCountdownConfig);
        }

        public static void ShowNativeRewardLoopMax(
            PlacementOrder currentPlacement,
            PlacementOrder nextPlacement,
            string position,
            Action onShow,
            Action onClose,
            NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
            NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            WrapRewardCallbacks(onShow, onClose, out var wrappedOnShow, out var wrappedOnClose);
            ShowLoopMaxCore(NativeRewardConfig, currentPlacement, nextPlacement, position, AdsManager.Instance.adsConfigs.maxNativeRewardLoadLoop, wrappedOnShow, wrappedOnClose, defaultCountdownConfig, metaCountdownConfig);
        }

        public static void ShowNativeRewardNoLoop(
            PlacementOrder placementOrder,
            string position,
            Action onShow,
            Action onClose,
            Action onAdDismissedFullScreenContent,
            NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
            NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            WrapRewardCallbacks(onShow, onClose, out var wrappedOnShow, out var wrappedOnClose);
            ShowNoLoopCore(NativeRewardConfig, placementOrder, position, wrappedOnShow, wrappedOnClose, onAdDismissedFullScreenContent, defaultCountdownConfig, metaCountdownConfig);
        }

        #endregion
    }
}
