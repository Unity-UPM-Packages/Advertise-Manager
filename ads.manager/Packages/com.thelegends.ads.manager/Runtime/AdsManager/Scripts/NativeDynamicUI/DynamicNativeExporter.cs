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
            var config = new NativeAdLayoutConfig { 
                layoutId = layoutId,
                referenceWidth = rootCanvasRect.rect.width,
                referenceHeight = rootCanvasRect.rect.height
            };
            
            // Collect all marked parts of the Native Ad
            var marks = rootCanvasRect.GetComponentsInChildren<DynamicNativeMark>(true); 

            float screenW = rootCanvasRect.rect.width;
            float screenH = rootCanvasRect.rect.height;

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
                Vector3 bl = rootCanvasRect.InverseTransformPoint(corners[0]);
                Vector3 tr = rootCanvasRect.InverseTransformPoint(corners[2]);

                float xMin = (bl.x + screenW * rootCanvasRect.pivot.x) / screenW;
                float yMin = (bl.y + screenH * rootCanvasRect.pivot.y) / screenH;
                float xMax = (tr.x + screenW * rootCanvasRect.pivot.x) / screenW;
                float yMax = (tr.y + screenH * rootCanvasRect.pivot.y) / screenH;

                elementConfig.rectTransform = new RectTransformConfig
                {
                    anchorMin = new Vector2(xMin, yMin),
                    anchorMax = new Vector2(xMax, yMax),
                    offsetMin = Vector2.zero, // Zero absolute scaling, eliminating device DPI differences!
                    offsetMax = Vector2.zero,
                    pivot = rt.pivot,
                    rotationZ = rt.eulerAngles.z,
                    scaleX = 1f, scaleY = 1f 
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
            if (img != null && img.enabled)
            {
                elementConfig.image = new ImageConfig
                {
                    color = "#" + ColorUtility.ToHtmlStringRGBA(img.color),
                    cornerRadius = mark.customCornerRadius,
                    isRadialFill = mark.isRadialFill,
                    imagePath = ProcessAndCacheImage(img.sprite)
                };
            }

            // Cải tiến: Quét cả text ở GameObject con (Vì Unity Button thường thiết kế lớp Nền (Image) bao bọc Text (Con))
            var txt = mark.GetComponentInChildren<Text>(true);
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
                    
                    if(Application.isPlaying) GameObject.Destroy(tex);
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
