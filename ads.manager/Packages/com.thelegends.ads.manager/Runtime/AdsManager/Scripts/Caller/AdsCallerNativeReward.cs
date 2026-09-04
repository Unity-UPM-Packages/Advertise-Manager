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
                Media1 = NativeName.Native_Reward_Media,
                NoMedia1 = NativeName.Native_Reward_No_Media,
                Media2 = NativeName.Native_Reward_Media_2,
                NoMedia2 = NativeName.Native_Reward_No_Media_2
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

        public static void ShowNativeRewardLoop2(
            PlacementOrder currentPlacement,
            PlacementOrder nextPlacement,
            string position,
            Action onShow,
            Action onClose,
            NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
            NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            ShowLoop2Core(NativeRewardConfig, currentPlacement, nextPlacement, position, onShow, onClose, defaultCountdownConfig, metaCountdownConfig);
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
            ShowLoopMaxCore(NativeRewardConfig, currentPlacement, nextPlacement, position, AdsManager.Instance.adsConfigs.maxNativeRewardLoadLoop, onShow, onClose, defaultCountdownConfig, metaCountdownConfig);
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
            ShowNoLoopCore(NativeRewardConfig, placementOrder, position, onShow, onClose, onAdDismissedFullScreenContent, defaultCountdownConfig, metaCountdownConfig);
        }

        #endregion
    }
}
