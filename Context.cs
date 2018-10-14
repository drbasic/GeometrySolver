using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ng;

namespace WindowsFormsApp2
{
    public class Context : IContext
    {
        public Context(Graphics gr, Matrix m)
        {
            graphics_ = gr;
            pen_ = new Pen(Color.Black);
            brush_ = Brushes.Black;
            selected_brush_ = Brushes.Red;
            highlited_brush_ = Brushes.Purple;
            matrix_ = m;
        }
        Graphics IContext.Graphics => graphics_;

        Pen IContext.Pen => pen_;

        Brush IContext.Brush => brush_;

        public Brush SelectedBrush => selected_brush_;

        public Brush HighlitedBrush => highlited_brush_;

        public void SetTransformMatrix(Matrix matrix)
        {
            matrix_ = matrix;
        }

        Graphics graphics_;
        Pen pen_;
        readonly Brush brush_;
        readonly Brush selected_brush_;
        readonly Brush highlited_brush_;
        Matrix matrix_;

        public System.Drawing.PointF Transform(ng.Point p)
        {
            PointF[] result = { new PointF((float)p.X, (float)p.Y) };
            matrix_.TransformPoints(result);
            return result[0];
        }
    }
}
