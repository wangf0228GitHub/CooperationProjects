using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WFNetLib;

namespace DewePLC
{
    public enum _R2
    {
        R0=0,
        R160=1,
        R182,
        R206,
        R219,
        R246,
        R266,
        R318,
        R348,
        R368,
        R383
    }
    public partial class Form1 : Form
    {
        WFNetLib.PID.positionPID NiuZhenPID = new WFNetLib.PID.positionPID();
        WFNetLib.PID.positionPID NiuJuPID = new WFNetLib.PID.positionPID();     
        private void label1_Click(object sender, EventArgs e)
        {
            PIDSettingForm f = new PIDSettingForm(1, NiuZhenPID,R2);
            if(f.ShowDialog()==DialogResult.OK)
            {
                NiuZhenPID.pgain = double.Parse(iniFileOP.Read(R2.ToString(), R2.ToString() + "_pgain"));
                NiuZhenPID.igain = double.Parse(iniFileOP.Read(R2.ToString(), R2.ToString() + "_igain"));
                NiuZhenPID.dgain = double.Parse(iniFileOP.Read(R2.ToString(), R2.ToString() + "_dgain"));
                listView1.Items[3].SubItems[1].Text = NiuZhenPID.pgain.ToString();
                listView1.Items[4].SubItems[1].Text = NiuZhenPID.igain.ToString();
                listView1.Items[5].SubItems[1].Text = NiuZhenPID.dgain.ToString();
                listView1.Columns[1].Width = listView1.ClientSize.Width - listView1.Columns[0].Width;// - listView1.Columns[1].Width;
            }
        }
        private void label2_Click(object sender, EventArgs e)
        {
            PIDSettingForm f = new PIDSettingForm(2, NiuJuPID,R2);
            if (f.ShowDialog() == DialogResult.OK)
            {
                NiuJuPID.pgain = double.Parse(iniFileOP.Read("System Setting", "NiuJu_pgain"));
                NiuJuPID.igain = double.Parse(iniFileOP.Read("System Setting", "NiuJu_igain"));
                NiuJuPID.dgain = double.Parse(iniFileOP.Read("System Setting", "NiuJu_dgain"));
                listView2.Items[3].SubItems[1].Text = NiuJuPID.pgain.ToString();
                listView2.Items[4].SubItems[1].Text = NiuJuPID.igain.ToString();
                listView2.Items[5].SubItems[1].Text = NiuJuPID.dgain.ToString();
                listView2.Columns[1].Width = listView2.ClientSize.Width - listView2.Columns[0].Width;
            }
        }
    }
}
