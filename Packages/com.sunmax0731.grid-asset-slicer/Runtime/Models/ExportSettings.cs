namespace Sunmax.GridAssetSlicer
{
    public sealed class ExportSettings
    {
        public string OutputFolder { get; set; }

        public string FilePrefix { get; set; }

        public int StartIndex { get; set; }

        public int NumberPadding { get; set; }

        public ExportConflictBehavior ConflictBehavior { get; set; }
    }
}
