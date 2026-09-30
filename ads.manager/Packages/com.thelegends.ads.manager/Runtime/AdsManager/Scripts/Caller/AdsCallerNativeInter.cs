using System;
using System.Collections;
using TheLegends.Base.UI;

namespace TheLegends.Base.Ads
{
    public static partial class AdsCaller
    {
        #region NativeInter

        public static void LoadNativeInter(PlacementOrder currentPlacement, PlacementOrder nextPlacement)
        {
            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeInter, currentPlacement) == AdsEvents.LoadAvailable ||
                AdsManager.Instance.GetAdsStatus(AdsType.NativeInter, nextPlacement) == AdsEvents.LoadAvailable)
            {
                return;
            }

            AdsManager.Instance.StartCoroutine(IELoadNativeInter(currentPlacement, nextPlacement));
        }

        private static IEnumerator IELoadNativeInter(PlacementOrder currentPlacement, PlacementOrder nextPlacement)
        {
            AdsManager.Instance.LoadNativeInter(currentPlacement);
            yield return AdsManager.Instance.WaitAdLoaded(AdsType.NativeInter, currentPlacement);

            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeInter, currentPlacement) == AdsEvents.LoadNotAvailable &&
                AdsManager.Instance.GetAdsStatus(AdsType.NativeInter, nextPlacement) != AdsEvents.LoadAvailable)
            {
                AdsManager.Instance.LoadNativeInter(nextPlacement);
            }
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
            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeInter, nextPlacement) != AdsEvents.LoadAvailable &&
                AdsManager.Instance.GetAdsStatus(AdsType.NativeInter, currentPlacement) != AdsEvents.LoadAvailable)
            {
                onClose?.Invoke();
                return;
            }

            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeInter, nextPlacement) == AdsEvents.LoadAvailable &&
                AdsManager.Instance.GetAdsStatus(AdsType.NativeInter, currentPlacement) != AdsEvents.LoadAvailable)
            {
                var temp = currentPlacement;
                currentPlacement = nextPlacement;
                nextPlacement = temp;
            }

            ShowAd(currentPlacement, nextPlacement, onShow);

            void ShowAd(PlacementOrder current, PlacementOrder? next, Action currentOnShow)
            {
                var network = AdsManager.Instance.GetNetworkName(AdsType.NativeInter, current);
                string layoutName = NativeName.Native_FullScreen_Media;
                NativePlatformShowBuilder.CountdownConfig countdownConfig = defaultCountdownConfig;

                if (network == "facebook" || network == "meta" || network == "fan")
                {
                    layoutName = NativeName.Native_FullScreen_No_Media;
                    countdownConfig = metaCountdownConfig;
                }

                void OnAdClose()
                {
                    AdsManager.Instance.HideNativeInter(current);

                    if (next.HasValue && AdsManager.Instance.GetAdsStatus(AdsType.NativeInter, next.Value) == AdsEvents.LoadAvailable)
                    {
                        ShowAd(next.Value, null, null);
                    }
                    else
                    {
                        UILoadingController.Show(1f, () => onClose?.Invoke());
                    }
                }

                AdsManager.Instance.ShowNativeInter(current, position, layoutName, () =>
                {
                    if (next.HasValue)
                    {
                        AdsManager.Instance.LoadNativeInter(next.Value);
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
                }, null)
                .WithCountdown(countdownConfig.InitialDelaySeconds, countdownConfig.CountdownDurationSeconds, countdownConfig.CloseButtonDelaySeconds)
                .Execute();
            }
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
            int remainingLoops = AdsManager.Instance.adsConfigs.maxNativeFullScreenLoadLoop;

            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeInter, nextPlacement) != AdsEvents.LoadAvailable &&
                AdsManager.Instance.GetAdsStatus(AdsType.NativeInter, currentPlacement) != AdsEvents.LoadAvailable)
            {
                onClose?.Invoke();
                return;
            }

            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeInter, nextPlacement) == AdsEvents.LoadAvailable &&
                AdsManager.Instance.GetAdsStatus(AdsType.NativeInter, currentPlacement) != AdsEvents.LoadAvailable)
            {
                var temp = currentPlacement;
                currentPlacement = nextPlacement;
                nextPlacement = temp;
            }

            ShowAd(currentPlacement, nextPlacement, onShow);

            void ShowAd(PlacementOrder current, PlacementOrder next, Action currentOnShow)
            {
                var network = AdsManager.Instance.GetNetworkName(AdsType.NativeInter, current);
                string layoutName = NativeName.Native_FullScreen_Media;
                NativePlatformShowBuilder.CountdownConfig countdownConfig = defaultCountdownConfig;

                if (network == "facebook" || network == "meta" || network == "fan")
                {
                    layoutName = NativeName.Native_FullScreen_No_Media;
                    countdownConfig = metaCountdownConfig;
                }

                AdsManager.Instance.ShowNativeInter(current, position, layoutName, () =>
                {
                    if (remainingLoops > 0)
                    {
                        AdsManager.Instance.LoadNativeInter(next);
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
                        if (remainingLoops > 0 && AdsManager.Instance.GetAdsStatus(AdsType.NativeInter, next) == AdsEvents.LoadAvailable)
                        {
                            remainingLoops--;
                            AdsManager.Instance.HideNativeInter(current);
                            ShowAd(next, current, null);
                        }
                    });
                })
                .WithCountdown(countdownConfig.InitialDelaySeconds, countdownConfig.CountdownDurationSeconds, countdownConfig.CloseButtonDelaySeconds)
                .Execute();
            }
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
            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeInter, placementOrder) != AdsEvents.LoadAvailable)
            {
                onClose?.Invoke();
                return;
            }

            var network = AdsManager.Instance.GetNetworkName(AdsType.NativeInter, placementOrder);
            string layoutName = NativeName.Native_FullScreen_Media;
            NativePlatformShowBuilder.CountdownConfig countdownConfig = defaultCountdownConfig;

            if (network == "facebook" || network == "meta" || network == "fan")
            {
                layoutName = NativeName.Native_FullScreen_No_Media;
                countdownConfig = metaCountdownConfig;
            }

            AdsManager.Instance.ShowNativeInter(placementOrder, position, layoutName, onShow,
            () =>
            {
                UILoadingController.Show(1f, () => onClose?.Invoke());
            },
            onAdDismissedFullScreenContent, null)
            .WithCountdown(countdownConfig.InitialDelaySeconds, countdownConfig.CountdownDurationSeconds, countdownConfig.CloseButtonDelaySeconds)
            .Execute();
        }

        public static void ShowNativeInterHalfScreen(
            PlacementOrder placementOrder,
            string position,
            Action onShow,
            Action onClose,
            Action onAdDismissedFullScreenContent,
            NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
            NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeInter, placementOrder) != AdsEvents.LoadAvailable)
            {
                onClose?.Invoke();
                return;
            }

            var network = AdsManager.Instance.GetNetworkName(AdsType.NativeInter, placementOrder);
            string layoutName = NativeName.Native_HalfScreen_Media;
            NativePlatformShowBuilder.CountdownConfig countdownConfig = defaultCountdownConfig;

            if (network == "facebook" || network == "meta" || network == "fan")
            {
                layoutName = NativeName.Native_HalfScreen_No_Media;
                countdownConfig = metaCountdownConfig;
            }

            AdsManager.Instance.ShowNativeInter(placementOrder, position, layoutName, onShow, onClose, onAdDismissedFullScreenContent, null)
            .WithCountdown(countdownConfig.InitialDelaySeconds, countdownConfig.CountdownDurationSeconds, countdownConfig.CloseButtonDelaySeconds)
            .Execute();
        }

        #endregion
    }
}
