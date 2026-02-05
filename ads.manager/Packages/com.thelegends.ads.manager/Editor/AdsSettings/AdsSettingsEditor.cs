using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEditor.PackageManager;
using UnityEngine.Networking;

#if USE_ADMOB
using GoogleMobileAds.Editor;
#endif

namespace TheLegends.Base.Ads
{
    [CustomEditor(typeof(AdsSettings))]
    public class AdsSettingsEditor : Editor
    {
        private static AdsSettings instance = null;

        public static AdsSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<AdsSettings>(AdsSettings.FileName);
                }

                if (instance != null)
                {
                    Selection.activeObject = instance;
                }
                else
                {
                    Directory.CreateDirectory(AdsSettings.ResDir);

                    instance = CreateInstance<AdsSettings>();

                    string assetPath = Path.Combine(AdsSettings.ResDir, AdsSettings.FileName);
                    string assetPathWithExtension = Path.ChangeExtension(assetPath, AdsSettings.FileExtension);
                    AssetDatabase.CreateAsset(instance, assetPathWithExtension);
                    AssetDatabase.SaveAssets();
                }

                return instance;
            }
        }

        [MenuItem("TripSoft/Ads Settings")]
        public static void OpenInspector()
        {
            if (Instance == null)
            {
                Debug.Log("Creat new Ads Settings");
            }
        }

        // ─── Foldout persistence ─────────────────────────────────────────────────
        private bool _networksFoldout = true;
        private bool _servicesFoldout = true;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            // ╔══════════════════════════════════════════════════════════════╗
            // ║                    STYLES & COLORS                          ║
            // ╚══════════════════════════════════════════════════════════════╝
            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = Color.cyan },
                fontSize = 12,
            };

            var sectionHeaderStyle = new GUIStyle(EditorStyles.foldoutHeader)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 11,
            };

            Color panelBg = EditorGUIUtility.isProSkin
                ? new Color(0.18f, 0.18f, 0.18f)
                : new Color(0.88f, 0.88f, 0.88f);

            // ╔══════════════════════════════════════════════════════════════╗
            // ║              SECTION 1 — AD NETWORKS                        ║
            // ╚══════════════════════════════════════════════════════════════╝
            int activeNetworkCount = CountActiveNetworks();
            string networksHeader = $"AD NETWORKS  ({activeNetworkCount} active)";

            DrawSectionBackground(panelBg, () =>
            {
                _networksFoldout = EditorGUILayout.Foldout(_networksFoldout, networksHeader, true, sectionHeaderStyle);
                if (_networksFoldout)
                {
                    EditorGUILayout.Space(4);
                    DrawNetworkGrid();
                    EditorGUILayout.Space(4);

                    // Primary Network — only show when 2+ networks are active
                    var active = GetActiveNetworkList();
                    if (active.Count >= 2)
                    {
                        if (!active.Contains(Instance.primaryNetwork))
                            Instance.primaryNetwork = active[0];

                        var names = active.Select(n => n.ToString()).ToArray();
                        var values = active.Select(n => (int)n).ToArray();
                        int idx = Array.IndexOf(values, (int)Instance.primaryNetwork);
                        int newIdx = EditorGUILayout.Popup(
                            new GUIContent("  Primary Network",
                                "The network used as the main ad source when multiple are enabled."),
                            idx, names);
                        if (newIdx != idx)
                            Instance.primaryNetwork = (AdsNetworks)values[newIdx];
                    }
                    else
                    {
                        Instance.primaryNetwork = active.Count == 1 ? active[0] : AdsNetworks.None;
                    }

                    // Inline symbol warnings
                    DrawSymbolWarning(AdsNetworks.Iron, "USE_IRON");
                    DrawSymbolWarning(AdsNetworks.Max, "USE_MAX");
                    DrawSymbolWarning(AdsNetworks.Admob, "USE_ADMOB");
                }
            });

            EditorGUILayout.Space(6);

            // ╔══════════════════════════════════════════════════════════════╗
            // ║            SECTION 2 — SERVICES (Firebase / AF)             ║
            // ╚══════════════════════════════════════════════════════════════╝
            int activeServiceCount = (Instance.useFirebase ? 1 : 0) + (Instance.useAppsFlyer ? 1 : 0);
            string servicesHeader = $"SERVICES  ({activeServiceCount} active)";

            DrawSectionBackground(panelBg, () =>
            {
                _servicesFoldout = EditorGUILayout.Foldout(_servicesFoldout, servicesHeader, true, sectionHeaderStyle);
                if (_servicesFoldout)
                {
                    EditorGUILayout.Space(4);
                    DrawServicesGrid();
                    EditorGUILayout.Space(4);
                }
            });

            EditorGUILayout.Space(8);

            // ╔══════════════════════════════════════════════════════════════╗
            // ║                      SAVE BUTTON                            ║
            // ╚══════════════════════════════════════════════════════════════╝
            GUI.backgroundColor = new Color(0.3f, 0.85f, 0.45f);
            if (GUILayout.Button("SAVE", GUILayout.Height(32)))
            {
                GUI.backgroundColor = Color.white;
                Save(Instance);

                PackagesManagerIntergration.SetSymbolEnabled("USE_IRON", Instance.showIRON);
                PackagesManagerIntergration.SetSymbolEnabled("USE_MAX", Instance.showMAX);
                PackagesManagerIntergration.SetSymbolEnabled("USE_ADMOB", Instance.showADMOB);
                PackagesManagerIntergration.SetSymbolEnabled("USE_FIREBASE", Instance.useFirebase);
                PackagesManagerIntergration.SetSymbolEnabled("USE_APPSFLYER", Instance.useAppsFlyer);

                PackagesManagerIntergration.SetSymbolEnabled("USE_ADMOB_NATIVE_UNITY",
                    Instance.showADMOB && Instance.isUseNativeUnity);
                PackagesManagerIntergration.SetSymbolEnabled("HIDE_WHEN_FULLSCREEN_SHOWED",
                    Instance.showADMOB && Instance.isHideWhenFullscreenShowed);

                PackagesManagerIntergration.UpdateManifest();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);

            // ╔══════════════════════════════════════════════════════════════╗
            // ║                   GENERAL SETTINGS                          ║
            // ╚══════════════════════════════════════════════════════════════╝
            EditorGUILayout.LabelField("General Settings", titleStyle);
            EditorGUILayout.Space(2);

            Instance.bannerPosition = (BannerPos)EditorGUILayout.EnumPopup("Banner Position", Instance.bannerPosition);
            Instance.fixBannerSmallSize = EditorGUILayout.Toggle("Fix Banner Small Size 320x50", Instance.fixBannerSmallSize);
            Instance.autoReLoadMax = EditorGUILayout.IntField("Max Auto Reload If No Ads", Instance.autoReLoadMax);
            Instance.isTest = EditorGUILayout.Toggle("Is Testing", Instance.isTest);

            EditorGUILayout.Space(6);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("preloadSettings"), true);
            EditorGUILayout.Space(6);

            DrawGoogleSheetSync();

            // ╔══════════════════════════════════════════════════════════════╗
            // ║               PER-NETWORK DETAIL CONFIGS                    ║
            // ╚══════════════════════════════════════════════════════════════╝

            #region IronSource
