#if USE_APPSFLYER
using System;
using System.Collections.Generic;
using TheLegends.Base.AppsFlyer;
using AppsFlyerSDK;

namespace TheLegends.Base.Ads.Tracking
{
    public class AppsFlyerImpressionTracker : IImpressionTracker
    {
        private List<AdsType> _trackedTypes;

        public void Initialize(AdsSettings settings)
        {
            _trackedTypes = settings.appsFlyerTrackedTypes ?? new List<AdsType>();
        }

        public bool CanTrack(AdsType adsType)
        {
            return _trackedTypes.Contains(adsType);
        }

        public void Track(ImpressionData data)
        {
            string mediationStr = "GoogleAdMob";
            if (data.AdMediation == AdsMediation.Max) mediationStr = "ApplovinMax";
            else if (data.AdMediation == AdsMediation.Iron) mediationStr = "IronSource";

            if (Enum.TryParse<MediationNetwork>(mediationStr, out var appsflyersMediation))
            {
                AppsFlyerManager.Instance.LogRevenue(data.AdNetwork, appsflyersMediation, data.Currency, data.Revenue, new Dictionary<string, string>()
                {
                    { AdRevenueScheme.AD_UNIT, data.AdUnitName },
                    { AdRevenueScheme.AD_TYPE, data.AdFormat },
                    { AdRevenueScheme.COUNTRY, data.Country },
                    { AdRevenueScheme.PLACEMENT, data.Placement }
                });
            }
        }
    }
}
#endif
