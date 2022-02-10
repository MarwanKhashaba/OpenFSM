using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace EigenValue_Problem.Models
{
    
    public class Element
    {
        public int ID;
        //public int node1;
        //public int node2;
        public Node node1;
        public Node node2;
        public double deltax;
        public double deltay;
        public double b;
        public double t;
        public double alpha;
        public double CosAlpha;
        public double SinAlpha;
        public double thetax;
        public double thetay;
        public double Xcg;
        public double Ycg;
        public double deltaxdeltay12; //new
        public double deltaxdeltax12; //new
        public double deltaydeltay12; //new
        public double ymeanminusycg; //new needs section calculations first
        public double xmeanminusxcg; //new needs section calculations first
        public double xmeanminusxcgymeanminusycg; //new needs section calculations first
        public double ymeanminusycg2; //new needs section calculations first
        public double xmeanminusxcg2; //new needs section calculations first
        public double Ix;
        public double Iy;
        public double Ixy;
        public double A;
        public double Ax;
        public double Axx;
        public double Ay;
        public double Ayy;
        public double Axy;
        public double J;
        public double dx;
        public double dy;
        public double d1;
        public double dxy;
        public double f1;
        public double f2;
        public double t1;
        public double t2;
        //public double i;
        //public double j;
        //public double x;
        //public double y;

        // For the overall 8*8 matrices
        public Dictionary<Member, double[,]> ke = new Dictionary<Member, double[,]>();
        public Dictionary<Member, double[,]> kg = new Dictionary<Member, double[,]>();

        // For Transformation matrix 8*8
        public Matrix<double> TransformationMatrix = Matrix<double>.Build.Dense(8, 8);

        // For the overall 8*8 Transformed matrices
        //public Dictionary<Member, double[,]> Transformedke = new Dictionary<Member, double[,]>();
        //public Dictionary<Member, double[,]> Transformedkg = new Dictionary<Member, double[,]>();
        public Matrix<double> Transformedke = Matrix<double>.Build.Dense(8, 8);
        public Matrix<double> Transformedkg = Matrix<double>.Build.Dense(8, 8);

        //public Matrix<double> IMatrix = Matrix<double>.Build.Dense(2, 2);

        // For elastic stiffness matrices (4x 4*4) After Transformation
        public Dictionary<Member, double[,]> kemm = new Dictionary<Member, double[,]>();
        public Dictionary<Member, double[,]> kemb = new Dictionary<Member, double[,]>();
        public Dictionary<Member, double[,]> kebm = new Dictionary<Member, double[,]>();
        public Dictionary<Member, double[,]> kebb = new Dictionary<Member, double[,]>();
        // For geometric stiffness matrices (4x 4*4) After Transformation
        public Dictionary<Member, double[,]> kgmm = new Dictionary<Member, double[,]>();
        public Dictionary<Member, double[,]> kgmb = new Dictionary<Member, double[,]>();
        public Dictionary<Member, double[,]> kgbm = new Dictionary<Member, double[,]>();
        public Dictionary<Member, double[,]> kgbb = new Dictionary<Member, double[,]>();



        //public double[,] elementTotalMatrixKe = new double[SizeX,SizeY];
        //public double[,] elementTotalMatrixKg = new double[SizeX,SizeY];

        //public double[,] matrix name(SizeX, std::vector<double>(SizeY))
        //public double[,] KeForM = new double[i,j];
        //public double[,] KgForM = new double[x,y];

        public Element()  //inputs in program 
        {
            this.ID = 0;
            //this.node1 = 0;
            //this.node2 = 0;
            this.t = 0.0;
        }

        

        public override string ToString()
        {
            string ve = "";
            foreach (var item in ke)
            {
                ve += "Ke beforeTransformation      a =" + item.Key.a + " m=" + item.Key.m + Environment.NewLine;
                for (int i = 0; i < 8; i++)
                {
                    ve += Environment.NewLine;
                    for (int j = 0; j < 8; j++)
                    {
                        ve = ve + item.Value[i, j] + "  ";
                    }
                }
                ve += Environment.NewLine;
                ve += Environment.NewLine;
            }


            string vg = "";
            foreach (var item in kg)
            {
                vg += "Kg beforeTransformation      a =" + item.Key.a + " m=" + item.Key.m + Environment.NewLine;
                for (int i = 0; i < 8; i++)
                {
                    vg += Environment.NewLine;
                    for (int j = 0; j < 8; j++)
                    {
                        vg = vg + item.Value[i, j] + "  ";
                    }
                }
                vg += Environment.NewLine;
                vg += Environment.NewLine;
            }

            //Transformation matrix
            string T = "";   

            T += " Transformation matrix "    + Environment.NewLine;
            for (int i = 0; i < 8 ; i++)
            {
                T += Environment.NewLine;
                for (int j = 0; j < 8 ; j++)
                {
                    T = T + TransformationMatrix[i, j] + " ";
                }
            }
            T += Environment.NewLine;
            T += Environment.NewLine;

            //Transformed Ke 8*8
            //string KeT = "";
            //foreach (var item in kemm)
            //{

            //    KeT += " Transformed Ke    a = " + item.Key.a + " m = " + item.Key.m + Environment.NewLine;
            //    for (int i = 0; i < 8; i++)
            //    {
            //        KeT += Environment.NewLine;
            //        for (int j = 0; j < 8; j++)
            //        {
            //            KeT = KeT + Transformedke[i, j] + "  ";
            //        }
            //    }
            //    KeT += Environment.NewLine;
            //    KeT += Environment.NewLine;
            //}

            //Transformed Kg 8*8
            //string KgT = "";
            //foreach (var item in kemm )
            //{

            //    KgT += " Transformed Kg    a = " + item.Key.a + " m = " + item.Key.m+ Environment.NewLine;
            //for (int i = 0; i < 8; i++)
            //{
            //    KgT += Environment.NewLine;
            //    for (int j = 0; j < 8; j++)
            //    {
            //        KgT = KgT + Transformedkg[i, j] + "  ";
            //    }
            //}
            //KgT += Environment.NewLine;
            //KgT += Environment.NewLine;

            //} 



            string vemm = "";
            foreach (var item in kemm)
            {
                vemm += "Kemm      a =" + item.Key.a + " m=" + item.Key.m + Environment.NewLine;
                for (int i = 0; i < 4; i++)
                {
                    vemm += Environment.NewLine;
                    for (int j = 0; j < 4; j++)
                    {
                        vemm = vemm + item.Value[i, j] + "  ";
                    }
                }
                vemm += Environment.NewLine;
                vemm += Environment.NewLine;
            }

            string vemb = "";
            foreach (var item in kemb)
            {
                vemb += "Kemb      a =" + item.Key.a + " m=" + item.Key.m + Environment.NewLine;
                for (int i = 0; i < 4; i++)
                {
                    vemb += Environment.NewLine;
                    for (int j = 0; j < 4; j++)
                    {
                        vemb = vemb + item.Value[i, j] + "  ";
                    }
                }
                vemb += Environment.NewLine;
                vemb += Environment.NewLine;
            }

            string vebm = "";
            foreach (var item in kebm)
            {
                vebm += "Kebm      a =" + item.Key.a + " m=" + item.Key.m + Environment.NewLine;
                for (int i = 0; i < 4; i++)
                {
                    vebm += Environment.NewLine;
                    for (int j = 0; j < 4; j++)
                    {
                        vebm = vebm + item.Value[i, j] + "  ";
                    }
                }
                vebm += Environment.NewLine;
                vebm += Environment.NewLine;
            }

            string vebb = "";
            foreach (var item in kebb)
            {
                vebb += "Kebb      a =" + item.Key.a + " m=" + item.Key.m + Environment.NewLine;
                for (int i = 0; i < 4; i++)
                {
                    vebb += Environment.NewLine;
                    for (int j = 0; j < 4; j++)
                    {
                        vebb = vebb + item.Value[i, j] + "  ";
                    }
                }
                vebb += Environment.NewLine;
                vebb += Environment.NewLine;
            }

            // for geometric stiffness matrices
            string vgmm = "";
            foreach (var item in kgmm)
            {
                vgmm += "Kgmm      a =" + item.Key.a + " m=" + item.Key.m + Environment.NewLine;
                for (int i = 0; i < 4; i++)
                {
                    vgmm += Environment.NewLine;
                    for (int j = 0; j < 4; j++)
                    {
                        vgmm = vgmm + item.Value[i, j] + "  ";
                    }
                }
                vgmm += Environment.NewLine;
                vgmm += Environment.NewLine;
            }

            string vgmb = "";
            foreach (var item in kgmb)
            {
                vgmb += "Kgmb      a =" + item.Key.a + " m=" + item.Key.m + Environment.NewLine;
                for (int i = 0; i < 4; i++)
                {
                    vgmb += Environment.NewLine;
                    for (int j = 0; j < 4; j++)
                    {
                        vgmb = vgmb + item.Value[i, j] + "  ";
                    }
                }
                vgmb += Environment.NewLine;
                vgmb += Environment.NewLine;
            }

            string vgbm = "";
            foreach (var item in kgbm)
            {
                vgbm += "Kgbm      a =" + item.Key.a + " m=" + item.Key.m + Environment.NewLine;
                for (int i = 0; i < 4; i++)
                {
                    vgbm += Environment.NewLine;
                    for (int j = 0; j < 4; j++)
                    {
                        vgbm = vgbm + item.Value[i, j] + "  ";
                    }
                }
                vgbm += Environment.NewLine;
                vgbm += Environment.NewLine;
            }

            string vgbb = "";
            foreach (var item in kgbb)
            {
                vgbb += "Kgbb      a =" + item.Key.a + " m=" + item.Key.m + Environment.NewLine;
                for (int i = 0; i < 4; i++)
                {
                    vgbb += Environment.NewLine;
                    for (int j = 0; j < 4; j++)
                    {
                        vgbb = vgbb + item.Value[i, j] + "  ";
                    }
                }
                vgbb += Environment.NewLine;
                vgbb += Environment.NewLine;
            }

            return "" + deltax + '\t' + deltay + '\t' +
                      b + '\t' + alpha  + '\t' + SinAlpha + '\t' + CosAlpha + '\t' +
                      thetax + '\t' + thetay + '\t' +
                      Xcg + '\t' + Ycg + '\t' +
                      A + '\t' + Ax + '\t' +
                      Axx + '\t' + Ay + '\t'+ Ayy + '\t'+
                      Axy + '\t' 
                      + deltaxdeltay12 + '\t' +
                      deltaxdeltax12 + '\t' + deltaydeltay12 + '\t' +
                      ymeanminusycg + '\t' + xmeanminusxcg + '\t' + 
                      xmeanminusxcgymeanminusycg + '\t' + ymeanminusycg2 
                      + '\t' + xmeanminusxcg2 + '\t' 
                      + Ix + '\t'+
                      Iy + '\t' + Ixy + '\t' +
                      J + '\t' + dx + '\t' +
                      dy + '\t' + dxy + '\t' +
                      d1 + '\t' + f1 + '\t' +
                      f2 + '\t' + t1 + '\t' +
                      t2 + Environment.NewLine + Environment.NewLine
                      + ve + Environment.NewLine + Environment.NewLine
                      + vg + Environment.NewLine + Environment.NewLine
                      + T + Environment.NewLine + Environment.NewLine
                      //+ KeT + Environment.NewLine + Environment.NewLine
                      //+ KgT + Environment.NewLine + Environment.NewLine
                      + vemm + Environment.NewLine + Environment.NewLine
                      + vemb + Environment.NewLine + Environment.NewLine
                      + vebm + Environment.NewLine + Environment.NewLine
                      + vebb + Environment.NewLine  + Environment.NewLine 
                      + vgmm + Environment.NewLine + Environment.NewLine
                      + vgmb + Environment.NewLine + Environment.NewLine
                      + vgbm + Environment.NewLine + Environment.NewLine
                      + vgbb + Environment.NewLine;
        }

    }

}
