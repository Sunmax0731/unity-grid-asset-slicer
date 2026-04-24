using System;

namespace Sunmax.GridAssetSlicer
{
    public readonly struct CellRect : IEquatable<CellRect>
    {
        public CellRect(CellCoordinate coordinate, int x, int y, int width, int height)
        {
            Coordinate = coordinate;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public CellCoordinate Coordinate { get; }

        public int X { get; }

        public int Y { get; }

        public int Width { get; }

        public int Height { get; }

        public int Right => X + Width;

        public int Bottom => Y + Height;

        public bool Equals(CellRect other)
        {
            return Coordinate.Equals(other.Coordinate)
                   && X == other.X
                   && Y == other.Y
                   && Width == other.Width
                   && Height == other.Height;
        }

        public override bool Equals(object obj)
        {
            return obj is CellRect other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Coordinate.GetHashCode();
                hashCode = (hashCode * 397) ^ X;
                hashCode = (hashCode * 397) ^ Y;
                hashCode = (hashCode * 397) ^ Width;
                hashCode = (hashCode * 397) ^ Height;
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{Coordinate}: x={X}, y={Y}, w={Width}, h={Height}";
        }
    }
}
