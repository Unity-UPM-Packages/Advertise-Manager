using UnityEngine;

namespace TheLegends.Base.Ads
{
    public interface INativeAdvancedShowStrategy
    {
        NativeLayer ZLayer { get; }
        void ExecuteShow(AdmobNativeAdvancedController advancedController);
        void OnAdsLoaded(AdmobNativeAdvancedController advancedController);
        void OnAdsLoadFailed(AdmobNativeAdvancedController advancedController);
        void OnAdsClosed(AdmobNativeAdvancedController advancedController);
    }
}
