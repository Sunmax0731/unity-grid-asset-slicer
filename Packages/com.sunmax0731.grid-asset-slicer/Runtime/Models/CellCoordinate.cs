using System;

namespace Sunmax.GridAssetSlicer
{
    public readonly struct CellCoordinate : IEquatable<CellCoordinate>
    {
        public CellCoordinate(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public int Row { get; }

        public int Column { get; }

        public bool Equals(CellCoordinate other)
        {
            return Row == other.Row && Column == other.Column;
        }

        public override bool Equals(object obj)
        {
            return obj is CellCoordinate other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Row * 397) ^ Column;
            }
        }

        public override string ToString()
        {
            return $"({Row}, {Column})";
        }
    }
}
