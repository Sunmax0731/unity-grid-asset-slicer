using System;
using System.Collections.Generic;
using System.IO;

namespace Sunmax.GridAssetSlicer
{
    public static class ExportFileNameResolver
    {
        private const int MaxDuplicateAttempts = 9999;

        public static ExportPlan BuildPlan(
            ExportSettings settings,
            IReadOnlyList<CellCoordinate> cells,
            Func<string, bool> fileExists)
        {
            if (fileExists == null)
            {
                throw new ArgumentNullException(nameof(fileExists));
            }

            var errors = Validate(settings, cells);
            if (errors.Count > 0)
            {
                return new ExportPlan(Array.Empty<ExportPlanItem>(), errors);
            }

            var items = new List<ExportPlanItem>(cells.Count);
            var reservedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (var index = 0; index < cells.Count; index++)
            {
                var sequence = settings.StartIndex + index;
                var targetPath = BuildPath(settings, sequence);
                var exists = fileExists(targetPath) || reservedPaths.Contains(targetPath);

                switch (settings.ConflictBehavior)
                {
                    case ExportConflictBehavior.Overwrite:
                        items.Add(new ExportPlanItem(cells[index], sequence, targetPath, ExportActionType.Write, exists));
                        reservedPaths.Add(targetPath);
                        break;
                    case ExportConflictBehavior.Skip:
                        items.Add(new ExportPlanItem(cells[index], sequence, targetPath, exists ? ExportActionType.Skip : ExportActionType.Write, exists));
                        if (!exists)
                        {
                            reservedPaths.Add(targetPath);
                        }

                        break;
                    case ExportConflictBehavior.Duplicate:
                        var duplicatePath = exists ? ResolveDuplicatePath(targetPath, fileExists, reservedPaths) : targetPath;
                        if (duplicatePath == null)
                        {
                            items.Add(new ExportPlanItem(cells[index], sequence, targetPath, ExportActionType.Blocked, true));
                            errors.Add($"Could not resolve a duplicate file name for {targetPath}.");
                        }
                        else
                        {
                            items.Add(new ExportPlanItem(cells[index], sequence, duplicatePath, ExportActionType.Write, exists));
                            reservedPaths.Add(duplicatePath);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(settings.ConflictBehavior), settings.ConflictBehavior, null);
                }
            }

            return new ExportPlan(items, errors);
        }

        private static List<string> Validate(ExportSettings settings, IReadOnlyList<CellCoordinate> cells)
        {
            var errors = new List<string>();

            if (settings == null)
            {
                errors.Add("Export settings are required.");
                return errors;
            }

            if (cells == null)
            {
                errors.Add("Cells are required.");
                return errors;
            }

            if (string.IsNullOrWhiteSpace(settings.OutputFolder))
            {
                errors.Add("OutputFolder is required.");
            }

            if (settings.FilePrefix == null)
            {
                errors.Add("FilePrefix is required.");
            }

            if (settings.StartIndex < 0)
            {
                errors.Add("StartIndex must be zero or greater.");
            }

            if (settings.NumberPadding < 0)
            {
                errors.Add("NumberPadding must be zero or greater.");
            }

            return errors;
        }

        private static string BuildPath(ExportSettings settings, int sequence)
        {
            var number = settings.NumberPadding > 0
                ? sequence.ToString("D" + settings.NumberPadding)
                : sequence.ToString();
            return NormalizePath(Path.Combine(settings.OutputFolder, settings.FilePrefix + number + ".png"));
        }

        private static string ResolveDuplicatePath(string targetPath, Func<string, bool> fileExists, ISet<string> reservedPaths)
        {
            var directory = Path.GetDirectoryName(targetPath) ?? string.Empty;
            var fileName = Path.GetFileNameWithoutExtension(targetPath);
            var extension = Path.GetExtension(targetPath);

            for (var attempt = 1; attempt <= MaxDuplicateAttempts; attempt++)
            {
                var candidate = NormalizePath(Path.Combine(directory, $"{fileName}_copy{attempt:00}{extension}"));
                if (!fileExists(candidate) && !reservedPaths.Contains(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static string NormalizePath(string path)
        {
            return path.Replace('\\', '/');
        }
    }
}
