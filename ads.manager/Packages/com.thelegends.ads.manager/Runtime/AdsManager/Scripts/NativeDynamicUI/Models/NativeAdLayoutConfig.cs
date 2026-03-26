using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

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
        public string elementType; 
        public RectTransformConfig rectTransform;
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ImageConfig image;
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TextConfig text;
    }

    [Serializable]
    public class SerializableVector2
    {
        public float x;
        public float y;
        public SerializableVector2(float x, float y) { this.x = x; this.y = y; }
        public SerializableVector2() { }
    }

    [Serializable]
    public class RectTransformConfig
    {
        public SerializableVector2 anchorMin;
        public SerializableVector2 anchorMax;
        public SerializableVector2 offsetMin;
        public SerializableVector2 offsetMax;
        public SerializableVector2 pivot;
        public float rotationZ;
        public float scaleX;
        public float scaleY;
    }

    [Serializable]
    public class ImageConfig
    {
        public string color; // Hex format #RRGGBBAA
        public float cornerRadius;
        public string imagePath; // Absolute path to physical file
        public bool isRadialFill;
    }

    [Serializable]
    public class TextConfig
    {
        public string textContent;
        public string color;
        public float fontSize;
        public string alignment; // e.g., "MiddleCenter"
        public bool isBold;
        public bool isItalic;
        public float lineSpacing;
    }
}
