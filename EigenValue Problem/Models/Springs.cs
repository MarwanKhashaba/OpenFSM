namespace EigenValue_Problem.Models
{
    public class Springs
    {
        public double ID;
        public double node;
        public double Dof;
        public double SpringStiffness;

        public Springs(double node, double Dof, double SpringStiffness)  //inputs in program 
        {
            this.node = node;
            this.Dof = Dof;
            this.SpringStiffness = SpringStiffness;
        }
    }
}
