using System.Collections.Generic;

namespace Sunmax.GridAssetSlicer
{
    public sealed class SliceSelection
    {
        public List<CellCoordinate> ExcludedCells { get; } = new List<CellCoordinate>();
    }
}
