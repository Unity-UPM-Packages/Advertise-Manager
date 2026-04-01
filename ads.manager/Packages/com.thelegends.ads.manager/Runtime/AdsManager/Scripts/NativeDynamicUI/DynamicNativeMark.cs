using UnityEngine;

namespace TheLegends.Base.Ads.NativeDynamicUI
{
    [RequireComponent(typeof(RectTransform))]
    public class DynamicNativeMark : MonoBehaviour
    {
        [Header("Role Specification")]
        [Tooltip("What part of the Google AdMob structure does this UI act as?")]
        public NativeAdElement elementTag;

    }
}
