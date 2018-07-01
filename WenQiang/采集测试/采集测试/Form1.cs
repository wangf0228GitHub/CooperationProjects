using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using CMOSTestLib;
using DALSA.SaperaLT.SapClassBasic;
using CMOSTestLib.SaperaGUI;
using System.Runtime.InteropServices;
using System.IO;
using WFNetLib;
using System.Threading;
using WFNetLib.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using WFNetLib.Log;
using System.Diagnostics;

namespace 采集测试
{
    public partial class Form1 : Form
    {
        public static int CamEx = 0;
        public Form1()
        {
            InitializeComponent();
//             this.m_ImageBox = new CMOSTestLib.SaperaGUI.ImageBox();
//             this.m_ImageBox.Dock = System.Windows.Forms.DockStyle.Fill;
//             this.m_ImageBox.Location = new System.Drawing.Point(0, 0);
//             this.m_ImageBox.Name = "m_ImageBox";
//             this.m_ImageBox.PixelValueDisplay = this.PixelDataValue;
//             //this.m_ImageBox.Size = new System.Drawing.Size(606, 283);
//             this.m_ImageBox.SliderEnable = false;
//             this.m_ImageBox.SliderMaximum = 10;
//             this.m_ImageBox.SliderMinimum = 0;
//             this.m_ImageBox.SliderValue = 0;
//             this.m_ImageBox.SliderVisible = false;
//             this.m_ImageBox.TabIndex = 13;
//             this.m_ImageBox.TrackerEnable = false;
//             this.m_ImageBox.View = null;
//             this.m_ImageBox.bScroll = false;
//             this.splitContainer3.Panel1.Controls.Add(this.m_ImageBox);
            string fileName = Calc1.TempPicPath_Light + "1.tif";
            FileInfo f = new FileInfo(fileName);
            if (!Directory.Exists(f.DirectoryName))
                Directory.CreateDirectory(f.DirectoryName);
            fileName = Calc1.TempPicPath_Dark + "1.tif";
            f = new FileInfo(fileName);
            if (!Directory.Exists(f.DirectoryName))
                Directory.CreateDirectory(f.DirectoryName);
            fileName = Calc2.TempPicPath_Light + "1.tif";
            f = new FileInfo(fileName);
            if (!Directory.Exists(f.DirectoryName))
                Directory.CreateDirectory(f.DirectoryName);
            fileName = Calc2.TempPicPath_Dark + "1.tif";
            f = new FileInfo(fileName);
            if (!Directory.Exists(f.DirectoryName))
                Directory.CreateDirectory(f.DirectoryName);

            fileName = SystemParam.TempPicPath + "1.tif";
            f = new FileInfo(fileName);
            if (!Directory.Exists(f.DirectoryName))
                Directory.CreateDirectory(f.DirectoryName);

            fileName = SystemParam.TxtDataPath + "1.tif";
            f = new FileInfo(fileName);
            if (!Directory.Exists(f.DirectoryName))
                Directory.CreateDirectory(f.DirectoryName);
        }
        CMOSTestLib.WaitingProc waitProc;
        private void 开始测试_Click(object sender, EventArgs e)
        {
            //EnvironmentInfo ei = SerialFunc.SerialCommand2();  
            MessageBox.Show("请转入明场，点击确定继续");
            /************************************************************************/
            /* 界面初始化                                                           */
            /************************************************************************/
            textBox1.AppendText("-------------------------------------------------\r\n");
            for (int i = 0; i < chart1.Series.Count;i++ )
                chart1.Series[i].Points.Clear();
            chart1.ChartAreas[0].AxisY.Title = "明场均值";
            chart1.ChartAreas[1].AxisY.Title = "明场方差";
            toolStrip1.Enabled = false;
            /************************************************************************/
            /*                                                                      */
            /************************************************************************/
			SystemParam.DeviceID = InputBox.ShowInputBox("请设定当前测试器件的芯片编号", SystemParam.DeviceID);
            iniFileOP.Write("System Run", "DeviceID", SystemParam.DeviceID);
            Calc1.p1 = (int)((Calc1.percent_base - Calc1.percent) * SystemParam.ExposureTest_Ns/100);
            Calc1.p2 = (int)((Calc1.percent_base + Calc1.percent) * SystemParam.ExposureTest_Ns/100);
            InitListView();
            if (m_online)
            {
                CMOSInfo cmosInfo = SerialFunc.SerialCommand1();
                if (cmosInfo == null)
                {
                    MessageBox.Show("与采集器通信失败");
                    toolStrip1.Enabled = true;
                    return;
                }
                SystemParam.cmosInfo.Ts = cmosInfo.Ts;

                SystemParam.Ts = (double)SystemParam.cmosInfo.Ts / 100 / 1000 / 1000;//ms
                //SystemParam.Pixel4Pic = (int)SystemParam.cmosInfo.ColPixels * SystemParam.cmosInfo.RowPixels;
            }

            /************************************************************************/
            /*                                                                      */
            /************************************************************************/
            //第一步、采集图像
            testStep = 1;
            InitCam(2+CamEx);
            if (SystemParam.cmosInfo.bRGB != 0)
            {
                wfSapGUI.GetRGBPixelInfo(m_Buffers.Width, m_Buffers.Height, SystemParam.cmosInfo.RGB1, SystemParam.cmosInfo.RGB2, SystemParam.cmosInfo.RGB3, SystemParam.cmosInfo.RGB4);
            }
            StatusLabelInfoTrash.Text = "";  
            m_Xfer.Grab();
            waitProc = new CMOSTestLib.WaitingProc();
            waitProc.MaxProgress = SystemParam.ExposureTest_Ns;
            CMOSTestLib.WaitingProcFunc wpf = new CMOSTestLib.WaitingProcFunc(曝光测试);
            if (!waitProc.Execute(wpf, "曝光时间测试", CMOSTestLib.WaitingType.None, ""))
            {
                textBox1.AppendText("用户终止自动测试\r\n");
                toolStrip1.Enabled = true;
                return;
            }

            MessageBox.Show("请转入暗场，点击确定继续");
            waitProc = new CMOSTestLib.WaitingProc();
            waitProc.MaxProgress = SystemParam.ExposureTest_Ns;
            wpf = new CMOSTestLib.WaitingProcFunc(暗场采集);
            if (!waitProc.Execute(wpf, "曝光递进暗场采集", CMOSTestLib.WaitingType.None, ""))
            {
                textBox1.AppendText("用户终止自动测试\r\n");
                toolStrip1.Enabled = true;
                return;
            }


            waitProc = new CMOSTestLib.WaitingProc();
            if(SystemParam.cmosInfo.bRGB==0)
            {
                wpf = new CMOSTestLib.WaitingProcFunc(计算饱和输出电压_动态范围_平均暗信号_暗信号均方差);
                if (!waitProc.Execute(wpf, "计算饱和输出电压_动态范围_平均暗信号_暗信号均方差", CMOSTestLib.WaitingType.None, ""))
                {
                    textBox1.AppendText("用户终止自动测试\r\n");
                    toolStrip1.Enabled = true;
                    return;
                }
            }
            else
            {
                wpf = new CMOSTestLib.WaitingProcFunc(计算饱和输出电压_动态范围_平均暗信号_暗信号均方差);
                if (!waitProc.Execute(wpf, "灰度：计算饱和输出电压_动态范围_平均暗信号_暗信号均方差", CMOSTestLib.WaitingType.None, ""))
                {
                    textBox1.AppendText("用户终止自动测试\r\n");
                    toolStrip1.Enabled = true;
                    return;
                }
                
                wpf = new CMOSTestLib.WaitingProcFunc(RGB_计算饱和输出电压_动态范围_平均暗信号_暗信号均方差);
                if (!waitProc.Execute(wpf, "RGB：计算饱和输出电压_动态范围_平均暗信号_暗信号均方差", CMOSTestLib.WaitingType.None, ""))
                {
                    textBox1.AppendText("用户终止自动测试\r\n");
                    toolStrip1.Enabled = true;
                    return;
                }
            }
            m_Xfer.Freeze();
// 
//             计算饱和输出电压_动态范围_平均暗信号_暗信号均方差();

            //Calc1.Saturated50Index = 118 / 2;
            //Calc1.OverAllGain_K = 0.01;
            //第二步、在同一曝光时间下，采集某一光照条件下和无光照条件下的L组数据
            testStep = 2;
            InitCam(SystemParam.Step2_len + CamEx);
            m_Xfer.Grab();
            toolStripLabel3.Text = SystemParam.GetTime(Calc1.Saturated50Index).ToString("F2") + " ms";            
            SystemParam.CreateTempFile(m_Buffers.Height,m_Buffers.Width, m_Buffers.BytesPerPixel, SystemParam.L,Calc2.LightTempFile);
            SystemParam.CreateTempFile(m_Buffers.Height, m_Buffers.Width, m_Buffers.BytesPerPixel, SystemParam.L, Calc2.DarkTempFile);
            MessageBox.Show("曝光测试完成，请转入明场，点击确定进行下一步测试");
            waitProc = new CMOSTestLib.WaitingProc();
            waitProc.MaxProgress = SystemParam.L;
            /*CMOSTestLib.WaitingProcFunc*/ wpf = new CMOSTestLib.WaitingProcFunc(第二步明场采集);
            if(!waitProc.Execute(wpf, "相同曝光条件下，明场采集", CMOSTestLib.WaitingType.None, ""))
            {
                textBox1.AppendText("用户终止自动测试\r\n");
                toolStrip1.Enabled = true;
                return;
            }


            MessageBox.Show("请转入暗场，点击确定继续");
            waitProc = new CMOSTestLib.WaitingProc();
            waitProc.MaxProgress = SystemParam.L;
            wpf = new CMOSTestLib.WaitingProcFunc(第二步暗场采集);
            if (!waitProc.Execute(wpf, "相同曝光条件下，暗场采集", CMOSTestLib.WaitingType.None, ""))
            {
                textBox1.AppendText("用户终止自动测试\r\n");
                toolStrip1.Enabled = true;
                return;
            }
            
            
            waitProc = new CMOSTestLib.WaitingProc();
            waitProc.MaxProgress = SystemParam.L ;
            if(SystemParam.cmosInfo.bRGB==0)
            {
                wpf = new CMOSTestLib.WaitingProcFunc(第二步数据处理);
                if (!waitProc.Execute(wpf, "相同曝光条件下数据处理", CMOSTestLib.WaitingType.None, ""))
                {
                    textBox1.AppendText("用户终止自动测试\r\n");
                    toolStrip1.Enabled = true;
                    return;
                }
            }
            else
            {
                wpf = new CMOSTestLib.WaitingProcFunc(RGB_第二步数据处理);
                if (!waitProc.Execute(wpf, "相同曝光条件下数据处理", CMOSTestLib.WaitingType.None, ""))
                {
                    textBox1.AppendText("用户终止自动测试\r\n");
                    toolStrip1.Enabled = true;
                    return;
                }
            }
            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
            {
                textBox1.AppendText("自动测试结束,用户未保存报告\r\n");
                MessageBox.Show("CMOS测试完成");
                toolStrip1.Enabled = true;
                return;
            }
            if(SystemParam.cmosInfo.bRGB==0)
            {
                /************************************************************************/
                /* 生成报告用图片                                                        */
                /************************************************************************/
                double t;
                for (int i = 0; i < chart2.Series.Count; i++)
                {
                    chart2.Series[i].Points.Clear();
                }
                chart2.ChartAreas[0].AxisY.Title = "明场均值";
                chart2.ChartAreas[0].AxisY.Minimum = double.NaN;
                for (int i = 0; i < SystemParam.ExposureTest_Ns; i++)
                {
                    t = SystemParam.GetTime(i);
                    chart2.Series[0].Points.AddXY(t, Calc1.miu_y[i]);
                }
                chart2.SaveImage(SystemParam.TempPicPath + "1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                chart2.ChartAreas[0].AxisY.Title = "平均暗信号";
                chart2.ChartAreas[0].AxisY.Minimum = Calc1.miu_d.Min<double>();
                chart2.Series[0].Points.Clear();
                for (int i = 0; i < SystemParam.ExposureTest_Ns; i++)
                {
                    t = SystemParam.GetTime(i);
                    chart2.Series[0].Points.AddXY(t, Calc1.miu_d[i]);
                }
                chart2.SaveImage(SystemParam.TempPicPath + "2.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);


                chart2.ChartAreas[0].AxisY.Title = "暗信号均方差";
                List<double> axhjfc = new List<double>();
                for (int i = 0; i < SystemParam.ExposureTest_Ns; i++)
                {
                    axhjfc.Add(Math.Sqrt(Calc1.delta_y_dark[i]) / Calc1.OverAllGain_K);
                }
                chart2.Series[0].Points.Clear();
                chart2.ChartAreas[0].AxisY.Minimum = axhjfc.Min<double>();
                for (int i = 0; i < SystemParam.ExposureTest_Ns; i++)
                {
                    t = SystemParam.GetTime(i);
                    chart2.Series[0].Points.AddXY(t, axhjfc[i]);
                }
                chart2.SaveImage(SystemParam.TempPicPath + "3.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                chart2.ChartAreas[0].AxisY.Title = "信噪比";
                chart2.Series[0].Points.Clear();
                chart2.ChartAreas[0].AxisY.Minimum = double.NaN;
                for (int i = 0; i < SystemParam.ExposureTest_Ns; i++)
                {
                    t = SystemParam.GetTime(i);
                    if (Calc1.SNR[i] == double.MaxValue)
                    {
                        int p = chart2.Series[0].Points.AddXY(t, 0);
                        chart2.Series[0].Points[p].IsEmpty = true;
                    }
                    else
                        chart2.Series[0].Points.AddXY(t, Calc1.SNR[i]);
                }
                chart2.SaveImage(SystemParam.TempPicPath + "4.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                TestReport.MakeReport(saveFileDialog1.FileName);
            }
            else
            {
                /************************************************************************/
                /* 生成报告用图片                                                        */
                /************************************************************************/
                double t;
                for (int i = 0; i < chart2.Series.Count; i++)
                {
                    chart2.Series[i].Points.Clear();
                }
                chart2.ChartAreas[0].AxisY.Title = "明场均值";
                chart2.ChartAreas[0].AxisY.Minimum = double.NaN;
                chart2.Series[0].Color = System.Drawing.Color.Red; 
                for (int i = 0; i < Calc1.R_miu_y.Count; i++)
                {
                    t = SystemParam.GetTime(i);
                    chart2.Series[0].Points.AddXY(t, Calc1.R_miu_y[i]);
                }
                chart2.SaveImage(SystemParam.TempPicPath + "1_R.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                chart2.Series[0].Points.Clear();
                chart2.Series[0].Color = System.Drawing.Color.Lime; 
                for (int i = 0; i < Calc1.G_miu_y.Count; i++)
                {
                    t = SystemParam.GetTime(i);
                    chart2.Series[0].Points.AddXY(t, Calc1.G_miu_y[i]);
                }
                chart2.SaveImage(SystemParam.TempPicPath + "1_G.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                chart2.Series[0].Points.Clear();
                chart2.Series[0].Color = System.Drawing.Color.Blue; 
                for (int i = 0; i < Calc1.B_miu_y.Count; i++)
                {
                    t = SystemParam.GetTime(i);
                    chart2.Series[0].Points.AddXY(t, Calc1.B_miu_y[i]);
                }
                chart2.SaveImage(SystemParam.TempPicPath + "1_B.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                /************************************************************************/
                /*                                                                      */
                /************************************************************************/
                chart2.ChartAreas[0].AxisY.Title = "平均暗信号";                
                chart2.Series[0].Points.Clear();
                chart2.Series[0].Color = System.Drawing.Color.Red;
                chart2.ChartAreas[0].AxisY.Minimum = Calc1.R_miu_d.Min<double>();
                for (int i = 0; i < Calc1.R_miu_d.Count; i++)
                {
                    t = SystemParam.GetTime(i);
                    chart2.Series[0].Points.AddXY(t, Calc1.R_miu_d[i]);
                }
                chart2.SaveImage(SystemParam.TempPicPath + "2_R.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                chart2.Series[0].Points.Clear();
                chart2.Series[0].Color = System.Drawing.Color.Lime;
                chart2.ChartAreas[0].AxisY.Minimum = Calc1.G_miu_d.Min<double>();
                for (int i = 0; i < Calc1.G_miu_d.Count; i++)
                {
                    t = SystemParam.GetTime(i);
                    chart2.Series[0].Points.AddXY(t, Calc1.G_miu_d[i]);
                }
                chart2.SaveImage(SystemParam.TempPicPath + "2_G.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                chart2.Series[0].Points.Clear();
                chart2.Series[0].Color = System.Drawing.Color.Blue;
                chart2.ChartAreas[0].AxisY.Minimum = Calc1.B_miu_d.Min<double>();
                for (int i = 0; i < Calc1.B_miu_d.Count; i++)
                {
                    t = SystemParam.GetTime(i);
                    chart2.Series[0].Points.AddXY(t, Calc1.B_miu_d[i]);
                }
                chart2.SaveImage(SystemParam.TempPicPath + "2_B.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                /************************************************************************/
                /*                                                                      */
                /************************************************************************/


                chart2.ChartAreas[0].AxisY.Title = "暗信号均方差";
                chart2.Series[0].Color = System.Drawing.Color.Red; 
                List<double> axhjfc = new List<double>();
                for (int i = 0; i < Calc1.R_delta_y_dark.Count; i++)
                {
                    axhjfc.Add(Math.Sqrt(Calc1.R_delta_y_dark[i]) / Calc1.R_OverAllGain_K);
                }
                chart2.Series[0].Points.Clear();
                chart2.ChartAreas[0].AxisY.Minimum = axhjfc.Min<double>();
                for (int i = 0; i < axhjfc.Count; i++)
                {
                    t = SystemParam.GetTime(i);
                    chart2.Series[0].Points.AddXY(t, axhjfc[i]);
                }
                chart2.SaveImage(SystemParam.TempPicPath + "3_R.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                axhjfc = new List<double>();
                for (int i = 0; i < Calc1.G_delta_y_dark.Count; i++)
                {
                    axhjfc.Add(Math.Sqrt(Calc1.G_delta_y_dark[i]) / Calc1.G_OverAllGain_K);
                }
                chart2.Series[0].Points.Clear();
                chart2.ChartAreas[0].AxisY.Minimum = axhjfc.Min<double>();
                chart2.Series[0].Color = System.Drawing.Color.Lime; 
                for (int i = 0; i < axhjfc.Count; i++)
                {
                    t = SystemParam.GetTime(i);
                    chart2.Series[0].Points.AddXY(t, axhjfc[i]);
                }
                chart2.SaveImage(SystemParam.TempPicPath + "3_G.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                axhjfc = new List<double>();
                for (int i = 0; i < Calc1.B_delta_y_dark.Count; i++)
                {
                    axhjfc.Add(Math.Sqrt(Calc1.B_delta_y_dark[i]) / Calc1.B_OverAllGain_K);
                }
                chart2.Series[0].Points.Clear();
                chart2.ChartAreas[0].AxisY.Minimum = axhjfc.Min<double>();
                chart2.Series[0].Color = System.Drawing.Color.Blue; 
                for (int i = 0; i < axhjfc.Count; i++)
                {
                    t = SystemParam.GetTime(i);
                    chart2.Series[0].Points.AddXY(t, axhjfc[i]);
                }
                chart2.SaveImage(SystemParam.TempPicPath + "3_B.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                /************************************************************************/
                /*                                                                      */
                /************************************************************************/

                chart2.ChartAreas[0].AxisY.Title = "信噪比";
                chart2.Series[0].Points.Clear();
                chart2.Series[0].Color = System.Drawing.Color.Red; 
                chart2.ChartAreas[0].AxisY.Minimum = double.NaN;
                for (int i = 0; i < Calc1.R_SNR.Count; i++)
                {
                    t = SystemParam.GetTime(i);
                    if (Calc1.R_SNR[i] == double.MaxValue)
                    {
                        int p = chart2.Series[0].Points.AddXY(t, 0);
                        chart2.Series[0].Points[p].IsEmpty = true;
                    }
                    else
                        chart2.Series[0].Points.AddXY(t, Calc1.R_SNR[i]);
                }
                chart2.SaveImage(SystemParam.TempPicPath + "4_R.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                chart2.Series[0].Points.Clear();
                chart2.ChartAreas[0].AxisY.Minimum = double.NaN;
                chart2.Series[0].Color = System.Drawing.Color.Lime; 
                for (int i = 0; i < Calc1.G_SNR.Count; i++)
                {
                    t = SystemParam.GetTime(i);
                    if (Calc1.G_SNR[i] == double.MaxValue)
                    {
                        int p = chart2.Series[0].Points.AddXY(t, 0);
                        chart2.Series[0].Points[p].IsEmpty = true;
                    }
                    else
                        chart2.Series[0].Points.AddXY(t, Calc1.G_SNR[i]);
                }
                chart2.SaveImage(SystemParam.TempPicPath + "4_G.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                chart2.Series[0].Points.Clear();
                chart2.ChartAreas[0].AxisY.Minimum = double.NaN;
                chart2.Series[0].Color = System.Drawing.Color.Blue; 
                for (int i = 0; i < Calc1.B_SNR.Count; i++)
                {
                    t = SystemParam.GetTime(i);
                    if (Calc1.B_SNR[i] == double.MaxValue)
                    {
                        int p = chart2.Series[0].Points.AddXY(t, 0);
                        chart2.Series[0].Points[p].IsEmpty = true;
                    }
                    else
                        chart2.Series[0].Points.AddXY(t, Calc1.B_SNR[i]);
                }
                chart2.SaveImage(SystemParam.TempPicPath + "4_B.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                TestReport.RGBMakeReport(saveFileDialog1.FileName);
            }
            textBox1.AppendText("自动测试结束,测试报告保存位置为:\r\n");
            textBox1.AppendText(saveFileDialog1.FileName);
            MessageBox.Show("CMOS测试完成");
            toolStrip1.Enabled = true;
        }
        int testStep;
        void InitSystemParam()
        {
            iniFileOP.iniFilePath = System.Windows.Forms.Application.StartupPath + "\\Config.ini";

            SystemParam.ExposureTest_Ns = int.Parse(iniFileOP.Read("System Setting", "ExposureTest_Ns"));
            SystemParam.ExposurePointThreshold = double.Parse(iniFileOP.Read("System Setting", "ExposurePointThreshold"));

            SystemParam.L = ushort.Parse(iniFileOP.Read("System Setting", "L"));
            SystemParam.Step2_len = ushort.Parse(iniFileOP.Read("System Setting", "Step2_len"));
            SystemParam.DarkPointPer = double.Parse(iniFileOP.Read("System Setting", "DarkPointPer"));
            SystemParam.LightPointPer = double.Parse(iniFileOP.Read("System Setting", "LightPointPer"));

            SystemParam.eStart = uint.Parse(iniFileOP.Read("System Run", "eStart")); 
            SystemParam.eStep = uint.Parse(iniFileOP.Read("System Run", "eStep"));
            SystemParam.DeviceID = iniFileOP.Read("System Run", "DeviceID");

            Calc1.k1 = int.Parse(iniFileOP.Read("System Setting", "k1"));
            Calc1.k2 = int.Parse(iniFileOP.Read("System Setting", "k2"));

            Calc1.percent_base = double.Parse(iniFileOP.Read("System Setting", "percent_base"));
            Calc1.percent = double.Parse(iniFileOP.Read("System Setting", "percent"));

            SystemParam.PicDelay = int.Parse(iniFileOP.Read("System Setting", "PicDelay"));
			SystemParam.WaitTimeOut = int.Parse(iniFileOP.Read("System Setting", "WaitTimeOut"));

            SystemParam.cmosInfo = new CMOSInfo();
//             SystemParam.cmosInfo.RowPixels = int.Parse(iniFileOP.Read("CMOS Param", "RowPixels"));
//             SystemParam.cmosInfo.ColPixels = int.Parse(iniFileOP.Read("CMOS Param", "ColPixels"));
//             SystemParam.cmosInfo.PixelDepth = int.Parse(iniFileOP.Read("CMOS Param", "PixelDepth"));
            SystemParam.cmosInfo.PixelArea = int.Parse(iniFileOP.Read("CMOS Param", "PixelArea"));
            //SystemParam.cmosInfo.Ts = double.Parse(iniFileOP.Read("CMOS Param", "Ts"));
            SystemParam.cmosInfo.Lambda = int.Parse(iniFileOP.Read("CMOS Param", "Lambda"));
            SystemParam.cmosInfo.bRGB = int.Parse(iniFileOP.Read("CMOS Param", "bRGB"));
            SystemParam.cmosInfo.RGB1 = int.Parse(iniFileOP.Read("CMOS Param", "RGB1"));
            SystemParam.cmosInfo.RGB2 = int.Parse(iniFileOP.Read("CMOS Param", "RGB2"));
            SystemParam.cmosInfo.RGB3 = int.Parse(iniFileOP.Read("CMOS Param", "RGB3"));
            SystemParam.cmosInfo.RGB4 = int.Parse(iniFileOP.Read("CMOS Param", "RGB4"));

            //SystemParam.Ts = SystemParam.cmosInfo.Ts / 1000 / 1000;//ns
            //SystemParam.Pixel4Pic = (int)SystemParam.cmosInfo.ColPixels * SystemParam.cmosInfo.RowPixels;

            SystemParam.eInfo = new EInfo();
            SystemParam.eInfo.Rf = double.Parse(iniFileOP.Read("E Calc", "Rf"));
            SystemParam.eInfo.rho = double.Parse(iniFileOP.Read("E Calc", "rho"));
            SystemParam.eInfo.S = double.Parse(iniFileOP.Read("E Calc", "S"));
            
            SystemParam.ccfPath = iniFileOP.Read("CMOS Param", "ccfPath"); ;
//             iniFileOP.GetAllKeyValues("System Setting", out keys, out values);
// 
//             iniFileOP.Write("System Param", "DarkPointPer", "20");
// 
            
//             SystemParam.ExposureTest_Ns = 150;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            InitSystemParam();
//            TestReport.MakeReport();
            //this.Controls.Add(this.m_ImageBox);
            AcqConfigDlg acConfigDlg = new AcqConfigDlg(null, "", AcqConfigDlg.ServerCategory.ServerAcq);
            if (acConfigDlg.InitServerCombo())
                m_online = true;
            else
                m_online = false;
            if (m_online)
            {
                //SerialFunc.SerialPortName = "COM2";
                if (!SerialFunc.OpenSerialPort())
                    this.Close();
            }
            else
            {
                toolStripButton3.Enabled = false;
                toolStripButton6.Enabled = false;
                toolStripButton8.Enabled = false;
                toolStripButton7.Enabled = false;
                m_Buffers = new SapBuffer(1, 600 , 480, SapFormat.Mono16, SapBuffer.MemoryType.ScatterGather);
                //m_View = new SapView(m_Buffers);
                StatusLabelInfo.Text = "offline... Load images";
                float WidthScalor = (float)(splitContainer1.Panel2.Size.Width) / m_Buffers.Width;
                float HeightScalor = (float)(splitContainer1.Panel2.Size.Height) / m_Buffers.Height;
                //m_View.SetScalingMode(WidthScalor, HeightScalor);
            }            
            //m_ImageBox.View = m_View;

            if (!CreateObjects())
            {
                DisposeObjects();
                return;
            }
            //m_ImageBox.OnSize();
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                listView1.Items[i].SubItems.Add("");
                listView1.Items[i].SubItems.Add("");
            }
            //EnableSignalStatus();
        }
        void InitListView()
        {
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                listView1.Items[i].SubItems[1].Text="";
                listView1.Items[i].SubItems[2].Text="";
            }
        }
        //ImageBox m_ImageBox;
        private SapAcquisition m_Acquisition;
        private SapBuffer m_Buffers;
        private SapAcqToBuf m_Xfer;
        //private SapView m_View;
        private bool m_IsSignalDetected;
        private bool m_online;
        private SapLocation m_ServerLocation;
        private string m_ConfigFileName;
        private delegate void DisplayFrameAcquired(int number, bool trash);
        static void xfer_XferNotify(object sender, SapXferNotifyEventArgs argsNotify)
        {
            Form1 GrabDlg = argsNotify.Context as Form1;
            // If grabbing in trash buffer, do not display the image, update the
            // appropriate number of frames on the status bar instead
            Debug.WriteLine(DateTime.Now.ToString() + ":" + argsNotify.EventCount.ToString());
            if (GrabDlg.testStep==1)
            {
                if (argsNotify.Trash)
                    GrabDlg.Invoke(new DisplayFrameAcquired(GrabDlg.ShowFrameNumber), argsNotify.EventCount, true);
                // Refresh view
                else
                {
                    GrabDlg.Invoke(new DisplayFrameAcquired(GrabDlg.TestStep1), argsNotify.EventCount, false);
                    //GrabDlg.m_View.Show();
                }
            }
            else if (GrabDlg.testStep == 2)
            {
                if (argsNotify.Trash)
                    GrabDlg.Invoke(new DisplayFrameAcquired(GrabDlg.ShowFrameNumber), argsNotify.EventCount, true);
                // Refresh view
                else
                {
                    GrabDlg.Invoke(new DisplayFrameAcquired(GrabDlg.TestStep2), argsNotify.EventCount, false);
                    //GrabDlg.m_View.Show();
                }
            }
            else if (GrabDlg.testStep == 3)
            {
                if (argsNotify.Trash)
                    GrabDlg.Invoke(new DisplayFrameAcquired(GrabDlg.ShowFrameNumber), argsNotify.EventCount, true);
                // Refresh view
                else
                {
                    GrabDlg.Invoke(new DisplayFrameAcquired(GrabDlg.FPNTest), argsNotify.EventCount, false);
                    //GrabDlg.m_View.Show();
                }
            }
        }
        static void GetSignalStatus(object sender, SapSignalNotifyEventArgs argsSignal)
        {
            Form1 GrabDlg = argsSignal.Context as Form1;
            SapAcquisition.AcqSignalStatus signalStatus = argsSignal.SignalStatus;
            GrabDlg.Invoke((EventHandler)(delegate
            {
                GrabDlg.m_IsSignalDetected = (signalStatus != SapAcquisition.AcqSignalStatus.None);
                if (GrabDlg.m_IsSignalDetected == false)
                    GrabDlg.StatusLabelInfo.Text = "Online... No camera signal detected";
                else GrabDlg.StatusLabelInfo.Text = "Online... Camera signal detected";
            }));
            
        }
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
                textBox1.AppendText((m_Buffers.Index+1).ToString() + "\r\n");
                str = String.Format("Frames acquired :{0}", number * m_Buffers.Count);
                this.StatusLabelInfo.Text = str;                
            }
        }
        List<byte[]> AllList=new List<byte[]>();
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
        

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            
        }

//         private void Form1_SizeChanged(object sender, EventArgs e)
//         {
//             lTest.Text = splitContainer1.Panel2.Size.Height.ToString() + "_" + splitContainer1.Panel2.Size.Width.ToString();
//             m_ImageBox.OnSize();
//         }
// 
//         private void toolStripButton4_Click(object sender, EventArgs e)
//         {
//             LoadSaveDlg newDialogLoad = new LoadSaveDlg(m_Buffers, true, false);
//             // Show the dialog and process the result
//             newDialogLoad.ShowDialog();
//             newDialogLoad.Dispose();
//             m_ImageBox.Refresh(); 
//         }
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            Random random = new Random();
            int t = 0;
            for (int pointIndex = 0; pointIndex < 13; pointIndex++)
            {
                chart1.Series[0].Points.AddXY(t, random.Next(5, 1000));
                chart1.Series[1].Points.AddXY(t, random.Next(8000, 8200));                
            }
        }
        
        void InitCam(int n)
        {
            while (true)
            {
                try
                {
                    DestroyObjects();
                    DisposeObjects();
                    if (m_online)
                    {
                        m_ServerLocation = new SapLocation("X64-CL_iPro_1", 0);
                        m_ConfigFileName = SystemParam.ccfPath;//@"C:\Program Files\Teledyne DALSA\Sapera\CamFiles\User\w512x512.ccf";
                        // define on-line object
                        m_Acquisition = new SapAcquisition(m_ServerLocation, m_ConfigFileName);

                        m_Buffers = new SapBufferWithTrash(n, m_Acquisition, SapBuffer.MemoryType.ScatterGather);


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
                    }
                    else
                    {
                        m_Buffers = new SapBuffer(1, 600, 480, SapFormat.Mono16, SapBuffer.MemoryType.ScatterGather);
                        //                 m_View = new SapView(m_Buffers);
                        //                 m_View.SetScalingMode(true);
                        StatusLabelInfo.Text = "offline... Load images";
                    }

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
//                     SystemParam.cmosInfo.PixelDepth = m_Buffers.PixelDepth;
//                     SystemParam.cmosInfo.ColPixels = m_Buffers.Height;
//                     SystemParam.cmosInfo.RowPixels = m_Buffers.Width;
                    if (SystemParam.cmosInfo.bRGB != 0)
                    {
                        wfSapGUI.GetRGBPixelInfo(m_Buffers.Width, m_Buffers.Height, SystemParam.cmosInfo.RGB1, SystemParam.cmosInfo.RGB2, SystemParam.cmosInfo.RGB3, SystemParam.cmosInfo.RGB4);
                    }
                    SystemParam.Pixel4Pic = m_Buffers.Height*m_Buffers.Width;//(int)SystemParam.cmosInfo.ColPixels * SystemParam.cmosInfo.RowPixels;
                    float WidthScalor = (float)(splitContainer1.Panel2.Size.Width) / m_Buffers.Width;
                    float HeightScalor = (float)(splitContainer1.Panel2.Size.Height) / m_Buffers.Height;
                    //m_View.SetScalingMode(WidthScalor, HeightScalor);
                    //m_ImageBox.OnSize();
                    EnableSignalStatus();
                    SystemParam.ByteLen4Pic = SystemParam.Pixel4Pic * m_Buffers.BytesPerPixel;
                    m_Xfer.Freeze();
                    return;
                }
                catch
                {
                }
            }
        }    

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            StatusLabelInfoTrash.Text = "";
            m_Xfer.Grab();
        }

//         private void toolStripButton7_Click(object sender, EventArgs e)
//         {
//             LoadSaveDlg newDialogLoad = new LoadSaveDlg(m_Buffers, true, false);
//             // Show the dialog and process the result
//             newDialogLoad.ShowDialog();
//             newDialogLoad.Dispose();
//             m_ImageBox.Refresh(); 
//         }

        private void splitContainer1_Panel2_SizeChanged(object sender, EventArgs e)
        {
//             if (m_View != null)
//             {
//                 float WidthScalor = (float)(splitContainer1.Panel2.Size.Width) / m_Buffers.Width;
//                 float HeightScalor = (float)(splitContainer1.Panel2.Size.Height) / m_Buffers.Height;
//                 m_View.SetScalingMode(WidthScalor, HeightScalor);
//                 if (m_ImageBox != null)
//                     m_ImageBox.OnSize();
//             }
            
        }        
        private void 设置系统参数_Click_1(object sender, EventArgs e)
        {
            Form3 f = new Form3();
            f.ShowDialog();
        }

        private void toolStripButton2_Click_1(object sender, EventArgs e)
        {
            Random random = new Random();
            int t = 0;
            for (int pointIndex = 0; pointIndex < 13; pointIndex++)
            {
                chart2.Series[0].Points.AddXY(t, random.Next(5, 1000));
            }
            chart2.SaveImage("1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            chart2.SaveImage("1.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            chart2.SaveImage("1.tif", System.Drawing.Imaging.ImageFormat.Tiff);
//             chart2.SaveImage("1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
//             chart2.SaveImage("1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        private void toolStripButton4_Click_1(object sender, EventArgs e)
        {
            SystemParam.Ts = 6.25/1000000;
            double t;
            for (int i = 0; i < chart2.Series.Count; i++)
            {
                chart2.Series[i].Points.Clear();
            }
            for (int i = 0; i < SystemParam.ExposureTest_Ns; i++)
            {
                t = SystemParam.GetTime(i);
                chart2.Series[0].Points.AddXY(t, i);
            }
            chart2.SaveImage(SystemParam.TempPicPath + "1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

            chart2.ChartAreas[0].AxisY.Title = "平均暗信号";
            Calc1.miu_d = new List<double>();
            Random r = new Random();
            for (int i = 0; i < SystemParam.ExposureTest_Ns; i++)
            {
                Calc1.miu_d.Add(r.Next(1000, 1100));
            }
            chart2.Series[0].Points.Clear();
            chart2.ChartAreas[0].AxisY.Minimum = Calc1.miu_d.Min<double>();            
            for (int i = 0; i < SystemParam.ExposureTest_Ns; i++)
            {
                t = SystemParam.GetTime(i);
                chart2.Series[0].Points.AddXY(t, Calc1.miu_d[i]);
            }
            chart2.SaveImage(SystemParam.TempPicPath + "2.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);


            chart2.ChartAreas[0].AxisY.Title = "暗信号均方差";
            chart2.ChartAreas[0].AxisY.Minimum = double.NaN;
            chart2.Series[0].Points.Clear();
            for (int i = 0; i < SystemParam.ExposureTest_Ns; i++)
            {
                t = SystemParam.GetTime(i);
                chart2.Series[0].Points.AddXY(t, i);
            }
            chart2.SaveImage(SystemParam.TempPicPath + "3.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

            chart2.ChartAreas[0].AxisY.Title = "信噪比";
            chart2.Series[0].Points.Clear();
            for (int i = 0; i < SystemParam.ExposureTest_Ns; i++)
            {
                t = SystemParam.GetTime(i);
                chart2.Series[0].Points.AddXY(t,i);
            }
            chart2.SaveImage(SystemParam.TempPicPath + "4.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            saveFileDialog1.ShowDialog();
            TestReport.MakeReport(saveFileDialog1.FileName);
        }

        private void 手动拍摄_Click_1(object sender, EventArgs e)
        {
            SerialFunc.CloseSerialPort();
            DestroyObjects();
            DisposeObjects();
            Form4 f = new Form4();
            f.ShowDialog();
            if (!SerialFunc.OpenSerialPort())
                this.Close();
        }

        private void 自定义暗场测试_Click(object sender, EventArgs e)
        {
            SerialFunc.CloseSerialPort();
            DestroyObjects();
            DisposeObjects();
            Form5 f = new Form5();
            f.ShowDialog();
            if (!SerialFunc.OpenSerialPort())
                this.Close();
        }
        double t=0, y=0, d=0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            t += 10;
            y += 10;
            d += 10;
            chart1.Series["Gray_miu"].Points.AddXY(t, y);
            chart1.Series["Gray_delta"].Points.AddXY(t, d);
        }        
    }
}
