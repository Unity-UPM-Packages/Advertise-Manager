using System;
using System.Collections;
using TheLegends.Base.UI;

namespace TheLegends.Base.Ads
{
    public static partial class AdsCaller
    {
        #region NativeReward

        public static void LoadNativeReward(PlacementOrder currentPlacement, PlacementOrder nextPlacement)
        {
            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeReward, currentPlacement) == AdsEvents.LoadAvailable ||
                AdsManager.Instance.GetAdsStatus(AdsType.NativeReward, nextPlacement) == AdsEvents.LoadAvailable)
            {
                return;
            }

            AdsManager.Instance.StartCoroutine(IELoadNativeReward(currentPlacement, nextPlacement));
        }

        private static IEnumerator IELoadNativeReward(PlacementOrder currentPlacement, PlacementOrder nextPlacement)
        {
            AdsManager.Instance.LoadNativeReward(currentPlacement);
            yield return AdsManager.Instance.WaitAdLoaded(AdsType.NativeReward, currentPlacement);

            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeReward, currentPlacement) == AdsEvents.LoadNotAvailable &&
                AdsManager.Instance.GetAdsStatus(AdsType.NativeReward, nextPlacement) != AdsEvents.LoadAvailable)
            {
                AdsManager.Instance.LoadNativeReward(nextPlacement);
            }
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
            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeReward, nextPlacement) != AdsEvents.LoadAvailable &&
                AdsManager.Instance.GetAdsStatus(AdsType.NativeReward, currentPlacement) != AdsEvents.LoadAvailable)
            {
                UIToatsController.Show("Ads not available", 0.5f, ToastPosition.BottomCenter);

                if (AdsManager.Instance.SettingsAds.preloadSettings.nativeAds.preloadNativeReward)
                {
                    AdsManager.Instance.LoadNativeReward(currentPlacement);
                }
                return;
            }

            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeReward, nextPlacement) == AdsEvents.LoadAvailable &&
                AdsManager.Instance.GetAdsStatus(AdsType.NativeReward, currentPlacement) != AdsEvents.LoadAvailable)
            {
                var temp = currentPlacement;
                currentPlacement = nextPlacement;
                nextPlacement = temp;
            }

            ShowAd(currentPlacement, nextPlacement, onShow);

            void ShowAd(PlacementOrder current, PlacementOrder? next, Action currentOnShow)
            {
                var network = AdsManager.Instance.GetNetworkName(AdsType.NativeReward, current);
                string layoutName = NativeName.Native_FullScreen_Media;
                NativePlatformShowBuilder.CountdownConfig countdownConfig = defaultCountdownConfig;

                if (network == "facebook" || network == "meta" || network == "fan")
                {
                    layoutName = NativeName.Native_FullScreen_No_Media;
                    countdownConfig = metaCountdownConfig;
                }

                void OnAdClose()
                {
                    AdsManager.Instance.HideNativeReward(current);

                    if (next.HasValue && AdsManager.Instance.GetAdsStatus(AdsType.NativeReward, next.Value) == AdsEvents.LoadAvailable)
                    {
                        ShowAd(next.Value, null, null);
                    }
                    else
                    {
                        UILoadingController.Show(1f, () => onClose?.Invoke());
                    }
                }

                AdsManager.Instance.ShowNativeReward(current, position, layoutName, () =>
                {
                    if (next.HasValue)
                    {
                        AdsManager.Instance.LoadNativeReward(next.Value);
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

        public static void ShowNativeRewardLoopMax(
            PlacementOrder currentPlacement,
            PlacementOrder nextPlacement,
            string position,
            Action onShow,
            Action onClose,
            NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
            NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            int remainingLoops = AdsManager.Instance.adsConfigs.maxNativeRewardLoadLoop;

            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeReward, nextPlacement) != AdsEvents.LoadAvailable &&
                AdsManager.Instance.GetAdsStatus(AdsType.NativeReward, currentPlacement) != AdsEvents.LoadAvailable)
            {
                UIToatsController.Show("Ads not available", 0.5f, ToastPosition.BottomCenter);

                if (AdsManager.Instance.SettingsAds.preloadSettings.nativeAds.preloadNativeReward)
                {
                    AdsManager.Instance.LoadNativeReward(currentPlacement);
                }
                return;
            }

            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeReward, nextPlacement) == AdsEvents.LoadAvailable &&
                AdsManager.Instance.GetAdsStatus(AdsType.NativeReward, currentPlacement) != AdsEvents.LoadAvailable)
            {
                var temp = currentPlacement;
                currentPlacement = nextPlacement;
                nextPlacement = temp;
            }

            ShowAd(currentPlacement, nextPlacement, onShow);

            void ShowAd(PlacementOrder current, PlacementOrder next, Action currentOnShow)
            {
                var network = AdsManager.Instance.GetNetworkName(AdsType.NativeReward, current);
                string layoutName = NativeName.Native_FullScreen_Media;
                NativePlatformShowBuilder.CountdownConfig countdownConfig = defaultCountdownConfig;

                if (network == "facebook" || network == "meta" || network == "fan")
                {
                    layoutName = NativeName.Native_FullScreen_No_Media;
                    countdownConfig = metaCountdownConfig;
                }

                AdsManager.Instance.ShowNativeReward(current, position, layoutName, () =>
                {
                    if (remainingLoops > 0)
                    {
                        AdsManager.Instance.LoadNativeReward(next);
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
                        if (remainingLoops > 0 && AdsManager.Instance.GetAdsStatus(AdsType.NativeReward, next) == AdsEvents.LoadAvailable)
                        {
                            remainingLoops--;
                            AdsManager.Instance.HideNativeReward(current);
                            ShowAd(next, current, null);
                        }
                    });
                })
                .WithCountdown(countdownConfig.InitialDelaySeconds, countdownConfig.CountdownDurationSeconds, countdownConfig.CloseButtonDelaySeconds)
                .Execute();
            }
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
            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeReward, placementOrder) != AdsEvents.LoadAvailable)
            {
                UIToatsController.Show("Ads not available", 0.5f, ToastPosition.BottomCenter);

                if (AdsManager.Instance.SettingsAds.preloadSettings.nativeAds.preloadNativeReward)
                {
                    AdsManager.Instance.LoadNativeReward(placementOrder);
                }
                return;
            }

            var network = AdsManager.Instance.GetNetworkName(AdsType.NativeReward, placementOrder);
            string layoutName = NativeName.Native_FullScreen_Media;
            NativePlatformShowBuilder.CountdownConfig countdownConfig = defaultCountdownConfig;

            if (network == "facebook" || network == "meta" || network == "fan")
            {
                layoutName = NativeName.Native_FullScreen_No_Media;
                countdownConfig = metaCountdownConfig;
            }

            AdsManager.Instance.ShowNativeReward(placementOrder, position, layoutName, onShow,
                () => UILoadingController.Show(1f, () => onClose?.Invoke()),
                onAdDismissedFullScreenContent, null)
            .WithCountdown(countdownConfig.InitialDelaySeconds, countdownConfig.CountdownDurationSeconds, countdownConfig.CloseButtonDelaySeconds)
            .Execute();
        }

        #endregion
    }
}