#if USE_IRON
            if ((Instance.FlagNetWorks & AdsNetworks.Iron) != 0)
            {
                if (AssetDatabase.IsValidFolder("Assets/LevelPlay/Editor/"))
                {
                    IronSourceMediationSettings ironSourceMediationSettings = Resources.Load<IronSourceMediationSettings>(IronSourceConstants.IRONSOURCE_MEDIATION_SETTING_NAME);
                    if (ironSourceMediationSettings == null)
                    {
                        IronSourceMediationSettings asset = CreateInstance<IronSourceMediationSettings>();
                        Directory.CreateDirectory(IronSourceConstants.IRONSOURCE_RESOURCES_PATH);
                        AssetDatabase.CreateAsset(asset, IronSourceMediationSettings.IRONSOURCE_SETTINGS_ASSET_PATH);
                        ironSourceMediationSettings = asset;
                    }
                    ironSourceMediationSettings.AndroidAppKey = instance.ironAndroidAppKey;
                    ironSourceMediationSettings.IOSAppKey     = instance.ironIOSAppKey;
                    ironSourceMediationSettings.AddIronsourceSkadnetworkID    = true;
                    ironSourceMediationSettings.DeclareAD_IDPermission        = true;
                    ironSourceMediationSettings.EnableIronsourceSDKInitAPI    = false;

                    IronSourceMediatedNetworkSettings ironSourceMediatedNetworkSettings = Resources.Load<IronSourceMediatedNetworkSettings>(IronSourceConstants.IRONSOURCE_MEDIATED_NETWORK_SETTING_NAME);
                    if (ironSourceMediatedNetworkSettings == null)
                    {
                        IronSourceMediatedNetworkSettings asset = CreateInstance<IronSourceMediatedNetworkSettings>();
                        Directory.CreateDirectory(IronSourceConstants.IRONSOURCE_RESOURCES_PATH);
                        AssetDatabase.CreateAsset(asset, IronSourceMediatedNetworkSettings.MEDIATION_SETTINGS_ASSET_PATH);
                        ironSourceMediatedNetworkSettings = asset;
                    }

                    EditorGUILayout.Space(6);
                    EditorGUILayout.LabelField("IronSource / LevelPlay", titleStyle);

                    Instance.ironAndroidAppKey = EditorGUILayout.TextField("Android AppKey", Instance.ironAndroidAppKey);
                    Instance.ironIOSAppKey     = EditorGUILayout.TextField("iOS AppKey",     Instance.ironIOSAppKey);
                    Instance.isIronTest        = EditorGUILayout.Toggle("Is Testing",        Instance.isIronTest);

                    Instance.ironEnableAdmob = EditorGUILayout.Toggle("Enable Admob Mediation", Instance.ironEnableAdmob);
                    ironSourceMediatedNetworkSettings.EnableAdmob = Instance.ironEnableAdmob;
                    if (Instance.ironEnableAdmob)
                    {
                        Instance.ironAdmobAndroidAppID = EditorGUILayout.TextField("Admob Android AppID", Instance.ironAdmobAndroidAppID);
                        Instance.ironAdmobIOSAppID     = EditorGUILayout.TextField("Admob iOS AppID",     Instance.ironAdmobIOSAppID);
                        ironSourceMediatedNetworkSettings.AdmobAndroidAppId = Instance.ironAdmobAndroidAppID;
                        ironSourceMediatedNetworkSettings.AdmobIOSAppId     = Instance.ironAdmobIOSAppID;
                    }
                    else
                    {
                        Instance.ironAdmobAndroidAppID = Instance.ironAdmobIOSAppID = string.Empty;
                        ironSourceMediatedNetworkSettings.AdmobAndroidAppId = ironSourceMediatedNetworkSettings.AdmobIOSAppId = string.Empty;
                    }

                    AssetDatabase.SaveAssetIfDirty(ironSourceMediationSettings);
                }
            }
