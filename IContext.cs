using System.Drawing;

namespace ng
{
    public interface IContext
    {
        System.Drawing.Graphics Graphics { get; }
        Pen Pen { get; }
        Brush Brush { get; }
        Brush SelectedBrush { get; }
        Brush HighlitedBrush { get; }

        PointF Transform(Point p);
    }
}
