using TheLegends.Base.Ads;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class NativeBannerShowStrategy : MonoBehaviour, INativeAdvancedShowStrategy
    {
        [SerializeField] private NativeLayer zLayer = NativeLayer.Banner;
        [SerializeField] private bool _isShowOnLoaded = false;
        [SerializeField] private float _autoReloadTime = 0f;

        public NativeLayer ZLayer => zLayer;

        private INativeAdvancedHelper _helper;

        private void Awake()
        {
            _helper = GetComponent<INativeAdvancedHelper>();
        }

        public void ExecuteShow(AdmobNativeAdvancedController advancedController)
        {
            _helper.Help();

            string zLayerName = zLayer.ToString();

            string jsonBlueprint = NativeAdAssetManager.GetLayoutJson(advancedController.LayoutId);
            if (!string.IsNullOrEmpty(jsonBlueprint))
            {
                advancedController.NativeAdvancedAd.WithLayoutJson(jsonBlueprint);
                advancedController.NativeAdvancedAd.WithZLayer(zLayerName);
                advancedController.NativeAdvancedAd.Show(advancedController.LayoutId);

                if (_autoReloadTime > 0)
                {
                    advancedController.DelayReloadAd(_autoReloadTime);
                }
            }

        }

        public void OnAdLoaded(AdmobNativeAdvancedController advancedController)
        {
            if (_isShowOnLoaded)
            {
                advancedController.ShowAds();
            }
        }

        public void SetTimeReload(float time)
        {
            _autoReloadTime = time;
        }
    }
}
