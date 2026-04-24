using UnityEditor;

namespace Sunmax.GridAssetSlicer.Editor
{
    public static class AssetDatabaseExportRefresher
    {
        public static void RefreshIfExportedUnderAssets(PngExportResult result)
        {
            if (result == null || result.ExportedFiles.Count == 0)
            {
                return;
            }

            foreach (var exportedFile in result.ExportedFiles)
            {
                if (exportedFile.OutputPath.StartsWith("Assets/"))
                {
                    AssetDatabase.Refresh();
                    return;
                }
            }
        }
    }
}
