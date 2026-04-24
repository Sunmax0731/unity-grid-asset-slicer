using System;
using System.Collections.Generic;
using System.Linq;

namespace Sunmax.GridAssetSlicer
{
    public sealed class PngExportResult
    {
        public PngExportResult(
            IReadOnlyList<PngExportFileResult> exportedFiles,
            IReadOnlyList<PngExportFileResult> skippedFiles,
            IReadOnlyList<string> errors)
        {
            ExportedFiles = exportedFiles ?? throw new ArgumentNullException(nameof(exportedFiles));
            SkippedFiles = skippedFiles ?? throw new ArgumentNullException(nameof(skippedFiles));
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }

        public IReadOnlyList<PngExportFileResult> ExportedFiles { get; }

        public IReadOnlyList<PngExportFileResult> SkippedFiles { get; }

        public IReadOnlyList<string> Errors { get; }

        public bool IsSuccess => Errors.Count == 0;

        public IReadOnlyList<string> ExportedPaths => ExportedFiles.Select(file => file.OutputPath).ToArray();

        public IReadOnlyList<string> SkippedPaths => SkippedFiles.Select(file => file.OutputPath).ToArray();
    }
}
