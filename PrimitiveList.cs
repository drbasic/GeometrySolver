using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using WindowsFormsApp2;

namespace ng
{
    public class PrimitiveList
    {
        public void Add(Point p)
        {
            if (points_.TryGetValue(p, out int counter))
            {
                points_[p] = counter + 1;
            }
            else
            {
                points_.Add(p, 1);
                Primitives.Add(p);
            }
        }

        public void Remove(Point p)
        {
            if (points_.TryGetValue(p, out int counter) && counter > 0)
            {
                if (counter == 1)
                {
                    points_.Remove(p);
                }
                else
                {
                    --points_[p];
                }
            }
            else
            {
                throw new Exception("Point not found");
            }
        }

        public void AddLine(Line line)
        {
            AddIntersections(line);

            Primitives.Add(line);
            Primitives.Add(line.P1);
            Primitives.Add(line.P2);
        }

        public void RemoveLine(Line line)
        {
            RemoveIntersections(line);

            Primitives.Remove(line);
            Primitives.Remove(line.P1);
            Primitives.Remove(line.P2);
        }

        public void AddCircle(Circle circle)
        {
            AddIntersections(circle);

            Primitives.Add(circle);
            Primitives.Add(circle.P);
        }

        public void AddAllLines()
        {
            var lines_to_add = GetNewLines();
            foreach (var line in lines_to_add)
                AddLine(line);

            EnlargeAllLines();
        }

        public void AddAllCircles()
        {
            var circles_to_add = new List<Circle>();
            for (var ii = points_.GetEnumerator(); ii.MoveNext();)
            {
                if (!ii.Current.Key.IsBindable)
                    continue;
                for (var jj = ii; jj.MoveNext();)
                {
                    if (!jj.Current.Key.IsBindable)
                        continue;
                    if (ii.Current.Key == jj.Current.Key)
                        continue;
                    Circle circle = new Circle(ii.Current.Key, ii.Current.Key.Distance(jj.Current.Key));
                    if (!HasCircle(circle))
                        circles_to_add.Add(circle);
                    circle = new Circle(jj.Current.Key, ii.Current.Key.Distance(jj.Current.Key));
                    if (!HasCircle(circle))
                        circles_to_add.Add(circle);
                }
            }
            foreach (var circle in circles_to_add)
                AddCircle(circle);
            EnlargeAllLines();
        }

        public Point Find(Context context, System.Drawing.Point point_at)
        {
            foreach(var p in Primitives)
            {
                if (p.HitTest(context, point_at.X, point_at.Y))
                    return p as Point;
            }
            return null;
        }

        public void EnlargeAllLines()
        {
            var lines_to_add = new List<Line>();
            var lines_to_remove = new List<Line>();
            foreach (var p in Primitives)
            {
                var l = p as Line;
                if (l == null)
                    continue;
                for (var ii = points_.GetEnumerator(); ii.MoveNext();)
                {
                    if (l.P1 == ii.Current.Key || l.P2 == ii.Current.Key)
                        continue;
                    if (l.Classify(ii.Current.Key) == 0)
                    {
                        Line line1 = new Line(ii.Current.Key, l.P1);
                        Line line2 = new Line(ii.Current.Key, l.P2);
                        Line long_line = line1.Length2 > line2.Length2 ? line1 : line2;
                        if (l.Length2 > long_line.Length2)
                            continue;

                        lines_to_add.Add(long_line);
                        lines_to_remove.Add(l);
                    }
                }
            }
            foreach (var line in lines_to_remove)
                Primitives.Remove(line);
            foreach (var line in lines_to_add)
                Primitives.Add(line);
        }

        internal List<Line> GetNewLines()
        {
            List<Line> result = new List<Line>();
            for (var ii = points_.GetEnumerator(); ii.MoveNext();)
            {
                for (var jj = ii; jj.MoveNext();)
                {
                    if (ii.Current.Key == jj.Current.Key)
                        continue;
                    Line line = new Line(ii.Current.Key, jj.Current.Key);
                    if (!HasLine(line))
                        result.Add(line);
                }
            }
            return result;
        }

        internal void Clear()
        {
            Primitives.Clear();
            points_.Clear();
        }

        private bool HasLine(Line line)
        {
            foreach(var p in Primitives)
            {
                var l = p as Line;
                if (l == null)
                    continue;
                if (l.IsSame(line))
                    return true;
                if (l.IsCollinear(line))
                    return true;
            }
            return false;
        }

        private bool HasCircle(Circle circle)
        {
            foreach (var p in Primitives)
            {
                var l = p as Circle;
                if (l == null)
                    continue;
                if (l.IsSame(circle))
                    return true;
            }
            return false;
        }

        private void AddIntersections(IPrimitive primitive)
        {
            List<Point> added = null;
            foreach (var a in Primitives)
            {
                Point[] points = a.Intersect(primitive);
                if (points != null)
                {
                    if (added == null)
                        added = new List<Point>();
                    added.AddRange(points);
                }
            }
            if (added != null)
            {
                foreach (var p in added)
                    Add(p);
            }
        }

        private void RemoveIntersections(IPrimitive primitive)
        {
            List<Point> found = new List<Point>();
            foreach (var a in Primitives)
            {
                Point[] points = a.Intersect(primitive);
                if (points != null)
                {
                    found.AddRange(points);
                }
            }
            foreach (var p in found)
                Remove(p);
        }

        public bool Contains(Point p)
        {
            return points_.TryGetValue(p, out int counter) && counter > 0;
        }

        public List<IPrimitive> Primitives { get; } = new List<ng.IPrimitive>();

        private readonly Dictionary<Point, Int32> points_ = new Dictionary<Point, int>(new PointInt64Cmp());
    }
}
