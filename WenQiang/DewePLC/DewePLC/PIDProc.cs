using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DewePLC
{
    public partial class Form1 : Form
    {
        WFNetLib.PID.PID NiuZhenPID = new WFNetLib.PID.PID();
        WFNetLib.PID.PID NiuJuPID = new WFNetLib.PID.PID();     
        private void label1_Click(object sender, EventArgs e)
        {
            PIDSettingForm f = new PIDSettingForm(1, NiuZhenPID);
            if(f.ShowDialog()==DialogResult.OK)
            {
                InitSystemParam();
            }
        }
        private void label2_Click(object sender, EventArgs e)
        {
            PIDSettingForm f = new PIDSettingForm(2, NiuJuPID);
            if (f.ShowDialog() == DialogResult.OK)
            {
                InitSystemParam();
            }
        }
    }
}
