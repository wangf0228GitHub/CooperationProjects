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
    public partial class PIDSettingForm : Form
    {
        int type;
        WFNetLib.PID.positionPID pid;
        _R2 R2;
        public PIDSettingForm(int _type, WFNetLib.PID.positionPID _pid,_R2 r2)
        {
            InitializeComponent();
            type = _type;
            pid = _pid;
            R2 = r2;
        }

        private void PIDSettingForm_Load(object sender, EventArgs e)
        {
            if(type==1)
            {
                label1.Text = "扭振参数设定";
                label2.Text = "扭振目标值";
                label3.Text = "毫米";
                this.Text = "扭振参数设定";
            }
            else
            {
                label1.Text = "扭矩参数设定";
                label2.Text = "扭矩目标值";
                label3.Text = "牛米";
                this.Text = "扭矩参数设定";
            }
            textBox1.Text = pid.sp.ToString();
            textBox2.Text = pid.pgain.ToString();
            textBox3.Text = pid.igain.ToString();
            textBox4.Text = pid.dgain.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(!double.TryParse(textBox1.Text,out pid.sp))
            {
                MessageBox.Show("目标值输入有误");
                return;          
            }
            if (!double.TryParse(textBox2.Text, out pid.pgain))
            {
                MessageBox.Show("P值输入有误");
                return;
            }
            if (!double.TryParse(textBox3.Text, out pid.igain))
            {
                MessageBox.Show("I值输入有误");
                return;
            }
            if (!double.TryParse(textBox4.Text, out pid.dgain))
            {
                MessageBox.Show("D值输入有误");
                return;
            }
            if (type==1)
            {
                iniFileOP.Write("System Setting", "NiuZhen_sp", pid.sp.ToString());
                iniFileOP.Write(R2.ToString(), R2.ToString() + "_pgain", pid.pgain.ToString());
                iniFileOP.Write(R2.ToString(), R2.ToString() + "_igain", pid.igain.ToString());
                iniFileOP.Write(R2.ToString(), R2.ToString() + "_dgain", pid.dgain.ToString());
            }
            else if (type == 2)
            {
                iniFileOP.Write("System Setting", "NiuJu_sp", pid.sp.ToString());
                iniFileOP.Write("System Setting", "NiuJu_pgain", pid.pgain.ToString());
                iniFileOP.Write("System Setting", "NiuJu_igain", pid.igain.ToString());
                iniFileOP.Write("System Setting", "NiuJu_dgain", pid.dgain.ToString());
            }
            DialogResult = DialogResult.OK;
        }
    }
}
