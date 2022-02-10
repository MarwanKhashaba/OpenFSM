namespace EigenValue_Problem.Models
{
    public class Constraints
    {
        public double ID;
        public double masternode;
        public double slavenode;
        public double Dof;
        public double coeff;

        public Constraints(double masternode, double slavenode, double Dof, double coeff)  //inputs in program 
        {
            this.masternode = masternode;
            this.slavenode = slavenode;
            this.Dof = Dof;
            this.coeff = coeff;
        }
    }
}
