namespace Sunmax.GridAssetSlicer
{
    public sealed class ExportPlanItem
    {
        public ExportPlanItem(CellCoordinate cell, int sequence, string outputPath, ExportActionType action, bool targetExisted)
        {
            Cell = cell;
            Sequence = sequence;
            OutputPath = outputPath;
            Action = action;
            TargetExisted = targetExisted;
        }

        public CellCoordinate Cell { get; }

        public int Sequence { get; }

        public string OutputPath { get; }

        public ExportActionType Action { get; }

        public bool TargetExisted { get; }
    }
}
