using System.Collections.Generic;

namespace EigenValue_Problem
{
    public class Section
    {
        public double ID;
        public double NoOfNodes;
        public double NoOfElememts;
        public double sigmadeltaxdeltay12; //new
        public double sigmadeltaxdeltax12; //new
        public double sigmadeltaydeltay12; //new
        public double sigmaymeanminusycg; //new needs section calculations first
        public double sigmaxmeanminusxcg; //new needs section calculations first
        public double sigmaxmeanminusxcgymeanminusycg; //new needs section calculations first
        public double sigmaymeanminusycg2; //new needs section calculations first
        public double sigmaxmeanminusxcg2; //new needs section calculations first
        public double Ix;
        public double Iy;
        public double Ixy;
        public double sigmaIx;
        public double sigmaIy;
        public double sigmaIxy;
        public double I1;
        public double I2;
        public double A;
        public double Ax;
        public double Axx;
        public double Ay;
        public double Ayy;
        public double Axy;
        public double Sx;  // get an equation for this
        public double Zx;  // get an equation for this
        public double My; //another way to get Mxyield
        public double Mp; // needed for design equations
        public double Xcg;
        public double Ycg;
        public double PolarTheta;
        public double XShearCenter;
        public double YShearCenter;
        public double J;
        public double Cw;
        public double Wn;
        public double B1;
        public double B2;
        public double Bx;
        public double By;
        // section to coordinates 
        public double b; 
        public double d;
        public double h;
        public double r;
        public double t;
        public double w;    //for hat only (stiffener)
        public double s;     //for hat only (stiffener)
        public string sectionfinaltextnode;
        public string sectionfinaltextelement;

        public double J1;   //for FILLET
        public double J2;   //for FILLET
        public double J3;   //for FILLET
        public List<Node> GeneratedNodes = new List<Node>();

       public double node1x          ;
       public double     node1y      ;
       public double node2x          ;
       public double     node2y      ;
       public double node3x          ;
       public double     node3y      ;
       public double node4x          ;
       public double     node4y      ;
       public double node5x          ;
       public double     node5y      ;
       public double node6x          ;
       public double     node6y      ;
       public double node7x          ;
       public double     node7y      ;
       public double node8x          ;
       public double     node8y      ;
       public double node9x          ;
       public double     node9y      ;
       public double node10x         ;
       public double     node10y     ;
       public double node11x         ;
       public double     node11y     ;
       public double node12x         ;
       public double     node12y     ;
       public double node13x         ;
       public double     node13y     ;
       public double node14x         ;
       public double     node14y     ;
       public double node15x         ;
       public double     node15y     ;
       public double node16x         ;
       public double     node16y     ;
       public double node17x         ;
       public double     node17y     ;
       public double node18x         ;
       public double     node18y     ;
       public double node19x         ;
       public double     node19y     ;
       public double node20x         ;
       public double     node20y     ;
       public double node21x         ;
       public double     node21y     ;
       public double node22x         ;
       public double     node22y     ;
       public double node23x         ;
       public double     node23y     ;
       public double node24x         ;
       public double     node24y     ;
       public double node25x         ;
       public double     node25y     ;
       public double node26x         ;
       public double     node26y     ;
       public double node27x         ;
       public double     node27y     ;
       public double node28x         ;
       public double     node28y     ;
       public double node29x         ;
       public double     node29y     ;
       public double node30x         ;
       public double     node30y     ;
       public double node31x         ;
        public double node31y;


        public override string ToString()
        {
            return "" + Xcg + '\t' + Ycg + '\t' +
                      A + '\t' + Ax + '\t' +
                      Axx + '\t' + Ay + '\t' + Ayy + '\t'+
                      Axy + '\t' + sigmaIx + '\t' + sigmaIy + '\t' + sigmaIxy + '\t' 
                      + sigmadeltaxdeltay12 + '\t' + sigmadeltaxdeltax12 + '\t' + sigmadeltaydeltay12 + '\t' 
                      + sigmaymeanminusycg + '\t' + sigmaxmeanminusxcg + '\t' 
                      + sigmaxmeanminusxcgymeanminusycg + '\t' + sigmaymeanminusycg2 + '\t' 
                      + sigmaxmeanminusxcg2 + '\t' +
                      + Ix + '\t'+ Iy + '\t' + Ixy + '\t' +
                      + I1 + '\t' +
                      I2 + '\t' + PolarTheta + '\t' + J;
        }
    }
}