namespace Sunmax.GridAssetSlicer
{
    public sealed class GridSliceSession
    {
        public const int CurrentFormatVersion = 1;

        public int FormatVersion { get; set; } = CurrentFormatVersion;

        public string CreatedUtc { get; set; }

        public string ToolVersion { get; set; }

        public SourceImageReference Source { get; set; }

        public GridSettings Grid { get; set; }

        public SliceSelection Selection { get; set; } = new SliceSelection();

        public ExportSettings Export { get; set; }
    }
}
