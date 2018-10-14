using System;

namespace ng
{
    public class Circle : IPrimitive
    {
        public Circle(Point p, double r)
        {
            this.P = p;
            this.R = r;
        }

        void IPrimitive.Draw(IContext context)
        {
            var p1 = context.Transform(P - new Point(R, R));
            var p2 = context.Transform(P + new Point(R, R));
            context.Graphics.DrawEllipse(context.Pen, p1.X, p1.Y, p2.X - p1.X, p2.Y - p1.Y);
        }

        internal bool IsSame(Circle other)
        {
            return (P == other.P) && Math.Abs(R - other.R) < Point.Epsilon;
        }

        public Point[] Intersect(IPrimitive other)
        {
            return other.IntersectCircle(this);
        }

        public Point[] IntersectLine(Line other)
        {
            double x1 = other.P1.X - P.X;
            double x2 = other.P2.X - P.X;
            double y1 = other.P1.Y - P.Y;
            double y2 = other.P2.Y - P.Y;

            double a = y2 - y1;
            double b = x1 - x2;
            double c = a * x1 + b * y1;

            double a2b2 = a * a + b * b;
            double x0 = a * c / a2b2;
            double y0 = b * c / a2b2;

            double rr = R * R;
            double rr2 = x0 * x0 + y0 * y0;
            if (rr2 > rr)
                return null;

            double d = rr - (c * c) / a2b2;
            double mult = Math.Sqrt(d / (a * a + b * b));

            Point p1 = new Point(P.X + x0 + b * mult, P.Y + y0 - a * mult);
            Point p2 = new Point(P.X + x0 - b * mult, P.Y + y0 + a * mult);

            if (p1.IsSame(p2))
                return new Point[] { p1 };
            return new Point[] { p1, p2 };
        }

        public Point[] IntersectCircle(Circle other)
        {
            if (P == other.P)
                return null;

            //double x1 = P.X;
            double x2 = other.P.X - P.X;
            //double y1 = P.Y;
            double y2 = other.P.Y - P.Y;
            double r1 = R;
            double r2 = other.R;

            double a = -2.0 * x2;
            double b = -2.0 * y2;
            double c = x2 * x2 + y2 * y2 + r1 * r1 - r2 * r2;

            double a2b2 = a * a + b * b;
            double x0 = a * c / a2b2;
            double y0 = b * c / a2b2;

            double rr = r1 * r1;
            double rr2 = x0 * x0 + y0 * y0;
            if (rr2 > rr)
                return null;

            double d = rr - (c * c) / a2b2;
            double mult = Math.Sqrt(d / (a * a + b * b));

            Point p1 = new Point(P.X - x0 + b * mult, P.Y - y0 - a * mult);
            Point p2 = new Point(P.X - x0 - b * mult, P.Y - y0 + a * mult);

            if (p1.IsSame(p2))
                return new Point[] { p1 };
            return new Point[] { p1, p2 };
        }

        public bool HitTest(IContext context, int x, int y)
        {
            return false;
        }

        public Point P { get; set; }
        public double R { get; set; }
    }
}
