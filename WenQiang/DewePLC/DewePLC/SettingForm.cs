using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WFNetLib;

namespace DewePLC
{
    public partial class SettingForm : Form
    {
        public SettingForm()
        {
            InitializeComponent();
        }
        _R2 R2;
        private void SettingForm_Load(object sender, EventArgs e)
        {
            nKp.Text= iniFileOP.Read("System Setting", "NiuJu_pgain");
            nKi.Text = iniFileOP.Read("System Setting", "NiuJu_igain");
            nKd.Text = iniFileOP.Read("System Setting", "NiuJu_dgain");

            njK.Text = iniFileOP.Read("System Setting", "deweNiuJu_k");
            njB.Text = iniFileOP.Read("System Setting", "deweNiuJu_b");

            nzK.Text = iniFileOP.Read("System Setting", "deweNiuZhen_k");
            nzB.Text = iniFileOP.Read("System Setting", "deweNiuZhen_b");

            R2Combox.SelectedIndex = 0;
            R2 = _R2.R160;
            fKp.Text = iniFileOP.Read(R2.ToString(), R2.ToString() + "_pgain");
            fKi.Text = iniFileOP.Read(R2.ToString(), R2.ToString() + "_igain");
            fKd.Text = iniFileOP.Read(R2.ToString(), R2.ToString() + "_dgain");
            AllTime1.Text= iniFileOP.Read(R2.ToString(), R2.ToString() + "_AllTime1");
            AllTime2.Text = iniFileOP.Read(R2.ToString(), R2.ToString() + "_AllTime2");
            OpenTime.Text = iniFileOP.Read(R2.ToString(), R2.ToString() + "_OpenTime");
        }

        private void R2Combox_SelectedIndexChanged(object sender, EventArgs e)
        {
            R2 = (_R2)R2Combox.SelectedIndex;
            fKp.Text = iniFileOP.Read(R2.ToString(), R2.ToString() + "_pgain");
            fKi.Text = iniFileOP.Read(R2.ToString(), R2.ToString() + "_igain");
            fKd.Text = iniFileOP.Read(R2.ToString(), R2.ToString() + "_dgain");
            AllTime1.Text = iniFileOP.Read(R2.ToString(), R2.ToString() + "_AllTime1");
            AllTime2.Text = iniFileOP.Read(R2.ToString(), R2.ToString() + "_AllTime2");
            OpenTime.Text = iniFileOP.Read(R2.ToString(), R2.ToString() + "_OpenTime");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double x;
            if (!double.TryParse(nKp.Text, out x))
            {
                MessageBox.Show("NiuJu_pgain输入有误");
                return;
            }
            iniFileOP.Write("System Setting", "NiuJu_pgain", x.ToString());
            if (!double.TryParse(nKi.Text, out x))
            {
                MessageBox.Show("NiuJu_igain输入有误");
                return;
            }
            iniFileOP.Write("System Setting", "NiuJu_igain", x.ToString());
            if (!double.TryParse(nKd.Text, out x))
            {
                MessageBox.Show("NiuJu_dgain输入有误");
                return;
            }
            iniFileOP.Write("System Setting", "NiuJu_dgain", x.ToString());            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            double x;
            if (!double.TryParse(njK.Text, out x))
            {
                MessageBox.Show("deweNiuJu_k输入有误");
                return;
            }
            iniFileOP.Write("System Setting", "deweNiuJu_k", x.ToString());
            if (!double.TryParse(njB.Text, out x))
            {
                MessageBox.Show("deweNiuJu_b输入有误");
                return;
            }
            iniFileOP.Write("System Setting", "deweNiuJu_b", x.ToString());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            double x;
            if (!double.TryParse(nzK.Text, out x))
            {
                MessageBox.Show("deweNiuZhen_k输入有误");
                return;
            }
            iniFileOP.Write("System Setting", "deweNiuZhen_k", x.ToString());
            if (!double.TryParse(nzB.Text, out x))
            {
                MessageBox.Show("deweNiuZhen_b输入有误");
                return;
            }
            iniFileOP.Write("System Setting", "deweNiuZhen_b", x.ToString());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            double x;
            int t;
            if (!double.TryParse(fKp.Text, out x))
            {
                MessageBox.Show(R2.ToString() + "_pgain输入有误");
                return;
            }
            iniFileOP.Write(R2.ToString(), R2.ToString() + "_pgain", x.ToString());
            if (!double.TryParse(fKi.Text, out x))
            {
                MessageBox.Show(R2.ToString() + "_igain输入有误");
                return;
            }
            iniFileOP.Write(R2.ToString(), R2.ToString() + "_igain", x.ToString());
            if (!double.TryParse(fKd.Text, out x))
            {
                MessageBox.Show(R2.ToString() + "_dgain输入有误");
                return;
            }
            iniFileOP.Write(R2.ToString(), R2.ToString() + "_dgain", x.ToString());

            if (!int.TryParse(AllTime1.Text, out t))
            {
                MessageBox.Show(R2.ToString() + "_AllTime1输入有误");
                return;
            }
            iniFileOP.Write(R2.ToString(), R2.ToString() + "_AllTime1", t.ToString());

            if (!int.TryParse(AllTime2.Text, out t))
            {
                MessageBox.Show(R2.ToString() + "_AllTime2输入有误");
                return;
            }
            iniFileOP.Write(R2.ToString(), R2.ToString() + "_AllTime2", t.ToString());

            if (!int.TryParse(OpenTime.Text, out t))
            {
                MessageBox.Show(R2.ToString() + "_OpenTime输入有误");
                return;
            }
            iniFileOP.Write(R2.ToString(), R2.ToString() + "_OpenTime", t.ToString());
        }
    }
}
