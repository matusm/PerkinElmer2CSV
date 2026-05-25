using System;

namespace PerkinElmerSP2CSV
{
    public struct Point2d : IComparable<Point2d>
    {
        public double X;
        public double Y;

        public int CompareTo(Point2d other) => X.CompareTo(other.X);
    }
}
