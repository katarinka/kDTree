using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using kDTree.Structure;

namespace kDTree.Graphics
{
    // v skutocnosti sa nevykresluju oddelovacie ciary, ale bunky
    // tato trieda sa nepouziva
    class SepLinePainter : NodePaintInterface
    {
        System.Drawing.Graphics g;
        int width;
        int height;

        public SepLinePainter(System.Drawing.Graphics g)
        {
            this.g = g;
            //width = Form1.ActiveForm.Width;
            //height = Form1.ActiveForm.Height;
        }

        void NodePaintInterface.repaint(KDNode n)
        {
            /*
            System.Drawing.Font font = new System.Drawing.Font("Verdana", 10);
            System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
            System.Drawing.Pen myPen = new System.Drawing.Pen(System.Drawing.Color.Black);

            if (n.highlight)
            {
                myBrush.Color = System.Drawing.Color.Green;
                myPen.Color = System.Drawing.Color.Green;
            }

            if (n.getDim() == 0)
            {
                g.DrawLine(myPen, n.getSplit(), n.from, n.getSplit(), n.to);
            } else {
                g.DrawLine(myPen, n.from, n.getSplit(), n.to, n.getSplit());
            }
            

            string s = "(" + n.getPoint().X.ToString() + "," + n.getPoint().Y.ToString() + ")";
            g.DrawString(s, font, myBrush, n.getPoint().X - 35, n.getPoint().Y);
            */
        }
    }
}
