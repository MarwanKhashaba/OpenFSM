using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using EigenValue_Problem.Models;
using MathNet.Numerics.LinearAlgebra;
using Syncfusion.Data.Extensions;
using Syncfusion.Windows.Forms.Chart;
using Syncfusion.Windows.Forms.Tools;

namespace EigenValue_Problem
{
    public partial class NewForm : Form
    {
        //Material material = new Material(0, 0, 0, 0 ,0);
        //Node node = new Node(0, 0);
        List<Element> paintelements = new List<Element>();
        public bool IsParametric = false;
        Section section = new Section();
        //Member member = new Member(0, 0, 0, 0);
        LoadCases loadcases = new LoadCases(0, 0, 0);
        Design design = new Design();
        //Springs springs = new Springs(0, 0, 0);
        //Constraints constraints = new Constraints(0, 0, 0, 0);
        Dictionary<Member, double[,]> globalStiff = new Dictionary<Member, double[,]>();
        private List<Node> Nodes;
        private List<Member> Members;
        private string BoundryCondition;


        //For Testing
        Vector<double> EigenValues;


        //For output naming
        private string output1stPartPathName;
        private string outputPathName;
        private int runs;

        //For signature curve
        private BindingList<SignatureCurvePoint> signatureCurvePoints;

        public NewForm()
        {
            runs = 0;

            InitializeComponent();

            ShowSplash();
            splashPanel1.SuspendLayout();



            //Material concrete25 = new Material(4000, 4000, 0.2, 0.2);
            //Material concrete30 = new Material(5000, 4000, 0.3, 0.3);
            //Material Custom = new Material(0, 0, 0, 0);
            //List<Material> materials = new List<Material> { concrete25, concrete30 };

            //foreach (Material material in materials)
            //{
            //    //materialComboBox.Items.Add(material.ex);
            //}

        }

        private void SetupOutputFolder()
        {
            if (runs < 1)
            {
                output1stPartPathName = DateTime.Now.ToString("dd-MM-yyyy h-mm-ss tt");
            }
            runs++;
            outputPathName = Path.Combine("OpenFSM", Environment.GetFolderPath(
                    Environment.SpecialFolder.MyDocuments), output1stPartPathName, "Run no." + runs);
            if (!File.Exists(outputPathName))
            {
                Directory.CreateDirectory(outputPathName);
            }
        }

        private void ShowSplash()
        {
            SplashPanel currentPanel = splashPanel1;
            currentPanel.ShowDialogSplash(this);
        }
        private void btndesigncompression_Click(object sender, EventArgs e)
        {
            design.Py = double.Parse(PytextBox.Text);

            design.Pcrl = double.Parse(PcrltextBox.Text);
            design.Pcrd = double.Parse(PcrdtextBox.Text);
            design.PcrG = double.Parse(PcrgtextBox.Text);

            SolveDesignEquations();

            labelPulLocal.Text = design.Pull.ToString();
            labelPulDistortional.Text = design.Puld.ToString();
            labelPulGlobal.Text = design.PulG.ToString();
        }

        private void btndesignmoment_Click(object sender, EventArgs e)
        {
            design.Mxy = double.Parse(MxytextBox.Text);
            design.Mp = double.Parse(MxptextBox.Text);

            design.Mcrl = double.Parse(McrltextBox.Text);
            design.Mcrd = double.Parse(McrdtextBox.Text);
            design.McrLTB = double.Parse(McrLTBtextBox.Text);

            SolveDesignEquations();

            labelMulLocal.Text = design.Mull.ToString();
            labelMulDistortional.Text = design.Muld.ToString();
            labelMulLTB.Text = design.MulLTB.ToString();
        }

        private void refreshPullResults(object sender, Syncfusion.WinForms.Input.Events.ValueChangedEventArgs e)
        {
            labelPulLocal.Text = @"Will be computed ..";
            labelPulDistortional.Text = @"Will be computed ..";
            labelPulGlobal.Text = @"Will be computed ..";
        }

        private void refreshMullResults(object sender, Syncfusion.WinForms.Input.Events.ValueChangedEventArgs e)
        {
            labelMulLocal.Text = @"Will be computed ..";
            labelMulDistortional.Text = @"Will be computed ..";
            labelMulLTB.Text = @"Will be computed ..";
        }



        //private void MaterialComboBox_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //if (materialComboBox.Text.ToString() == "Concrete 25")
        // {
        //this will be added later
        //}
        // }

        public List<Material> GetMaterialValues(string text = "")
        {
            //material.Ex = double.Parse(exTextBox.Text);
            //material.Ey = double.Parse(eyTextBox.Text);
            //material.Vx = double.Parse(vxTextBox.Text);
            //material.Vy = double.Parse(vyTextBox.Text);
            //material.fy = double.Parse(fytextBox.Text);

            // STEP ONE: READ TEXT
            // VERIFY THAT INPUT is correct

            if (!IsParametric) //Not parametric
            {
                //Get Input Values
                text = txt_materialsData.Text;
            }
            else
            {
                text = parmaterialTextBox.Text;
            }

            string[] lines = text.Split('\n');

            List<Material> materials = new List<Material>();

            foreach (var line in lines)
            {
                string[] values = line.Trim().Split(',');
                var id = values[0];
                var Ex = values[1];
                var Ey = values[2];
                var Vx = values[3];
                var Vy = values[4];
                var fy = values[5];


                // Create a new Material
                Material material1 = new Material();
                material1.ID = int.Parse(id);
                material1.Ex = double.Parse(Ex);
                material1.Ey = double.Parse(Ey);
                material1.Vx = double.Parse(Vx);
                material1.Vy = double.Parse(Vy);
                material1.fy = double.Parse(fy);


                // Add to materials array
                materials.Add(material1);
            }
            return materials;
        }

        public void Getsectionvalues(double[] sectionsdata = null)
        {
            if (sectionsdata == null)
            {
                //Not Testing
                section.b = double.Parse(bTextBox.Text); //relate to interface
                section.h = double.Parse(hTextBox.Text);
                section.d = double.Parse(dTextBox.Text);
                section.r = double.Parse(rTextBox.Text);
                section.t = double.Parse(tTextBox.Text);
                section.w = double.Parse(wTextBox.Text); //if not hat make it zero
                section.s = double.Parse(sTextBox.Text);  //if not hat make it zero

            }
            else
            {
                //Testing

                section.b = (sectionsdata[0]);
                section.h = (sectionsdata[1]);
                section.d = (sectionsdata[2]);
                section.r = (sectionsdata[3]);
                section.t = (sectionsdata[4]);
                section.w = (sectionsdata[5]);
                section.s = (sectionsdata[6]);

            }

        }

