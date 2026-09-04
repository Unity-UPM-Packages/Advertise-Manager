using System;
using System.Collections;
using TheLegends.Base.UI;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public delegate NativePlatformShowBuilder ShowNativeAdDelegate(
        PlacementOrder order,
        string position,
        string layoutName,
        Action onShow,
        Action onClose,
        Action onDismiss,
        Action onClick);

    public delegate void HideNativeAdDelegate(PlacementOrder order);
    public delegate void LoadNativeAdDelegate(PlacementOrder order);

    public struct NativeLayoutPair
    {
        public string Media1;
        public string NoMedia1;
        public string Media2;
        public string NoMedia2;

        public string GetRandomLayout(bool isMeta)
        {
            bool isLayout1 = UnityEngine.Random.Range(0, 2) == 0;
            if (isLayout1 || string.IsNullOrEmpty(Media2))
            {
                return isMeta ? NoMedia1 : Media1;
            }
            return isMeta ? (NoMedia2 ?? NoMedia1) : (Media2 ?? Media1);
        }

        public string GetLayout(bool isMeta, bool isFirstStep)
        {
            if (isFirstStep || string.IsNullOrEmpty(Media2))
            {
                return isMeta ? NoMedia1 : Media1;
            }
            return isMeta ? (NoMedia2 ?? NoMedia1) : (Media2 ?? Media1);
        }
    }

    public struct NativeAdFormatConfig
    {
        public AdsType AdsType;
        public NativeLayoutPair LayoutPair;
        public bool UseLoadingAnimation;
        public bool ShowToastOnUnavailable;
        public Func<bool> ShouldPreloadOnUnavailable;
        public ShowNativeAdDelegate ShowAction;
        public HideNativeAdDelegate HideAction;
        public LoadNativeAdDelegate LoadAction;
    }

    public static partial class AdsCaller
    {
        #region Common Load Logic

        private static void LoadDualPlacement(
            AdsType adsType,
            LoadNativeAdDelegate loadAction,
            PlacementOrder currentPlacement,
            PlacementOrder nextPlacement)
        {
            if (AdsManager.Instance.GetAdsStatus(adsType, currentPlacement) == AdsEvents.LoadAvailable ||
                AdsManager.Instance.GetAdsStatus(adsType, nextPlacement) == AdsEvents.LoadAvailable)
            {
                return;
            }

            AdsManager.Instance.StartCoroutine(IELoadDualPlacement(adsType, loadAction, currentPlacement, nextPlacement));
        }

        private static IEnumerator IELoadDualPlacement(
            AdsType adsType,
            LoadNativeAdDelegate loadAction,
            PlacementOrder currentPlacement,
            PlacementOrder nextPlacement)
        {
            loadAction(currentPlacement);
            yield return AdsManager.Instance.WaitAdLoaded(adsType, currentPlacement);

            if (AdsManager.Instance.GetAdsStatus(adsType, currentPlacement) == AdsEvents.LoadNotAvailable)
            {
                if (AdsManager.Instance.GetAdsStatus(adsType, nextPlacement) != AdsEvents.LoadAvailable)
                {
                    loadAction(nextPlacement);
                }
            }
        }

        #endregion

        #region Common Show Execution Engines

        private static bool PreparePlacements(
            NativeAdFormatConfig config,
            ref PlacementOrder currentPlacement,
            ref PlacementOrder nextPlacement,
            Action onClose)
        {
            if (AdsManager.Instance.GetAdsStatus(config.AdsType, nextPlacement) != AdsEvents.LoadAvailable &&
                AdsManager.Instance.GetAdsStatus(config.AdsType, currentPlacement) != AdsEvents.LoadAvailable)
            {
                if (config.ShowToastOnUnavailable)
                {
                    UIToatsController.Show("Ads not available", 0.5f, ToastPosition.BottomCenter);
                }

                if (config.ShouldPreloadOnUnavailable != null && config.ShouldPreloadOnUnavailable())
                {
                    config.LoadAction?.Invoke(currentPlacement);
                }

                onClose?.Invoke();
                return false;
            }

            if (AdsManager.Instance.GetAdsStatus(config.AdsType, nextPlacement) == AdsEvents.LoadAvailable &&
                AdsManager.Instance.GetAdsStatus(config.AdsType, currentPlacement) != AdsEvents.LoadAvailable)
            {
                var temp = currentPlacement;
                currentPlacement = nextPlacement;
                nextPlacement = temp;
            }

            return true;
        }

        private static void FinishAd(bool useLoadingAnimation, Action onClose)
        {
            if (useLoadingAnimation)
            {
                UILoadingController.Show(1f, () =>
                {
                    onClose?.Invoke();
                });
            }
            else
            {
                onClose?.Invoke();
            }
        }

        private static void ShowLoop2Core(
            NativeAdFormatConfig config,
            PlacementOrder currentPlacement,
            PlacementOrder nextPlacement,
            string position,
            Action onShow,
            Action onClose,
            NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
            NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            if (!PreparePlacements(config, ref currentPlacement, ref nextPlacement, onClose))
            {
                return;
            }

            ShowAd(currentPlacement, nextPlacement, onShow);

            void ShowAd(PlacementOrder current, PlacementOrder? next, Action currentOnShow)
            {
                var network = AdsManager.Instance.GetNetworkName(config.AdsType, current);
                bool isMeta = (network == "facebook" || network == "meta" || network == "fan");
                string layoutName = config.LayoutPair.GetLayout(isMeta, isFirstStep: next.HasValue);
                var countdownConfig = isMeta ? metaCountdownConfig : defaultCountdownConfig;

                void OnAdClose()
                {
                    config.HideAction(current);

                    if (next.HasValue && AdsManager.Instance.GetAdsStatus(config.AdsType, next.Value) == AdsEvents.LoadAvailable)
                    {
                        ShowAd(next.Value, null, null);
                    }
                    else
                    {
                        FinishAd(config.UseLoadingAnimation, onClose);
                    }
                }

                void OnAdDismiss()
                {
                    OnAdClose();
                }

                config.ShowAction(
                    current,
                    position,
                    layoutName,
                    () =>
                    {
                        if (next.HasValue) config.LoadAction?.Invoke(next.Value);
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

        private static void ShowLoopMaxCore(
            NativeAdFormatConfig config,
            PlacementOrder currentPlacement,
            PlacementOrder nextPlacement,
            string position,
            int maxLoops,
            Action onShow,
            Action onClose,
            NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
            NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            int remainingLoops = maxLoops;

            if (!PreparePlacements(config, ref currentPlacement, ref nextPlacement, onClose))
            {
                return;
            }

            ShowAd(currentPlacement, nextPlacement, onShow);

            void ShowAd(PlacementOrder current, PlacementOrder next, Action currentOnShow)
            {
                var network = AdsManager.Instance.GetNetworkName(config.AdsType, current);
                bool isMeta = (network == "facebook" || network == "meta" || network == "fan");
                string layoutName = config.LayoutPair.GetRandomLayout(isMeta);
                var countdownConfig = isMeta ? metaCountdownConfig : defaultCountdownConfig;

                void OnAdClose()
                {
                    config.HideAction(current);
                    FinishAd(config.UseLoadingAnimation, onClose);
                }

                void OnAdClick()
                {
                    PimDeWitte.UnityMainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        if (remainingLoops > 0 && AdsManager.Instance.GetAdsStatus(config.AdsType, next) == AdsEvents.LoadAvailable)
                        {
                            remainingLoops--;
                            config.HideAction(current);
                            ShowAd(next, current, null);
                        }
                    });
                }

                config.ShowAction(
                    current,
                    position,
                    layoutName,
                    () =>
                    {
                        if (remainingLoops > 0)
                        {
                            config.LoadAction?.Invoke(next);
                        }
                        currentOnShow?.Invoke();
                    },
                    OnAdClose,
                    null,
                    OnAdClick
                )
                .WithCountdown(countdownConfig.InitialDelaySeconds, countdownConfig.CountdownDurationSeconds, countdownConfig.CloseButtonDelaySeconds)
                .Execute();
            }
        }

        private static void ShowNoLoopCore(
            NativeAdFormatConfig config,
            PlacementOrder placementOrder,
            string position,
            Action onShow,
            Action onClose,
            Action onAdDismissedFullScreenContent,
            NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig,
            NativePlatformShowBuilder.CountdownConfig metaCountdownConfig)
        {
            if (AdsManager.Instance.GetAdsStatus(config.AdsType, placementOrder) == AdsEvents.LoadAvailable)
            {
                var network = AdsManager.Instance.GetNetworkName(config.AdsType, placementOrder);
                bool isMeta = (network == "facebook" || network == "meta" || network == "fan");
                string layoutName = config.LayoutPair.GetLayout(isMeta, isFirstStep: true);
                var countdownConfig = isMeta ? metaCountdownConfig : defaultCountdownConfig;

                config.ShowAction(
                    placementOrder,
                    position,
                    layoutName,
                    onShow,
                    () => FinishAd(config.UseLoadingAnimation, onClose),
                    onAdDismissedFullScreenContent,
                    null
                )
                .WithCountdown(countdownConfig.InitialDelaySeconds, countdownConfig.CountdownDurationSeconds, countdownConfig.CloseButtonDelaySeconds)
                .Execute();
            }
            else
            {
                if (config.ShowToastOnUnavailable)
                {
                    UIToatsController.Show("Ads not available", 0.5f, ToastPosition.BottomCenter);
                }

                if (config.ShouldPreloadOnUnavailable != null && config.ShouldPreloadOnUnavailable())
                {
                    config.LoadAction?.Invoke(placementOrder);
                }

                onClose?.Invoke();
            }
        }

        #endregion
    }
}
