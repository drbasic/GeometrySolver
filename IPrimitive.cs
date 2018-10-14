namespace ng
{
    public interface IPrimitive
    {
        void Draw(IContext context);

        Point[] Intersect(IPrimitive other);
        Point[] IntersectLine(Line other);
        Point[] IntersectCircle(Circle other);

        bool HitTest(IContext context, int x, int y);
    }
}
