using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using kDTree.Graphics;
using System.Drawing;


namespace kDTree.Structure
{
    class KDTree
    {
        KDNode root;
        System.Drawing.Graphics g;
        // velkost plochy
        int width;
        int height;

        List<KDNode> neighbors; // zoznam pouzivany pri hladani susednych buniek
        List<KDNode> inside; // zoznam pouzivany pri hladani vrcholov vo vnurti obdlznika
        SortedSet<KDNode> nearestK; // zoznam pouzivany pri hladani 'k' najblizsich vrcholov
        int numNearest; // = 'k' pri hladani 'k' najblizsich vrcholov

        public KDTree(System.Drawing.Graphics g)
        {
            root = null;
            this.g = g;
            width = 900;
            height = 600;
            numNearest = 1;
        }

        # region Inserting
        /* Insert funguje tak, ze ked osetrime okrajove pripady, 
         * rekurzivne hladame podstrom (porovnavanim n.point so subtree.split), 
         * do ktoreho sa ma n vlozit. No nejdeme az po List, ale skoncime v takom
         * koreni podstromu, v ktorom by bola narusena rovnovaha, keby sa do jeho syna
         * vlozil n (zistime to porovnanim left.numLeafs a right.numLeafs). 
         * Vieme, ze n patri do tohto podstromu, ale kedze jeho vlozenim by
         * bola narusena rovnovaha tak cely tento podstrom prebudujeme.
         */
        public void insertLeaf(int x, int y)
        {
            // vkladame vrchol n
            KDNode n = new KDNode(x, y, null, g);
            // pripad, ked je strom prazdny
            if (root == null)
            {
                root = n;
            } else {
                // neda sa pridat vrchol na to iste miesto
                if (alreadyIn(n)) {
                    return;
                }
                // najde sa miesto pre n -> k je koren podstromu, ktoreho rovnovaha by bola narusena
                KDNode k = findPlace(root, n);
                //Console.WriteLine(k.getSplit());
                if (k.getParent() != null) {
                    //Console.WriteLine(k.getParent().getNumLeafs());
                }
                // nastavi sa, ci je k lavym alebo pravym synom svojho otca
                Boolean isLeftUp = false;
                if (!k.Equals(root))
                {
                    isLeftUp = k.Equals(k.getParent().getLeft());
                }

                List<KDNode> l = new List<KDNode>();
                // najdu sa vsetky listy v prebudovavanom podstrome - 
                // z nich a z n sa vytvori novy vyvazeny podstrom
                l.AddRange(findLeafs(k)); 
                l.Add(n);

                // rebuilt() vracia root noveho podstromu. uz je pripojeny k parent.
                KDNode m = rebuilt(l, k.getDim(), k.getParent(), isLeftUp);
                // a ohranicenia buniek
                updateFromTo(m);
                // ak sa prebudoval cely strom, meni sa root.
                if (k.Equals(root))
                {
                    root = m;
                    updateNumLeafsDown(root);
                    //updateNumLeafsUp(root);
                } else {
                    updateNumLeafsDown(m);
                    if (isLeftUp)
                    {
                        m.getParent().setLeft(m);
                    } else {
                        m.getParent().setRight(m);
                    }
                    updateNumLeafsUp(m);
                }
                    // pocet listov sa musi preratat aj pre rodicov nad prebudovanym podstromom
                // pre kazdy vrchol z noveho podstromu sa updatene pocet listov, ktori su jeho potomkami
            }
        }

        private Boolean alreadyIn(KDNode n)
        {
            KDNode k = findBlockIn(n);
            if ((k.getPoint().X == n.getPoint().X) && (k.getPoint().Y == n.getPoint().Y)) 
            {
                return true;
            }
            return false;
        }


