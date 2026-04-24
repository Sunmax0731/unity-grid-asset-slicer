using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Sunmax.GridAssetSlicer
{
    public static class PngExporter
    {
        public static PngExportResult Export(PngExportRequest request, Func<string, bool> fileExists = null)
        {
            if (request == null)
            {
                return Error("PNG export request is required.");
            }

            if (request.SourceTexture == null)
            {
                return Error("Source texture is required.");
            }

            fileExists ??= File.Exists;

            var includedCells = GetIncludedCells(request.Cells, request.Selection);
            var plan = ExportFileNameResolver.BuildPlan(
                request.ExportSettings,
                includedCells.Select(cell => cell.Coordinate).ToArray(),
                fileExists);

            if (!plan.IsValid)
            {
                return new PngExportResult(Array.Empty<PngExportFileResult>(), Array.Empty<PngExportFileResult>(), plan.Errors);
            }

            var cellMap = includedCells.ToDictionary(cell => cell.Coordinate);
            var exported = new List<PngExportFileResult>();
            var skipped = new List<PngExportFileResult>();
            var errors = new List<string>();

            foreach (var item in plan.Items)
            {
                if (item.Action == ExportActionType.Skip)
                {
                    skipped.Add(new PngExportFileResult(item.Cell, item.OutputPath));
                    continue;
                }

                if (item.Action == ExportActionType.Blocked)
                {
                    errors.Add($"Export was blocked for cell {item.Cell} at {item.OutputPath}.");
                    continue;
                }

                if (!cellMap.TryGetValue(item.Cell, out var rect))
                {
                    errors.Add($"Cell {item.Cell} was not found in the export cell list.");
                    continue;
                }

                try
                {
                    var pngBytes = TextureCellExtractor.ExtractPng(request.SourceTexture, rect);
                    var directory = Path.GetDirectoryName(item.OutputPath);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.WriteAllBytes(item.OutputPath, pngBytes);
                    exported.Add(new PngExportFileResult(item.Cell, item.OutputPath));
                }
                catch (Exception exception)
                {
                    errors.Add($"Failed to export cell {item.Cell} to {item.OutputPath}: {exception.Message}");
                }
            }

            return new PngExportResult(exported, skipped, errors);
        }

        private static PngExportResult Error(string error)
        {
            return new PngExportResult(Array.Empty<PngExportFileResult>(), Array.Empty<PngExportFileResult>(), new[] { error });
        }

        private static IReadOnlyList<CellRect> GetIncludedCells(IReadOnlyList<CellRect> cells, SliceSelection selection)
        {
            var excluded = new HashSet<CellCoordinate>(selection.ExcludedCells);
            return cells.Where(cell => !excluded.Contains(cell.Coordinate)).ToArray();
        }
    }
}
