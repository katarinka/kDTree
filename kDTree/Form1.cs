using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using kDTree.Structure;

namespace kDTree
{
    public partial class Form1 : Form
    {
        private List<Point> D;
        private System.Drawing.Graphics formGraphics;
        private KDTree tree;
        private Point rectFrom;
        private Boolean down;
        int defK = 1;

        public Form1()
        {
            InitializeComponent();
            formGraphics = this.CreateGraphics();
            tree = new KDTree(formGraphics);
            D = new List<Point>();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                rectFrom = new Point(e.X, e.Y);
                down = true;
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Middle) && (D.Count > 0))
            {
                Point rectTo = new Point(e.X, e.Y);
                Point trueRectFrom = new Point();
                Point trueRectTo = new Point();
                trueRectFrom.X = Math.Min(rectFrom.X, rectTo.X);
                trueRectFrom.Y = Math.Min(rectFrom.Y, rectTo.Y);
                trueRectTo.X = Math.Max(rectFrom.X, rectTo.X);
                trueRectTo.Y = Math.Max(rectFrom.Y, rectTo.Y);
                tree.findInside1(trueRectFrom, trueRectTo);
                tree.drawRect(trueRectFrom, trueRectTo);
                //tree.repaint();
            }
        }
        //mouse move - kreslenie obdlznika
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            tree.notHighlight1();
            if (down)
            {
                Point rectTo = new Point(e.X, e.Y);
                Point trueRectFrom = new Point();
                Point trueRectTo = new Point();
                trueRectFrom.X = Math.Min(rectFrom.X, rectTo.X);
                trueRectFrom.Y = Math.Min(rectFrom.Y, rectTo.Y);
                trueRectTo.X = Math.Max(rectFrom.X, rectTo.X);
                trueRectTo.Y = Math.Max(rectFrom.Y, rectTo.Y);
                //tree.repaint();
                tree.drawRect(trueRectFrom, trueRectTo);
            }
        }

        private void addPoint(int x, int y)
        {
            D.Add(new Point(x, y));
            tree.insertLeaf(x, y);

        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            // highlight = false pre vsetky
            tree.notHighlight1();

            // hladanie k najblizsich susedov
            if (e.Button == MouseButtons.Right)
            {
                if (D.Count > 0)
                {
                    tree.findNearest1(e.X, e.Y);
                    tree.repaint();
                    tree.drawCircle(e.X, e.Y);
                }
            }
            if (e.Button == MouseButtons.Left)
            {
                if (Control.ModifierKeys == Keys.Shift)
                {
                    //Console.WriteLine("hladaju sa susedia");
                    tree.findNeighbors1(e.X, e.Y);
                    tree.repaint();
                } else {
                    addPoint(e.X, e.Y);
                    tree.repaint();
                }
            }
            
            down = false;
        }



        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Convert.ToInt32(textBox1.Text);
            }
            catch
            {
                textBox1.Text = Convert.ToString(tree.getNumNearest());
                tree.setNumNearest(defK);

            }
            finally
            {
                int k = Convert.ToInt32(textBox1.Text);
                if (k > 0)
                {
                    tree.setNumNearest(k);
                    label1.Text = "k = " + textBox1.Text;
                } else {
                    textBox1.Text = Convert.ToString(tree.getNumNearest());
                }
            }
        }

        private void inputN_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)System.Windows.Forms.Keys.Enter)
            {
                button1_Click(sender, e);
            }
        }
    }
}