        //vrati podstrom, do ktoreho syna ma ist novy vrchol a bola by v nom narusena rovnovaha
        public KDNode findPlace(KDNode n, KDNode leaf) 
        {
            // ak n je list, potom sa rovnovaha nenarusi a iba sa novy vrchol vlozi.
            // z listu spravime otca dvoch listov v procedure rebuilt
            if(n.isLeaf)
            {
                //Console.Write(n.getPoint());
                //Console.WriteLine(" numLeafs " + n.getNumLeafs());
                return n;
            }
            //Console.Write(n.getSplit());
            //Console.WriteLine(" numLeafs right" + n.getRight().getNumLeafs());
            //Console.WriteLine(" numLeafs left" + n.getLeft().getNumLeafs());
            // ak to nie je list, potom hladame podstrom, ktoremu sa narusi rovnovaha.
            if (n.getDim() == 0) // split je v x-ovej suradnici
            {
                if (n.getSplit() >= leaf.getPoint().X) // ak ma ist do laveho podstromu
                {
                    // a uz teraz je vlavo viac listov ako vpravo, narusi sa rovnovaha n
                    if (n.getLeft().getNumLeafs() > n.getRight().getNumLeafs())
                    {
                        return n;
                    } else { // inak sa vrati podstrom z laveho podstromu, v ktorom je narusena rovnovaha
                        return findPlace(n.getLeft(), leaf);
                    }
                } else { // ak ma ist do praveho podstromu
                    // a uz teraz je vpravo viac listov ako vlavo, narusi sa rovnovaha n
                    if (n.getLeft().getNumLeafs() < n.getRight().getNumLeafs()) 
                    {
                        return n;
                    } else { // inak sa vrati podstrom z praveho podstromu, v ktorom je narusena rovnovaha
                        return findPlace(n.getRight(), leaf);
                    }
                }
            } else { // split je v y-ovej suradnici
                if (n.getSplit() >= leaf.getPoint().Y) // ak ma ist do laveho podstromu (= patri hore, nad split)
                {
                    // a uz teraz je vlavo viac listov ako vpravo, narusi sa rovnovaha n
                    if (n.getLeft().getNumLeafs() > n.getRight().getNumLeafs())
                    {
                        return n;
                    }
                    else
                    { // inak sa vrati podstrom z laveho podstromu, v ktorom je narusena rovnovaha
                        return findPlace(n.getLeft(), leaf);
                    }
                }
                else
                { // ak ma ist do praveho podstromu (= patri dole, pod split)
                    // a uz teraz je vpravo viac listov ako vlavo, narusi sa rovnovaha n
                    if (n.getLeft().getNumLeafs() < n.getRight().getNumLeafs())
                    {
                        return n;
                    }
                    else
                    { // inak sa vrati podstrom z praveho podstromu, v ktorom je narusena rovnovaha
                        return findPlace(n.getRight(), leaf);
                    }
                }
            }
        }


        // vrati zoznam vsetkych listov podstromu n
        public List<KDNode> findLeafs(KDNode k)
        {
            KDNode n = k;
            List<KDNode> l = new List<KDNode>();
            if (n.isLeaf)
            {
                l.Add(n);
                return l;
            }
            l.AddRange(findLeafs(n.getLeft()));
            l.AddRange(findLeafs(n.getRight()));

            return l;
        }

        // prebudovanie stromu
        // na vstupe je zoznam vsetkych buducich listov podstromu.
        public KDNode rebuilt(List<KDNode> l1, int dim1, KDNode newParent, Boolean isLeft) 
        {
            List<KDNode> l = l1;
            
            // ak je v zozname iba 1 vrchol, vrati sa ako list
            if (l.Count == 1)
            {   
                return l.ElementAt(0);
            }

            int newSplit;
            int dimension;
            if (newParent == null)
            {
                // root ma nastavenu deliacu suradnicu na x.
                dimension = 0;
            } else {
                dimension = 1 - newParent.getDim();
            }
            // vrati split noveho vrchola
            newSplit = findSplit(l, dimension);
            //Console.WriteLine(newSplit);

            List<KDNode> newLeft = new List<KDNode>();
            List<KDNode> newRight = new List<KDNode>();
            Boolean equal = true;
            //rozdelenie na 2 podstromy podla split a dim
            if (dimension == 0) //split cez x-ovu os
            {
                foreach (KDNode n in l) 
                {
                    // zoznam listov v lavom a pravom podstrome podla split
                    if (n.getPoint().X < newSplit)
                    {
                        newLeft.Add(n);
                    }
                    if (n.getPoint().X > newSplit)
                    {
                        newRight.Add(n);
                    }
                    // ak by mali rovnaku suradnicu, aby sa nepridavali len do jedneho podstromu
                    if (n.getPoint().X == newSplit)
                    {
                        if (equal){
                            newLeft.Add(n);
                            equal = false;
                        } else {
                            newRight.Add(n);
                            equal = true;
                        }
                    }
                }

            } else { //split cez y-ovu os
                foreach (KDNode n in l)
                {
                    // zoznam listov v lavom a pravom podstrome podla split
                    if (n.getPoint().Y < newSplit)
                    {
                        newLeft.Add(n);
                    }
                    if (n.getPoint().Y > newSplit)
                    {
                        newRight.Add(n);
                    }
                    // ak by mali rovnaku suradnicu, aby sa nepridavali len do jedneho podstromu
                    if (n.getPoint().Y == newSplit)
                    {
                        if (equal)
                        {
                            newLeft.Add(n);
                            equal = false;
                        } else {
                            newRight.Add(n);
                            equal = true;
                        }
                    }
                }
            }

            // vytvori sa koren noveho podstromu
            KDNode subroot = new KDNode(true, newSplit, dimension, null, this.g);
            if (newParent != null)
            {
                if (isLeft)
                {
                    newParent.setLeft(subroot);
                } else {
                    newParent.setRight(subroot);
                }
            }
            subroot.setParent(newParent);
            subroot.setLeft(rebuilt(newLeft, 1 - dimension, subroot, true));
            subroot.getLeft().setParent(subroot);
            subroot.setRight(rebuilt(newRight, 1 - dimension, subroot, false));
            subroot.getRight().setParent(subroot);

            return subroot;
        }

