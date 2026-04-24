namespace Sunmax.GridAssetSlicer
{
    public sealed class PngExportFileResult
    {
        public PngExportFileResult(CellCoordinate cell, string outputPath)
        {
            Cell = cell;
            OutputPath = outputPath;
        }

        public CellCoordinate Cell { get; }

        public string OutputPath { get; }
    }
}
