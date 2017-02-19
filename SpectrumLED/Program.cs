using System;
using System.Windows.Forms;

namespace SpectrumLED
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
            
            SpectrumLEDApplicationContext app = new SpectrumLEDApplicationContext();
            Application.ApplicationExit += app.OnApplicationExit;

            Application.Run(app);
        }
    }
}
