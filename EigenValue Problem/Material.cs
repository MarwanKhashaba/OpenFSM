namespace EigenValue_Problem
{
    public class Material
    {
        public int ID;
        public double Ex;
        public double Ey;
        public double Vx;
        public double Vy;
        public double Gxy;
        public double e1;
        public double e2;
        public double fy;

        public Material() //inputs in program 
        {
            this.ID = 0;
            this.Ex = 0.0;
            this.Ey = 0.0;
            this.Vx = 0.0;
            this.Vy = 0.0;
            this.fy = 0.0;
        }

    }
}