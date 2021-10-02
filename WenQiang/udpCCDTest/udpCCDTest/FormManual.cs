using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WFNetLib;

namespace udpCCDTest
{
    public partial class FormManual : Form
    {
        public FormManual()
        {
            InitializeComponent();
        }
        WaitingProc wp;
        int nCount, Tex;
        private void toolStripButton1_Click(object sender, EventArgs e)
        {      
            double d;
            if(!(int.TryParse(tbCount.Text,out nCount)&& (double.TryParse(tbTex.Text, out d))))
            {
                MessageBox.Show("参数设置有误");
                return;
            }
            Tex = (int)(d / SystemParam.Ts);
            UDPProc.CollectImage(this, Tex, nCount);
        }        
        private void FormManual_Load(object sender, EventArgs e)
        {
            trackBar1.Minimum = SystemParam.NTmin;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            tbTex.Text = (trackBar1.Value * SystemParam.Ts).ToString("F3");
        }
    }
}
