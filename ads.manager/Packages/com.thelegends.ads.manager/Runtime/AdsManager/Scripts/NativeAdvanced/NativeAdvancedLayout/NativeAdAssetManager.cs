using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace TheLegends.Base.Ads
{
    /// <summary>
    /// Manages the persistence and memory caching of generated Native Ad Layouts.
    /// Ensures that physical assets (textures) and JSON blueprints are synchronized with the current build.
    /// </summary>
    public static class NativeAdAssetManager
    {
        private const string PREF_KEY_BUILD_GUID = "AdsUI_CachedBuildGUID";

        // Memory cache for rapid JSON retrieval
        private static Dictionary<string, string> _layoutJsonCache = new Dictionary<string, string>();
        private static bool _isFileSystemInitialized = false;

        private static void EnsureCacheDirectoryInitialized()
        {
            if (_isFileSystemInitialized) return;

            string currentBuildGUID = Application.buildGUID;
            string lastCachedGUID = PlayerPrefs.GetString(PREF_KEY_BUILD_GUID, "");
            string cacheDir = Path.Combine(Application.persistentDataPath, "NativeAdLayoutCache");

            if (currentBuildGUID != lastCachedGUID)
            {
                // Purge deprecated cache if a new build version is detected
                if (Directory.Exists(cacheDir))
                {
                    Directory.Delete(cacheDir, true);
                    Debug.Log("[NativeAdAssetManager] Cache validation: Purged deprecated physical texture payloads.");
                }
                Directory.CreateDirectory(cacheDir);

                PlayerPrefs.SetString(PREF_KEY_BUILD_GUID, currentBuildGUID);
                PlayerPrefs.Save();
            }
            _isFileSystemInitialized = true;
        }

        /// <summary>
        /// Scans and exports a specific UI layout, storing the result in memory for rapid retrieval.
        /// This method should be called during initialization or when a custom layout needs to be refreshed.
        /// </summary>
        /// <param name="rootObj">The root GameObject containing the UI elements and NativeAdLayoutMark components.</param>
        public static void InitializeAndCache(GameObject rootObj)
        {
            EnsureCacheDirectoryInitialized();

            if (rootObj == null) return;

            string layoutId = rootObj.GetInstanceID().ToString();
            RectTransform rectTransform = rootObj.GetComponent<RectTransform>();

            if (rectTransform != null)
            {
                NativeAdLayoutConfig config = NativeAdLayoutExporter.GenerateConfig(layoutId, rectTransform);
                string jsonString = JsonConvert.SerializeObject(config);

                // Add or update the layout in the memory cache
                _layoutJsonCache[layoutId] = jsonString;
                
                Debug.Log($"[NativeAdAssetManager] Layout '{layoutId}' (from {rootObj.name}) cached successfully.");
            }
            else
            {
                Debug.LogWarning($"[NativeAdAssetManager] Failed to cache '{rootObj.name}': No RectTransform found on root object.");
            }
        }

        /// <summary>
        /// Retrieves the cached JSON blueprint for a specific layout ID.
        /// </summary>
        /// <param name="layoutId">The unique ID assigned during InitializeAndCache.</param>
        /// <returns>The localized JSON string if found; null otherwise.</returns>
        public static string GetLayoutJson(string layoutId)
        {
            if (_layoutJsonCache.TryGetValue(layoutId, out string json))
            {
                return json;
            }
            Debug.LogError($"[NativeAdAssetManager] Critical fault: Layout ID '{layoutId}' not found in cache registry.");
            return null;
        }

        /// <summary>
        /// Overload for batch initializing multiple UI layouts at once.
        /// </summary>
        /// <param name="rootCanvasesToExport">A list of root UI objects to be processed and cached.</param>
        public static void InitializeAndCache(params GameObject[] rootCanvasesToExport)
        {
            foreach (var root in rootCanvasesToExport)
            {
                if (root != null) InitializeAndCache(root);
            }
        }
    }
}
