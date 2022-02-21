using System;

namespace EigenValue_Problem.Models
{
    public class Node
    {
        public int ID;
        public double xcood;
        public double ycood;
        public double Stress;
        public double StressAtMxYield;
        public double StressAtMyYield;


        public Node()  //inputs in program 
        {
            ID= 0;
            xcood = 0.0;
            ycood = 0.0;
        }
        public override string ToString()
        {
            return "" + Stress + '\t' + StressAtMxYield + '\t' +
                   StressAtMyYield  + Environment.NewLine;
        }

    }
}
