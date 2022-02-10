using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using EigenValue_Problem.Models;
using MathNet.Numerics.LinearAlgebra;
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
        Dictionary<Member,double[,]> globalStiff = new Dictionary<Member,double[,]>();
        private List<Node> Nodes;
        private List<Member> Members;
        private string BoundryCondition;
        

        //For Testing
        Vector<double> EigenValues;

        public NewForm()
        {
            InitializeComponent();

            this.ShowSplash();
            this.splashPanel1.SuspendLayout();

            //Material concrete25 = new Material(4000, 4000, 0.2, 0.2);
            //Material concrete30 = new Material(5000, 4000, 0.3, 0.3);
            //Material Custom = new Material(0, 0, 0, 0);
            //List<Material> materials = new List<Material> { concrete25, concrete30 };

            //foreach (Material material in materials)
            //{
            //    //materialComboBox.Items.Add(material.ex);
            //}

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

            //SolveDesignEquations();

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

            //SolveDesignEquations();

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
    }
}
