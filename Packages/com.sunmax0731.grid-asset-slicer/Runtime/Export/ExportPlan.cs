using System;
using System.Collections.Generic;
using System.Linq;

namespace Sunmax.GridAssetSlicer
{
    public sealed class ExportPlan
    {
        public ExportPlan(IReadOnlyList<ExportPlanItem> items, IReadOnlyList<string> errors)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }

        public IReadOnlyList<ExportPlanItem> Items { get; }

        public IReadOnlyList<string> Errors { get; }

        public bool IsValid => Errors.Count == 0;

        public IReadOnlyList<ExportPlanItem> WritableItems => Items.Where(item => item.Action == ExportActionType.Write).ToArray();

        public IReadOnlyList<ExportPlanItem> SkippedItems => Items.Where(item => item.Action == ExportActionType.Skip).ToArray();

        public IReadOnlyList<ExportPlanItem> BlockedItems => Items.Where(item => item.Action == ExportActionType.Blocked).ToArray();
    }
}
