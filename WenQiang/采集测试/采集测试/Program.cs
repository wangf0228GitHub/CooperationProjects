using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CMOSTestLib;

namespace 采集测试
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
            SerialFunc.SerialPortName = "COM2";
            Form1 f = new Form1();
            if(!f.IsDisposed)
                Application.Run(f);
        }
    }
}
