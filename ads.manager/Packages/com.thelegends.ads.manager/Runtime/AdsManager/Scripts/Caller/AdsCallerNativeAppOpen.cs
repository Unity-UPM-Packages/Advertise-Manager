using System;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public static partial class AdsCaller
    {
        #region NativeAppOpen

        public static void ShowNativeAppOpen(
            PlacementOrder currentPlacement,
            PlacementOrder nextPlacement,
            string position,
            Action onShow,
            Action onClose,
            NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
            NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            int remainingLoops = AdsManager.Instance.adsConfigs.maxNativeFullScreenLoadLoop;

            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeAppOpen, nextPlacement) == AdsEvents.LoadAvailable &&
            AdsManager.Instance.GetAdsStatus(AdsType.NativeAppOpen, currentPlacement) != AdsEvents.LoadAvailable)
            {
                var temp = currentPlacement;
                currentPlacement = nextPlacement;
                nextPlacement = temp;
            }

            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeAppOpen, currentPlacement) == AdsEvents.LoadAvailable)
            {
                ShowLoop(currentPlacement, nextPlacement, onShow);
            }
            else
            {
                if (AdsManager.Instance.SettingsAds.preloadSettings.nativeAds.preloadNativeAppOpen)
                {
                    AdsManager.Instance.LoadNativeAppOpen(currentPlacement);
                }
            }

            void ShowLoop(PlacementOrder current, PlacementOrder next, Action currentOnShow)
            {
                var network = AdsManager.Instance.GetNetworkName(AdsType.NativeAppOpen, current);
                string layoutName = NativeName.Native_FullScreen_Media;

                NativePlatformShowBuilder.CountdownConfig countdownConfig = defaultCountdownConfig;

                if (network == "facebook" || network == "meta" || network == "fan")
                {
                    layoutName = NativeName.Native_FullScreen_No_Media;
                    countdownConfig = metaCountdownConfig;
                }

                AdsManager.Instance.ShowNativeAppOpen(current, position, layoutName, () =>
                {
                    if (remainingLoops > 0)
                    {
                        AdsManager.Instance.LoadNativeAppOpen(next);
                    }
                    currentOnShow?.Invoke();
                },
                onClose, null, () =>
                {
                    PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        if (remainingLoops > 0 && AdsManager.Instance.GetAdsStatus(AdsType.NativeAppOpen, next) == AdsEvents.LoadAvailable)
                        {
                            remainingLoops--;
                            AdsManager.Instance.HideNativeAppOpen(current);
                            ShowLoop(next, current, null);
                        }
                    });

                })
                .WithCountdown(countdownConfig.InitialDelaySeconds, countdownConfig.CountdownDurationSeconds, countdownConfig.CloseButtonDelaySeconds)
                .Execute();
            }
        }

        public static void ShowNativeAppOpenNoLoop(PlacementOrder placementOrder,
        string position,
        Action onShow,
        Action onClose,
        Action onAdDismissedFullScreenContent,
        NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
        NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeAppOpen, placementOrder) == AdsEvents.LoadAvailable)
            {
                var network = AdsManager.Instance.GetNetworkName(AdsType.NativeAppOpen, placementOrder);
                string layoutName = NativeName.Native_FullScreen_Media;
                NativePlatformShowBuilder.CountdownConfig countdownConfig = defaultCountdownConfig;

                if (network == "facebook" || network == "meta" || network == "fan")
                {
                    layoutName = NativeName.Native_FullScreen_No_Media;
                    countdownConfig = metaCountdownConfig;
                }

                AdsManager.Instance.ShowNativeAppOpen(placementOrder, position, layoutName, onShow, onClose, onAdDismissedFullScreenContent, null)
                .WithCountdown(countdownConfig.InitialDelaySeconds, countdownConfig.CountdownDurationSeconds, countdownConfig.CloseButtonDelaySeconds)
                .Execute();
            }
            else
            {
                if (AdsManager.Instance.SettingsAds.preloadSettings.nativeAds.preloadNativeAppOpen)
                {

                    AdsManager.Instance.LoadNativeAppOpen(placementOrder);
                }
            }
        }

        #endregion
    }
}
