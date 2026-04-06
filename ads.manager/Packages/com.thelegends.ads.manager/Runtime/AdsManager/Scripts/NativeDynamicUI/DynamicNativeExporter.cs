using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace TheLegends.Base.Ads
{
    public static class DynamicNativeExporter
    {
        public static NativeAdLayoutConfig GenerateConfig(string layoutId, RectTransform rootCanvasRect)
        {
            // CHỮA LỖI MẤT TỶ LỆ (Distortion / Fullscreen Stretch):
            // Lấy Root Canvas làm hệ quy chiếu tuyệt đối thay vì chính cái Banner.
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
            var marks = rootCanvasRect.GetComponentsInChildren<DynamicNativeMark>(true);

            foreach (var mark in marks)
            {
                var elementConfig = new NativeAdElementConfig
                {
                    elementType = mark.elementTag.ToString()
                };

                /* ----------------------------------------------------
                 * SMART COORDINATE CONVERSION (The Anti-Notch Trick)
                 * We bypass LocalPivots/LayoutGroups by fetching true absolute local corners
                 * relative to the main Canvas, locking into an exact 0.0 -> 1.0 range.
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

                // Extract Visual Shapes and Fonts (Background Colors, Tints, text content)
                ExtractGraphicComponents(mark, elementConfig);

                config.elements.Add(elementConfig);
            }

            return config;
        }

        private static void ExtractGraphicComponents(DynamicNativeMark mark, NativeAdElementConfig elementConfig)
        {
            var img = mark.GetComponent<Image>();
            // Loại trừ trường hợp là IconView, không xuất ra image json
            if (img != null && img.enabled && mark.elementTag != NativeAdElement.IconView)
            {
                var imageConfig = new ImageConfig
                {
                    color = "#" + ColorUtility.ToHtmlStringRGBA(img.color),
                    imagePath = ProcessAndCacheImage(img.sprite)
                };

                // 9-SLICE SUPPORT: Chỉ trích xuất border nếu Image Type được set là Sliced
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

            // GỠ LỖI CỐT LÕI: Chỉ quét đúng bản thân và CON TRỰC TIẾP, không cào toàn bộ cây cháu chắt
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

        private static string ProcessAndCacheImage(Sprite sprite)
        {
            if (sprite == null) return null;
            if (sprite.name == "Background" || sprite.name == "UISprite") return null;

            string cacheDir = Path.Combine(Application.persistentDataPath, "DynamicAdsCache");
            if (!Directory.Exists(cacheDir)) Directory.CreateDirectory(cacheDir);

            // Utilizing Sprite Name inside the cache so atlased sprites don't overwrite each other 
            string filePath = Path.Combine(cacheDir, sprite.name + ".png");

            // Cache Invalidation Check -> we skip expensive compression if cached natively
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
                    Debug.LogError("[DynamicNativeAds] Texture Encode Failed for " + sprite.name + ". " + e.Message);
                    return null;
                }
            }
            return filePath;
        }

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
