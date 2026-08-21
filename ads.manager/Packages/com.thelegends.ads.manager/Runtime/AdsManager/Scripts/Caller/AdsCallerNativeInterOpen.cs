using System;
using System.Collections;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public static partial class AdsCaller
    {
        #region NativeInterOpen

        public static void LoadNativeInterOpen(PlacementOrder currentPlacement, PlacementOrder nextPlacement)
        {
            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeInterOpen, currentPlacement) == AdsEvents.LoadAvailable ||
                AdsManager.Instance.GetAdsStatus(AdsType.NativeInterOpen, nextPlacement) == AdsEvents.LoadAvailable)
            {
                return;
            }

            AdsManager.Instance.StartCoroutine(IELoadNativeInterOpen(currentPlacement, nextPlacement));
        }

        private static IEnumerator IELoadNativeInterOpen(PlacementOrder currentPlacement, PlacementOrder nextPlacement)
        {
            AdsManager.Instance.LoadNativeInterOpen(currentPlacement);

            yield return AdsManager.Instance.WaitAdLoaded(AdsType.NativeInterOpen, currentPlacement);

            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeInterOpen, currentPlacement) == AdsEvents.LoadNotAvailable)
            {
                if (AdsManager.Instance.GetAdsStatus(AdsType.NativeInterOpen, nextPlacement) != AdsEvents.LoadAvailable)
                {
                    AdsManager.Instance.LoadNativeInterOpen(nextPlacement);
                }
            }
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
                bool isMeta = (network == "facebook" || network == "meta" || network == "fan");

                string layoutName;
                if (next.HasValue)
                {
                    layoutName = isMeta ? NativeName.Native_Inter_No_Media : NativeName.Native_Inter_Media;
                }
                else
                {
                    layoutName = isMeta ? NativeName.Native_Inter_No_Media_2 : NativeName.Native_Inter_Media_2;
                }

                NativePlatformShowBuilder.CountdownConfig countdownConfig = isMeta ? metaCountdownConfig : defaultCountdownConfig;

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
                    OnAdClose();
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
                bool isMeta = (network == "facebook" || network == "meta" || network == "fan");
                string layoutName = isMeta ? NativeName.Native_Inter_No_Media : NativeName.Native_Inter_Media;
                NativePlatformShowBuilder.CountdownConfig countdownConfig = isMeta ? metaCountdownConfig : defaultCountdownConfig;

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
