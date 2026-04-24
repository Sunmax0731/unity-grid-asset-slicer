using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sunmax.GridAssetSlicer
{
    public static class SliceSessionSerializer
    {
        public static string ToJson(GridSliceSession session, bool prettyPrint = true)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            var validationErrors = Validate(session);
            if (validationErrors.Count > 0)
            {
                throw new ArgumentException(string.Join(Environment.NewLine, validationErrors), nameof(session));
            }

            return JsonUtility.ToJson(ToDto(session), prettyPrint);
        }

        public static SliceSessionLoadResult FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return Invalid("Session JSON is required.");
            }

            SessionDto dto;
            try
            {
                dto = JsonUtility.FromJson<SessionDto>(json);
            }
            catch (Exception exception)
            {
                return Invalid($"Session JSON could not be parsed: {exception.Message}");
            }

            if (dto == null)
            {
                return Invalid("Session JSON could not be parsed.");
            }

            var errors = Validate(dto);
            if (errors.Count > 0)
            {
                return new SliceSessionLoadResult(null, errors);
            }

            return new SliceSessionLoadResult(FromDto(dto), Array.Empty<string>());
        }

        private static SliceSessionLoadResult Invalid(string error)
        {
            return new SliceSessionLoadResult(null, new[] { error });
        }

        private static IReadOnlyList<string> Validate(GridSliceSession session)
        {
            var errors = new List<string>();

            if (session.FormatVersion != GridSliceSession.CurrentFormatVersion)
            {
                errors.Add($"Unsupported formatVersion: {session.FormatVersion}.");
            }

            if (session.Source == null)
            {
                errors.Add("source is required.");
            }
            else
            {
                ValidateSource(session.Source.AssetPath, session.Source.Width, session.Source.Height, errors);
            }

            if (session.Grid == null)
            {
                errors.Add("grid is required.");
            }

            if (session.Export == null)
            {
                errors.Add("export is required.");
            }
            else
            {
                ValidateExport(session.Export.OutputFolder, session.Export.FilePrefix, session.Export.StartIndex, session.Export.NumberPadding, errors);
            }

            return errors;
        }

        private static List<string> Validate(SessionDto dto)
        {
            var errors = new List<string>();

            if (dto.formatVersion != GridSliceSession.CurrentFormatVersion)
            {
                errors.Add(dto.formatVersion <= 0
                    ? "formatVersion is required."
                    : $"Unsupported formatVersion: {dto.formatVersion}.");
            }

            if (dto.source == null || IsDefault(dto.source))
            {
                errors.Add("source is required.");
            }
            else
            {
                ValidateSource(dto.source.assetPath, dto.source.width, dto.source.height, errors);
            }

            if (dto.grid == null)
            {
                errors.Add("grid is required.");
            }
            else
            {
                var gridErrors = GridCalculator.Calculate(dto.source == null ? 0 : dto.source.width, dto.source == null ? 0 : dto.source.height, ToGridSettings(dto.grid)).Errors;
                foreach (var error in gridErrors)
                {
                    errors.Add($"grid: {error}");
                }
            }

            if (dto.export == null)
            {
                errors.Add("export is required.");
            }
            else
            {
                ValidateExport(dto.export.outputFolder, dto.export.filePrefix, dto.export.startIndex, dto.export.numberPadding, errors);
                if (!TryParseConflictBehavior(dto.export.conflictBehavior, out _))
                {
                    errors.Add($"export.conflictBehavior is invalid: {dto.export.conflictBehavior}.");
                }
            }

            return errors;
        }

        private static void ValidateSource(string assetPath, int width, int height, ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                errors.Add("source.assetPath is required.");
            }

            if (width <= 0)
            {
                errors.Add("source.width must be greater than zero.");
            }

            if (height <= 0)
            {
                errors.Add("source.height must be greater than zero.");
            }
        }

        private static bool IsDefault(SourceDto source)
        {
            return string.IsNullOrWhiteSpace(source.assetPath)
                   && source.width == 0
                   && source.height == 0
                   && string.IsNullOrWhiteSpace(source.contentHash);
        }

        private static void ValidateExport(string outputFolder, string filePrefix, int startIndex, int numberPadding, ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(outputFolder))
            {
                errors.Add("export.outputFolder is required.");
            }

            if (filePrefix == null)
            {
                errors.Add("export.filePrefix is required.");
            }

            if (startIndex < 0)
            {
                errors.Add("export.startIndex must be zero or greater.");
            }

            if (numberPadding < 0)
            {
                errors.Add("export.numberPadding must be zero or greater.");
            }
        }

        private static GridSliceSession FromDto(SessionDto dto)
        {
            var session = new GridSliceSession
            {
                FormatVersion = dto.formatVersion,
                CreatedUtc = dto.createdUtc,
                ToolVersion = dto.toolVersion,
                Source = new SourceImageReference
                {
                    AssetPath = dto.source.assetPath,
                    Width = dto.source.width,
                    Height = dto.source.height,
                    ContentHash = dto.source.contentHash
                },
                Grid = ToGridSettings(dto.grid),
                Export = new ExportSettings
                {
                    OutputFolder = dto.export.outputFolder,
                    FilePrefix = dto.export.filePrefix,
                    StartIndex = dto.export.startIndex,
                    NumberPadding = dto.export.numberPadding,
                    ConflictBehavior = ParseConflictBehavior(dto.export.conflictBehavior)
                },
                Selection = new SliceSelection()
            };

            if (dto.selection?.excludedCells != null)
            {
                foreach (var cell in dto.selection.excludedCells)
                {
                    session.Selection.ExcludedCells.Add(new CellCoordinate(cell.row, cell.column));
                }
            }

            return session;
        }

        private static GridSettings ToGridSettings(GridDto dto)
        {
            return new GridSettings
            {
                Rows = dto.rows,
                Columns = dto.columns,
                MarginLeft = dto.marginLeft,
                MarginTop = dto.marginTop,
                MarginRight = dto.marginRight,
                MarginBottom = dto.marginBottom,
                GutterX = dto.gutterX,
                GutterY = dto.gutterY,
                CellWidth = dto.cellWidth > 0 ? dto.cellWidth : (int?)null,
                CellHeight = dto.cellHeight > 0 ? dto.cellHeight : (int?)null
            };
        }

        private static SessionDto ToDto(GridSliceSession session)
        {
            var excludedCells = new CellCoordinateDto[session.Selection?.ExcludedCells.Count ?? 0];
            for (var index = 0; index < excludedCells.Length; index++)
            {
                var cell = session.Selection.ExcludedCells[index];
                excludedCells[index] = new CellCoordinateDto { row = cell.Row, column = cell.Column };
            }

            return new SessionDto
            {
                formatVersion = session.FormatVersion,
                createdUtc = session.CreatedUtc,
                toolVersion = session.ToolVersion,
                source = new SourceDto
                {
                    assetPath = session.Source.AssetPath,
                    width = session.Source.Width,
                    height = session.Source.Height,
                    contentHash = session.Source.ContentHash
                },
                grid = new GridDto
                {
                    rows = session.Grid.Rows,
                    columns = session.Grid.Columns,
                    marginLeft = session.Grid.MarginLeft,
                    marginTop = session.Grid.MarginTop,
                    marginRight = session.Grid.MarginRight,
                    marginBottom = session.Grid.MarginBottom,
                    gutterX = session.Grid.GutterX,
                    gutterY = session.Grid.GutterY,
                    cellWidth = session.Grid.CellWidth ?? 0,
                    cellHeight = session.Grid.CellHeight ?? 0
                },
                selection = new SelectionDto { excludedCells = excludedCells },
                export = new ExportDto
                {
                    outputFolder = session.Export.OutputFolder,
                    filePrefix = session.Export.FilePrefix,
                    startIndex = session.Export.StartIndex,
                    numberPadding = session.Export.NumberPadding,
                    conflictBehavior = ToJsonValue(session.Export.ConflictBehavior)
                }
            };
        }

        private static ExportConflictBehavior ParseConflictBehavior(string value)
        {
            TryParseConflictBehavior(value, out var behavior);
            return behavior;
        }

        private static bool TryParseConflictBehavior(string value, out ExportConflictBehavior behavior)
        {
            switch (value)
            {
                case "overwrite":
                    behavior = ExportConflictBehavior.Overwrite;
                    return true;
                case "skip":
                    behavior = ExportConflictBehavior.Skip;
                    return true;
                case "duplicate":
                    behavior = ExportConflictBehavior.Duplicate;
                    return true;
                default:
                    behavior = default;
                    return false;
            }
        }

        private static string ToJsonValue(ExportConflictBehavior behavior)
        {
            switch (behavior)
            {
                case ExportConflictBehavior.Overwrite:
                    return "overwrite";
                case ExportConflictBehavior.Skip:
                    return "skip";
                case ExportConflictBehavior.Duplicate:
                    return "duplicate";
                default:
                    throw new ArgumentOutOfRangeException(nameof(behavior), behavior, null);
            }
        }

        [Serializable]
        private sealed class SessionDto
        {
            public int formatVersion;
            public string createdUtc;
            public string toolVersion;
            public SourceDto source;
            public GridDto grid;
            public SelectionDto selection;
            public ExportDto export;
        }

        [Serializable]
        private sealed class SourceDto
        {
            public string assetPath;
            public int width;
            public int height;
            public string contentHash;
        }

        [Serializable]
        private sealed class GridDto
        {
            public int rows;
            public int columns;
            public int marginLeft;
            public int marginTop;
            public int marginRight;
            public int marginBottom;
            public int gutterX;
            public int gutterY;
            public int cellWidth;
            public int cellHeight;
        }

        [Serializable]
        private sealed class SelectionDto
        {
            public CellCoordinateDto[] excludedCells;
        }

        [Serializable]
        private sealed class CellCoordinateDto
        {
            public int row;
            public int column;
        }

        [Serializable]
        private sealed class ExportDto
        {
            public string outputFolder;
            public string filePrefix;
            public int startIndex;
            public int numberPadding;
            public string conflictBehavior;
        }
    }
}
