using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TheLegends.Base.Ads.NativeDynamicUI;

namespace TheLegends.Base.Ads.NativeDynamicUI
{
    public static class DynamicAdsCacheManager
    {
        private const string PREF_KEY_BUILD_GUID = "AdsUI_CachedBuildGUID";
        
        // Fast JSON access registry
        private static Dictionary<string, string> _layoutJsonCache = new Dictionary<string, string>();

        public static void InitializeAndCache(params GameObject[] rootCanvasesToExport)
        {
            string currentBuildGUID = Application.buildGUID;
            string lastCachedGUID = PlayerPrefs.GetString(PREF_KEY_BUILD_GUID, "");
            
            string cacheDir = Path.Combine(Application.persistentDataPath, "DynamicAdsCache");

            if (currentBuildGUID != lastCachedGUID)
            {
                // App Installation Upgrade Detected
                if (Directory.Exists(cacheDir))
                {
                    Directory.Delete(cacheDir, true);
                    Debug.Log("[DynamicAdsCache] Garbage collection invoked: Purged deprecated physical texture payloads.");
                }
                Directory.CreateDirectory(cacheDir);

                PlayerPrefs.SetString(PREF_KEY_BUILD_GUID, currentBuildGUID);
                PlayerPrefs.Save();
            }

            // Routine payload parse over the UI designs in RAM
            _layoutJsonCache.Clear();

            foreach (var rootObj in rootCanvasesToExport)
            {
                if (rootObj == null) continue;
                
                string layoutId = rootObj.name; // Use prefab's name as unique key definition
                RectTransform rectTransform = rootObj.GetComponent<RectTransform>();
                
                if (rectTransform != null)
                {
                    NativeAdLayoutConfig config = DynamicNativeExporter.GenerateConfig(layoutId, rectTransform);
                    string jsonString = JsonUtility.ToJson(config);
                    
                    _layoutJsonCache[layoutId] = jsonString;
                }
            }
            
            Debug.Log($"[DynamicAdsCache] Standby state acquired: Hosted {_layoutJsonCache.Count} Dynamic Native UI packets.");
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
    }
}