#endif
            #endregion

            #region MAX
#if USE_MAX
            if ((Instance.FlagMediations & AdsMediation.Max) != 0)
            {
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("MAX AppLovin", titleStyle);
                Instance.isShowMediationDebugger = EditorGUILayout.Toggle("Use Mediation Debugger", Instance.isShowMediationDebugger);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MAX_Android"), new GUIContent("Android Unit IDs"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MAX_iOS"),     new GUIContent("iOS Unit IDs"),     true);
            }
#endif
            #endregion

            #region ADMOB
#if USE_ADMOB
            if ((Instance.FlagMediations & AdsMediation.Admob) != 0)
            {
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("Google Admob", titleStyle);
                Instance.isShowAdmobNativeValidator = EditorGUILayout.Toggle("Show Admob Validator", Instance.isShowAdmobNativeValidator);
                Instance.isUseNativeUnity = EditorGUILayout.Toggle("Use Native Unity", Instance.isUseNativeUnity);
                Instance.isHideWhenFullscreenShowed = EditorGUILayout.Toggle("Hide When Fullscreen Showed", Instance.isHideWhenFullscreenShowed);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ADMOB_Android"), new GUIContent("Android Unit IDs"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ADMOB_IOS"), new GUIContent("iOS Unit IDs"), true);
            }
