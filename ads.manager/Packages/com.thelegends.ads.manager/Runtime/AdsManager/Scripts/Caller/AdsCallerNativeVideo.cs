using System;

namespace TheLegends.Base.Ads
{
    public static partial class AdsCaller
    {
        #region NativeVideo

        public static void ShowNativeVideo(PlacementOrder placementOrder, string position, Action onShow, Action onClose, Action OnAdDismissedFullScreenContent, NativePlatformShowBuilder.CountdownConfig defaultCountdownConfig)
        {
            AdsManager.Instance.ShowNativeVideo(placementOrder, position, NativeName.Native_Video, onShow, onClose, OnAdDismissedFullScreenContent)
            .WithCountdown(defaultCountdownConfig.InitialDelaySeconds, defaultCountdownConfig.CountdownDurationSeconds, defaultCountdownConfig.CloseButtonDelaySeconds)
            .Execute();
        }

        #endregion
    }
}
