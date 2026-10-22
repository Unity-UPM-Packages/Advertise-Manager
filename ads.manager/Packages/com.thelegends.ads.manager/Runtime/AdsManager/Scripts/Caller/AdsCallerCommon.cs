using System;

namespace TheLegends.Base.Ads
{
    public static partial class AdsCaller
    {
        #region Common

        // Only check at the entry point, the next ads inside Loop2/LoopMax are shown without checking
        private static bool IsTimeToShowAds(Action onClose)
        {
            if (AdsManager.Instance.IsTimeToShowAd)
            {
                return true;
            }

            onClose?.Invoke();
            return false;
        }

        #endregion
    }
}
