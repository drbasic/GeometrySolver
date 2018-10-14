using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace ng
{

    public class Point : IPrimitive
    {
        public static double Epsilon => 0.00001;
        public static Int64 EpsilonInv => (Int64)(1.0 / Epsilon);
        public static float DrawSize => 5;

        public Point(double x, double y, bool bindable = true)
        {
            X = x;
            Y = y;
            IsBindable = bindable;
        }

        public double X { get; set; }
        public double Y { get; set; }
        public bool IsBindable { get; set; } = true;
        public bool IsSelected { get; set; } = false;
        public bool IsHilighted { get; set; } = false;

        public bool IsSame(Point b)
        {
            return DefaultCmp.Equals(this, b);
        }

        public void Draw(IContext context)
        {
            if (!IsBindable)
                return;
            PointF a = context.Transform(this);
            context.Graphics.FillRectangle(
                IsSelected ? context.SelectedBrush : IsHilighted ? context.HighlitedBrush : context.Brush,
                a.X - (DrawSize / 2),
                a.Y - (DrawSize / 2),
                DrawSize,
                DrawSize);
        }

        public Point[] Intersect(IPrimitive other)
        {
            return null;
        }

        public Point[] IntersectLine(Line other)
        {
            return null;
        }

        public Point[] IntersectCircle(Circle other)
        {
            return null;
        }

        public double Distance(Point other)
        {
            return Math.Sqrt((X - other.X) * (X - other.X) + (Y - other.Y) * (Y - other.Y));
        }

        public bool HitTest(IContext context, int x, int y)
        {
            if (!IsBindable)
                return false;
            PointF a = context.Transform(this);
            return (Math.Abs(a.X - x) < DrawSize/2) && (Math.Abs(a.Y - y) < DrawSize/2);
        }

        public override string ToString()
        {
            return string.Format("point {0};{1}", X, Y);
        }

        public static Point operator-(Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y);
        }
        public static Point operator +(Point a, Point b)
        {
            return new Point(a.X + b.X, a.Y + b.Y);
        }

        private static PointInt64Cmp DefaultCmp = new PointInt64Cmp();
    }

    class PointInt64Cmp : EqualityComparer<Point>
    {
        public override bool Equals(Point a, Point b)
        {
            if (a == null && b == null)
                return true;
            else if (a == null || b == null)
                return false;
            return (Int64)(a.X * Point.EpsilonInv) == (Int64)(b.X * Point.EpsilonInv) &&
                   (Int64)(a.Y * Point.EpsilonInv) == (Int64)(b.Y * Point.EpsilonInv);
        }

        public override int GetHashCode(Point p)
        {
            Int64 x = (Int64)(p.X * Point.EpsilonInv) + (Int64)(p.X * Point.EpsilonInv);
            return x.GetHashCode();
        }
    }
}
