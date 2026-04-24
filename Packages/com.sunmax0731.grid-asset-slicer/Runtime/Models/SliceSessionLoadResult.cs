using System;
using System.Collections.Generic;

namespace Sunmax.GridAssetSlicer
{
    public sealed class SliceSessionLoadResult
    {
        public SliceSessionLoadResult(GridSliceSession session, IReadOnlyList<string> errors)
        {
            Session = session;
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }

        public GridSliceSession Session { get; }

        public IReadOnlyList<string> Errors { get; }

        public bool IsValid => Errors.Count == 0;
    }
}
