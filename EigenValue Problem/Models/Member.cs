using System;
using MathNet.Numerics.LinearAlgebra;

namespace EigenValue_Problem.Models
{
    public class Member
    {
        public double L;
        public double m;
        public double NumberOfm;
        public double n;
        public double NumberOfn;
        public double a;
        public double c1;
        public double c2;
        public double meum;
        public double meun;
        public double i1;
        public double i2;
        public double i3;
        public double i4;
        public double i5;

        public double[,] FinalGlobalKe ;
        public double[,] FinalGlobalKg ;
        

        public Matrix<double> GlobalKgGlobalKe;

        public Matrix<double> U;
        public Matrix<double> S;
        public Matrix<double> Vt;
        public Matrix<double> I;
        //public Matrix<double> S12;
        // public double[,] I;

        public double MinEigenValue;
        public Vector<double> EigenValue;
        public Vector<double> EigenValue0;
        public Vector<double> EigenVectors;

        public Member(double m, double n, double a, double L)  //inputs in program 
        {
            this.L = L;
            this.m = m;
            this.n = n;
            this.a = a;
        }

        public override string ToString()
       {
            string ve = "";   // For output notepad of Global Ke for each m and a

                ve += " Ke Final    a =" + a + " m =" + m + " min eigen value =" + MinEigenValue + Environment.NewLine;
                for (int i = 0; i < Math.Sqrt(FinalGlobalKe.Length); i++)
                {
                    ve += Environment.NewLine;
                    for (int j = 0; j < Math.Sqrt(FinalGlobalKe.Length); j++)
                    {
                        ve = ve + FinalGlobalKe[i, j] + " ";
                    }
                }
                ve += Environment.NewLine;
                ve += Environment.NewLine;




            string vg = "";            // For output notepad of Global Ke for each m and a

            vg += "Kg Final     a =" + a + " m =" + m + " min eigen value =" + MinEigenValue + Environment.NewLine;
            for (int i = 0; i < Math.Sqrt(FinalGlobalKg.Length); i++)
            {
                vg += Environment.NewLine;
                for (int j = 0; j < Math.Sqrt(FinalGlobalKg.Length); j++)
                {
                    vg = vg + FinalGlobalKg[i, j] + " ";
                }
            }
            vg += Environment.NewLine;
            vg += Environment.NewLine;

            string kgke = "";            // For output notepad of Global Kgke for each m and a

            kgke += "Kgke Final     a =" + a + " m =" + m + " min eigen value =" + MinEigenValue + Environment.NewLine;
            for (int i = 0; i < (GlobalKgGlobalKe.RowCount); i++)
            {
                kgke += Environment.NewLine;
                for (int j = 0; j < (GlobalKgGlobalKe.ColumnCount); j++)
                {
                    kgke = kgke + GlobalKgGlobalKe[i, j] + " ";
                }
            }
            kgke += Environment.NewLine;
            kgke += Environment.NewLine;




            //string KgKe = "";            // For output notepad of Ke-1Kg for each m and a

            //KgKe += " KgKe Final     a =" + a + " m =" + m + " min eigen value =" + MinEigenValue + Environment.NewLine;
            //KgKe = GlobalKgGlobalKe.ToString();
            //KgKe += Environment.NewLine;
            //KgKe += Environment.NewLine;



            string E = "";            // For output notepad of Eigen values for each m and a

            E += "Eigen values      a =" + a + " m =" + m + " min eigen value =" + MinEigenValue + Environment.NewLine;
            E = EigenValue.ToString();
            E += Environment.NewLine;
            E += Environment.NewLine;




            return "" + c1 + '\t' + c2 + '\t' +
                   meum + '\t' + meun + '\t' +
                   i1 + '\t' + i2 + '\t' +
                   i3 + '\t' + i4 + '\t' +
                   i5 + Environment.NewLine + ve + Environment.NewLine + Environment.NewLine + vg + Environment.NewLine 
                   + kgke + Environment.NewLine + E + Environment.NewLine;
        }
    }
}
