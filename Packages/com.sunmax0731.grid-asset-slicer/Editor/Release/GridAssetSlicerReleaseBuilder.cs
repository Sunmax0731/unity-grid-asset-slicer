using System;
using System.IO;
using UnityEditor;

namespace Sunmax.GridAssetSlicer.Editor.Release
{
    internal static class GridAssetSlicerReleaseBuilder
    {
        private const string PackageRoot = "Packages/com.sunmax0731.grid-asset-slicer";
        private const string StageRoot = "Assets/__UnityGridAssetSlicerReleaseStaging__";
        private const string StageToolRoot = StageRoot + "/UnityGridAssetSlicer";

        [MenuItem("Tools/Grid Asset Slicer/Build Release UnityPackage")]
        private static void BuildUnityPackageInteractive()
        {
            var manifest = LoadManifest();
            var outputPath = EditorUtility.SaveFilePanel(
                "Build Unity Grid Asset Slicer UnityPackage",
                Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..")),
                $"UnityGridAssetSlicer-{manifest.version}",
                "unitypackage");

            if (string.IsNullOrWhiteSpace(outputPath))
            {
                return;
            }

            BuildUnityPackage(outputPath);
            EditorUtility.RevealInFinder(outputPath);
        }

        public static void BuildUnityPackageBatch()
        {
            var outputPath = Environment.GetEnvironmentVariable("UNITY_GRID_ASSET_SLICER_UNITYPACKAGE_OUTPUT");
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                throw new InvalidOperationException("UNITY_GRID_ASSET_SLICER_UNITYPACKAGE_OUTPUT is required.");
            }

            BuildUnityPackage(outputPath);
        }

        internal static void BuildUnityPackage(string outputPath)
        {
            var normalizedOutputPath = NormalizeUnityPackagePath(outputPath);

            try
            {
                CleanupStageRoot();
                CopyDirectoryContents(ProjectToAbsolutePath($"{PackageRoot}/Runtime"), ProjectToAbsolutePath($"{StageToolRoot}/Runtime"));
                CopyDirectoryContents(ProjectToAbsolutePath($"{PackageRoot}/Editor"), ProjectToAbsolutePath($"{StageToolRoot}/Editor"));
                CopyDirectoryContents(ProjectToAbsolutePath($"{PackageRoot}/Documentation~"), ProjectToAbsolutePath($"{StageToolRoot}/Documentation"));
                CopyDirectoryContents(ProjectToAbsolutePath($"{PackageRoot}/Samples~"), ProjectToAbsolutePath($"{StageToolRoot}/Samples"));
                CopyFile(ProjectToAbsolutePath($"{PackageRoot}/package.json"), ProjectToAbsolutePath($"{StageToolRoot}/package.json"));
                CopyFile(ProjectToAbsolutePath($"{PackageRoot}/README.md"), ProjectToAbsolutePath($"{StageToolRoot}/README.md"));
                CopyFile(ProjectToAbsolutePath($"{PackageRoot}/CHANGELOG.md"), ProjectToAbsolutePath($"{StageToolRoot}/CHANGELOG.md"));
                CopyFile(ProjectToAbsolutePath($"{PackageRoot}/LICENSE.md"), ProjectToAbsolutePath($"{StageToolRoot}/LICENSE.md"));

                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                var outputDirectory = Path.GetDirectoryName(normalizedOutputPath);
                if (string.IsNullOrWhiteSpace(outputDirectory))
                {
                    throw new InvalidOperationException("UnityPackage output directory could not be resolved.");
                }

                Directory.CreateDirectory(outputDirectory);
                AssetDatabase.ExportPackage(StageRoot, normalizedOutputPath, ExportPackageOptions.Recurse);
                UnityEngine.Debug.Log($"Unity Grid Asset Slicer unitypackage exported to: {normalizedOutputPath}");
            }
            finally
            {
                CleanupStageRoot();
            }
        }

        private static void CleanupStageRoot()
        {
            FileUtil.DeleteFileOrDirectory(StageRoot);
            FileUtil.DeleteFileOrDirectory($"{StageRoot}.meta");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private static void CopyDirectoryContents(string sourceRoot, string destinationRoot)
        {
            Directory.CreateDirectory(destinationRoot);

            foreach (var sourceFile in Directory.GetFiles(sourceRoot, "*", SearchOption.AllDirectories))
            {
                if (sourceFile.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var relativePath = Path.GetRelativePath(sourceRoot, sourceFile);
                var destinationFile = Path.Combine(destinationRoot, relativePath);
                var destinationDirectory = Path.GetDirectoryName(destinationFile);
                if (!string.IsNullOrWhiteSpace(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                File.Copy(sourceFile, destinationFile, true);
            }
        }

        private static void CopyFile(string sourceFile, string destinationFile)
        {
            var destinationDirectory = Path.GetDirectoryName(destinationFile);
            if (!string.IsNullOrWhiteSpace(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            File.Copy(sourceFile, destinationFile, true);
        }

        private static string ProjectToAbsolutePath(string projectRelativePath)
        {
            return Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", projectRelativePath.Replace('/', Path.DirectorySeparatorChar)));
        }

        private static string NormalizeUnityPackagePath(string path)
        {
            var normalized = Path.GetFullPath(path);
            return normalized.EndsWith(".unitypackage", StringComparison.OrdinalIgnoreCase)
                ? normalized
                : $"{normalized}.unitypackage";
        }

        private static PackageManifest LoadManifest()
        {
            var json = File.ReadAllText(ProjectToAbsolutePath($"{PackageRoot}/package.json"));
            var manifest = UnityEngine.JsonUtility.FromJson<PackageManifest>(json);
            if (manifest == null || string.IsNullOrWhiteSpace(manifest.version))
            {
                throw new InvalidOperationException("Package manifest version could not be read.");
            }

            return manifest;
        }

        [Serializable]
        private sealed class PackageManifest
        {
            public string version;
        }
    }
}
