﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
            PLCConnect();
            //ConnectOPCServer();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                opcUaClient.Disconnect();
                tcpPLC.Close();
            }
            catch
            {

            }            
        }
        string opcUrl;
        double deweNiuJu_k, deweNiuJu_b, deweNiuZhen_k, deweNiuZhen_b;
        void InitSystemParam()
        {
            iniFileOP.iniFilePath = System.Windows.Forms.Application.StartupPath + "\\Config.ini";
            R2 = (_R2)int.Parse(iniFileOP.Read("System Setting", "R2"));
            NiuZhenPID.sp = double.Parse(iniFileOP.Read("System Setting", "NiuZhen_sp"));
            NiuZhenPID.pgain = double.Parse(iniFileOP.Read(R2.ToString(), R2.ToString() + "_pgain"));
            NiuZhenPID.igain = double.Parse(iniFileOP.Read(R2.ToString(), R2.ToString() + "_igain"));
            NiuZhenPID.dgain = double.Parse(iniFileOP.Read(R2.ToString(), R2.ToString() + "_dgain"));
            listView1.Items[0].SubItems[1].Text = NiuZhenPID.sp.ToString();
            listView1.Items[2].SubItems[1].Text = NiuZhenPID.pgain.ToString();
            listView1.Items[3].SubItems[1].Text = NiuZhenPID.igain.ToString();
            listView1.Items[4].SubItems[1].Text = NiuZhenPID.dgain.ToString();
            listView1.Columns[1].Width = listView1.ClientSize.Width - listView1.Columns[0].Width;


            NiuJuPID.sp = double.Parse(iniFileOP.Read("System Setting", "NiuJu_sp"));
            NiuJuPID.pgain = double.Parse(iniFileOP.Read("System Setting", "NiuJu_pgain"));
            NiuJuPID.igain = double.Parse(iniFileOP.Read("System Setting", "NiuJu_igain"));
            NiuJuPID.dgain = double.Parse(iniFileOP.Read("System Setting", "NiuJu_dgain"));


            deweNiuJu_k = double.Parse(iniFileOP.Read("System Setting", "deweNiuJu_k"));
            deweNiuJu_b = double.Parse(iniFileOP.Read("System Setting", "deweNiuJu_b"));
            deweNiuZhen_k = double.Parse(iniFileOP.Read("System Setting", "deweNiuZhen_k"));
            deweNiuZhen_b = double.Parse(iniFileOP.Read("System Setting", "deweNiuZhen_b"));
            opcUrl = iniFileOP.Read("System Run", "opcUrl");
            deweNodes = new string[2];
            deweNodes[0] = iniFileOP.Read("System Run", "Node1Addr");
            deweNodes[1] = iniFileOP.Read("System Run", "Node2Addr");

            listView2.Items[0].SubItems[1].Text = NiuJuPID.sp.ToString();
            listView2.Items[2].SubItems[1].Text = NiuJuPID.pgain.ToString();
            listView2.Items[3].SubItems[1].Text = NiuJuPID.igain.ToString();
            listView2.Items[4].SubItems[1].Text = NiuJuPID.dgain.ToString();
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
        _R2 R2;
        double openRev;
        WaitingProc wpStart;        
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            NiuJuPID.ResetPIDParam();
            NiuZhenPID.ResetPIDParam();
            WorkSetForm f = new WorkSetForm();
            if(f.ShowDialog()==DialogResult.OK)
            {
                NiuJuPID.deadband = (double)f.numericUpDown3.Value;
                NiuZhenPID.deadband = (double)f.numericUpDown4.Value;
                R2=(_R2)f.comboBox1.SelectedIndex;
                NiuJuPID.sp = (double)f.numericUpDown1.Value;
                listView2.Items[0].SubItems[1].Text = NiuJuPID.sp.ToString();
                NiuZhenPID.sp = (double)f.numericUpDown2.Value;                
                NiuZhenPID.pgain = double.Parse(iniFileOP.Read(R2.ToString(), R2.ToString()+"_pgain"));
                NiuZhenPID.igain = double.Parse(iniFileOP.Read(R2.ToString(), R2.ToString() + "_igain"));
                NiuZhenPID.dgain = double.Parse(iniFileOP.Read(R2.ToString(), R2.ToString() + "_dgain"));
                listView1.Items[0].SubItems[1].Text = NiuZhenPID.sp.ToString();
                listView1.Items[2].SubItems[1].Text = NiuZhenPID.pgain.ToString();
                listView1.Items[3].SubItems[1].Text = NiuZhenPID.igain.ToString();
                listView1.Items[4].SubItems[1].Text = NiuZhenPID.dgain.ToString();
                listView1.Columns[1].Width = listView1.ClientSize.Width - listView1.Columns[0].Width;// - listView1.Columns[1].Width;
                double k, b;
                k= double.Parse(iniFileOP.Read(R2.ToString(), R2.ToString() + "_k"));
                b = double.Parse(iniFileOP.Read(R2.ToString(), R2.ToString() + "_b"));
                openRev = k * NiuZhenPID.sp + b;                
                wpStart = new WaitingProc();
                wpStart.MaxProgress = 10;
                WaitingProcFunc wpf = new WaitingProcFunc(WaitingStart);
                if (wpStart.Execute(wpf, "等待电机启动", WaitingType.WithCancel, ""))
                    toolStripButton4.Enabled = false;
            }
        }
        bool bForeward;
        void WaitingStart(object LockWatingThread)
        {
            while (true)
            {
                TcpModbusPacket tp = ReadPLC();
                if(tp!=null)
                {
                    ushort D100 = BytesOP.MakeByte(tp.Data[1], tp.Data[2]);
                    if (BytesOP.GetBit(D100, 0))//系统已经启动
                    {
                        if (BytesOP.GetBit(D100, 1))//判断正反转
                        {
                            bForeward = true;
                            this.Invoke((EventHandler)(delegate
                            {
                                listView1.Items[6].SubItems[1].Text = "正转";
                            }));
                        }
                        else
                        {
                            bForeward = false;
                            this.Invoke((EventHandler)(delegate
                            {
                                listView1.Items[6].SubItems[1].Text = "反转";
                            }));
                        }                        
                        SetMotor(openRev, NiuJuPID.sp);
                        motorRev = openRev;
                        motorTorque = NiuJuPID.sp;
                        this.Invoke((EventHandler)(delegate
                        {
                            listView1.Items[5].SubItems[1].Text = openRev.ToString("F3");
                            listView2.Items[5].SubItems[1].Text = NiuJuPID.sp.ToString();
                            listView1.Items[1].SubItems[1].Text = "";
                            listView2.Items[1].SubItems[1].Text = "";
                            chart1.Series[0].Points.Clear();
                            chart1.Series[1].Points.Clear();
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
        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
            ReadDeweData();
            this.Invoke((EventHandler)(delegate
            {
                listView1.Items[1].SubItems[1].Text = deweNiuZhen.ToString();
                listView2.Items[1].SubItems[1].Text = deweNiuJu.ToString();
            }));
        }

        int openTimes;
        private void openTimer_Tick(object sender, EventArgs e)
        {
            openTimes++;
            if(openTimes<10)
            {
                ReadDeweData();
                this.Invoke((EventHandler)(delegate
                {
                    listView1.Items[1].SubItems[1].Text = deweNiuZhen.ToString();
                    listView2.Items[1].SubItems[1].Text = deweNiuJu.ToString();
                }));
            }
            else
            {
                openTimer.Enabled = false;
                waitTimer.Enabled = true;
            }
        }
        double motorRev, motorTorque;
        private void waitTimer_Tick(object sender, EventArgs e)
        {
            ReadDeweData();
            motorRev = NiuZhenPID.PIDCalc(deweNiuZhen);            
            chart1.Series[0].Points.AddY(deweNiuZhen);
            chart1.Series[1].Points.AddY(NiuZhenPID.sp);
            motorTorque = NiuJuPID.PIDCalc(deweNiuJu);
            chart2.Series[0].Points.AddY(deweNiuJu);
            chart2.Series[1].Points.AddY(NiuJuPID.sp);
            this.Invoke((EventHandler)(delegate
            {
                listView1.Items[5].SubItems[1].Text = motorRev.ToString("F3");
                listView1.Items[1].SubItems[1].Text = deweNiuZhen.ToString();
                listView2.Items[5].SubItems[1].Text = motorTorque.ToString();
                listView2.Items[1].SubItems[1].Text = deweNiuJu.ToString();
                if (NiuZhenPID.bOk)
                    listView1.BackColor = Color.GreenYellow;
                else
                    listView1.BackColor = SystemColors.Window;
                if (NiuJuPID.bOk)
                    listView2.BackColor = Color.GreenYellow;
                else
                    listView2.BackColor = SystemColors.Window;
            }));
            SetMotor(motorRev, motorTorque);
            TcpModbusPacket tp = ReadPLC();
            if (tp != null)
            {
                ushort D100 = BytesOP.MakeByte(tp.Data[1], tp.Data[2]);
                if (!BytesOP.GetBit(D100, 0))//系统外部停机
                {
                    waitTimer.Enabled = false;
                    splitContainer1.Enabled = false;
                    toolStripButton4.Enabled = true;
                    MessageBox.Show("测试停止");
                    this.Invoke((EventHandler)(delegate
                    {
                        listView1.BackColor = SystemColors.Window;
                        listView2.BackColor = SystemColors.Window;
                    }));
                }
            }
        }
    }
}
