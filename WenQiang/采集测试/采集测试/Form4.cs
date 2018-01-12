using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CMOSTestLib.SaperaGUI;
using DALSA.SaperaLT.SapClassBasic;
using CMOSTestLib;
using System.Threading;

namespace 采集测试
{
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
            this.m_ImageBox = new CMOSTestLib.SaperaGUI.ImageBox();
            this.m_ImageBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_ImageBox.Location = new System.Drawing.Point(0, 0);
            this.m_ImageBox.Name = "m_ImageBox";
            this.m_ImageBox.PixelValueDisplay = this.PixelDataValue;
            //this.m_ImageBox.Size = new System.Drawing.Size(606, 283);
            this.m_ImageBox.SliderEnable = false;
            this.m_ImageBox.SliderMaximum = 10;
            this.m_ImageBox.SliderMinimum = 0;
            this.m_ImageBox.SliderValue = 0;
            this.m_ImageBox.SliderVisible = false;
            this.m_ImageBox.TabIndex = 13;
            this.m_ImageBox.TrackerEnable = false;
            this.m_ImageBox.View = null;
            this.Controls.Add(this.m_ImageBox);
        }
        ImageBox m_ImageBox;
        SapBuffer m_Buffers;
        SapView m_View;
        private SapAcquisition m_Acquisition;
        private SapAcqToBuf m_Xfer;
        private void Form4_Load(object sender, EventArgs e)
        {            
            StatusLabelInfo.Text = "offline... Load images";
            if (!SerialFunc.OpenSerialPort())
                this.Close();
//             if(SystemParam.cmosInfo==null)
//             {
//                 SystemParam.cmosInfo = SerialFunc.SerialCommand1();
//                 if (SystemParam.cmosInfo == null)
//                 {
//                     MessageBox.Show("与采集器通信失败");
//                     this.Close();
//                 }
//                 SystemParam.Ts = (double)SystemParam.cmosInfo.Ts / 100 / 1000 / 1000;//ms
//                 SystemParam.Pixel4Pic = (int)SystemParam.cmosInfo.ColPixels * SystemParam.cmosInfo.RowPixels;
//             }
            InitCam(1);
            toolStripTextBox2.Text = (trackBar1.Value * SystemParam.Ts).ToString("F3");
        }
        private SapLocation m_ServerLocation;
        private string m_ConfigFileName;
        private delegate void DisplayFrameAcquired(int number, bool trash);
        static void xfer_XferNotify1(object sender, SapXferNotifyEventArgs argsNotify)
        {
            Form4 GrabDlg = argsNotify.Context as Form4;
            // If grabbing in trash buffer, do not display the image, update the
            // appropriate number of frames on the status bar instead
            if (argsNotify.Trash)
                GrabDlg.Invoke(new DisplayFrameAcquired(GrabDlg.ShowFrameNumber), argsNotify.EventCount, true);
            // Refresh view
            else
            {
                GrabDlg.Invoke(new DisplayFrameAcquired(GrabDlg.ShowFrameNumber), argsNotify.EventCount, false);
                GrabDlg.m_View.Show();
            }            
        }
        static void GetSignalStatus1(object sender, SapSignalNotifyEventArgs argsSignal)
        {
            Form4 GrabDlg = argsSignal.Context as Form4;
            SapAcquisition.AcqSignalStatus signalStatus = argsSignal.SignalStatus;
            GrabDlg.Invoke((EventHandler)(delegate
            {
                GrabDlg.m_IsSignalDetected = (signalStatus != SapAcquisition.AcqSignalStatus.None);
                if (GrabDlg.m_IsSignalDetected == false)
                    GrabDlg.StatusLabelInfo.Text = "Online... No camera signal detected";
                else GrabDlg.StatusLabelInfo.Text = "Online... Camera signal detected";
            }));

        }
        private bool m_IsSignalDetected;
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
                str = String.Format("Frames acquired :{0}", number * m_Buffers.Count);
                this.StatusLabelInfo.Text = str;
            }
        }
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
            m_View = new SapView(m_Buffers);
            m_View.SetScalingMode(true);

            //event for view
            m_Xfer.Pairs[0].EventType = SapXferPair.XferEventType.EndOfFrame;
            m_Xfer.XferNotify += new SapXferNotifyHandler(xfer_XferNotify1);
            m_Xfer.XferNotifyContext = this;

            // event for signal status
            m_Acquisition.SignalNotify += new SapSignalNotifyHandler(GetSignalStatus1);
            m_Acquisition.SignalNotifyContext = this;

            m_ImageBox.View = m_View;
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
            float WidthScalor = (float)(this.Size.Width) / m_Buffers.Width;
            float HeightScalor = (float)(this.Size.Height) / m_Buffers.Height;
            m_View.SetScalingMode(WidthScalor, HeightScalor);
            m_ImageBox.OnSize();
            EnableSignalStatus();
            SystemParam.ByteLen4Pic = SystemParam.Pixel4Pic * m_Buffers.BytesPerPixel;
            m_Xfer.Grab();
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
            if (m_View != null && !m_View.Initialized)
            {
                if (m_View.Create() == false)
                {
                    DestroyObjects();
                    return false;
                }
            }
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
            if (m_View != null && m_View.Initialized)
                m_View.Destroy();
            if (m_Buffers != null && m_Buffers.Initialized)
                m_Buffers.Destroy();
            if (m_Acquisition != null && m_Acquisition.Initialized)
                m_Acquisition.Destroy();
        }

        private void DisposeObjects()
        {
            if (m_Xfer != null)
            { m_Xfer.Dispose(); m_Xfer = null; }
            if (m_View != null)
            { m_View.Dispose(); m_View = null; m_ImageBox.View = null; }
            if (m_Buffers != null)
            { m_Buffers.Dispose(); m_Buffers = null; }
            if (m_Acquisition != null)
            { m_Acquisition.Dispose(); m_Acquisition = null; }

        }

        private void Form4_SizeChanged(object sender, EventArgs e)
        {
            if (m_View != null)
            {
                float WidthScalor = (float)(this.Size.Width) / m_Buffers.Width;
                float HeightScalor = (float)(this.Size.Height) / m_Buffers.Height;
                m_View.SetScalingMode(WidthScalor, HeightScalor);
                m_ImageBox.OnSize();
            }
        }

        private void panel1_SizeChanged(object sender, EventArgs e)
        {
            if (m_View != null)
            {
                float WidthScalor = (float)(this.Size.Width) / m_Buffers.Width;
                float HeightScalor = (float)(this.Size.Height) / m_Buffers.Height;
                m_View.SetScalingMode(WidthScalor, HeightScalor);
                m_ImageBox.OnSize();
            }
        }

        private void Form4_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_Xfer.Freeze();
            DestroyObjects();
            DisposeObjects();
            SerialFunc.CloseSerialPort();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            toolStripTextBox2.Text = (trackBar1.Value * SystemParam.Ts).ToString("F3");
        }
        int rxFrame;
        CMOSTestLib.WaitingProc waitProc;
        ushort rxNeed;
        uint ls;
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            try
            {
                rxNeed = ushort.Parse(toolStripTextBox1.Text);
            }
            catch
            {
                MessageBox.Show("拍照张数设定有误");
            }
            ls = (uint)trackBar1.Value;
            rxFrame = 0;            
            waitProc = new CMOSTestLib.WaitingProc();
            waitProc.MaxProgress = rxNeed;
            CMOSTestLib.WaitingProcFunc wpf = new CMOSTestLib.WaitingProcFunc(手动拍照);
            if (!waitProc.Execute(wpf, "手动拍照", CMOSTestLib.WaitingType.None, ""))
            {
                toolStrip1.Enabled = true;
                m_ImageBox.OnSize();
                return;
            }
            m_ImageBox.OnSize();
        }
        void 手动拍照(object LockWatingThread)
        {
            SerialFunc.SerialCommand3(rxNeed, ls);
            while(true)
            {
                WFNetLib.WFGlobal.WaitMS(1);
                if (waitProc.HasBeenCancelled())
                {
                    break;
                }
                else
                {
                    waitProc.SetProcessBar(rxFrame);
                }
                if (rxFrame == rxNeed)
                    break;
            }            
        }
    }
}
