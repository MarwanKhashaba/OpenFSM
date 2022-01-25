
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EigenValue_Problem
{

    public partial class mListComboBox : Form
    {
        Material material = new Material(0, 0, 0, 0);
        Element element = new Element(0,0,0,0,0);
        Member member = new Member(0, 0, 0);
        public mListComboBox()
        {
            InitializeComponent();

            //Material concrete25 = new Material(4000, 4000, 0.2, 0.2);
            //Material concrete30 = new Material(5000, 4000, 0.3, 0.3);
            //Material Custom = new Material(0, 0, 0, 0);
            //List<Material> materials = new List<Material> { concrete25, concrete30 };

            //foreach (Material material in materials)
            //{
            //    //materialComboBox.Items.Add(material.ex);
            //}
            
        }

        private void MaterialComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (materialComboBox.Text.ToString() == "Concrete 25")
            {
                //this will be added later
            }
        }

        public void GetMaterialValues()
        {
            material.Ex = double.Parse(exTextBox.Text);
            material.Ey = double.Parse(exTextBox.Text);
            material.Vx = double.Parse(exTextBox.Text);
            material.Vy = double.Parse(exTextBox.Text);
        }
        public void GetElementValues()
        {
            element.b = double.Parse(bTextBox.Text);
            element.t = double.Parse(tTextBox.Text);
            element.alpha = double.Parse(alphaTextBox.Text);
            element.f1 = double.Parse(f1TextBox.Text);
            element.f2 = double.Parse(f2TextBox.Text);
        }
        public void GetMemberValues()
        {
            member.m = double.Parse(addMTextBox.Text);
            member.n = double.Parse(nTextBox.Text);
            member.a = double.Parse(addATextBox.Text);
        }

        public void SolveMaterial()
        {
            material.Gxy = (material.Ex) / (2 * (1 + material.Vx));
            gxyTextBox.Text = material.Gxy.ToString();
            material.e1 = material.Ex / (1 - material.Vx * material.Vy);
            material.e2 = material.Ey / (1 - material.Vx * material.Vy);
        }
        
        public void PreSolveElement()
        {
            element.dx = material.Ex * element.t * element.t * element.t / (12 * (1 - material.Vx * material.Vy));
            element.dy = material.Ey * element.t * element.t * element.t / (12 * (1 - material.Vx * material.Vy));
            element.dxy = material.Gxy * element.t * element.t * element.t / 12;
            element.d1 = material.Ex * material.Vy * element.t * element.t * element.t / (12 * (1 - material.Vx * material.Vy));
            element.t1 = element.f1* element.t;
            element.t2 = element.f2* element.t;
        }
        public void PreSolveMember()
        {

            member.c1 = member.m * Math.PI / member.a;
            member.c2 = member.n * Math.PI / member.a;
            member.meum = member.m * Math.PI;
            member.meun = member.n * Math.PI;
            if (bcComboBox.SelectedText == "Simple - Simple")
            {
                if (member.m == member.n)
                {
                    member.i1 = member.a / 2;
                    member.i2 = -Math.PI * Math.PI * member.m * member.m / (2 * member.a);
                    member.i3 = -Math.PI * Math.PI * member.n * member.n / (2 * member.a);
                    member.i4 = Math.PI * Math.PI * Math.PI * Math.PI * member.m * member.m * member.m * member.m / (2 * member.a * member.a * member.a);
                    member.i5 = Math.PI * Math.PI * member.m * member.m / (2 * member.a);
                }
                else
                {
                    member.i1 = 0;
                    member.i2 = 0;
                    member.i3 = 0;
                    member.i4 = 0;
                    member.i5 = 0;
                }
            }
            else if (bcComboBox.SelectedText == "Fixed - Fixed")
            {
                if (member.m == member.n)
                {
                    if (member.m==1)
                    {
                        member.i1 = 3 * member.a / 8;
                    }
                    else
                    {
                        member.i1 = member.a / 4;
                    }
                    member.i2 = -Math.PI * Math.PI * (member.m * member.m + 1) / (4 * member.a);
                    member.i3 = -Math.PI * Math.PI * (member.n * member.n + 1) / (4 * member.a);
                    member.i4 = Math.Pow(Math.PI,4) * (Math.Sqrt(member.m * member.m + 1) + (4 * member.m * member.m)) / (4 * member.a * member.a * member.a);
                    member.i5 = Math.PI * Math.PI * (1 + member.m * member.m) / (4 * member.a);
                }
                else if (member.m - member.n ==2)
                {
                    member.i1 = - member.a / 8;
                    member.i2 = (Math.PI * Math.PI * (member.m * member.m + 1) / (8 * member.a))-(member.m * Math.PI * Math.PI / (4 * member.a));
                    member.i3 = (Math.PI * Math.PI * (member.n * member.n + 1) / (8 * member.a)) - (member.n * Math.PI * Math.PI / (4 * member.a));
                    member.i4 = -Math.Sqrt(member.m - 1) * Math.Sqrt(member.n + 1) * Math.Pow(Math.PI, 4) / (8 * member.a * member.a * member.a);
                    member.i5 = Math.PI * Math.PI * -(1 + member.m * member.n) / (8 * member.a);
                }
                else if (member.m - member.n == -2)
                {
                    member.i1 = -member.a / 8;
                    member.i2 = (Math.PI * Math.PI * (member.m * member.m + 1) / (8 * member.a)) + (member.m * Math.PI * Math.PI / (4 * member.a));
                    member.i3 = (Math.PI * Math.PI * (member.n * member.n + 1) / (8 * member.a)) + (member.n * Math.PI * Math.PI / (4 * member.a));
                    member.i4 = -Math.Sqrt(member.m + 1) * Math.Sqrt(member.n - 1) * Math.Pow(Math.PI, 4) / (8 * member.a * member.a * member.a);
                    member.i5 = Math.PI * Math.PI * -(1 + member.m * member.n) / (8 * member.a);
                }
                else
                {
                    member.i1 = 0;
                    member.i2 = 0;
                    member.i3 = 0;
                    member.i4 = 0;
                    member.i5 = 0;
                }
            }
            else if (bcComboBox.SelectedText == "Simple - Fixed")//no
            {
                if (member.m == member.n)
                {                
                    member.i1 = 3 * member.a / 8;
                    member.i2 = -Math.PI * Math.PI * (member.m * member.m + 1) / (4 * member.a);
                    member.i3 = -Math.PI * Math.PI * (member.n * member.n + 1) / (4 * member.a);
                    member.i4 = Math.Pow(Math.PI, 4) * (Math.Sqrt(member.m * member.m + 1) + (4 * member.m * member.m)) / (4 * member.a * member.a * member.a);
                    member.i5 = Math.PI * Math.PI * (1 + member.m * member.m) / (4 * member.a);
                }
                else if (member.m - member.n == 1)
                {
                    member.i1 = -member.a / 8;
                    member.i2 = (Math.PI * Math.PI * (member.m * member.m + 1) / (8 * member.a)) - (member.m * Math.PI * Math.PI / (4 * member.a));
                    member.i3 = (Math.PI * Math.PI * (member.n * member.n + 1) / (8 * member.a)) - (member.n * Math.PI * Math.PI / (4 * member.a));
                    member.i4 = -Math.Sqrt(member.m - 1) * Math.Sqrt(member.n + 1) * Math.Pow(Math.PI, 4) / (8 * member.a * member.a * member.a);
                    member.i5 = Math.PI * Math.PI * -(1 + member.m * member.n) / (8 * member.a);
                }
                else if (member.m - member.n == -1)
                {
                    member.i1 = -member.a / 8;
                    member.i2 = (Math.PI * Math.PI * (member.m * member.m + 1) / (8 * member.a)) + (member.m * Math.PI * Math.PI / (4 * member.a));
                    member.i3 = (Math.PI * Math.PI * (member.n * member.n + 1) / (8 * member.a)) + (member.n * Math.PI * Math.PI / (4 * member.a));
                    member.i4 = -Math.Sqrt(member.m + 1) * Math.Sqrt(member.n - 1) * Math.Pow(Math.PI, 4) / (8 * member.a * member.a * member.a);
                    member.i5 = Math.PI * Math.PI * -(1 + member.m * member.n) / (8 * member.a);
                }
                else
                {
                    member.i1 = 0;
                    member.i2 = 0;
                    member.i3 = 0;
                    member.i4 = 0;
                    member.i5 = 0;
                }              
            }
            else if (bcComboBox.SelectedText == "Fixed - Free")//no
            {
                if (member.m == member.n)
                {
                    member.i1 = (3 * member.a / 2) - ((2 * member.a * Math.Pow(-1,member.m-1)) / (( member.m - 0.5) * Math.PI));
                    member.i2 = Math.Sqrt(member.m - 0.5) * Math.Pow(Math.PI, 2) * (Math.Pow(-1,member.m - 1))/1;
                    member.i3 = -Math.PI * Math.PI * (member.n * member.n + 1) / (4 * member.a);
                    member.i4 = Math.Pow(Math.PI, 4) * (Math.Sqrt(member.m * member.m + 1) + (4 * member.m * member.m)) / (4 * member.a * member.a * member.a);
                    member.i5 = Math.PI * Math.PI * (1 + member.m * member.m) / (4 * member.a);
                }
                else
                {
                    member.i1 = -member.a / 8;
                    member.i2 = (Math.PI * Math.PI * (member.m * member.m + 1) / (8 * member.a)) - (member.m * Math.PI * Math.PI / (4 * member.a));
                    member.i3 = (Math.PI * Math.PI * (member.n * member.n + 1) / (8 * member.a)) - (member.n * Math.PI * Math.PI / (4 * member.a));
                    member.i4 = -Math.Sqrt(member.m - 1) * Math.Sqrt(member.n + 1) * Math.Pow(Math.PI, 4) / (8 * member.a * member.a * member.a);
                    member.i5 = Math.PI * Math.PI * -(1 + member.m * member.n) / (8 * member.a);
                } 
            }
            else if (bcComboBox.SelectedText == "Fixed - Guided")//no
            {
                if (member.m == member.n)
                {
                    if (member.m == 1)
                    {
                        member.i1 = 3 * member.a / 8;
                    }
                    else
                    {
                        member.i1 = member.a / 4;
                    }
                    member.i2 = -Math.PI * Math.PI * (member.m * member.m + 1) / (4 * member.a);
                    member.i3 = -Math.PI * Math.PI * (member.n * member.n + 1) / (4 * member.a);
                    member.i4 = Math.Pow(Math.PI, 4) * (Math.Sqrt(member.m * member.m + 1) + (4 * member.m * member.m)) / (4 * member.a * member.a * member.a);
                    member.i5 = Math.PI * Math.PI * (1 + member.m * member.m) / (4 * member.a);
                }
                else if (member.m - member.n == 1)
                {
                    member.i1 = -member.a / 8;
                    member.i2 = (Math.PI * Math.PI * (member.m * member.m + 1) / (8 * member.a)) - (member.m * Math.PI * Math.PI / (4 * member.a));
                    member.i3 = (Math.PI * Math.PI * (member.n * member.n + 1) / (8 * member.a)) - (member.n * Math.PI * Math.PI / (4 * member.a));
                    member.i4 = -Math.Sqrt(member.m - 1) * Math.Sqrt(member.n + 1) * Math.Pow(Math.PI, 4) / (8 * member.a * member.a * member.a);
                    member.i5 = Math.PI * Math.PI * -(1 + member.m * member.n) / (8 * member.a);
                }
                else if (member.m - member.n == -1)
                {
                    member.i1 = -member.a / 8;
                    member.i2 = (Math.PI * Math.PI * (member.m * member.m + 1) / (8 * member.a)) + (member.m * Math.PI * Math.PI / (4 * member.a));
                    member.i3 = (Math.PI * Math.PI * (member.n * member.n + 1) / (8 * member.a)) + (member.n * Math.PI * Math.PI / (4 * member.a));
                    member.i4 = -Math.Sqrt(member.m + 1) * Math.Sqrt(member.n - 1) * Math.Pow(Math.PI, 4) / (8 * member.a * member.a * member.a);
                    member.i5 = Math.PI * Math.PI * -(1 + member.m * member.n) / (8 * member.a);
                }
                else
                {
                    member.i1 = 0;
                    member.i2 = 0;
                    member.i3 = 0;
                    member.i4 = 0;
                    member.i5 = 0;
                }
            }
        }

        public void PreSolveMatrices()
        {
            //Kem
            element.kem[0, 0] = (material.Gxy * element.b * member.i5 * element.t / 3) + (material.e1 * member.i1 * element.t / element.b);
            element.kem[0, 1] = (-material.e2 * material.Vx * member.i3 * element.t / (2 * member.c2)) - (material.Gxy * member.i5 * element.t / (2 * member.c2));
            element.kem[0, 2] = (material.Gxy * element.b * member.i5 * element.t / 6) - (material.e1 * member.i1 * element.t / element.b);
            element.kem[0, 3] = (-material.e2 * material.Vx * member.i3 * element.t / (2 * member.c2))
                + (material.Gxy * member.i5 * element.t / (2 * member.c2));
            element.kem[1, 0] = (-material.e2 * material.Vx * member.i2 * element.t / (2 * member.c1))
                - (material.Gxy * member.i5 * element.t / (2 * member.c1));
            element.kem[1, 1] = (material.e2 * element.b * member.i4 * element.t / (3 * member.c1 * member.c2))
                + (material.Gxy * member.i5 * element.t / (element.b * member.c1 * member.c2));
            element.kem[1, 2] = (material.e2 * material.Vx * member.i2 * element.t / (2 * member.c1)) - (material.Gxy * member.i5 * element.t / (2 * member.c1));
            element.kem[1, 3] = (material.e2 * element.b * member.i4 * element.t / (6 * member.c1 * member.c2)) 
                - (material.Gxy * member.i5 * element.t / (element.b * member.c1 * member.c2));
            element.kem[2, 0] = (material.Gxy * element.b * member.i5 * element.t / 6) - (material.e1 * member.i1 * element.t / element.b);
            element.kem[2, 1] = (material.e2 * material.Vx * member.i3 * element.t / (2 * member.c2)) - (material.Gxy * member.i5 * element.t / (2 * member.c2));
            element.kem[2, 2] = (material.Gxy * element.b * member.i5 * element.t / 3) 
                + (material.e1 * member.i1 * element.t / element.b);
            element.kem[2, 3] = (material.e2 * material.Vx * member.i3 * element.t / (2 * member.c2))
                + (material.Gxy * member.i5 * element.t / (2 * member.c2));
            element.kem[3, 0] = (-material.e2 * material.Vx * member.i2 * element.t / (2 * member.c1)) 
                + (material.Gxy * member.i5 * element.t / (2 * member.c1));
            element.kem[3, 1] = (material.e2 * element.b * member.i4 * element.t / (6 * member.c1 * member.c2)) 
                - (material.Gxy * member.i5 * element.t / (element.b * member.c1 * member.c2));
            element.kem[3, 2] = (material.e2 * material.Vx * member.i2 * element.t / (2 * member.c1)) + (material.Gxy * member.i5 * element.t / (2 * member.c1));
            element.kem[3, 3] = (material.e2 * element.b * member.i4 * element.t / (3 * member.c1 * member.c2)) 
                + (material.Gxy * member.i5 * element.t / (element.b * member.c1 * member.c2));

            //Keb
            element.keb[0, 0] = (5040 * (element.dx * member.i1 / (420 * element.b * element.b * element.b)) 
                - 504 * element.b * element.b * (element.d1* member.i2 / (420 * element.b * element.b * element.b))
                - 504 * element.b * element.b * (element.d1* member.i3 / (420 * element.b * element.b * element.b)) 
                + 156 * element.b * element.b * element.b * element.b * (element.dy * member.i4 / (420 * element.b * element.b * element.b))
                + 2016 * element.b * element.b * (element.dxy * member.i5 / (420 * element.b * element.b * element.b)));

            element.keb[0, 1] = (2520 * element.b * (element.dx * member.i1 / (420 * element.b *element.b* element.b)) 
                - 462 *element.b*element.b*element.b* (element.d1* member.i2 / (420 *element.b*element.b* element.b)) 
                - 42 *element.b*element.b*element.b* (element.d1* member.i3 / (420 *element.b*element.b* element.b)) 
                + 22 *element.b*element.b*element.b*element.b* element.b * (element.dy * member.i4 / (420 *element.b*element.b* element.b)) 
                + 168 *element.b*element.b*element.b* (element.dxy * member.i5 / (420 *element.b*element.b* element.b)));

            element.keb[0, 2] = (-5040 * (element.dx * member.i1 / (420 *element.b*element.b* element.b)) 
                + 504 *element.b*element.b* (element.d1* member.i2 / (420 *element.b*element.b* element.b)) 
                + 504 *element.b*element.b* (element.d1* member.i3 / (420 *element.b*element.b* element.b)) 
                + 54 *element.b*element.b*element.b*element.b* (element.dy * member.i4 / (420 *element.b*element.b* element.b)) 
                - 2016 *element.b*element.b* (element.dxy * member.i5 / (420 *element.b*element.b* element.b)));

            element.keb[0, 3] = (2520 *element.b* (element.dx * member.i1 / (420 *element.b*element.b* element.b)) 
                - 42 *element.b*element.b*element.b* (element.d1* member.i2 / (420 *element.b*element.b* element.b)) 
                - 42 *element.b*element.b*element.b* (element.d1* member.i3 / (420 *element.b*element.b* element.b)) 
                - 13 *element.b*element.b*element.b*element.b*element.b* (element.dy * member.i4 / (420 *element.b*element.b* element.b)) 
                + 168 *element.b*element.b*element.b* (element.dxy * member.i5 / (420 *element.b*element.b* element.b)));

            element.keb[1, 0] = (2520 *element.b* (element.dx * member.i1 / (420 *element.b*element.b* element.b)) 
                - 42 *element.b*element.b*element.b* (element.d1* member.i2 / (420 *element.b*element.b* element.b)) 
                - 462 *element.b*element.b*element.b* (element.d1* member.i3 / (420 *element.b*element.b* element.b)) 
                + 22 *element.b*element.b*element.b*element.b*element.b* (element.dy * member.i4 / (420 *element.b*element.b* element.b)) 
                + 168 *element.b*element.b*element.b* (element.dxy * member.i5 / (420 *element.b*element.b* element.b)));

            element.keb[1, 1] = (1680 *element.b*element.b* (element.dx * member.i1 / (420 *element.b*element.b* element.b)) 
                - 56 *element.b*element.b*element.b*element.b* (element.d1* member.i2 / (420 *element.b*element.b* element.b)) 
                - 56 *element.b*element.b*element.b*element.b* (element.d1* member.i3 / (420 *element.b*element.b* element.b))
                + 4 *element.b*element.b*element.b*element.b*element.b*element.b* (element.dy * member.i4 / (420 *element.b*element.b* element.b)) 
                + 224 *element.b*element.b*element.b*element.b* (element.dxy * member.i5 / (420 *element.b*element.b* element.b)));

            element.keb[1, 2] = (-2520 *element.b* (element.dx * member.i1 / (420 *element.b*element.b* element.b)) 
                + 42 *element.b*element.b*element.b* (element.d1* member.i2 / (420 *element.b*element.b* element.b)) 
                + 42 *element.b*element.b*element.b* (element.d1* member.i3 / (420 *element.b*element.b* element.b))
                + 13 *element.b*element.b*element.b*element.b*element.b* (element.dy * member.i4 / (420 *element.b*element.b* element.b)) 
                - 168 *element.b*element.b*element.b* (element.dxy * member.i5 / (420 *element.b*element.b* element.b)));

            element.keb[1, 3] = (840 *element.b*element.b* (element.dx * member.i1 / (420 *element.b*element.b* element.b)) 
                + 14 *element.b*element.b*element.b*element.b* (element.d1* member.i2 / (420 *element.b*element.b* element.b)) 
                + 14 *element.b*element.b*element.b*element.b* (element.d1* member.i3 / (420 *element.b*element.b* element.b)) 
                - 3 *element.b*element.b*element.b*element.b*element.b*element.b* (element.dy * member.i4 / (420 *element.b*element.b* element.b))
                - 56 *element.b*element.b*element.b*element.b* (element.dxy * member.i5 / (420 *element.b*element.b* element.b)));

            element.keb[2, 0] = (-5040 * (element.dx * member.i1 / (420 *element.b*element.b* element.b)) 
                + 504 *element.b*element.b* (element.d1* member.i2 / (420 *element.b*element.b* element.b)) 
                + 504 *element.b*element.b* (element.d1* member.i3 / (420 *element.b*element.b* element.b)) 
                + 54 *element.b*element.b*element.b*element.b* (element.dy * member.i4 / (420 *element.b*element.b* element.b)) 
                - 2016 *element.b*element.b* (element.dxy * member.i5 / (420 *element.b*element.b* element.b)));

            element.keb[2, 1] = (-2520 *element.b* (element.dx * member.i1 / (420 *element.b*element.b* element.b)) 
                + 42 *element.b*element.b*element.b* (element.d1* member.i2 / (420 *element.b*element.b* element.b)) 
                + 42 *element.b*element.b*element.b* (element.d1* member.i3 / (420 *element.b*element.b* element.b)) 
                + 13 *element.b*element.b*element.b*element.b*element.b* (element.dy * member.i4 / (420 *element.b*element.b* element.b)) 
                - 168 *element.b*element.b*element.b* (element.dxy * member.i5 / (420 *element.b*element.b* element.b)));

            element.keb[2, 2] = (5040 * (element.dx * member.i1 / (420 *element.b*element.b* element.b)) 
                - 504 *element.b*element.b* (element.d1* member.i2 / (420 *element.b*element.b* element.b))
                - 504 *element.b*element.b* (element.d1* member.i3 / (420 *element.b*element.b* element.b)) 
                + 156 *element.b*element.b*element.b*element.b* (element.dy * member.i4 / (420 *element.b*element.b* element.b)) 
                + 2016 *element.b*element.b* (element.dxy * member.i5 / (420 *element.b*element.b* element.b)));

            element.keb[2, 3] = (-2520 *element.b* (element.dx * member.i1 / (420 *element.b*element.b* element.b)) 
                + 462 *element.b*element.b*element.b* (element.d1* member.i2 / (420 *element.b*element.b* element.b)) 
                + 42 *element.b*element.b*element.b* (element.d1* member.i3 / (420 *element.b*element.b* element.b)) 
                - 22 *element.b*element.b*element.b*element.b*element.b* (element.dy * member.i4 / (420 *element.b*element.b* element.b)) 
                - 168 *element.b*element.b*element.b* (element.dxy * member.i5 / (420 *element.b*element.b* element.b)));

            element.keb[3, 0] = (2520 *element.b* (element.dx * member.i1 / (420 *element.b*element.b* element.b))
                - 42 *element.b*element.b*element.b* (element.d1* member.i2 / (420 *element.b*element.b* element.b)) 
                - 42 *element.b*element.b*element.b* (element.d1* member.i3 / (420 *element.b*element.b* element.b)) 
                - 13 *element.b*element.b*element.b*element.b*element.b* (element.dy * member.i4 / (420 *element.b*element.b* element.b)) 
                + 168 *element.b*element.b*element.b* (element.dxy * member.i5 / (420 *element.b*element.b* element.b)));

            element.keb[3, 1] = (840 *element.b*element.b* (element.dx * member.i1 / (420 *element.b*element.b* element.b)) 
                + 14 *element.b*element.b*element.b*element.b* (element.d1* member.i2 / (420 *element.b*element.b* element.b)) 
                + 14 *element.b*element.b*element.b*element.b* (element.d1* member.i3 / (420 *element.b*element.b* element.b)) 
                - 3 *element.b*element.b*element.b*element.b*element.b*element.b* (element.dy * member.i4 / (420 *element.b*element.b* element.b)) 
                - 56 *element.b*element.b*element.b*element.b* (element.dxy * member.i5 / (420 *element.b*element.b* element.b)));

            element.keb[3, 2] = (-2520 *element.b* (element.dx * member.i1 / (420 *element.b*element.b* element.b)) 
                + 42 *element.b*element.b*element.b* (element.d1* member.i2 / (420 *element.b*element.b* element.b)) 
                + 462 *element.b*element.b*element.b* (element.d1* member.i3 / (420 *element.b*element.b* element.b)) 
                - 22 *element.b*element.b*element.b*element.b*element.b* (element.dy * member.i4 / (420 *element.b*element.b* element.b)) 
                - 168 *element.b*element.b*element.b* (element.dxy * member.i5 / (420 *element.b*element.b* element.b)));

            element.keb[3, 3] = (1680 *element.b*element.b* (element.dx * member.i1 / (420 *element.b*element.b* element.b))
                - 56 *element.b*element.b*element.b*element.b* (element.d1* member.i2 / (420 *element.b*element.b* element.b))
                - 56 *element.b*element.b*element.b*element.b* (element.d1* member.i3 / (420 *element.b*element.b* element.b)) 
                + 4 *element.b*element.b*element.b*element.b*element.b*element.b* (element.dy * member.i4 / (420 *element.b*element.b* element.b)) 
                + 224 *element.b*element.b*element.b*element.b* (element.dxy * member.i5 / (420 *element.b*element.b* element.b)));

            //Kgm
            element.kgm[0, 0] = (3 * element.t1 + element.t2) * element.b * member.i5 / 12;
            element.kgm[0, 1] = 0;
            element.kgm[0, 2] = (element.t1 + element.t2) * element.b * member.i5 / 12;
            element.kgm[0, 3] = 0;
            element.kgm[1, 0] = 0;
            element.kgm[1, 1] = (3 * element.t1 + element.t2) * element.b * member.a * member.a * member.i4 / (12 * member.meum * member.meun);
            element.kgm[1, 2] = 0;
            element.kgm[1, 3] = (element.t1 + element.t2) * element.b * member.a * member.a * member.i4 / (12 * member.meum * member.meun);
            element.kgm[2, 0] = (element.t1 + element.t2) * element.b * member.i5 / 12;
            element.kgm[2, 1] = 0;
            element.kgm[2, 2] = (element.t1 + 3 * element.t2) * element.b * member.i5 / 12;
            element.kgm[2, 3] = 0;
            element.kgm[3, 0] = 0;
            element.kgm[3, 1] = (element.t1 + element.t2) * element.b * member.a * member.a * member.i4 / (12 * member.meum * member.meun);
            element.kgm[3, 2] = 0;
            element.kgm[3, 3] = (element.t1 + 3 * element.t2) * element.b * member.a * member.a * member.i4 / (12 * member.meum * member.meun);

            //Kgb
            element.kgb[0, 0] = (10 * element.t1 + 3 * element.t2) * element.b * member.i5 / 35;
            element.kgb[0, 1] = (15 * element.t1 + 7 * element.t2) * element.b * element.b * member.i5 / 420;
            element.kgb[0, 2] = (9 * element.t1 + 9 * element.t2) * element.b * member.i5 / 140;
            element.kgb[0, 3] = (-7 * element.t1 + 6 * element.t2) * element.b * element.b * member.i5 / 420;
            element.kgb[1, 0] = (15 * element.t1 + 7 * element.t2) * element.b * element.b * member.i5 / 420;
            element.kgb[1, 1] = (5 * element.t1 + 3 * element.t2) * element.b * element.b * element.b * member.i5 / 840;
            element.kgb[1, 2] = (6 * element.t1 + 7 * element.t2) * element.b * element.b * member.i5 / 420;
            element.kgb[1, 3] = -(element.t1 + element.t2) * element.b * element.b * element.b * member.i5 / 280;
            element.kgb[2, 0] = (9 * element.t1 + 9 * element.t2) * element.b * member.i5 / 140;
            element.kgb[2, 1] = (6 * element.t1 + 7 * element.t2) * element.b * element.b * member.i5 / 420;
            element.kgb[2, 2] = (3 * element.t1 + 10 * element.t2) * element.b * member.i5 / 35;
            element.kgb[2, 3] = -(7 * element.t1 + 15 * element.t2) * element.b * element.b * member.i5 / 420;
            element.kgb[3, 0] = -(7 * element.t1 + 6 * element.t2) * element.b * element.b * member.i5 / 420;
            element.kgb[3, 1] = -(element.t1 + element.t2) * element.b * element.b * element.b * member.i5 / 280;
            element.kgb[3, 2] = -(7 * element.t1 + 15 * element.t2) * element.b * element.b * member.i5 / 420;
            element.kgb[3, 3] = (3 * element.t1 + 5 * element.t2) * element.b * element.b * element.b * member.i5 / 840;

        }

        private void SolveButton_Click(object sender, EventArgs e)
        {
            GetMaterialValues();
            GetElementValues();
            GetMemberValues();
            SolveMaterial();
            PreSolveElement();
            PreSolveMember();
            PreSolveMatrices();


        }
    }
}
