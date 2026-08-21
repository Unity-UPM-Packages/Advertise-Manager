using System;
using System.Collections;
using TheLegends.Base.UI;
using UnityEngine;

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

            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeReward, currentPlacement) == AdsEvents.LoadNotAvailable)
            {
                if (AdsManager.Instance.GetAdsStatus(AdsType.NativeReward, nextPlacement) != AdsEvents.LoadAvailable)
                {
                    AdsManager.Instance.LoadNativeReward(nextPlacement);
                }
            }
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
                bool isMeta = (network == "facebook" || network == "meta" || network == "fan");

                string layoutName;
                if (next.HasValue)
                {
                    layoutName = isMeta ? NativeName.Native_Reward_No_Media : NativeName.Native_Reward_Media;
                }
                else
                {
                    layoutName = isMeta ? NativeName.Native_Reward_No_Media_2 : NativeName.Native_Reward_Media_2;
                }

                NativePlatformShowBuilder.CountdownConfig countdownConfig = isMeta ? metaCountdownConfig : defaultCountdownConfig;

                void OnAdClose()
                {
                    AdsManager.Instance.HideNativeReward(current);

                    if (next.HasValue && AdsManager.Instance.GetAdsStatus(AdsType.NativeReward, next.Value) == AdsEvents.LoadAvailable)
                    {
                        ShowAd(next.Value, null, null);
                    }
                    else
                    {
                        UILoadingController.Show(1f, () =>
                        {
                            onClose?.Invoke();
                        });
                    }
                }

                void OnAdDismiss()
                {
                    if (next.HasValue && AdsManager.Instance.GetAdsStatus(AdsType.NativeReward, next.Value) == AdsEvents.LoadAvailable)
                    {
                        AdsManager.Instance.HideNativeReward(current);
                        ShowAd(next.Value, null, null);
                    }
                }

                AdsManager.Instance.ShowNativeReward(
                    current,
                    position,
                    layoutName,
                    () =>
                    {
                        if (next.HasValue) AdsManager.Instance.LoadNativeReward(next.Value);
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

        public static void ShowNativeRewardNoLoop(PlacementOrder placementOrder,
            string position,
            Action onShow,
            Action onClose,
            Action onAdDismissedFullScreenContent,
            NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
            NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            if (AdsManager.Instance.GetAdsStatus(AdsType.NativeReward, placementOrder) == AdsEvents.LoadAvailable)
            {
                var network = AdsManager.Instance.GetNetworkName(AdsType.NativeReward, placementOrder);
                bool isMeta = (network == "facebook" || network == "meta" || network == "fan");
                string layoutName = isMeta ? NativeName.Native_Reward_No_Media : NativeName.Native_Reward_Media;
                NativePlatformShowBuilder.CountdownConfig countdownConfig = isMeta ? metaCountdownConfig : defaultCountdownConfig;

                AdsManager.Instance.ShowNativeReward(placementOrder, position, layoutName, onShow, () =>
                {
                    UILoadingController.Show(1f, () =>
                    {
                        onClose?.Invoke();
                    });
                }, onAdDismissedFullScreenContent, null)
                .WithCountdown(countdownConfig.InitialDelaySeconds, countdownConfig.CountdownDurationSeconds, countdownConfig.CloseButtonDelaySeconds)
                .Execute();

            }
            else
            {
                UIToatsController.Show("Ads not available", 0.5f, ToastPosition.BottomCenter);


                if (AdsManager.Instance.SettingsAds.preloadSettings.nativeAds.preloadNativeReward)
                {

                    AdsManager.Instance.LoadNativeReward(placementOrder);
                }
            }
        }

        #endregion
    }
}
