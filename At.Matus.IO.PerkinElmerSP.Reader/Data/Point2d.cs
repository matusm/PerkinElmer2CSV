using System;

namespace At.Matus.IO.PerkinElmerSP.Reader
{
    public struct Point2d : IComparable<Point2d>
    {
        public double X;
        public double Y;

        public int CompareTo(Point2d other) => X.CompareTo(other.X);
    }
}
