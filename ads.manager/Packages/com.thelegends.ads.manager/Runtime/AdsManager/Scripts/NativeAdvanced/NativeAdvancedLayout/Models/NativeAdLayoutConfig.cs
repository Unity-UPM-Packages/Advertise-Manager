using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace TheLegends.Base.Ads
{
    /// <summary>
    /// Represents the full blueprint of a Native Ad Layout, including its ID, reference resolution, and child elements.
    /// </summary>
    [Serializable]
    public class NativeAdLayoutConfig
    {
        public string layoutId;
        public float referenceWidth;
        public float referenceHeight;
        public List<NativeAdElementConfig> elements = new List<NativeAdElementConfig>();
    }

    /// <summary>
    /// Configuration for an individual AdMob or decorative UI element.
    /// </summary>
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

    /// <summary>
    /// A simple serializable wrapper for Vector2 to avoid Unity-specific serialization issues in JSON.
    /// </summary>
    [Serializable]
    public class SerializableVector2
    {
        public float x;
        public float y;
        public SerializableVector2(float x, float y) { this.x = x; this.y = y; }
        public SerializableVector2() { }
    }

    /// <summary>
    /// Metadata for 9-slice image slicing, including border pixel thickness and PPU multiplier.
    /// </summary>
    [Serializable]
    public class ImageBorderConfig
    {
        public float left;
        public float bottom;
        public float right;
        public float top;
        public float ppuMultiplier;
        public ImageBorderConfig(float left, float bottom, float right, float top, float ppuMultiplier = 1.0f)
        {
            this.left = left; this.bottom = bottom; this.right = right; this.top = top; this.ppuMultiplier = ppuMultiplier;
        }
        public ImageBorderConfig() { }
    }

    /// <summary>
    /// Serializable representation of a Unity RectTransform's properties, including anchors, offsets, and pivots.
    /// </summary>
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

    /// <summary>
    /// Visual configuration for Image components, covering colors and 9-slice metadata.
    /// </summary>
    [Serializable]
    public class ImageConfig
    {
        public string color; // Hex format #RRGGBBAA
        public float cornerRadius;
        public string imagePath; // Absolute path to physical file

        /// <summary>
        /// NinePatch border in PIXELS (left, bottom, right, top).
        /// Null means the image is NOT a 9-slice and should be stretched normally.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ImageBorderConfig border;
    }

    /// <summary>
    /// Visual and typographic configuration for Text components, exported from Unity UI Text.
    /// </summary>
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

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public RectTransformConfig rectTransform;
    }
}
