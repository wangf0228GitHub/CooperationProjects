﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace udpCCDTest
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            FormMain f = new FormMain();
            if (!f.IsDisposed)
                Application.Run(f);
        }
    }
}