        // najde hodnotu, ktora rozdeli vrcholy podla dim1 suradnice na
        // dve rovnake (+-1) casti
        public int findSplit(List<KDNode> l1, int dim1)
        {
            List<KDNode> l = l1;
            int dim = dim1;

            // hladame middle a middle+1 najmensi vrchol (podla dim) a stred medzi nimi je split
            int middle = (l.Count + 1) / 2;
            
            List<KDNode> mid1 = findIthSmallest(l, middle, dim);
            List<KDNode> mid2 = findIthSmallest(l, middle + 1, dim);

            // vrcholy, ktore prestaviame su zelene
            mid1.ElementAt(0).highlight = true;
            mid2.ElementAt(0).highlight = true;
            if (dim == 0)
            {
                return ((mid2.ElementAt(0).getPoint().X - mid1.ElementAt(0).getPoint().X) / 2) + mid1.ElementAt(0).getPoint().X;
            } else {
                return ((mid2.ElementAt(0).getPoint().Y - mid1.ElementAt(0).getPoint().Y) / 2) + mid1.ElementAt(0).getPoint().Y;
            }
        }


        // hladanie vrchola s i-tou najmensou d1 suradnicou zo zoznamu l1
        public List<KDNode> findIthSmallest(List<KDNode> l1, int i1, int d1) 
        {
            List<KDNode> l = new List<KDNode>(l1);
            int i = i1;
            int d = d1;
            
            // ak je iba jeden, nie je co delit
            if (l.Count == 1)
            {
                List<KDNode> res = new List<KDNode>();
                res.Add(l.ElementAt(0));
                return res;
            }

            // podobne ako pri Quick sort, zvolime si pivot - prvy prvok v zozname
            KDNode pivot = l.ElementAt(0);
            int pivX = pivot.getPoint().X;
            int pivY = pivot.getPoint().Y;

            // co ak sa vsetky v zozname rovnaju?
            Boolean allSame = true;
            if (d == 0)
            {
                foreach (KDNode n in l)
                {
                    if (n.getPoint().X != pivX)
                    {
                        allSame = false;
                    }
                }
            } else {
                foreach (KDNode n in l)
                {
                    if (n.getPoint().Y != pivY)
                    {
                        allSame = false;
                    }
                }
            }

            // tak vratime ity v zozname
            if (allSame) 
            {
                List<KDNode> res = new List<KDNode>();
                res.Add(l.ElementAt(i - 1));
                return res;
            }

            // odstranime pivot, aby sa nepridal do zoznamu
            l.RemoveAt(0);

            List<KDNode> newLeft = new List<KDNode>();
            List<KDNode> newRight = new List<KDNode>();
            if (d == 0) //split cez x-ovu os
            {
                foreach (KDNode n in l)
                {
                    if (n.getPoint().X <= pivX)
                    {
                        newLeft.Add(n);
                    } else {
                        newRight.Add(n);
                    }
                }
            } else { //split cez y-ovu os
                foreach (KDNode n in l)
                {
                    if (n.getPoint().Y <= pivY)
                    {
                        newLeft.Add(n);
                    } else {
                        newRight.Add(n);
                    }
                }
            }

            // pridame naspat pivot na koniec newLeft zoznamu
            // ak bude newRight list prazdny, tak sa to zavola este raz s inym pivotom
            newLeft.Add(pivot);
            if (newLeft.Count >= i)
            {
                return findIthSmallest(newLeft, i, d);
            } else {
                return findIthSmallest(newRight, i - newLeft.Count, d);
            }

        }

