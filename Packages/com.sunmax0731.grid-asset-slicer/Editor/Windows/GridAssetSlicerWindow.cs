using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Sunmax.GridAssetSlicer.Editor
{
    public sealed class GridAssetSlicerWindow : EditorWindow
    {
        private const string ToolVersion = "0.1.0";
        private const float LeftPaneWidth = 280f;
        private const float RightPaneWidth = 320f;
        private const float CellSize = 72f;
        private const float CellGap = 4f;

        private Texture2D _sourceTexture;
        private GridSettings _gridSettings = new GridSettings
        {
            Rows = 2,
            Columns = 2,
            MarginLeft = 0,
            MarginTop = 0,
            MarginRight = 0,
            MarginBottom = 0,
            GutterX = 0,
            GutterY = 0
        };

        private ExportSettings _exportSettings = new ExportSettings
        {
            OutputFolder = "Assets/Generated/GridSlicer/items",
            FilePrefix = "item_",
            StartIndex = 1,
            NumberPadding = 3,
            ConflictBehavior = ExportConflictBehavior.Duplicate
        };

        private SliceSelection _selection = new SliceSelection();
        private CellCoordinate? _selectedCell;
        private Vector2 _previewScroll;
        private Vector2 _reportScroll;
        private GridCalculationResult _lastGridResult = new GridCalculationResult(Array.Empty<CellRect>(), Array.Empty<string>());
        private PngExportResult _lastExportResult;
        private string _statusMessage = "Ready.";

        [MenuItem("Tools/Grid Asset Slicer")]
        public static void Open()
        {
            var window = GetWindow<GridAssetSlicerWindow>("Grid Asset Slicer");
            window.Show();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Grid Asset Slicer");
            minSize = new Vector2(980, 620);
        }

        private void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.Space(6f);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawLeftPane();
                DrawCenterPane();
                DrawRightPane();
            }

            DrawQualityReport();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUILayout.LabelField("Source Image", GUILayout.Width(90f));
                var nextTexture = (Texture2D)EditorGUILayout.ObjectField(_sourceTexture, typeof(Texture2D), false, GUILayout.MinWidth(240f));
                if (nextTexture != _sourceTexture)
                {
                    _sourceTexture = nextTexture;
                    _selectedCell = null;
                    _statusMessage = _sourceTexture == null ? "Source image cleared." : $"Source image selected: {AssetDatabase.GetAssetPath(_sourceTexture)}";
                }

                using (new EditorGUI.DisabledScope(_sourceTexture == null))
                {
                    if (GUILayout.Button("Preview", EditorStyles.toolbarButton, GUILayout.Width(90f)))
                    {
                        CalculatePreview();
                    }

                    if (GUILayout.Button("Export...", EditorStyles.toolbarButton, GUILayout.Width(90f)))
                    {
                        ExportCells();
                    }
                }

                if (GUILayout.Button("Save Session", EditorStyles.toolbarButton, GUILayout.Width(110f)))
                {
                    SaveSession();
                }

                if (GUILayout.Button("Load Session", EditorStyles.toolbarButton, GUILayout.Width(110f)))
                {
                    LoadSession();
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(_statusMessage, EditorStyles.miniLabel, GUILayout.MinWidth(180f));
            }
        }

        private void DrawLeftPane()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(LeftPaneWidth)))
            {
                EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);
                _gridSettings.Rows = EditorGUILayout.IntField("Rows", _gridSettings.Rows);
                _gridSettings.Columns = EditorGUILayout.IntField("Columns", _gridSettings.Columns);
                _gridSettings.MarginLeft = EditorGUILayout.IntField("Margin Left", _gridSettings.MarginLeft);
                _gridSettings.MarginTop = EditorGUILayout.IntField("Margin Top", _gridSettings.MarginTop);
                _gridSettings.MarginRight = EditorGUILayout.IntField("Margin Right", _gridSettings.MarginRight);
                _gridSettings.MarginBottom = EditorGUILayout.IntField("Margin Bottom", _gridSettings.MarginBottom);
                _gridSettings.GutterX = EditorGUILayout.IntField("Gutter X", _gridSettings.GutterX);
                _gridSettings.GutterY = EditorGUILayout.IntField("Gutter Y", _gridSettings.GutterY);
                _gridSettings.CellWidth = DrawNullableInt("Cell Width", _gridSettings.CellWidth);
                _gridSettings.CellHeight = DrawNullableInt("Cell Height", _gridSettings.CellHeight);

                EditorGUILayout.Space(10f);
                EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    _exportSettings.OutputFolder = EditorGUILayout.TextField("Output Folder", _exportSettings.OutputFolder);
                    if (GUILayout.Button("...", GUILayout.Width(28f)))
                    {
                        var selected = EditorUtility.OpenFolderPanel("Output Folder", Application.dataPath, "");
                        if (!string.IsNullOrWhiteSpace(selected))
                        {
                            _exportSettings.OutputFolder = ToProjectRelativePath(selected);
                        }
                    }
                }

                _exportSettings.FilePrefix = EditorGUILayout.TextField("Output Prefix", _exportSettings.FilePrefix);
                _exportSettings.StartIndex = EditorGUILayout.IntField("Start Index", _exportSettings.StartIndex);
                _exportSettings.NumberPadding = EditorGUILayout.IntField("Serial Digits", _exportSettings.NumberPadding);
                _exportSettings.ConflictBehavior = (ExportConflictBehavior)EditorGUILayout.EnumPopup("Conflict Mode", _exportSettings.ConflictBehavior);

                EditorGUILayout.Space(10f);
                EditorGUILayout.LabelField("Display", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("The preview layout follows docs/Image.png: settings on the left, grid in the center, inspector on the right, quality report below.", MessageType.Info);
            }
        }

        private void DrawCenterPane()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.MinWidth(360f), GUILayout.ExpandWidth(true)))
            {
                var sourceName = _sourceTexture == null ? "No source image" : $"{_sourceTexture.name} - {_sourceTexture.width}x{_sourceTexture.height}";
                EditorGUILayout.LabelField($"Preview ({sourceName})", EditorStyles.boldLabel);
                CalculatePreview();

                if (_lastGridResult.Errors.Count > 0)
                {
                    foreach (var error in _lastGridResult.Errors)
                    {
                        EditorGUILayout.HelpBox(error, MessageType.Error);
                    }

                    return;
                }

                if (_sourceTexture == null)
                {
                    EditorGUILayout.HelpBox("Select a PNG texture asset to preview cells.", MessageType.Info);
                    return;
                }

                var contentWidth = (_gridSettings.Columns * (CellSize + CellGap)) + CellGap;
                var contentHeight = (_gridSettings.Rows * (CellSize + CellGap)) + CellGap;
                _previewScroll = EditorGUILayout.BeginScrollView(_previewScroll, GUI.skin.box, GUILayout.Height(Mathf.Min(430f, contentHeight + 24f)));
                var contentRect = GUILayoutUtility.GetRect(contentWidth, contentHeight);

                foreach (var cell in _lastGridResult.Cells)
                {
                    DrawPreviewCell(contentRect, cell);
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawRightPane()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(RightPaneWidth)))
            {
                EditorGUILayout.LabelField("Cell Inspector", EditorStyles.boldLabel);
                var selectedRect = GetSelectedRect();
                if (selectedRect == null)
                {
                    EditorGUILayout.HelpBox("Select a cell in the preview.", MessageType.Info);
                }
                else
                {
                    var rect = selectedRect.Value;
                    EditorGUILayout.LabelField("Index", GetCellIndex(rect.Coordinate).ToString());
                    EditorGUILayout.LabelField("Coordinate", $"Row {rect.Coordinate.Row}, Column {rect.Coordinate.Column}");
                    var included = !IsExcluded(rect.Coordinate);
                    var nextIncluded = EditorGUILayout.Toggle("Include", included);
                    if (nextIncluded != included)
                    {
                        SetIncluded(rect.Coordinate, nextIncluded);
                    }

                    EditorGUILayout.LabelField("Bounds", $"X:{rect.X} Y:{rect.Y} W:{rect.Width} H:{rect.Height}");
                    EditorGUILayout.LabelField("Output File", BuildOutputFileName(rect.Coordinate));
                    DrawSelectedPreview(rect);
                }

                EditorGUILayout.Space(12f);
                EditorGUILayout.LabelField("Session Info", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Source", _sourceTexture == null ? "-" : AssetDatabase.GetAssetPath(_sourceTexture));
                EditorGUILayout.LabelField("Detected Grid", $"{_gridSettings.Columns} x {_gridSettings.Rows}");
                EditorGUILayout.LabelField("Total Cells", _lastGridResult.Cells.Count.ToString());
                EditorGUILayout.LabelField("Included", _lastGridResult.Cells.Count(cell => !IsExcluded(cell.Coordinate)).ToString());
                EditorGUILayout.LabelField("Excluded", _selection.ExcludedCells.Count.ToString());
            }
        }

        private void DrawQualityReport()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Quality Check Report", EditorStyles.boldLabel);
            _reportScroll = EditorGUILayout.BeginScrollView(_reportScroll, GUI.skin.box, GUILayout.Height(110f));
            DrawReportHeader();

            if (_lastExportResult != null)
            {
                foreach (var exported in _lastExportResult.ExportedFiles)
                {
                    DrawReportRow(GetCellIndex(exported.Cell), exported.Cell, "Exported", exported.OutputPath);
                }

                foreach (var skipped in _lastExportResult.SkippedFiles)
                {
                    DrawReportRow(GetCellIndex(skipped.Cell), skipped.Cell, "Skipped", skipped.OutputPath);
                }

                foreach (var error in _lastExportResult.Errors)
                {
                    DrawReportRow(-1, new CellCoordinate(-1, -1), "Error", error);
                }
            }
            else
            {
                EditorGUILayout.LabelField("No export result yet.", EditorStyles.miniLabel);
            }

            EditorGUILayout.EndScrollView();
        }

        private static void DrawReportHeader()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Index", EditorStyles.boldLabel, GUILayout.Width(60f));
                EditorGUILayout.LabelField("Cell", EditorStyles.boldLabel, GUILayout.Width(90f));
                EditorGUILayout.LabelField("Status", EditorStyles.boldLabel, GUILayout.Width(90f));
                EditorGUILayout.LabelField("Details", EditorStyles.boldLabel);
            }
        }

        private static void DrawReportRow(int index, CellCoordinate cell, string status, string details)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(index >= 0 ? index.ToString() : "-", GUILayout.Width(60f));
                EditorGUILayout.LabelField(cell.Row >= 0 ? cell.ToString() : "-", GUILayout.Width(90f));
                EditorGUILayout.LabelField(status, GUILayout.Width(90f));
                EditorGUILayout.LabelField(details, EditorStyles.miniLabel);
            }
        }

        private void DrawPreviewCell(Rect contentRect, CellRect cell)
        {
            var x = contentRect.x + CellGap + (cell.Coordinate.Column * (CellSize + CellGap));
            var y = contentRect.y + CellGap + (cell.Coordinate.Row * (CellSize + CellGap));
            var drawRect = new Rect(x, y, CellSize, CellSize);
            var uv = new Rect(
                (float)cell.X / _sourceTexture.width,
                1f - ((float)cell.Bottom / _sourceTexture.height),
                (float)cell.Width / _sourceTexture.width,
                (float)cell.Height / _sourceTexture.height);

            GUI.DrawTextureWithTexCoords(drawRect, _sourceTexture, uv, true);
            EditorGUI.DrawRect(new Rect(drawRect.x, drawRect.y, drawRect.width, 1f), Color.gray);
            EditorGUI.DrawRect(new Rect(drawRect.x, drawRect.yMax - 1f, drawRect.width, 1f), Color.gray);
            EditorGUI.DrawRect(new Rect(drawRect.x, drawRect.y, 1f, drawRect.height), Color.gray);
            EditorGUI.DrawRect(new Rect(drawRect.xMax - 1f, drawRect.y, 1f, drawRect.height), Color.gray);

            if (IsExcluded(cell.Coordinate))
            {
                EditorGUI.DrawRect(drawRect, new Color(0f, 0f, 0f, 0.55f));
                GUI.Label(drawRect, "X", CenteredWhiteLabel());
            }

            if (_selectedCell.HasValue && _selectedCell.Value.Equals(cell.Coordinate))
            {
                EditorGUI.DrawRect(new Rect(drawRect.x, drawRect.y, drawRect.width, 2f), Color.cyan);
                EditorGUI.DrawRect(new Rect(drawRect.x, drawRect.yMax - 2f, drawRect.width, 2f), Color.cyan);
                EditorGUI.DrawRect(new Rect(drawRect.x, drawRect.y, 2f, drawRect.height), Color.cyan);
                EditorGUI.DrawRect(new Rect(drawRect.xMax - 2f, drawRect.y, 2f, drawRect.height), Color.cyan);
            }

            GUI.Label(new Rect(drawRect.x + 4f, drawRect.y + 2f, 40f, 18f), GetCellIndex(cell.Coordinate).ToString(), EditorStyles.whiteMiniLabel);

            if (Event.current.type == EventType.MouseDown && drawRect.Contains(Event.current.mousePosition))
            {
                _selectedCell = cell.Coordinate;
                if (Event.current.button == 1)
                {
                    SetIncluded(cell.Coordinate, IsExcluded(cell.Coordinate));
                }

                Event.current.Use();
                Repaint();
            }
        }

        private void DrawSelectedPreview(CellRect rect)
        {
            if (_sourceTexture == null)
            {
                return;
            }

            var previewRect = GUILayoutUtility.GetRect(128f, 128f, GUILayout.Width(128f), GUILayout.Height(128f));
            var uv = new Rect(
                (float)rect.X / _sourceTexture.width,
                1f - ((float)rect.Bottom / _sourceTexture.height),
                (float)rect.Width / _sourceTexture.width,
                (float)rect.Height / _sourceTexture.height);
            GUI.DrawTextureWithTexCoords(previewRect, _sourceTexture, uv, true);
        }

        private void CalculatePreview()
        {
            if (_sourceTexture == null)
            {
                _lastGridResult = new GridCalculationResult(Array.Empty<CellRect>(), Array.Empty<string>());
                return;
            }

            _lastGridResult = GridCalculator.Calculate(_sourceTexture.width, _sourceTexture.height, _gridSettings);
        }

        private void ExportCells()
        {
            CalculatePreview();
            if (!_lastGridResult.IsValid)
            {
                _statusMessage = "Export blocked by invalid grid settings.";
                return;
            }

            var result = PngExporter.Export(new PngExportRequest(_sourceTexture, _lastGridResult.Cells, _selection, _exportSettings));
            _lastExportResult = result;
            AssetDatabaseExportRefresher.RefreshIfExportedUnderAssets(result);
            _statusMessage = result.IsSuccess
                ? $"Exported {result.ExportedFiles.Count}, skipped {result.SkippedFiles.Count}."
                : $"Export failed with {result.Errors.Count} error(s).";
        }

        private void SaveSession()
        {
            if (_sourceTexture == null)
            {
                _statusMessage = "Select a source image before saving a session.";
                return;
            }

            var path = EditorUtility.SaveFilePanelInProject("Save Slice Session", "slice-session", "json", "Save slice session JSON.");
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            var session = CreateSession();
            File.WriteAllText(path, SliceSessionSerializer.ToJson(session));
            AssetDatabase.Refresh();
            _statusMessage = $"Saved session: {path}";
        }

        private void LoadSession()
        {
            var absolutePath = EditorUtility.OpenFilePanel("Load Slice Session", Application.dataPath, "json");
            if (string.IsNullOrWhiteSpace(absolutePath))
            {
                return;
            }

            var result = SliceSessionSerializer.FromJson(File.ReadAllText(absolutePath));
            if (!result.IsValid)
            {
                _statusMessage = $"Session load failed: {string.Join("; ", result.Errors)}";
                return;
            }

            ApplySession(result.Session);
            _statusMessage = $"Loaded session: {ToProjectRelativePath(absolutePath)}";
        }

        private GridSliceSession CreateSession()
        {
            return new GridSliceSession
            {
                FormatVersion = GridSliceSession.CurrentFormatVersion,
                CreatedUtc = DateTime.UtcNow.ToString("o"),
                ToolVersion = ToolVersion,
                Source = new SourceImageReference
                {
                    AssetPath = AssetDatabase.GetAssetPath(_sourceTexture),
                    Width = _sourceTexture.width,
                    Height = _sourceTexture.height
                },
                Grid = _gridSettings,
                Selection = _selection,
                Export = _exportSettings
            };
        }

        private void ApplySession(GridSliceSession session)
        {
            _sourceTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(session.Source.AssetPath);
            _gridSettings = session.Grid;
            _selection = session.Selection ?? new SliceSelection();
            _exportSettings = session.Export;
            _selectedCell = null;
            CalculatePreview();
        }

        private static int? DrawNullableInt(string label, int? value)
        {
            var enabled = value.HasValue;
            using (new EditorGUILayout.HorizontalScope())
            {
                enabled = EditorGUILayout.Toggle(label, enabled);
                using (new EditorGUI.DisabledScope(!enabled))
                {
                    var nextValue = EditorGUILayout.IntField(value ?? 0);
                    return enabled ? nextValue : (int?)null;
                }
            }
        }

        private CellRect? GetSelectedRect()
        {
            if (!_selectedCell.HasValue)
            {
                return null;
            }

            foreach (var cell in _lastGridResult.Cells)
            {
                if (cell.Coordinate.Equals(_selectedCell.Value))
                {
                    return cell;
                }
            }

            return null;
        }

        private bool IsExcluded(CellCoordinate coordinate)
        {
            return _selection.ExcludedCells.Contains(coordinate);
        }

        private void SetIncluded(CellCoordinate coordinate, bool included)
        {
            var existingIndex = _selection.ExcludedCells.FindIndex(cell => cell.Equals(coordinate));
            if (included && existingIndex >= 0)
            {
                _selection.ExcludedCells.RemoveAt(existingIndex);
            }
            else if (!included && existingIndex < 0)
            {
                _selection.ExcludedCells.Add(coordinate);
            }
        }

        private int GetCellIndex(CellCoordinate coordinate)
        {
            return (coordinate.Row * _gridSettings.Columns) + coordinate.Column + _exportSettings.StartIndex;
        }

        private string BuildOutputFileName(CellCoordinate coordinate)
        {
            var sequence = GetCellIndex(coordinate);
            var number = _exportSettings.NumberPadding > 0
                ? sequence.ToString("D" + _exportSettings.NumberPadding)
                : sequence.ToString();
            return $"{_exportSettings.FilePrefix}{number}.png";
        }

        private static string ToProjectRelativePath(string absolutePath)
        {
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (projectRoot == null)
            {
                return absolutePath.Replace('\\', '/');
            }

            var normalizedRoot = projectRoot.Replace('\\', '/').TrimEnd('/');
            var normalizedPath = absolutePath.Replace('\\', '/');
            return normalizedPath.StartsWith(normalizedRoot + "/", StringComparison.OrdinalIgnoreCase)
                ? normalizedPath.Substring(normalizedRoot.Length + 1)
                : normalizedPath;
        }

        private static GUIStyle CenteredWhiteLabel()
        {
            return new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
        }
    }
}
