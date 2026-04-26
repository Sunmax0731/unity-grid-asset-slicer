using UnityEditor;
using UnityEngine;

namespace Sunmax.GridAssetSlicer.Editor
{
    internal sealed class GridAssetSlicerInfoWindow : EditorWindow
    {
        private const string ProductName = "Unity Grid Asset Slicer";
        private const string PackageId = "com.sunmax0731.grid-asset-slicer";
        private const string Version = "0.1.1";
        private const string ValidatedUnity = "6000.4.0f1";
        private const string RepositoryUrl = "https://github.com/Sunmax0731/unity-grid-asset-slicer";
        private const string License = "MIT License";
        private InfoMode mode;
        private Vector2 scroll;

        public static void OpenLicense()
        {
            var window = GetWindow<GridAssetSlicerInfoWindow>("ライセンス");
            window.mode = InfoMode.License;
            window.minSize = new Vector2(520f, 360f);
            window.Show();
            window.Focus();
        }

        public static void OpenVersionInfo()
        {
            var window = GetWindow<GridAssetSlicerInfoWindow>("バージョン情報");
            window.mode = InfoMode.Version;
            window.minSize = new Vector2(520f, 360f);
            window.Show();
            window.Focus();
        }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            if (mode == InfoMode.License)
            {
                DrawLicense();
            }
            else
            {
                DrawVersionInfo();
            }

            EditorGUILayout.EndScrollView();
        }

        private static void DrawVersionInfo()
        {
            EditorGUILayout.LabelField(ProductName, EditorStyles.boldLabel);
            EditorGUILayout.Space(6f);
            DrawRow("パッケージ", PackageId);
            DrawRow("バージョン", Version);
            DrawRow("検証済み Unity", ValidatedUnity);
            DrawRow("メニュー", "Tools > Grid Asset Slicer > メイン画面");
            DrawRow("ライセンス", License);
            EditorGUILayout.Space(8f);
            EditorGUILayout.TextField("リポジトリ", RepositoryUrl);
        }

        private static void DrawLicense()
        {
            EditorGUILayout.LabelField("ライセンス", EditorStyles.boldLabel);
            EditorGUILayout.Space(6f);
            DrawRow("ライセンス種別", License);
            DrawRow("対象パッケージ", PackageId);
            EditorGUILayout.Space(8f);
            EditorGUILayout.HelpBox(
                "本 Unity エディタ拡張は MIT License で提供されます。利用、改変、再配布、商用利用が可能です。",
                MessageType.None);
            EditorGUILayout.HelpBox(
                "再配布時は、パッケージに含まれる LICENSE.md の著作権表示とライセンス本文を保持してください。",
                MessageType.Info);
            EditorGUILayout.TextField("LICENSE", $"Packages/{PackageId}/LICENSE.md");
        }

        private static void DrawRow(string label, string value)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(130f));
                EditorGUILayout.SelectableLabel(value, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
        }

        private enum InfoMode
        {
            Version,
            License
        }
    }
}