        public void MESHSECTION()
        {


            if ((string)sectionComboBox.SelectedItem == "General") return;

            section.J1 = (2 * (Math.Pow(2, 0.5)) / 3) * section.r;
            section.J2 = ((Math.Pow(5, 0.5)) / 3) * section.r;
            section.J3 = ((Math.Pow(3, 0.5) / 2)) * section.r;


            //if sectionComboBox.SelectedItem="HAT-Section with fillet"

            if ((string)sectionComboBox.SelectedItem == "HAT-Section with fillet")
            {

                section.node1x = -(section.b / 2) - (section.d);
                section.node1y = 0;
                section.node2x = -(section.b / 2) - (section.d / 2);
                section.node2y = 0;
                section.node3x = -(section.b / 2) - (section.r);
                section.node3y = 0;
                section.node4x = -section.b / 2 - section.r + section.J2;
                section.node4y = section.r / 3;
                section.node5x = -section.b / 2 - section.r + section.J3;
                section.node5y = section.r / 2;
                section.node6x = -section.b / 2 - section.r + section.J1;
                section.node6y = 2 * section.r / 3;
                section.node7x = -(section.b / 2);
                section.node7y = section.r;
                section.node8x = -(section.b / 2);
                section.node8y = section.h / 2;
                section.node9x = -section.b / 2;
                section.node9y = section.h - section.r;
                section.node10x = -section.b / 2 + section.r - section.J1;
                section.node10y = section.h - (2 * section.r / 3);
                section.node11x = -section.b / 2 + section.r - section.J3;
                section.node11y = section.h - section.r / 2;
                section.node12x = -section.b / 2 + section.r - section.J2;
                section.node12y = section.h - section.r / 3;
                section.node13x = -(section.b / 2) + section.r;
                section.node13y = section.h;
                section.node14x = -section.b / 4;
                section.node14y = section.h;
                section.node15x = -section.w / 2;
                section.node15y = section.h;
                section.node16x = 0;
                section.node16y = section.h - section.s;
                section.node17x = section.w / 2;
                section.node17y = section.h;
                section.node18x = section.b / 4;
                section.node18y = section.h;
                section.node19x = (section.b / 2) - section.r;
                section.node19y = section.h;
                section.node20x = -section.node12x;
                section.node20y = section.h - section.r / 3;
                section.node21x = -section.node11x;
                section.node21y = section.h - section.r / 2;
                section.node22x = -section.node10x;
                section.node22y = section.h - (2 * section.r / 3);
                section.node23x = section.b / 2;
                section.node23y = section.h - section.r;
                section.node24x = section.b / 2;
                section.node24y = section.h / 2;
                section.node25x = section.b / 2;
                section.node25y = section.r;
                section.node26x = -section.node6x;
                section.node26y = 2 * section.r / 3;
                section.node27x = -section.node5x;
                section.node27y = section.r / 2;
                section.node28x = -section.node4x;
                section.node28y = section.r / 3;
                section.node29x = (section.b / 2) + (section.r);
                section.node29y = 0;
                section.node30x = (section.b / 2) + (section.d / 2);
                section.node30y = 0;
                section.node31x = (section.b / 2) + (section.d);
                section.node31y = 0;

                section.sectionfinaltextnode = "1" + "," + section.node1x + "," + section.node1y + Environment.NewLine
                                               + "2" + "," + section.node2x + "," + section.node2y + Environment.NewLine
                                               + "3" + "," + section.node3x + "," + section.node3y + Environment.NewLine
                                               + "4" + "," + section.node4x + "," + section.node4y + Environment.NewLine
                                               + "5" + "," + section.node5x + "," + section.node5y + Environment.NewLine
                                               + "6" + "," + section.node6x + "," + section.node6y + Environment.NewLine
                                               + "7" + "," + section.node7x + "," + section.node7y + Environment.NewLine
                                               + "8" + "," + section.node8x + "," + section.node8y + Environment.NewLine
                                               + "9" + "," + section.node9x + "," + section.node9y + Environment.NewLine
                                               + "10" + "," + section.node10x + "," + section.node10y +
                                               Environment.NewLine
                                               + "11" + "," + section.node11x + "," + section.node11y +
                                               Environment.NewLine
                                               + "12" + "," + section.node12x + "," + section.node12y +
                                               Environment.NewLine
                                               + "13" + "," + section.node13x + "," + section.node13y +
                                               Environment.NewLine
                                               + "14" + "," + section.node14x + "," + section.node14y +
                                               Environment.NewLine
                                               + "15" + "," + section.node15x + "," + section.node15y +
                                               Environment.NewLine
                                               + "16" + "," + section.node16x + "," + section.node16y +
                                               Environment.NewLine
                                               + "17" + "," + section.node17x + "," + section.node17y +
                                               Environment.NewLine
                                               + "18" + "," + section.node18x + "," + section.node18y +
                                               Environment.NewLine
                                               + "19" + "," + section.node19x + "," + section.node19y +
                                               Environment.NewLine
                                               + "20" + "," + section.node20x + "," + section.node20y +
                                               Environment.NewLine
                                               + "21" + "," + section.node21x + "," + section.node21y +
                                               Environment.NewLine
                                               + "22" + "," + section.node22x + "," + section.node22y +
                                               Environment.NewLine
                                               + "23" + "," + section.node23x + "," + section.node23y +
                                               Environment.NewLine
                                               + "24" + "," + section.node24x + "," + section.node24y +
                                               Environment.NewLine
                                               + "25" + "," + section.node25x + "," + section.node25y +
                                               Environment.NewLine
                                               + "26" + "," + section.node26x + "," + section.node26y +
                                               Environment.NewLine
                                               + "27" + "," + section.node27x + "," + section.node27y +
                                               Environment.NewLine
                                               + "28" + "," + section.node28x + "," + section.node28y +
                                               Environment.NewLine
                                               + "29" + "," + section.node29x + "," + section.node29y +
                                               Environment.NewLine
                                               + "30" + "," + section.node30x + "," + section.node30y +
                                               Environment.NewLine
                                               + "31" + "," + section.node31x + "," + section.node31y;

                section.sectionfinaltextelement = "1" + "," + "1" + "," + "2" + "," + section.t + Environment.NewLine
                                                  + "2" + "," + "2" + "," + "3" + "," + section.t + Environment.NewLine
                                                  + "3" + "," + "3" + "," + "4" + "," + section.t + Environment.NewLine
                                                  + "4" + "," + "4" + "," + "5" + "," + section.t + Environment.NewLine
                                                  + "5" + "," + "5" + "," + "6" + "," + section.t + Environment.NewLine
                                                  + "6" + "," + "6" + "," + "7" + "," + section.t + Environment.NewLine
                                                  + "7" + "," + "7" + "," + "8" + "," + section.t + Environment.NewLine
                                                  + "8" + "," + "8" + "," + "9" + "," + section.t + Environment.NewLine
                                                  + "9" + "," + "9" + "," + "10" + "," + section.t + Environment.NewLine
                                                  + "10" + "," + "10" + "," + "11" + "," + section.t + Environment.NewLine
                                                  + "11" + "," + "11" + "," + "12" + "," + section.t + Environment.NewLine
                                                  + "12" + "," + "12" + "," + "13" + "," + section.t + Environment.NewLine
                                                  + "13" + "," + "13" + "," + "14" + "," + section.t + Environment.NewLine
                                                  + "14" + "," + "14" + "," + "15" + "," + section.t + Environment.NewLine
                                                  + "15" + "," + "15" + "," + "16" + "," + section.t + Environment.NewLine
                                                  + "16" + "," + "16" + "," + "17" + "," + section.t + Environment.NewLine
                                                  + "17" + "," + "17" + "," + "18" + "," + section.t + Environment.NewLine
                                                  + "18" + "," + "18" + "," + "19" + "," + section.t + Environment.NewLine
                                                  + "19" + "," + "19" + "," + "20" + "," + section.t + Environment.NewLine
                                                  + "20" + "," + "20" + "," + "21" + "," + section.t + Environment.NewLine
                                                  + "21" + "," + "21" + "," + "22" + "," + section.t + Environment.NewLine
                                                  + "22" + "," + "22" + "," + "23" + "," + section.t + Environment.NewLine
                                                  + "23" + "," + "23" + "," + "24" + "," + section.t + Environment.NewLine
                                                  + "24" + "," + "24" + "," + "25" + "," + section.t + Environment.NewLine
                                                  + "25" + "," + "25" + "," + "26" + "," + section.t + Environment.NewLine
                                                  + "26" + "," + "26" + "," + "27" + "," + section.t + Environment.NewLine
                                                  + "27" + "," + "27" + "," + "28" + "," + section.t + Environment.NewLine
                                                  + "28" + "," + "28" + "," + "29" + "," + section.t + Environment.NewLine
                                                  + "29" + "," + "29" + "," + "30" + "," + section.t + Environment.NewLine
                                                  + "30" + "," + "30" + "," + "31" + "," + section.t;
            }
            //if sectionComboBox.SelectedItem=""HAT - section with no fillet"
            else if ((string)sectionComboBox.SelectedItem == "HAT-Section with no fillet")
            {

                section.r = 0;
                section.node1x = -(section.b / 2) - (section.d);
                section.node1y = 0;
                section.node2x = -(section.b / 2) - (section.d / 2);
                section.node2y = 0;
                section.node3x = -(section.b / 2) - (0); //r
                section.node3y = 0;
                section.node4x = -(section.b / 2);
                section.node4y = section.h / 2;
                section.node5x = -section.b / 2;
                section.node5y = section.h - 0; //r
                section.node6x = -section.b / 4;
                section.node6y = section.h;
                section.node7x = -section.w / 2;
                section.node7y = section.h;
                section.node8x = 0;
                section.node8y = section.h - section.s;
                section.node9x = section.w / 2;
                section.node9y = section.h;
                section.node10x = section.b / 4;
                section.node10y = section.h;
                section.node11x = (section.b / 2) - 0; //r
                section.node11y = section.h;
                section.node12x = -section.node4x;
                section.node12y = section.node4y;
                section.node13x = section.b / 2;
                section.node13y = section.node3y;
                section.node14x = -section.node2x;
                section.node14y = section.node2y;
                section.node15x = -section.node1x;
                section.node15y = section.node1y;

                section.sectionfinaltextnode = "1" + "," + section.node1x + "," + section.node1y + Environment.NewLine
                                               + "2" + "," + section.node2x + "," + section.node2y + Environment.NewLine
                                               + "3" + "," + section.node3x + "," + section.node3y + Environment.NewLine
                                               + "4" + "," + section.node4x + "," + section.node4y + Environment.NewLine
                                               + "5" + "," + section.node5x + "," + section.node5y + Environment.NewLine
                                               + "6" + "," + section.node6x + "," + section.node6y + Environment.NewLine
                                               + "7" + "," + section.node7x + "," + section.node7y + Environment.NewLine
                                               + "8" + "," + section.node8x + "," + section.node8y + Environment.NewLine
                                               + "9" + "," + section.node9x + "," + section.node9y + Environment.NewLine
                                               + "10" + "," + section.node10x + "," + section.node10y +
                                               Environment.NewLine
                                               + "11" + "," + section.node11x + "," + section.node11y +
                                               Environment.NewLine
                                               + "12" + "," + section.node12x + "," + section.node12y +
                                               Environment.NewLine
                                               + "13" + "," + section.node13x + "," + section.node13y +
                                               Environment.NewLine
                                               + "14" + "," + section.node14x + "," + section.node14y +
                                               Environment.NewLine
                                               + "15" + "," + section.node15x + "," + section.node15y;

                section.sectionfinaltextelement = "1" + "," + "1" + "," + "2" + "," + section.t + Environment.NewLine
                                                  + "2" + "," + "2" + "," + "3" + "," + section.t + Environment.NewLine
                                                  + "3" + "," + "3" + "," + "4" + "," + section.t + Environment.NewLine
                                                  + "4" + "," + "4" + "," + "5" + "," + section.t + Environment.NewLine
                                                  + "5" + "," + "5" + "," + "6" + "," + section.t + Environment.NewLine
                                                  + "6" + "," + "6" + "," + "7" + "," + section.t + Environment.NewLine
                                                  + "7" + "," + "7" + "," + "8" + "," + section.t + Environment.NewLine
                                                  + "8" + "," + "8" + "," + "9" + "," + section.t + Environment.NewLine
                                                  + "9" + "," + "9" + "," + "10" + "," + section.t + Environment.NewLine
                                                  + "10" + "," + "10" + "," + "11" + "," + section.t + Environment.NewLine
                                                  + "11" + "," + "11" + "," + "12" + "," + section.t + Environment.NewLine
                                                  + "12" + "," + "12" + "," + "13" + "," + section.t + Environment.NewLine
                                                  + "13" + "," + "13" + "," + "14" + "," + section.t + Environment.NewLine
                                                  + "14" + "," + "14" + "," + "15" + "," + section.t;

            }

            //if sectionComboBox.SelectedItem="C - section lipped with fillet"
            else if ((string)sectionComboBox.SelectedItem == "C - Section lipped with fillet")
            {

                section.w = 0; //if not hat make it zero
                section.s = 0;  //if not hat make it zero
                section.node1x = section.b;
                section.node1y = -section.h / 2 + section.d;
                section.node2x = section.b;
                section.node2y = -section.h / 2 + section.r;
                section.node3x = section.b - section.r + section.J1;
                section.node3y = -section.h / 2 + (2 * section.r / 3);
                section.node4x = section.b - section.r + section.J3;
                section.node4y = -section.h / 2 + (section.r / 2);
                section.node5x = section.b - section.r + section.J2;
                section.node5y = -section.h / 2 + (section.r / 3);
                section.node6x = section.b - section.r;
                section.node6y = -section.h / 2;
                section.node7x = section.b / 2;
                section.node7y = -section.h / 2;
                section.node8x = section.r;
                section.node8y = -section.h / 2;
                section.node9x = section.r - section.J2;
                section.node9y = -section.h / 2 + section.r - 2 * section.r / 3;
                section.node10x = section.r - section.J3;
                section.node10y = -section.h / 2 + section.r - section.r / 2;
                section.node11x = section.r - section.J1;
                section.node11y = -section.h / 2 + section.r - section.r / 3;
                section.node12x = 0;
                section.node12y = -section.h / 2 + section.r;
                section.node13x = 0;
                section.node13y = -section.h / 4;
                section.node14x = 0;
                section.node14y = 0;
                section.node15x = 0;
                section.node15y = section.h / 4;
                section.node16x = 0;
                section.node16y = section.h / 2 - section.r;
                section.node17x = section.r - section.J1;
                section.node17y = section.h / 2 - section.r + section.r / 3;
                section.node18x = section.r - section.J3;
                section.node18y = section.h / 2 - section.r + section.r / 2;
                section.node19x = section.r - section.J2;
                section.node19y = section.h / 2 - section.r + 2 * section.r / 3;
                section.node20x = section.r;
                section.node20y = section.h / 2;
                section.node21x = section.b / 2;
                section.node21y = section.h / 2;
                section.node22x = section.b - section.r;
                section.node22y = section.h / 2;
                section.node23x = section.b - section.r + section.J2;
                section.node23y = section.h / 2 - section.r / 3;
                section.node24x = section.b - section.r + section.J3;
                section.node24y = section.h / 2 - section.r / 2;
                section.node25x = section.b - section.r + section.J1;
                section.node25y = section.h / 2 - 2 * section.r / 3;
                section.node26x = section.b;
                section.node26y = section.h / 2 - section.r;
                section.node27x = section.b;
                section.node27y = section.h / 2 - section.d;

                section.sectionfinaltextnode = "1" + "," + section.node1x + "," + section.node1y + Environment.NewLine
                                               + "2" + "," + section.node2x + "," + section.node2y + Environment.NewLine
                                               + "3" + "," + section.node3x + "," + section.node3y + Environment.NewLine
                                               + "4" + "," + section.node4x + "," + section.node4y + Environment.NewLine
                                               + "5" + "," + section.node5x + "," + section.node5y + Environment.NewLine
                                               + "6" + "," + section.node6x + "," + section.node6y + Environment.NewLine
                                               + "7" + "," + section.node7x + "," + section.node7y + Environment.NewLine
                                               + "8" + "," + section.node8x + "," + section.node8y + Environment.NewLine
                                               + "9" + "," + section.node9x + "," + section.node9y + Environment.NewLine
                                               + "10" + "," + section.node10x + "," + section.node10y +
                                               Environment.NewLine
                                               + "11" + "," + section.node11x + "," + section.node11y +
                                               Environment.NewLine
                                               + "12" + "," + section.node12x + "," + section.node12y +
                                               Environment.NewLine
                                               + "13" + "," + section.node13x + "," + section.node13y +
                                               Environment.NewLine
                                               + "14" + "," + section.node14x + "," + section.node14y +
                                               Environment.NewLine
                                               + "15" + "," + section.node15x + "," + section.node15y +
                                               Environment.NewLine
                                               + "16" + "," + section.node16x + "," + section.node16y +
                                               Environment.NewLine
                                               + "17" + "," + section.node17x + "," + section.node17y +
                                               Environment.NewLine
                                               + "18" + "," + section.node18x + "," + section.node18y +
                                               Environment.NewLine
                                               + "19" + "," + section.node19x + "," + section.node19y +
                                               Environment.NewLine
                                               + "20" + "," + section.node20x + "," + section.node20y +
                                               Environment.NewLine
                                               + "21" + "," + section.node21x + "," + section.node21y +
                                               Environment.NewLine
                                               + "22" + "," + section.node22x + "," + section.node22y +
                                               Environment.NewLine
                                               + "23" + "," + section.node23x + "," + section.node23y +
                                               Environment.NewLine
                                               + "24" + "," + section.node24x + "," + section.node24y +
                                               Environment.NewLine
                                               + "25" + "," + section.node25x + "," + section.node25y +
                                               Environment.NewLine
                                               + "26" + "," + section.node26x + "," + section.node26y +
                                               Environment.NewLine
                                               + "27" + "," + section.node27x + "," + section.node27y;


                section.sectionfinaltextelement = "1" + "," + "1" + "," + "2" + "," + section.t + Environment.NewLine
                                                  + "2" + "," + "2" + "," + "3" + "," + section.t + Environment.NewLine
                                                  + "3" + "," + "3" + "," + "4" + "," + section.t + Environment.NewLine
                                                  + "4" + "," + "4" + "," + "5" + "," + section.t + Environment.NewLine
                                                  + "5" + "," + "5" + "," + "6" + "," + section.t + Environment.NewLine
                                                  + "6" + "," + "6" + "," + "7" + "," + section.t + Environment.NewLine
                                                  + "7" + "," + "7" + "," + "8" + "," + section.t + Environment.NewLine
                                                  + "8" + "," + "8" + "," + "9" + "," + section.t + Environment.NewLine
                                                  + "9" + "," + "9" + "," + "10" + "," + section.t + Environment.NewLine
                                                  + "10" + "," + "10" + "," + "11" + "," + section.t + Environment.NewLine
                                                  + "11" + "," + "11" + "," + "12" + "," + section.t + Environment.NewLine
                                                  + "12" + "," + "12" + "," + "13" + "," + section.t + Environment.NewLine
                                                  + "13" + "," + "13" + "," + "14" + "," + section.t + Environment.NewLine
                                                  + "14" + "," + "14" + "," + "15" + "," + section.t + Environment.NewLine
                                                  + "15" + "," + "15" + "," + "16" + "," + section.t + Environment.NewLine
                                                  + "16" + "," + "16" + "," + "17" + "," + section.t + Environment.NewLine
                                                  + "17" + "," + "17" + "," + "18" + "," + section.t + Environment.NewLine
                                                  + "18" + "," + "18" + "," + "19" + "," + section.t + Environment.NewLine
                                                  + "19" + "," + "19" + "," + "20" + "," + section.t + Environment.NewLine
                                                  + "20" + "," + "20" + "," + "21" + "," + section.t + Environment.NewLine
                                                  + "21" + "," + "21" + "," + "22" + "," + section.t + Environment.NewLine
                                                  + "22" + "," + "22" + "," + "23" + "," + section.t + Environment.NewLine
                                                  + "23" + "," + "23" + "," + "24" + "," + section.t + Environment.NewLine
                                                  + "24" + "," + "24" + "," + "25" + "," + section.t + Environment.NewLine
                                                  + "25" + "," + "25" + "," + "26" + "," + section.t + Environment.NewLine
                                                  + "26" + "," + "26" + "," + "27" + "," + section.t;
            }

            //if sectionComboBox.SelectedItem="C - section with fillet(no lip)"
            else if ((string)sectionComboBox.SelectedItem == "C - Section with fillet (no lip)")
            {

                section.w = 0; //if not hat make it zero
                section.s = 0;  //if not hat make it zero
                section.node1x = section.b;
                section.node1y = -section.h / 2;
                section.node2x = section.b / 2;
                section.node2y = -section.h / 2;
                section.node3x = section.r;
                section.node3y = -section.h / 2;
                section.node4x = section.r - section.J2;
                section.node4y = -section.h / 2 + section.r - 2 * section.r / 3;
                section.node5x = section.r - section.J3;
                section.node5y = -section.h / 2 + section.r - section.r / 2;
                section.node6x = section.r - section.J1;
                section.node6y = -section.h / 2 + section.r - section.r / 3;
                section.node7x = 0;
                section.node7y = -section.h / 2 + section.r;
                section.node8x = 0;
                section.node8y = -section.h / 4;
                section.node9x = 0;
                section.node9y = 0;
                section.node10x = 0;
                section.node10y = section.h / 4;
                section.node11x = 0;
                section.node11y = section.h / 2 - section.r;
                section.node12x = section.r - section.J1;
                section.node12y = section.h / 2 - section.r + section.r / 3;
                section.node13x = section.r - section.J3;
                section.node13y = section.h / 2 - section.r + section.r / 2;
                section.node14x = section.r - section.J2;
                section.node14y = section.h / 2 - section.r + 2 * section.r / 3;
                section.node15x = section.r;
                section.node15y = section.h / 2;
                section.node16x = section.b / 2;
                section.node16y = section.h / 2;
                section.node17x = section.b;
                section.node17y = section.h / 2;

                section.sectionfinaltextnode = "1" + "," + section.node1x + "," + section.node1y + Environment.NewLine
                                               + "2" + "," + section.node2x + "," + section.node2y + Environment.NewLine
                                               + "3" + "," + section.node3x + "," + section.node3y + Environment.NewLine
                                               + "4" + "," + section.node4x + "," + section.node4y + Environment.NewLine
                                               + "5" + "," + section.node5x + "," + section.node5y + Environment.NewLine
                                               + "6" + "," + section.node6x + "," + section.node6y + Environment.NewLine
                                               + "7" + "," + section.node7x + "," + section.node7y + Environment.NewLine
                                               + "8" + "," + section.node8x + "," + section.node8y + Environment.NewLine
                                               + "9" + "," + section.node9x + "," + section.node9y + Environment.NewLine
                                               + "10" + "," + section.node10x + "," + section.node10y +
                                               Environment.NewLine
                                               + "11" + "," + section.node11x + "," + section.node11y +
                                               Environment.NewLine
                                               + "12" + "," + section.node12x + "," + section.node12y +
                                               Environment.NewLine
                                               + "13" + "," + section.node13x + "," + section.node13y +
                                               Environment.NewLine
                                               + "14" + "," + section.node14x + "," + section.node14y +
                                               Environment.NewLine
                                               + "15" + "," + section.node15x + "," + section.node15y +
                                               Environment.NewLine
                                               + "16" + "," + section.node16x + "," + section.node16y +
                                               Environment.NewLine
                                               + "17" + "," + section.node17x + "," + section.node17y;

                section.sectionfinaltextelement = "1" + "," + "1" + "," + "2" + "," + section.t + Environment.NewLine
                                                  + "2" + "," + "2" + "," + "3" + "," + section.t + Environment.NewLine
                                                  + "3" + "," + "3" + "," + "4" + "," + section.t + Environment.NewLine
                                                  + "4" + "," + "4" + "," + "5" + "," + section.t + Environment.NewLine
                                                  + "5" + "," + "5" + "," + "6" + "," + section.t + Environment.NewLine
                                                  + "6" + "," + "6" + "," + "7" + "," + section.t + Environment.NewLine
                                                  + "7" + "," + "7" + "," + "8" + "," + section.t + Environment.NewLine
                                                  + "8" + "," + "8" + "," + "9" + "," + section.t + Environment.NewLine
                                                  + "9" + "," + "9" + "," + "10" + "," + section.t + Environment.NewLine
                                                  + "10" + "," + "10" + "," + "11" + "," + section.t + Environment.NewLine
                                                  + "11" + "," + "11" + "," + "12" + "," + section.t + Environment.NewLine
                                                  + "12" + "," + "12" + "," + "13" + "," + section.t + Environment.NewLine
                                                  + "13" + "," + "13" + "," + "14" + "," + section.t + Environment.NewLine
                                                  + "14" + "," + "14" + "," + "15" + "," + section.t + Environment.NewLine
                                                  + "15" + "," + "15" + "," + "16" + "," + section.t + Environment.NewLine
                                                  + "16" + "," + "16" + "," + "17" + "," + section.t;
            }

            //if sectionComboBox.SelectedItem="C - section lipped with no fillet"
            else if ((string)sectionComboBox.SelectedItem == "C - Section lipped with no fillet")
            {

                section.r = 0;
                section.w = 0; //if not hat make it zero
                section.s = 0;  //if not hat make it zero

                section.node1x = section.b;
                section.node1y = -section.h / 2 + section.d;
                section.node2x = section.b;
                section.node2y = -section.h / 2 + 0; //r
                section.node3x = section.b / 2;
                section.node3y = -section.h / 2;
                section.node4x = 0; //r
                section.node4y = -section.h / 2;
                section.node5x = 0;
                section.node5y = -section.h / 4;
                section.node6x = 0;
                section.node6y = 0;
                section.node7x = 0;
                section.node7y = section.h / 4;
                section.node8x = 0;
                section.node8y = section.h / 2 - 0; //r
                section.node9x = section.b / 2;
                section.node9y = section.h / 2;
                section.node10x = section.b - 0; //r
                section.node10y = section.h / 2;
                section.node11x = section.b;
                section.node11y = section.h / 2 - section.d;

                section.sectionfinaltextnode = "1" + "," + section.node1x + "," + section.node1y + Environment.NewLine
                                               + "2" + "," + section.node2x + "," + section.node2y + Environment.NewLine
                                               + "3" + "," + section.node3x + "," + section.node3y + Environment.NewLine
                                               + "4" + "," + section.node4x + "," + section.node4y + Environment.NewLine
                                               + "5" + "," + section.node5x + "," + section.node5y + Environment.NewLine
                                               + "6" + "," + section.node6x + "," + section.node6y + Environment.NewLine
                                               + "7" + "," + section.node7x + "," + section.node7y + Environment.NewLine
                                               + "8" + "," + section.node8x + "," + section.node8y + Environment.NewLine
                                               + "9" + "," + section.node9x + "," + section.node9y + Environment.NewLine
                                               + "10" + "," + section.node10x + "," + section.node10y +
                                               Environment.NewLine
                                               + "11" + "," + section.node11x + "," + section.node11y;

                section.sectionfinaltextelement = "1" + "," + "1" + "," + "2" + "," + section.t + Environment.NewLine
                                                  + "2" + "," + "2" + "," + "3" + "," + section.t + Environment.NewLine
                                                  + "3" + "," + "3" + "," + "4" + "," + section.t + Environment.NewLine
                                                  + "4" + "," + "4" + "," + "5" + "," + section.t + Environment.NewLine
                                                  + "5" + "," + "5" + "," + "6" + "," + section.t + Environment.NewLine
                                                  + "6" + "," + "6" + "," + "7" + "," + section.t + Environment.NewLine
                                                  + "7" + "," + "7" + "," + "8" + "," + section.t + Environment.NewLine
                                                  + "8" + "," + "8" + "," + "9" + "," + section.t + Environment.NewLine
                                                  + "9" + "," + "9" + "," + "10" + "," + section.t + Environment.NewLine
                                                  + "10" + "," + "10" + "," + "11" + "," + section.t;
            }

            //if sectionComboBox.SelectedItem="C - section with no fillet(no lip)"
            else if ((string)sectionComboBox.SelectedItem == "C - Section with no fillet (no lip)")
            {
                section.r = 0;
                section.w = 0; //if not hat make it zero
                section.s = 0;  //if not hat make it zero

                section.node1x = section.b;
                section.node1y = -section.h / 2;
                section.node2x = section.b / 2;
                section.node2y = -section.h / 2;
                section.node3x = 0; //r
                section.node3y = -section.h / 2;
                section.node4x = 0;
                section.node4y = -section.h / 4;
                section.node5x = 0;
                section.node5y = 0;
                section.node6x = 0;
                section.node6y = section.h / 4;
                section.node7x = 0;
                section.node7y = section.h / 2 - 0; //r
                section.node8x = section.b / 2;
                section.node8y = section.h / 2;
                section.node9x = section.b;
                section.node9y = section.h / 2;

                section.sectionfinaltextnode = "1" + "," + section.node1x + "," + section.node1y + Environment.NewLine
                                               + "2" + "," + section.node2x + "," + section.node2y + Environment.NewLine
                                               + "3" + "," + section.node3x + "," + section.node3y + Environment.NewLine
                                               + "4" + "," + section.node4x + "," + section.node4y + Environment.NewLine
                                               + "5" + "," + section.node5x + "," + section.node5y + Environment.NewLine
                                               + "6" + "," + section.node6x + "," + section.node6y + Environment.NewLine
                                               + "7" + "," + section.node7x + "," + section.node7y + Environment.NewLine
                                               + "8" + "," + section.node8x + "," + section.node8y + Environment.NewLine
                                               + "9" + "," + section.node9x + "," + section.node9y;

                section.sectionfinaltextelement = "1" + "," + "1" + "," + "2" + "," + section.t + Environment.NewLine
                                                  + "2" + "," + "2" + "," + "3" + "," + section.t + Environment.NewLine
                                                  + "3" + "," + "3" + "," + "4" + "," + section.t + Environment.NewLine
                                                  + "4" + "," + "4" + "," + "5" + "," + section.t + Environment.NewLine
                                                  + "5" + "," + "5" + "," + "6" + "," + section.t + Environment.NewLine
                                                  + "6" + "," + "6" + "," + "7" + "," + section.t + Environment.NewLine
                                                  + "7" + "," + "7" + "," + "8" + "," + section.t + Environment.NewLine
                                                  + "8" + "," + "8" + "," + "9" + "," + section.t;
            }



            //if sectionComboBox.SelectedItem="Z - section lipped with fillet"
            else if ((string)sectionComboBox.SelectedItem == "Z - Section lipped with fillet")
            {
                section.w = 0; //if not hat make it zero
                section.s = 0;  //if not hat make it zero

                section.node1x = section.b;
                section.node1y = -section.h / 2 + section.d;
                section.node2x = section.b;
                section.node2y = -section.h / 2 + section.r;
                section.node3x = section.b - section.r + section.J1;
                section.node3y = -section.h / 2 + (2 * section.r / 3);
                section.node4x = section.b - section.r + section.J3;
                section.node4y = -section.h / 2 + (section.r / 2);
                section.node5x = section.b - section.r + section.J2;
                section.node5y = -section.h / 2 + (section.r / 3);
                section.node6x = section.b - section.r;
                section.node6y = -section.h / 2;
                section.node7x = section.b / 2;
                section.node7y = -section.h / 2;
                section.node8x = section.r;
                section.node8y = -section.h / 2;
                section.node9x = section.r - section.J2;
                section.node9y = -section.h / 2 + section.r - 2 * section.r / 3;
                section.node10x = section.r - section.J3;
                section.node10y = -section.h / 2 + section.r - section.r / 2;
                section.node11x = section.r - section.J1;
                section.node11y = -section.h / 2 + section.r - section.r / 3;
                section.node12x = 0;
                section.node12y = -section.h / 2 + section.r;
                section.node13x = 0;
                section.node13y = -section.h / 4;
                section.node14x = 0;
                section.node14y = 0;
                section.node15x = 0;
                section.node15y = section.h / 4;
                section.node16x = 0;
                section.node16y = section.h / 2 - section.r;
                section.node17x = -(section.r - section.J1);
                section.node17y = section.h / 2 - section.r + section.r / 3;
                section.node18x = -(section.r - section.J3);
                section.node18y = section.h / 2 - section.r + section.r / 2;
                section.node19x = -(section.r - section.J2);
                section.node19y = section.h / 2 - section.r + 2 * section.r / 3;
                section.node20x = -section.r;
                section.node20y = section.h / 2;
                section.node21x = -section.b / 2;
                section.node21y = section.h / 2;
                section.node22x = -(section.b - section.r);
                section.node22y = section.h / 2;
                section.node23x = -(section.b - section.r + section.J2);
                section.node23y = section.h / 2 - section.r / 3;
                section.node24x = -(section.b - section.r + section.J3);
                section.node24y = section.h / 2 - section.r / 2;
                section.node25x = -(section.b - section.r + section.J1);
                section.node25y = section.h / 2 - 2 * section.r / 3;
                section.node26x = -section.b;
                section.node26y = section.h / 2 - section.r;
                section.node27x = -section.b;
                section.node27y = section.h / 2 - section.d;

                section.sectionfinaltextnode = "1" + "," + section.node1x + "," + section.node1y + Environment.NewLine
                                               + "2" + "," + section.node2x + "," + section.node2y + Environment.NewLine
                                               + "3" + "," + section.node3x + "," + section.node3y + Environment.NewLine
                                               + "4" + "," + section.node4x + "," + section.node4y + Environment.NewLine
                                               + "5" + "," + section.node5x + "," + section.node5y + Environment.NewLine
                                               + "6" + "," + section.node6x + "," + section.node6y + Environment.NewLine
                                               + "7" + "," + section.node7x + "," + section.node7y + Environment.NewLine
                                               + "8" + "," + section.node8x + "," + section.node8y + Environment.NewLine
                                               + "9" + "," + section.node9x + "," + section.node9y + Environment.NewLine
                                               + "10" + "," + section.node10x + "," + section.node10y +
                                               Environment.NewLine
                                               + "11" + "," + section.node11x + "," + section.node11y +
                                               Environment.NewLine
                                               + "12" + "," + section.node12x + "," + section.node12y +
                                               Environment.NewLine
                                               + "13" + "," + section.node13x + "," + section.node13y +
                                               Environment.NewLine
                                               + "14" + "," + section.node14x + "," + section.node14y +
                                               Environment.NewLine
                                               + "15" + "," + section.node15x + "," + section.node15y +
                                               Environment.NewLine
                                               + "16" + "," + section.node16x + "," + section.node16y +
                                               Environment.NewLine
                                               + "17" + "," + section.node17x + "," + section.node17y +
                                               Environment.NewLine
                                               + "18" + "," + section.node18x + "," + section.node18y +
                                               Environment.NewLine
                                               + "19" + "," + section.node19x + "," + section.node19y +
                                               Environment.NewLine
                                               + "20" + "," + section.node20x + "," + section.node20y +
                                               Environment.NewLine
                                               + "21" + "," + section.node21x + "," + section.node21y +
                                               Environment.NewLine
                                               + "22" + "," + section.node22x + "," + section.node22y +
                                               Environment.NewLine
                                               + "23" + "," + section.node23x + "," + section.node23y +
                                               Environment.NewLine
                                               + "24" + "," + section.node24x + "," + section.node24y +
                                               Environment.NewLine
                                               + "25" + "," + section.node25x + "," + section.node25y +
                                               Environment.NewLine
                                               + "26" + "," + section.node26x + "," + section.node26y +
                                               Environment.NewLine
                                               + "27" + "," + section.node27x + "," + section.node27y;

                section.sectionfinaltextelement = "1" + "," + "1" + "," + "2" + "," + section.t + Environment.NewLine
                                                  + "2" + "," + "2" + "," + "3" + "," + section.t + Environment.NewLine
                                                  + "3" + "," + "3" + "," + "4" + "," + section.t + Environment.NewLine
                                                  + "4" + "," + "4" + "," + "5" + "," + section.t + Environment.NewLine
                                                  + "5" + "," + "5" + "," + "6" + "," + section.t + Environment.NewLine
                                                  + "6" + "," + "6" + "," + "7" + "," + section.t + Environment.NewLine
                                                  + "7" + "," + "7" + "," + "8" + "," + section.t + Environment.NewLine
                                                  + "8" + "," + "8" + "," + "9" + "," + section.t + Environment.NewLine
                                                  + "9" + "," + "9" + "," + "10" + "," + section.t + Environment.NewLine
                                                  + "10" + "," + "10" + "," + "11" + "," + section.t + Environment.NewLine
                                                  + "11" + "," + "11" + "," + "12" + "," + section.t + Environment.NewLine
                                                  + "12" + "," + "12" + "," + "13" + "," + section.t + Environment.NewLine
                                                  + "13" + "," + "13" + "," + "14" + "," + section.t + Environment.NewLine
                                                  + "14" + "," + "14" + "," + "15" + "," + section.t + Environment.NewLine
                                                  + "15" + "," + "15" + "," + "16" + "," + section.t + Environment.NewLine
                                                  + "16" + "," + "16" + "," + "17" + "," + section.t + Environment.NewLine
                                                  + "17" + "," + "17" + "," + "18" + "," + section.t + Environment.NewLine
                                                  + "18" + "," + "18" + "," + "19" + "," + section.t + Environment.NewLine
                                                  + "19" + "," + "19" + "," + "20" + "," + section.t + Environment.NewLine
                                                  + "20" + "," + "20" + "," + "21" + "," + section.t + Environment.NewLine
                                                  + "21" + "," + "21" + "," + "22" + "," + section.t + Environment.NewLine
                                                  + "22" + "," + "22" + "," + "23" + "," + section.t + Environment.NewLine
                                                  + "23" + "," + "23" + "," + "24" + "," + section.t + Environment.NewLine
                                                  + "24" + "," + "24" + "," + "25" + "," + section.t + Environment.NewLine
                                                  + "25" + "," + "25" + "," + "26" + "," + section.t + Environment.NewLine
                                                  + "26" + "," + "26" + "," + "27" + "," + section.t;
            }

            //if sectionComboBox.SelectedItem="Z - section lipped with no fillet"
            else if ((string)sectionComboBox.SelectedItem == "Z - Section lipped with no fillet")
            {
                section.r = 0;
                section.w = 0; //if not hat make it zero
                section.s = 0;  //if not hat make it zero

                section.node1x = section.b;
                section.node1y = -section.h / 2 + section.d;
                section.node2x = section.b;
                section.node2y = -section.h / 2 + 0; //r
                section.node3x = section.b / 2;
                section.node3y = -section.h / 2;
                section.node4x = 0; //r
                section.node4y = -section.h / 2;
                section.node5x = 0;
                section.node5y = -section.h / 4;
                section.node6x = 0;
                section.node6y = 0;
                section.node7x = 0;
                section.node7y = section.h / 4;
                section.node8x = 0;
                section.node8y = section.h / 2 - 0; //r
                section.node9x = -section.b / 2;
                section.node9y = section.h / 2;
                section.node10x = -(section.b - 0); //r
                section.node10y = section.h / 2;
                section.node11x = -section.b;
                section.node11y = section.h / 2 - section.d;

                section.sectionfinaltextnode = "1" + "," + section.node1x + "," + section.node1y + Environment.NewLine
                                               + "2" + "," + section.node2x + "," + section.node2y + Environment.NewLine
                                               + "3" + "," + section.node3x + "," + section.node3y + Environment.NewLine
                                               + "4" + "," + section.node4x + "," + section.node4y + Environment.NewLine
                                               + "5" + "," + section.node5x + "," + section.node5y + Environment.NewLine
                                               + "6" + "," + section.node6x + "," + section.node6y + Environment.NewLine
                                               + "7" + "," + section.node7x + "," + section.node7y + Environment.NewLine
                                               + "8" + "," + section.node8x + "," + section.node8y + Environment.NewLine
                                               + "9" + "," + section.node9x + "," + section.node9y + Environment.NewLine
                                               + "10" + "," + section.node10x + "," + section.node10y +
                                               Environment.NewLine
                                               + "11" + "," + section.node11x + "," + section.node11y;

                section.sectionfinaltextelement = "1" + "," + "1" + "," + "2" + "," + section.t + Environment.NewLine
                                                  + "2" + "," + "2" + "," + "3" + "," + section.t + Environment.NewLine
                                                  + "3" + "," + "3" + "," + "4" + "," + section.t + Environment.NewLine
                                                  + "4" + "," + "4" + "," + "5" + "," + section.t + Environment.NewLine
                                                  + "5" + "," + "5" + "," + "6" + "," + section.t + Environment.NewLine
                                                  + "6" + "," + "6" + "," + "7" + "," + section.t + Environment.NewLine
                                                  + "7" + "," + "7" + "," + "8" + "," + section.t + Environment.NewLine
                                                  + "8" + "," + "8" + "," + "9" + "," + section.t + Environment.NewLine
                                                  + "9" + "," + "9" + "," + "10" + "," + section.t + Environment.NewLine
                                                  + "10" + "," + "10" + "," + "11" + "," + section.t;
            }


            //if sectionComboBox.SelectedItem="Z - section with fillet(no lip)"
            else if ((string)sectionComboBox.SelectedItem == "Z - Section with fillet (no lip)")
            {
                section.w = 0; //if not hat make it zero
                section.s = 0;  //if not hat make it zero

                section.node1x = section.b;
                section.node1y = -section.h / 2;
                section.node2x = section.b / 2;
                section.node2y = -section.h / 2;
                section.node3x = section.r;
                section.node3y = -section.h / 2;
                section.node4x = section.r - section.J2;
                section.node4y = -section.h / 2 + section.r - 2 * section.r / 3;
                section.node5x = section.r - section.J3;
                section.node5y = -section.h / 2 + section.r - section.r / 2;
                section.node6x = section.r - section.J1;
                section.node6y = -section.h / 2 + section.r - section.r / 3;
                section.node7x = 0;
                section.node7y = -section.h / 2 + section.r;
                section.node8x = 0;
                section.node8y = -section.h / 4;
                section.node9x = 0;
                section.node9y = 0;
                section.node10x = 0;
                section.node10y = section.h / 4;
                section.node11x = 0;
                section.node11y = section.h / 2 - section.r;
                section.node12x = -(section.r - section.J1);
                section.node12y = section.h / 2 - section.r + section.r / 3;
                section.node13x = -(section.r - section.J3);
                section.node13y = section.h / 2 - section.r + section.r / 2;
                section.node14x = -(section.r - section.J2);
                section.node14y = section.h / 2 - section.r + 2 * section.r / 3;
                section.node15x = -section.r;
                section.node15y = section.h / 2;
                section.node16x = -section.b / 2;
                section.node16y = section.h / 2;
                section.node17x = -section.b;
                section.node17y = section.h / 2;

                section.sectionfinaltextnode = "1" + "," + section.node1x + "," + section.node1y + Environment.NewLine
                                               + "2" + "," + section.node2x + "," + section.node2y + Environment.NewLine
                                               + "3" + "," + section.node3x + "," + section.node3y + Environment.NewLine
                                               + "4" + "," + section.node4x + "," + section.node4y + Environment.NewLine
                                               + "5" + "," + section.node5x + "," + section.node5y + Environment.NewLine
                                               + "6" + "," + section.node6x + "," + section.node6y + Environment.NewLine
                                               + "7" + "," + section.node7x + "," + section.node7y + Environment.NewLine
                                               + "8" + "," + section.node8x + "," + section.node8y + Environment.NewLine
                                               + "9" + "," + section.node9x + "," + section.node9y + Environment.NewLine
                                               + "10" + "," + section.node10x + "," + section.node10y +
                                               Environment.NewLine
                                               + "11" + "," + section.node11x + "," + section.node11y +
                                               Environment.NewLine
                                               + "12" + "," + section.node12x + "," + section.node12y +
                                               Environment.NewLine
                                               + "13" + "," + section.node13x + "," + section.node13y +
                                               Environment.NewLine
                                               + "14" + "," + section.node14x + "," + section.node14y +
                                               Environment.NewLine
                                               + "15" + "," + section.node15x + "," + section.node15y +
                                               Environment.NewLine
                                               + "16" + "," + section.node16x + "," + section.node16y +
                                               Environment.NewLine
                                               + "17" + "," + section.node17x + "," + section.node17y;

                section.sectionfinaltextelement = "1" + "," + "1" + "," + "2" + "," + section.t + Environment.NewLine
                                                  + "2" + "," + "2" + "," + "3" + "," + section.t + Environment.NewLine
                                                  + "3" + "," + "3" + "," + "4" + "," + section.t + Environment.NewLine
                                                  + "4" + "," + "4" + "," + "5" + "," + section.t + Environment.NewLine
                                                  + "5" + "," + "5" + "," + "6" + "," + section.t + Environment.NewLine
                                                  + "6" + "," + "6" + "," + "7" + "," + section.t + Environment.NewLine
                                                  + "7" + "," + "7" + "," + "8" + "," + section.t + Environment.NewLine
                                                  + "8" + "," + "8" + "," + "9" + "," + section.t + Environment.NewLine
                                                  + "9" + "," + "9" + "," + "10" + "," + section.t + Environment.NewLine
                                                  + "10" + "," + "10" + "," + "11" + "," + section.t + Environment.NewLine
                                                  + "11" + "," + "11" + "," + "12" + "," + section.t + Environment.NewLine
                                                  + "12" + "," + "12" + "," + "13" + "," + section.t + Environment.NewLine
                                                  + "13" + "," + "13" + "," + "14" + "," + section.t + Environment.NewLine
                                                  + "14" + "," + "14" + "," + "15" + "," + section.t + Environment.NewLine
                                                  + "15" + "," + "15" + "," + "16" + "," + section.t + Environment.NewLine
                                                  + "16" + "," + "16" + "," + "17" + "," + section.t;
            }

            //if sectionComboBox.SelectedItem="Z - section with no fillet(no lip)
            else if ((string)sectionComboBox.SelectedItem == "Z - Section with no fillet (no lip)")
            {
                section.r = 0;
                section.w = 0; //if not hat make it zero
                section.s = 0;  //if not hat make it zero

                section.node1x = section.b;
                section.node1y = -section.h / 2;
                section.node2x = section.b / 2;
                section.node2y = -section.h / 2;
                section.node3x = 0; //r
                section.node3y = -section.h / 2;
                section.node4x = 0;
                section.node4y = -section.h / 4;
                section.node5x = 0;
                section.node5y = 0;
                section.node6x = 0;
                section.node6y = section.h / 4;
                section.node7x = 0;
                section.node7y = section.h / 2 - 0; //r
                section.node8x = -section.b / 2;
                section.node8y = section.h / 2;
                section.node9x = -section.b;
                section.node9y = section.h / 2;


                section.sectionfinaltextnode = "1" + "," + section.node1x + "," + section.node1y + Environment.NewLine
                                               + "2" + "," + section.node2x + "," + section.node2y + Environment.NewLine
                                               + "3" + "," + section.node3x + "," + section.node3y + Environment.NewLine
                                               + "4" + "," + section.node4x + "," + section.node4y + Environment.NewLine
                                               + "5" + "," + section.node5x + "," + section.node5y + Environment.NewLine
                                               + "6" + "," + section.node6x + "," + section.node6y + Environment.NewLine
                                               + "7" + "," + section.node7x + "," + section.node7y + Environment.NewLine
                                               + "8" + "," + section.node8x + "," + section.node8y + Environment.NewLine
                                               + "9" + "," + section.node9x + "," + section.node9y;

                section.sectionfinaltextelement = "1" + "," + "1" + "," + "2" + "," + "," + section.t + Environment.NewLine
                                                  + "2" + "," + "2" + "," + "3" + "," + section.t + Environment.NewLine
                                                  + "3" + "," + "3" + "," + "4" + "," + section.t + Environment.NewLine
                                                  + "4" + "," + "4" + "," + "5" + "," + section.t + Environment.NewLine
                                                  + "5" + "," + "5" + "," + "6" + "," + section.t + Environment.NewLine
                                                  + "6" + "," + "6" + "," + "7" + "," + section.t + Environment.NewLine
                                                  + "7" + "," + "7" + "," + "8" + "," + section.t + Environment.NewLine
                                                  + "8" + "," + "8" + "," + "9" + "," + section.t;
            }


        }


