using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WFNetLib;
using WFNetLib.PacketProc;

namespace DewePLC
{
    public partial class RevTestForm : Form
    {
        Form1 form1;
        public void ShowText(string str)
        {
            this.Invoke((EventHandler)(delegate
            {
                if (textBox1.Text.Length > 20000)
                    textBox1.Clear();
                textBox1.AppendText(str + "\r\n");
            }));
        }
        public RevTestForm(Form1 _f)
        {
            InitializeComponent();
            form1 = _f;
        }
        double motorRev;
        WaitingProc wpStart;
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (!double.TryParse(toolStripTextBox1.Text, out motorRev))
            {
                MessageBox.Show("转速设定有误");
                return;
            }
            motorRev *= 7;
            if (motorRev > 3200 || motorRev <= 0)
            {
                MessageBox.Show("转速设定有误");
                return;
            }
            wpStart = new WaitingProc();
            wpStart.MaxProgress = 10;
            WaitingProcFunc wpf = new WaitingProcFunc(WaitingStart);
            if (wpStart.Execute(wpf, "等待电机启动", WaitingType.WithCancel, ""))
                toolStripButton1.Enabled = false;
        }

        private void RevTestForm_Load(object sender, EventArgs e)
        {

        }

        private void RevTestForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
        void WaitingStart(object LockWatingThread)
        {
            while (true)
            {
                TcpModbusPacket tp = form1.ReadPLC();
                if (tp != null)
                {
                    ushort D100 = BytesOP.MakeByte(tp.Data[1], tp.Data[2]);
                    if (BytesOP.GetBit(D100, 0))//系统已经启动
                    {
                        form1.SetMotor(motorRev, 0);
                        this.Invoke((EventHandler)(delegate
                        {
                            chart1.Series[0].Points.Clear();
                            waitTimer.Enabled = true;
                        }));
                        return;
                    }
                }
                Thread.Sleep(1000);
                lock (LockWatingThread)
                {
                    wpStart.SetProcessBarPerformStep();
                    if (wpStart.HasBeenCancelled())
                    {
                        return;
                    }
                }
            }
        }

        private void waitTimer_Tick(object sender, EventArgs e)
        {
            form1.ReadDeweRev();
            this.Invoke((EventHandler)(delegate
            {
                double d = form1.deweRev;
                chart1.Series[0].Points.AddY(d);
                lRev.Text = d.ToString("f1");
            }));
            TcpModbusPacket tp = form1.ReadPLC();
            if (tp != null)
            {
                ushort D100 = BytesOP.MakeByte(tp.Data[1], tp.Data[2]);
                if (!BytesOP.GetBit(D100, 0))//系统外部停机
                {
                    MessageBox.Show("测试停止");
                    this.Invoke((EventHandler)(delegate
                    {
                        waitTimer.Enabled = false;
                        toolStripButton1.Enabled = true;
                    }));
                }
            }
        }
    }
}
