using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TheLegends.Base.Ads.NativeDynamicUI;

namespace TheLegends.Base.Ads.NativeDynamicUI
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
                elementConfig.image = new ImageConfig
                {
                    color = "#" + ColorUtility.ToHtmlStringRGBA(img.color),
                    imagePath = ProcessAndCacheImage(img.sprite)
                };
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
                elementConfig.text = new TextConfig
                {
                    textContent = txt.text,
                    color = "#" + ColorUtility.ToHtmlStringRGBA(txt.color),
                    fontSize = txt.fontSize,
                    alignment = txt.alignment.ToString(),
                    isBold = txt.fontStyle == FontStyle.Bold || txt.fontStyle == FontStyle.BoldAndItalic,
                    isItalic = txt.fontStyle == FontStyle.Italic || txt.fontStyle == FontStyle.BoldAndItalic,
                    lineSpacing = txt.lineSpacing
                };
            }
        }

        private static string ProcessAndCacheImage(Sprite sprite)
        {
            if (sprite == null) return null;
            if (sprite.name == "Background" || sprite.name == "UISprite") return null;

            string cacheDir = Path.Combine(Application.persistentDataPath, "DynamicAdsCache");
            if (!Directory.Exists(cacheDir)) Directory.CreateDirectory(cacheDir);

            // Utilizing Sprite Name inside the cache. 
            string filePath = Path.Combine(cacheDir, sprite.texture.name + ".png");

            // Cache Invalidation Check -> we skip expensive compression if cached natively
            if (!File.Exists(filePath))
            {
                try
                {
                    // Trick: Blit to temporary RenderTexture to bypass Unity's 'Not Readable' texture settings restriction
                    Texture2D tex = DuplicateReadableTexture(sprite.texture);
                    byte[] bytes = tex.EncodeToPNG();
                    File.WriteAllBytes(filePath, bytes);

                    if (Application.isPlaying) GameObject.Destroy(tex);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[DynamicNativeAds] Texture Encode Failed for " + sprite.texture.name + ". " + e.Message);
                    return null;
                }
            }
            return filePath;
        }

        private static Texture2D DuplicateReadableTexture(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;

            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }
    }
}
