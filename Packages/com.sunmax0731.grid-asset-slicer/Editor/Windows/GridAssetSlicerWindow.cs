using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sunmax.GridAssetSlicer.Editor.Localization;
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
        private const string QualityPrefsPrefix = "Sunmax.GridAssetSlicer.Quality.";
        private const string LanguageModePrefsKey = "Sunmax.GridAssetSlicer.LanguageMode";
        private const string ParameterHelpPrefsKey = "Sunmax.GridAssetSlicer.ParameterHelp";

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
        private Vector2 _reportScroll;
        private GridCalculationResult _lastGridResult = new GridCalculationResult(Array.Empty<CellRect>(), Array.Empty<string>());
        private PngExportResult _lastExportResult;
        private DetachedPreviewWindow _detachedPreviewWindow;
        private QualityCheckSettings _qualityChecks;
        private GridAssetSlicerLanguageMode _languageMode = GridAssetSlicerLanguageMode.Auto;
        private GridAssetSlicerDisplayLanguage _displayLanguage = GridAssetSlicerDisplayLanguage.English;
        private bool _showParameterHelp;
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
            _qualityChecks = QualityCheckSettings.Load();
            _languageMode = LoadLanguageMode();
            _displayLanguage = GridAssetSlicerLocalization.ResolveLanguage(_languageMode);
            _showParameterHelp = EditorPrefs.GetBool(ParameterHelpPrefsKey, false);
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawToolbarHelp();
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
                EditorGUILayout.LabelField(T("sourceImage", "Source Image"), GUILayout.Width(90f));
                var nextTexture = (Texture2D)EditorGUILayout.ObjectField(_sourceTexture, typeof(Texture2D), false, GUILayout.MinWidth(240f));
                if (nextTexture != _sourceTexture)
                {
                    _sourceTexture = nextTexture;
                    _selectedCell = null;
                    _statusMessage = _sourceTexture == null
                        ? T("status.sourceCleared", "Source image cleared.")
                        : TFormat("status.sourceSelected", "Source image selected: {0}", AssetDatabase.GetAssetPath(_sourceTexture));
                }

                using (new EditorGUI.DisabledScope(_sourceTexture == null))
                {
                    if (GUILayout.Button(T("preview", "Preview"), EditorStyles.toolbarButton, GUILayout.Width(90f)))
                    {
                        OpenDetachedPreviewWindow();
                    }

                    if (GUILayout.Button(T("export", "Export..."), EditorStyles.toolbarButton, GUILayout.Width(90f)))
                    {
                        ExportCells();
                    }
                }

                if (GUILayout.Button(T("saveSession", "Save Session"), EditorStyles.toolbarButton, GUILayout.Width(110f)))
                {
                    SaveSession();
                }

                if (GUILayout.Button(T("loadSession", "Load Session"), EditorStyles.toolbarButton, GUILayout.Width(110f)))
                {
                    LoadSession();
                }

                DrawLanguagePopup();
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(_statusMessage, EditorStyles.miniLabel, GUILayout.MinWidth(180f));
            }
        }

        private void DrawLanguagePopup()
        {
            EditorGUILayout.LabelField(T("language", "Language"), GUILayout.Width(70f));
            var modes = (GridAssetSlicerLanguageMode[])Enum.GetValues(typeof(GridAssetSlicerLanguageMode));
            var labels = modes
                .Select(mode => GridAssetSlicerLocalization.GetLanguageModeLabel(_displayLanguage, mode))
                .ToArray();
            var currentIndex = Math.Max(0, Array.IndexOf(modes, _languageMode));
            var nextIndex = EditorGUILayout.Popup(currentIndex, labels, EditorStyles.toolbarPopup, GUILayout.Width(110f));
            var nextMode = modes[nextIndex];
            if (nextMode == _languageMode)
            {
                return;
            }

            _languageMode = nextMode;
            _displayLanguage = GridAssetSlicerLocalization.ResolveLanguage(_languageMode);
            EditorPrefs.SetString(LanguageModePrefsKey, _languageMode.ToString());
            Repaint();
        }

        private void DrawLeftPane()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(LeftPaneWidth)))
            {
                EditorGUILayout.LabelField(T("gridSettings", "Grid Settings"), EditorStyles.boldLabel);
                _gridSettings.Rows = EditorGUILayout.IntField(T("rows", "Rows"), _gridSettings.Rows);
                DrawParameterHelp("help.rows", "Vertical cell count in the source image.");
                _gridSettings.Columns = EditorGUILayout.IntField(T("columns", "Columns"), _gridSettings.Columns);
                DrawParameterHelp("help.columns", "Horizontal cell count in the source image.");
                _gridSettings.MarginLeft = EditorGUILayout.IntField(T("marginLeft", "Margin Left"), _gridSettings.MarginLeft);
                DrawParameterHelp("help.marginLeft", "Pixels to skip from the left edge before the grid starts.");
                _gridSettings.MarginTop = EditorGUILayout.IntField(T("marginTop", "Margin Top"), _gridSettings.MarginTop);
                DrawParameterHelp("help.marginTop", "Pixels to skip from the top edge before the grid starts.");
                _gridSettings.MarginRight = EditorGUILayout.IntField(T("marginRight", "Margin Right"), _gridSettings.MarginRight);
                DrawParameterHelp("help.marginRight", "Pixels excluded from the right edge after the grid ends.");
                _gridSettings.MarginBottom = EditorGUILayout.IntField(T("marginBottom", "Margin Bottom"), _gridSettings.MarginBottom);
                DrawParameterHelp("help.marginBottom", "Pixels excluded from the bottom edge after the grid ends.");
                _gridSettings.GutterX = EditorGUILayout.IntField(T("gutterX", "Gutter X"), _gridSettings.GutterX);
                DrawParameterHelp("help.gutterX", "Horizontal pixel spacing between neighboring cells.");
                _gridSettings.GutterY = EditorGUILayout.IntField(T("gutterY", "Gutter Y"), _gridSettings.GutterY);
                DrawParameterHelp("help.gutterY", "Vertical pixel spacing between neighboring cells.");
                _gridSettings.CellWidth = DrawNullableInt(T("cellWidth", "Cell Width"), _gridSettings.CellWidth);
                DrawParameterHelp("help.cellWidth", "Explicit cell width. Turn it off to calculate width from the source image, margins, gutters, and column count.");
                _gridSettings.CellHeight = DrawNullableInt(T("cellHeight", "Cell Height"), _gridSettings.CellHeight);
                DrawParameterHelp("help.cellHeight", "Explicit cell height. Turn it off to calculate height from the source image, margins, gutters, and row count.");

                EditorGUILayout.Space(10f);
                EditorGUILayout.LabelField(T("output", "Output"), EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    _exportSettings.OutputFolder = EditorGUILayout.TextField(T("outputFolder", "Output Folder"), _exportSettings.OutputFolder);
                    if (GUILayout.Button("...", GUILayout.Width(28f)))
                    {
                        var selected = EditorUtility.OpenFolderPanel("Output Folder", Application.dataPath, "");
                        if (!string.IsNullOrWhiteSpace(selected))
                        {
                            _exportSettings.OutputFolder = ToProjectRelativePath(selected);
                        }
                    }
                }
                DrawParameterHelp("help.outputFolder", "Project-relative folder where generated PNG files are written.");

                _exportSettings.FilePrefix = EditorGUILayout.TextField(T("outputPrefix", "Output Prefix"), _exportSettings.FilePrefix);
                DrawParameterHelp("help.outputPrefix", "Prefix used before the serial number in each generated file name.");
                _exportSettings.StartIndex = EditorGUILayout.IntField(T("startIndex", "Start Index"), _exportSettings.StartIndex);
                DrawParameterHelp("help.startIndex", "First serial number used when naming exported cells.");
                _exportSettings.NumberPadding = EditorGUILayout.IntField(T("serialDigits", "Serial Digits"), _exportSettings.NumberPadding);
                DrawParameterHelp("help.serialDigits", "Minimum digit count for serial numbers. For example, 3 produces 001.");
                _exportSettings.ConflictBehavior = (ExportConflictBehavior)EditorGUILayout.EnumPopup(T("conflictMode", "Conflict Mode"), _exportSettings.ConflictBehavior);
                DrawParameterHelp("help.conflictMode", "Select how export behaves when a target file already exists: overwrite, skip, or create a duplicate name.");

                EditorGUILayout.Space(10f);
                DrawQualityCheckSettings();

                EditorGUILayout.Space(10f);
                EditorGUILayout.LabelField(T("display", "Display"), EditorStyles.boldLabel);
                var nextShowHelp = EditorGUILayout.Toggle(T("parameterHelp", "Parameter Help"), _showParameterHelp);
                if (nextShowHelp != _showParameterHelp)
                {
                    _showParameterHelp = nextShowHelp;
                    EditorPrefs.SetBool(ParameterHelpPrefsKey, _showParameterHelp);
                }

                DrawParameterHelp("help.parameterHelp", "Shows inline parameter descriptions without blocking edits or preview interaction.");
                EditorGUILayout.HelpBox(T("displayHelp", "The main window keeps settings and inspection controls visible. Use the separate preview window for the grid preview."), MessageType.Info);
            }
        }

        private void DrawQualityCheckSettings()
        {
            EditorGUILayout.LabelField(T("qualityChecks", "Quality Checks"), EditorStyles.boldLabel);
            var nextGridBounds = EditorGUILayout.Toggle(T("gridBounds", "Grid Bounds"), _qualityChecks.GridBounds);
            DrawParameterHelp("help.gridBounds", "Checks whether the calculated grid fits inside the source image.");
            var nextReadableSource = EditorGUILayout.Toggle(T("readableSource", "Readable Source"), _qualityChecks.ReadableSource);
            DrawParameterHelp("help.readableSource", "Checks whether Unity can read source texture pixels for PNG export.");
            var nextOutputSettings = EditorGUILayout.Toggle(T("outputSettings", "Output Settings"), _qualityChecks.OutputSettings);
            DrawParameterHelp("help.outputSettings", "Checks whether export folder and file naming settings can produce valid output paths.");
            var nextIncludedCells = EditorGUILayout.Toggle(T("includedCells", "Included Cells"), _qualityChecks.IncludedCells);
            DrawParameterHelp("help.includedCells", "Checks whether at least one cell is still included for export.");

            if (nextGridBounds != _qualityChecks.GridBounds
                || nextReadableSource != _qualityChecks.ReadableSource
                || nextOutputSettings != _qualityChecks.OutputSettings
                || nextIncludedCells != _qualityChecks.IncludedCells)
            {
                _qualityChecks = new QualityCheckSettings(
                    nextGridBounds,
                    nextReadableSource,
                    nextOutputSettings,
                    nextIncludedCells);
                _qualityChecks.Save();
                Repaint();
            }
        }

        private void DrawToolbarHelp()
        {
            if (!_showParameterHelp)
            {
                return;
            }

            EditorGUILayout.HelpBox(T("help.sourceImage", "Source Image selects the texture asset to preview and export. Language changes the tool UI text."), MessageType.Info);
        }

        private void DrawParameterHelp(string key, string englishText)
        {
            if (!_showParameterHelp)
            {
                return;
            }

            EditorGUILayout.HelpBox(T(key, englishText), MessageType.None);
        }

        private void DrawCenterPane()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.MinWidth(360f), GUILayout.ExpandWidth(true)))
            {
                var sourceName = _sourceTexture == null ? T("noSourceImage", "No source image") : $"{_sourceTexture.name} - {_sourceTexture.width}x{_sourceTexture.height}";
                EditorGUILayout.LabelField(TFormat("previewTitle", "Preview ({0})", sourceName), EditorStyles.boldLabel);
                CalculatePreview();

                EditorGUILayout.HelpBox(T("detachedPreviewHelp", "Preview is shown in a separate resizable window. Open it to inspect cells while editing settings here."), MessageType.Info);
                using (new EditorGUI.DisabledScope(_sourceTexture == null))
                {
                    if (GUILayout.Button(T("openPreviewWindow", "Open Preview Window"), GUILayout.Width(180f)))
                    {
                        OpenDetachedPreviewWindow();
                    }
                }

                if (_sourceTexture != null && _lastGridResult.Errors.Count > 0)
                {
                    foreach (var error in _lastGridResult.Errors)
                    {
                        EditorGUILayout.HelpBox(LocalizeMessage(error), MessageType.Warning);
                    }
                }
            }
        }

        private void DrawPreviewContent(ref Vector2 scroll, float maxHeight)
        {
            CalculatePreview();

            if (_sourceTexture == null)
            {
                EditorGUILayout.HelpBox(T("selectSource", "Select a PNG texture asset to preview cells."), MessageType.Info);
                return;
            }

            if (_lastGridResult.Errors.Count > 0)
            {
                foreach (var error in _lastGridResult.Errors)
                {
                    EditorGUILayout.HelpBox(LocalizeMessage(error), MessageType.Warning);
                }
            }

            if (_lastGridResult.Cells.Count == 0)
            {
                EditorGUILayout.HelpBox(T("noPreviewCells", "No preview cells can be drawn with the current settings."), MessageType.Info);
                return;
            }

            var contentWidth = (_gridSettings.Columns * (CellSize + CellGap)) + CellGap;
            var contentHeight = (_gridSettings.Rows * (CellSize + CellGap)) + CellGap;
            var viewportHeight = Mathf.Min(maxHeight, contentHeight + 24f);
            scroll = EditorGUILayout.BeginScrollView(scroll, GUI.skin.box, GUILayout.Height(viewportHeight), GUILayout.ExpandWidth(true));
            var contentRect = GUILayoutUtility.GetRect(contentWidth, contentHeight);

            foreach (var cell in _lastGridResult.Cells)
            {
                DrawPreviewCell(contentRect, cell);
            }

            EditorGUILayout.EndScrollView();
        }

        private void OpenDetachedPreviewWindow()
        {
            _detachedPreviewWindow = GetWindow<DetachedPreviewWindow>();
            _detachedPreviewWindow.SetOwner(this);
            _detachedPreviewWindow.Show();
        }

        private sealed class DetachedPreviewWindow : EditorWindow
        {
            private GridAssetSlicerWindow _owner;
            private Vector2 _scroll;

            public void SetOwner(GridAssetSlicerWindow owner)
            {
                _owner = owner;
                titleContent = new GUIContent(owner.T("detachedPreviewTitle", "Grid Preview"));
                minSize = new Vector2(360f, 260f);
            }

            private void OnGUI()
            {
                if (_owner == null)
                {
                    var language = GridAssetSlicerLocalization.ResolveLanguage(GridAssetSlicerLanguageMode.Auto);
                    EditorGUILayout.HelpBox(GridAssetSlicerLocalization.Get(language, "detachedPreviewReconnect", "Open Tools > Grid Asset Slicer again to reconnect the preview."), MessageType.Info);
                    return;
                }

                var sourceName = _owner._sourceTexture == null
                    ? _owner.T("noSourceImage", "No source image")
                    : $"{_owner._sourceTexture.name} - {_owner._sourceTexture.width}x{_owner._sourceTexture.height}";
                EditorGUILayout.LabelField(_owner.TFormat("previewTitle", "Preview ({0})", sourceName), EditorStyles.boldLabel);
                _owner.DrawPreviewContent(ref _scroll, Mathf.Max(120f, position.height - 36f));

                if (GUI.changed)
                {
                    _owner.Repaint();
                }
            }

            private void OnInspectorUpdate()
            {
                Repaint();
            }
        }

        private void DrawRightPane()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(RightPaneWidth)))
            {
                EditorGUILayout.LabelField(T("cellInspector", "Cell Inspector"), EditorStyles.boldLabel);
                var selectedRect = GetSelectedRect();
                if (selectedRect == null)
                {
                    EditorGUILayout.HelpBox(T("selectCell", "Select a cell in the preview."), MessageType.Info);
                }
                else
                {
                    var rect = selectedRect.Value;
                    EditorGUILayout.LabelField(T("index", "Index"), GetCellIndex(rect.Coordinate).ToString());
                    EditorGUILayout.LabelField(T("coordinate", "Coordinate"), TFormat("coordinateValue", "Row {0}, Column {1}", rect.Coordinate.Row, rect.Coordinate.Column));
                    var included = !IsExcluded(rect.Coordinate);
                    var nextIncluded = EditorGUILayout.Toggle(T("include", "Include"), included);
                    if (nextIncluded != included)
                    {
                        SetIncluded(rect.Coordinate, nextIncluded);
                    }

                    EditorGUILayout.LabelField(T("bounds", "Bounds"), $"X:{rect.X} Y:{rect.Y} W:{rect.Width} H:{rect.Height}");
                    EditorGUILayout.LabelField(T("outputFile", "Output File"), BuildOutputFileName(rect.Coordinate));
                    DrawSelectedPreview(rect);
                }

                EditorGUILayout.Space(12f);
                EditorGUILayout.LabelField(T("sessionInfo", "Session Info"), EditorStyles.boldLabel);
                EditorGUILayout.LabelField(T("source", "Source"), _sourceTexture == null ? "-" : AssetDatabase.GetAssetPath(_sourceTexture));
                EditorGUILayout.LabelField(T("detectedGrid", "Detected Grid"), $"{_gridSettings.Columns} x {_gridSettings.Rows}");
                EditorGUILayout.LabelField(T("totalCells", "Total Cells"), _lastGridResult.Cells.Count.ToString());
                EditorGUILayout.LabelField(T("included", "Included"), _lastGridResult.Cells.Count(cell => !IsExcluded(cell.Coordinate)).ToString());
                EditorGUILayout.LabelField(T("excluded", "Excluded"), _selection.ExcludedCells.Count.ToString());
            }
        }

        private void DrawQualityReport()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(T("qualityReport", "Quality Check Report"), EditorStyles.boldLabel);
            _reportScroll = EditorGUILayout.BeginScrollView(_reportScroll, GUI.skin.box, GUILayout.Height(110f));
            DrawReportHeader();

            foreach (var check in BuildQualityReport())
            {
                DrawReportRow("-", check.Name, LocalizeReportStatus(check.Status), check.Details);
            }

            if (_lastExportResult != null)
            {
                EditorGUILayout.Space(4f);
                foreach (var exported in _lastExportResult.ExportedFiles)
                {
                    DrawReportRow(GetCellIndex(exported.Cell).ToString(), exported.Cell.ToString(), T("exported", "Exported"), exported.OutputPath);
                }

                foreach (var skipped in _lastExportResult.SkippedFiles)
                {
                    DrawReportRow(GetCellIndex(skipped.Cell).ToString(), skipped.Cell.ToString(), T("skipped", "Skipped"), skipped.OutputPath);
                }

                foreach (var error in _lastExportResult.Errors)
                {
                    DrawReportRow("-", "-", T("error", "Error"), LocalizeMessage(error));
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawReportHeader()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(T("index", "Index"), EditorStyles.boldLabel, GUILayout.Width(60f));
                EditorGUILayout.LabelField(T("cell", "Cell"), EditorStyles.boldLabel, GUILayout.Width(90f));
                EditorGUILayout.LabelField(T("status", "Status"), EditorStyles.boldLabel, GUILayout.Width(90f));
                EditorGUILayout.LabelField(T("details", "Details"), EditorStyles.boldLabel);
            }
        }

        private static void DrawReportRow(string index, string cell, string status, string details)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(index, GUILayout.Width(60f));
                EditorGUILayout.LabelField(cell, GUILayout.Width(90f));
                EditorGUILayout.LabelField(status, GUILayout.Width(90f));
                EditorGUILayout.LabelField(details, EditorStyles.miniLabel);
            }
        }

        private IReadOnlyList<QualityReportEntry> BuildQualityReport()
        {
            var entries = new List<QualityReportEntry>();

            entries.Add(_qualityChecks.GridBounds
                ? _lastGridResult.IsValid
                    ? QualityReportEntry.Pass(T("gridBounds", "Grid Bounds"), T("quality.grid.pass", "Grid fits inside the source image."))
                    : QualityReportEntry.Fail(T("gridBounds", "Grid Bounds"), LocalizeMessages(_lastGridResult.Errors))
                : QualityReportEntry.Disabled(T("gridBounds", "Grid Bounds"), T("quality.disabled", "This quality check is turned off.")));

            entries.Add(_qualityChecks.ReadableSource
                ? IsSourceReadable()
                    ? QualityReportEntry.Pass(T("readableSource", "Readable Source"), T("quality.readable.pass", "Source texture pixels can be read."))
                    : QualityReportEntry.Fail(T("readableSource", "Readable Source"), T("quality.readable.fail", "Source texture is missing or not readable. Export may fail."))
                : QualityReportEntry.Disabled(T("readableSource", "Readable Source"), T("quality.disabled", "This quality check is turned off.")));

            var outputErrors = ExportFileNameResolver.BuildPlan(
                _exportSettings,
                new[] { new CellCoordinate(0, 0) },
                _ => false).Errors;
            entries.Add(_qualityChecks.OutputSettings
                ? outputErrors.Count == 0
                    ? QualityReportEntry.Pass(T("outputSettings", "Output Settings"), T("quality.output.pass", "Output settings are valid."))
                    : QualityReportEntry.Fail(T("outputSettings", "Output Settings"), LocalizeMessages(outputErrors))
                : QualityReportEntry.Disabled(T("outputSettings", "Output Settings"), T("quality.disabled", "This quality check is turned off.")));

            var includedCount = _lastGridResult.Cells.Count(cell => !IsExcluded(cell.Coordinate));
            entries.Add(_qualityChecks.IncludedCells
                ? includedCount > 0
                    ? QualityReportEntry.Pass(T("includedCells", "Included Cells"), TFormat("quality.included.pass", "{0} cell(s) included.", includedCount))
                    : QualityReportEntry.Fail(T("includedCells", "Included Cells"), T("quality.included.fail", "No cells are included for export."))
                : QualityReportEntry.Disabled(T("includedCells", "Included Cells"), T("quality.disabled", "This quality check is turned off.")));

            return entries;
        }

        private bool IsSourceReadable()
        {
            if (_sourceTexture == null)
            {
                return false;
            }

            try
            {
                _sourceTexture.GetPixel(0, 0);
                return true;
            }
            catch (UnityException)
            {
                return false;
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
            if (_lastGridResult.Cells.Count == 0)
            {
                _statusMessage = T("status.exportNoCells", "Export blocked because no cells can be calculated.");
                return;
            }

            var result = PngExporter.Export(new PngExportRequest(_sourceTexture, _lastGridResult.Cells, _selection, _exportSettings));
            _lastExportResult = result;
            AssetDatabaseExportRefresher.RefreshIfExportedUnderAssets(result);
            _statusMessage = result.IsSuccess
                ? TFormat("status.exported", "Exported {0}, skipped {1}.", result.ExportedFiles.Count, result.SkippedFiles.Count)
                : TFormat("status.exportFailed", "Export completed with {0} error(s).", result.Errors.Count);
        }

        private void SaveSession()
        {
            if (_sourceTexture == null)
            {
                _statusMessage = T("status.selectSourceBeforeSave", "Select a source image before saving a session.");
                return;
            }

            var path = EditorUtility.SaveFilePanelInProject(T("dialog.saveSessionTitle", "Save Slice Session"), "slice-session", "json", T("dialog.saveSessionMessage", "Save slice session JSON."));
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            var session = CreateSession();
            File.WriteAllText(path, SliceSessionSerializer.ToJson(session));
            AssetDatabase.Refresh();
            _statusMessage = TFormat("status.savedSession", "Saved session: {0}", path);
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
                _statusMessage = TFormat("status.sessionLoadFailed", "Session load failed: {0}", LocalizeMessages(result.Errors));
                return;
            }

            ApplySession(result.Session);
            _statusMessage = TFormat("status.loadedSession", "Loaded session: {0}", ToProjectRelativePath(absolutePath));
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

        private string LocalizeReportStatus(string status)
        {
            switch (status)
            {
                case "Pass":
                    return T("pass", "Pass");
                case "Fail":
                    return T("fail", "Fail");
                case "Disabled":
                    return T("disabled", "Disabled");
                default:
                    return status;
            }
        }

        private string LocalizeMessages(IEnumerable<string> messages)
        {
            return string.Join("; ", messages.Select(LocalizeMessage));
        }

        private string LocalizeMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return message;
            }

            switch (message)
            {
                case "Grid settings are required.":
                    return T("error.gridSettingsRequired", "Grid settings are required.");
                case "Image width must be greater than zero.":
                    return T("error.imageWidthPositive", "Image width must be greater than zero.");
                case "Image height must be greater than zero.":
                    return T("error.imageHeightPositive", "Image height must be greater than zero.");
                case "Rows must be greater than zero.":
                    return T("error.rowsPositive", "Rows must be greater than zero.");
                case "Columns must be greater than zero.":
                    return T("error.columnsPositive", "Columns must be greater than zero.");
                case "CellWidth must be greater than zero when specified.":
                    return T("error.cellWidthPositive", "Cell Width must be greater than zero when specified.");
                case "CellHeight must be greater than zero when specified.":
                    return T("error.cellHeightPositive", "Cell Height must be greater than zero when specified.");
                case "Available grid width must be greater than zero.":
                    return T("error.availableWidthPositive", "Available grid width must be greater than zero.");
                case "Available grid height must be greater than zero.":
                    return T("error.availableHeightPositive", "Available grid height must be greater than zero.");
                case "Available grid width is not evenly divisible by the cell count.":
                    return T("error.availableWidthDivisible", "Available grid width is not evenly divisible by the cell count.");
                case "Available grid height is not evenly divisible by the cell count.":
                    return T("error.availableHeightDivisible", "Available grid height is not evenly divisible by the cell count.");
                case "OutputFolder is required.":
                    return T("error.outputFolderRequired", "Output Folder is required.");
                case "StartIndex must be zero or greater.":
                    return T("error.startIndexNonNegative", "Start Index must be zero or greater.");
                case "NumberPadding must be zero or greater.":
                    return T("error.numberPaddingNonNegative", "Serial Digits must be zero or greater.");
                case "PNG export request is required.":
                    return T("error.exportRequestRequired", "PNG export request is required.");
                case "Source texture is required.":
                    return T("error.sourceTextureRequired", "Source texture is required.");
            }

            if (message.EndsWith(" must be zero or greater.", StringComparison.Ordinal))
            {
                var fieldName = message.Substring(0, message.Length - " must be zero or greater.".Length);
                return TFormat("error.fieldNonNegative", "{0} must be zero or greater.", LocalizeFieldName(fieldName));
            }

            if (message.StartsWith("Grid width exceeds image bounds.", StringComparison.Ordinal))
            {
                return T("error.gridWidthExceeds", "Grid width exceeds image bounds.");
            }

            if (message.StartsWith("Grid height exceeds image bounds.", StringComparison.Ordinal))
            {
                return T("error.gridHeightExceeds", "Grid height exceeds image bounds.");
            }

            if (message.StartsWith("Export was blocked for cell ", StringComparison.Ordinal))
            {
                var cell = ExtractBetween(message, "Export was blocked for cell ", " at ");
                return TFormat("error.exportBlockedCell", "Export was blocked for cell {0}.", cell);
            }

            if (message.StartsWith("Cell ", StringComparison.Ordinal) && message.Contains(" was not found in the export cell list."))
            {
                var cell = ExtractBetween(message, "Cell ", " was not found");
                return TFormat("error.cellNotFound", "Cell {0} was not found in the export cell list.", cell);
            }

            if (message.StartsWith("Failed to export cell ", StringComparison.Ordinal))
            {
                var cell = ExtractBetween(message, "Failed to export cell ", " to ");
                var path = ExtractBetween(message, " to ", ": ");
                return TFormat("error.failedToExportCell", "Failed to export cell {0} to {1}.", cell, path);
            }

            if (message.StartsWith("source.", StringComparison.Ordinal)
                || message.StartsWith("export.", StringComparison.Ordinal)
                || message.StartsWith("Unsupported formatVersion:", StringComparison.Ordinal))
            {
                return TFormat("error.sessionValidation", "Session validation failed. {0}", message);
            }

            return message;
        }

        private string LocalizeFieldName(string fieldName)
        {
            switch (fieldName)
            {
                case nameof(GridSettings.MarginLeft):
                    return T("marginLeft", "Margin Left");
                case nameof(GridSettings.MarginTop):
                    return T("marginTop", "Margin Top");
                case nameof(GridSettings.MarginRight):
                    return T("marginRight", "Margin Right");
                case nameof(GridSettings.MarginBottom):
                    return T("marginBottom", "Margin Bottom");
                case nameof(GridSettings.GutterX):
                    return T("gutterX", "Gutter X");
                case nameof(GridSettings.GutterY):
                    return T("gutterY", "Gutter Y");
                default:
                    return fieldName;
            }
        }

        private static string ExtractBetween(string text, string prefix, string suffix)
        {
            var start = text.IndexOf(prefix, StringComparison.Ordinal);
            if (start < 0)
            {
                return text;
            }

            start += prefix.Length;
            var end = text.IndexOf(suffix, start, StringComparison.Ordinal);
            return end < 0 ? text.Substring(start) : text.Substring(start, end - start);
        }

        private string T(string key, string englishText)
        {
            return GridAssetSlicerLocalization.Get(_displayLanguage, key, englishText);
        }

        private string TFormat(string key, string englishFormat, params object[] args)
        {
            return GridAssetSlicerLocalization.Format(_displayLanguage, key, englishFormat, args);
        }

        private static GridAssetSlicerLanguageMode LoadLanguageMode()
        {
            var value = EditorPrefs.GetString(LanguageModePrefsKey, GridAssetSlicerLanguageMode.Auto.ToString());
            return Enum.TryParse<GridAssetSlicerLanguageMode>(value, out var mode)
                ? mode
                : GridAssetSlicerLanguageMode.Auto;
        }

        private readonly struct QualityReportEntry
        {
            private QualityReportEntry(string name, string status, string details)
            {
                Name = name;
                Status = status;
                Details = details;
            }

            public string Name { get; }

            public string Status { get; }

            public string Details { get; }

            public static QualityReportEntry Pass(string name, string details)
            {
                return new QualityReportEntry(name, "Pass", details);
            }

            public static QualityReportEntry Fail(string name, string details)
            {
                return new QualityReportEntry(name, "Fail", details);
            }

            public static QualityReportEntry Disabled(string name, string details)
            {
                return new QualityReportEntry(name, "Disabled", details);
            }
        }

        private readonly struct QualityCheckSettings
        {
            public QualityCheckSettings(bool gridBounds, bool readableSource, bool outputSettings, bool includedCells)
            {
                GridBounds = gridBounds;
                ReadableSource = readableSource;
                OutputSettings = outputSettings;
                IncludedCells = includedCells;
            }

            public bool GridBounds { get; }

            public bool ReadableSource { get; }

            public bool OutputSettings { get; }

            public bool IncludedCells { get; }

            public static QualityCheckSettings Load()
            {
                return new QualityCheckSettings(
                    EditorPrefs.GetBool(QualityPrefsPrefix + nameof(GridBounds), true),
                    EditorPrefs.GetBool(QualityPrefsPrefix + nameof(ReadableSource), true),
                    EditorPrefs.GetBool(QualityPrefsPrefix + nameof(OutputSettings), true),
                    EditorPrefs.GetBool(QualityPrefsPrefix + nameof(IncludedCells), true));
            }

            public void Save()
            {
                EditorPrefs.SetBool(QualityPrefsPrefix + nameof(GridBounds), GridBounds);
                EditorPrefs.SetBool(QualityPrefsPrefix + nameof(ReadableSource), ReadableSource);
                EditorPrefs.SetBool(QualityPrefsPrefix + nameof(OutputSettings), OutputSettings);
                EditorPrefs.SetBool(QualityPrefsPrefix + nameof(IncludedCells), IncludedCells);
            }
        }
    }
}
