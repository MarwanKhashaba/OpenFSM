using System;
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
            //OldForm form = new OldForm();
            var form = new NewForm();
            Application.Run(form);
            
        }
    }
}