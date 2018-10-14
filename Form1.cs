using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        private readonly Graphics gr_;
        private readonly Context context_;
        private Point offset_ = new Point(0, 0);
        private double zoom_ = 1.0;
        private Point mouse_down_at_ = new Point(0, 0);
        private ng.PrimitiveList primitiveList_ = new ng.PrimitiveList();
        private ng.Point last_hilighted_ = null;
        private List<ng.Line> new_lines_;
        private List<ng.Line>.Enumerator new_lines_iterator_;

        public Form1()
        {
            InitializeComponent();
            offset_.X = panel1.Width / 2;
            offset_.Y = panel1.Height / 2;
            gr_ = panel1.CreateGraphics();
            context_ = new Context(gr_, GetTransformMatrix());

            MouseWheel += new MouseEventHandler(panel1_MouseWheel);
        }

        private void PaintPrimitives()
        {
            gr_.Clear(Color.White);
            StringBuilder builder = new StringBuilder();
            foreach (var a in primitiveList_.Primitives)
            {
                a.Draw(context_);
                builder.AppendLine(a.ToString());
            }
            textBox1.Text = builder.ToString();
        }

        private Matrix GetTransformMatrix()
        {
            Matrix myMatrix = new Matrix();
            myMatrix.Scale(1, -1);
            myMatrix.Scale((float)zoom_, (float)zoom_, MatrixOrder.Append);
            myMatrix.Translate(offset_.X, offset_.Y, MatrixOrder.Append);
            return myMatrix;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            bool need_full_repaint = false;

            if (e.Button.HasFlag(MouseButtons.Left))
            {
                offset_.X += e.X - mouse_down_at_.X;
                offset_.Y += e.Y - mouse_down_at_.Y;
                need_full_repaint = true;
            }

            if (e.Button.HasFlag(MouseButtons.Right))
            {
            }

            if (need_full_repaint)
            {
                context_.SetTransformMatrix(GetTransformMatrix());
                mouse_down_at_.X = e.X;
                mouse_down_at_.Y = e.Y;
                PaintPrimitives();
            }

            ng.Point p = primitiveList_.Find(context_, e.Location);
            if (p != last_hilighted_)
            {
                if (last_hilighted_ != null)
                {
                    last_hilighted_.IsHilighted = false;
                    last_hilighted_.Draw(context_);
                }
                if (p != null)
                {
                    p.IsHilighted = true;
                    p.Draw(context_);
                }
                last_hilighted_ = p;
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            mouse_down_at_.X = e.X;
            mouse_down_at_.Y = e.Y;
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            ng.Point p = primitiveList_.Find(context_, e.Location);
            if (p != null && e.Button.HasFlag(MouseButtons.Left))
            {
                p.IsSelected = !p.IsSelected;
                p.Draw(context_);
            }
        }

        private void panel1_MouseWheel(object sender, MouseEventArgs e)
        {
            zoom_ *= e.Delta > 0 ? 1.05 : 0.95;
            context_.SetTransformMatrix(GetTransformMatrix());
            PaintPrimitives();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ng.Line l1 = new ng.Line(new ng.Point(-50, 0, false), new ng.Point(50, 0, false));
            ng.Circle c1 = new ng.Circle(new ng.Point(0, 0), 10);

            primitiveList_.AddLine(l1);
            primitiveList_.AddCircle(c1);

            PaintPrimitives();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            primitiveList_.AddAllLines();
            PaintPrimitives();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            primitiveList_.AddAllCircles();
            PaintPrimitives();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            primitiveList_.EnlargeAllLines();
            PaintPrimitives();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            primitiveList_.Clear();
            PaintPrimitives();
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            if (new_lines_ == null)
            {
                new_lines_ = primitiveList_.GetNewLines();
                new_lines_iterator_ = new_lines_.GetEnumerator();
            }
            if (new_lines_iterator_.Current != null)
            {
                primitiveList_.RemoveLine(new_lines_iterator_.Current);
            }
            if (new_lines_iterator_.MoveNext())
            {
                primitiveList_.AddLine(new_lines_iterator_.Current);
            }
            else
            {
                new_lines_ = null;
                new_lines_iterator_.Dispose();
            }
            PaintPrimitives();
        }
    }
}
