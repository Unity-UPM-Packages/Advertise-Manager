#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UI;

namespace TheLegends.Base.Ads.Editor
{
    public class NativeAdFontBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;
        
        private const string FONT_CACHE_DIR = "Assets/StreamingAssets/NativeAdFonts";

        public void OnPreprocessBuild(BuildReport report)
        {
            // Only Android and iOS need the native fonts
            if (report.summary.platform != BuildTarget.Android && report.summary.platform != BuildTarget.iOS)
                return;

            Debug.Log("[NativeAdFontBuildProcessor] Scanning project for Native Ad Fonts...");
            
            if (!Directory.Exists(FONT_CACHE_DIR))
            {
                Directory.CreateDirectory(FONT_CACHE_DIR);
            }

            // Find all prefabs that might contain NativeAdLayoutMark
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            HashSet<Font> processedFonts = new HashSet<Font>();

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                var marks = prefab.GetComponentsInChildren<NativeAdLayoutMark>(true);
                if (marks.Length == 0) continue;

                foreach (var mark in marks)
                {
                    Text txt = mark.GetComponent<Text>();
                    if (txt == null)
                    {
                        foreach (Transform child in mark.transform)
                        {
                            txt = child.GetComponent<Text>();
                            if (txt != null) break;
                        }
                    }

                    if (txt != null && txt.font != null)
                    {
                        ProcessFont(txt.font, processedFonts);
                    }
                }
            }

            // 2. Find all scenes enabled in Build Settings
            foreach (var buildScene in EditorBuildSettings.scenes)
            {
                if (!buildScene.enabled || string.IsNullOrEmpty(buildScene.path)) continue;

                // Load the scene additively so we don't disrupt the developer's active workspace
                var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(buildScene.path, UnityEditor.SceneManagement.OpenSceneMode.Additive);
                
                var marks = Object.FindObjectsOfType<NativeAdLayoutMark>(true);
                foreach (var mark in marks)
                {
                    // Ensure we only process marks belonging to the newly loaded scene
                    if (mark.gameObject.scene != scene) continue;

                    Text txt = mark.GetComponent<Text>();
                    if (txt == null)
                    {
                        foreach (Transform child in mark.transform)
                        {
                            txt = child.GetComponent<Text>();
                            if (txt != null) break;
                        }
                    }

                    if (txt != null && txt.font != null)
                    {
                        ProcessFont(txt.font, processedFonts);
                    }
                }
                
                // Close the scene without saving to keep the build process clean
                UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
            }

            AssetDatabase.Refresh();
        }

        private void ProcessFont(Font font, HashSet<Font> processedFonts)
        {
            if (processedFonts.Contains(font)) return;
            processedFonts.Add(font);

            string assetPath = AssetDatabase.GetAssetPath(font);
            // Skip built-in fonts (Arial, etc.) which are usually in "Resources/unity_builtin_extra" or "Library/..."
            if (string.IsNullOrEmpty(assetPath) || assetPath.StartsWith("Resources") || assetPath.StartsWith("Library"))
            {
                return;
            }

            string extension = Path.GetExtension(assetPath).ToLower();
            if (extension == ".ttf" || extension == ".otf")
            {
                // We name the copied file EXACTLY as the font's internal name so runtime exporter can map it
                string destPath = Path.Combine(FONT_CACHE_DIR, font.name + extension);
                File.Copy(assetPath, destPath, true);
                Debug.Log($"[NativeAdFontBuildProcessor] Cached Custom Font: {font.name} -> {destPath}");
            }
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android && report.summary.platform != BuildTarget.iOS)
                return;

            // Clean up StreamingAssets after build
            if (Directory.Exists(FONT_CACHE_DIR))
            {
                Directory.Delete(FONT_CACHE_DIR, true);
                // Also delete the meta file generated by Unity
                string metaFile = FONT_CACHE_DIR + ".meta";
                if (File.Exists(metaFile))
                {
                    File.Delete(metaFile);
                }
                AssetDatabase.Refresh();
                Debug.Log("[NativeAdFontBuildProcessor] Cleaned up temporary Native Ad Fonts from StreamingAssets.");
            }
        }
    }
}
#endif
