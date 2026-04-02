using UnityEngine;

namespace TheLegends.Base.Ads
{
    public class NativeFullScreenShowStratery : MonoBehaviour, INativeAdvancedShowStrategy
    {

        [SerializeField] private NativeLayer zLayer = NativeLayer.FullScreen;
        public NativeLayer ZLayer => zLayer;
        private INativeAdvancedHelper _helper;

        [SerializeField] private float initialDelaySeconds = 5f;
        [SerializeField] private float countdownDurationSeconds = 5f;
        [SerializeField] private float closeButtonDelaySeconds = 2f;

        public void OnAdLoaded(AdmobNativeAdvancedController advancedController)
        {

        }

        private void Awake()
        {
            _helper = GetComponent<INativeAdvancedHelper>();
        }

        public void ExecuteShow(AdmobNativeAdvancedController advancedController)
        {
            _helper.Help();

            string zLayerName = zLayer.ToString();

            string jsonBlueprint = DynamicAdsCacheManager.GetLayoutJson(advancedController.LayoutId);
            if (!string.IsNullOrEmpty(jsonBlueprint))
            {
                advancedController.NativePlatformAd.WithLayoutJson(jsonBlueprint);
                advancedController.NativePlatformAd.WithZLayer(zLayerName);
                advancedController.NativePlatformAd.WithCountdown(countdownDurationSeconds, closeButtonDelaySeconds, initialDelaySeconds);
                advancedController.NativePlatformAd.Show(advancedController.LayoutId);
            }

        }

        public void SetTimeCountdown(float countdownDurationSeconds, float closeButtonDelaySeconds, float initialDelaySeconds)
        {
            this.countdownDurationSeconds = countdownDurationSeconds;
            this.closeButtonDelaySeconds = closeButtonDelaySeconds;
            this.initialDelaySeconds = initialDelaySeconds;
        }

    }
}
