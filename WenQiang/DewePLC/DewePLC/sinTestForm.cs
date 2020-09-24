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
    public partial class sinTestForm : Form
    {
        public sinTestForm(Form1 _f)
        {
            InitializeComponent();
            form1 = _f;
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
        Form1 form1;
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            motorRev = double.Parse(toolStripTextBox1.Text)*7;
            motorTorque = double.Parse(toolStripTextBox2.Text);
            Amplitude = double.Parse(toolStripTextBox3.Text);
            Torque = motorTorque;
            omega = 50;
            wpStart = new WaitingProc();
            wpStart.MaxProgress = 10;
            WaitingProcFunc wpf = new WaitingProcFunc(WaitingStart);
            if (wpStart.Execute(wpf, "等待电机启动", WaitingType.WithCancel, ""))
                toolStripButton1.Enabled = false;
        }
        double motorRev, motorTorque, Amplitude, Torque;
        double omega,t;

        private void sinTestForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            waitTimer.Enabled = false;
            openTimer.Enabled = false;
        }

        private void openTimer_Tick(object sender, EventArgs e)
        {
            openTimes++;
            if (openTimes < 10)
            {
                form1.ReadDeweData();
                this.Invoke((EventHandler)(delegate
                {
                    chart1.Series[0].Points.AddY(form1.deweNiuZhen);
                    chart2.Series[0].Points.AddY(form1.deweNiuJu);
                    chart2.Series[1].Points.AddY(Torque);
                    chart2.Series[2].Points.AddY(motorTorque);
                }));
            }
            else
            {
                openTimer.Enabled = false;
                waitTimer.Enabled = true;
            }
        }

        private void waitTimer_Tick(object sender, EventArgs e)
        {
            form1.ReadDeweData();
            this.Invoke((EventHandler)(delegate
            {
                chart1.Series[0].Points.AddY(form1.deweNiuZhen);
                chart2.Series[0].Points.AddY(form1.deweNiuJu);
                chart2.Series[1].Points.AddY(Torque);
                chart2.Series[2].Points.AddY(motorTorque);
            }));
            t = t + waitTimer.Interval / 1000;
            motorTorque = Torque + Amplitude * Math.Sin(omega * t);
            form1.SetMotor(motorRev, motorTorque);
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

        int openTimes;
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
                        form1.SetMotor(motorRev, motorTorque);
                        this.Invoke((EventHandler)(delegate
                        {
                            chart1.Series[0].Points.Clear();
                            chart2.Series[0].Points.Clear();
                            chart2.Series[1].Points.Clear();
                            chart2.Series[2].Points.Clear();
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
    }
}
