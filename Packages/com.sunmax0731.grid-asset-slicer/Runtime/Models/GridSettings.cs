namespace Sunmax.GridAssetSlicer
{
    public sealed class GridSettings
    {
        public int Rows { get; set; }

        public int Columns { get; set; }

        public int MarginLeft { get; set; }

        public int MarginTop { get; set; }

        public int MarginRight { get; set; }

        public int MarginBottom { get; set; }

        public int GutterX { get; set; }

        public int GutterY { get; set; }

        public int? CellWidth { get; set; }

        public int? CellHeight { get; set; }
    }
}
