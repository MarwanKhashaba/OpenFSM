using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EigenValue_Problem
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            mListComboBox form = new mListComboBox();
            Application.Run(form);
            
            
        }
        
    
    }
    
    public class Material
    {
        public double Ex;
        public double Ey;
        public double Vx;
        public double Vy;
        public double Gxy;
        public double e1;
        public double e2;

        public Material(double Ex, double Ey, double Vx, double Vy)
        {
            this.Ex = Ex;
            this.Ey = Ey;
            this.Vx = Vx;
            this.Vy = Vy;
            
        }
    }

    public class Element
    {
        public double b;
        public double t;
        public double alpha;
        public double f1;
        public double f2;
        public double dx;
        public double dy;
        public double d1;
        public double dxy;
        public double t1;
        public double t2;
        public double[,] kem = new double[3, 3];
        public double[,] keb = new double[3, 3];
        public double[,] kgm = new double[3, 3];
        public double[,] kgb = new double[3, 3];
        public double[,] ke = new double[7, 7];
        public double[,] kg = new double[7, 7];

        public Element(double b, double t, double alpha, double f1, double f2)
        {
            this.b = b;
            this.t = t;
            this.alpha = alpha;
            this.f1 = f1;
            this.f2 = f2;
        }
    }

    public class Member
    {
        public double m;
        public double n;
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
        public Member(double m, double n, double a)
        {
            this.m = m;
            this.n = n;
            this.a = a;
        }
    }
    



}