        // pre kazdy vrchol podstromu n updatne pocet listov,
        // ktore su pod nim
        public int updateNumLeafsDown(KDNode n)
        {
            if (n.isLeaf) {
                n.setNumLeafs(1);
                Console.WriteLine("update " + n.getPoint() + " numLeafs = 1");
                return 1;
            }
            int r = updateNumLeafsDown(n.getRight());
            int l = updateNumLeafsDown(n.getLeft());
            n.setNumLeafs(r + l);
            Console.WriteLine("update " + n.getSplit() + " numLeafs = " + n.getNumLeafs());
            return r + l;
        }

        // pre kazdeho predchodcu n updatne pocet listov,
        // ktore su nad nim
        public void updateNumLeafsUp(KDNode n) 
        {
            n.setNumLeafs(n.getLeft().getNumLeafs() + n.getRight().getNumLeafs());
            Console.WriteLine("update to root " + n.getSplit() + " numLeafs = " + n.getNumLeafs());
            Console.WriteLine("update to root right " + n.getRight().getNumLeafs() + " left " + n.getLeft().getNumLeafs());
            if (n.Equals(root)) 
            {
                return;
            }

            updateNumLeafsUp(n.getParent());

        }

        // pre kazdy vrchol v podstrome n updatne jeho bunku
        public void updateFromTo(KDNode n) 
        {
            if (n == null)
            {
                return;
            }
            n.setFromTo();
            updateFromTo(n.getRight());
            updateFromTo(n.getLeft());

        }
    #endregion

        #region Find k Nearest

        public void findNearest1(int x, int y) 
        {
            KDNode n = new KDNode(x, y, null, g);

            // utrieduje sa na zaklade vzdialenosti ku target
            KDNodeComparer com = new KDNodeComparer();
            // zoznam doposial najdenych najblizsich vrcholov
            nearestK = new SortedSet<KDNode>(com);
            findNearest(root, n);
            foreach (KDNode k in nearestK)
            {
                k.highlightRed = true;
            }
        }

        // najde numNearest najblizsich vrcholov ku target
        // na konci budu v usporiadanom zozname nearestK
        public void findNearest(KDNode subTree, KDNode target) 
        {
            // ak je to list, zistime, ci je blizsie ako doposial najdene, ak ich je dost
            if (subTree.isLeaf) {
                subTree.dist = distance(target.getPoint(), subTree.getPoint());
                if (nearestK.Count < numNearest)
                { 
                    nearestK.Add(subTree);
                } else {
                    if (nearestK.Last().dist > subTree.dist) 
                    {
                        nearestK.Remove(nearestK.Last());
                        nearestK.Add(subTree);
                    }
                }
                
                return;
            }


            int dim = subTree.getDim();

            // ak je target v lavom podstrome, nearer je lavy a further je pravy, inak opacne
            KDNode nearer;
            KDNode further;
            if (dim == 0)
            {
                if (subTree.getSplit() >= target.getPoint().X)
                {
                    nearer = subTree.getLeft();
                    further = subTree.getRight();
                } else {
                    further = subTree.getLeft();
                    nearer = subTree.getRight();
                }
            } else {
                if (subTree.getSplit() >= target.getPoint().Y)
                {
                    nearer = subTree.getLeft();
                    further = subTree.getRight();
                }else {
                    further = subTree.getLeft();
                    nearer = subTree.getRight();
                }
            }
            // najprv prehladavame blizsi podstrom 
            // (prvy prehladany blok bude ten, v kt. je target)
            // teda si ohranicime v priemere celkom slusne najblizsie vrcholy
            findNearest(nearer, target); 

            // zrata najblizsi bod od target na bunke further
            // ak je dalej ako doposial najdeny k-ty nejmensi, tak vo further
            // nemoze byt blizsi bod => nevnara sa
            Point nearestF = nearestTo(further, target);
            double dist = distance(nearestF, target.getPoint());

            if ((nearestK.Last().dist > dist) || (nearestK.Count < numNearest))
            {
                findNearest(further, target);
            }
        }

