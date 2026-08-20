using System;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public static partial class AdsCaller
    {
        #region NativeMrec

        public static void ShowNativeMrec(PlacementOrder placementOrder, string position, Action onShow, Action onClose, Action OnAdDismissedFullScreenContent, AdsPos adpos, Vector2Int offset)
        {
            var network = AdsManager.Instance.GetNetworkName(AdsType.NativeMrec, placementOrder);
            string layoutName = NativeName.Native_Mrec_Media;

            if (network == "facebook" || network == "meta" || network == "fan")
            {
                layoutName = NativeName.Native_Mrec_No_Media;
            }

            AdsManager.Instance.ShowNativeMrec(placementOrder, position, layoutName, onShow, onClose, OnAdDismissedFullScreenContent)
            .WithPosition(adpos, offset)
            .WithShowOnLoaded(false)
            .Execute();
        }

        public static void ShowNativeMrecOpen(PlacementOrder placementOrder, string position, Action onShow, Action onClose, Action OnAdDismissedFullScreenContent, AdsPos adpos, Vector2Int offset)
        {
            var network = AdsManager.Instance.GetNetworkName(AdsType.NativeMrecOpen, placementOrder);
            string layoutName = NativeName.Native_Mrec_Media;

            if (network == "facebook" || network == "meta" || network == "fan")
            {
                layoutName = NativeName.Native_Mrec_No_Media;
            }

            AdsManager.Instance.ShowNativeMrecOpen(placementOrder, position, layoutName, onShow, onClose, OnAdDismissedFullScreenContent)
            .WithPosition(adpos, offset)
            .WithShowOnLoaded(false)
            .Execute();
        }

        #endregion
    }
}
