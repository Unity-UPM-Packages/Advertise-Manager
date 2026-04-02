using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace TheLegends.Base.Ads
{
    public static class DynamicAdsCacheManager
    {
        private const string PREF_KEY_BUILD_GUID = "AdsUI_CachedBuildGUID";

        // Fast JSON access registry
        private static Dictionary<string, string> _layoutJsonCache = new Dictionary<string, string>();
        private static bool _isFileSystemInitialized = false;

        private static void EnsureCacheDirectoryInitialized()
        {
            if (_isFileSystemInitialized) return;

            string currentBuildGUID = Application.buildGUID;
            string lastCachedGUID = PlayerPrefs.GetString(PREF_KEY_BUILD_GUID, "");
            string cacheDir = Path.Combine(Application.persistentDataPath, "DynamicAdsCache");

            if (currentBuildGUID != lastCachedGUID)
            {
                // App Installation Upgrade Detected or New Build
                if (Directory.Exists(cacheDir))
                {
                    Directory.Delete(cacheDir, true);
                    Debug.Log("[DynamicAdsCache] Garbage collection: Purged deprecated physical texture payloads.");
                }
                Directory.CreateDirectory(cacheDir);

                PlayerPrefs.SetString(PREF_KEY_BUILD_GUID, currentBuildGUID);
                PlayerPrefs.Save();
            }
            _isFileSystemInitialized = true;
        }

        /// <summary>
        /// Automatically registers a UI layout using the GameObject's InstanceID.
        /// </summary>
        public static void InitializeAndCache(GameObject rootObj)
        {
            EnsureCacheDirectoryInitialized();

            if (rootObj == null) return;

            string layoutId = rootObj.GetInstanceID().ToString();
            RectTransform rectTransform = rootObj.GetComponent<RectTransform>();

            if (rectTransform != null)
            {
                NativeAdLayoutConfig config = DynamicNativeExporter.GenerateConfig(layoutId, rectTransform);
                string jsonString = JsonConvert.SerializeObject(config);

                // Add or update the layout in the memory cache
                _layoutJsonCache[layoutId] = jsonString;
                
                Debug.Log($"[DynamicAdsCache] Instance '{layoutId}' (from {rootObj.name}) cached successfully.");
            }
            else
            {
                Debug.LogWarning($"[DynamicAdsCache] Failed to cache '{rootObj.name}': No RectTransform found on root object.");
            }
        }

        public static string GetLayoutJson(string layoutId)
        {
            if (_layoutJsonCache.TryGetValue(layoutId, out string json))
            {
                return json;
            }
            Debug.LogError($"[DynamicAdsCache] Crucial fault: Layout ID '{layoutId}' could not be located in cache!");
            return null;
        }

        // Keep original method for backward compatibility if needed, but updated to use new logic
        public static void InitializeAndCache(params GameObject[] rootCanvasesToExport)
        {
            foreach (var root in rootCanvasesToExport)
            {
                if (root != null) InitializeAndCache(root);
            }
        }
    }
}
