using System;
using System.Collections;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public static class AdsCaller
    {
        public static void ShowNativeInter(
            PlacementOrder currentPlacement,
            PlacementOrder nextPlacement,
            string position,
            Action onShow,
            Action onClose,
            NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
            NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            var network = AdsManager.Instance.GetNetworkName(AdsType.NativeInter, currentPlacement);
            string layoutName = NativeName.Native_FullScreen_Media;

            NativePlatformShowBuilder.CountdownConfig countdownConfig = defaultCountdownConfig;

            if (network == "facebook" || network == "meta" || network == "fan")
            {
                layoutName = NativeName.Native_FullScreen_No_Media;
                countdownConfig = metaCountdownConfig;
            }
            AdsManager.Instance.ShowNativeInter(currentPlacement, position, layoutName, () =>
            {
                AdsManager.Instance.LoadNativeInter(nextPlacement);
                onShow?.Invoke();
            },
            onClose, null, () =>
            {
                PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    if (AdsManager.Instance.GetAdsStatus(AdsType.NativeInter, nextPlacement) == AdsEvents.LoadAvailable)
                    {
                        AdsManager.Instance.HideNativeInter(currentPlacement);
                        ShowNativeInter(nextPlacement, currentPlacement, position, null, onClose, defaultCountdownConfig, metaCountdownConfig);
                    }
                });

            })
            .WithCountdown(countdownConfig.InitialDelaySeconds, countdownConfig.CountdownDurationSeconds, countdownConfig.CloseButtonDelaySeconds)
            .Execute();
        }

        public static void ShowNativeInterOpen(
            PlacementOrder currentPlacement,
            PlacementOrder nextPlacement,
            string position,
            Action onShow,
            Action onClose,
            NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
            NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            var network = AdsManager.Instance.GetNetworkName(AdsType.NativeInterOpen, currentPlacement);
            string layoutName = NativeName.Native_FullScreen_Media;

            NativePlatformShowBuilder.CountdownConfig countdownConfig = defaultCountdownConfig;

            if (network == "facebook" || network == "meta" || network == "fan")
            {
                layoutName = NativeName.Native_FullScreen_No_Media;
                countdownConfig = metaCountdownConfig;
            }
            AdsManager.Instance.ShowNativeInterOpen(currentPlacement, position, layoutName, () =>
            {
                AdsManager.Instance.LoadNativeInterOpen(nextPlacement);
                onShow?.Invoke();
            },
            onClose, null, () =>
            {
                PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    if (AdsManager.Instance.GetAdsStatus(AdsType.NativeInterOpen, nextPlacement) == AdsEvents.LoadAvailable)
                    {
                        AdsManager.Instance.HideNativeInterOpen(currentPlacement);
                        ShowNativeInterOpen(nextPlacement, currentPlacement, position, null, onClose, defaultCountdownConfig, metaCountdownConfig);
                    }
                });

            })
            .WithCountdown(countdownConfig.InitialDelaySeconds, countdownConfig.CountdownDurationSeconds, countdownConfig.CloseButtonDelaySeconds)
            .Execute();
        }

        public static void ShowNativeAppOpen(
            PlacementOrder currentPlacement,
            PlacementOrder nextPlacement,
            string position,
            Action onShow,
            Action onClose,
            NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
            NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            var network = AdsManager.Instance.GetNetworkName(AdsType.NativeAppOpen, currentPlacement);
            string layoutName = NativeName.Native_FullScreen_Media;

            NativePlatformShowBuilder.CountdownConfig countdownConfig = defaultCountdownConfig;

            if (network == "facebook" || network == "meta" || network == "fan")
            {
                layoutName = NativeName.Native_FullScreen_No_Media;
                countdownConfig = metaCountdownConfig;
            }
            AdsManager.Instance.ShowNativeAppOpen(currentPlacement, position, layoutName, () =>
            {
                AdsManager.Instance.LoadNativeAppOpen(nextPlacement);
                onShow?.Invoke();
            },
            onClose, null, () =>
            {
                PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    if (AdsManager.Instance.GetAdsStatus(AdsType.NativeAppOpen, nextPlacement) == AdsEvents.LoadAvailable)
                    {
                        AdsManager.Instance.HideNativeAppOpen(currentPlacement);
                        ShowNativeAppOpen(nextPlacement, currentPlacement, position, null, onClose, defaultCountdownConfig, metaCountdownConfig);
                    }
                });

            })
            .WithCountdown(countdownConfig.InitialDelaySeconds, countdownConfig.CountdownDurationSeconds, countdownConfig.CloseButtonDelaySeconds)
            .Execute();
        }

        public static void ShowNativeReward(
            PlacementOrder currentPlacement,
            PlacementOrder nextPlacement,
            string position,
            Action onShow,
            Action onClose,
            NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
            NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            var network = AdsManager.Instance.GetNetworkName(AdsType.NativeReward, currentPlacement);
            string layoutName = NativeName.Native_FullScreen_Media;

            NativePlatformShowBuilder.CountdownConfig countdownConfig = defaultCountdownConfig;

            if (network == "facebook" || network == "meta" || network == "fan")
            {
                layoutName = NativeName.Native_FullScreen_No_Media;
                countdownConfig = metaCountdownConfig;
            }
            AdsManager.Instance.ShowNativeReward(currentPlacement, position, layoutName, () =>
            {
                AdsManager.Instance.LoadNativeReward(nextPlacement);
                onShow?.Invoke();
            },
            onClose, null, () =>
            {
                PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    if (AdsManager.Instance.GetAdsStatus(AdsType.NativeReward, nextPlacement) == AdsEvents.LoadAvailable)
                    {
                        AdsManager.Instance.HideNativeReward(currentPlacement);
                        ShowNativeReward(nextPlacement, currentPlacement, position, null, onClose, defaultCountdownConfig, metaCountdownConfig);
                    }
                });

            })
            .WithCountdown(countdownConfig.InitialDelaySeconds, countdownConfig.CountdownDurationSeconds, countdownConfig.CloseButtonDelaySeconds)
            .Execute();
        }

        public static void ShowNativeInterHalfScreen(PlacementOrder placementOrder, string position, Action onShow, Action onClose, Action OnAdDismissedFullScreenContent, NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig, NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            var network = AdsManager.Instance.GetNetworkName(AdsType.NativeInter, placementOrder);
            string layoutName = NativeName.Native_HalfScreen_Media;
            NativePlatformShowBuilder.CountdownConfig countdownConfig = defaultCountdownConfig;

            if (network == "facebook" || network == "meta" || network == "fan")
            {
                layoutName = NativeName.Native_HalfScreen_No_Media;
                countdownConfig = metaCountdownConfig;
            }

            var builder = AdsManager.Instance.ShowNativeInter(placementOrder, position, layoutName, onShow, onClose, OnAdDismissedFullScreenContent);

            if (countdownConfig != null)
            {
                builder.WithCountdown(countdownConfig.InitialDelaySeconds, countdownConfig.CountdownDurationSeconds, countdownConfig.CloseButtonDelaySeconds);
            }

            builder.Execute();
        }

        public static void ShowNativeMrec(PlacementOrder placementOrder, string position, Action onShow, Action onClose, Action OnAdDismissedFullScreenContent, AdsPos adpos, Vector2Int offset)
        {
            var network = AdsManager.Instance.GetNetworkName(AdsType.NativeMrec, placementOrder);
            string layoutName = NativeName.Native_Mrec_Media;

            if (network == "facebook" || network == "meta" || network == "fan")
            {
                layoutName = NativeName.Native_Mrec_No_Media;
            }

            AdsManager.Instance.ShowNativeMrec(placementOrder, position, layoutName, onShow, onClose, OnAdDismissedFullScreenContent)
            .WithPosition(adpos, offset)
            .WithShowOnLoaded(false)
            .Execute();
        }

        public static void ShowNativeMrecOpen(PlacementOrder placementOrder, string position, Action onShow, Action onClose, Action OnAdDismissedFullScreenContent, AdsPos adpos, Vector2Int offset)
        {
            var network = AdsManager.Instance.GetNetworkName(AdsType.NativeMrecOpen, placementOrder);
            string layoutName = NativeName.Native_Mrec_Media;

            if (network == "facebook" || network == "meta" || network == "fan")
            {
                layoutName = NativeName.Native_Mrec_No_Media;
            }

            AdsManager.Instance.ShowNativeMrecOpen(placementOrder, position, layoutName, onShow, onClose, OnAdDismissedFullScreenContent)
            .WithPosition(adpos, offset)
            .WithShowOnLoaded(false)
            .Execute();
        }

        public static void ShowNativeBanner(PlacementOrder placementOrder, string position, Action onShow, Action onClose, Action OnAdDismissedFullScreenContent, Action OnClick)
        {
            string layoutName = NativeName.Native_Banner;

            AdsManager.Instance.ShowNativeBanner(placementOrder, position, layoutName, onShow, onClose, OnAdDismissedFullScreenContent, OnClick)
            .WithAutoReload(AdsManager.Instance.adsConfigs.nativeBannerTimeReload)
            .WithShowOnLoaded(true)
            .Execute();
        }

        public static void ShowNativeVideo(PlacementOrder placementOrder, string position, Action onShow, Action onClose, Action OnAdDismissedFullScreenContent, NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig)
        {
            AdsManager.Instance.ShowNativeVideo(placementOrder, position, NativeName.Native_Video, onShow, onClose, OnAdDismissedFullScreenContent)
            .WithCountdown(defaultCountdownConfig.InitialDelaySeconds, defaultCountdownConfig.CountdownDurationSeconds, defaultCountdownConfig.CloseButtonDelaySeconds)
            .Execute();
        }

    }

}
