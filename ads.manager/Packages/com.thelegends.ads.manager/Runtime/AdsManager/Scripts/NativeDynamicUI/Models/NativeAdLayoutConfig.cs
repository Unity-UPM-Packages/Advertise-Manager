using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheLegends.Base.Ads.NativeDynamicUI
{
    [Serializable]
    public class NativeAdLayoutConfig
    {
        public string layoutId;
        public float referenceWidth;
        public float referenceHeight;
        public List<NativeAdElementConfig> elements = new List<NativeAdElementConfig>();
    }

    [Serializable]
    public class NativeAdElementConfig
    {
        public string elementType; // String representation of NativeAdElement
        
        public RectTransformConfig rectTransform;
        public ImageConfig image;
        public TextConfig text;
    }

    [Serializable]
    public class RectTransformConfig
    {
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 offsetMin;
        public Vector2 offsetMax;
        public Vector2 pivot;
        public float rotationZ;
        public float scaleX;
        public float scaleY;
    }

    [Serializable]
    public class ImageConfig
    {
        public bool hasData;
        public string color; // Hex format #RRGGBBAA
        public float cornerRadius;
        public string imagePath; // Absolute path to physical file
        public bool isRadialFill;
    }

    [Serializable]
    public class TextConfig
    {
        public bool hasData;
        public string textContent;
        public string color;
        public float fontSize;
        public string alignment; // e.g., "MiddleCenter"
        public bool isBold;
        public bool isItalic;
        public float lineSpacing;
    }
}
