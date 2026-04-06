using UnityEngine;

namespace TheLegends.Base.Ads
{
    /// <summary>
    /// Marks a UI element to be included in the exported Native Ad Layout.
    /// This component identifies the role of the GameObject (e.g., Headline, Icon, CTA) for the native renderer.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class NativeAdLayoutMark : MonoBehaviour
    {
        [Header("Ad Role Specification")]
        [Tooltip("The specific AdMob element role this GameObject represents.")]
        public NativeAdElement elementTag;
    }
}
