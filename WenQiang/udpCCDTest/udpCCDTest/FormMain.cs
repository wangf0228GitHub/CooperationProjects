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
        public FormMain()
        {
            InitializeComponent();
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
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            FormManual f = new FormManual();
            f.ShowDialog();
        }

        private void toolStripButton2_ButtonClick(object sender, EventArgs e)
        {

        }
    }
}
