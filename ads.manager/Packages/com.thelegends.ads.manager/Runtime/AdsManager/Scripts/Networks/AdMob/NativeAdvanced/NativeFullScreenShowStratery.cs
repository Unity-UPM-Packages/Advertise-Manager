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

        public void OnAdLoadFailed(AdmobNativeAdvancedController advancedController)
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

            string jsonBlueprint = NativeAdAssetManager.GetLayoutJson(advancedController.LayoutId);
            if (!string.IsNullOrEmpty(jsonBlueprint))
            {
                advancedController.NativeAdvancedAd.WithLayoutJson(jsonBlueprint);
                advancedController.NativeAdvancedAd.WithZLayer(zLayerName);
                advancedController.NativeAdvancedAd.WithCountdown(countdownDurationSeconds, closeButtonDelaySeconds, initialDelaySeconds);
                advancedController.NativeAdvancedAd.Show(advancedController.LayoutId);
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
