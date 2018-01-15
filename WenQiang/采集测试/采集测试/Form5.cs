using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WFNetLib;
using CMOSTestLib;
using DALSA.SaperaLT.SapClassBasic;
using WFNetLib.Log;
using System.Threading;
using CMOSTestLib.SaperaGUI;
using System.IO;

namespace 采集测试
{
    public partial class Form5 : Form
    {
        public Form5()
        {
            InitializeComponent();
        }
        int MaxTime;
        int PhotoNs;
        private void Form5_Load(object sender, EventArgs e)
        {
            toolStripTextBox1.Text = iniFileOP.Read("System Run", "MaxTime");
            toolStripTextBox2.Text = iniFileOP.Read("System Run", "PhotoNs");
            toolStripTextBox3.Text = iniFileOP.Read("System Run", "PicN");
            if (!SerialFunc.OpenSerialPort())
                this.Close();
            if (SystemParam.cmosInfo == null)
            {
                SystemParam.cmosInfo = SerialFunc.SerialCommand1();
                if (SystemParam.cmosInfo == null)
                {
                    MessageBox.Show("与采集器通信失败");
                    this.Close();
                }
                SystemParam.Ts = (double)SystemParam.cmosInfo.Ts / 100 / 1000 / 1000;//ms
                SystemParam.Pixel4Pic = (int)SystemParam.cmosInfo.ColPixels * SystemParam.cmosInfo.RowPixels;
            }
            if (!CreateObjects())
            {
                DisposeObjects();
                return;
            }
        }
        CMOSTestLib.WaitingProc waitProc;
        DateTime dtStart;
        string fileName;
        int PicN;
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("请转入暗场，点击确定继续");
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            chart1.ChartAreas[0].AxisY.Title = "暗场均值";
            chart1.ChartAreas[1].AxisY.Title = "暗场方差";
            toolStrip1.Enabled = false;
            try
            {
                MaxTime = int.Parse(toolStripTextBox1.Text);
                PhotoNs = int.Parse(toolStripTextBox2.Text);
                PicN = int.Parse(toolStripTextBox3.Text);
                iniFileOP.Write("System Run", "MaxTime", toolStripTextBox1.Text);
                iniFileOP.Write("System Run", "PhotoNs", toolStripTextBox2.Text);
                iniFileOP.Write("System Run", "PicN", toolStripTextBox3.Text);
            }
            catch //(System.Exception ex)
            {
                return;
            }
            dtStart = DateTime.Now;
            fileName = SystemParam.TxtCustomDarkDataPath + dtStart.ToString("yyyyMMdd_HHmmss") + "\\数据" + ".txt";
            TextLog.AddTextLog("--------------" + dtStart.ToString() + "----------------",fileName , false);
            TextLog.AddTextLog(String.Format(SystemParam.TxtCustomDarkDataTitleFormat, "曝光时间(ms)", "暗场均值", "暗场方差"), fileName, false);
            //第一步、采集图像
            InitCam(PicN+1);
            StatusLabelInfoTrash.Text = "";
            m_Xfer.Grab();
            waitProc = new CMOSTestLib.WaitingProc();
            waitProc.MaxProgress = PhotoNs;
            CMOSTestLib.WaitingProcFunc wpf = new CMOSTestLib.WaitingProcFunc(暗场采集);
            if (!waitProc.Execute(wpf, "自定义暗场测试", CMOSTestLib.WaitingType.None, ""))
            {
                MessageBox.Show("用户终止自定义暗场测试");
                toolStrip1.Enabled = true;
                return;
            }
            MessageBox.Show("自定义暗场测试完成");
            toolStrip1.Enabled = true;            
        }
        //uint maxLs = 0xffff00;
        byte[] ya, yb;
        void 暗场采集(object LockWatingThread)
        {
            double y, d;
            double t;
            Calc1.miu_y_dark = new List<double>();
            Calc1.delta_y_dark = new List<double>();
            this.Invoke((EventHandler)(delegate
            {
                chart1.Series[0].Points.Clear();
                chart1.Series[1].Points.Clear();
                chart1.ChartAreas[0].AxisY.Title = "暗场均值";
                chart1.ChartAreas[1].AxisY.Title = "暗场方差";
            }));
            NopCam((ushort)(PicN+1), SystemParam.eStart);
            int saveindex = 0;
            int step = (int)(MaxTime / SystemParam.Ts / PhotoNs);
            for (uint i = 0; i < PhotoNs; i++)
            {
                uint ls = SystemParam.eStart + (uint)(i * step);                
                this.Invoke((EventHandler)(delegate
                {
                    toolStripLabel3.Text = SystemParam.GetTime((int)i).ToString("F2") + " ms";
                }));
                if (waitProc.HasBeenCancelled())
                {
                    return;
                }
                SerialFunc.SerialCommand3((ushort)(PicN + 1), ls);
                if (!WaitCam((ushort)(PicN + 1)))
                {
                    i--;
                    continue;
                }
                for (int j = 1; j < PicN + 1; j++)
                {
                    saveindex++;
                    m_Buffers.Save(Calc1.TempPicPath_Dark + saveindex.ToString() + ".bmp", "-format bmp", j, 0);
                    Thread.Sleep(SystemParam.PicDelay);
                }
                byte[] yByteArray;
                t = ls * SystemParam.Ts;
                for (int j = 1; j < PicN + 1; j++)
                {
                    yByteArray = wfSapGUI.ReadPicDatas(m_Buffers, j);
                    SystemParam.CreateBINFile(yByteArray, SystemParam.TxtCustomDarkDataPath + dtStart.ToString("yyyyMMdd_HHmmss") + "\\" + t.ToString("F3") + "_"+j.ToString()+".bin");                    
                }
//                 yByteArray = wfSapGUI.ReadPicDatas(m_Buffers, 1);
//                 SystemParam.CreateBINFile(yByteArray, SystemParam.TxtCustomDarkDataPath + dtStart.ToString("yyyyMMdd_HHmmss") + "\\" + t.ToString("F3") + "_1.bin");
//                 yByteArray = wfSapGUI.ReadPicDatas(m_Buffers, 2);
//                 SystemParam.CreateBINFile(yByteArray, SystemParam.TxtCustomDarkDataPath + dtStart.ToString("yyyyMMdd_HHmmss") + "\\" + t.ToString("F3") + "_2.bin");
                ya = wfSapGUI.ReadPicDatas(m_Buffers, PicN);
                //yb = wfSapGUI.ReadPicDatas(m_Buffers, 2);
                Calc1.TestExposureTime1(ya, m_Buffers.Height, m_Buffers.Width,SystemParam.cmosInfo.PixelDepth, out y, out d);
                this.Invoke((EventHandler)(delegate
                {                    
                    chart1.Series[0].Points.AddXY(t, y);
                    chart1.Series[1].Points.AddXY(t, d);
                    waitProc.SetProcessBar((int)(i + 1));
                }));
                TextLog.AddTextLog(String.Format(SystemParam.TxtCustomDarkDataFormat, t.ToString("F3"), y.ToString("F6"), d.ToString("F6")), fileName, false);
            }
        }
        bool bWaitTimeOut;
        bool WaitCam(int count)
        {
            rxFrame = 0;
            bTrask = false;
            WaitMsTime = 0;
            bWaitTimeOut = false;
            while (rxFrame < count)
            {
                WFNetLib.WFGlobal.WaitMS(1);
                if (waitProc.HasBeenCancelled())
                {
                    return true;
                }
                if (bTrask)
                    break;
                lock (oWaitMsTime)
                {
                    WaitMsTime++;
                }
                if (WaitMsTime > 2000)
                {
                    bWaitTimeOut = true;
                    this.Invoke((EventHandler)(delegate
                    {
                        //textBox1.AppendText("采集图片超时重试\r\n");
                    }));
                    break;
                }
            }
            if (bTrask)
            {
                WFNetLib.WFGlobal.WaitMS(1000);
                MessageBox.Show("垃圾图片");
                return false;
            }
            if (bWaitTimeOut)
            {
                MessageBox.Show("采集超时");
                return false;                
            }
            return true;
        }
        void NopCam(ushort count, uint ls)
        {
            while (true)
            {
                SerialFunc.SerialCommand3(count, ls);
                if (WaitCam(count))
                    break;
            }
            WFGlobal.WaitMS(200);
        }
        private SapAcquisition m_Acquisition;
        private SapBuffer m_Buffers;
        private SapAcqToBuf m_Xfer;
        private bool CreateObjects()
        {
            // Create acquisition object
            if (m_Acquisition != null && !m_Acquisition.Initialized)
            {
                if (m_Acquisition.Create() == false)
                {
                    DestroyObjects();
                    return false;
                }
            }
            // Create buffer object
            if (m_Buffers != null && !m_Buffers.Initialized)
            {
                if (m_Buffers.Create() == false)
                {
                    DestroyObjects();
                    return false;
                }
                m_Buffers.Clear();
            }
            // Create view object
            //             if (m_View != null && !m_View.Initialized)
            //             {
            //                 if (m_View.Create() == false)
            //                 {
            //                     DestroyObjects();
            //                     return false;
            //                 }
            //             }
            // Create Xfer object
            if (m_Xfer != null && !m_Xfer.Initialized)
            {
                if (m_Xfer.Create() == false)
                {
                    DestroyObjects();
                    return false;
                }
            }
            return true;
        }
        private void DestroyObjects()
        {
            if (m_Xfer != null && m_Xfer.Initialized)
                m_Xfer.Destroy();
            //             if (m_View != null && m_View.Initialized)
            //                 m_View.Destroy();
            if (m_Buffers != null && m_Buffers.Initialized)
                m_Buffers.Destroy();
            if (m_Acquisition != null && m_Acquisition.Initialized)
                m_Acquisition.Destroy();
        }