        public List<Node> GetNodeValues(string text = "")
        {
            // STEP ONE: READ TEXT
            // VERIFY THAT INPUT is correct
            // if (string.IsNullOrEmpty(text)) //Not Testing
            //{
            //Get Input Values
            text = txt_nodeData.Text;
            //}

            string[] lines = text.Split('\n');

            List<Node> nodes = new List<Node>();

            foreach (var line in lines)
            {
                string[] values = line.Trim().Split(',');
                var id = values[0];
                var x = values[1];
                var y = values[2];

                // Create a new Node
                Node node1 = new Node();
                node1.ID = int.Parse(id);
                node1.xcood = double.Parse(x);
                node1.ycood = double.Parse(y);


                // Add to nodes array
                nodes.Add(node1);
            }



            return nodes;
        }

        public List<Element> GetElementValues(string text = "")
        {


            // STEP ONE: READ TEXT
            // VERIFY THAT INPUT is correct

            if (string.IsNullOrEmpty(text)) //Not Testing
            {
                //Get Input Values
                text = txt_elementsData.Text;
            }

            string[] lines = text.Split('\n');

            List<Element> elements = new List<Element>();

            foreach (var line in lines)
            {
                string[] values = line.Trim().Split(',');
                var id = values[0];
                var Node1 = Nodes.First(n => n.ID == int.Parse(values[1]));
                var Node2 = Nodes.First(n => n.ID == int.Parse(values[2]));
                var t = values[3];

                // Create a new Element
                Element element1 = new Element();
                element1.ID = int.Parse(id);
                element1.node1 = Node1;
                element1.node2 = Node2;
                element1.t = double.Parse(t);


                // Add to elements array
                elements.Add(element1);
                paintelements.Add(element1);
            }

            return elements;
        }

