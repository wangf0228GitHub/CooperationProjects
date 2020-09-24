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

namespace sinTest
{
    public partial class sinTestForm : Form
    {
        public sinTestForm()
        {
            InitializeComponent();
        }
        public void ShowText(string str)
        {
            this.Invoke((EventHandler)(delegate
            {
                if (textBox1.Text.Length > 20000)
                    textBox1.Clear();
                textBox1.AppendText(str + "\r\n");
            }));
        }
        WaitingProc wpStart;
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            motorRev = double.Parse(toolStripTextBox1.Text);
            motorTorque = double.Parse(toolStripTextBox2.Text);
            Amplitude = double.Parse(toolStripTextBox3.Text);
            wpStart = new WaitingProc();
            wpStart.MaxProgress = 10;
            WaitingProcFunc wpf = new WaitingProcFunc(WaitingStart);
            wpStart.Execute(wpf, "等待电机启动", WaitingType.WithCancel, "");
        }
        double motorRev, motorTorque, Amplitude;
        int openTimes;

        private void openTimer_Tick(object sender, EventArgs e)
        {
            openTimes++;
            if (openTimes < 10)
            {
                ReadDeweData();
                this.Invoke((EventHandler)(delegate
                {
                    listView1.Items[1].SubItems[1].Text = deweNiuZhen.ToString();
                    listView2.Items[1].SubItems[1].Text = deweNiuJu.ToString();
                    listView1.Items[2].SubItems[1].Text = (100 * (NiuZhenPID.sp - deweNiuZhen) / NiuZhenPID.sp).ToString("F1");
                    listView2.Items[2].SubItems[1].Text = (100 * (NiuJuPID.sp - deweNiuJu) / NiuJuPID.sp).ToString("F1");
                    chart1.Series[0].Points.AddY(deweNiuZhen);
                    chart1.Series[1].Points.AddY(NiuZhenPID.sp);
                    chart2.Series[0].Points.AddY(deweNiuJu);
                    chart2.Series[1].Points.AddY(NiuJuPID.sp);
                }));
            }
            else
            {
                openTimer.Enabled = false;
                waitTimer.Enabled = true;
            }
        }

        void WaitingStart(object LockWatingThread)
        {
            while (true)
            {
                TcpModbusPacket tp = ReadPLC();
                if (tp != null)
                {
                    ushort D100 = BytesOP.MakeByte(tp.Data[1], tp.Data[2]);
                    if (BytesOP.GetBit(D100, 0))//系统已经启动
                    {                        
                        SetMotor(motorRev, motorTorque);
                        
                        this.Invoke((EventHandler)(delegate
                        {
                            chart2.Series[0].Points.Clear();
                            chart2.Series[1].Points.Clear();
                            openTimes = 0;
                            openTimer.Enabled = true;
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
        private void Form1_Load(object sender, EventArgs e)
        {
            PLCConnect();
        }
    }
}
