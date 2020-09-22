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
    public partial class WorkSetForm : Form
    {
        public WorkSetForm()
        {
            InitializeComponent();
        }

        private void WorkSetForm_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = int.Parse(iniFileOP.Read("System Setting", "R2"));
            numericUpDown1.Value= decimal.Parse(iniFileOP.Read("System Setting", "NiuJu_sp"));
            numericUpDown2.Value = decimal.Parse(iniFileOP.Read("System Setting", "NiuZhen_sp"));
            numericUpDown3.Value = decimal.Parse(iniFileOP.Read("System Setting", "NiuJu_Deadband"));
            numericUpDown4.Value = decimal.Parse(iniFileOP.Read("System Setting", "NiuZhen_Deadband"));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            iniFileOP.Write("System Setting", "NiuZhen_sp", numericUpDown2.Value.ToString());
            iniFileOP.Write("System Setting", "NiuJu_sp", numericUpDown1.Value.ToString());
            iniFileOP.Write("System Setting", "R2", comboBox1.SelectedIndex.ToString());
            iniFileOP.Write("System Setting", "NiuZhen_Deadband", numericUpDown4.Value.ToString());
            iniFileOP.Write("System Setting", "NiuJu_Deadband", numericUpDown3.Value.ToString());            
            this.DialogResult = DialogResult.OK;
        }
    }
}
