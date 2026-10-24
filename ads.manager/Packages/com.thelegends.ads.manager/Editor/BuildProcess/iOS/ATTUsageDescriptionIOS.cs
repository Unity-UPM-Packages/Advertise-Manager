#if UNITY_EDITOR && UNITY_IOS
using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    /// <summary>
    /// A post-build script that adds NSUserTrackingUsageDescription to Info.plist,
    /// required by iOS to show the App Tracking Transparency prompt (AdsManager.RequestATT).
    /// An existing value is kept so a game can set its own text.
    /// </summary>
    public class ATTUsageDescriptionIOS
    {
        private const string TrackingUsageKey = "NSUserTrackingUsageDescription";
        private const string TrackingUsageDescription = "This game includes ads. To improve your experience and see ads that match your interests, allow tracking.";

        [PostProcessBuildAttribute(1000)]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToXcode)
        {
            if (buildTarget == BuildTarget.iOS)
            {
                AddTrackingUsageDescription(pathToXcode);
            }
        }

        private static void AddTrackingUsageDescription(string pathToXcode)
        {
            try
            {
                string plistPath = pathToXcode + "/Info.plist";

                if (!System.IO.File.Exists(plistPath))
                {
                    Debug.LogError("ATTUsageDescriptionIOS: Info.plist not found at path: " + plistPath);
                    return;
                }

                PlistDocument plist = new PlistDocument();
                plist.ReadFromFile(plistPath);

                PlistElementDict rootDict = plist.root;

                if (rootDict.values.ContainsKey(TrackingUsageKey))
                {
                    Debug.Log("ATTUsageDescriptionIOS: " + TrackingUsageKey + " already set, keeping it.");
                    return;
                }

                rootDict.SetString(TrackingUsageKey, TrackingUsageDescription);
                plist.WriteToFile(plistPath);

                Debug.Log("ATTUsageDescriptionIOS: Added " + TrackingUsageKey + " to Info.plist.");
            }
            catch (Exception ex)
            {
                Debug.LogError("ATTUsageDescriptionIOS: Error modifying Info.plist - " + ex.Message);
                Debug.LogException(ex);
            }
        }
    }
}
#endif