        // najde najmensiu vzdialenost medzi target a bunkou further.
        public Point nearestTo(KDNode further, KDNode target)
        {
            Point from = further.getFrom();
            Point to = further.getTo();
            Point tar = target.getPoint();
            Point result = new Point();
            result.X = (tar.X <= from.X) ?  from.X: 
                ((tar.X >= to.X) ? to.X : tar.X);
            result.Y = (tar.Y <= from.Y) ? from.Y :
                ((tar.Y >= to.Y) ? to.Y : tar.Y);
            return result;
        }

        // najde vchol, v ktoreho bunke(bloku) je click1
        public KDNode findBlockIn(KDNode click1) 
        {
            KDNode click = click1;
            KDNode n = new KDNode(g);
            n = findIn(click, root, 0);
            return n;
        }

        private KDNode findIn(KDNode click, KDNode n, int dim)
        {
            if (n.isLeaf)
            {
                return n;
            }

            if (dim == 0)
            {
                if (n.getSplit() >= click.getPoint().X)
                {
                    return findIn(click, n.getLeft(), 1 - dim);
                } else {
                    return findIn(click, n.getRight(), 1 - dim);
                }
            } else {
                if (n.getSplit() >= click.getPoint().Y)
                {
                    return findIn(click, n.getLeft(), 1 - dim);
                } else {
                    return findIn(click, n.getRight(), 1 - dim);
                }
            }
        }
        
        // vzdialenost dvoch bodov
        public double distance(Point a, Point b)
        {
            int x = a.X - b.X;
            int y = a.Y - b.Y;
            return Math.Sqrt(x*x + y*y);
        }
        #endregion

        #region Find Neighbor Blocks

        public void findNeighbors1(int x, int y)
        {
            KDNode k = new KDNode(x, y, null, g);
            // najde sa bunka, do ktorej bolo kliknute
            KDNode target = findBlockIn(k);

            // na konci su v tomto zozname susedia
            neighbors = new List<KDNode>();
            if (root == null || root.isLeaf)
            {
                return;
            }
            findNeighbors(target, root.getLeft());
            findNeighbors(target, root.getRight());
            // Console.WriteLine("pocet susedov: " + neighbors.Count);
            foreach(KDNode i in neighbors)
            {
                i.highligthBox = true;
            }
        }

        // zisti, ci bunka vrchola n susedi s bunkou target.
        // ak ano, hladaju sa susedia deti n, ak nie, 
        // potom ani deti s nim nemozu susedit
        public void findNeighbors(KDNode target, KDNode n) 
        {
            if (n.isLeaf)
            {
                if (!isIn(target, n) && isNeighbor(target, n))
                {
                    neighbors.Add(n);
                }
                return;
            }

            // je sused alebo je vo vnutri neho.
            if (isNeighbor(target, n) || (isIn(target, n)))
            {
                findNeighbors(target,n.getLeft());
                findNeighbors(target, n.getRight());
            }

        }

        // zistuje, ci je bunka target vo vnutri bunky n
        private Boolean isIn(KDNode target, KDNode n)
        {
            if ((target.getFrom().X >= n.getFrom().X) && (target.getTo().X <= n.getTo().X) &&
                (target.getFrom().Y >= n.getFrom().Y) && (target.getTo().Y <= n.getTo().Y))
            {
                return true;
            }

            return false;
        }

