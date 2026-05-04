using System;
using System.Collections.Generic;

namespace Sunmax.GridAssetSlicer
{
    public static class GridCalculator
    {
        public static GridCalculationResult Calculate(int imageWidth, int imageHeight, GridSettings settings)
        {
            if (settings == null)
            {
                return Invalid("Grid settings are required.");
            }

            var errors = new List<string>();
            ValidateImageSize(imageWidth, imageHeight, errors);
            ValidateSettings(settings, errors);

            if (errors.Count > 0)
            {
                return new GridCalculationResult(Array.Empty<CellRect>(), errors);
            }

            var cellWidths = ResolveCellSizes(
                imageWidth,
                settings.MarginLeft,
                settings.MarginRight,
                settings.GutterX,
                settings.Columns,
                settings.CellWidth,
                settings.ColumnWidths,
                "width",
                errors);

            var cellHeights = ResolveCellSizes(
                imageHeight,
                settings.MarginTop,
                settings.MarginBottom,
                settings.GutterY,
                settings.Rows,
                settings.CellHeight,
                settings.RowHeights,
                "height",
                errors);

            if (errors.Count > 0)
            {
                return new GridCalculationResult(Array.Empty<CellRect>(), errors);
            }

            var rightLimit = imageWidth - settings.MarginRight;
            var bottomLimit = imageHeight - settings.MarginBottom;
            var gridRight = settings.MarginLeft + Sum(cellWidths) + ((settings.Columns - 1) * settings.GutterX);
            var gridBottom = settings.MarginTop + Sum(cellHeights) + ((settings.Rows - 1) * settings.GutterY);

            if (gridRight > rightLimit)
            {
                errors.Add($"Grid width exceeds image bounds. Grid right={gridRight}, allowed right={rightLimit}.");
            }

            if (gridBottom > bottomLimit)
            {
                errors.Add($"Grid height exceeds image bounds. Grid bottom={gridBottom}, allowed bottom={bottomLimit}.");
            }

            var cells = new List<CellRect>(settings.Rows * settings.Columns);
            var rowOffsets = BuildOffsets(settings.MarginTop, cellHeights, settings.GutterY);
            var columnOffsets = BuildOffsets(settings.MarginLeft, cellWidths, settings.GutterX);
            for (var row = 0; row < settings.Rows; row++)
            {
                for (var column = 0; column < settings.Columns; column++)
                {
                    cells.Add(new CellRect(
                        new CellCoordinate(row, column),
                        columnOffsets[column],
                        rowOffsets[row],
                        cellWidths[column],
                        cellHeights[row]));
                }
            }

            return new GridCalculationResult(cells, errors);
        }

        private static GridCalculationResult Invalid(string error)
        {
            return new GridCalculationResult(Array.Empty<CellRect>(), new[] { error });
        }

        private static void ValidateImageSize(int imageWidth, int imageHeight, ICollection<string> errors)
        {
            if (imageWidth <= 0)
            {
                errors.Add("Image width must be greater than zero.");
            }

            if (imageHeight <= 0)
            {
                errors.Add("Image height must be greater than zero.");
            }
        }

        private static void ValidateSettings(GridSettings settings, ICollection<string> errors)
        {
            if (settings.Rows <= 0)
            {
                errors.Add("Rows must be greater than zero.");
            }

            if (settings.Columns <= 0)
            {
                errors.Add("Columns must be greater than zero.");
            }

            AddNonNegativeError(settings.MarginLeft, nameof(settings.MarginLeft), errors);
            AddNonNegativeError(settings.MarginTop, nameof(settings.MarginTop), errors);
            AddNonNegativeError(settings.MarginRight, nameof(settings.MarginRight), errors);
            AddNonNegativeError(settings.MarginBottom, nameof(settings.MarginBottom), errors);
            AddNonNegativeError(settings.GutterX, nameof(settings.GutterX), errors);
            AddNonNegativeError(settings.GutterY, nameof(settings.GutterY), errors);

            if (settings.CellWidth.HasValue && settings.CellWidth.Value <= 0)
            {
                errors.Add("CellWidth must be greater than zero when specified.");
            }

            if (settings.CellHeight.HasValue && settings.CellHeight.Value <= 0)
            {
                errors.Add("CellHeight must be greater than zero when specified.");
            }

            ValidateCustomSizes(settings.ColumnWidths, settings.Columns, "ColumnWidths", errors);
            ValidateCustomSizes(settings.RowHeights, settings.Rows, "RowHeights", errors);
        }

        private static void AddNonNegativeError(int value, string name, ICollection<string> errors)
        {
            if (value < 0)
            {
                errors.Add($"{name} must be zero or greater.");
            }
        }

        private static int[] ResolveCellSizes(
            int imageSize,
            int leadingMargin,
            int trailingMargin,
            int gutter,
            int cellCount,
            int? explicitCellSize,
            int[] customCellSizes,
            string axisName,
            ICollection<string> errors)
        {
            if (customCellSizes != null && customCellSizes.Length > 0)
            {
                var sizes = new int[customCellSizes.Length];
                Array.Copy(customCellSizes, sizes, customCellSizes.Length);
                return sizes;
            }

            if (explicitCellSize.HasValue)
            {
                var sizes = new int[cellCount];
                for (var index = 0; index < cellCount; index++)
                {
                    sizes[index] = explicitCellSize.Value;
                }

                return sizes;
            }

            var available = imageSize - leadingMargin - trailingMargin - (gutter * (cellCount - 1));
            if (available <= 0)
            {
                errors.Add($"Available grid {axisName} must be greater than zero.");
                return Array.Empty<int>();
            }

            if (available < cellCount)
            {
                errors.Add($"Available grid {axisName} must allocate at least one pixel to every cell.");
                return Array.Empty<int>();
            }

            var boundaries = new int[cellCount + 1];
            for (var index = 0; index <= cellCount; index++)
            {
                boundaries[index] = (int)Math.Round((double)(index * available) / cellCount, MidpointRounding.AwayFromZero);
            }

            var cellSizes = new int[cellCount];
            for (var index = 0; index < cellCount; index++)
            {
                cellSizes[index] = boundaries[index + 1] - boundaries[index];
                if (cellSizes[index] <= 0)
                {
                    errors.Add($"Available grid {axisName} must allocate at least one pixel to every cell.");
                    return Array.Empty<int>();
                }
            }

            return cellSizes;
        }

        private static void ValidateCustomSizes(int[] customSizes, int expectedCount, string fieldName, ICollection<string> errors)
        {
            if (customSizes == null || customSizes.Length == 0)
            {
                return;
            }

            if (customSizes.Length != expectedCount)
            {
                errors.Add($"{fieldName} length must match the configured cell count.");
                return;
            }

            for (var index = 0; index < customSizes.Length; index++)
            {
                if (customSizes[index] <= 0)
                {
                    errors.Add($"{fieldName}[{index}] must be greater than zero.");
                }
            }
        }

        private static int[] BuildOffsets(int start, IReadOnlyList<int> sizes, int gutter)
        {
            var offsets = new int[sizes.Count];
            var current = start;
            for (var index = 0; index < sizes.Count; index++)
            {
                offsets[index] = current;
                current += sizes[index] + gutter;
            }

            return offsets;
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
    }
}
