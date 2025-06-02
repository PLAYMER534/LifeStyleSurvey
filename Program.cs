using System;
using System.Windows.Forms;
using Assignment_LifeStyleSurvey; // Make sure this matches your Form1 namespace

namespace Assignment__LifeStyleSurvey_
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}

