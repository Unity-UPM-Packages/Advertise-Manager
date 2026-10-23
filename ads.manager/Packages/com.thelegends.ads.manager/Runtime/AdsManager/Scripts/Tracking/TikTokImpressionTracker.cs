#if USE_TIKTOK
using System.Collections.Generic;
using TheLegends.Base.TikTok;

namespace TheLegends.Base.Ads.Tracking
{
    public class TikTokImpressionTracker : IImpressionTracker
    {
        private List<AdsType> _trackedTypes;

        public void Initialize(AdsSettings settings)
        {
            _trackedTypes = settings.tiktokTrackedTypes ?? new List<AdsType>();
        }

        public bool CanTrack(AdsType adsType)
        {
            return _trackedTypes.Contains(adsType);
        }

        public void Track(ImpressionData data)
        {
            TikTokManager.Instance.LogAdRevenue(new Dictionary<string, object>
            {
                { "ad_mediation", data.AdMediation.ToString() },
                { "ad_network", data.AdNetwork },
                { "ad_format", data.AdFormat },
                { "ad_unit_name", data.AdUnitName },
                { "country", data.Country },
                { "revenue", data.Revenue },
                { "currency", data.Currency },
                { "placement", data.Placement }
            });
        }
    }
}
#endif
