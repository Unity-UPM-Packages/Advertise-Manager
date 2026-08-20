#if USE_ADMOB

namespace TheLegends.Base.Ads
{
    public class AdmobNativeMrecController : AdmobNativePlatformController
    {
        public override AdsType GetAdsType()
        {
#if USE_ADMOB
            return AdsType.NativeMrec;
#else
            return AdsType.None;
#endif
        }
    }
}

#endif
