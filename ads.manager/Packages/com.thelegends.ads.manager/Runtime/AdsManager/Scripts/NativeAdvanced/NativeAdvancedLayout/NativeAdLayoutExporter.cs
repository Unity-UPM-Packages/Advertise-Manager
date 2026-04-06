using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace TheLegends.Base.Ads
{
    /// <summary>
    /// Utility class responsible for extracting Unity UI hierarchy and mapping it into a serializable Layout Config.
    /// Manages the translation from Unity's World Space into a normalized (0-1) coordinate system for Native Android/iOS.
    /// </summary>
    public static class NativeAdLayoutExporter
    {
        /// <summary>
        /// Generates a serializable NativeAdLayoutConfig by scanning the provided RectTransform and its children.
        /// Converts Unity's world space coordinates of elements marked with NativeAdLayoutMark into 
        /// normalized (0..1) relative coordinates for the native layout engine.
        /// </summary>
        /// <param name="layoutId">Unique ID assigned to this layout.</param>
        /// <param name="rootCanvasRect">The root UI container containing the Native Ad elements.</param>
        /// <returns>A fully configured NativeAdLayoutConfig object.</returns>
        public static NativeAdLayoutConfig GenerateConfig(string layoutId, RectTransform rootCanvasRect)
        {
            // COORDINATE NORMALIZATION:
            // Fetch the root Canvas to use as the absolute coordinate reference.
            Canvas rootCanvas = rootCanvasRect.GetComponentInParent<Canvas>();
            if (rootCanvas != null) rootCanvas = rootCanvas.rootCanvas;
            RectTransform canvasRect = rootCanvas != null ? rootCanvas.GetComponent<RectTransform>() : rootCanvasRect;

            float screenW = canvasRect.rect.width;
            float screenH = canvasRect.rect.height;

            var config = new NativeAdLayoutConfig
            {
                layoutId = layoutId,
                referenceWidth = screenW,
                referenceHeight = screenH
            };

            // Collect all marked parts of the Native Ad
            var marks = rootCanvasRect.GetComponentsInChildren<NativeAdLayoutMark>(true);

            foreach (var mark in marks)
            {
                var elementConfig = new NativeAdElementConfig
                {
                    elementType = mark.elementTag.ToString()
                };

                /* ----------------------------------------------------
                 * COORDINATE CALCULATION
                 * Calculates absolute screen corners for the element and transforms them
                 * into local normalized space relative to the RootAdView.
                 * ---------------------------------------------------- */
                RectTransform rt = mark.GetComponent<RectTransform>();
                Vector3[] corners = new Vector3[4];
                rt.GetWorldCorners(corners);

                // Convert to Root bounds
                Vector3 bl = canvasRect.InverseTransformPoint(corners[0]);
                Vector3 tr = canvasRect.InverseTransformPoint(corners[2]);

                float xMin = (bl.x + screenW * canvasRect.pivot.x) / screenW;
                float yMin = (bl.y + screenH * canvasRect.pivot.y) / screenH;
                float xMax = (tr.x + screenW * canvasRect.pivot.x) / screenW;
                float yMax = (tr.y + screenH * canvasRect.pivot.y) / screenH;

                elementConfig.rectTransform = new RectTransformConfig
                {
                    anchorMin = new SerializableVector2(xMin, yMin),
                    anchorMax = new SerializableVector2(xMax, yMax),
                    offsetMin = new SerializableVector2(0, 0),
                    offsetMax = new SerializableVector2(0, 0),
                    pivot = new SerializableVector2(rt.pivot.x, rt.pivot.y),
                    rotationZ = rt.localEulerAngles.z,
                    scaleX = rt.localScale.x,
                    scaleY = rt.localScale.y
                };

                // Extract Visual Styles: background colors, textures, and typography
                ExtractGraphicComponents(mark, elementConfig);

                config.elements.Add(elementConfig);
            }

            return config;
        }

        private static void ExtractGraphicComponents(NativeAdLayoutMark mark, NativeAdElementConfig elementConfig)
        {
            var img = mark.GetComponent<Image>();
            // Exclude IconViews and MediaViews from image extraction as they are handled natively by the AdMob platform.
            if (img != null && img.enabled && mark.elementTag != NativeAdElement.IconView && mark.elementTag != NativeAdElement.MediaView)
            {
                var imageConfig = new ImageConfig
                {
                    color = "#" + ColorUtility.ToHtmlStringRGBA(img.color),
                    imagePath = ProcessAndCacheImage(img.sprite)
                };

                // NINE-PATCH (9-SLICE) EXTRACTION:
                // Only extract border metadata if Image Type is set to Sliced in Unity.
                if (img.type == Image.Type.Sliced && img.sprite != null && img.sprite.border != Vector4.zero)
                {
                    Vector4 b = img.sprite.border;
                    float ppum = img.pixelsPerUnitMultiplier;
                    if (ppum <= 0) ppum = 1;

                    // Export the RAW corner pixels and the multiplier
                    imageConfig.border = new ImageBorderConfig(b.x, b.y, b.z, b.w, ppum);
                }

                elementConfig.image = imageConfig;
            }

            var txt = mark.GetComponent<Text>();
            if (txt == null)
            {
                foreach (Transform child in mark.transform)
                {
                    txt = child.GetComponent<Text>();
                    if (txt != null && txt.enabled) break;
                }
            }

            if (txt != null && txt.enabled)
            {
                // Calculate local bounds of the Text relative to the parent mark
                RectTransform markRt = mark.GetComponent<RectTransform>();
                RectTransform txtRt = txt.GetComponent<RectTransform>();
                
                Vector3[] corners = new Vector3[4];
                txtRt.GetWorldCorners(corners);
                
                Vector3 bl = markRt.InverseTransformPoint(corners[0]);
                Vector3 tr = markRt.InverseTransformPoint(corners[2]);
                
                float markW = markRt.rect.width;
                float markH = markRt.rect.height;
                
                // Normalized to 0..1 inside the parent (Bottom-Left = 0,0, Top-Right = 1,1)
                float tXMin = markW == 0 ? 0 : (bl.x + markW * markRt.pivot.x) / markW;
                float tYMin = markH == 0 ? 0 : (bl.y + markH * markRt.pivot.y) / markH;
                float tXMax = markW == 0 ? 1 : (tr.x + markW * markRt.pivot.x) / markW;
                float tYMax = markH == 0 ? 1 : (tr.y + markH * markRt.pivot.y) / markH;

                elementConfig.text = new TextConfig
                {
                    textContent = txt.text,
                    color = "#" + ColorUtility.ToHtmlStringRGBA(txt.color),
                    fontSize = txt.fontSize,
                    alignment = txt.alignment.ToString(),
                    isBold = txt.fontStyle == FontStyle.Bold || txt.fontStyle == FontStyle.BoldAndItalic,
                    isItalic = txt.fontStyle == FontStyle.Italic || txt.fontStyle == FontStyle.BoldAndItalic,
                    lineSpacing = txt.lineSpacing,
                    rectTransform = new RectTransformConfig
                    {
                        anchorMin = new SerializableVector2(tXMin, tYMin),
                        anchorMax = new SerializableVector2(tXMax, tYMax),
                        offsetMin = new SerializableVector2(0, 0),
                        offsetMax = new SerializableVector2(0, 0),
                        pivot = new SerializableVector2(txtRt.pivot.x, txtRt.pivot.y),
                        rotationZ = txtRt.localEulerAngles.z,
                        scaleX = txtRt.localScale.x,
                        scaleY = txtRt.localScale.y
                    }
                };
            }
        }

        /// <summary>
        /// Duplicates a sprite's texture into a readable ARGB32 format, handling atlased and non-readable sprites.
        /// </summary>
        private static string ProcessAndCacheImage(Sprite sprite)
        {
            if (sprite == null) return null;
            if (sprite.name == "Background" || sprite.name == "UISprite") return null;

            string cacheDir = Path.Combine(Application.persistentDataPath, "NativeAdLayoutCache");
            if (!Directory.Exists(cacheDir)) Directory.CreateDirectory(cacheDir);

            // Unique name based on Sprite so atlased elements don't conflict 
            string filePath = Path.Combine(cacheDir, sprite.name + ".png");

            // Cache validation: Avoid expensive texture duplication/encoding if already exists
            if (!File.Exists(filePath))
            {
                try
                {
                    Texture2D tex = DuplicateReadableTexture(sprite);
                    byte[] bytes = tex.EncodeToPNG();
                    File.WriteAllBytes(filePath, bytes);

                    if (Application.isPlaying) GameObject.Destroy(tex);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[NativeAdLayout] Texture Encode Failed for " + sprite.name + ". " + e.Message);
                    return null;
                }
            }
            return filePath;
        }

        /// <summary>
        /// Uses Graphics.Blit to copy texture data from potentially non-readable or GPU-stored sprites into a CPU-readable Texture2D.
        /// Handles cropping for sprites that are part of a larger atlas.
        /// </summary>
        private static Texture2D DuplicateReadableTexture(Sprite sprite)
        {
            Texture2D source = sprite.texture;
            Rect rect = sprite.rect;

            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;

            Texture2D readableText = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
            readableText.ReadPixels(new Rect(rect.x, rect.y, rect.width, rect.height), 0, 0);
            readableText.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }
    }
}
