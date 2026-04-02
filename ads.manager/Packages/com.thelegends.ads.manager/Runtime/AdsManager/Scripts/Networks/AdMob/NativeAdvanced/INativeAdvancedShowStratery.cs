using UnityEngine;

namespace TheLegends.Base.Ads
{
    public interface INativeAdvancedShowStrategy
    {
        NativeLayer ZLayer { get; }
        void ExecuteShow(AdmobNativeAdvancedController advancedController);
        void OnAdLoaded(AdmobNativeAdvancedController advancedController);
    }
}
