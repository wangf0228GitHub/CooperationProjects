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
    public partial class ResultCurveForm : Form
    {
        public ResultCurveForm()
        {
            InitializeComponent();
        }

        private void ResultCurveForm_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < 100; i++)
            {
                chart.Series[0].Points.AddXY(i * 10.0, i * 6.0);
            }
        }
    }
}
