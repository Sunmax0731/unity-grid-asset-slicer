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
        private const string ToolVersion = "0.1.2";
        private const float LeftPaneWidth = 460f;
        private const float PaneGap = 18f;
        private const float CellSize = 72f;
        private const float CellGap = 4f;
        private const float MarginControllerMaxHeight = 340f;
        private const float MarginControllerLineThickness = 2f;
        private const float MarginControllerHitPadding = 8f;
        private const string QualityPrefsPrefix = "Sunmax.GridAssetSlicer.Quality.";
        private const string LanguageModePrefsKey = "Sunmax.GridAssetSlicer.LanguageMode";
        private const string InspectorOutlinePrefsKey = "Sunmax.GridAssetSlicer.InspectorOutline";
        private const string ReportHeightPrefsKey = "Sunmax.GridAssetSlicer.ReportHeight";
        private const float DefaultReportHeight = 140f;
        private const float MinReportHeight = 90f;
        private const float MaxReportHeight = 320f;

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
            OutputWidth = null,
            OutputHeight = null,
            ConflictBehavior = ExportConflictBehavior.Duplicate
        };

        private SliceSelection _selection = new SliceSelection();
        private CellCoordinate? _selectedCell;
        private Vector2 _settingsScroll;
        private Vector2 _reportScroll;
        private GridCalculationResult _lastGridResult = new GridCalculationResult(Array.Empty<CellRect>(), Array.Empty<string>());
        private PngExportResult _lastExportResult;
        private DetachedPreviewWindow _detachedPreviewWindow;
        private ParameterHelpWindow _parameterHelpWindow;
        private QualityCheckSettings _qualityChecks;
        private GridAssetSlicerLanguageMode _languageMode = GridAssetSlicerLanguageMode.Auto;
        private GridAssetSlicerDisplayLanguage _displayLanguage = GridAssetSlicerDisplayLanguage.English;
        private bool _showInspectorPreviewOutline = true;
        private Color _inspectorPreviewBackground = new Color(0.18f, 0.18f, 0.18f, 1f);
        private Color _inspectorPreviewOutlineColor = Color.cyan;
        private float _reportHeight = DefaultReportHeight;
        private string _statusMessage = "Ready.";
        private MarginDragHandle _activeMarginDragHandle;
        private bool _showGridSettings = true;
        private bool _showMarginController = true;
        private bool _showVariableGrid = true;
        private bool _showOutputSettings = true;
        private bool _showCellInspector = true;
        private bool _showQualityChecks = false;

        [MenuItem("Tools/Grid Asset Slicer/メイン画面")]
        public static void Open()
        {
            var window = GetWindow<GridAssetSlicerWindow>("Grid Asset Slicer");
            window.Show();
        }

        [MenuItem("Tools/Grid Asset Slicer/ライセンス")]
        public static void OpenLicense()
        {
            GridAssetSlicerInfoWindow.OpenLicense();
        }

        [MenuItem("Tools/Grid Asset Slicer/バージョン情報")]
        public static void OpenVersionInfo()
        {
            GridAssetSlicerInfoWindow.OpenVersionInfo();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Grid Asset Slicer");
            minSize = new Vector2(980, 620);
            _qualityChecks = QualityCheckSettings.Load();
            _languageMode = LoadLanguageMode();
            _displayLanguage = GridAssetSlicerLocalization.ResolveLanguage(_languageMode);
            _showInspectorPreviewOutline = EditorPrefs.GetBool(InspectorOutlinePrefsKey, true);
            _reportHeight = Mathf.Clamp(EditorPrefs.GetFloat(ReportHeightPrefsKey, DefaultReportHeight), MinReportHeight, MaxReportHeight);
        }

        private void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.Space(6f);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawLeftPane();
                DrawPaneSeparator();
                GUILayout.Space(PaneGap);
                DrawCenterPane();
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

                if (GUILayout.Button(T("help", "Help"), EditorStyles.toolbarButton, GUILayout.Width(80f)))
                {
                    OpenParameterHelpWindow();
                }

                DrawLanguagePopup();
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(_statusMessage, EditorStyles.miniLabel, GUILayout.MinWidth(180f));
            }
        }

        private void DrawLanguagePopup()
        {
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
                _settingsScroll = EditorGUILayout.BeginScrollView(_settingsScroll, GUI.skin.box);
                DrawFoldoutSection(ref _showGridSettings, T("gridSettings", "Grid Settings"), () =>
                {
                    _gridSettings.Rows = DrawIntFieldWithSlider(T("rows", "Rows"), _gridSettings.Rows, 1, GetCountSliderMax(_gridSettings.Rows));
                    _gridSettings.Columns = DrawIntFieldWithSlider(T("columns", "Columns"), _gridSettings.Columns, 1, GetCountSliderMax(_gridSettings.Columns));
                    _gridSettings.MarginLeft = DrawIntFieldWithSlider(T("marginLeft", "Margin Left"), _gridSettings.MarginLeft, 0, GetPixelSliderMax(_gridSettings.MarginLeft, GetSourceWidth()));
                    _gridSettings.MarginTop = DrawIntFieldWithSlider(T("marginTop", "Margin Top"), _gridSettings.MarginTop, 0, GetPixelSliderMax(_gridSettings.MarginTop, GetSourceHeight()));
                    _gridSettings.MarginRight = DrawIntFieldWithSlider(T("marginRight", "Margin Right"), _gridSettings.MarginRight, 0, GetPixelSliderMax(_gridSettings.MarginRight, GetSourceWidth()));
                    _gridSettings.MarginBottom = DrawIntFieldWithSlider(T("marginBottom", "Margin Bottom"), _gridSettings.MarginBottom, 0, GetPixelSliderMax(_gridSettings.MarginBottom, GetSourceHeight()));
                    _gridSettings.GutterX = DrawIntFieldWithSlider(T("gutterX", "Gutter X"), _gridSettings.GutterX, 0, GetPixelSliderMax(_gridSettings.GutterX, GetSourceWidth()));
                    _gridSettings.GutterY = DrawIntFieldWithSlider(T("gutterY", "Gutter Y"), _gridSettings.GutterY, 0, GetPixelSliderMax(_gridSettings.GutterY, GetSourceHeight()));
                    _gridSettings.CellWidth = DrawNullableIntWithSlider(T("cellWidth", "Cell Width"), _gridSettings.CellWidth, 1, GetPixelSliderMax(_gridSettings.CellWidth ?? 1, GetSourceWidth()));
                    _gridSettings.CellHeight = DrawNullableIntWithSlider(T("cellHeight", "Cell Height"), _gridSettings.CellHeight, 1, GetPixelSliderMax(_gridSettings.CellHeight ?? 1, GetSourceHeight()));
                });

                NormalizeCustomGridArrays();
                DrawFoldoutSection(ref _showMarginController, T("marginController", "Margin Controller"), DrawMarginController);
                DrawFoldoutSection(ref _showVariableGrid, T("variableGrid", "Variable Grid Boundaries"), DrawVariableGridSettings);
                DrawFoldoutSection(ref _showOutputSettings, T("output", "Output"), DrawOutputSettings);
                DrawFoldoutSection(ref _showCellInspector, T("cellInspector", "Cell Inspector"), DrawInspectorPaneContent);
                DrawFoldoutSection(ref _showQualityChecks, T("qualityChecks", "Quality Checks"), DrawQualityCheckSettings);
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawQualityCheckSettings()
        {
            var nextGridBounds = EditorGUILayout.Toggle(T("gridBounds", "Grid Bounds"), _qualityChecks.GridBounds);
            var nextReadableSource = EditorGUILayout.Toggle(T("readableSource", "Readable Source"), _qualityChecks.ReadableSource);
            var nextOutputSettings = EditorGUILayout.Toggle(T("outputSettings", "Output Settings"), _qualityChecks.OutputSettings);
            var nextIncludedCells = EditorGUILayout.Toggle(T("includedCells", "Included Cells"), _qualityChecks.IncludedCells);

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

        private void DrawMarginController()
        {
            EditorGUILayout.HelpBox(T("marginControllerHelp", "Drag the guide lines on this source-image controller to adjust the margins without opening the preview window."), MessageType.None);

            if (_sourceTexture == null)
            {
                EditorGUILayout.HelpBox(T("marginControllerNoSource", "Select a source image to enable the margin controller."), MessageType.Info);
                return;
            }

            var aspect = Mathf.Max(0.01f, (float)_sourceTexture.width / _sourceTexture.height);
            var controlRect = GUILayoutUtility.GetAspectRect(aspect, GUILayout.ExpandWidth(true), GUILayout.MaxHeight(MarginControllerMaxHeight));
            GUI.DrawTexture(controlRect, _sourceTexture, ScaleMode.ScaleToFit, true);
            DrawMarginControllerOverlay(controlRect);
            HandleMarginControllerInput(controlRect);

            EditorGUILayout.LabelField(
                TFormat(
                    "marginControllerValues",
                    "L {0} / T {1} / R {2} / B {3}",
                    _gridSettings.MarginLeft,
                    _gridSettings.MarginTop,
                    _gridSettings.MarginRight,
                    _gridSettings.MarginBottom),
                EditorStyles.miniLabel);
        }

        private void DrawVariableGridSettings()
        {
            EditorGUILayout.HelpBox(T("variableGridHelp", "Enable custom column widths or row heights when each grid segment should use a different boundary position."), MessageType.None);

            var customColumns = _gridSettings.ColumnWidths != null && _gridSettings.ColumnWidths.Length == _gridSettings.Columns;
            var nextCustomColumns = EditorGUILayout.Toggle(T("customColumns", "Custom Columns"), customColumns);
            if (nextCustomColumns != customColumns)
            {
                _gridSettings.ColumnWidths = nextCustomColumns ? BuildDefaultCustomSizes(true) : null;
            }

            if (_gridSettings.ColumnWidths != null)
            {
                DrawBoundaryEditors(true);
            }

            EditorGUILayout.Space(4f);

            var customRows = _gridSettings.RowHeights != null && _gridSettings.RowHeights.Length == _gridSettings.Rows;
            var nextCustomRows = EditorGUILayout.Toggle(T("customRows", "Custom Rows"), customRows);
            if (nextCustomRows != customRows)
            {
                _gridSettings.RowHeights = nextCustomRows ? BuildDefaultCustomSizes(false) : null;
            }

            if (_gridSettings.RowHeights != null)
            {
                DrawBoundaryEditors(false);
            }
        }

        private void DrawBoundaryEditors(bool columns)
        {
            var sizes = columns ? _gridSettings.ColumnWidths : _gridSettings.RowHeights;
            var count = columns ? _gridSettings.Columns : _gridSettings.Rows;
            var sourceSize = columns ? GetSourceWidth() : GetSourceHeight();
            var gutter = columns ? _gridSettings.GutterX : _gridSettings.GutterY;
            var labelPrefix = columns ? T("columnBoundary", "Column Boundary") : T("rowBoundary", "Row Boundary");
            var summaryPrefix = columns ? T("columnWidth", "Column Width") : T("rowHeight", "Row Height");

            if (sizes == null || count <= 0)
            {
                return;
            }

            if (sourceSize <= 0)
            {
                if (columns)
                {
                    _gridSettings.MarginLeft = DrawIntFieldWithSlider(T("marginLeft", "Margin Left"), _gridSettings.MarginLeft, 0, GetPixelSliderMax(_gridSettings.MarginLeft, 0));
                    _gridSettings.MarginRight = DrawIntFieldWithSlider(T("marginRight", "Margin Right"), _gridSettings.MarginRight, 0, GetPixelSliderMax(_gridSettings.MarginRight, 0));
                }
                else
                {
                    _gridSettings.MarginTop = DrawIntFieldWithSlider(T("marginTop", "Margin Top"), _gridSettings.MarginTop, 0, GetPixelSliderMax(_gridSettings.MarginTop, 0));
                    _gridSettings.MarginBottom = DrawIntFieldWithSlider(T("marginBottom", "Margin Bottom"), _gridSettings.MarginBottom, 0, GetPixelSliderMax(_gridSettings.MarginBottom, 0));
                }

                for (var index = 0; index < sizes.Length; index++)
                {
                    sizes[index] = DrawIntFieldWithSlider($"{summaryPrefix} {index + 1}", sizes[index], 1, GetPixelSliderMax(sizes[index], 0));
                }

                return;
            }

            if (columns)
            {
                DrawCustomMarginSlider(
                    T("marginLeft", "Margin Left"),
                    _gridSettings.MarginLeft,
                    0,
                    GetCustomMarginMax(sourceSize, _gridSettings.MarginRight, gutter, sizes, true),
                    nextValue =>
                    {
                        var margin = _gridSettings.MarginLeft;
                        AdjustLeadingMargin(ref margin, sizes, nextValue);
                        _gridSettings.MarginLeft = margin;
                    });
                DrawCustomMarginSlider(
                    T("marginRight", "Margin Right"),
                    _gridSettings.MarginRight,
                    0,
                    GetCustomMarginMax(sourceSize, _gridSettings.MarginLeft, gutter, sizes, false),
                    nextValue =>
                    {
                        var margin = _gridSettings.MarginRight;
                        AdjustTrailingMargin(ref margin, sizes, nextValue);
                        _gridSettings.MarginRight = margin;
                    });
            }
            else
            {
                DrawCustomMarginSlider(
                    T("marginTop", "Margin Top"),
                    _gridSettings.MarginTop,
                    0,
                    GetCustomMarginMax(sourceSize, _gridSettings.MarginBottom, gutter, sizes, true),
                    nextValue =>
                    {
                        var margin = _gridSettings.MarginTop;
                        AdjustLeadingMargin(ref margin, sizes, nextValue);
                        _gridSettings.MarginTop = margin;
                    });
                DrawCustomMarginSlider(
                    T("marginBottom", "Margin Bottom"),
                    _gridSettings.MarginBottom,
                    0,
                    GetCustomMarginMax(sourceSize, _gridSettings.MarginTop, gutter, sizes, false),
                    nextValue =>
                    {
                        var margin = _gridSettings.MarginBottom;
                        AdjustTrailingMargin(ref margin, sizes, nextValue);
                        _gridSettings.MarginBottom = margin;
                    });
            }

            var leadingMargin = columns ? _gridSettings.MarginLeft : _gridSettings.MarginTop;
            var trailingMargin = columns ? _gridSettings.MarginRight : _gridSettings.MarginBottom;

            for (var index = 0; index < count - 1; index++)
            {
                var minBoundary = GetBoundaryMinimum(sizes, leadingMargin, gutter, index);
                var maxBoundary = GetBoundaryMaximum(sizes, sourceSize, trailingMargin, gutter, index);
                var boundary = GetBoundaryPosition(sizes, leadingMargin, gutter, index);
                var clampedBoundary = DrawIntFieldWithSlider($"{labelPrefix} {index + 1}", boundary, minBoundary, maxBoundary);
                if (clampedBoundary != boundary)
                {
                    SetBoundaryPosition(sizes, leadingMargin, gutter, index, clampedBoundary);
                }
            }

            EditorGUILayout.LabelField(
                string.Join(", ", sizes.Select((size, index) => $"{summaryPrefix} {index + 1}: {size}")),
                EditorStyles.miniLabel);
        }

        private int[] BuildDefaultCustomSizes(bool columns)
        {
            var settings = new GridSettings
            {
                Rows = _gridSettings.Rows,
                Columns = _gridSettings.Columns,
                MarginLeft = _gridSettings.MarginLeft,
                MarginTop = _gridSettings.MarginTop,
                MarginRight = _gridSettings.MarginRight,
                MarginBottom = _gridSettings.MarginBottom,
                GutterX = _gridSettings.GutterX,
                GutterY = _gridSettings.GutterY,
                CellWidth = _gridSettings.CellWidth,
                CellHeight = _gridSettings.CellHeight
            };

            var result = GridCalculator.Calculate(GetSourceWidth(), GetSourceHeight(), settings);
            if (result.Cells.Count > 0)
            {
                return columns
                    ? result.Cells.Where(cell => cell.Coordinate.Row == 0).Select(cell => cell.Width).ToArray()
                    : result.Cells.Where(cell => cell.Coordinate.Column == 0).Select(cell => cell.Height).ToArray();
            }

            var count = columns ? Mathf.Max(1, _gridSettings.Columns) : Mathf.Max(1, _gridSettings.Rows);
            var fallback = new int[count];
            for (var index = 0; index < count; index++)
            {
                fallback[index] = 32;
            }

            return fallback;
        }

        private void NormalizeCustomGridArrays()
        {
            if (_gridSettings.ColumnWidths != null && _gridSettings.ColumnWidths.Length != _gridSettings.Columns)
            {
                _gridSettings.ColumnWidths = BuildDefaultCustomSizes(true);
            }

            if (_gridSettings.RowHeights != null && _gridSettings.RowHeights.Length != _gridSettings.Rows)
            {
                _gridSettings.RowHeights = BuildDefaultCustomSizes(false);
            }
        }

        private static int GetBoundaryPosition(IReadOnlyList<int> sizes, int leadingMargin, int gutter, int boundaryIndex)
        {
            var position = leadingMargin;
            for (var index = 0; index <= boundaryIndex; index++)
            {
                position += sizes[index];
                if (index < boundaryIndex)
                {
                    position += gutter;
                }
            }

            return position;
        }

        private static int GetBoundaryMinimum(IReadOnlyList<int> sizes, int leadingMargin, int gutter, int boundaryIndex)
        {
            var minimum = leadingMargin;
            for (var index = 0; index < boundaryIndex; index++)
            {
                minimum += sizes[index] + gutter;
            }

            return minimum + 1;
        }

        private static int GetBoundaryMaximum(IReadOnlyList<int> sizes, int sourceSize, int trailingMargin, int gutter, int boundaryIndex)
        {
            var remaining = trailingMargin;
            for (var index = sizes.Count - 1; index > boundaryIndex + 1; index--)
            {
                remaining += sizes[index] + gutter;
            }

            remaining += sizes[boundaryIndex + 1] > 0 ? 1 : 0;
            return sourceSize - remaining - gutter;
        }

        private void DrawCustomMarginSlider(string label, int value, int min, int max, Action<int> applyValue)
        {
            var nextValue = DrawIntFieldWithSlider(label, value, min, Mathf.Max(min, max));
            if (nextValue != value)
            {
                applyValue(nextValue);
            }
        }

        private static int GetCustomMarginMax(int sourceSize, int oppositeMargin, int gutter, IReadOnlyList<int> sizes, bool leading)
        {
            var otherCells = 0;
            for (var index = 0; index < sizes.Count; index++)
            {
                var isAdjustableEdge = leading ? index == 0 : index == sizes.Count - 1;
                if (!isAdjustableEdge)
                {
                    otherCells += sizes[index];
                }
            }

            var minEdgeSize = 1;
            var gutters = Mathf.Max(0, sizes.Count - 1) * gutter;
            return Mathf.Max(0, sourceSize - oppositeMargin - gutters - otherCells - minEdgeSize);
        }

        private static void AdjustLeadingMargin(ref int margin, IList<int> sizes, int nextMargin)
        {
            var delta = nextMargin - margin;
            margin = nextMargin;
            sizes[0] = Mathf.Max(1, sizes[0] - delta);
        }

        private static void AdjustTrailingMargin(ref int margin, IList<int> sizes, int nextMargin)
        {
            var delta = nextMargin - margin;
            margin = nextMargin;
            var lastIndex = sizes.Count - 1;
            sizes[lastIndex] = Mathf.Max(1, sizes[lastIndex] - delta);
        }

        private static int Sum(IReadOnlyList<int> values)
        {
            var total = 0;
            for (var index = 0; index < values.Count; index++)
            {
                total += values[index];
            }

            return total;
        }

        private static void SetBoundaryPosition(IList<int> sizes, int leadingMargin, int gutter, int boundaryIndex, int boundaryPosition)
        {
            var previousBoundary = leadingMargin;
            for (var index = 0; index < boundaryIndex; index++)
            {
                previousBoundary += sizes[index] + gutter;
            }

            var nextBoundary = previousBoundary + sizes[boundaryIndex] + gutter + sizes[boundaryIndex + 1];
            sizes[boundaryIndex] = Mathf.Max(1, boundaryPosition - previousBoundary);
            sizes[boundaryIndex + 1] = Mathf.Max(1, nextBoundary - gutter - boundaryPosition);
        }

        private void DrawMarginControllerOverlay(Rect rect)
        {
            var left = GetMarginGuideX(rect, true);
            var right = GetMarginGuideX(rect, false);
            var top = GetMarginGuideY(rect, true);
            var bottom = GetMarginGuideY(rect, false);

            EditorGUI.DrawRect(new Rect(rect.x, rect.y, Mathf.Max(0f, left - rect.x), rect.height), new Color(0f, 0f, 0f, 0.2f));
            EditorGUI.DrawRect(new Rect(right, rect.y, Mathf.Max(0f, rect.xMax - right), rect.height), new Color(0f, 0f, 0f, 0.2f));
            EditorGUI.DrawRect(new Rect(left, rect.y, Mathf.Max(0f, right - left), Mathf.Max(0f, top - rect.y)), new Color(0f, 0f, 0f, 0.2f));
            EditorGUI.DrawRect(new Rect(left, bottom, Mathf.Max(0f, right - left), Mathf.Max(0f, rect.yMax - bottom)), new Color(0f, 0f, 0f, 0.2f));

            var handleColor = new Color(0.12f, 0.78f, 1f, 0.95f);
            EditorGUI.DrawRect(new Rect(left - (MarginControllerLineThickness * 0.5f), rect.y, MarginControllerLineThickness, rect.height), handleColor);
            EditorGUI.DrawRect(new Rect(right - (MarginControllerLineThickness * 0.5f), rect.y, MarginControllerLineThickness, rect.height), handleColor);
            EditorGUI.DrawRect(new Rect(rect.x, top - (MarginControllerLineThickness * 0.5f), rect.width, MarginControllerLineThickness), handleColor);
            EditorGUI.DrawRect(new Rect(rect.x, bottom - (MarginControllerLineThickness * 0.5f), rect.width, MarginControllerLineThickness), handleColor);
            DrawOutline(rect, handleColor, 1f);
        }

        private void HandleMarginControllerInput(Rect rect)
        {
            var currentEvent = Event.current;
            switch (currentEvent.type)
            {
                case EventType.MouseDown:
                    if (currentEvent.button == 0 && rect.Contains(currentEvent.mousePosition))
                    {
                        _activeMarginDragHandle = GetMarginDragHandle(rect, currentEvent.mousePosition);
                        if (_activeMarginDragHandle != MarginDragHandle.None)
                        {
                            UpdateMarginFromMouse(rect, currentEvent.mousePosition);
                            currentEvent.Use();
                        }
                    }

                    break;

                case EventType.MouseDrag:
                    if (_activeMarginDragHandle != MarginDragHandle.None)
                    {
                        UpdateMarginFromMouse(rect, currentEvent.mousePosition);
                        currentEvent.Use();
                        Repaint();
                    }

                    break;

                case EventType.MouseUp:
                    if (_activeMarginDragHandle != MarginDragHandle.None)
                    {
                        UpdateMarginFromMouse(rect, currentEvent.mousePosition);
                        _activeMarginDragHandle = MarginDragHandle.None;
                        currentEvent.Use();
                        Repaint();
                    }

                    break;
            }
        }

        private MarginDragHandle GetMarginDragHandle(Rect rect, Vector2 mousePosition)
        {
            var leftDistance = Mathf.Abs(mousePosition.x - GetMarginGuideX(rect, true));
            var rightDistance = Mathf.Abs(mousePosition.x - GetMarginGuideX(rect, false));
            var topDistance = Mathf.Abs(mousePosition.y - GetMarginGuideY(rect, true));
            var bottomDistance = Mathf.Abs(mousePosition.y - GetMarginGuideY(rect, false));
            var nearest = MarginControllerHitPadding;
            var handle = MarginDragHandle.None;

            if (leftDistance <= nearest)
            {
                nearest = leftDistance;
                handle = MarginDragHandle.Left;
            }

            if (rightDistance <= nearest)
            {
                nearest = rightDistance;
                handle = MarginDragHandle.Right;
            }

            if (topDistance <= nearest)
            {
                nearest = topDistance;
                handle = MarginDragHandle.Top;
            }

            if (bottomDistance <= nearest)
            {
                handle = MarginDragHandle.Bottom;
            }

            return handle;
        }

        private void UpdateMarginFromMouse(Rect rect, Vector2 mousePosition)
        {
            var sourceWidth = GetSourceWidth();
            var sourceHeight = GetSourceHeight();
            if (sourceWidth <= 0 || sourceHeight <= 0)
            {
                return;
            }

            var localX = Mathf.Clamp(mousePosition.x, rect.x, rect.xMax) - rect.x;
            var localY = Mathf.Clamp(mousePosition.y, rect.y, rect.yMax) - rect.y;
            var xPixels = Mathf.RoundToInt((localX / rect.width) * sourceWidth);
            var yPixels = Mathf.RoundToInt((localY / rect.height) * sourceHeight);

            switch (_activeMarginDragHandle)
            {
                case MarginDragHandle.Left:
                    _gridSettings.MarginLeft = Mathf.Clamp(xPixels, 0, sourceWidth - _gridSettings.MarginRight);
                    break;
                case MarginDragHandle.Top:
                    _gridSettings.MarginTop = Mathf.Clamp(yPixels, 0, sourceHeight - _gridSettings.MarginBottom);
                    break;
                case MarginDragHandle.Right:
                    _gridSettings.MarginRight = Mathf.Clamp(sourceWidth - xPixels, 0, sourceWidth - _gridSettings.MarginLeft);
                    break;
                case MarginDragHandle.Bottom:
                    _gridSettings.MarginBottom = Mathf.Clamp(sourceHeight - yPixels, 0, sourceHeight - _gridSettings.MarginTop);
                    break;
            }
        }

        private float GetMarginGuideX(Rect rect, bool leading)
        {
            var sourceWidth = Mathf.Max(1, GetSourceWidth());
            var pixels = leading ? _gridSettings.MarginLeft : sourceWidth - _gridSettings.MarginRight;
            return rect.x + ((float)pixels / sourceWidth * rect.width);
        }

        private float GetMarginGuideY(Rect rect, bool leading)
        {
            var sourceHeight = Mathf.Max(1, GetSourceHeight());
            var pixels = leading ? _gridSettings.MarginTop : sourceHeight - _gridSettings.MarginBottom;
            return rect.y + ((float)pixels / sourceHeight * rect.height);
        }

        private int DrawIntFieldWithSlider(string label, int value, int min, int max)
        {
            var headerRect = EditorGUILayout.GetControlRect();
            var labelRect = new Rect(headerRect.x, headerRect.y, 140f, headerRect.height);
            var fieldRect = new Rect(headerRect.xMax - 72f, headerRect.y, 72f, headerRect.height);
            EditorGUI.LabelField(labelRect, label);
            value = EditorGUI.IntField(fieldRect, value);

            var sliderRect = EditorGUILayout.GetControlRect();
            sliderRect.xMin += 18f;
            var sliderValue = GUI.HorizontalSlider(sliderRect, value, min, max);
            return Mathf.Clamp(Mathf.RoundToInt(sliderValue), min, max);
        }

        private int? DrawNullableIntWithSlider(string label, int? value, int min, int max)
        {
            var enabled = value.HasValue;
            var headerRect = EditorGUILayout.GetControlRect();
            var toggleRect = new Rect(headerRect.x, headerRect.y, 16f, headerRect.height);
            var labelRect = new Rect(headerRect.x + 20f, headerRect.y, 120f, headerRect.height);
            var fieldRect = new Rect(headerRect.xMax - 72f, headerRect.y, 72f, headerRect.height);
            enabled = EditorGUI.Toggle(toggleRect, enabled);
            EditorGUI.LabelField(labelRect, label);
            using (new EditorGUI.DisabledScope(!enabled))
            {
                value = EditorGUI.IntField(fieldRect, value ?? min);
            }

            using (new EditorGUI.DisabledScope(!enabled))
            {
                var sliderRect = EditorGUILayout.GetControlRect();
                sliderRect.xMin += 18f;
                var sliderValue = GUI.HorizontalSlider(sliderRect, value ?? min, min, max);
                var nextValue = Mathf.Clamp(Mathf.RoundToInt(sliderValue), min, max);
                return enabled ? nextValue : (int?)null;
            }
        }

        private int GetCountSliderMax(int currentValue)
        {
            return Mathf.Max(32, currentValue);
        }

        private int GetPixelSliderMax(int currentValue, int sourceDimension)
        {
            return Mathf.Max(64, Mathf.Max(currentValue, sourceDimension));
        }

        private int GetSourceWidth()
        {
            return _sourceTexture == null ? 0 : _sourceTexture.width;
        }

        private int GetSourceHeight()
        {
            return _sourceTexture == null ? 0 : _sourceTexture.height;
        }

        private void OpenParameterHelpWindow()
        {
            _parameterHelpWindow = GetWindow<ParameterHelpWindow>();
            _parameterHelpWindow.SetOwner(this);
            _parameterHelpWindow.Show();
        }

        private void DrawCenterPane()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.MinWidth(360f), GUILayout.ExpandWidth(true)))
            {
                DrawSectionHeader(T("workspace", "Workspace"));
                CalculatePreview();

                if (_sourceTexture == null)
                {
                    using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                    {
                        EditorGUILayout.HelpBox(T("workspaceEmptyTitle", "Source image is not selected."), MessageType.Info);
                        EditorGUILayout.LabelField(T("workspaceEmptyBody", "Select a Texture2D in the toolbar, then adjust rows, columns, and margins from the left settings pane."), EditorStyles.wordWrappedMiniLabel);
                        EditorGUILayout.Space(4f);
                        EditorGUILayout.LabelField(T("workspaceFlowHint", "Workflow: Source -> Settings -> Preview -> Export -> Save Session"), EditorStyles.miniBoldLabel);
                    }

                    return;
                }

                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    DrawSectionHeader(T("workspaceSummary", "Current Summary"));
                    DrawSummaryRow(T("source", "Source"), AssetDatabase.GetAssetPath(_sourceTexture));
                    DrawSummaryRow(T("detectedGrid", "Detected Grid"), $"{_gridSettings.Columns} x {_gridSettings.Rows}");
                    DrawSummaryRow(T("totalCells", "Total Cells"), _lastGridResult.Cells.Count.ToString());
                    DrawSummaryRow(T("included", "Included"), _lastGridResult.Cells.Count(cell => !IsExcluded(cell.Coordinate)).ToString());
                    DrawSummaryRow(T("excluded", "Excluded"), _selection.ExcludedCells.Count.ToString());
                    DrawSummaryRow(T("readableSource", "Readable Source"), IsSourceReadable() ? T("pass", "Pass") : T("warning", "Warning"));
                }

                EditorGUILayout.Space(6f);
                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    DrawSectionHeader(T("workspaceActions", "Next Actions"));
                    EditorGUILayout.HelpBox(T("detachedPreviewHelp", "Preview is shown in a separate resizable window. Use the Preview button to inspect cells while editing settings here."), MessageType.Info);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button(T("preview", "Preview"), GUILayout.Width(120f)))
                        {
                            OpenDetachedPreviewWindow();
                        }

                        using (new EditorGUI.DisabledScope(_lastGridResult.Cells.Count == 0))
                        {
                            if (GUILayout.Button(T("export", "Export..."), GUILayout.Width(120f)))
                            {
                                ExportCells();
                            }
                        }
                    }

                    if (_selectedCell.HasValue)
                    {
                        var selectedRect = GetSelectedRect();
                        if (selectedRect.HasValue)
                        {
                            EditorGUILayout.LabelField(
                                TFormat(
                                    "workspaceSelectedCell",
                                    "Selected cell: Row {0}, Column {1}",
                                    selectedRect.Value.Coordinate.Row,
                                    selectedRect.Value.Coordinate.Column),
                                EditorStyles.miniLabel);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField(T("workspaceNoSelectedCell", "No cell is selected. Use the preview window to inspect one cell in detail."), EditorStyles.miniLabel);
                    }
                }

                if (_lastGridResult.Errors.Count > 0)
                {
                    EditorGUILayout.Space(6f);
                    using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                    {
                        DrawSectionHeader(T("workspaceWarnings", "Current Warnings"));
                        foreach (var error in _lastGridResult.Errors)
                        {
                            EditorGUILayout.HelpBox(LocalizeMessage(error), MessageType.Warning);
                        }
                    }
                }

                EditorGUILayout.Space(6f);
                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    DrawSectionHeader(T("latestExportSummary", "Latest Export"));
                    if (_lastExportResult == null)
                    {
                        EditorGUILayout.LabelField(T("noExportResult", "No export result yet."), EditorStyles.miniLabel);
                    }
                    else
                    {
                        DrawSummaryRow(T("exported", "Exported"), _lastExportResult.ExportedFiles.Count.ToString());
                        DrawSummaryRow(T("skipped", "Skipped"), _lastExportResult.SkippedFiles.Count.ToString());
                        DrawSummaryRow(T("error", "Error"), _lastExportResult.Errors.Count.ToString());
                    }
                }
            }
        }

        private void DrawOutputSettings()
        {
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

            _exportSettings.FilePrefix = EditorGUILayout.TextField(T("outputPrefix", "Output Prefix"), _exportSettings.FilePrefix);
            _exportSettings.StartIndex = EditorGUILayout.IntField(T("startIndex", "Start Index"), _exportSettings.StartIndex);
            _exportSettings.NumberPadding = EditorGUILayout.IntField(T("serialDigits", "Serial Digits"), _exportSettings.NumberPadding);
            _exportSettings.OutputWidth = DrawNullableIntWithSlider(T("outputWidth", "Output Width"), _exportSettings.OutputWidth, 1, GetPixelSliderMax(_exportSettings.OutputWidth ?? 64, 4096));
            _exportSettings.OutputHeight = DrawNullableIntWithSlider(T("outputHeight", "Output Height"), _exportSettings.OutputHeight, 1, GetPixelSliderMax(_exportSettings.OutputHeight ?? 64, 4096));
            _exportSettings.ConflictBehavior = (ExportConflictBehavior)EditorGUILayout.EnumPopup(T("conflictMode", "Conflict Mode"), _exportSettings.ConflictBehavior);
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
                    EditorGUILayout.HelpBox(GridAssetSlicerLocalization.Get(language, "detachedPreviewReconnect", "Open Tools > Grid Asset Slicer > メイン画面 again to reconnect the preview."), MessageType.Info);
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

        private enum MarginDragHandle
        {
            None,
            Left,
            Top,
            Right,
            Bottom
        }

        private sealed class ParameterHelpWindow : EditorWindow
        {
            private GridAssetSlicerWindow _owner;
            private Vector2 _scroll;

            public void SetOwner(GridAssetSlicerWindow owner)
            {
                _owner = owner;
                titleContent = new GUIContent(owner.T("parameterHelp", "Parameter Help"));
                minSize = new Vector2(420f, 360f);
            }

            private void OnGUI()
            {
                if (_owner == null)
                {
                    var language = GridAssetSlicerLocalization.ResolveLanguage(GridAssetSlicerLanguageMode.Auto);
                    EditorGUILayout.HelpBox(GridAssetSlicerLocalization.Get(language, "detachedHelpReconnect", "Open Tools > Grid Asset Slicer > メイン画面 again to reconnect help."), MessageType.Info);
                    return;
                }

                _scroll = EditorGUILayout.BeginScrollView(_scroll);
                _owner.DrawHelpSection("sourceImage", "Source Image", new[]
                {
                    ("sourceImage", "Source Image", "help.sourceImage", "Source Image selects the texture asset to preview and export. Language changes the tool UI text.")
                });
                _owner.DrawHelpSection("gridSettings", "Grid Settings", new[]
                {
                    ("rows", "Rows", "help.rows", "Vertical cell count in the source image."),
                    ("columns", "Columns", "help.columns", "Horizontal cell count in the source image."),
                    ("marginLeft", "Margin Left", "help.marginLeft", "Pixels to skip from the left edge before the grid starts."),
                    ("marginTop", "Margin Top", "help.marginTop", "Pixels to skip from the top edge before the grid starts."),
                    ("marginRight", "Margin Right", "help.marginRight", "Pixels excluded from the right edge after the grid ends."),
                    ("marginBottom", "Margin Bottom", "help.marginBottom", "Pixels excluded from the bottom edge after the grid ends."),
                    ("gutterX", "Gutter X", "help.gutterX", "Horizontal pixel spacing between neighboring cells."),
                    ("gutterY", "Gutter Y", "help.gutterY", "Vertical pixel spacing between neighboring cells."),
                    ("cellWidth", "Cell Width", "help.cellWidth", "Explicit cell width. Turn it off to calculate width from the source image, margins, gutters, and column count."),
                    ("cellHeight", "Cell Height", "help.cellHeight", "Explicit cell height. Turn it off to calculate height from the source image, margins, gutters, and row count.")
                });
                _owner.DrawHelpSection("marginController", "Margin Controller", new[]
                {
                    ("marginController", "Margin Controller", "marginControllerHelp", "Drag the guide lines on this source-image controller to adjust the margins without opening the preview window.")
                });
                _owner.DrawHelpSection("variableGrid", "Variable Grid Boundaries", new[]
                {
                    ("customColumns", "Custom Columns", "variableGridHelp", "Enable custom widths when each column boundary must be adjusted independently."),
                    ("customRows", "Custom Rows", "variableGridHelp", "Enable custom heights when each row boundary must be adjusted independently.")
                });
                _owner.DrawHelpSection("output", "Output", new[]
                {
                    ("outputFolder", "Output Folder", "help.outputFolder", "Project-relative folder where generated PNG files are written."),
                    ("outputPrefix", "Output Prefix", "help.outputPrefix", "Prefix used before the serial number in each generated file name."),
                    ("startIndex", "Start Index", "help.startIndex", "First serial number used when naming exported cells."),
                    ("serialDigits", "Serial Digits", "help.serialDigits", "Minimum digit count for serial numbers. For example, 3 produces 001."),
                    ("outputWidth", "Output Width", "help.outputWidth", "Uniform width used for every exported PNG when enabled."),
                    ("outputHeight", "Output Height", "help.outputHeight", "Uniform height used for every exported PNG when enabled."),
                    ("conflictMode", "Conflict Mode", "help.conflictMode", "Select how export behaves when a target file already exists: overwrite, skip, or create a duplicate name.")
                });
                _owner.DrawHelpSection("qualityChecks", "Quality Checks", new[]
                {
                    ("gridBounds", "Grid Bounds", "help.gridBounds", "Checks whether the calculated grid fits inside the source image."),
                    ("readableSource", "Readable Source", "help.readableSource", "Checks whether Unity can read source texture pixels for PNG export."),
                    ("outputSettings", "Output Settings", "help.outputSettings", "Checks whether export folder and file naming settings can produce valid output paths."),
                    ("includedCells", "Included Cells", "help.includedCells", "Checks whether at least one cell is still included for export.")
                });
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawInspectorPaneContent()
        {
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
                DrawSectionSeparator();
                DrawInspectorPreviewSettings();
                DrawSelectedPreview(rect);
            }
        }

        private void DrawQualityReport()
        {
            EditorGUILayout.Space(6f);
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawSectionHeader(T("qualityReport", "Quality Check Report"), GUILayout.Width(260f));
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(T("reportHeight", "Report Height"), GUILayout.Width(90f));
                var nextReportHeight = EditorGUILayout.Slider(_reportHeight, MinReportHeight, MaxReportHeight, GUILayout.Width(220f));
                if (!Mathf.Approximately(nextReportHeight, _reportHeight))
                {
                    _reportHeight = nextReportHeight;
                    EditorPrefs.SetFloat(ReportHeightPrefsKey, _reportHeight);
                }
            }

            _reportScroll = EditorGUILayout.BeginScrollView(_reportScroll, GUI.skin.box, GUILayout.Height(_reportHeight));
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
                    : QualityReportEntry.Warning(T("readableSource", "Readable Source"), T("quality.readable.fail", "Source texture is not directly readable. Export uses a temporary readable copy."))
                : QualityReportEntry.Disabled(T("readableSource", "Readable Source"), T("quality.disabled", "This quality check is turned off.")));

            var outputErrors = ExportSettingsValidator.Validate(_exportSettings).ToList();
            if (outputErrors.Count == 0)
            {
                outputErrors.AddRange(ExportFileNameResolver.BuildPlan(
                    _exportSettings,
                    new[] { new CellCoordinate(0, 0) },
                    _ => false).Errors);
            }

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
            EditorGUI.DrawRect(previewRect, _inspectorPreviewBackground);
            var uv = new Rect(
                (float)rect.X / _sourceTexture.width,
                1f - ((float)rect.Bottom / _sourceTexture.height),
                (float)rect.Width / _sourceTexture.width,
                (float)rect.Height / _sourceTexture.height);
            GUI.DrawTextureWithTexCoords(previewRect, _sourceTexture, uv, true);
            if (_showInspectorPreviewOutline)
            {
                DrawOutline(previewRect, _inspectorPreviewOutlineColor, 2f);
            }
        }

        private void DrawInspectorPreviewSettings()
        {
            EditorGUILayout.Space(8f);
            DrawSectionHeader(T("inspectorPreviewSettings", "Preview Display"));
            var nextOutline = EditorGUILayout.Toggle(T("showOutline", "Show Outline"), _showInspectorPreviewOutline);
            if (nextOutline != _showInspectorPreviewOutline)
            {
                _showInspectorPreviewOutline = nextOutline;
                EditorPrefs.SetBool(InspectorOutlinePrefsKey, _showInspectorPreviewOutline);
            }

            _inspectorPreviewBackground = EditorGUILayout.ColorField(T("backgroundColor", "Background Color"), _inspectorPreviewBackground);
            _inspectorPreviewOutlineColor = EditorGUILayout.ColorField(T("outlineColor", "Outline Color"), _inspectorPreviewOutlineColor);
        }

        private void DrawHelpSection(string titleKey, string englishTitle, IEnumerable<(string LabelKey, string EnglishLabel, string HelpKey, string EnglishText)> entries)
        {
            EditorGUILayout.LabelField(T(titleKey, englishTitle), EditorStyles.boldLabel);
            foreach (var entry in entries)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(T(entry.LabelKey, entry.EnglishLabel), EditorStyles.boldLabel, GUILayout.Width(150f));
                    EditorGUILayout.LabelField(T(entry.HelpKey, entry.EnglishText), EditorStyles.wordWrappedLabel);
                }

                EditorGUILayout.Space(4f);
            }

            EditorGUILayout.Space(8f);
        }

        private static void DrawSectionHeader(string label, params GUILayoutOption[] options)
        {
            using (new EditorGUILayout.HorizontalScope(options))
            {
                EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            }

            EditorGUILayout.Space(4f);
        }

        private void DrawFoldoutSection(ref bool isExpanded, string label, Action drawContent)
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                isExpanded = EditorGUILayout.Foldout(isExpanded, label, true);
                if (isExpanded)
                {
                    EditorGUILayout.Space(4f);
                    drawContent?.Invoke();
                }
            }

            EditorGUILayout.Space(6f);
        }

        private static void DrawSummaryRow(string label, string value)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(110f));
                EditorGUILayout.LabelField(value, EditorStyles.wordWrappedMiniLabel);
            }
        }

        private static void DrawSectionSeparator()
        {
            EditorGUILayout.Space(8f);
            var rect = GUILayoutUtility.GetRect(1f, 1f, GUILayout.ExpandWidth(true), GUILayout.Height(1f));
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, new Color(0.42f, 0.42f, 0.42f, 1f));
            }

            EditorGUILayout.Space(8f);
        }

        private static void DrawPaneSeparator()
        {
            var rect = GUILayoutUtility.GetRect(1f, 1f, GUILayout.Width(1f), GUILayout.ExpandHeight(true));
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, new Color(0.25f, 0.25f, 0.25f, 1f));
            }
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

            Texture2D exportTexture = null;
            var shouldDestroyExportTexture = false;
            try
            {
                exportTexture = PrepareExportTexture(out shouldDestroyExportTexture);
                var result = PngExporter.Export(new PngExportRequest(exportTexture, _lastGridResult.Cells, _selection, _exportSettings));
                _lastExportResult = result;
                AssetDatabaseExportRefresher.RefreshIfExportedUnderAssets(result);
                _statusMessage = result.IsSuccess
                    ? TFormat("status.exported", "Exported {0}, skipped {1}.", result.ExportedFiles.Count, result.SkippedFiles.Count)
                    : TFormat("status.exportFailed", "Export completed with {0} error(s).", result.Errors.Count);
            }
            finally
            {
                CleanupReadableTexture(exportTexture, shouldDestroyExportTexture);
            }
        }

        private Texture2D PrepareExportTexture(out bool shouldDestroyExportTexture)
        {
            shouldDestroyExportTexture = false;
            if (IsSourceReadable())
            {
                return _sourceTexture;
            }

            var sourcePath = AssetDatabase.GetAssetPath(_sourceTexture);
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            {
                return _sourceTexture;
            }

            var readableTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(readableTexture, File.ReadAllBytes(sourcePath)))
            {
                UnityEngine.Object.DestroyImmediate(readableTexture);
                return _sourceTexture;
            }

            shouldDestroyExportTexture = true;
            return readableTexture;
        }

        private static void CleanupReadableTexture(Texture2D exportTexture, bool shouldDestroyExportTexture)
        {
            if (!shouldDestroyExportTexture || exportTexture == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(exportTexture);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(exportTexture);
            }
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

        private static void DrawOutline(Rect rect, Color color, float thickness)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }

        private string LocalizeReportStatus(string status)
        {
            switch (status)
            {
                case "Pass":
                    return T("pass", "Pass");
                case "Warning":
                    return T("warning", "Warning");
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
                case "Available grid width must allocate at least one pixel to every cell.":
                    return T("error.availableWidthPerCell", "Available grid width must allocate at least one pixel to every cell.");
                case "Available grid height must allocate at least one pixel to every cell.":
                    return T("error.availableHeightPerCell", "Available grid height must allocate at least one pixel to every cell.");
                case "Output Folder is required.":
                    return T("error.outputFolderRequired", "Output Folder is required.");
                case "Output Prefix is required.":
                    return T("error.outputPrefixRequired", "Output Prefix is required.");
                case "Start Index must be zero or greater.":
                    return T("error.startIndexNonNegative", "Start Index must be zero or greater.");
                case "Serial Digits must be zero or greater.":
                    return T("error.numberPaddingNonNegative", "Serial Digits must be zero or greater.");
                case "Output Width and Output Height must both be specified together.":
                    return T("error.outputResizeBothRequired", "Output Width and Output Height must both be specified together.");
                case "Output Width must be greater than zero when specified.":
                    return T("error.outputWidthPositive", "Output Width must be greater than zero when specified.");
                case "Output Height must be greater than zero when specified.":
                    return T("error.outputHeightPositive", "Output Height must be greater than zero when specified.");
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
                var reason = ExtractAfter(message, ": ");
                return TFormat("error.failedToExportCell", "Failed to export cell {0} to {1}. Reason: {2}", cell, path, reason);
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

        private static string ExtractAfter(string text, string marker)
        {
            var index = text.IndexOf(marker, StringComparison.Ordinal);
            return index < 0 ? text : text.Substring(index + marker.Length);
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

            public static QualityReportEntry Warning(string name, string details)
            {
                return new QualityReportEntry(name, "Warning", details);
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