        private void DisposeObjects()
        {
            if (m_Xfer != null)
            { m_Xfer.Dispose(); m_Xfer = null; }
            //             if (m_View != null)
            //             { m_View.Dispose(); m_View = null; m_ImageBox.View = null; }
            if (m_Buffers != null)
            { m_Buffers.Dispose(); m_Buffers = null; }
            if (m_Acquisition != null)
            { m_Acquisition.Dispose(); m_Acquisition = null; }

        }
        private SapLocation m_ServerLocation;
        private string m_ConfigFileName;
        void InitCam(int n)
        {
            DestroyObjects();
            DisposeObjects();
            m_ServerLocation = new SapLocation("X64-CL_iPro_1", 0);
            m_ConfigFileName = "IAG3.ccf";
            // define on-line object
            m_Acquisition = new SapAcquisition(m_ServerLocation, m_ConfigFileName);

            m_Buffers = new SapBufferWithTrash(n, m_Acquisition, SapBuffer.MemoryType.ScatterGather);

            m_Buffers.PixelDepth = SystemParam.cmosInfo.PixelDepth;
            m_Buffers.Format = SapFormat.Mono16;
            m_Buffers.Height = SystemParam.cmosInfo.ColPixels;
            m_Buffers.Width = SystemParam.cmosInfo.RowPixels;
            m_Xfer = new SapAcqToBuf(m_Acquisition, m_Buffers);
            //                 m_View = new SapView(m_Buffers);
            //                 m_View.SetScalingMode(true);

            //event for view
            m_Xfer.Pairs[0].EventType = SapXferPair.XferEventType.EndOfFrame;
            m_Xfer.XferNotify += new SapXferNotifyHandler(xfer_XferNotify);
            m_Xfer.XferNotifyContext = this;

            // event for signal status
            m_Acquisition.SignalNotify += new SapSignalNotifyHandler(GetSignalStatus);
            m_Acquisition.SignalNotifyContext = this;

            //m_ImageBox.View = m_View;
            while (true)
            {
                try
                {
                    if (!CreateObjects())
                    {
                        DisposeObjects();
                        WFNetLib.WFGlobal.WaitMS(20);
                        continue;
                    }
                    break;
                }
                catch
                {
                    WFNetLib.WFGlobal.WaitMS(20);
                }
            }
            //m_View.SetScalingMode(WidthScalor, HeightScalor);
            //m_ImageBox.OnSize();
            EnableSignalStatus();
            SystemParam.ByteLen4Pic = SystemParam.Pixel4Pic * m_Buffers.BytesPerPixel;
            m_Xfer.Freeze();

        }
        private void EnableSignalStatus()
        {
            if (m_Acquisition != null)
            {
                m_IsSignalDetected = (m_Acquisition.SignalStatus != SapAcquisition.AcqSignalStatus.None);
                if (m_IsSignalDetected == false)
                    StatusLabelInfo.Text = "Online... No camera signal detected";
                else
                    StatusLabelInfo.Text = "Online... Camera signal detected";
                m_Acquisition.SignalNotifyEnable = true;
            }
        }
        private delegate void DisplayFrameAcquired(int number, bool trash);
        static void xfer_XferNotify(object sender, SapXferNotifyEventArgs argsNotify)
        {
            Form5 GrabDlg = argsNotify.Context as Form5;
            // If grabbing in trash buffer, do not display the image, update the
            // appropriate number of frames on the status bar instead
            if (argsNotify.Trash)
                GrabDlg.Invoke(new DisplayFrameAcquired(GrabDlg.ShowFrameNumber), argsNotify.EventCount, true);
            // Refresh view
            else
            {
                GrabDlg.Invoke(new DisplayFrameAcquired(GrabDlg.TestStep1), argsNotify.EventCount, false);
                //GrabDlg.m_View.Show();
            }            
        }
        bool bTrask;
        int WaitMsTime;
        private void TestStep1(int number, bool trash)
        {
            String str;
            if (trash)
            {
                str = String.Format("Frames acquired in trash buffer: {0}", number * m_Buffers.Count);
                this.StatusLabelInfoTrash.Text = str;
                bTrask = true;
            }
            else
            {
                rxFrame++;
                lock (oWaitMsTime)
                {
                    WaitMsTime = 0;
                }
                //textBox1.AppendText((m_Buffers.Index + 1).ToString() + "\r\n");
                str = String.Format("Frames acquired :{0}", number * m_Buffers.Count);
                this.StatusLabelInfo.Text = str;
            }
        }
        object oWaitMsTime = new object();
        int rxFrame;
        private void ShowFrameNumber(int number, bool trash)
        {
            String str;
            if (trash)
            {
                str = String.Format("Frames acquired in trash buffer: {0}", number * m_Buffers.Count);
                this.StatusLabelInfoTrash.Text = str;
            }
            else
            {
                rxFrame++;
                //textBox1.AppendText((m_Buffers.Index + 1).ToString() + "\r\n");
                str = String.Format("Frames acquired :{0}", number * m_Buffers.Count);
                this.StatusLabelInfo.Text = str;
            }
        }
        private bool m_IsSignalDetected;
        static void GetSignalStatus(object sender, SapSignalNotifyEventArgs argsSignal)
        {
            Form5 GrabDlg = argsSignal.Context as Form5;
            SapAcquisition.AcqSignalStatus signalStatus = argsSignal.SignalStatus;
            GrabDlg.Invoke((EventHandler)(delegate
            {
                GrabDlg.m_IsSignalDetected = (signalStatus != SapAcquisition.AcqSignalStatus.None);
                if (GrabDlg.m_IsSignalDetected == false)
                    GrabDlg.StatusLabelInfo.Text = "Online... No camera signal detected";
                else GrabDlg.StatusLabelInfo.Text = "Online... Camera signal detected";
            }));

        }

        private void Form5_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                m_Xfer.Freeze();
            }
            catch { }
            DestroyObjects();
            DisposeObjects();
            SerialFunc.CloseSerialPort();
        }
    }
}