        public List<Member> GetMemberValues(string[] membersData = null)
        {
            string[] linesOfA;
            string[] linesOfM;
            double n;
            double l;

            if (!IsParametric) //Not parametric
            {

                linesOfA = txt_listOfA.Text.Split('\n');
                linesOfM = txt_listOfM.Text.Split('\n');
                n = double.Parse(nTextBox.Text);
                l = double.Parse(LtextBox.Text);
            }
            else
            {
                //Testing

                linesOfA = ParametricAtextbox.Text.Split('\n');
                linesOfM = ParametricMtextbox.Text.Split('\n');
                n = double.Parse(ParametricNtextbox.Text);
                l = double.Parse(ParametricLtextbox.Text);
            }

            //if (membersData == null)
            //{
            //    //Not Testing
            //    linesOfA = txt_listOfA.Text.Split('\n');
            //    linesOfM = txt_listOfM.Text.Split('\n');
            //    n = double.Parse(nTextBox.Text);
            //    l = double.Parse(LtextBox.Text);
            //}
            //else
            //{
            //    //Testing

            //    linesOfA = parATextBox.Text.Split('\n');
            //    linesOfM = parMTextBox.Text.Split('\n');
            //    n = double.Parse(parNtextbox.Text);
            //    l = double.Parse(parLtextbox.Text);
            //linesOfA = membersData[0].Split('\n');
            //linesOfM = membersData[1].Split('\n');
            //n = double.Parse(membersData[2]);
            //l = double.Parse(membersData[3]);



            List<Member> members = new List<Member>();


            foreach (var m in linesOfM)
            {
                foreach (var a in linesOfA)
                {
                    members.Add(new Member(int.Parse(m), n, int.Parse(a), l));
                }
            }

            return members;
        }
        public void GetLoadCasesValues(double[] loadsData = null)
        {
            if (loadsData == null)
            {
                //Not Testing
                loadcases.P = double.Parse(pTextBox.Text);
                loadcases.Mx = double.Parse(MxTextBox.Text);
                loadcases.My = double.Parse(MytextBox.Text);
            }
            else
            {
                //Testing

                loadcases.P = (loadsData[0]);
                loadcases.Mx = (loadsData[1]);
                loadcases.My = (loadsData[2]);

            }

        }

        public void SolveMaterial(List<Material> materials)
        {
            foreach (var material in materials)
            {
                material.Gxy = Math.Round(material.Ex / (2 * (1 + material.Vx)), 2);
                material.e1 = material.Ex / (1 - material.Vx * material.Vy);
                material.e2 = material.Ey / (1 - material.Vx * material.Vy);
            }
        }

        public void SolveElement(List<Element> elements, Material material)
        {
            foreach (var element in elements)
            {
                element.deltax = element.node2.xcood - element.node1.xcood; // take care of 2 different nodes
                element.deltay = element.node2.ycood - element.node1.ycood;  // take care of 2 different nodes
                element.b = Math.Sqrt((element.deltax * element.deltax) + (element.deltay * element.deltay));
                if (element.deltax == 0) //vertical element
                {
                    element.alpha = element.node2.ycood > element.node1.ycood ? 90 : 270;
                }
                else
                {
                    element.alpha = 180 * (Math.Atan2(element.deltay, element.deltax)) / Math.PI;
                }

                element.SinAlpha = Math.Sin(element.alpha * Math.PI / 180);
                element.CosAlpha = Math.Cos(element.alpha * Math.PI / 180);
                element.thetax = (90 - element.alpha);
                element.thetay = (90 + element.thetax);
                element.Xcg = (element.node1.xcood + element.node2.xcood) / 2;   // take care of 2 different nodes
                element.Ycg = (element.node1.ycood + element.node2.ycood) / 2;   // take care of 2 different nodes


                element.A = element.b * element.t;
                element.Ax = element.A * element.Xcg;
                element.Ay = element.A * element.Ycg;
                element.Axx = element.A * element.Xcg * element.Xcg;
                element.Ayy = element.A * element.Ycg * element.Ycg;
                element.Axy = element.A * element.Xcg * element.Ycg;
                element.Ix = (element.b * element.t / 12) *
                             ((element.t * element.t * Math.Pow(Math.Sin(element.thetax * Math.PI / 180), 2))
                             + (element.b * element.b * Math.Pow(Math.Cos(element.thetax * Math.PI / 180), 2)));

                element.Iy = (element.b * element.t / 12) *
                             ((element.t * element.t * Math.Pow(Math.Sin(element.thetay * Math.PI / 180), 2))
                             + (element.b * element.b * Math.Pow(Math.Cos(element.thetay * Math.PI / 180), 2)));

                element.Ixy = (element.b * element.t / 12) *
                              ((element.t * element.t * Math.Sin(element.thetax * Math.PI / 180) * Math.Cos(element.thetax * Math.PI / 180))
                              + (element.b * element.b * Math.Cos(element.thetax * Math.PI / 180) * Math.Sin(element.thetax * Math.PI / 180)));

                element.J = element.b * Math.Pow(element.t, 3) / 3; //opensections only
                element.dx = material.Ex * Math.Pow(element.t, 3) / (12 * (1 - material.Vx * material.Vy));
                element.dy = material.Ey * Math.Pow(element.t, 3) / (12 * (1 - material.Vx * material.Vy));
                element.dxy = material.Gxy * Math.Pow(element.t, 3) / 12;
                element.d1 = material.Ex * material.Vy * Math.Pow(element.t, 3) / (12 * (1 - material.Vx * material.Vy));
                element.f1 = element.node1.Stress;   // take stress of node no. 1
                element.f2 = element.node2.Stress;   // take stress of node no. 2
                element.t1 = element.f1 * element.t;
                element.t2 = element.f2 * element.t;
            }

            //Calculate Ycg & Xcg
            UpdateSectionYcgXcg(elements);
            //Update elements values
            foreach (var element in elements)
            {
                element.deltaxdeltay12 = element.b * element.t * element.deltax * element.deltay / 12; //new
                element.deltaxdeltax12 = element.b * element.t * element.deltax * element.deltax / 12; //new
                element.deltaydeltay12 = element.b * element.t * element.deltay * element.deltay / 12; //new
                element.ymeanminusycg = element.b * element.t * (element.Ycg - section.Ycg); //can be omitted //new needs section calculations first
                element.xmeanminusxcg = element.b * element.t * (element.Xcg - section.Xcg); //can be omitted //new needs section calculations first
                element.xmeanminusxcgymeanminusycg = element.b * element.t * (element.Ycg - section.Ycg) * (element.Xcg - section.Xcg); //new needs section calculations first
                element.ymeanminusycg2 = element.b * element.t * (element.Ycg - section.Ycg) * (element.Ycg - section.Ycg); //new needs section calculations first
                element.xmeanminusxcg2 = element.b * element.t * (element.Xcg - section.Xcg) * (element.Xcg - section.Xcg); //new needs section calculations first
            }
        }

        private void UpdateSectionYcgXcg(List<Element> elements)
        {
            section.A = elements.Sum(el => el.A); //summation of element.A
            section.Ax = elements.Sum(el => el.Ax);  //summation of element.Ax
            section.Ay = elements.Sum(el => el.Ay);  //summation of element.Ay
            section.Xcg = section.Ax / section.A;
            section.Ycg = section.Ay / section.A;
        }

        public void SolveSection(List<Element> elements)
        {
            section.A = elements.Sum(el => el.A); //summation of element.A
            section.Ax = elements.Sum(el => el.Ax);  //summation of element.Ax
            section.Ay = elements.Sum(el => el.Ay);  //summation of element.Ay
            section.Axx = elements.Sum(el => el.Axx);  //summation of element.Axx
            section.Ayy = elements.Sum(el => el.Ayy);  //summation of element.Ayy
            section.Axy = elements.Sum(el => el.Axy);  //summation of element.Axy
            section.Xcg = section.Ax / section.A;
            section.Ycg = section.Ay / section.A;
            section.sigmaIx = elements.Sum(el => el.Ix); //summation of element.Ix
            section.sigmaIy = elements.Sum(el => el.Iy);  //summation of element.Iy
            section.sigmaIxy = elements.Sum(el => el.Ixy);  //summation of element.Ixy

            section.sigmadeltaxdeltay12 = elements.Sum(el => el.deltaxdeltay12); //new
            section.sigmadeltaxdeltax12 = elements.Sum(el => el.deltaxdeltax12); //new
            section.sigmadeltaydeltay12 = elements.Sum(el => el.deltaydeltay12); //new
            section.sigmaymeanminusycg = elements.Sum(el => el.ymeanminusycg); //new needs section calculations first
            section.sigmaxmeanminusxcg = elements.Sum(el => el.xmeanminusxcg); //new needs section calculations first
            section.sigmaxmeanminusxcgymeanminusycg = elements.Sum(el => el.xmeanminusxcgymeanminusycg); //new needs section calculations first
            section.sigmaymeanminusycg2 = elements.Sum(el => el.ymeanminusycg2); //new needs section calculations first
            section.sigmaxmeanminusxcg2 = elements.Sum(el => el.xmeanminusxcg2); //new needs section calculations first

            //section.Ix = section.sigmaIx + section.Ayy - (section.A * section.Ycg * section.Ycg) ;   //first inertia calculations
            //section.Iy = section.sigmaIy + section.Axx - (section.A * section.Xcg * section.Xcg);
            //section.Ixy = section.sigmaIxy + section.Axy - (section.A * section.Xcg * section.Ycg) ;

            section.Ix = section.sigmadeltaydeltay12 + section.sigmaymeanminusycg2;         //second inertia calculations
            section.Iy = section.sigmadeltaxdeltax12 + section.sigmaxmeanminusxcg2;
            section.Ixy = section.sigmadeltaxdeltay12 + section.sigmaxmeanminusxcgymeanminusycg;

            section.I1 = ((section.Ix + section.Iy) / 2) +
                Math.Sqrt(((section.Ix - section.Iy) / 2) * ((section.Ix - section.Iy) / 2) + section.Ixy * section.Ixy);
            section.I2 = ((section.Ix + section.Iy) / 2) -
                Math.Sqrt(((section.Ix - section.Iy) / 2) * ((section.Ix - section.Iy) / 2) + section.Ixy * section.Ixy);
            section.PolarTheta = (180 / Math.PI) * 0.5 * Math.Atan(-2 * section.Ixy / (section.Ix - section.Iy));
            section.J = elements.Sum(el => el.J); //summation of element.J
        }

        public void SolveNodeStress(List<Element> elements, List<Material> materials)
        {
            foreach (var node in Nodes)
            {
                node.Stress = /*Math.Round(*/(loadcases.P / section.A) -
                              ((loadcases.My * section.Ix + loadcases.Mx * section.Ixy) * (node.xcood - section.Xcg) -
                                (loadcases.Mx * section.Iy + loadcases.My * section.Ixy) * (node.ycood - section.Ycg))
                              / (section.Iy * section.Ix - section.Ixy * section.Ixy)/* ,3)*/;

                node.StressAtMxYield = Math.Abs(((section.Ixy * (node.xcood - section.Xcg)) -
                                                 (section.Iy * (node.ycood - section.Ycg)))
                                                / (section.Iy * section.Ix - section.Ixy * section.Ixy));

                node.StressAtMyYield = Math.Abs(((section.Ix * (node.xcood - section.Xcg)) -
                                                 (section.Ixy * (node.ycood - section.Ycg)))
                                                / (section.Iy * section.Ix - section.Ixy * section.Ixy));

            }
            foreach (var element in elements)
            {
                element.f1 = element.node1.Stress;   // take stress of node no. 1
                element.f2 = element.node2.Stress;   // take stress of node no. 2
                element.t1 = /*Math.Round(*/element.f1 * element.t /*,3)*/;
                element.t2 = /*Math.Round(*/element.f2 * element.t/*,3)*/;
            }

        }
        public void SolveNodeYield(List<Element> elements, Material material) //yield loads but not their combination (P, Mx, My)
        {

            foreach (var node in Nodes)
            {
                loadcases.Py = material.fy * section.A;

                if (loadcases.Mxy < material.fy / node.StressAtMxYield)
                    loadcases.Mxy = material.fy / node.StressAtMxYield;    // get max absolute stress for all nodes at Mx=1, My=0
                if (loadcases.Myy < material.fy / node.StressAtMyYield)
                    loadcases.Myy = material.fy / node.StressAtMyYield;   // get max absolute stress for all nodes at My=1, Mx=0
            }

        }

