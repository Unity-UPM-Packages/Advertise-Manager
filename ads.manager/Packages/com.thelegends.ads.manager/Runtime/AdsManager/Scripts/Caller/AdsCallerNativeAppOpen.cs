using System;
using TheLegends.Base.UI;

namespace TheLegends.Base.Ads
{
    public static partial class AdsCaller
    {
        #region NativeAppOpen

        public static void ShowNativeAppOpenLoop2(
            PlacementOrder currentPlacement,
            PlacementOrder nextPlacement,
            string position,
            Action onShow,
            Action onClose,
            NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
            NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeAppOpen, nextPlacement) != AdsEvents.LoadAvailable &&
                AdsManager.Instance.GetAdsStatus(AdsType.NativeAppOpen, currentPlacement) != AdsEvents.LoadAvailable)
            {
                if (AdsManager.Instance.SettingsAds.preloadSettings.nativeAds.preloadNativeAppOpen)
                {
                    AdsManager.Instance.LoadNativeAppOpen(currentPlacement);
                }

                onClose?.Invoke();
                return;
            }

            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeAppOpen, nextPlacement) == AdsEvents.LoadAvailable &&
                AdsManager.Instance.GetAdsStatus(AdsType.NativeAppOpen, currentPlacement) != AdsEvents.LoadAvailable)
            {
                var temp = currentPlacement;
                currentPlacement = nextPlacement;
                nextPlacement = temp;
            }

            ShowAd(currentPlacement, nextPlacement, onShow);

            void ShowAd(PlacementOrder current, PlacementOrder? next, Action currentOnShow)
            {
                var network = AdsManager.Instance.GetNetworkName(AdsType.NativeAppOpen, current);
                string layoutName = NativeName.Native_FullScreen_Media;
                NativePlatformShowBuilder.CountdownConfig countdownConfig = defaultCountdownConfig;

                if (network == "facebook" || network == "meta" || network == "fan")
                {
                    layoutName = NativeName.Native_FullScreen_No_Media;
                    countdownConfig = metaCountdownConfig;
                }

                void OnAdClose()
                {
                    AdsManager.Instance.HideNativeAppOpen(current);

                    if (next.HasValue && AdsManager.Instance.GetAdsStatus(AdsType.NativeAppOpen, next.Value) == AdsEvents.LoadAvailable)
                    {
                        ShowAd(next.Value, null, null);
                    }
                    else
                    {
                        UILoadingController.Show(1f, () => onClose?.Invoke());
                    }
                }

                AdsManager.Instance.ShowNativeAppOpen(current, position, layoutName, () =>
                {
                    if (next.HasValue)
                    {
                        AdsManager.Instance.LoadNativeAppOpen(next.Value);
                    }
                    currentOnShow?.Invoke();
                },
                () =>
                {
                    OnAdClose();
                },
                () =>
                {
                    OnAdClose();
                },
                null)
                .WithCountdown(countdownConfig.InitialDelaySeconds, countdownConfig.CountdownDurationSeconds, countdownConfig.CloseButtonDelaySeconds)
                .Execute();
            }
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
            int remainingLoops = AdsManager.Instance.adsConfigs.maxNativeFullScreenLoadLoop;

            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeAppOpen, nextPlacement) != AdsEvents.LoadAvailable &&
                AdsManager.Instance.GetAdsStatus(AdsType.NativeAppOpen, currentPlacement) != AdsEvents.LoadAvailable)
            {
                if (AdsManager.Instance.SettingsAds.preloadSettings.nativeAds.preloadNativeAppOpen)
                {
                    AdsManager.Instance.LoadNativeAppOpen(currentPlacement);
                }

                onClose?.Invoke();
                return;
            }

            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeAppOpen, nextPlacement) == AdsEvents.LoadAvailable &&
                AdsManager.Instance.GetAdsStatus(AdsType.NativeAppOpen, currentPlacement) != AdsEvents.LoadAvailable)
            {
                var temp = currentPlacement;
                currentPlacement = nextPlacement;
                nextPlacement = temp;
            }

            ShowAd(currentPlacement, nextPlacement, onShow);

            void ShowAd(PlacementOrder current, PlacementOrder next, Action currentOnShow)
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
                () =>
                {
                    UILoadingController.Show(1f, () => onClose?.Invoke());
                },
                null,
                () =>
                {
                    PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        if (remainingLoops > 0 && AdsManager.Instance.GetAdsStatus(AdsType.NativeAppOpen, next) == AdsEvents.LoadAvailable)
                        {
                            remainingLoops--;
                            AdsManager.Instance.HideNativeAppOpen(current);
                            ShowAd(next, current, null);
                        }
                    });
                })
                .WithCountdown(countdownConfig.InitialDelaySeconds, countdownConfig.CountdownDurationSeconds, countdownConfig.CloseButtonDelaySeconds)
                .Execute();
            }
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
            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeAppOpen, placementOrder) != AdsEvents.LoadAvailable)
            {
                if (AdsManager.Instance.SettingsAds.preloadSettings.nativeAds.preloadNativeAppOpen)
                {
                    AdsManager.Instance.LoadNativeAppOpen(placementOrder);
                }

                onClose?.Invoke();
                return;
            }

            var network = AdsManager.Instance.GetNetworkName(AdsType.NativeAppOpen, placementOrder);
            string layoutName = NativeName.Native_FullScreen_Media;
            NativePlatformShowBuilder.CountdownConfig countdownConfig = defaultCountdownConfig;

            if (network == "facebook" || network == "meta" || network == "fan")
            {
                layoutName = NativeName.Native_FullScreen_No_Media;
                countdownConfig = metaCountdownConfig;
            }

            AdsManager.Instance.ShowNativeAppOpen(placementOrder, position, layoutName, onShow,
                () => UILoadingController.Show(1f, () => onClose?.Invoke()),
                onAdDismissedFullScreenContent, null)
            .WithCountdown(countdownConfig.InitialDelaySeconds, countdownConfig.CountdownDurationSeconds, countdownConfig.CloseButtonDelaySeconds)
            .Execute();
        }

        #endregion
    }
}
