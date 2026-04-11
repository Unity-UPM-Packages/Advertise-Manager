#if UNITY_EDITOR && UNITY_IOS
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    /// <summary>
    /// Post-process build script that ensures the admob_native_unity.xcframework
    /// is properly embedded into the Xcode project when building for iOS.
    ///
    /// WHY THIS IS NEEDED:
    /// When the AdsManager package is imported via UPM (Git/Registry), Unity treats it as
    /// read-only (immutable). In some Unity versions, xcframework plugins inside UPM packages
    /// are NOT automatically added to the Xcode project's "Embed Frameworks" build phase.
    /// This script detects and fixes that by:
    ///   1. Locating the xcframework in the built Xcode project
    ///   2. Ensuring it is added to the Xcode project's framework references
    ///   3. Ensuring it is added to the "Embed Frameworks" build phase
    ///   4. Setting "User Script Sandboxing" to NO (required for framework embedding)
    /// </summary>
    public class EmbedAdmobNativeFrameworkIOS
    {
        private const string FRAMEWORK_NAME = "admob_native_unity.framework";
        private const string XCFRAMEWORK_NAME = "admob_native_unity.xcframework";

        [PostProcessBuildAttribute(999)] // Run after other post-process scripts
        public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToXcode)
        {
            if (buildTarget != BuildTarget.iOS) return;

            try
            {
                Debug.Log("--- EmbedAdmobNativeFrameworkIOS: Executing... ---");
                EnsureFrameworkEmbedded(pathToXcode);
                DisableScriptSandboxing(pathToXcode);
                Debug.Log("--- EmbedAdmobNativeFrameworkIOS: Completed successfully! ---");
            }
            catch (Exception ex)
            {
                Debug.LogError($"EmbedAdmobNativeFrameworkIOS: Error - {ex.Message}");
                Debug.LogException(ex);
            }
        }

        private static void EnsureFrameworkEmbedded(string pathToXcode)
        {
            string pbxProjectPath = PBXProject.GetPBXProjectPath(pathToXcode);
            PBXProject project = new PBXProject();
            project.ReadFromFile(pbxProjectPath);

            // Unity 2019.3+ uses UnityFramework target for frameworks
            string mainTargetGuid = project.GetUnityMainTargetGuid();
            string frameworkTargetGuid = project.GetUnityFrameworkTargetGuid();

            // Search for the framework in the built Xcode project's Frameworks directory
            string frameworksPath = Path.Combine(pathToXcode, "Frameworks");
            string frameworkPath = FindFrameworkRecursive(frameworksPath, FRAMEWORK_NAME);

            if (string.IsNullOrEmpty(frameworkPath))
            {
                // Framework not found in Frameworks/ — try to locate the xcframework from the package
                // and copy it manually
                string sourceXcframework = FindXcframeworkInPackage();
                if (!string.IsNullOrEmpty(sourceXcframework))
                {
                    string destPath = Path.Combine(frameworksPath, XCFRAMEWORK_NAME);
                    if (!Directory.Exists(destPath))
                    {
                        CopyDirectory(sourceXcframework, destPath);
                        Debug.Log($"EmbedAdmobNativeFrameworkIOS: Copied xcframework to {destPath}");
                    }

                    // After copying, find the arm64 framework inside the xcframework
                    frameworkPath = FindFrameworkRecursive(frameworksPath, FRAMEWORK_NAME);
                }

                if (string.IsNullOrEmpty(frameworkPath))
                {
                    Debug.LogWarning("EmbedAdmobNativeFrameworkIOS: Could not locate admob_native_unity.framework in Xcode project. Skipping.");
                    return;
                }
            }

            // Convert absolute path to Xcode-relative path
            string relativePath = frameworkPath.Replace(pathToXcode + Path.DirectorySeparatorChar, "")
                                               .Replace(pathToXcode + "/", "");

            Debug.Log($"EmbedAdmobNativeFrameworkIOS: Found framework at: {relativePath}");

            // Add framework to the project if not already present
            string fileGuid = project.FindFileGuidByProjectPath(relativePath);
            if (string.IsNullOrEmpty(fileGuid))
            {
                fileGuid = project.AddFile(relativePath, relativePath);
                Debug.Log($"EmbedAdmobNativeFrameworkIOS: Added framework file to project.");
            }

            // Add to main target's framework references
            if (!project.ContainsFramework(mainTargetGuid, FRAMEWORK_NAME))
            {
                project.AddFrameworkToProject(mainTargetGuid, FRAMEWORK_NAME, false);
                Debug.Log($"EmbedAdmobNativeFrameworkIOS: Added framework to main target.");
            }

            // Ensure the framework is in the "Embed Frameworks" build phase (Code & Sign)
            project.AddFileToEmbedFrameworks(mainTargetGuid, fileGuid);
            Debug.Log($"EmbedAdmobNativeFrameworkIOS: Ensured framework is embedded (Code & Sign).");

            // Set framework search paths
            project.AddBuildProperty(mainTargetGuid, "FRAMEWORK_SEARCH_PATHS", "$(inherited)");
            project.AddBuildProperty(mainTargetGuid, "FRAMEWORK_SEARCH_PATHS", "$(PROJECT_DIR)/Frameworks/**");
            project.AddBuildProperty(frameworkTargetGuid, "FRAMEWORK_SEARCH_PATHS", "$(inherited)");
            project.AddBuildProperty(frameworkTargetGuid, "FRAMEWORK_SEARCH_PATHS", "$(PROJECT_DIR)/Frameworks/**");

            // Ensure LD_RUNPATH_SEARCH_PATHS includes @executable_path/Frameworks
            project.AddBuildProperty(mainTargetGuid, "LD_RUNPATH_SEARCH_PATHS", "$(inherited)");
            project.AddBuildProperty(mainTargetGuid, "LD_RUNPATH_SEARCH_PATHS", "@executable_path/Frameworks");

            project.WriteToFile(pbxProjectPath);
        }

        /// <summary>
        /// Disables User Script Sandboxing in the Xcode project, which is required
        /// for frameworks embedded from external packages.
        /// </summary>
        private static void DisableScriptSandboxing(string pathToXcode)
        {
            string pbxProjectPath = PBXProject.GetPBXProjectPath(pathToXcode);
            PBXProject project = new PBXProject();
            project.ReadFromFile(pbxProjectPath);

            string mainTargetGuid = project.GetUnityMainTargetGuid();
            string frameworkTargetGuid = project.GetUnityFrameworkTargetGuid();

            project.SetBuildProperty(mainTargetGuid, "ENABLE_USER_SCRIPT_SANDBOXING", "NO");
            project.SetBuildProperty(frameworkTargetGuid, "ENABLE_USER_SCRIPT_SANDBOXING", "NO");

            project.WriteToFile(pbxProjectPath);
            Debug.Log("EmbedAdmobNativeFrameworkIOS: Disabled User Script Sandboxing.");
        }

        /// <summary>
        /// Locates the xcframework within the UPM package directory.
        /// </summary>
        private static string FindXcframeworkInPackage()
        {
            // Try to resolve package path via Unity's PackageManager
            string packagePath = Path.GetFullPath("Packages/com.thelegends.ads.manager");
            string expectedPath = Path.Combine(packagePath, "Runtime", "Plugins", "iOS", XCFRAMEWORK_NAME);

            if (Directory.Exists(expectedPath))
            {
                Debug.Log($"EmbedAdmobNativeFrameworkIOS: Found xcframework in package at: {expectedPath}");
                return expectedPath;
            }

            Debug.LogWarning($"EmbedAdmobNativeFrameworkIOS: xcframework not found at expected path: {expectedPath}");
            return null;
        }

        /// <summary>
        /// Recursively searches for a directory with the specified name.
        /// </summary>
        private static string FindFrameworkRecursive(string searchDir, string frameworkName)
        {
            if (!Directory.Exists(searchDir)) return null;

            try
            {
                var found = Directory.GetDirectories(searchDir, frameworkName, SearchOption.AllDirectories);
                return found.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Recursively copies a directory and all its contents.
        /// </summary>
        private static void CopyDirectory(string source, string destination)
        {
            Directory.CreateDirectory(destination);

            foreach (string file in Directory.GetFiles(source))
            {
                string destFile = Path.Combine(destination, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (string dir in Directory.GetDirectories(source))
            {
                string destDir = Path.Combine(destination, Path.GetFileName(dir));
                CopyDirectory(dir, destDir);
            }
        }
    }
}
#endif
