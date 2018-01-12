using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CMOSTestLib;

namespace 采集测试
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }
        public Form2(List<double> _yy, List<double> _dd, uint eStart, uint eStep)
        {
            InitializeComponent();
            yy = _yy;
            dd = _dd;
            toolStripTextBox1.Text = eStart.ToString();
            toolStripTextBox2.Text = eStep.ToString();
            m_eStart = eStart;
            m_eStep = eStep;
            toolStripTextBox3.Text = toolStripTextBox1.Text;
            toolStripTextBox4.Text = toolStripTextBox2.Text;
        }
        public double k=0, b=0;
        List<double> yy;
        List<double> dd;
        private void Form2_Load(object sender, EventArgs e)
        {
            double t,t1;
            for (int i = 0; i < SystemParam.ExposureTest_Ns; i++)
            {
                t = (m_eStart + i * m_eStep) * SystemParam.Ts;
                chart1.Series[0].Points.AddXY(t,yy[i]);
                if (k != 0)
                {
                    if (i < 60)
                        chart1.Series[1].Points.AddXY(t, k * t + b);
                    else
                    {
                        t1 = (int)(((m_eStart + 59 * m_eStep) * SystemParam.Ts) / 1000);
                        chart1.Series[1].Points.AddXY(t, k * t1 + b);
                    }
                }
            }
            m_bClose = false;
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_bClose)
                return;
            e.Cancel = true;
        }
        public uint m_eStart;
        public uint m_eStep;
        public bool m_bOK;
        public bool m_bClose;
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            try
            {
                m_eStart =uint.Parse(toolStripTextBox3.Text);
                m_eStep = uint.Parse(toolStripTextBox4.Text);
            }
            catch
            {
                MessageBox.Show("输入数据有误，请检查");
                return;
            }
            m_bOK = false;
            m_bClose = true;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            m_bOK = true;
            m_bClose = true;
            this.Close();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            chart1.SaveImage("e://2.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
        }
    }
}
