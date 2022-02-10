namespace EigenValue_Problem.Models
{
    public class LoadCases
    {
        public double ID;
        public double Mx;
        public double My;
        public double P;
        public double Mxy;
        public double Myy;
        public double Py;

        public LoadCases(double Mx, double My, double P)   //inputs in program 
        {
            this.Mx = Mx;
            this.My = My;
            this.P = P;
        }
    }
}
