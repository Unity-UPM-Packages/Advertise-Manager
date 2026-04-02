using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class FullScreenStrateryHelper : MonoBehaviour, INativeAdvancedHelper
    {
        private NativeFullScreenShowStratery _nativeFullScreenShowStrategy;

        private void Awake()
        {
            _nativeFullScreenShowStrategy = GetComponent<NativeFullScreenShowStratery>();
        }

        public void Help()
        {
            if (_nativeFullScreenShowStrategy != null)
            {
                _nativeFullScreenShowStrategy.SetTimeCountdown(AdsManager.Instance.adsConfigs.nativeVideoCountdownTimerDuration, AdsManager.Instance.adsConfigs.nativeVideoDelayBeforeCountdown, AdsManager.Instance.adsConfigs.nativeVideoCloseClickableDelay);
            }

        }
    }
}