#endif
            #endregion

            if (EditorGUI.EndChangeCheck())
            {
                Save((AdsSettings)target);
            }
        }

        // ─── Layout Helpers ──────────────────────────────────────────────────────

        /// <summary>Wraps content in a tinted background box.</summary>
        private static void DrawSectionBackground(Color bg, System.Action content)
        {
            var prevBg = GUI.backgroundColor;
            GUI.backgroundColor = bg;
            EditorGUILayout.BeginVertical("helpBox");
            GUI.backgroundColor = prevBg;
            content?.Invoke();
            EditorGUILayout.EndVertical();
        }

        /// <summary>Draws the ad-network checkboxes in a 3-column grid.</summary>
        private void DrawNetworkGrid()
        {
            // Define all known networks with their labels. Add more here as needed.
            var networks = new (AdsNetworks network, string label, bool enabled)[]
            {
                (AdsNetworks.Admob, "Google Admob",   Instance.showADMOB),
                (AdsNetworks.Max,   "AppLovin MAX",   Instance.showMAX),
                (AdsNetworks.Iron,  "IronSource / LP",Instance.showIRON),
                // Future networks go here — the grid wraps automatically.
            };

            const int columns = 3;
            int col = 0;

            for (int i = 0; i < networks.Length; i++)
            {
                if (col == 0) EditorGUILayout.BeginHorizontal();

                var (network, label, enabled) = networks[i];
                bool newEnabled = GUILayout.Toggle(enabled, label, "Button",
                    GUILayout.Height(24), GUILayout.MinWidth(90));

                if (newEnabled != enabled)
                {
                    // Sync AdsSettings flags
                    if (newEnabled) Instance.FlagNetWorks |= network;
                    else Instance.FlagNetWorks &= ~network;

                    switch (network)
                    {
                        case AdsNetworks.Admob: Instance.showADMOB = newEnabled; break;
                        case AdsNetworks.Max: Instance.showMAX = newEnabled; break;
                        case AdsNetworks.Iron: Instance.showIRON = newEnabled; break;
                    }
                }

                col++;
                if (col == columns || i == networks.Length - 1)
                {
                    // Pad remaining cells so the grid stays aligned
                    while (col < columns) { GUILayout.FlexibleSpace(); col++; }
                    EditorGUILayout.EndHorizontal();
                    col = 0;
                }
            }
        }

        /// <summary>Draws the optional services (Firebase, AppsFlyer) in a grid.</summary>
        private void DrawServicesGrid()
        {
            var services = new (string label, bool enabled, System.Action<bool> setter)[]
            {
                ("Firebase",   Instance.useFirebase,  v => Instance.useFirebase  = v),
                ("AppsFlyer",  Instance.useAppsFlyer, v => Instance.useAppsFlyer = v),
                // Add more optional services here in the future.
            };

            const int columns = 3;
            int col = 0;

            for (int i = 0; i < services.Length; i++)
            {
                if (col == 0) EditorGUILayout.BeginHorizontal();

                var (label, enabled, setter) = services[i];
                bool newEnabled = GUILayout.Toggle(enabled, label, "Button",
                    GUILayout.Height(24), GUILayout.MinWidth(90));

                if (newEnabled != enabled) setter(newEnabled);

                col++;
                if (col == columns || i == services.Length - 1)
                {
                    while (col < columns) { GUILayout.FlexibleSpace(); col++; }
                    EditorGUILayout.EndHorizontal();
                    col = 0;
                }
            }
        }

        private List<AdsNetworks> GetActiveNetworkList()
        {
            var list = new List<AdsNetworks>();
            foreach (AdsNetworks n in Enum.GetValues(typeof(AdsNetworks)))
            {
                if (n == AdsNetworks.None) continue;
                if ((Instance.FlagNetWorks & n) == n) list.Add(n);
            }
            return list;
        }

        private int CountActiveNetworks() => GetActiveNetworkList().Count;

        private void DrawSymbolWarning(AdsNetworks network, string symbol)
        {
            if ((Instance.FlagNetWorks & network) != 0 &&
                !PackagesManagerIntergration.IsSymbolEnabled(symbol))
            {
                EditorGUILayout.HelpBox(
                    $"Press SAVE to add scripting define \"{symbol}\" to Player Settings.",
                    MessageType.Warning);
            }
        }

        private void Save(Object target)
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void DrawGoogleSheetSync()
        {
            EditorGUILayout.BeginVertical(GUI.skin.window);
            EditorGUILayout.LabelField("Google Sheet Sync", new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = Color.yellow } });
            EditorGUILayout.Space(5);

            // Admob
            Instance.admobSheetUrl = EditorGUILayout.TextField("Admob CSV URL", Instance.admobSheetUrl);
            if (GUILayout.Button("Fetch Admob Data"))
            {
                if (!string.IsNullOrEmpty(Instance.admobSheetUrl))
                {
                    FetchDataFromGoogleSheet(Instance.admobSheetUrl, "ADMOB");
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Admob Sheet URL is empty.", "OK");
                }
            }

            EditorGUILayout.Space(5);

            // MAX
            Instance.maxSheetUrl = EditorGUILayout.TextField("MAX CSV URL", Instance.maxSheetUrl);
            if (GUILayout.Button("Fetch MAX Data"))
            {
                if (!string.IsNullOrEmpty(Instance.maxSheetUrl))
                {
                    FetchDataFromGoogleSheet(Instance.maxSheetUrl, "MAX");
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "MAX Sheet URL is empty.", "OK");
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
        }

        private void FetchDataFromGoogleSheet(string url, string mediationPrefix)
        {
            Debug.Log("Fetching data from Google Sheet...");
            UnityWebRequest www = UnityWebRequest.Get(url);
            var operation = www.SendWebRequest();

            EditorApplication.update += () =>
            {
                if (!operation.isDone)
                    return;

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Fetch successful!");
                    ParseAndApplyData(www.downloadHandler.text, mediationPrefix);
                }
                else
                {
                    Debug.LogError("Fetch failed: " + www.error);
                    EditorUtility.DisplayDialog("Error", "Failed to fetch data from URL. Check console for details.", "OK");
                }

                EditorApplication.update = null;
            };
        }

        private void ParseAndApplyData(string csvData, string mediationPrefix)
        {
            if (string.IsNullOrEmpty(csvData)) return;

            var lines = csvData.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 1)
            {
                Debug.LogWarning("CSV data is empty.");
                return;
            }

            var header = lines[0].Split(',');
            var dataRows = new List<string[]>();
            for (int i = 1; i < lines.Length; i++)
            {
                dataRows.Add(lines[i].Split(','));
            }

            var newData = new Dictionary<string, List<string>>();
            for (int j = 0; j < header.Length; j++)
            {
                var columnName = header[j].Trim();
                if (string.IsNullOrEmpty(columnName)) continue;

                var idsInColumn = new List<string>();
                for (int i = 0; i < dataRows.Count; i++)
                {
                    if (j < dataRows[i].Length)
                    {
                        string id = dataRows[i][j].Trim();
                        if (!string.IsNullOrEmpty(id))
                        {
                            idsInColumn.Add(id);
                        }
                    }
                }
                newData[columnName] = idsInColumn;
            }

            Action<object, string> applyDataToUnitId = (unitId, platformPrefix) =>
            {
                if (unitId == null) return;
                var fields = unitId.GetType().GetFields().Where(f => f.FieldType == typeof(List<Placement>));

                foreach (var field in fields)
                {
                    string adType = field.Name.Replace("Ids", "");
                    string columnName = $"{platformPrefix}_{adType}";

                    var idList = (List<Placement>)field.GetValue(unitId);
                    idList.Clear();

                    if (newData.ContainsKey(columnName))
                    {
                        foreach (var id in newData[columnName])
                        {
                            idList.Add(new Placement { stringIDs = new List<string> { id } });
                        }
                    }
                }
            };

            if (mediationPrefix.Equals("ADMOB", StringComparison.OrdinalIgnoreCase))
            {
                applyDataToUnitId(Instance.ADMOB_Android, "Android");
                applyDataToUnitId(Instance.ADMOB_IOS, "iOS");
            }
            else if (mediationPrefix.Equals("MAX", StringComparison.OrdinalIgnoreCase))
            {
                applyDataToUnitId(Instance.MAX_Android, "Android");
                applyDataToUnitId(Instance.MAX_iOS, "iOS");
            }

            Debug.Log($"Successfully applied data for {mediationPrefix} from Google Sheet!");
            EditorUtility.SetDirty(Instance);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            serializedObject.Update();
        }
    }
}