        // zistuje, ci dve bunky spolu susedia
        private Boolean isNeighbor(KDNode t, KDNode n) 
        {
            Point tf = t.getFrom();
            Point tt = t.getTo();
            Point nf = n.getFrom();
            Point nt = n.getTo();

            // t je tesne pod n
            if(nt.Y == tf.Y)
            {
                if ((nf.X < tt.X) && (nt.X > tf.X))
                {
                    return true;
                }
            }

            // t je tesne nad n
            if (nf.Y == tt.Y)
            {
                if ((tf.X < nt.X) && (tt.X > nf.X))
                {
                    return true;
                }

            }

            // t je tesne vlavo od n
            if (nt.X == tf.X)
            {
                if ((tf.Y < nt.Y) && (tt.Y > nf.Y))
                {
                    return true;
                }
            }

            // t je tesne vpravo od n
            if (tt.X == nf.X)
            {
                if ((nf.Y < tt.Y) && (nt.Y > tf.Y))
                {
                    return true;
                }
            }
            // Console.WriteLine("nie je to sused");
            return false;
        }

        #endregion

        #region Find Inside Rectangle

        public void findInside1(Point from, Point to) 
        {
            // na konci bude zoznam vrcholov vo vnutri v inside
            inside = new List<KDNode>();
            //Console.WriteLine(from + "from a to" + to);
            findInside(root, from, to);
            foreach (KDNode k in inside) 
            {
                k.highligthIn = true;
            }
        }

        // zistuje, ci je vrchol n vo vnutri obdlznika from, to 
        public void findInside(KDNode n, Point from, Point to) 
        {
            if (n.isLeaf)
            {
                // je vrchol vnutri obdlznika?
                if (pointInsideRect(n.getPoint(), from, to))
                {
                    inside.Add(n);
                }
                return;
            }

            // ak ma obdlznik s bunkou nejaky prienik vnori sa
            if (intersects(n, from, to)) 
            {
                findInside(n.getLeft(), from, to);
                findInside(n.getRight(), from, to);
            } else {
                return;
            }
        }

        // prienik bunky n s obdlznikom
        private Boolean intersects(KDNode n, Point from, Point to) 
        {
            // prienik je nie je vtedy, ak je cela bunka nad, pod, vlavo alebo vpravo voci obd.
            Point nodeF = n.getFrom();
            Point nodeT = n.getTo();

            //Console.WriteLine("ma, ci nema prienik?");
            //cely hore
            if(nodeT.Y < from.Y) {

                return false;
            }

            //cely dole
            if (nodeF.Y > to.Y)
            {
                return false;
            }

            //cely vlavo
            if (nodeF.X > to.X)
            {
                return false;
            }

            //cely vpravo
            if (nodeT.X < from.X)
            {
                return false;
            }
            // Console.WriteLine("ma prienik");
            return true;
        }

        // true, ak je bod p vo vnutri obdlznika from, to
        private Boolean pointInsideRect(Point p, Point from, Point to)
        {
            if ((p.X >= from.X) && (p.X <= to.X) && (p.Y >= from.Y) && (p.Y <= to.Y))
            {
                return true;
            }
            return false;
        }


        #endregion

        public void repaint()
        {
            System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White);
            g.FillRectangle(myBrush, 0, 0, width, height);
            if (root != null)
            {
                repaint(root);
            }
        }

        public void drawRect(Point from, Point to) {
            repaint();
            System.Drawing.Pen myPen = new System.Drawing.Pen(System.Drawing.Color.Green);
            g.DrawRectangle(myPen, from.X, from.Y, to.X - from.X, to.Y - from.Y);
        }

        public void drawCircle(int x, int y)
        {
            System.Drawing.Pen myPen = new System.Drawing.Pen(System.Drawing.Color.Green);
            System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Green);
            if (nearestK.Last() != null)
            {
                int n = (int)nearestK.Last().dist;
                g.DrawEllipse(myPen, x - n, y - n, 2 * n, 2 * n);
                g.FillEllipse(myBrush, x - 2, y - 2, 2 * 2, 2 * 2);
            }
        }

        private void repaint(KDNode n) 
        {
            n.repaint();
            if (n.getLeft() != null) 
            {
                repaint(n.getLeft());
            }
            if (n.getRight() != null)
            {
                repaint(n.getRight());
            }
        }

        public void notHighlight1()
        {
            notHighlight(root);
        }

        public void notHighlight(KDNode n)
        {
            if (n == null) return;
            n.highlight = false;
            n.highlightRed = false;
            n.highligthBox = false;
            n.highligthIn = false;
            notHighlight(n.getLeft());
            notHighlight(n.getRight());
        }

        public int getNumNearest() 
        {
            return numNearest;
        }
        public void setNumNearest(int k)
        {
            numNearest = k;
        }

    }
}
