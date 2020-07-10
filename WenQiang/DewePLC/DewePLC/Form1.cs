using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WFNetLib;
using WFNetLib.Log;
using WFNetLib.PacketProc;
using WFNetLib.TCP;

namespace DewePLC
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        
        private delegate void ShowTextLog(string str, bool newSection);
        private void ShowTextLogProc(string str, bool newSection)
        {
            if (textBox1.Text.Length > 20000)
                textBox1.Clear();
            if (newSection)
                textBox1.AppendText("---------------------" + DateTime.Now.ToString("yy年MM月dd日 hh:mm:ss") + "-------------------\r\n");
            textBox1.AppendText(str + "\r\n");
            TextLog.AddTextLog(str, newSection);
        }
        public void ShowText(string str, bool newSection)
        {
            this.Invoke(new ShowTextLog(ShowTextLogProc), str, newSection);
        }
        public void ShowText(string str)
        {
            this.Invoke(new ShowTextLog(ShowTextLogProc), str, false);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitSystemParam();        

            //ConnectOPCServer();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                opcUaClient.Disconnect();
            }
            catch
            {

            }            
        }
        string opcUrl;
        string Node1Addr;
        string Node2Addr;
        void InitSystemParam()
        {
            iniFileOP.iniFilePath = System.Windows.Forms.Application.StartupPath + "\\Config.ini";

            NiuZhenPID.pidParam.sp= double.Parse(iniFileOP.Read("System Setting", "NiuZhen_sp"));
            NiuZhenPID.pidParam.pgain = double.Parse(iniFileOP.Read("System Setting", "NiuZhen_pgain"));
            NiuZhenPID.pidParam.igain = double.Parse(iniFileOP.Read("System Setting", "NiuZhen_igain"));
            NiuZhenPID.pidParam.dgain = double.Parse(iniFileOP.Read("System Setting", "NiuZhen_dgain"));

            NiuJuPID.pidParam.sp = double.Parse(iniFileOP.Read("System Setting", "NiuJu_sp"));
            NiuJuPID.pidParam.pgain = double.Parse(iniFileOP.Read("System Setting", "NiuJu_pgain"));
            NiuJuPID.pidParam.igain = double.Parse(iniFileOP.Read("System Setting", "NiuJu_igain"));
            NiuJuPID.pidParam.dgain = double.Parse(iniFileOP.Read("System Setting", "NiuJu_dgain"));
                
            opcUrl= iniFileOP.Read("System Run", "opcUrl");
            Node1Addr = iniFileOP.Read("System Run", "Node1Addr");
            Node2Addr = iniFileOP.Read("System Run", "Node2Addr");

            listView1.Items[0].SubItems[1].Text = NiuZhenPID.pidParam.sp.ToString();
            listView1.Items[2].SubItems[1].Text = NiuZhenPID.pidParam.pgain.ToString();
            listView1.Items[3].SubItems[1].Text = NiuZhenPID.pidParam.igain.ToString();
            listView1.Items[4].SubItems[1].Text = NiuZhenPID.pidParam.dgain.ToString();
            listView1.Columns[1].Width = listView1.ClientSize.Width - listView1.Columns[0].Width;// - listView1.Columns[1].Width;
            listView2.Items[0].SubItems[1].Text = NiuJuPID.pidParam.sp.ToString();
            listView2.Items[2].SubItems[1].Text = NiuJuPID.pidParam.pgain.ToString();
            listView2.Items[3].SubItems[1].Text = NiuJuPID.pidParam.igain.ToString();
            listView2.Items[4].SubItems[1].Text = NiuJuPID.pidParam.dgain.ToString();
            listView2.Columns[1].Width = listView2.ClientSize.Width - listView2.Columns[0].Width;// - listView1.Columns[1].Width;
        }

        private void listView1_Resize(object sender, EventArgs e)
        {
            listView1.Columns[1].Width = listView1.ClientSize.Width - listView1.Columns[0].Width;// - listView1.Columns[1].Width;
        }

        private void listView2_Resize(object sender, EventArgs e)
        {
            listView2.Columns[1].Width = listView2.ClientSize.Width - listView2.Columns[0].Width;// - listView1.Columns[1].Width;
        }
    }
}
