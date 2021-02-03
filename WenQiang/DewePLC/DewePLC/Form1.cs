using System;
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
            iniFileOP.iniFilePath = System.Windows.Forms.Application.StartupPath + "\\Config.ini";
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
            opcUrl = iniFileOP.Read("System Run", "opcUrl");
            deweNodes = new string[2];
            deweNodes[0] = iniFileOP.Read("System Run", "NodeNiuJuAddr");
            deweNodes[1] = iniFileOP.Read("System Run", "NodeRevAddr");
            //InitSystemParam();
            PLCConnect();
            //ConnectOPCServer();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            waitTimer.Enabled = false;
            openTimer.Enabled = false;
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
        void ReadSystemParam()
        {
            double x;
            NiuJuPID.pgain = double.Parse(iniFileOP.Read("System Setting", "NiuJu_pgain"));
            NiuJuPID.igain = double.Parse(iniFileOP.Read("System Setting", "NiuJu_igain"));
            NiuJuPID.dgain = double.Parse(iniFileOP.Read("System Setting", "NiuJu_dgain"));

            deweNiuJu_k = double.Parse(iniFileOP.Read("System Setting", "deweNiuJu_k"));
            deweNiuJu_b = double.Parse(iniFileOP.Read("System Setting", "deweNiuJu_b"));
            deweNiuZhen_k = double.Parse(iniFileOP.Read("System Setting", "deweNiuZhen_k"));
            deweNiuZhen_b = double.Parse(iniFileOP.Read("System Setting", "deweNiuZhen_b"));

            NiuZhenPID.sp = double.Parse(iniFileOP.Read("System Setting", "NiuZhen_sp"));
            NiuJuPID.sp = double.Parse(iniFileOP.Read("System Setting", "NiuJu_sp"));
            R2 = (_R2)int.Parse(iniFileOP.Read("System Setting", "R2"));

            x = double.Parse(iniFileOP.Read("System Setting", "NiuZhen_Deadband"));
            NiuZhenPID.deadband = x * NiuZhenPID.sp;
            x = double.Parse(iniFileOP.Read("System Setting", "NiuJu_Deadband"));
            NiuJuPID.deadband = x * NiuJuPID.sp;


            NiuZhenPID.pgain = double.Parse(iniFileOP.Read(R2.ToString(), R2.ToString() + "_pgain"));
            NiuZhenPID.igain = double.Parse(iniFileOP.Read(R2.ToString(), R2.ToString() + "_igain"));
            NiuZhenPID.dgain = double.Parse(iniFileOP.Read(R2.ToString(), R2.ToString() + "_dgain"));

            double a, b, c;
            a = double.Parse(iniFileOP.Read(R2.ToString(), R2.ToString() + "_a"));
            b = double.Parse(iniFileOP.Read(R2.ToString(), R2.ToString() + "_b"));
            c = double.Parse(iniFileOP.Read(R2.ToString(), R2.ToString() + "_c"));
            openRev = a * NiuZhenPID.sp * NiuZhenPID.sp + b * NiuZhenPID.sp + c;
            openRev = openRev * 7;


            AllTime1 = int.Parse(iniFileOP.Read(R2.ToString(), R2.ToString() + "_AllTime1"));
            AllTime2 = int.Parse(iniFileOP.Read(R2.ToString(), R2.ToString() + "_AllTime2"));
            OpenTime = int.Parse(iniFileOP.Read(R2.ToString(), R2.ToString() + "_OpenTime"));
            openTimesCount = OpenTime / openTimer.Interval;


            listView1.Items[0].SubItems[1].Text = NiuZhenPID.sp.ToString();
            listView1.Items[3].SubItems[1].Text = NiuZhenPID.pgain.ToString();
            listView1.Items[4].SubItems[1].Text = NiuZhenPID.igain.ToString();
            listView1.Items[5].SubItems[1].Text = NiuZhenPID.dgain.ToString();
            listView1.Columns[1].Width = listView1.ClientSize.Width - listView1.Columns[0].Width;       

            listView2.Items[0].SubItems[1].Text = NiuJuPID.sp.ToString();
            listView2.Items[3].SubItems[1].Text = NiuJuPID.pgain.ToString();
            listView2.Items[4].SubItems[1].Text = NiuJuPID.igain.ToString();
            listView2.Items[5].SubItems[1].Text = NiuJuPID.dgain.ToString();
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
        int openTimesCount;
        int AllTime1, AllTime2,OpenTime;
        DateTime startDT;
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            NiuJuPID.ResetPIDParam();
            NiuZhenPID.ResetPIDParam();
            WorkSetForm f = new WorkSetForm();
            if(f.ShowDialog()==DialogResult.OK)
            {                
                ReadSystemParam();
                SetAllTime(AllTime1, AllTime2);
                wpStart = new WaitingProc();
                wpStart.MaxProgress = 10;
                WaitingProcFunc wpf = new WaitingProcFunc(WaitingStart);
                if (wpStart.Execute(wpf, "等待电机启动", WaitingType.WithCancel, ""))
                {
                    startDT = DateTime.Now;
                    DateTime dt = DateTime.Now;
                    TimeSpan ts = dt.Subtract(startDT);
                    tbTime1.Visible = true;
                    tbTime.Visible = true;
                    tbTime.Text = ts.ToString(@"hh\:mm\:ss");
                    timer1.Enabled = true;
                    toolStripButton4.Enabled = false;
                    splitContainer1.Enabled = true;
                }
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
                            listView1.Items[6].SubItems[1].Text = openRev.ToString("F3");
                            listView2.Items[6].SubItems[1].Text = NiuJuPID.sp.ToString();
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

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            sinTestForm f = new sinTestForm(this);
            f.ShowDialog();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime dt = DateTime.Now;
            TimeSpan ts = dt.Subtract(startDT);
            tbTime.Text = ts.ToString(@"hh\:mm\:ss");
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            SettingForm f = new SettingForm();
            if(f.ShowDialog()==DialogResult.OK)
            {

            }
        }

        private void openTimer_Tick(object sender, EventArgs e)
        {
            openTimes++;
            if(openTimes<openTimesCount)
            {
                ReadDeweData();
                this.Invoke((EventHandler)(delegate
                {
                    listView1.Items[1].SubItems[1].Text = deweNiuZhen.ToString();
                    listView2.Items[1].SubItems[1].Text = deweNiuJu.ToString();
                    listView1.Items[2].SubItems[1].Text = (100*(NiuZhenPID.sp-deweNiuZhen)/NiuZhenPID.sp).ToString("F1");
                    listView2.Items[2].SubItems[1].Text = (100*(NiuJuPID.sp - deweNiuJu) / NiuJuPID.sp).ToString("F1");
                    chart1.Series[0].Points.AddY(deweNiuZhen);
                    chart1.Series[1].Points.AddY(NiuZhenPID.sp);
                    chart2.Series[0].Points.AddY(deweNiuJu);
                    chart2.Series[1].Points.AddY(NiuJuPID.sp);
                }));
                checkPLC();
            }
            else
            {
                openTimer.Enabled = false;
                waitTimer.Enabled = true;
            }
        }
        void checkPLC()
        {
            TcpModbusPacket tp = ReadPLC();
            if (tp != null)
            {
                ushort D100 = BytesOP.MakeByte(tp.Data[1], tp.Data[2]);
                if (!BytesOP.GetBit(D100, 0))//系统外部停机
                {                    
                    this.Invoke((EventHandler)(delegate
                    {
                        openTimer.Enabled = false;
                        waitTimer.Enabled = false;
                        splitContainer1.Enabled = false;
                        toolStripButton4.Enabled = true;
                        listView1.BackColor = SystemColors.Window;
                        listView2.BackColor = SystemColors.Window;
                        tbTime1.Visible = false;
                        tbTime.Visible = false;
                        timer1.Enabled = false;
                    }));
                    MessageBox.Show("测试停止");
                }
            }
        }
        double motorRev, motorTorque;
        private void waitTimer_Tick(object sender, EventArgs e)
        {
            ReadDeweData();            
            this.Invoke((EventHandler)(delegate
            {
                motorRev = NiuZhenPID.PIDCalc(deweNiuZhen);
                chart1.Series[0].Points.AddY(deweNiuZhen);
                chart1.Series[1].Points.AddY(NiuZhenPID.sp);
                motorTorque = NiuJuPID.PIDCalc(deweNiuJu);
                chart2.Series[0].Points.AddY(deweNiuJu);
                chart2.Series[1].Points.AddY(NiuJuPID.sp);
                listView1.Items[6].SubItems[1].Text = motorRev.ToString("F3");
                listView1.Items[1].SubItems[1].Text = deweNiuZhen.ToString();
                listView2.Items[6].SubItems[1].Text = motorTorque.ToString();
                listView2.Items[1].SubItems[1].Text = deweNiuJu.ToString();
                listView1.Items[2].SubItems[1].Text = (100*(NiuZhenPID.sp - deweNiuZhen) / NiuZhenPID.sp).ToString("F1");
                listView2.Items[2].SubItems[1].Text = (100*(NiuJuPID.sp - deweNiuJu) / NiuJuPID.sp).ToString("F1");
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
            checkPLC();
        }
    }
}
