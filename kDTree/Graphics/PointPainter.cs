using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using kDTree.Structure;

namespace kDTree.Graphics
{
    class PointPainter : NodePaintInterface
    {
        System.Drawing.Graphics g;
        const int RADIUS = 3;
        public PointPainter(System.Drawing.Graphics g)
        {
            this.g = g;
        }

        void NodePaintInterface.repaint(KDNode n) 
        {
            //Console.WriteLine("(" + n.getPoint().X.ToString() + "," + n.getPoint().Y.ToString() + ")");
            repaintBoxes(n);
            repaintNodes(n);

        }



        // prekresli sa vrchol
        private void repaintNodes(KDNode n)
        {
            System.Drawing.Font font = new System.Drawing.Font("Verdana", 10);
            System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
            System.Drawing.Pen myPen = new System.Drawing.Pen(System.Drawing.Color.Black);

            if (n.highlight)
            {
                myBrush.Color = System.Drawing.Color.Green;
                myPen.Color = System.Drawing.Color.Green;
            }

            if (n.highlightRed)
            {
                myBrush.Color = System.Drawing.Color.Red;
                myPen.Color = System.Drawing.Color.Red;
            }
            if (n.highligthIn)
            {
                myBrush.Color = System.Drawing.Color.Red;
                myPen.Color = System.Drawing.Color.Red;
                g.DrawEllipse(myPen, n.getPoint().X - (RADIUS + 2), n.getPoint().Y - (RADIUS + 2), 2 * (RADIUS + 2), 2 * (RADIUS + 2));
                myPen.Color = System.Drawing.Color.Black;
            }

            g.FillEllipse(myBrush, n.getPoint().X - RADIUS, n.getPoint().Y - RADIUS, 2*RADIUS, 2*RADIUS);

            string s = "(" + n.getPoint().X.ToString() + "," + n.getPoint().Y.ToString() + ")";
            g.DrawString(s, font, myBrush, n.getPoint().X - 35, n.getPoint().Y);
        }

        // prekresli sa bunka vrchola
        private void repaintBoxes(KDNode n)
        {

            if (n.isLeaf)
            {
                System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                System.Drawing.Pen myPen = new System.Drawing.Pen(System.Drawing.Color.Black);
                if (n.highligthBox)
                {
                    myBrush.Color = System.Drawing.Color.LightSteelBlue;
                    g.FillRectangle(myBrush, n.from.X, n.from.Y, n.to.X - n.from.X, n.to.Y - n.from.Y);
                }
                //Console.WriteLine("vrchol: " + n.getPoint() + " from: " + n.from + " to: " + n.to);
                g.DrawRectangle(myPen, n.from.X, n.from.Y, n.to.X - n.from.X, n.to.Y - n.from.Y);
            } else {
                //Console.WriteLine("vrchol: (" + n.getSplit() + " from: " + n.from + " to: " + n.to);
            }
        }
    }
}
