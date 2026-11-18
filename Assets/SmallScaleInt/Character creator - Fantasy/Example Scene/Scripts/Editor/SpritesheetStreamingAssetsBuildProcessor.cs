using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SmallScaleInc.CharacterCreatorFantasy.Editor
{
    public class SpritesheetStreamingAssetsBuildProcessor : IPreprocessBuildWithReport
    {
        private const string SourceFolder = "Assets/SmallScaleInt/Character creator - Fantasy/spritesheets";
        private const string AlternateSourceFolder = "Assets/SmallScaleInt/Character creator - Fantasy/Spritesheets";
        private const string TargetFolder = "Assets/StreamingAssets/Spritesheets";

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            CopySpritesheetsToStreamingAssets();
        }

        [MenuItem("Tools/Character Creator/Copy Spritesheets To StreamingAssets")]
        public static void CopySpritesheetsToStreamingAssets()
        {
            string source = Directory.Exists(SourceFolder) ? SourceFolder : AlternateSourceFolder;
            if (!Directory.Exists(source))
            {
                Debug.LogError($"Could not find source spritesheets folder at '{SourceFolder}' or '{AlternateSourceFolder}'.");
                return;
            }

            if (Directory.Exists(TargetFolder))
                Directory.Delete(TargetFolder, true);

            CopyPngDirectory(source, TargetFolder);
            AssetDatabase.Refresh();
            Debug.Log($"Copied spritesheets to build StreamingAssets folder: {TargetFolder}");
        }

        private static void CopyPngDirectory(string sourceRoot, string targetRoot)
        {
            Directory.CreateDirectory(targetRoot);

            foreach (string sourceDirectory in Directory.GetDirectories(sourceRoot, "*", SearchOption.AllDirectories))
            {
                string relativeDirectory = sourceDirectory.Substring(sourceRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                Directory.CreateDirectory(Path.Combine(targetRoot, relativeDirectory));
            }

            foreach (string sourceFile in Directory.GetFiles(sourceRoot, "*.png", SearchOption.AllDirectories))
            {
                string relativeFile = sourceFile.Substring(sourceRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string targetFile = Path.Combine(targetRoot, relativeFile);
                Directory.CreateDirectory(Path.GetDirectoryName(targetFile));
                File.Copy(sourceFile, targetFile, true);
            }
        }
    }
}
