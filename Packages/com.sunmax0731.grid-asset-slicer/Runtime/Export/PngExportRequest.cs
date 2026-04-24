using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sunmax.GridAssetSlicer
{
    public sealed class PngExportRequest
    {
        public PngExportRequest(
            Texture2D sourceTexture,
            IReadOnlyList<CellRect> cells,
            SliceSelection selection,
            ExportSettings exportSettings)
        {
            SourceTexture = sourceTexture;
            Cells = cells ?? throw new ArgumentNullException(nameof(cells));
            Selection = selection ?? new SliceSelection();
            ExportSettings = exportSettings ?? throw new ArgumentNullException(nameof(exportSettings));
        }

        public Texture2D SourceTexture { get; }

        public IReadOnlyList<CellRect> Cells { get; }

        public SliceSelection Selection { get; }

        public ExportSettings ExportSettings { get; }
    }
}
