﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace LicenseParser
{
    static class Program
    {
        public static MainMenu obj;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            obj = new MainMenu();
            Application.Run(obj);
        }
    }
}
