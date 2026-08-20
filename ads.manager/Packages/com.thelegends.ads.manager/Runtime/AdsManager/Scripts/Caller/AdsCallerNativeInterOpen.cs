using System;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public static partial class AdsCaller
    {
        #region NativeInterOpen

        public static void ShowNativeInterOpen(
            PlacementOrder currentPlacement,
            PlacementOrder nextPlacement,
            string position,
            Action onShow,
            Action onClose,
            NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
            NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeInterOpen, nextPlacement) != AdsEvents.LoadAvailable &&
                AdsManager.Instance.GetAdsStatus(AdsType.NativeInterOpen, currentPlacement) != AdsEvents.LoadAvailable)
            {
                onClose?.Invoke();
                return;
            }

            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeInterOpen, nextPlacement) == AdsEvents.LoadAvailable &&
                AdsManager.Instance.GetAdsStatus(AdsType.NativeInterOpen, currentPlacement) != AdsEvents.LoadAvailable)
            {
                var temp = currentPlacement;
                currentPlacement = nextPlacement;
                nextPlacement = temp;
            }

            ShowAd(currentPlacement, nextPlacement, onShow);

            void ShowAd(PlacementOrder current, PlacementOrder? next, Action currentOnShow)
            {
                var network = AdsManager.Instance.GetNetworkName(AdsType.NativeInterOpen, current);
                string layoutName = NativeName.Native_FullScreen_Media;

                NativePlatformShowBuilder.CountdownConfig countdownConfig = defaultCountdownConfig;

                if (network == "facebook" || network == "meta" || network == "fan")
                {
                    layoutName = NativeName.Native_FullScreen_No_Media;
                    countdownConfig = metaCountdownConfig;
                }

                void OnAdClose()
                {
                    AdsManager.Instance.HideNativeInterOpen(current);

                    if (next.HasValue && AdsManager.Instance.GetAdsStatus(AdsType.NativeInterOpen, next.Value) == AdsEvents.LoadAvailable)
                    {
                        ShowAd(next.Value, null, null);
                    }
                    else
                    {
                        onClose?.Invoke();
                    }
                }

                void OnAdDismiss()
                {
                    if (next.HasValue && AdsManager.Instance.GetAdsStatus(AdsType.NativeInterOpen, next.Value) == AdsEvents.LoadAvailable)
                    {
                        AdsManager.Instance.HideNativeInterOpen(current);
                        ShowAd(next.Value, null, null);
                    }
                }

                AdsManager.Instance.ShowNativeInterOpen(
                    current,
                    position,
                    layoutName,
                    () =>
                    {
                        if (next.HasValue) AdsManager.Instance.LoadNativeInterOpen(next.Value);
                        currentOnShow?.Invoke();
                    },
                    OnAdClose,
                    OnAdDismiss,
                    null
                )
                .WithCountdown(countdownConfig.InitialDelaySeconds, countdownConfig.CountdownDurationSeconds, countdownConfig.CloseButtonDelaySeconds)
                .Execute();
            }
        }

        public static void ShowNativeInterOpenNoLoop(PlacementOrder placementOrder,
            string position,
            Action onShow,
            Action onClose,
            Action onAdDismissedFullScreenContent,
            NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
            NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeInterOpen, placementOrder) == AdsEvents.LoadAvailable)
            {
                var network = AdsManager.Instance.GetNetworkName(AdsType.NativeInterOpen, placementOrder);
                string layoutName = NativeName.Native_FullScreen_Media;
                NativePlatformShowBuilder.CountdownConfig countdownConfig = defaultCountdownConfig;

                if (network == "facebook" || network == "meta" || network == "fan")
                {
                    layoutName = NativeName.Native_FullScreen_No_Media;
                    countdownConfig = metaCountdownConfig;
                }

                AdsManager.Instance.ShowNativeInterOpen(placementOrder, position, layoutName, onShow, onClose, onAdDismissedFullScreenContent, null)
                .WithCountdown(countdownConfig.InitialDelaySeconds, countdownConfig.CountdownDurationSeconds, countdownConfig.CloseButtonDelaySeconds)
                .Execute();
            }
            else
            {
                onClose?.Invoke();
            }
        }

        #endregion
    }
}
