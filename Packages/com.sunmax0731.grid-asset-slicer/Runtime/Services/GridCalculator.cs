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

            var cellWidth = ResolveCellSize(
                imageWidth,
                settings.MarginLeft,
                settings.MarginRight,
                settings.GutterX,
                settings.Columns,
                settings.CellWidth,
                "width",
                errors);

            var cellHeight = ResolveCellSize(
                imageHeight,
                settings.MarginTop,
                settings.MarginBottom,
                settings.GutterY,
                settings.Rows,
                settings.CellHeight,
                "height",
                errors);

            if (errors.Count > 0)
            {
                return new GridCalculationResult(Array.Empty<CellRect>(), errors);
            }

            var rightLimit = imageWidth - settings.MarginRight;
            var bottomLimit = imageHeight - settings.MarginBottom;
            var gridRight = settings.MarginLeft + (settings.Columns * cellWidth) + ((settings.Columns - 1) * settings.GutterX);
            var gridBottom = settings.MarginTop + (settings.Rows * cellHeight) + ((settings.Rows - 1) * settings.GutterY);

            if (gridRight > rightLimit)
            {
                errors.Add($"Grid width exceeds image bounds. Grid right={gridRight}, allowed right={rightLimit}.");
            }

            if (gridBottom > bottomLimit)
            {
                errors.Add($"Grid height exceeds image bounds. Grid bottom={gridBottom}, allowed bottom={bottomLimit}.");
            }

            var cells = new List<CellRect>(settings.Rows * settings.Columns);
            for (var row = 0; row < settings.Rows; row++)
            {
                for (var column = 0; column < settings.Columns; column++)
                {
                    var x = settings.MarginLeft + (column * (cellWidth + settings.GutterX));
                    var y = settings.MarginTop + (row * (cellHeight + settings.GutterY));
                    cells.Add(new CellRect(new CellCoordinate(row, column), x, y, cellWidth, cellHeight));
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
        }

        private static void AddNonNegativeError(int value, string name, ICollection<string> errors)
        {
            if (value < 0)
            {
                errors.Add($"{name} must be zero or greater.");
            }
        }

        private static int ResolveCellSize(
            int imageSize,
            int leadingMargin,
            int trailingMargin,
            int gutter,
            int cellCount,
            int? explicitCellSize,
            string axisName,
            ICollection<string> errors)
        {
            if (explicitCellSize.HasValue)
            {
                return explicitCellSize.Value;
            }

            var available = imageSize - leadingMargin - trailingMargin - (gutter * (cellCount - 1));
            if (available <= 0)
            {
                errors.Add($"Available grid {axisName} must be greater than zero.");
                return 0;
            }

            if (available % cellCount != 0)
            {
                errors.Add($"Available grid {axisName} is not evenly divisible by the cell count.");
                return 0;
            }

            return available / cellCount;
        }
    }
}
