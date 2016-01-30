using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using kDTree.Graphics;

namespace kDTree.Structure
{
    public class KDNode
    {
        private NodePaintInterface painter;
        // ak true, je to listovy vrchol, takze ma nastaveny Point, 
        // inak je to rozdelovaci vrchol a ma nastaveny split a dim
        public Boolean isLeaf; 
        private int split; // hodnota rozdelovacej suradnice
        private int dim;  // suradnica - 0 pre x, 1 pre y
        private Point point; // suradnice klikuteho vrchola - leaf
        private KDNode left; // vsetky, co su voci split v dim mensie rovne
        private KDNode right; // vsetky, co su voci split v dim vacsie
        private KDNode parent; 
        // ohranicenie bunky, v ktorej sa nachadza vrchol
        public Point from; // min x y
        public Point to; // max x y
        // pocet listov, ktore su pod nim. - sluzi na vybratie podstromu, 
        // ktory sa ma upravit pri pridani noveho vrchola 
        public int numLeafs;
        // velkost plochy
        int width;
        int height;
        public double dist = 10000; // vzdialenost od target pri kladani 'k' najblizsich

        // booleany na zvyraznenie vrcholov
        public Boolean highlight; // prepocitane
        public Boolean highlightRed; // k najblizsich
        public Boolean highligthBox; // susedne bunky
        public Boolean highligthIn; // vo vnutri obdlznika


        // konstruktor pre listovy vrchol
        public KDNode(int x, int y, KDNode parent, System.Drawing.Graphics g)
        {
            isLeaf = true;
            point = new Point(x,y);
            this.parent = parent;
            dim = -1000;
            split = -1000;
            left = null;
            right = null;
            //width = Form1.ActiveForm.Width; 
            //height = Form1.ActiveForm.Height; 
            //Console.WriteLine(width + " " + height);
            width = 900;
            height = 600;
            from = new Point();
            to = new Point();
            setFromTo();
            numLeafs = 1;

            highlight = false;
            highlightRed = false;
            highligthBox = false;
            highligthIn = false;
            painter = new PointPainter(g);
        }

        // konstruktor pre vnutorny vrchol
        public KDNode(Boolean isSplit, int c, int dim, KDNode parent, System.Drawing.Graphics g) //Boolean isLeft, 
        {
            isLeaf = false;
            this.dim = dim;
            split = c;
            this.parent = parent;
            point = new Point(-1000,-1000);
            left = null;
            right = null;
            //width = Form1.ActiveForm.Width; 
            //height = Form1.ActiveForm.Height; 
            //Console.WriteLine(width + " " + height);
            width = 900;
            height = 600;
            from = new Point();
            to = new Point();
            setFromTo();
            numLeafs = 1;

            highlight = false;
            highlightRed = false;
            highligthBox = false;
            highligthIn = false;
            painter = new SepLinePainter(g);
        }

        // inicializacny konstruktor
        public KDNode(System.Drawing.Graphics g) 
        {
            isLeaf = true;
            this.dim = -1;
            split = -1000;
            this.parent = null;
            point = new Point(-1000, -1000);
            left = null;
            right = null;
            //isLeftUp = isLeft;
            //width = Form1.ActiveForm.Width; 
            //height = Form1.ActiveForm.Height; 
            //Console.WriteLine(width + " " + height);
            width = 900;
            height = 600;
            from = new Point(-1000, -1000);
            to = new Point(-1000, -1000);
            //setFromTo();
            numLeafs = 1;

            highlight = false;
            highlightRed = false;
            highligthBox = false;
            highligthIn = false;
            painter = new SepLinePainter(g);
        }

        // nastavenie ohranicenia bunky pre vrchol --
        // predpokladame, ze parent ma nastavene ohranicenie spravne.
        public void setFromTo()
        {
            if (parent == null) // je to root, alebo jeho From a To zatial nemaju zmysel
            {
                from.X = 0;
                from.Y = 0;
                to.X = width;
                to.Y = height;
                return;
            }

            Boolean isLeftUp = (parent.left.Equals(this));
            
            if (isLeftUp) // je to lavy syn
            {
                from = parent.getFrom();
                if (parent.getDim() == 0)   // je vlavo
                {
                    to.X = parent.getSplit();
                    to.Y = parent.getTo().Y;
                } else {    // je hore
                    to.X = parent.getTo().X;
                    to.Y = parent.getSplit();
                }
            } else { // je to pravy syn
                to = parent.getTo();
                if (parent.getDim() == 0)   // je vpravo
                {
                    from.X = parent.getSplit();
                    from.Y = parent.getFrom().Y;
                } else {    // je dole
                    from.X = parent.getFrom().X;
                    from.Y = parent.getSplit();
                }
            }
        
        }

        public void repaint()
        {
            painter.repaint(this);
        }



    #region Getters Setters 

        public KDNode getLeft()
        {
            return left;
        }

        public KDNode getRight()
        {
            return right;
        }

        public void setLeft(KDNode n) 
        {
            left = n;
        }

        public void setRight(KDNode n)
        {
            right = n;
        }

        public KDNode getParent()
        {
            return parent;
        }

        public void setParent(KDNode n)
        {
            parent = n;
            //ak sa zmeni rodic, zmenia sa aj ohranicenia
            //setFromTo();
        }

        public Point getPoint()
        {
            return this.point;
        }

        public void setPoint(Point p)
        {
            this.point.X = p.X;
            this.point.Y = p.Y;
        }

        public int getSplit()
        {
            return split;
        }

        public void setSplit(int s)
        {
            this.split = s;
        }

        public int getDim()
        {
            return dim;
        }

        public void setDim(int dim)
        {
            this.dim = dim;
        }

        public Point getFrom()
        {
            return from;
        }

        public void setFrom(Point from)
        {
            this.from = from;
        }

        public Point getTo()
        {
            return to;
        }

        public void setTo(Point to)
        {
            this.to = to;
        }

        public int getNumLeafs()
        {
            return numLeafs;
        }

        public void setNumLeafs(int num)
        {
            this.numLeafs = num;
        }

    #endregion
    }


    public class KDNodeComparer: IComparer<KDNode> {
        public int Compare(KDNode x, KDNode y)
        {
            if (x.dist < y.dist)
            {
                return -1;
            }
            if (x.dist == y.dist){
                return 0;
            }
            if (x.dist > y.dist)
            {
                return 1;
            }
            return -10;
        }
    }
}
