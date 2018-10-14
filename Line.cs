using System;

namespace ng
{
    public class Line : IPrimitive
    {
        public Line(Point p1, Point p2)
        {
            this.P1 = p1;
            this.P2 = p2;
        }

        public bool IsSame(Line line)
        {
            return (P1 == line.P1 && P2 == line.P2) ||
                (P1 == line.P2 && P2 == line.P1);
        }

        public  bool IsCollinear(Line l2)
        {
            var a = Classify(l2.P1);
            var b = Classify(l2.P2);
            return (a == 0 && b == 0);
        }

        public int Classify(Point p)
        {
            Point a = p - P1;
            Point b = P2 - P1;
            double s = (a.X * b.Y) - (b.X * a.Y);
            if (Math.Abs(s) < Point.Epsilon)
                return 0;
            else if (s < 0)
                return -1;
            return 1;
        }

        void IPrimitive.Draw(IContext context)
        {
            var p1 = context.Transform(P1);
            var p2 = context.Transform(P2);
            context.Graphics.DrawLine(context.Pen, p1.X + 0.5f, p1.Y + 0.5f, p2.X + 0.5f, p2.Y + 0.5f);
        }

        public Point[] Intersect(IPrimitive other)
        {
            return other.IntersectLine(this);
        }

        public Point[] IntersectLine(Line line)
        {
            Func<double, double, double, double, double> det = (double a, double b, double c, double d) => (a * d) - (b * c);

            double zn = det(A, B, line.A, line.B);
            if (Math.Abs(zn) < Point.Epsilon)
                return null;

            double x = det(C, B, line.C, line.B) / zn;
            double y = det(A, C, line.A, line.C) / zn;
            return new Point[] { new Point(x, y) };
        }

        public Point[] IntersectCircle(Circle other)
        {
            return other.IntersectLine(this);
        }

        public bool HitTest(IContext context, int x, int y)
        {
            return false;
        }

        public override string ToString()
        {
            return string.Format("line ({0})-({1})", P1.ToString(), P2.ToString());
        }

        public Point P1 { get; set; }
        public Point P2 { get; set; }
        public double A => P2.Y - P1.Y;
        public double B => P1.X - P2.X;
        public double C => A * P1.X + B * P1.Y;
        public double Length2 => (P1.X - P2.X) * (P1.X - P2.X) + (P1.Y - P2.Y) * (P1.Y - P2.Y);
    }
}
