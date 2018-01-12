using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace 流量计算
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double p = (double)numericUpDown1.Value;
            double Q;
            if (p < 0.004)
            {
                Q = 17500 * p;
            }
            else if (p < 0.138)
            {
                Q = 60.7 + 2100 * p;
            }
            else if (p < 3.5)
            {
                Q = 306.5 + 312.5 * p;
            }
            else if (p < 7.1)
            {
                Q = 525 + 250 * p;
            }
            else if (p < 11.4)
            {
                Q = 1144.4 + 168.8 * p;
            }
            else if (p < 12.5)
            {
                Q = 1276.92 + 153.85 * p;
            }
            else if (p < 16)
            {
                Q = 1772 + 114.5 * p;
            }
            else if (p < 20)
            {
                Q = 2000 + 100 * p;
            }
            else if (p < 26)
            {
                Q = 2333.5 + 83.33 * p;
            }
            else// if(p<)
            {
                Q = 2613.21 + 72.57 * p;
            }
            textBox1.Text = Q.ToString("F2");
        }
    }
}
