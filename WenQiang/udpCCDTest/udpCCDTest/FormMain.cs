using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace udpCCDTest
{
    public partial class FormMain : Form
    {
        public delegate void _ShowText(string s);
        public void __ShowText(string s)
        {
            textBox1.AppendText(s + "\r\n");
        }
        public static _ShowText ShowText;
        public _tcpCCS tcpCCS; 
        public FormMain()
        {
            InitializeComponent();
            ShowText = new _ShowText(__ShowText);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            FormParam f = new FormParam();
            f.ShowDialog();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            SystemParam.InitSystemParam();
            DeviceState.fMain = this;
            tcpCCS = new _tcpCCS();
            tcpCCS.Connect();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            FormManual f = new FormManual();
            f.ShowDialog();
        }

        private void toolStripButton2_ButtonClick(object sender, EventArgs e)
        {

        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            tcpCCS.LightSet(550, 0.5);
        }
    }
}
