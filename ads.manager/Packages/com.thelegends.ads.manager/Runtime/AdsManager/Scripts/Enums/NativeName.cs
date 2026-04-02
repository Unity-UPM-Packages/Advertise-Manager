using UnityEngine;

namespace TheLegends.Base.Ads
{
    public static class NativeName
    {
        public const string Native_Banner = "native_banner";
        public const string Native_FullScreen = "native_fullscreen";
        public const string Native_Mrec = "native_mrec";
        public const string Native_Video = "native_video";

        public static string GetZLayerAssigned(string layoutName)
        {
            switch (layoutName)
            {
                case Native_Banner:
                    return "BannerLayer";
                case Native_Mrec:
                    return "MRECLayer";
                case Native_FullScreen:
                case Native_Video:
                    return "FullscreenLayer";
                default:
                    return "BannerLayer";
            }
        }
    }

    public enum NativeLayer
    {
        None = 0,
        Banner = 1,
        FullScreen = 2,
    }
}
