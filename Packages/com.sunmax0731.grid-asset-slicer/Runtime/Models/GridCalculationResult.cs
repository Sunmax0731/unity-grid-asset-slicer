using System;
using System.Collections.Generic;

namespace Sunmax.GridAssetSlicer
{
    public sealed class GridCalculationResult
    {
        public GridCalculationResult(IReadOnlyList<CellRect> cells, IReadOnlyList<string> errors)
        {
            Cells = cells ?? throw new ArgumentNullException(nameof(cells));
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }

        public IReadOnlyList<CellRect> Cells { get; }

        public IReadOnlyList<string> Errors { get; }

        public bool IsValid => Errors.Count == 0;
    }
}