        private void SolveMembers() //Solve for all a & m of member (considered as different members)
        {

            foreach (var member in Members)
            {
                SolveMember(member);

            }

        }
        public void SolveMember(Member member) //For the member with certain a & m
        {

            member.c1 = member.m * Math.PI / member.a;
            member.c2 = member.n * Math.PI / member.a;
            member.meum = member.m * Math.PI;
            member.meun = member.n * Math.PI;
            if (BoundryCondition == "Simple - Simple")
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
            else if (BoundryCondition == "Fixed - Fixed")
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
                    member.i2 = -Math.PI * Math.PI * (member.m * member.m + 1) / (4) / (member.a);
                    member.i3 = -Math.PI * Math.PI * (member.n * member.n + 1) / (4) / (member.a);
                    member.i4 = Math.Pow(Math.PI, 4) * (Math.Pow(member.m * member.m + 1, 2) + (4 * member.m * member.m)) / (4) / (member.a * member.a * member.a);
                    member.i5 = Math.PI * Math.PI * (1 + member.m * member.m) / (4) / (member.a);
                }
                else if (member.m - member.n == 2)
                {
                    member.i1 = -member.a / 8;
                    member.i2 = (Math.PI * Math.PI * (member.m * member.m + 1) / (8 * member.a)) - (member.m * Math.PI * Math.PI / (4) / (member.a));
                    member.i3 = (Math.PI * Math.PI * (member.n * member.n + 1) / (8 * member.a)) - (member.n * Math.PI * Math.PI / (4) / (member.a));
                    member.i4 = -Math.Pow(member.m - 1, 2) * Math.Pow(member.n + 1, 2) * Math.Pow(Math.PI, 4) / (8) / (member.a * member.a * member.a);
                    member.i5 = Math.PI * Math.PI * -(1 + member.m * member.n) / (8 * member.a);
                }
                else if (member.m - member.n == -2)
                {
                    member.i1 = -member.a / 8;
                    member.i2 = (Math.PI * Math.PI * (member.m * member.m + 1) / (8 * member.a)) + (member.m * Math.PI * Math.PI / (4) / (member.a));
                    member.i3 = (Math.PI * Math.PI * (member.n * member.n + 1) / (8 * member.a)) + (member.n * Math.PI * Math.PI / (4) / (member.a));
                    member.i4 = -Math.Pow(member.m + 1, 2) * Math.Pow(member.n - 1, 2) * Math.Pow(Math.PI, 4) / (8) / (member.a * member.a * member.a);
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
            else if (BoundryCondition == "Simple - Fixed")
            {
                if (member.m == member.n)
                {
                    member.i1 = ((1 + Math.Pow((member.m + 1), 2)) / Math.Pow(member.m, 2)) * member.a / 2;
                    member.i2 = -Math.Pow((member.m + 1), 2) * Math.Pow(Math.PI, 2) / member.a;
                    member.i3 = -Math.Pow((member.m + 1), 2) * Math.Pow(Math.PI, 2) / member.a;
                    member.i4 = Math.Pow((member.m + 1), 2) * Math.Pow(Math.PI, 4) * (Math.Pow((member.m + 1), 2) + Math.Pow(member.m, 2)) / (2 * Math.Pow(member.a, 3));
                    member.i5 = Math.Pow((member.m + 1), 2) * Math.Pow(Math.PI, 2) / member.a;
                }
                else if (member.m - member.n == 1)
                {
                    member.i1 = (member.m + 1) * member.a / (2 * member.m);
                    member.i2 = -(member.m + 1) * member.m * Math.Pow(Math.PI, 2) / (2 * member.a);
                    member.i3 = -Math.Pow((member.n + 1), 2) * Math.Pow(Math.PI, 2) * (member.m + 1) / (2 * member.a * member.m);
                    member.i4 = (member.m + 1) * member.m * Math.Pow((member.n + 1), 2) * Math.Pow(Math.PI, 4) / (2 * Math.Pow(member.a, 3));
                    member.i5 = (member.m + 1) * (member.n + 1) * Math.Pow(Math.PI, 2) / (2 * member.a);
                }
                else if (member.m - member.n == -1)
                {
                    member.i1 = (member.n + 1) * member.a / (2 * member.n);
                    member.i2 = -Math.Pow((member.m + 1), 2) * (member.n + 1) * Math.Pow(Math.PI, 2) / (2 * member.a * member.n);
                    member.i3 = -(member.n + 1) * Math.Pow(Math.PI, 2) * member.n / (2 * member.a);
                    member.i4 = Math.Pow((member.m + 1), 2) * member.n * (member.n + 1) * Math.Pow(Math.PI, 4) / (2 * Math.Pow(member.a, 3));
                    member.i5 = (member.m + 1) * (member.n + 1) * Math.Pow(Math.PI, 2) / (2 * member.a);
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
            else if (BoundryCondition == "Fixed - Free")
            {
                if (member.m == member.n)
                {
                    member.i1 = (3 * member.a / 2) - ((2 * member.a * Math.Pow(-1, member.m - 1)) / ((member.m - 0.5) * Math.PI));
                    member.i2 = Math.Pow((member.m - 0.5), 2) * Math.Pow(Math.PI, 2) * (Math.Pow(-1, member.m - 1) / ((member.m - 0.5) * Math.PI) - 0.5) / member.a;
                    member.i3 = Math.Pow((member.n - 0.5), 2) * Math.Pow(Math.PI, 2) * (Math.Pow(-1, member.n - 1) / ((member.n - 0.5) * Math.PI) - 0.5) / member.a;
                    member.i4 = Math.Pow((member.m - 0.5), 4) * Math.Pow(Math.PI, 4) / (2 * Math.Pow(member.a, 3));
                    member.i5 = Math.Pow((member.m - 0.5), 2) * Math.Pow(Math.PI, 2) / (2 * member.a);
                }
                else
                {
                    member.i1 = member.a -
                        (member.a * Math.Pow(-1, member.m - 1) / ((member.m - 0.5) * Math.PI)) -
                        (member.a * Math.Pow(-1, member.n - 1) / ((member.n - 0.5) * Math.PI));
                    member.i2 = Math.Pow((member.m - 0.5), 2) * Math.Pow(Math.PI, 2) * (Math.Pow(-1, member.m - 1) / ((member.m - 0.5) * Math.PI)) / member.a;
                    member.i3 = Math.Pow((member.n - 0.5), 2) * Math.Pow(Math.PI, 2) * (Math.Pow(-1, member.n - 1) / ((member.n - 0.5) * Math.PI)) / member.a;
                    member.i4 = 0;
                    member.i5 = 0;
                }
            }
            else if (BoundryCondition == "Fixed - Guided")
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
                    member.i2 = -(Math.Pow((member.m - 0.5), 2) + 0.25) * Math.Pow(Math.PI, 2) / (4 * member.a);
                    member.i3 = -(Math.Pow((member.m - 0.5), 2) + 0.25) * Math.Pow(Math.PI, 2) / (4 * member.a);
                    member.i4 = (Math.Pow((Math.Pow((member.m - 0.5), 2) + 0.25), 2) * Math.Pow(Math.PI, 4) / (4 * Math.Pow(member.a, 3)))
                        + (Math.Pow((member.m - 0.5), 2) * Math.Pow(Math.PI, 4) / (4 * Math.Pow(member.a, 3)));
                    member.i5 = (Math.Pow((member.m - 0.5), 2) * Math.Pow(Math.PI, 2) / (4 * member.a)) + (Math.Pow(Math.PI, 2) / (16 * member.a));
                }
                else if (member.m - member.n == 1)
                {
                    member.i1 = -member.a / 8;
                    member.i2 = ((Math.Pow((member.m - 0.5), 2) + 0.25) * Math.Pow(Math.PI, 2) / (8 * member.a)) -
                        ((member.m - 0.5) * Math.Pow(Math.PI, 2) / (8 * member.a));
                    member.i3 = (Math.Pow((member.n - 0.5), 2) + 0.25) * Math.Pow(Math.PI, 2) / (8 * member.a) +
                        ((member.n - 0.5) * Math.Pow(Math.PI, 2) / (8 * member.a));
                    member.i4 = -Math.Pow(member.n, 4) * Math.Pow(Math.PI, 4) / (8 * Math.Pow(member.a, 3));
                    member.i5 = -Math.Pow(member.n, 2) * Math.Pow(Math.PI, 2) / (8 * member.a);
                }
                else if (member.m - member.n == -1)
                {
                    member.i1 = -member.a / 8;
                    member.i2 = ((Math.Pow((member.m - 0.5), 2) + 0.25) * Math.Pow(Math.PI, 2) / (8 * member.a)) +
                        ((member.m - 0.5) * Math.Pow(Math.PI, 2) / (8 * member.a));
                    member.i3 = (Math.Pow((member.n - 0.5), 2) + 0.25) * Math.Pow(Math.PI, 2) / (8 * member.a) -
                        ((member.n - 0.5) * Math.Pow(Math.PI, 2) / (8 * member.a));
                    member.i4 = -Math.Pow(member.m, 4) * Math.Pow(Math.PI, 4) / (8 * Math.Pow(member.a, 3));
                    member.i5 = -Math.Pow(member.m, 2) * Math.Pow(Math.PI, 2) / (8 * member.a);
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

        public void SolveMatricesForAllElements(List<Element> elements, Material material)
        {
            foreach (var element in elements)
            {
                foreach (var member in Members)
                {
                    SolveMatricesForOneElement(element, material, member);
                }
            }

        }

        public void SolveMatricesForOneElement(Element element, Material material, Member member) //Using a certain state of the member (certain a & m)
        {
            //Ke

            double[,] ke = new double[8, 8];
            element.ke.Add(member, ke);
            ke[0, 0] = (material.Gxy * element.b * member.i5 * element.t / 3) + (material.e1 * member.i1 * element.t / element.b);
            ke[0, 1] = (-material.e2 * material.Vx * member.i3 * element.t / (2 * member.c2)) - (material.Gxy * member.i5 * element.t / (2 * member.c2));
            ke[0, 2] = (material.Gxy * element.b * member.i5 * element.t / 6) - (material.e1 * member.i1 * element.t / element.b);
            ke[0, 3] = (-material.e2 * material.Vx * member.i3 * element.t / (2 * member.c2))
                + (material.Gxy * member.i5 * element.t / (2 * member.c2));
            ke[1, 0] = (-material.e2 * material.Vx * member.i2 * element.t / (2 * member.c1))
                - (material.Gxy * member.i5 * element.t / (2 * member.c1));
            ke[1, 1] = (material.e2 * element.b * member.i4 * element.t / (3 * member.c1 * member.c2))
                + (material.Gxy * member.i5 * element.t / (element.b * member.c1 * member.c2));
            ke[1, 2] = (material.e2 * material.Vx * member.i2 * element.t / (2 * member.c1)) - (material.Gxy * member.i5 * element.t / (2 * member.c1));
            ke[1, 3] = (material.e2 * element.b * member.i4 * element.t / (6 * member.c1 * member.c2))
                - (material.Gxy * member.i5 * element.t / (element.b * member.c1 * member.c2));
            ke[2, 0] = (material.Gxy * element.b * member.i5 * element.t / 6) - (material.e1 * member.i1 * element.t / element.b);
            ke[2, 1] = (material.e2 * material.Vx * member.i3 * element.t / (2 * member.c2)) - (material.Gxy * member.i5 * element.t / (2 * member.c2));
            ke[2, 2] = (material.Gxy * element.b * member.i5 * element.t / 3)
                + (material.e1 * member.i1 * element.t / element.b);
            ke[2, 3] = (material.e2 * material.Vx * member.i3 * element.t / (2 * member.c2))
                + (material.Gxy * member.i5 * element.t / (2 * member.c2));
            ke[3, 0] = (-material.e2 * material.Vx * member.i2 * element.t / (2 * member.c1))
                + (material.Gxy * member.i5 * element.t / (2 * member.c1));
            ke[3, 1] = (material.e2 * element.b * member.i4 * element.t / (6 * member.c1 * member.c2))
                - (material.Gxy * member.i5 * element.t / (element.b * member.c1 * member.c2));
            ke[3, 2] = (material.e2 * material.Vx * member.i2 * element.t / (2 * member.c1)) + (material.Gxy * member.i5 * element.t / (2 * member.c1));
            ke[3, 3] = (material.e2 * element.b * member.i4 * element.t / (3 * member.c1 * member.c2))
                + (material.Gxy * member.i5 * element.t / (element.b * member.c1 * member.c2));



            ke[0, 4] = 0;
            ke[0, 5] = 0;
            ke[0, 6] = 0;
            ke[0, 7] = 0;
            ke[1, 4] = 0;
            ke[1, 5] = 0;
            ke[1, 6] = 0;
            ke[1, 7] = 0;
            ke[2, 4] = 0;
            ke[2, 5] = 0;
            ke[2, 6] = 0;
            ke[2, 7] = 0;
            ke[3, 4] = 0;
            ke[3, 5] = 0;
            ke[3, 6] = 0;
            ke[3, 7] = 0;

            ke[4, 0] = 0;
            ke[4, 1] = 0;
            ke[4, 2] = 0;
            ke[4, 3] = 0;
            ke[5, 0] = 0;
            ke[5, 1] = 0;
            ke[5, 2] = 0;
            ke[5, 3] = 0;
            ke[6, 0] = 0;
            ke[6, 1] = 0;
            ke[6, 2] = 0;
            ke[6, 3] = 0;
            ke[7, 0] = 0;
            ke[7, 1] = 0;
            ke[7, 2] = 0;
            ke[7, 3] = 0;


            ke[4, 4] = (5040 * (element.dx * member.i1 / (420 * element.b * element.b * element.b))
                - 504 * element.b * element.b * (element.d1 * member.i2 / (420 * element.b * element.b * element.b))
                - 504 * element.b * element.b * (element.d1 * member.i3 / (420 * element.b * element.b * element.b))
                + 156 * element.b * element.b * element.b * element.b * (element.dy * member.i4 / (420 * element.b * element.b * element.b))
                + 2016 * element.b * element.b * (element.dxy * member.i5 / (420 * element.b * element.b * element.b)));

            ke[4, 5] = (2520 * element.b * (element.dx * member.i1 / (420 * element.b * element.b * element.b))
                - 462 * element.b * element.b * element.b * (element.d1 * member.i2 / (420 * element.b * element.b * element.b))
                - 42 * element.b * element.b * element.b * (element.d1 * member.i3 / (420 * element.b * element.b * element.b))
                + 22 * element.b * element.b * element.b * element.b * element.b * (element.dy * member.i4 / (420 * element.b * element.b * element.b))
                + 168 * element.b * element.b * element.b * (element.dxy * member.i5 / (420 * element.b * element.b * element.b)));

            ke[4, 6] = (-5040 * (element.dx * member.i1 / (420 * element.b * element.b * element.b))
                + 504 * element.b * element.b * (element.d1 * member.i2 / (420 * element.b * element.b * element.b))
                + 504 * element.b * element.b * (element.d1 * member.i3 / (420 * element.b * element.b * element.b))
                + 54 * element.b * element.b * element.b * element.b * (element.dy * member.i4 / (420 * element.b * element.b * element.b))
                - 2016 * element.b * element.b * (element.dxy * member.i5 / (420 * element.b * element.b * element.b)));

            ke[4, 7] = (2520 * element.b * (element.dx * member.i1 / (420 * element.b * element.b * element.b))
                - 42 * element.b * element.b * element.b * (element.d1 * member.i2 / (420 * element.b * element.b * element.b))
                - 42 * element.b * element.b * element.b * (element.d1 * member.i3 / (420 * element.b * element.b * element.b))
                - 13 * element.b * element.b * element.b * element.b * element.b * (element.dy * member.i4 / (420 * element.b * element.b * element.b))
                + 168 * element.b * element.b * element.b * (element.dxy * member.i5 / (420 * element.b * element.b * element.b)));

            ke[5, 4] = (2520 * element.b * (element.dx * member.i1 / (420 * element.b * element.b * element.b))
                - 42 * element.b * element.b * element.b * (element.d1 * member.i2 / (420 * element.b * element.b * element.b))
                - 462 * element.b * element.b * element.b * (element.d1 * member.i3 / (420 * element.b * element.b * element.b))
                + 22 * element.b * element.b * element.b * element.b * element.b * (element.dy * member.i4 / (420 * element.b * element.b * element.b))
                + 168 * element.b * element.b * element.b * (element.dxy * member.i5 / (420 * element.b * element.b * element.b)));

            ke[5, 5] = (1680 * element.b * element.b * (element.dx * member.i1 / (420 * element.b * element.b * element.b))
                - 56 * element.b * element.b * element.b * element.b * (element.d1 * member.i2 / (420 * element.b * element.b * element.b))
                - 56 * element.b * element.b * element.b * element.b * (element.d1 * member.i3 / (420 * element.b * element.b * element.b))
                + 4 * element.b * element.b * element.b * element.b * element.b * element.b * (element.dy * member.i4 / (420 * element.b * element.b * element.b))
                + 224 * element.b * element.b * element.b * element.b * (element.dxy * member.i5 / (420 * element.b * element.b * element.b)));

            ke[5, 6] = (-2520 * element.b * (element.dx * member.i1 / (420 * element.b * element.b * element.b))
                + 42 * element.b * element.b * element.b * (element.d1 * member.i2 / (420 * element.b * element.b * element.b))
                + 42 * element.b * element.b * element.b * (element.d1 * member.i3 / (420 * element.b * element.b * element.b))
                + 13 * element.b * element.b * element.b * element.b * element.b * (element.dy * member.i4 / (420 * element.b * element.b * element.b))
                - 168 * element.b * element.b * element.b * (element.dxy * member.i5 / (420 * element.b * element.b * element.b)));

            ke[5, 7] = (840 * element.b * element.b * (element.dx * member.i1 / (420 * element.b * element.b * element.b))
                + 14 * element.b * element.b * element.b * element.b * (element.d1 * member.i2 / (420 * element.b * element.b * element.b))
                + 14 * element.b * element.b * element.b * element.b * (element.d1 * member.i3 / (420 * element.b * element.b * element.b))
                - 3 * element.b * element.b * element.b * element.b * element.b * element.b * (element.dy * member.i4 / (420 * element.b * element.b * element.b))
                - 56 * element.b * element.b * element.b * element.b * (element.dxy * member.i5 / (420 * element.b * element.b * element.b)));

            ke[6, 4] = (-5040 * (element.dx * member.i1 / (420 * element.b * element.b * element.b))
                + 504 * element.b * element.b * (element.d1 * member.i2 / (420 * element.b * element.b * element.b))
                + 504 * element.b * element.b * (element.d1 * member.i3 / (420 * element.b * element.b * element.b))
                + 54 * element.b * element.b * element.b * element.b * (element.dy * member.i4 / (420 * element.b * element.b * element.b))
                - 2016 * element.b * element.b * (element.dxy * member.i5 / (420 * element.b * element.b * element.b)));

            ke[6, 5] = (-2520 * element.b * (element.dx * member.i1 / (420 * element.b * element.b * element.b))
                + 42 * element.b * element.b * element.b * (element.d1 * member.i2 / (420 * element.b * element.b * element.b))
                + 42 * element.b * element.b * element.b * (element.d1 * member.i3 / (420 * element.b * element.b * element.b))
                + 13 * element.b * element.b * element.b * element.b * element.b * (element.dy * member.i4 / (420 * element.b * element.b * element.b))
                - 168 * element.b * element.b * element.b * (element.dxy * member.i5 / (420 * element.b * element.b * element.b)));

            ke[6, 6] = (5040 * (element.dx * member.i1 / (420 * element.b * element.b * element.b))
                - 504 * element.b * element.b * (element.d1 * member.i2 / (420 * element.b * element.b * element.b))
                - 504 * element.b * element.b * (element.d1 * member.i3 / (420 * element.b * element.b * element.b))
                + 156 * element.b * element.b * element.b * element.b * (element.dy * member.i4 / (420 * element.b * element.b * element.b))
                + 2016 * element.b * element.b * (element.dxy * member.i5 / (420 * element.b * element.b * element.b)));

            ke[6, 7] = (-2520 * element.b * (element.dx * member.i1 / (420 * element.b * element.b * element.b))
                + 462 * element.b * element.b * element.b * (element.d1 * member.i2 / (420 * element.b * element.b * element.b))
                + 42 * element.b * element.b * element.b * (element.d1 * member.i3 / (420 * element.b * element.b * element.b))
                - 22 * element.b * element.b * element.b * element.b * element.b * (element.dy * member.i4 / (420 * element.b * element.b * element.b))
                - 168 * element.b * element.b * element.b * (element.dxy * member.i5 / (420 * element.b * element.b * element.b)));

            ke[7, 4] = (2520 * element.b * (element.dx * member.i1 / (420 * element.b * element.b * element.b))
                - 42 * element.b * element.b * element.b * (element.d1 * member.i2 / (420 * element.b * element.b * element.b))
                - 42 * element.b * element.b * element.b * (element.d1 * member.i3 / (420 * element.b * element.b * element.b))
                - 13 * element.b * element.b * element.b * element.b * element.b * (element.dy * member.i4 / (420 * element.b * element.b * element.b))
                + 168 * element.b * element.b * element.b * (element.dxy * member.i5 / (420 * element.b * element.b * element.b)));

            ke[7, 5] = (840 * element.b * element.b * (element.dx * member.i1 / (420 * element.b * element.b * element.b))
                + 14 * element.b * element.b * element.b * element.b * (element.d1 * member.i2 / (420 * element.b * element.b * element.b))
                + 14 * element.b * element.b * element.b * element.b * (element.d1 * member.i3 / (420 * element.b * element.b * element.b))
                - 3 * element.b * element.b * element.b * element.b * element.b * element.b * (element.dy * member.i4 / (420 * element.b * element.b * element.b))
                - 56 * element.b * element.b * element.b * element.b * (element.dxy * member.i5 / (420 * element.b * element.b * element.b)));

            ke[7, 6] = (-2520 * element.b * (element.dx * member.i1 / (420 * element.b * element.b * element.b))
                + 42 * element.b * element.b * element.b * (element.d1 * member.i2 / (420 * element.b * element.b * element.b))
                + 462 * element.b * element.b * element.b * (element.d1 * member.i3 / (420 * element.b * element.b * element.b))
                - 22 * element.b * element.b * element.b * element.b * element.b * (element.dy * member.i4 / (420 * element.b * element.b * element.b))
                - 168 * element.b * element.b * element.b * (element.dxy * member.i5 / (420 * element.b * element.b * element.b)));

            ke[7, 7] = (1680 * element.b * element.b * (element.dx * member.i1 / (420 * element.b * element.b * element.b))
                - 56 * element.b * element.b * element.b * element.b * (element.d1 * member.i2 / (420 * element.b * element.b * element.b))
                - 56 * element.b * element.b * element.b * element.b * (element.d1 * member.i3 / (420 * element.b * element.b * element.b))
                + 4 * element.b * element.b * element.b * element.b * element.b * element.b * (element.dy * member.i4 / (420 * element.b * element.b * element.b))
                + 224 * element.b * element.b * element.b * element.b * (element.dxy * member.i5 / (420 * element.b * element.b * element.b)));

            //Kg
            double[,] kg = new double[8, 8];
            element.kg.Add(member, kg);
            kg[0, 0] = (3 * element.t1 + element.t2) * element.b * member.i5 / 12;
            kg[0, 1] = 0;
            kg[0, 2] = (element.t1 + element.t2) * element.b * member.i5 / 12;
            kg[0, 3] = 0;
            kg[1, 0] = 0;
            kg[1, 1] = (3 * element.t1 + element.t2) * element.b * member.a * member.a * member.i4 / (12 * member.meum * member.meun);
            kg[1, 2] = 0;
            kg[1, 3] = (element.t1 + element.t2) * element.b * member.a * member.a * member.i4 / (12 * member.meum * member.meun);
            kg[2, 0] = (element.t1 + element.t2) * element.b * member.i5 / 12;
            kg[2, 1] = 0;
            kg[2, 2] = (element.t1 + 3 * element.t2) * element.b * member.i5 / 12;
            kg[2, 3] = 0;
            kg[3, 0] = 0;
            kg[3, 1] = (element.t1 + element.t2) * element.b * member.a * member.a * member.i4 / (12 * member.meum * member.meun);
            kg[3, 2] = 0;
            kg[3, 3] = (element.t1 + 3 * element.t2) * element.b * member.a * member.a * member.i4 / (12 * member.meum * member.meun);




            kg[0, 4] = 0;
            kg[0, 5] = 0;
            kg[0, 6] = 0;
            kg[0, 7] = 0;
            kg[1, 4] = 0;
            kg[1, 5] = 0;
            kg[1, 6] = 0;
            kg[1, 7] = 0;
            kg[2, 4] = 0;
            kg[2, 5] = 0;
            kg[2, 6] = 0;
            kg[2, 7] = 0;
            kg[3, 4] = 0;
            kg[3, 5] = 0;
            kg[3, 6] = 0;
            kg[3, 7] = 0;

            kg[4, 0] = 0;
            kg[4, 1] = 0;
            kg[4, 2] = 0;
            kg[4, 3] = 0;
            kg[5, 0] = 0;
            kg[5, 1] = 0;
            kg[5, 2] = 0;
            kg[5, 3] = 0;
            kg[6, 0] = 0;
            kg[6, 1] = 0;
            kg[6, 2] = 0;
            kg[6, 3] = 0;
            kg[7, 0] = 0;
            kg[7, 1] = 0;
            kg[7, 2] = 0;
            kg[7, 3] = 0;


            kg[4, 4] = (10 * element.t1 + 3 * element.t2) * element.b * member.i5 / 35;
            kg[4, 5] = (15 * element.t1 + 7 * element.t2) * element.b * element.b * member.i5 / 420;
            kg[4, 6] = (9 * element.t1 + 9 * element.t2) * element.b * member.i5 / 140;
            kg[4, 7] = -(7 * element.t1 + 6 * element.t2) * element.b * element.b * member.i5 / 420;
            kg[5, 4] = (15 * element.t1 + 7 * element.t2) * element.b * element.b * member.i5 / 420;
            kg[5, 5] = (5 * element.t1 + 3 * element.t2) * element.b * element.b * element.b * member.i5 / 840;
            kg[5, 6] = (6 * element.t1 + 7 * element.t2) * element.b * element.b * member.i5 / 420;
            kg[5, 7] = -(element.t1 + element.t2) * element.b * element.b * element.b * member.i5 / 280;
            kg[6, 4] = (9 * element.t1 + 9 * element.t2) * element.b * member.i5 / 140;
            kg[6, 5] = (6 * element.t1 + 7 * element.t2) * element.b * element.b * member.i5 / 420;
            kg[6, 6] = (3 * element.t1 + 10 * element.t2) * element.b * member.i5 / 35;
            kg[6, 7] = -(7 * element.t1 + 15 * element.t2) * element.b * element.b * member.i5 / 420;
            kg[7, 4] = -(7 * element.t1 + 6 * element.t2) * element.b * element.b * member.i5 / 420;
            kg[7, 5] = -(element.t1 + element.t2) * element.b * element.b * element.b * member.i5 / 280;
            kg[7, 6] = -(7 * element.t1 + 15 * element.t2) * element.b * element.b * member.i5 / 420;
            kg[7, 7] = (3 * element.t1 + 5 * element.t2) * element.b * element.b * element.b * member.i5 / 840;

            //Ke
            //element.ke[0:3, 0:3] = element.kem;
            //element.ke[4:7, 4:7] = element.keb;


            //Kg
            //element.kg[0:3, 0:3] = element.kgm;
            //element.kg[4:7, 4:7] = element.kgb;

            //Ke For Assembly to m
            //count m and n and get i and j
            //element.KeForM = new double[i, j];
            //element.keForM[(8*(member.m-1)+1) : (8* member.m) , (8 * (member.n - 1) + 1) : (8 * member.n)] = element.ke;

            //Kg For Assembly to m
            //element.KeForM = new double[i, j];
            //element.kgForM[(8 * (member.m - 1) + 1) : (8 * member.m), (8 * (member.n - 1) + 1) : (8 * member.n)] = element.kg;


            //Transformation Matrix
            element.TransformationMatrix[0, 0] = element.CosAlpha;
            element.TransformationMatrix[0, 1] = 0;
            element.TransformationMatrix[0, 2] = 0;
            element.TransformationMatrix[0, 3] = 0;
            element.TransformationMatrix[1, 0] = 0;
            element.TransformationMatrix[1, 1] = 1;
            element.TransformationMatrix[1, 2] = 0;
            element.TransformationMatrix[1, 3] = 0;
            element.TransformationMatrix[2, 0] = 0;
            element.TransformationMatrix[2, 1] = 0;
            element.TransformationMatrix[2, 2] = element.CosAlpha;
            element.TransformationMatrix[2, 3] = 0;
            element.TransformationMatrix[3, 0] = 0;
            element.TransformationMatrix[3, 1] = 0;
            element.TransformationMatrix[3, 2] = 0;
            element.TransformationMatrix[3, 3] = 1;



            element.TransformationMatrix[0, 4] = -element.SinAlpha;
            element.TransformationMatrix[0, 5] = 0;
            element.TransformationMatrix[0, 6] = 0;
            element.TransformationMatrix[0, 7] = 0;


            element.TransformationMatrix[1, 4] = 0;
            element.TransformationMatrix[1, 5] = 0;
            element.TransformationMatrix[1, 6] = 0;
            element.TransformationMatrix[1, 7] = 0;


            element.TransformationMatrix[2, 4] = 0;
            element.TransformationMatrix[2, 5] = 0;
            element.TransformationMatrix[2, 6] = -element.SinAlpha;
            element.TransformationMatrix[2, 7] = 0;


            element.TransformationMatrix[3, 4] = 0;
            element.TransformationMatrix[3, 5] = 0;
            element.TransformationMatrix[3, 6] = 0;
            element.TransformationMatrix[3, 7] = 0;


            element.TransformationMatrix[4, 0] = element.SinAlpha;
            element.TransformationMatrix[4, 1] = 0;
            element.TransformationMatrix[4, 2] = 0;
            element.TransformationMatrix[4, 3] = 0;
            element.TransformationMatrix[4, 4] = element.CosAlpha;
            element.TransformationMatrix[4, 5] = 0;
            element.TransformationMatrix[4, 6] = 0;
            element.TransformationMatrix[4, 7] = 0;

            element.TransformationMatrix[5, 0] = 0;
            element.TransformationMatrix[5, 1] = 0;
            element.TransformationMatrix[5, 2] = 0;
            element.TransformationMatrix[5, 3] = 0;
            element.TransformationMatrix[5, 4] = 0;
            element.TransformationMatrix[5, 5] = 1;
            element.TransformationMatrix[5, 6] = 0;
            element.TransformationMatrix[5, 7] = 0;

            element.TransformationMatrix[6, 0] = 0;
            element.TransformationMatrix[6, 1] = 0;
            element.TransformationMatrix[6, 2] = element.SinAlpha;
            element.TransformationMatrix[6, 3] = 0;
            element.TransformationMatrix[6, 4] = 0;
            element.TransformationMatrix[6, 5] = 0;
            element.TransformationMatrix[6, 6] = element.CosAlpha;
            element.TransformationMatrix[6, 7] = 0;

            element.TransformationMatrix[7, 0] = 0;
            element.TransformationMatrix[7, 1] = 0;
            element.TransformationMatrix[7, 2] = 0;
            element.TransformationMatrix[7, 3] = 0;
            element.TransformationMatrix[7, 4] = 0;
            element.TransformationMatrix[7, 5] = 0;
            element.TransformationMatrix[7, 6] = 0;
            element.TransformationMatrix[7, 7] = 1;


            //element.IMatrix[0, 0] = 1;
            //element.IMatrix[0, 1] = 0;
            //element.IMatrix[1, 0] = 0;
            //element.IMatrix[1, 1] = 1;
        }

        public void TransformStiffnessMatrix(List<Element> elements, Member member) //for each element check transformation matrices size with Ke and Kg
        {
            foreach (var element in elements)
            {
                var ke = element.ke.Where(el => el.Key == member);  // just for making sure that this element has this matrix you can remove 3ady
                if (!ke.Any())
                {
                    continue;
                }
                Matrix<double> keMatrix = Matrix<double>.Build.DenseOfArray(ke.First().Value);  // transforming dictionary ke matrix into Mathnet Ke matrix to do operation

                var kg = element.kg.Where(el => el.Key == member);  // just for making sure that this element has this matrix you can remove 3ady
                if (!kg.Any())
                {
                    continue;
                }
                Matrix<double> kgMatrix = Matrix<double>.Build.DenseOfArray(kg.First().Value);  // transforming dictionary kg matrix into Mathnet Kg matrix to do operation

                //var Transformedke = element.Transformedke.Where(el => el.Key == member);  // just for making sure that this element has this matrix you can remove 3ady
                //if (!Transformedke.Any())
                //{
                //    continue;
                //}
                //Matrix<double> TransformedkeMatrix = Matrix<double>.Build.DenseOfArray(Transformedke.First().Value);


                //var Transformedkg = element.Transformedkg.Where(el => el.Key == member);  // just for making sure that this element has this matrix you can remove 3ady
                //if (!Transformedkg.Any())
                //{
                //    continue;
                //}
                //Matrix<double> TransformedkgMatrix = Matrix<double>.Build.DenseOfArray(Transformedkg.First().Value);


                // Doing the transformation process for both stiffness matrices
                element.Transformedke = element.TransformationMatrix.Multiply(keMatrix).Multiply(element.TransformationMatrix.Transpose());
                element.Transformedkg = element.TransformationMatrix.Multiply(kgMatrix).Multiply(element.TransformationMatrix.Transpose());

                //element.Transformedke;
                //return element.Transformedkg;

            }
            //you should return (element.Transformedke and element.Transformed kg) for each element
        }

        public void DivideStiffnessMatrix(List<Element> elements, Member member) //Using a certain state of the member (certain a & m)
        {
            foreach (var element in elements)
            {
                // For Ke
                double[,] kemm = new double[4, 4];
                element.kemm.Add(member, kemm);
                kemm[0, 0] = element.Transformedke[0, 0];
                kemm[0, 1] = element.Transformedke[0, 1];
                kemm[0, 2] = element.Transformedke[0, 2];
                kemm[0, 3] = element.Transformedke[0, 3];
                kemm[1, 0] = element.Transformedke[1, 0];
                kemm[1, 1] = element.Transformedke[1, 1];
                kemm[1, 2] = element.Transformedke[1, 2];
                kemm[1, 3] = element.Transformedke[1, 3];
                kemm[2, 0] = element.Transformedke[2, 0];
                kemm[2, 1] = element.Transformedke[2, 1];
                kemm[2, 2] = element.Transformedke[2, 2];
                kemm[2, 3] = element.Transformedke[2, 3];
                kemm[3, 0] = element.Transformedke[3, 0];
                kemm[3, 1] = element.Transformedke[3, 1];
                kemm[3, 2] = element.Transformedke[3, 2];
                kemm[3, 3] = element.Transformedke[3, 3];

                double[,] kemb = new double[4, 4];
                element.kemb.Add(member, kemb);
                kemb[0, 0] = element.Transformedke[0, 4];
                kemb[0, 1] = element.Transformedke[0, 5];
                kemb[0, 2] = element.Transformedke[0, 6];
                kemb[0, 3] = element.Transformedke[0, 7];
                kemb[1, 0] = element.Transformedke[1, 4];
                kemb[1, 1] = element.Transformedke[1, 5];
                kemb[1, 2] = element.Transformedke[1, 6];
                kemb[1, 3] = element.Transformedke[1, 7];
                kemb[2, 0] = element.Transformedke[2, 4];
                kemb[2, 1] = element.Transformedke[2, 5];
                kemb[2, 2] = element.Transformedke[2, 6];
                kemb[2, 3] = element.Transformedke[2, 7];
                kemb[3, 0] = element.Transformedke[3, 4];
                kemb[3, 1] = element.Transformedke[3, 5];
                kemb[3, 2] = element.Transformedke[3, 6];
                kemb[3, 3] = element.Transformedke[3, 7];

                double[,] kebm = new double[4, 4];
                element.kebm.Add(member, kebm);
                kebm[0, 0] = element.Transformedke[4, 0];
                kebm[0, 1] = element.Transformedke[4, 1];
                kebm[0, 2] = element.Transformedke[4, 2];
                kebm[0, 3] = element.Transformedke[4, 3];
                kebm[1, 0] = element.Transformedke[5, 0];
                kebm[1, 1] = element.Transformedke[5, 1];
                kebm[1, 2] = element.Transformedke[5, 2];
                kebm[1, 3] = element.Transformedke[5, 3];
                kebm[2, 0] = element.Transformedke[6, 0];
                kebm[2, 1] = element.Transformedke[6, 1];
                kebm[2, 2] = element.Transformedke[6, 2];
                kebm[2, 3] = element.Transformedke[6, 3];
                kebm[3, 0] = element.Transformedke[7, 0];
                kebm[3, 1] = element.Transformedke[7, 1];
                kebm[3, 2] = element.Transformedke[7, 2];
                kebm[3, 3] = element.Transformedke[7, 3];

                double[,] kebb = new double[4, 4];
                element.kebb.Add(member, kebb);
                kebb[0, 0] = element.Transformedke[4, 4];
                kebb[0, 1] = element.Transformedke[4, 5];
                kebb[0, 2] = element.Transformedke[4, 6];
                kebb[0, 3] = element.Transformedke[4, 7];
                kebb[1, 0] = element.Transformedke[5, 4];
                kebb[1, 1] = element.Transformedke[5, 5];
                kebb[1, 2] = element.Transformedke[5, 6];
                kebb[1, 3] = element.Transformedke[5, 7];
                kebb[2, 0] = element.Transformedke[6, 4];
                kebb[2, 1] = element.Transformedke[6, 5];
                kebb[2, 2] = element.Transformedke[6, 6];
                kebb[2, 3] = element.Transformedke[6, 7];
                kebb[3, 0] = element.Transformedke[7, 4];
                kebb[3, 1] = element.Transformedke[7, 5];
                kebb[3, 2] = element.Transformedke[7, 6];
                kebb[3, 3] = element.Transformedke[7, 7];

                // For Kg
                double[,] kgmm = new double[4, 4];
                element.kgmm.Add(member, kgmm);
                kgmm[0, 0] = element.Transformedkg[0, 0];
                kgmm[0, 1] = element.Transformedkg[0, 1];
                kgmm[0, 2] = element.Transformedkg[0, 2];
                kgmm[0, 3] = element.Transformedkg[0, 3];
                kgmm[1, 0] = element.Transformedkg[1, 0];
                kgmm[1, 1] = element.Transformedkg[1, 1];
                kgmm[1, 2] = element.Transformedkg[1, 2];
                kgmm[1, 3] = element.Transformedkg[1, 3];
                kgmm[2, 0] = element.Transformedkg[2, 0];
                kgmm[2, 1] = element.Transformedkg[2, 1];
                kgmm[2, 2] = element.Transformedkg[2, 2];
                kgmm[2, 3] = element.Transformedkg[2, 3];
                kgmm[3, 0] = element.Transformedkg[3, 0];
                kgmm[3, 1] = element.Transformedkg[3, 1];
                kgmm[3, 2] = element.Transformedkg[3, 2];
                kgmm[3, 3] = element.Transformedkg[3, 3];

                double[,] kgmb = new double[4, 4];
                element.kgmb.Add(member, kgmb);
                kgmb[0, 0] = element.Transformedkg[0, 4];
                kgmb[0, 1] = element.Transformedkg[0, 5];
                kgmb[0, 2] = element.Transformedkg[0, 6];
                kgmb[0, 3] = element.Transformedkg[0, 7];
                kgmb[1, 0] = element.Transformedkg[1, 4];
                kgmb[1, 1] = element.Transformedkg[1, 5];
                kgmb[1, 2] = element.Transformedkg[1, 6];
                kgmb[1, 3] = element.Transformedkg[1, 7];
                kgmb[2, 0] = element.Transformedkg[2, 4];
                kgmb[2, 1] = element.Transformedkg[2, 5];
                kgmb[2, 2] = element.Transformedkg[2, 6];
                kgmb[2, 3] = element.Transformedkg[2, 7];
                kgmb[3, 0] = element.Transformedkg[3, 4];
                kgmb[3, 1] = element.Transformedkg[3, 5];
                kgmb[3, 2] = element.Transformedkg[3, 6];
                kgmb[3, 3] = element.Transformedkg[3, 7];

                double[,] kgbm = new double[4, 4];
                element.kgbm.Add(member, kgbm);
                kgbm[0, 0] = element.Transformedkg[4, 0];
                kgbm[0, 1] = element.Transformedkg[4, 1];
                kgbm[0, 2] = element.Transformedkg[4, 2];
                kgbm[0, 3] = element.Transformedkg[4, 3];
                kgbm[1, 0] = element.Transformedkg[5, 0];
                kgbm[1, 1] = element.Transformedkg[5, 1];
                kgbm[1, 2] = element.Transformedkg[5, 2];
                kgbm[1, 3] = element.Transformedkg[5, 3];
                kgbm[2, 0] = element.Transformedkg[6, 0];
                kgbm[2, 1] = element.Transformedkg[6, 1];
                kgbm[2, 2] = element.Transformedkg[6, 2];
                kgbm[2, 3] = element.Transformedkg[6, 3];
                kgbm[3, 0] = element.Transformedkg[7, 0];
                kgbm[3, 1] = element.Transformedkg[7, 1];
                kgbm[3, 2] = element.Transformedkg[7, 2];
                kgbm[3, 3] = element.Transformedkg[7, 3];

                double[,] kgbb = new double[4, 4];
                element.kgbb.Add(member, kgbb);
                kgbb[0, 0] = element.Transformedkg[4, 4];
                kgbb[0, 1] = element.Transformedkg[4, 5];
                kgbb[0, 2] = element.Transformedkg[4, 6];
                kgbb[0, 3] = element.Transformedkg[4, 7];
                kgbb[1, 0] = element.Transformedkg[5, 4];
                kgbb[1, 1] = element.Transformedkg[5, 5];
                kgbb[1, 2] = element.Transformedkg[5, 6];
                kgbb[1, 3] = element.Transformedkg[5, 7];
                kgbb[2, 0] = element.Transformedkg[6, 4];
                kgbb[2, 1] = element.Transformedkg[6, 5];
                kgbb[2, 2] = element.Transformedkg[6, 6];
                kgbb[2, 3] = element.Transformedkg[6, 7];
                kgbb[3, 0] = element.Transformedkg[7, 4];
                kgbb[3, 1] = element.Transformedkg[7, 5];
                kgbb[3, 2] = element.Transformedkg[7, 6];
                kgbb[3, 3] = element.Transformedkg[7, 7];


            }

        }


        public double[,] AssembleStiffnessMatrix_e(List<Element> elements, Member member)        //Assembly for each m and a for Kelastic
        {
            int nNodes = elements.Count + 1;
            double[,] Overall_ke = new double[4 * nNodes, 4 * nNodes];
            for (int i = 0; i < 4 * nNodes; i++)
            {
                for (int j = 0; j < 4 * nNodes; j++)
                {
                    Overall_ke[i, j] = 0;
                }
            }
            elements = elements.OrderBy(o => o.node1.ID).ToList();
            int start;
            foreach (var element in elements)
            {
                start = (element.node1.ID - 1) * 2;
                for (int r = 0; r < 4; r++)
                {
                    for (int c = 0; c < 4; c++)
                    {
                        Overall_ke[start + r, start + c] = Overall_ke[start + r, start + c] + element.kemm[member][r, c];
                        Overall_ke[start + r + 2 * nNodes, start + c + 2 * nNodes] = Overall_ke[start + r + 2 * nNodes, start + c + 2 * nNodes] + element.kebb[member][r, c];
                        Overall_ke[start + r, start + c + 2 * nNodes] = Overall_ke[start + r, start + c + 2 * nNodes] + element.kemb[member][r, c];
                        Overall_ke[start + r + 2 * nNodes, start + c] = Overall_ke[start + r + 2 * nNodes, start + c] + element.kebm[member][r, c];
                    }
                }
            }
            return Overall_ke;
        }

        public double[,] AssembleStiffnessMatrix_g(List<Element> elements, Member member)   //Assembly for each m and a for Kgeometric
        {
            int nNodes = elements.Count + 1;
            double[,] Overall_kg = new double[4 * nNodes, 4 * nNodes];
            for (int i = 0; i < 4 * nNodes; i++)
            {
                for (int j = 0; j < 4 * nNodes; j++)
                {
                    Overall_kg[i, j] = 0;
                }
            }
            elements = elements.OrderBy(o => o.node1.ID).ToList();
            int start;
            foreach (var element in elements)
            {
                start = (element.node1.ID - 1) * 2;
                for (int r = 0; r < 4; r++)
                {
                    for (int c = 0; c < 4; c++)
                    {
                        Overall_kg[start + r, start + c] = Overall_kg[start + r, start + c] + element.kgmm[member][r, c];
                        Overall_kg[start + r + 2 * nNodes, start + c + 2 * nNodes] = Overall_kg[start + r + 2 * nNodes, start + c + 2 * nNodes] + element.kgbb[member][r, c];
                        Overall_kg[start + r, start + c + 2 * nNodes] = Overall_kg[start + r, start + c + 2 * nNodes] + element.kgmb[member][r, c];
                        Overall_kg[start + r + 2 * nNodes, start + c] = Overall_kg[start + r + 2 * nNodes, start + c] + element.kgbm[member][r, c];
                    }
                }
            }
            return Overall_kg;
        }

        //public double[,] AssembleIMatrix(List<Element> elements, Member member)  //Assemble unit matrix I
        //{
        //    int nNodes = elements.Count + 1;
        //    double[,] Overall_I = new double[4 * nNodes, 4 * nNodes];
        //    for (int i = 0; i < 2 * nNodes; i++)
        //    {
        //        for (int j = 0; j < 2 * nNodes; j++)
        //        {
        //            Overall_I[i, j] = 0;
        //        }
        //    }
        //    elements = elements.OrderBy(o => o.node1.ID).ToList();
        //    int start;
        //    foreach (var element in elements)
        //    {
        //        start = (element.node1.ID - 1) * 2;
        //        for (int r = 0; r < 1; r++)
        //        {
        //            for (int c = 0; c < 1; c++)
        //            {
        //                Overall_I[start + r, start + c] = Overall_I[start + r, start + c] + element.IMatrix[r, c];
        //            }
        //        }
        //    }
        //    return Overall_I;
        //}

        public void SolveEigenValueProblem(List<Element> elements, Member member)
        {

            Matrix<double> keFinalMatrix = Matrix<double>.Build.DenseOfArray(member.FinalGlobalKe);
            Matrix<double> kgFinalMatrix = Matrix<double>.Build.DenseOfArray(member.FinalGlobalKg);

            member.I = Matrix<double>.Build.DenseIdentity(kgFinalMatrix.RowCount, kgFinalMatrix.ColumnCount);
            //member.U = kgFinalMatrix.Svd().U;
            //member.S = kgFinalMatrix.Svd().W;
            //member.Vt = kgFinalMatrix.Svd().VT;
            //member.S12 = Matrix<double>.Sqrt(member.S);
            //Matrix<double> S12 = Matrix<double>.Build.Dense(member.S.RowCount, member.S.ColumnCount);
            //for (int r = 0; r < member.S.RowCount; r++)
            //{
            //    if (member.S[r, r] != 0)
            //        S12[r, r] = Math.Pow(member.S[r, r],-0.5);
            //}

            //Second Trial  kg+(10-5)I
            //if (loadcases.P == 0)
            //{

            // member.GlobalKgGlobalKe = (kgFinalMatrix + (Math.Pow(10, -5) * member.I)).Inverse().Multiply(keFinalMatrix);
            // member.EigenValue = member.GlobalKgGlobalKe.Evd().EigenValues.Real();

            //}
            //else
            //{ 
            //    //First Trial Kg-1 Ke
            //    member.GlobalKgGlobalKe = kgFinalMatrix.Inverse().Multiply(keFinalMatrix);
            //    member.EigenValue = member.GlobalKgGlobalKe.Evd().EigenValues.Real();
            //}

            //Third Trial  ke-1 kg
            member.GlobalKgGlobalKe = keFinalMatrix.Inverse().Multiply(kgFinalMatrix);
            member.EigenValue0 = member.GlobalKgGlobalKe.Evd().EigenValues.Real();
            member.EigenValue = 1 / member.EigenValue0;

            //Fourth Trial SVD
            //member.EigenValue = (S12.Multiply(member.U.Transpose()).Multiply(keFinalMatrix).Multiply(member.Vt.Transpose()).Multiply(S12)).Evd().EigenValues.Real();

            //Fifth Trial SVD
            //member.EigenValue = ((member.Vt.Transpose()).Multiply(S12).Multiply(member.U.Transpose()).Multiply(keFinalMatrix)).Evd().EigenValues.Real();

            //For testing
            EigenValues = member.EigenValue;
            //member.MinEigenValue = member.EigenValue.Where(v => v >= 0).Min();
            member.MinEigenValue = member.EigenValue.Where(v => v > 0).Min();
            //member.EigenVectors = member.GlobalKgGlobalKe.Evd().EigenVectors();
        }


        //public double[,] AssembleStiffnessMatrix_e(List<Element> elements,Member member)
        //{
        //    int nNodes = elements.Count +1;
        //    double[,] Overall_k = new double[4* nNodes, 4* nNodes];
        //    for (int i = 0; i < 4 * nNodes; i++)
        //    {
        //        for (int j = 0; j < 4 * nNodes; j++)
        //        {
        //            Overall_k[i, j] = 0;
        //        }
        //    }
        //    elements = elements.OrderBy(o => o.node1.ID).ToList();
        //    int start;
        //    foreach (var element in elements)
        //    {
        //        start = (element.node1.ID - 1) * 4;
        //        for (int r = 0; r < 8; r++)
        //        {
        //            for (int c = 0; c < 8; c++)
        //            {
        //                Overall_k[start + r, start + c] = Overall_k[start + r, start + c] + element.ke[member][r, c];
        //            }
        //        }
        //    }
        //    return Overall_k;
        //}


        public void SolveDesignEquations()
        {
            design.lamdaPG = Math.Sqrt(design.Py / design.PcrG); // kanet loadcases.py instead of design.py
            design.lamdaPLG = Math.Sqrt(design.PulG / design.Pcrl);
            design.lamdaPD = Math.Sqrt(design.Py / design.Pcrd);  //kanet loadcases.py instead of design.py

            design.lamdaMLTB = Math.Sqrt(design.Mxy / design.McrLTB); // kanet loadcases.Mxy instead of design.Mxy
            design.lamdaMLLTB = Math.Sqrt(Math.Min(design.MulLTB, design.Mxy) / design.Mcrl);  // kanet loadcases.Mxy instead of design.Mxy
            design.lamdaMD = Math.Sqrt(design.Mxy / design.Mcrd); // kanet loadcases.Mxy instead of design.Mxy

            //compression-Global 
            if (design.lamdaPG > 1.5)
            {
                design.PulG = 0.877 * Math.Pow(design.lamdaPG, -2) * design.Py;
            }
            else
            {
                design.PulG = Math.Pow(0.658, design.lamdaPG * design.lamdaPG) * design.Py;
            }

            //compression-Local-Global interaction
            if (design.lamdaPLG > 0.776)
            {
                design.Pull = (1 - (0.15 * Math.Pow(design.lamdaPLG, -0.8))) * Math.Pow(design.lamdaPLG, -0.8)
                                      * design.PulG;
            }
            else
            {
                design.Pull = design.PulG;
            }

            //compression-Distortional
            if (design.lamdaPD > 0.561)
            {
                design.Puld = (1 - (0.25 * Math.Pow(design.lamdaPLG, -1.2))) * Math.Pow(design.lamdaPLG, -1.2)
                                      * design.Py;
            }
            else
            {
                design.Puld = design.Py;
            }




            //Moment-LTB   
            if (design.lamdaMLTB <= 0.6)
            {
                design.MulLTB = ((design.Mp / design.Mxy) - (((design.Mp / design.Mxy) - 1) * ((design.lamdaMLTB - 0.23) / 0.37))) * design.Mxy;
            }
            else if (design.lamdaMLTB > 0.6 && design.lamdaMLTB <= 1.34)
            {
                design.MulLTB = (10 / 9 * (1 - (10 / 36 * design.lamdaMLTB * design.lamdaMLTB))) * design.Mxy;
            }
            else
            {
                design.MulLTB = Math.Pow(design.lamdaMLTB, -2);
            }

            //Moment-Local-LTB interaction   
            if (design.lamdaMLLTB > 0.776)
            {
                design.Mull = (1 - (0.15 * Math.Pow(design.lamdaMLLTB, -0.8))) * Math.Pow(design.lamdaMLLTB, -0.8)
                                         * Math.Min(design.MulLTB, design.Mxy);
            }
            else
            {
                design.Mull = (1 + ((1 - (design.lamdaMLLTB / 0.776)) * (design.Mp / design.Mxy - 1))) * Math.Min(design.MulLTB, design.Mxy);
            }

            //Moment-Distortional  wrong expressions (only REMOVE the // and put Mp and check brackets)
            if (design.lamdaMD > 0.673)
            {
                design.Muld = (1 - (0.22 * Math.Pow(design.lamdaMD, -1))) * Math.Pow(design.lamdaMD, -1)
                                      * design.Mxy;
            }
            else
            {
                design.Muld = (1 + ((1 - (design.lamdaMD / 0.673)) * (design.Mp / design.Mxy - 1))) * design.Mxy;
            }

        }



        private void PrintSummary()
        {
            
            //Elements
            StringBuilder elementBuilder = new StringBuilder();
            StringBuilder elementBuilder2 = new StringBuilder();

            SetupOutputFolder();
            string summaryPath = Path.Combine(outputPathName, "summary.csv");

            var isNotFirstCase = File.Exists(summaryPath);
            string[] file = new string[1];
            if (isNotFirstCase)
            {
                file = File.ReadAllLines(summaryPath);
                file[0] = file[0] + ",a,m,Min. eigen value,Area";
            }

            elementBuilder.Append("a,m,Min. eigen value,Area" + Environment.NewLine);
            elementBuilder2.Append("a,m,Min. eigen value of all minimums" + Environment.NewLine);
            int i = 1;
            foreach (var member in Members)
            {
                if (isNotFirstCase)
                {
                    file[i] = file[i] + "," + member.a + "," + member.m + "," + member.MinEigenValue + "," + section.A;
                    i++;
                }
                else
                {
                    elementBuilder.Append(member.a + "," + member.m + "," + member.MinEigenValue + "," + section.A + Environment.NewLine);
                }
            }

            var min_member = Members.OrderBy(m => m.MinEigenValue).First();
            elementBuilder2.Append(min_member.a + "," + min_member.m + "," + min_member.MinEigenValue + Environment.NewLine);
            if (isNotFirstCase)
            {
                //Not the first case
                File.WriteAllLines(summaryPath, file);
                //elementBuilder.Insert(0, Environment.NewLine);
                //System.IO.File.AppendAllText(Path.Combine(outputPathName, "summary.csv"), elementBuilder.ToString());
            }
            else
            {
                File.WriteAllText(summaryPath, elementBuilder.ToString());
            }

            string summaryOfSummaryPath = Path.Combine(outputPathName, "summary_of_summary.csv");
            if (File.Exists(summaryOfSummaryPath))
            {
                elementBuilder2.Insert(0, Environment.NewLine);
                File.AppendAllText(summaryOfSummaryPath, elementBuilder2.ToString());
            }
            else
            {
                File.WriteAllText(summaryOfSummaryPath, elementBuilder2.ToString());
            }

        }

        private void PrintOutput(List<Element> elements)
        {
            PrintSummary();
            //Elements
            StringBuilder elementBuilder = new StringBuilder();

            elementBuilder.Append("" + "deltax" + '\t' + "deltay" + '\t' +
                      "b " + '\t' + "alpha" + '\t' + "sinalpha" + '\t' + "cosalpha" + '\t' +
                      "thetax" + '\t' + "thetay" + '\t' +
                      "Xcg" + '\t' + "Ycg" + '\t' +
                      "A" + '\t' + "Ax" + '\t' +
                      "Axx" + '\t' + "Ay" + '\t' +
                      "Ayy" + '\t' + "Axy" + '\t'
                      + "deltaxdeltay12" + '\t' +
                      "deltaxdeltax12" + '\t' + "deltaydeltay12" + '\t' +
                      "ymeanminusycg" + '\t' + "xmeanminusxcg" + '\t' +
                      "xmeanminusxcgymeanminusycg" + '\t' + "ymeanminusycg2"
                      + '\t' + "xmeanminusxcg2" + '\t'
                      + "Ix" + '\t' +
                      "Iy" + '\t' + "Ixy" + '\t' +
                      "J" + '\t' + "dx" + '\t' +
                      "dy" + '\t' + "dxy" + '\t' +
                      "d1" + '\t' + "f1" + '\t' +
                      "f2" + '\t' + "t1" + '\t' +
                      "t2" + Environment.NewLine);
            foreach (var el in elements)
            {
                elementBuilder.Append(el.ToString());
            }
            File.WriteAllText(Path.Combine(outputPathName, "elements.txt"), elementBuilder.ToString());

            //Section
            StringBuilder sectionBuilder = new StringBuilder();

            sectionBuilder.Append("" + "Xcg" + '\t' + "Ycg" + '\t' +
                      "A" + '\t' + "Ax" + '\t' +
                      "Axx" + '\t' + "Ay" + '\t' +
                      "Ayy" + '\t' + "Axy" + '\t' +
                      "sigmaIx" + '\t' + "sigmaIy" + '\t'
                      + "sigmaIxy" + '\t'
                      + "sigmadeltaxdeltay12" + '\t' + "sigmadeltaxdeltax12" + '\t' + "sigmadeltaydeltay12" + '\t'
                      + "sigmaymeanminusycg" + '\t' + "sigmaxmeanminusxcg" + '\t'
                      + "sigmaxmeanminusxcgymeanminusycg" + '\t' + "sigmaymeanminusycg2" + '\t'
                      + "sigmaxmeanminusxcg2" + '\t'
                      + "Ix" + '\t' +
                      "Iy" + '\t' + "Ixy" + '\t' +
                      "I1" + '\t' + "I2" + '\t' +
                      "PolarTheta" + '\t' + "J" + Environment.NewLine);
            sectionBuilder.Append(section);
            File.WriteAllText(Path.Combine(outputPathName, "section.txt"), sectionBuilder.ToString());

            //Node Stresses
            StringBuilder nodeStressBuilder = new StringBuilder();

            nodeStressBuilder.Append("" + "stress" + '\t' + "stressmxyield" + '\t' +
                          "stressmyyield " + '\t' + Environment.NewLine);

            foreach (var node in Nodes)
            {

                nodeStressBuilder.Append(node.ToString());
            }


            File.WriteAllText(Path.Combine(outputPathName, "nodestress.txt"), nodeStressBuilder.ToString());

            //Members

            StringBuilder MembersBuilder = new StringBuilder();

            MembersBuilder.Append("" + "c1" + '\t' + "c2" + '\t' +
                          "meum " + '\t' + "meun " + '\t' + "i1 " + '\t' + "i2 " + '\t'
                          + "i3 " + '\t' + "i4 " + '\t' + "i5 " + '\t' + Environment.NewLine);
            foreach (var member in Members)
            {

                MembersBuilder.Append(member.ToString());
            }

            File.WriteAllText(Path.Combine(outputPathName, "member.txt"), MembersBuilder.ToString());

            /* Open files
            Process.Start(Path.Combine(outputPathName, "summary.csv"));
            Process.Start(Path.Combine(outputPathName, "elements.txt"));
            Process.Start(Path.Combine(outputPathName, "section.txt"));
            Process.Start(Path.Combine(outputPathName, "nodestress.txt"));
            Process.Start(Path.Combine(outputPathName, "member.txt"));
            */
            MessageBox.Show(@"Output Files Saved!");
            Process.Start(outputPathName);
        }
        private void SectionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void SolveButton_Click(object sender, EventArgs e)
        {

            {
                IsParametric = false;
                Solve();
            }
        }
        private void btnparametric_Click(object sender, EventArgs e)
        {
            IsParametric = true;
            performparametric();
        }
        public List<double[]> GetCases()
        {
            var res = new List<double[]>();

            string[] sections = parsectionTextBox.Text.Split('\n'); //ta2seem ellines 3shan dayman first line is first case
            string[] loads = parloadsTextBox.Text.Split('\n');



            for (int i = 0; i < sections.Length; i++)
            {
                string[] secStr = sections[i].Split(',');
                string[] loadStr = loads[i].Split(',');
                double[] caseA = new double[10];


                for (int j = 0; j < 7; j++)
                {
                    caseA[j] = double.Parse(secStr[j]);
                }

                for (int j = 0; j < 3; j++)
                {
                    caseA[7 + j] = double.Parse(loadStr[j]);
                }

                res.Add(caseA);
            }

            return res;
        }

        public void performparametric(List<Material> materials = null, List<Element> elems = null)
        {
            //Validation
            if (
                parbccomboBox.SelectedIndex < 0
                ||
                parsectioncombobox.SelectedIndex < 0
                )
            {
                var msg = @"Inputs are not valid! You must select boundary condition & section.";
                groupBarParametricStudies.SelectedItem = parbccomboBox.SelectedIndex < 0 ? 1 : 2; //Select section of missing data
                MessageBox.Show(msg);
                return;
            }

            var cases = GetCases();
            foreach (var caseA in cases)
            {
                //constants
                BoundryCondition = parbccomboBox.SelectedItem.ToString(); //elmafrood ad50l 3la kol wa7ed mn dool fe get w a5aleeh ya5od awl satr mn elparametric user interface
                materials = GetMaterialValues();
                Members = GetMemberValues();

                double[] sectionsData = { caseA[0], caseA[1], caseA[2], caseA[3], caseA[4], caseA[5], caseA[6] };
                Getsectionvalues(sectionsData);
                sectionComboBox.SelectedItem = parsectioncombobox.SelectedItem;
                MESHSECTION();
                txt_nodeData.Text = section.sectionfinaltextnode;
                txt_elementsData.Text = section.sectionfinaltextelement;

                Nodes = GetNodeValues();
                elems = GetElementValues();
                double[] loads = { caseA[7], caseA[8], caseA[9] };
                GetLoadCasesValues(loads);

                Solve(materials, elems);
            }
            Process.Start(Path.Combine(outputPathName, "summary.csv"));




            //#region Parse input from files
            ////Read Values from files
            ////var all_materials_txt = System.IO.File.ReadAllLines("E:\\Research\\cold formed\\Program coding\\Input files Parametric\\materials.txt");
            ////var all_sections_txt = System.IO.File.ReadAllLines("E:\\Research\\cold formed\\Program coding\\Input files Parametric\\sections.txt");
            //////var all_lists_of_nodes_txt = System.IO.File.ReadAllLines("E:\\Research\\cold formed\\Program coding\\Input files Parametric\\nodes_lists.txt");
            //////var all_lists_of_elements_txt = System.IO.File.ReadAllLines("E:\\Research\\cold formed\\Program coding\\Input files Parametric\\elements_lists.txt");
            ////var all_lists_of_members_txt = System.IO.File.ReadAllLines("E:\\Research\\cold formed\\Program coding\\Input files Parametric\\members_lists.txt");
            ////var all_lists_of_loads_txt = System.IO.File.ReadAllLines("E:\\Research\\cold formed\\Program coding\\Input files Parametric\\loads_lists.txt");
            ////var all_boundry_conditions_txt = System.IO.File.ReadAllLines("E:\\Research\\cold formed\\Program coding\\Input files Parametric\\boundry_conditions.txt");
            ////var all_constants_txt = System.IO.File.ReadAllLines("E:\\Research\\cold formed\\Program coding\\Input files Parametric\\constants.txt");

            //var all_materials_txt = parmaterialTextBox.Text;
            //var all_sections_txt = System.IO.File.ReadAllLines("E:\\Research\\cold formed\\Program coding\\Input files Parametric\\sections.txt");
            //var all_lists_of_members_txt = System.IO.File.ReadAllLines("E:\\Research\\cold formed\\Program coding\\Input files Parametric\\loads_lists.txt");
            //var all_lists_of_loads_txt = System.IO.File.ReadAllLines("E:\\Research\\cold formed\\Program coding\\Input files Parametric\\loads_lists.txt");
            //var all_boundry_conditions_txt = System.IO.File.ReadAllLines("E:\\Research\\cold formed\\Program coding\\Input files Parametric\\boundry_conditions.txt");
            //var all_constants_txt = System.IO.File.ReadAllLines("E:\\Research\\cold formed\\Program coding\\Input files Parametric\\constants.txt");
            //var cases_materials = new List<string>();
            //var cases_sections = new List<string>();
            //var cases_nodes = new List<string>();
            //var cases_elements = new List<string>();
            //var cases_members = new List<string[]>();
            //var cases_loads = new List<string[]>();
            //var cases_boundry_conditions = new List<string>();
            //var cases_constants = new List<string>();

            ////one_case = ""; //gdeed for section meshing
            ////string[] case_section = new string[7];
            ////case_repeats = 0;
            ////foreach (var line in all_sections_txt)
            ////{
            ////    if (line == "END")
            ////    {
            ////        break;
            ////    }
            ////    if (string.IsNullOrEmpty(line))
            ////    {
            ////        if (index < 7)
            ////        {
            ////            case_section[index] = one_case;
            ////            index++;
            ////            one_case = "";
            ////        }

            ////        if (index == 6)
            ////        {
            ////            while (case_repeats > 0)
            ////            {
            ////                case_section.Add(case_section);
            ////                case_repeats--;
            ////            }
            ////            index = 0;
            ////            case_section = new string[7];
            ////        }
            ////        continue;
            ////    }
            ////    if (one_case == "START")
            ////    {
            ////        Line of repeats
            ////        one_case = "";
            ////        case_repeats = int.Parse(line);
            ////        continue;
            ////    }
            ////    one_case += line + "\n";
            ////} //end of gdeed





            //string one_case = "START";
            //int case_repeats = 0;
            //foreach (var line in all_materials_txt)
            //{
            //    if (line == "END")
            //    {
            //        break;
            //    }
            //    if (string.IsNullOrEmpty(line))
            //    {
            //        while (case_repeats > 0)
            //        {
            //            cases_materials.Add(one_case);
            //            case_repeats--;
            //        }

            //        one_case = "";
            //        continue;
            //    }

            //    if (one_case == "START")
            //    {
            //        //Line of repeats
            //        one_case = "";
            //        case_repeats = int.Parse(line);
            //        continue;
            //    }
            //    one_case += line + "\n";
            //}

            ////one_case = "";
            ////foreach (var line in all_lists_of_nodes_txt)
            ////{
            ////    if (line == "END")
            ////    {
            ////        break;
            ////    }
            ////    if (string.IsNullOrEmpty(line))
            ////    {
            ////        cases_nodes.Add(one_case);
            ////        one_case = "";
            ////        continue;
            ////    }
            ////    //Values exists
            ////    one_case += line + "\n";
            ////}


            ////one_case = "";
            ////case_repeats = 0;
            ////foreach (var line in all_lists_of_elements_txt)
            ////{
            ////    if (line == "END")
            ////    {
            ////        break;
            ////    }
            ////    if (string.IsNullOrEmpty(line))
            ////    {
            ////        while (case_repeats > 0)
            ////        {
            ////            cases_elements.Add(one_case);
            ////            case_repeats--;
            ////        }

            ////        one_case = "";
            ////        continue;
            ////    }
            ////    if (one_case == "START")
            ////    {
            ////        //Line of repeats
            ////        one_case = "";
            ////        case_repeats = int.Parse(line);
            ////        continue;
            ////    }
            ////    one_case += line + "\n";
            ////}

            //one_case = "";
            //int index = 0;
            //case_repeats = 0;
            //string[] case_member = new string[4];
            //foreach (var line in all_lists_of_members_txt)
            //{
            //    if (line == "END")
            //    {
            //        break;
            //    }
            //    if (string.IsNullOrEmpty(line))
            //    {
            //        if (index < 4)
            //        {
            //            case_member[index] = one_case;
            //            index++;
            //            one_case = "";
            //        }

            //        if (index == 3)
            //        {
            //            while (case_repeats > 0)
            //            {
            //                cases_members.Add(case_member);
            //                case_repeats--;
            //            }

            //            index = 0;
            //            case_member = new string[4];
            //        }
            //        continue;
            //    }
            //    if (one_case == "START")
            //    {
            //        //Line of repeats
            //        one_case = "";
            //        case_repeats = int.Parse(line);
            //        continue;
            //    }
            //    one_case += line + "\n";
            //}

            //one_case = "";
            //string[] case_loads = new string[3];
            //case_repeats = 0;
            //foreach (var line in all_lists_of_loads_txt)
            //{
            //    if (line == "END")
            //    {
            //        break;
            //    }
            //    if (string.IsNullOrEmpty(line))
            //    {
            //        if (index < 3)
            //        {
            //            case_loads[index] = one_case;
            //            index++;
            //            one_case = "";
            //        }

            //        if (index == 2)
            //        {
            //            while (case_repeats > 0)
            //            {
            //                cases_loads.Add(case_loads);
            //                case_repeats--;
            //            }
            //            index = 0;
            //            case_loads = new string[3];
            //        }
            //        continue;
            //    }
            //    if (one_case == "START")
            //    {
            //        //Line of repeats
            //        one_case = "";
            //        case_repeats = int.Parse(line);
            //        continue;
            //    }
            //    one_case += line + "\n";
            //}

            //one_case = "";
            //case_repeats = 0;
            //foreach (var line in all_boundry_conditions_txt)
            //{
            //    if (line == "END")
            //    {
            //        break;
            //    }
            //    if (string.IsNullOrEmpty(line))
            //    {
            //        while (case_repeats > 0)
            //        {
            //            cases_boundry_conditions.Add(one_case);
            //            case_repeats--;
            //        }
            //        one_case = "";
            //        continue;
            //    }
            //    if (one_case == "START")
            //    {
            //        //Line of repeats
            //        one_case = "";
            //        case_repeats = int.Parse(line);
            //        continue;
            //    }
            //    one_case += line + "\n";
            //}

            //one_case = "";
            //case_repeats = 0;
            //foreach (var line in all_constants_txt)
            //{
            //    if (line == "END")
            //    {
            //        break;
            //    }
            //    if (string.IsNullOrEmpty(line))
            //    {
            //        while (case_repeats > 0)
            //        {
            //            cases_constants.Add(one_case);
            //            case_repeats--;
            //        }
            //        one_case = "";
            //        continue;
            //    }
            //    if (one_case == "START")
            //    {
            //        //Line of repeats
            //        one_case = "";
            //        case_repeats = int.Parse(line);
            //        continue;
            //    }
            //    one_case += line + "\n";
            //}
            //#endregion

            ////Solve Cases
            //for (int i = 0; i < cases_nodes.Count; i++)
            //{
            //    BoundryCondition = cases_boundry_conditions[i];
            //    var materials = GetMaterialValues(cases_materials[i]);
            //    Nodes = GetNodeValues(cases_nodes[i]);
            //    var elems = GetElementValues(cases_elements[i]);
            //    Members = GetMemberValues(cases_members[i]);
            //    GetLoadCasesValues(cases_loads[i]);

            //    //Solve a chunk & Save Output
            //    Solve(true, materials, elems);
            //    //Compare with Matlab
            //}
        }
        private void MakeResults()
        {
            StringBuilder elementBuilder = new StringBuilder();

            //string[] file = new string[1];

            foreach (var member in Members)
            {
                elementBuilder.Append(member.MinEigenValue + Environment.NewLine);

            }
            lamdaoutrichTextBox.Text = elementBuilder.ToString();

            //Chart1.series["series1"].points.databindxy(member.a, Member.eigenvalue);

        }

        //private void MakeGraph()
        //{
        //    string[] linesOfM;
        //    StringBuilder elementBuilder1 = new StringBuilder();
        //    linesOfM = txt_listOfM.Text.Split('\n');

        //    foreach (var m in linesOfM)
        //    {

        //        foreach (var member in Members)
        //        {


        //        }
        //    }

        //}


        protected void RefreshMainInputs()
        {
            labelOutMaterial.Text = @"To be Computed..";
            labelOutB.Text = @"To be Computed..";
            labelOutH.Text = @"To be Computed..";
            labelOutD.Text = @"To be Computed..";
            labelOutR.Text = @"To be Computed..";
            labelOutT.Text = @"To be Computed..";
            labelOutW.Text = @"To be Computed..";
            labelOutS.Text = @"To be Computed..";
            labelOutBC.Text = @"To be Computed..";
            labelOutP.Text = @"To be Computed..";
            labelOutMx.Text = @"To be Computed..";
            labelOutMy.Text = @"To be Computed..";

            moutrichTextBox.Text = "";
            aoutrichTextBox.Text = "";
        }

        private void Solve(List<Material> materials = null, List<Element> elems = null)
        {
            //Validation
            if (!ValidateInputs(materials, elems))
            {
                var msg = @"Inputs are not valid!";
                MessageBox.Show(msg);
                return;
            }

            MessageBox.Show(@"Solving started, note that this can take a couple of minutes, then you will be notified when it is done.");
            //var cases = GetCases();

            if (!IsParametric) //if not clicked on perform parametric study
            {
                BoundryCondition = bcComboBox.SelectedItem.ToString();
                materials = GetMaterialValues();
                Nodes = GetNodeValues();
                elems = GetElementValues();
                Members = GetMemberValues();
                GetLoadCasesValues();

            }


            SolveMaterial(materials);
            //SolveNodeStress(elems, materials);
            //SolveNodeYield(elems, materials[0]);

            Design designObject = new Design();
            SolveElement(elems, materials[0]);

            // designObject is now Filled

            SolveSection(elems);
            SolveNodeStress(elems, materials);
            SolveNodeYield(elems, materials[0]);
            SolveMembers();
            SolveMatricesForAllElements(elems, materials[0]);


            foreach (var member in Members)
            {

                TransformStiffnessMatrix(elems, member);
                DivideStiffnessMatrix(elems, member);
                member.FinalGlobalKe = AssembleStiffnessMatrix_e(elems, member);
                member.FinalGlobalKg = AssembleStiffnessMatrix_g(elems, member);
                SolveEigenValueProblem(elems, member);



            }

            if (!IsParametric)
            {
                labelOutMaterial.Text = txt_materialsData.Text;

                labelOutB.Text = bTextBox.Text;
                labelOutH.Text = hTextBox.Text;
                labelOutD.Text = dTextBox.Text;
                labelOutR.Text = rTextBox.Text;
                labelOutT.Text = tTextBox.Text;
                labelOutW.Text = wTextBox.Text;
                labelOutS.Text = sTextBox.Text;
                labelOutBC.Text = bcComboBox.SelectedItem.ToString();
                labelOutP.Text = pTextBox.Text;
                labelOutMx.Text = MxTextBox.Text;
                labelOutMy.Text = MytextBox.Text;

                aoutrichTextBox.Text = txt_listOfA.Text;
                moutrichTextBox.Items.AddRange(txt_listOfM.Lines);

                MakeResults();
                //lamdaoutrichTextBox.Text = member.mineigenvalues.Text;
            }

            // SolveDesignEquations();




            if (IsParametric)
            {
                PrintSummary();
                //Process.Start(Path.Combine(outputPathName, "summary.csv"));
            }
            else
            {
                //Print output
                PrintOutput(elems);

                DrawSignatureCurve();
                //Select analysis tab
                tabControlMain.SelectedTab = tabPageAnalysis;
                tabControlMain.BringSelectedTabToView();
            }
        }

        private void DrawSignatureCurve()
        {
            //Draw signature curve
            var xAxis = txt_listOfA.Lines;
            var yAxis = lamdaoutrichTextBox.Lines;
            if (xAxis.Length < 1 || yAxis.Length < 1 || moutrichTextBox.Items.Count < 1)
            {
                return;
            }
            if (moutrichTextBox.SelectedIndex < 0)
            {
                moutrichTextBox.SelectedIndex = 0;
            }
            string selectedM = moutrichTextBox.SelectedItem.ToString();

            var pointsbindingList = new BindingList<SignatureCurvePoint>();
            //BindingList<SignatureCurvePoint> dataSource = new BindingList<SignatureCurvePoint>();
            var len = Math.Min(xAxis.Length, yAxis.Length);
            for (int i = 0; i < len; i++)
            {
                pointsbindingList.Add(new SignatureCurvePoint()
                {
                    A = int.Parse(xAxis[i]),
                    M = int.Parse(selectedM),
                    MinEigenValue = Math.Round(double.Parse(yAxis[i]), 2)
                });
            }

            signatureCurvePointBindingSource.DataSource = pointsbindingList;
        }

        private bool ValidateInputs(List<Material> materials = null, List<Element> elems = null)
        {
            if (!IsParametric) //if not clicked on perform parametric study
            {
                return bcComboBox.SelectedItem != null
                       &&
                       txt_materialsData.TextLength > 0
                       &&
                       //Nodes & Elements
                       txt_nodeData.TextLength > 0
                       &&
                       txt_elementsData.TextLength > 0
                       &&
                       //Member Data
                       txt_listOfA.TextLength > 0
                       &&
                       txt_listOfM.TextLength > 0
                       &&
                       nTextBox.TextLength > 0
                       &&
                       LtextBox.TextLength > 0
                       &&
                       //Load Cases
                       pTextBox.TextLength > 0
                       &&
                       MxTextBox.TextLength > 0
                       &&
                       MytextBox.TextLength > 0
                    ;

            }


            return parmaterialTextBox.TextLength > 0
                &&
                //Nodes & Elements
                txt_nodeData.TextLength > 0
                &&
                txt_elementsData.TextLength > 0
                &&
                //Member Data
                ParametricAtextbox.TextLength > 0
                &&
                ParametricMtextbox.TextLength > 0
                &&
                ParametricNtextbox.TextLength > 0
                &&
                ParametricLtextbox.TextLength > 0
                ;
        }

        private void btnpaint_Click(object sender, EventArgs e)
        {
            pictureBox1.Refresh();

        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {


            // Create one Pen objects with black,

            Pen blackPen = new Pen(Color.Black, 4);

            foreach (var element in paintelements)
            {
                // Draw line using float coordinates
                float x1 = (float)(element.node1.xcood + 246.5);
                float x2 = (float)(element.node2.xcood + 246.5);
                float y1 = (float)(element.node1.ycood + 210);
                float y2 = (float)(element.node2.ycood + 210);

                // Draw line using float coordinates
                //float x1 = element.node1.xcood, y1 = element.node1.ycood;
                //float x2 = element.node2.xcood, y2 = element.node2.ycood;
                //e.Graphics.DrawLine(blackPen, x1, y1, x2, y2);

                // Draw line using Point structure
                //Point p1 = new Point(x1, y1);
                //Point p2 = new Point(x2, y2);
                //e.Graphics.DrawLine(blackPen, p1, p2);

                //Draw line using PointF structures
                PointF ptf1 = new PointF(x1, y1);
                PointF ptf2 = new PointF(x2, y2);
                e.Graphics.DrawLine(blackPen, ptf1, ptf2);

                // Draw line using integer coordinates
                //int X1 = element.node1.xcood, Y1 = element.node1.ycood, X2 = element.node2.xcood, Y2 = element.node2.ycood;
                //e.Graphics.DrawLine(blackPen, X1, Y1, X2, Y2);
            }

            //Dispose of objects
            blackPen.Dispose();
        }

        private void btnsectionmesh_Click(object sender, EventArgs e)
        {
            //Validate
            if (sectionComboBox.SelectedIndex < 0
                || string.IsNullOrEmpty(bTextBox.Text)
                || string.IsNullOrEmpty(hTextBox.Text)
                || string.IsNullOrEmpty(dTextBox.Text)
                || string.IsNullOrEmpty(rTextBox.Text)
                || string.IsNullOrEmpty(tTextBox.Text)
                || string.IsNullOrEmpty(wTextBox.Text)
                || string.IsNullOrEmpty(sTextBox.Text)
                )
            {
                MessageBox.Show(@"You must select section & fill all needed inputs [b, d, r, s, h, t & w].");
                return;
            }

            if ((string)sectionComboBox.SelectedItem == "General") return;
            Getsectionvalues();
            MESHSECTION();
            txt_nodeData.Text = section.sectionfinaltextnode;
            txt_elementsData.Text = section.sectionfinaltextelement;

            DialogResult dialog = MessageBox.Show(@"Nodes generated! Please proceed to the next stage.", "Ok", MessageBoxButtons.OK);
            if (dialog == DialogResult.OK)
            {
                groupBarDefine.SelectedItem = 2;
            }

            DrawSection();
        }

        private void ValidateCSVNumberFields(object sender, EventArgs e)
        {
            Control control = sender as TextBox;
            if (control == null)
            {
                control = sender as RichTextBox;
            }

            if (control == null)
            {
                return;
            }

            if (Regex.IsMatch(control.Text, "^([0-9,.]*)$")) return;

            MessageBox.Show(@"Input is invalid! you can only enter numbers and commas.", @"Ok", MessageBoxButtons.OK);
            control.Text = Regex.Replace(control.Text, "(?![0-9,.]).", "");
        }

        private void RefreshDrawing(object sender, EventArgs e)
        {
            DrawSection();
        }

        private void DrawSection()
        {
            try
            {
                paintelements.Clear();
                Nodes = GetNodeValues();
                GetElementValues();
                pictureBox1.Refresh();
            }
            catch (Exception e)
            {
                //
            }
        }

        private void moutrichTextBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DrawSignatureCurve();
        }
    }
}
