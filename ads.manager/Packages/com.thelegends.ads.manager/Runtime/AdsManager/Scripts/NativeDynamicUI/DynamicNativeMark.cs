using UnityEngine;

namespace TheLegends.Base.Ads.NativeDynamicUI
{
    [RequireComponent(typeof(RectTransform))]
    public class DynamicNativeMark : MonoBehaviour
    {
        [Header("Role Specification")]
        [Tooltip("What part of the Google AdMob structure does this UI act as?")]
        public NativeAdElement elementTag;
        
        [Header("Native Graphic Decorators")]
        [Tooltip("Force a radius on Native Android/iOS sides since Unity Native Image doesn't inherently serialize corner metadata.")]
        public float customCornerRadius = 0f;
        
        [Tooltip("Check this if this is a countdown timer circle. The Android native side will render it using SweepAngle algorithms instead of a static image.")]
        public bool isRadialFill = false;
    }
}
