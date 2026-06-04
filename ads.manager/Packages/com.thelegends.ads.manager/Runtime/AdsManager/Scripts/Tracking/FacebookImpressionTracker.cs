#if USE_FACEBOOK
using System.Collections.Generic;
using TheLegends.Base.Facebook;
using Facebook.Unity;

namespace TheLegends.Base.Ads.Tracking
{
    public class FacebookImpressionTracker : IImpressionTracker
    {
        private List<AdsType> _trackedTypes;
        private double _threshold;

        public void Initialize(AdsSettings settings)
        {
            _trackedTypes = settings.facebookTrackedTypes ?? new List<AdsType>();
            _threshold = settings.FacebookTrackingThreshold;
        }

        public bool CanTrack(AdsType adsType)
        {
            return _trackedTypes.Contains(adsType);
        }

        public void Track(ImpressionData data)
        {
            if (data.Revenue < _threshold)
            {
                return;
            }

            FacebookManager.Instance.LogEvent("AdImpression", (float)data.Revenue, new Dictionary<string, object>()
            {
                { "ad_mediation", data.AdMediation.ToString() },
                { "ad_network", data.AdNetwork },
                { "ad_format", data.AdFormat },
                { "ad_unit_name", data.AdUnitName },
                { "country", data.Country },
                { "revenue", data.Revenue },
                { AppEventParameterName.Currency, data.Currency },
                { "placement", data.Placement }
            });
        }
    }
}
#endif
