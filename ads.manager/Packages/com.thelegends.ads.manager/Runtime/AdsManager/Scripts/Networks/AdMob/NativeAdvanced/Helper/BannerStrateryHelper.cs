using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class BannerStrateryHelper : MonoBehaviour, INativeAdvancedHelper
    {
        private NativeBannerShowStrategy _nativeBannerShowStrategy;

        private void Awake()
        {
            _nativeBannerShowStrategy = GetComponent<NativeBannerShowStrategy>();
        }

        public void Help()
        {
            if (_nativeBannerShowStrategy != null)
            {
                _nativeBannerShowStrategy.SetTimeReload(AdsManager.Instance.adsConfigs.adTimeReload);
            }

        }
    }
}
