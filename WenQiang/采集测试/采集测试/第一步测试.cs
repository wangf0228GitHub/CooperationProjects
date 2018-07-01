using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CMOSTestLib;
using CMOSTestLib.SaperaGUI;
using DALSA.SaperaLT.SapClassBasic;
using WFNetLib.Algorithm;
using WFNetLib;
using System.Threading;
using WFNetLib.Log;

namespace 采集测试
{
    public partial class Form1 : Form
    {
        //uint maxLs = 0xffff00;
        int rxFrame;
        bool bTrask;
        int WaitMsTime;
        bool bWaitTimeOut;
        private void TestStep1(int number, bool trash)
        {
            String str;
            if (trash)
            {
                str = String.Format("Frames acquired in trash buffer: {0}", number * m_Buffers.Count);
                this.StatusLabelInfoTrash.Text = str;
                //bTrask = true;
            }
            else
            {                
                rxFrame++;
                lock (oWaitMsTime)
                {
                    WaitMsTime=0;
                }
                //textBox1.AppendText((m_Buffers.Index + 1).ToString() + "\r\n");
                str = String.Format("Frames acquired :{0}", number * m_Buffers.Count);
                this.StatusLabelInfo.Text = str;
            }
        }
        object oWaitMsTime = new object();
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
                if (WaitMsTime > SystemParam.WaitTimeOut)
                {
                    bWaitTimeOut = true;
                    this.Invoke((EventHandler)(delegate
                    {
                        textBox1.AppendText("采集图片超时重试\r\n");
                    }));
                    break;
                }
            }
            if (bTrask)
            {
                WFNetLib.WFGlobal.WaitMS(1000);
                return false;
            }
            if (bWaitTimeOut)
            {
                return false;
            }
            return true;
        }
        void NopCam(ushort count,uint ls)
        {            
            while(true)
            {
                SerialFunc.SerialCommand3(count, ls);
                if (WaitCam(count))
                    break;
            }
            WFGlobal.WaitMS(200);
        }
        byte[] ya, yb;
        //byte[] yra, yrb, yga, ygb, yba, ybb;
        void 曝光测试(object LockWatingThread)
        {                       
            double y, d;
            while (true)
            {
                waitProc.SetProcessBar(0);
                Calc1.TList = new List<double>();
                Calc1.EList = new List<double>();
                Calc1.miu_y = new List<double>();
                Calc1.delta_y = new List<double>();
                Calc1.R_miu_y = new List<double>();
                Calc1.G_miu_y = new List<double>();
                Calc1.B_miu_y = new List<double>();
                Calc1.R_delta_y = new List<double>();
                Calc1.G_delta_y = new List<double>();
                Calc1.B_delta_y = new List<double>();
                this.Invoke((EventHandler)(delegate
                {
                    listView1.Items[1].SubItems[1].Text = "";
                    listView1.Items[1].SubItems[2].Text = "";
                    chart1.ChartAreas[0].AxisY.Title = "明场均值";
                    chart1.ChartAreas[1].AxisY.Title = "明场方差";  
                    chart1.Series["Gray_miu"].Points.Clear();
                    chart1.Series["Gray_delta"].Points.Clear();
                    if (SystemParam.cmosInfo.bRGB != 0)
                    {
                        chart1.Series["R_miu"].Points.Clear();
                        chart1.Series["R_delta"].Points.Clear();
                        chart1.Series["G_miu"].Points.Clear();
                        chart1.Series["G_delta"].Points.Clear();
                        chart1.Series["B_miu"].Points.Clear();
                        chart1.Series["B_delta"].Points.Clear();
                    }
                    textBox1.AppendText("开始明场曝光测试\r\n");
                }));
                double t;
                //NopCam(3, SystemParam.eStart);
                int saveindex = 0;
                for (uint i = 0; i < SystemParam.ExposureTest_Ns; i++)
                {
                    /************************************************************************/
                    /* 环境信息                                                             */
                    /************************************************************************/                    
                    EnvironmentInfo ei = SerialFunc.SerialCommand2();
                    if (ei == null)
                    {
                        this.Invoke((EventHandler)(delegate
                        {
                            textBox1.AppendText("环境信息采集失败\r\n");
                        }));
                    }
                    else
                    {
                        double Temperature;
                        double E;
                        string str111 = "完成环境信息采集:";
                        //listView1.Items[0].SubItems[1].Text = "完成";
                        if (BytesOP.GetBit(ei.Temp, 15))//负温度
                        {
                            Temperature = ((double)(ei.Temp) - 65536) / 128;
                        }
                        else
                        {
                            Temperature = ((double)(ei.Temp)) / 128;
                        }
                        str111 += "T=" + Temperature.ToString("F1");
                        Calc1.TList.Add(Temperature);
                        Temperature = 0; 
                        for (int j = 0; j < Calc1.TList.Count; j++)
                        {
                            Temperature += Calc1.TList[j];
                        }
                        Temperature = Temperature / Calc1.TList.Count;
                        
                        E = (double)ei.E*3/4096 / SystemParam.eInfo.Rf / SystemParam.eInfo.rho / SystemParam.eInfo.S/100;
                        str111 += ",E=" + E.ToString("F1");
                        Calc1.EList.Add(E);
                        E = 0;
                        for (int j = 0; j < Calc1.EList.Count; j++)
                        {
                            E += Calc1.EList[j];
                        }
                        E = E / Calc1.EList.Count;
                        this.Invoke((EventHandler)(delegate
                        {
                            listView1.Items[0].SubItems[2].Text = Temperature.ToString("F1") + "℃";
                            listView1.Items[1].SubItems[2].Text = E.ToString("F1");// +"℃";
                            textBox1.AppendText(str111+"\r\n");
                         }));
                    }                    
                    /************************************************************************/
                    /* 采集图片                                                             */
                    /************************************************************************/
                    uint ls = SystemParam.eStart + i * SystemParam.eStep;                    
                    if (waitProc.HasBeenCancelled())
                    {
                        return;
                    }
                    this.Invoke((EventHandler)(delegate
                    {
                        toolStripLabel3.Text = SystemParam.GetTime((int)i).ToString("F2")+" ms";
                    }));
                    SerialFunc.SerialCommand3((ushort)(2 + CamEx), ls);
                    if (!WaitCam(2 + CamEx))
                    {
                        i--;
                        continue;
                    }
                    saveindex++;
                    m_Buffers.Save(Calc1.TempPicPath_Light+saveindex.ToString() + ".bmp", "-format bmp", 0+CamEx, 0);
                    saveindex++;
                    m_Buffers.Save(Calc1.TempPicPath_Light + saveindex.ToString() + ".bmp", "-format bmp", 1 + CamEx, 0);
                    Thread.Sleep(SystemParam.PicDelay);
                    ya = wfSapGUI.ReadPicDatas(m_Buffers, 0 + CamEx);
                    yb = wfSapGUI.ReadPicDatas(m_Buffers, 1 + CamEx);
//                     SystemParam.WriteTempFile(ya, 0, "1.bin");
//                     SystemParam.WriteTempFile(yb, 0, "2.bin");
                    Calc1.TestExposureTime(ya, yb, m_Buffers.Height, m_Buffers.Width, m_Buffers.PixelDepth, out y, out d);
					Calc1.miu_y.Add(y);
                    Calc1.delta_y.Add(d);
                    if (SystemParam.cmosInfo.bRGB != 0)
                    {
                        Calc1.Get_miu_delta(true, ya, yb, m_Buffers.Height, m_Buffers.Width, m_Buffers.PixelDepth, SystemParam.cmosInfo.RGB1, SystemParam.cmosInfo.RGB2, SystemParam.cmosInfo.RGB3, SystemParam.cmosInfo.RGB4);
                    }
                    t = ls * SystemParam.Ts;
                    this.Invoke((EventHandler)(delegate
                    {
                        chart1.Series["Gray_miu"].Points.AddXY(t, y);
                        chart1.Series["Gray_delta"].Points.AddXY(t, d);
                        if (SystemParam.cmosInfo.bRGB != 0)
                        {
                            chart1.Series["R_miu"].Points.AddXY(t, Calc1.R_miu_y.Last());
                            chart1.Series["R_delta"].Points.AddXY(t,Calc1.R_delta_y.Last());
                            chart1.Series["G_miu"].Points.AddXY(t, Calc1.G_miu_y.Last());
                            chart1.Series["G_delta"].Points.AddXY(t, Calc1.G_delta_y.Last());
                            chart1.Series["B_miu"].Points.AddXY(t, Calc1.B_miu_y.Last());
                            chart1.Series["B_delta"].Points.AddXY(t, Calc1.B_delta_y.Last());
                        }
                        listView1.Items[2].SubItems[1].Text = (((double)(i + 1))*100 / SystemParam.ExposureTest_Ns).ToString("F1") + "%";
                    }));
                    waitProc.SetProcessBar((int)(i + 1));                    
                }   
                List<double> deltaD=new List<double>();
                for (int i = 0; i < SystemParam.ExposureTest_Ns-1; i++)
                {
                    deltaD.Add(Calc1.delta_y[i + 1] - Calc1.delta_y[i]);
                }
                //Calc1.saturation = 0;
				Calc1.saturation = SystemParam.ExposureTest_Ns - 1;
				/************************************************************************/
                /*                                                                      */
                /************************************************************************/
                //饱和点查找
				for (int i = 0; i < SystemParam.ExposureTest_Ns - 5; i++)
				{
					if (deltaD[i] <= 0 && deltaD[i + 1] <= 0 && deltaD[i + 2] <= 0 && deltaD[i + 3] <= 0)
					{
						Calc1.saturation = i;
						break;
					}
				}
                /************************************************************************/
                /*                                                                      */
                /************************************************************************/
                double SaturatedIPer = (double)Calc1.saturation / SystemParam.ExposureTest_Ns;
                this.Invoke((EventHandler)(delegate
                {
                    chart1.Series["Gray_miu"].Points[Calc1.saturation].MarkerSize = 20;
                    chart1.Series["Gray_delta"].Points[Calc1.saturation].MarkerSize = 20;
                    chart1.Series["Gray_delta"].Points[Calc1.saturation].Label = (SaturatedIPer).ToString("F2");
                    chart1.Series["Gray_delta"].Points[Calc1.saturation].IsValueShownAsLabel = true;
                    if (SystemParam.cmosInfo.bRGB != 0)
                    {
                        chart1.Series["R_miu"].Points[Calc1.saturation].MarkerSize = 20;
                        chart1.Series["R_delta"].Points[Calc1.saturation].MarkerSize = 20;
                        chart1.Series["R_delta"].Points[Calc1.saturation].Label = (SaturatedIPer).ToString("F2");
                        chart1.Series["R_delta"].Points[Calc1.saturation].IsValueShownAsLabel = true;
                        
                        chart1.Series["G_miu"].Points[Calc1.saturation].MarkerSize = 20;
                        chart1.Series["G_delta"].Points[Calc1.saturation].MarkerSize = 20;
                        chart1.Series["G_delta"].Points[Calc1.saturation].Label = (SaturatedIPer).ToString("F2");
                        chart1.Series["G_delta"].Points[Calc1.saturation].IsValueShownAsLabel = true;

                        chart1.Series["G_miu"].Points[Calc1.saturation].MarkerSize = 20;
                        chart1.Series["G_delta"].Points[Calc1.saturation].MarkerSize = 20;
                        chart1.Series["G_delta"].Points[Calc1.saturation].Label = (SaturatedIPer).ToString("F2");
                        chart1.Series["G_delta"].Points[Calc1.saturation].IsValueShownAsLabel = true;
                    }
                }));
                this.Invoke((EventHandler)(delegate
                {
                    if (SaturatedIPer == (SystemParam.ExposureTest_Ns - 1) / SystemParam.ExposureTest_Ns)
                    {
                        textBox1.AppendText("曝光步长:" + (((double)SystemParam.eStep) * SystemParam.Ts).ToString("F2") + "ms,未找到方差极值点\n");
                    }
                    else
                    {
                        textBox1.AppendText("曝光步长:" + (((double)SystemParam.eStep) * SystemParam.Ts).ToString("F2") + "ms,方差极值点当前比例:" + SaturatedIPer.ToString("F2") + "\r\n");
                    }
					
					textBox1.AppendText("均值输出的最后两个值为：" + (Calc1.miu_y[SystemParam.ExposureTest_Ns - 2]).ToString("F3") + " , " + (Calc1.miu_y[SystemParam.ExposureTest_Ns - 1]).ToString("F3") + "\n");
					textBox1.AppendText("方差输出的最后两个值为：" + (Calc1.delta_y[SystemParam.ExposureTest_Ns - 2]).ToString("F3") + " , " + (Calc1.delta_y[SystemParam.ExposureTest_Ns - 1]).ToString("F3") + "\n");

                }));
                if (Calc1.CheckSaturatedIndex())
                {
                    this.Invoke((EventHandler)(delegate
                    {
                        textBox1.AppendText("成功找到合理曝光点\r\n");
                        listView1.Items[2].SubItems[1].Text = "完成";
                    }));
                    break;
                }
                else
                {
                    this.Invoke((EventHandler)(delegate
                    {
                        textBox1.AppendText((((double)SystemParam.eStep)*SystemParam.Ts/100/1000/1000).ToString("F2")+"ms,当前比例:"+SaturatedIPer.ToString("F2")+"\r\n");
                        textBox1.AppendText("曝光点不合理，重新查找\r\n");
                    }));
                }
                //iniFileOP.Write("System Run", "eStart", SystemParam.ExposureTest_Ns.ToString());
                iniFileOP.Write("System Run", "eStep", SystemParam.eStep.ToString());
//                 Form2 f2 = new Form2(yy, dd, eStart, eStep);
//                 f2.ShowDialog();
//                 if (f2.m_bOK)
//                 {
//                     Calc1.miu_y = new List<double>();
//                     Calc1.delta_y = new List<double>();
//                     for (int i = 0; i < yy.Count; i++)
//                     {                        
//                         Calc1.miu_y.Add(yy[i]);
//                         Calc1.delta_y.Add(dd[i]);
//                     }
//                     SystemParam.eStart = eStart;
//                     SystemParam.eStep = eStep;
//                     this.Invoke((EventHandler)(delegate
//                     {
//                         listView1.Items[1].SubItems[1].Text = "完成";
//                         textBox1.AppendText("曝光时间测试完成\r\n");
//                         StringBuilder sb = new StringBuilder();
//                         sb.Append("曝光初值为:");
//                         sb.Append((SystemParam.Ts * SystemParam.eStart).ToString("F2"));
//                         sb.Append("ms,曝光步长为:");
//                         sb.Append((SystemParam.Ts * SystemParam.eStep ).ToString("F2"));
//                         sb.Append("ms");
//                         listView1.Items[1].SubItems[2].Text = sb.ToString();
//                     }));
//                     iniFileOP.Write("System Run", "eStart", SystemParam.eStart.ToString());
//                     iniFileOP.Write("System Run", "eStep", SystemParam.eStep.ToString());
//                     break;
//                 }
//                 else
//                 {
//                     eStart = f2.m_eStart;
//                     eStep = f2.m_eStep;
//                 }
            }
        }
        void 暗场采集(object LockWatingThread)
        {
            double y, d;
            double t;
            Calc1.miu_y_dark = new List<double>();
            Calc1.delta_y_dark = new List<double>();
            Calc1.R_miu_y_dark = new List<double>();
            Calc1.G_miu_y_dark = new List<double>();
            Calc1.B_miu_y_dark = new List<double>();
            Calc1.R_delta_y_dark = new List<double>();
            Calc1.G_delta_y_dark = new List<double>();
            Calc1.B_delta_y_dark = new List<double>();
            this.Invoke((EventHandler)(delegate
            {                            
                chart1.ChartAreas[0].AxisY.Title = "暗场均值";
                chart1.ChartAreas[1].AxisY.Title = "暗场方差";
                chart1.Series["Gray_miu"].Points.Clear();
                chart1.Series["Gray_delta"].Points.Clear();
                if (SystemParam.cmosInfo.bRGB != 0)
                {
                    chart1.Series["R_miu"].Points.Clear();
                    chart1.Series["R_delta"].Points.Clear();
                    chart1.Series["G_miu"].Points.Clear();
                    chart1.Series["G_delta"].Points.Clear();
                    chart1.Series["B_miu"].Points.Clear();
                    chart1.Series["B_delta"].Points.Clear();
                }
                textBox1.AppendText("开始暗场曝光递进采集\r\n");
            }));
            //NopCam(3, SystemParam.eStart);
            int saveindex=0;
            for (uint i = 0; i < SystemParam.ExposureTest_Ns; i++)
            {
                uint ls = SystemParam.eStart + i * SystemParam.eStep;                
                this.Invoke((EventHandler)(delegate
                {
                    toolStripLabel3.Text = SystemParam.GetTime((int)i).ToString("F2") + " ms";
                }));
                if (waitProc.HasBeenCancelled())
                {
                    return;
                }
                SerialFunc.SerialCommand3((ushort)(2 + CamEx), ls);
                if (!WaitCam(2 + CamEx))
                {
                    i--;
                    continue;
                }
                saveindex++;
                m_Buffers.Save(Calc1.TempPicPath_Dark + saveindex.ToString() + ".bmp", "-format bmp", 0 + CamEx, 0);
                saveindex++;
                m_Buffers.Save(Calc1.TempPicPath_Dark + saveindex.ToString() + ".bmp", "-format bmp", 1 + CamEx, 0);
                Thread.Sleep(SystemParam.PicDelay);

                ya = wfSapGUI.ReadPicDatas(m_Buffers, 0 + CamEx);
                yb = wfSapGUI.ReadPicDatas(m_Buffers, 1 + CamEx);
                Calc1.TestExposureTime(ya, yb, m_Buffers.Height, m_Buffers.Width, m_Buffers.PixelDepth, out y, out d);
				Calc1.miu_y_dark.Add(y);
                Calc1.delta_y_dark.Add(d);
                if (SystemParam.cmosInfo.bRGB != 0)
                {
                    Calc1.Get_miu_delta(false, ya, yb, m_Buffers.Height, m_Buffers.Width, m_Buffers.PixelDepth, SystemParam.cmosInfo.RGB1, SystemParam.cmosInfo.RGB2, SystemParam.cmosInfo.RGB3, SystemParam.cmosInfo.RGB4);
                }
                this.Invoke((EventHandler)(delegate
                {
                    t = ls * SystemParam.Ts;
                    chart1.Series["Gray_miu"].Points.AddXY(t, y);
                    chart1.Series["Gray_delta"].Points.AddXY(t, d);
                    if (SystemParam.cmosInfo.bRGB != 0)
                    {
                        chart1.Series["R_miu"].Points.AddXY(t, Calc1.R_miu_y_dark.Last());
                        chart1.Series["R_delta"].Points.AddXY(t, Calc1.R_delta_y_dark.Last());
                        chart1.Series["G_miu"].Points.AddXY(t, Calc1.G_miu_y_dark.Last());
                        chart1.Series["G_delta"].Points.AddXY(t, Calc1.G_delta_y_dark.Last());
                        chart1.Series["B_miu"].Points.AddXY(t, Calc1.B_miu_y_dark.Last());
                        chart1.Series["B_delta"].Points.AddXY(t, Calc1.B_delta_y_dark.Last());
                    }
                    listView1.Items[3].SubItems[1].Text = (((double)(i + 1)*100) / SystemParam.ExposureTest_Ns).ToString("F1") + "%";
                    waitProc.SetProcessBar((int)(i + 1));
                }));
            }
            this.Invoke((EventHandler)(delegate
            {
                listView1.Items[2].SubItems[1].Text = "完成";
                textBox1.AppendText("暗场曝光递进采集完成\r\n");
            }));
            
        }
        void 计算饱和输出电压_动态范围_平均暗信号_暗信号均方差(object LockWatingThread)
        {
            this.Invoke((EventHandler)(delegate
            {
                for (int i = 0; i < chart1.Series.Count; i++)
                    chart1.Series[i].Points.Clear();
            }));            
            //输出txt文件
            TextLog.AddTextLog("--------------" + DateTime.Now.ToString() + "----------------", SystemParam.TxtDataPath + SystemParam.DeviceID + ".txt", false);
            TextLog.AddTextLog(String.Format(SystemParam.TxtDataTitleFormat, "曝光时间", "明场均值", "明场方差", "暗场均值", "暗场方差"), SystemParam.TxtDataPath + SystemParam.DeviceID + ".txt", false);
            for (int i = 0; i < SystemParam.ExposureTest_Ns; i++)
            {
                double t;
                t = SystemParam.GetTime(i);
                TextLog.AddTextLog(String.Format(SystemParam.TxtDataFormat,t.ToString("F3"), Calc1.miu_y[i].ToString("F6"), Calc1.delta_y[i].ToString("F6"), Calc1.miu_y_dark[i].ToString("F6"), Calc1.delta_y_dark[i].ToString("F6")), SystemParam.TxtDataPath + SystemParam.DeviceID + ".txt", false);
            }
            //信噪比
            Calc1.SNR = new List<double>();
            double snr;
            for (int i = 0; i < SystemParam.ExposureTest_Ns; i++)
            {
                if (Calc1.delta_y[i] == 0)
                {
                    Calc1.SNR.Add(double.MaxValue);
                }
                else
                {
                    snr = Calc1.miu_y[i] - Calc1.miu_y_dark[i];
                    snr = snr / Math.Sqrt(Calc1.delta_y[i]);
                    Calc1.SNR.Add(snr);
                }
            }
            this.Invoke((EventHandler)(delegate
            {
                listView1.Items[4].SubItems[1].Text = "完成";
                textBox1.AppendText("信噪比计算完成\r\n");
            }));
            //光电响应曲线拟合,利用一半的数据点进行拟合计算
            int fitlen = SystemParam.ExposureTest_Ns/2;
            double[] yy = new double[fitlen];
            double[] t1 = new double[fitlen];            
            for(int i=0;i<fitlen;i++)
            {
                yy[i]=Calc1.miu_y[i]-Calc1.miu_y_dark[i];
                t1[i]=SystemParam.GetTime(i);
            }
            double[] fitret=FittingMultiLine.MultiLine(t1, yy, fitlen, 1);
            Calc1.PhotoelectricResponseCurve_b = fitret[0];
            Calc1.PhotoelectricResponseCurve_k = fitret[1];

            this.Invoke((EventHandler)(delegate
            {
                chart1.Legends[0].Enabled = true;
                chart1.ChartAreas[0].AxisY.Title = "明暗场均值差值";
                chart1.ChartAreas[1].AxisY.Title = "信噪比";

                chart1.Series["Gray_miu"].LegendText = "采集曲线";
                chart1.Series["Gray_PELine"].LegendText = "光电响应曲线";
                chart1.Series["Gray_PELine"].IsVisibleInLegend = true;
                //chart1.Series["Gray_delta"].Points.Clear();//"信噪比"
                //chart1.Series[1].LegendText = "信噪比";
                textBox1.AppendText("光电响应曲线计算完成\r\n");
                listView1.Items[5].SubItems[1].Text = "完成";

                StringBuilder sb = new StringBuilder();
                sb.Append("y=");
                sb.Append(Calc1.PhotoelectricResponseCurve_k.ToString("F3"));
                if (Calc1.PhotoelectricResponseCurve_b > 0)
                    sb.Append("*t+");
                else
                    sb.Append("*t");
                sb.Append(Calc1.PhotoelectricResponseCurve_b.ToString("F3"));
                listView1.Items[5].SubItems[2].Text = sb.ToString();

                double t;
                int p;
                for (int i = 0; i < SystemParam.ExposureTest_Ns; i++)
                {
                    t = SystemParam.GetTime(i);
                    chart1.Series["Gray_miu"].Points.AddXY(t, Calc1.miu_y[i] - Calc1.miu_y_dark[i]);
                    if (i < fitlen)
                        chart1.Series["Gray_PELine"].Points.AddXY(t, Calc1.PhotoelectricResponseCurve_k * t + Calc1.PhotoelectricResponseCurve_b);
                    else
                    {
                        p = chart1.Series["Gray_PELine"].Points.AddXY(t, 0);
                        chart1.Series["Gray_PELine"].Points[p].IsEmpty = true;
                    }
                    if (Calc1.SNR[i] == double.MaxValue)
                    {
                        p = chart1.Series["Gray_delta"].Points.AddXY(t, 0);
                        chart1.Series["Gray_delta"].Points[p].IsEmpty = true;
                    }
                    else
                        chart1.Series["Gray_delta"].Points.AddXY(t, Calc1.SNR[i]); 
                }
            }));
            waitProc.SetProcessBar(20);
            //传感器总体增益K
            int K_Count= Calc1.saturation;
            int K_Start=0;
            double[] K_delta = new double[K_Count];
            double[] K_miu = new double[K_Count];
            for (int i = 0; i < K_Count; i++)
            {
                K_delta[i] = Calc1.delta_y[K_Start + i] - Calc1.delta_y_dark[K_Start + i];
                K_miu[i] = Calc1.miu_y[K_Start + i] - Calc1.miu_y_dark[K_Start + i];
            }
            fitret = FittingMultiLine.MultiLine(K_miu, K_delta, K_Count, 1);
            Calc1.OverAllGain_K = fitret[1];
            waitProc.SetProcessBar(40);
            this.Invoke((EventHandler)(delegate
            {
                listView1.Items[6].SubItems[1].Text = "完成";
                listView1.Items[6].SubItems[2].Text = Calc1.OverAllGain_K.ToString("F4");
                textBox1.AppendText("传感器总体增益K计算完成\r\n");
            }));
            //平均暗信号
            double[] miu_y_dark=Calc1.miu_y_dark.ToArray();
            double[] t2 = new double[miu_y_dark.Length];
            for (int i = 0; i < miu_y_dark.Length; i++)
            {
                t2[i] = SystemParam.GetTime(i);
            }
            fitret = FittingMultiLine.MultiLine(t2, miu_y_dark, miu_y_dark.Length, 1);
            Calc1.AverageDarkSignal_b = fitret[0];
            Calc1.AverageDarkSignal_k = fitret[1];

			double[] delta_y_dark = Calc1.delta_y_dark.ToArray();
			double[] t22 = new double[delta_y_dark.Length];
			for (int i = 0; i < delta_y_dark.Length; i++)
			{
				t22[i] = SystemParam.GetTime(i);
			}
			fitret = FittingMultiLine.MultiLine(t22, delta_y_dark, delta_y_dark.Length, 1);
			Calc1.AverageDarkSignal_b2 = fitret[0];
			Calc1.AverageDarkSignal_k2 = fitret[1];


            this.Invoke((EventHandler)(delegate
            {
                listView1.Items[7].SubItems[1].Text = "完成";
                StringBuilder sb = new StringBuilder();
                sb.Append("ydark=");
                sb.Append(Calc1.AverageDarkSignal_k.ToString("F4"));
                if (Calc1.AverageDarkSignal_b > 0)
                    sb.Append("*t+");
                else
                    sb.Append("*t");
                sb.Append(Calc1.AverageDarkSignal_b.ToString("F4"));
                listView1.Items[7].SubItems[2].Text = sb.ToString();
                textBox1.AppendText("平均暗信号计算完成\r\n");
            }));
            waitProc.SetProcessBar(60);
            //找出输出饱和点
            double per;
            double t3;
            double y1,y2;
            double diff;
            Calc1.SaturatedIndex = Calc1.miu_y.Count;
            for (int i = fitlen; i < Calc1.miu_y.Count; i++)
            {
                t3 = SystemParam.GetTime(i);
                y1 = Calc1.PhotoelectricResponseCurve_k * t3 + Calc1.PhotoelectricResponseCurve_b;
                y2=Calc1.miu_y[i]-Calc1.miu_y_dark[i];
                diff = y1 - y2;
                diff = Math.Abs(diff);
                per = diff / y1*100;
                if (per > SystemParam.ExposurePointThreshold)//找到了饱和点
                {
                    Calc1.SaturatedIndex = i;
                    break;
                }
            }
            Calc1.Saturated50Index = Calc1.SaturatedIndex / 2;
            this.Invoke((EventHandler)(delegate
            {
                textBox1.AppendText("查找输出饱和点完成\r\n");
                chart1.Series["Gray_miu"].Points[Calc1.SaturatedIndex].IsValueShownAsLabel = true;
                chart1.Series["Gray_miu"].Points[Calc1.SaturatedIndex].MarkerSize = 20;
                chart1.Series["Gray_miu"].Points[Calc1.SaturatedIndex].Label = "饱和点:" + SystemParam.GetTime(Calc1.SaturatedIndex).ToString("F2") + "ms";

                chart1.Series["Gray_miu"].Points[Calc1.Saturated50Index].IsValueShownAsLabel = true;
                chart1.Series["Gray_miu"].Points[Calc1.Saturated50Index].MarkerSize = 15;
                chart1.Series["Gray_miu"].Points[Calc1.Saturated50Index].Label = "50%饱和点:" + SystemParam.GetTime(Calc1.Saturated50Index).ToString("F2") + "ms";

                listView1.Items[8].SubItems[1].Text = "完成";
                listView1.Items[8].SubItems[2].Text = "饱和曝光时间为:" + SystemParam.GetTime(Calc1.SaturatedIndex).ToString("F2") + "ms";
            }));            
            waitProc.SetProcessBar(80);
            //平均暗信号
            Calc1.miu_d=new List<double>();
            for (int i = 0; i < Calc1.miu_y_dark.Count; i++)
            {
                Calc1.miu_d.Add(Calc1.miu_y_dark[i] / Calc1.OverAllGain_K);
            }
            //动态范围
            Calc1.DR = Calc1.miu_y[Calc1.SaturatedIndex] - Calc1.miu_y_dark[Calc1.SaturatedIndex];
            Calc1.DR = Calc1.DR / Calc1.miu_y_dark[Calc1.SaturatedIndex];
            Calc1.DR = Math.Log10(Calc1.DR);
            Calc1.DR = Calc1.DR * 20;
            this.Invoke((EventHandler)(delegate
            {
                listView1.Items[9].SubItems[1].Text = "完成";
                listView1.Items[9].SubItems[2].Text = "动态范围为:" + Calc1.DR.ToString("F2")+"dB";
                textBox1.AppendText("动态范围计算完成\r\n");
            }));
            //量子效率
            double E = 0;
            for (int i = 0; i < Calc1.EList.Count;i++ )
            {
                E += Calc1.EList.Count;
            }
            E = E / Calc1.EList.Count;
            Calc1.eta = Calc1.PhotoelectricResponseCurve_k/Calc1.OverAllGain_K/SystemParam.cmosInfo.Lambda/SystemParam.cmosInfo.PixelArea/E*6.626*Math.Pow(10,-34)*3*Math.Pow(10,8);
            this.Invoke((EventHandler)(delegate
            {
                listView1.Items[10].SubItems[1].Text = "完成";
                listView1.Items[10].SubItems[2].Text =  Calc1.eta.ToString("F2");
            }));
            //Calc1.FullCapacity = Calc1.miu_y[Calc1.SaturatedIndex] / Calc1.OverAllGain_K;
			Calc1.FullCapacity = Calc1.miu_y[SystemParam.ExposureTest_Ns-1] / Calc1.OverAllGain_K;
            this.Invoke((EventHandler)(delegate
            {
                listView1.Items[11].SubItems[1].Text = "完成";
                listView1.Items[11].SubItems[2].Text = Calc1.FullCapacity.ToString("F2");
            }));
            waitProc.SetProcessBar(100);
        }
        void RGB_计算饱和输出电压_动态范围_平均暗信号_暗信号均方差(object LockWatingThread)
        {
            this.Invoke((EventHandler)(delegate
            {
                for (int i = 0; i < chart1.Series.Count; i++)
                    chart1.Series[i].Points.Clear();
            }));  
            TextLog.AddTextLog("--------------" + DateTime.Now.ToString() + "----------------", SystemParam.TxtDataPath + SystemParam.DeviceID + ".txt", false);
            TextLog.AddTextLog(String.Format(SystemParam.TxtDataTitleFormat_RGB, "曝光时间", "R明场均值", "R明场方差", "R暗场均值", "R暗场方差", "G明场均值", "G明场方差", "G暗场均值", "G暗场方差", "B明场均值", "B明场方差", "B暗场均值", "B暗场方差"), SystemParam.TxtDataPath + SystemParam.DeviceID + ".txt", false);
            //输出txt文件
            for (int i = 0; i < SystemParam.ExposureTest_Ns; i++)
            {
                double t;
                t = SystemParam.GetTime(i);
                TextLog.AddTextLog(String.Format(SystemParam.TxtDataFormat_RGB, t.ToString("F3"), Calc1.R_miu_y[i].ToString("F6"), Calc1.R_delta_y[i].ToString("F6"), Calc1.R_miu_y_dark[i].ToString("F6"), Calc1.R_delta_y_dark[i].ToString("F6"), Calc1.G_miu_y[i].ToString("F6"), Calc1.G_delta_y[i].ToString("F6"), Calc1.G_miu_y_dark[i].ToString("F6"), Calc1.G_delta_y_dark[i].ToString("F6"), Calc1.B_miu_y[i].ToString("F6"), Calc1.B_delta_y[i].ToString("F6"), Calc1.B_miu_y_dark[i].ToString("F6"), Calc1.B_delta_y_dark[i].ToString("F6")), SystemParam.TxtDataPath + SystemParam.DeviceID + ".txt", false);
            }
            //信噪比
            Calc1.R_SNR = new List<double>();
            Calc1.G_SNR = new List<double>();
            Calc1.B_SNR = new List<double>();
            double snr;
            for (int i = 0; i < SystemParam.ExposureTest_Ns; i++)
            {
                /************************************************************************/
                /*                                                                      */
                /************************************************************************/
                if (Calc1.R_delta_y[i] == 0)
                {
                    Calc1.R_SNR.Add(double.MaxValue);
                }
                else
                {
                    snr = Calc1.R_miu_y[i] - Calc1.R_miu_y_dark[i];
                    snr = snr / Math.Sqrt(Calc1.R_delta_y[i]);
                    Calc1.R_SNR.Add(snr);
                }
                /************************************************************************/
                /*                                                                      */
                /************************************************************************/
                if (Calc1.G_delta_y[i] == 0)
                {
                    Calc1.G_SNR.Add(double.MaxValue);
                }
                else
                {
                    snr = Calc1.G_miu_y[i] - Calc1.G_miu_y_dark[i];
                    snr = snr / Math.Sqrt(Calc1.G_delta_y[i]);
                    Calc1.G_SNR.Add(snr);
                }
                /************************************************************************/
                /*                                                                      */
                /************************************************************************/
                if (Calc1.B_delta_y[i] == 0)
                {
                    Calc1.B_SNR.Add(double.MaxValue);
                }
                else
                {
                    snr = Calc1.B_miu_y[i] - Calc1.B_miu_y_dark[i];
                    snr = snr / Math.Sqrt(Calc1.B_delta_y[i]);
                    Calc1.B_SNR.Add(snr);
                }
            }
            this.Invoke((EventHandler)(delegate
            {
                listView1.Items[4].SubItems[1].Text = "完成";
                textBox1.AppendText("信噪比计算完成\r\n");
            }));
            //光电响应曲线拟合,利用一半的数据点进行拟合计算
            /************************************************************************/
            /*                                                                      */
            /************************************************************************/
            int fitlen = Calc1.R_miu_y.Count/2;
            double[] yy = new double[fitlen];
            double[] t1 = new double[fitlen];            
            for(int i=0;i<fitlen;i++)
            {
                yy[i]=Calc1.R_miu_y[i]-Calc1.R_miu_y_dark[i];
                t1[i]=SystemParam.GetTime(i);
            }
            double[] fitret=FittingMultiLine.MultiLine(t1, yy, fitlen, 1);
            Calc1.R_PhotoelectricResponseCurve_b = fitret[0];
            Calc1.R_PhotoelectricResponseCurve_k = fitret[1];
            /************************************************************************/
            /*                                                                      */
            /************************************************************************/
            fitlen = Calc1.G_miu_y.Count / 2;
            yy = new double[fitlen];
            t1 = new double[fitlen];
            for (int i = 0; i < fitlen; i++)
            {
                yy[i] = Calc1.G_miu_y[i] - Calc1.G_miu_y_dark[i];
                t1[i] = SystemParam.GetTime(i);
            }
            fitret = FittingMultiLine.MultiLine(t1, yy, fitlen, 1);
            Calc1.G_PhotoelectricResponseCurve_b = fitret[0];
            Calc1.G_PhotoelectricResponseCurve_k = fitret[1];
            /************************************************************************/
            /*                                                                      */
            /************************************************************************/
            fitlen = Calc1.B_miu_y.Count / 2;
            yy = new double[fitlen];
            t1 = new double[fitlen];
            for (int i = 0; i < fitlen; i++)
            {
                yy[i] = Calc1.B_miu_y[i] - Calc1.B_miu_y_dark[i];
                t1[i] = SystemParam.GetTime(i);
            }
            fitret = FittingMultiLine.MultiLine(t1, yy, fitlen, 1);
            Calc1.B_PhotoelectricResponseCurve_b = fitret[0];
            Calc1.B_PhotoelectricResponseCurve_k = fitret[1];

            this.Invoke((EventHandler)(delegate
            {
                chart1.Legends[0].Enabled = true;
                chart1.ChartAreas[0].AxisY.Title = "明暗场均值差值";
                chart1.ChartAreas[1].AxisY.Title = "信噪比";

                chart1.Series["Gray_miu"].LegendText = "采集曲线";
                chart1.Series["Gray_PELine"].LegendText = "光电响应曲线";
                chart1.Series["Gray_PELine"].IsVisibleInLegend = true;
                //chart1.Series["Gray_delta"].Points.Clear();//"信噪比"
                //chart1.Series[1].LegendText = "信噪比";
                textBox1.AppendText("光电响应曲线计算完成\r\n");                

                StringBuilder sb = new StringBuilder();
                sb.Append("y=");
                sb.Append(Calc1.R_PhotoelectricResponseCurve_k.ToString("F3"));
                if (Calc1.R_PhotoelectricResponseCurve_b > 0)
                    sb.Append("*t+");
                else
                    sb.Append("*t");
                sb.Append(Calc1.R_PhotoelectricResponseCurve_b.ToString("F3"));

                sb.Append("||");
                
                sb.Append("y=");
                sb.Append(Calc1.G_PhotoelectricResponseCurve_k.ToString("F3"));
                if (Calc1.G_PhotoelectricResponseCurve_b > 0)
                    sb.Append("*t+");
                else
                    sb.Append("*t");
                sb.Append(Calc1.G_PhotoelectricResponseCurve_b.ToString("F3"));

                sb.Append("||");

                sb.Append("y=");
                sb.Append(Calc1.B_PhotoelectricResponseCurve_k.ToString("F3"));
                if (Calc1.B_PhotoelectricResponseCurve_b > 0)
                    sb.Append("*t+");
                else
                    sb.Append("*t");
                sb.Append(Calc1.B_PhotoelectricResponseCurve_b.ToString("F3"));
                listView1.Items[5].SubItems[1].Text = "完成";
                listView1.Items[5].SubItems[2].Text = sb.ToString();

                double t;
                int p;
                for (int i = 0; i < SystemParam.ExposureTest_Ns; i++)
                {
                    t = SystemParam.GetTime(i);
                    chart1.Series["Gray_miu"].Points.AddXY(t, Calc1.miu_y[i] - Calc1.miu_y_dark[i]);
                    chart1.Series["R_miu"].Points.AddXY(t, Calc1.R_miu_y[i] - Calc1.R_miu_y_dark[i]);
                    chart1.Series["G_miu"].Points.AddXY(t, Calc1.G_miu_y[i] - Calc1.G_miu_y_dark[i]);
                    chart1.Series["B_miu"].Points.AddXY(t, Calc1.B_miu_y[i] - Calc1.B_miu_y_dark[i]);
                    if (i < fitlen)
                    {
                        chart1.Series["Gray_PELine"].Points.AddXY(t, Calc1.PhotoelectricResponseCurve_k * t + Calc1.PhotoelectricResponseCurve_b);
                        chart1.Series["R_PELine"].Points.AddXY(t, Calc1.R_PhotoelectricResponseCurve_k * t + Calc1.R_PhotoelectricResponseCurve_b);
                        chart1.Series["G_PELine"].Points.AddXY(t, Calc1.G_PhotoelectricResponseCurve_k * t + Calc1.G_PhotoelectricResponseCurve_b);
                        chart1.Series["B_PELine"].Points.AddXY(t, Calc1.B_PhotoelectricResponseCurve_k * t + Calc1.B_PhotoelectricResponseCurve_b);
                    }
                    else
                    {
                        p = chart1.Series["Gray_PELine"].Points.AddXY(t, 0);
                        chart1.Series["Gray_PELine"].Points[p].IsEmpty = true;
                        p = chart1.Series["R_PELine"].Points.AddXY(t, 0);
                        chart1.Series["R_PELine"].Points[p].IsEmpty = true;
                        p = chart1.Series["G_PELine"].Points.AddXY(t, 0);
                        chart1.Series["G_PELine"].Points[p].IsEmpty = true;
                        p = chart1.Series["B_PELine"].Points.AddXY(t, 0);
                        chart1.Series["B_PELine"].Points[p].IsEmpty = true;
                    }
                    if (Calc1.SNR[i] == double.MaxValue)
                    {
                        p = chart1.Series["Gray_delta"].Points.AddXY(t, 0);
                        chart1.Series["Gray_delta"].Points[p].IsEmpty = true;
                    }
                    else
                        chart1.Series["Gray_delta"].Points.AddXY(t, Calc1.SNR[i]);
                    
                    if (Calc1.R_SNR[i] == double.MaxValue)
                    {
                        p = chart1.Series["R_delta"].Points.AddXY(t, 0);
                        chart1.Series["R_delta"].Points[p].IsEmpty = true;
                    }
                    else
                        chart1.Series["R_delta"].Points.AddXY(t, Calc1.R_SNR[i]);
                    
                    if (Calc1.G_SNR[i] == double.MaxValue)
                    {
                        p = chart1.Series["G_delta"].Points.AddXY(t, 0);
                        chart1.Series["G_delta"].Points[p].IsEmpty = true;
                    }
                    else
                        chart1.Series["G_delta"].Points.AddXY(t, Calc1.G_SNR[i]);
                    
                    if (Calc1.B_SNR[i] == double.MaxValue)
                    {
                        p = chart1.Series["B_delta"].Points.AddXY(t, 0);
                        chart1.Series["B_delta"].Points[p].IsEmpty = true;
                    }
                    else
                        chart1.Series["B_delta"].Points.AddXY(t, Calc1.B_SNR[i]); 
                }
            }));
            waitProc.SetProcessBar(20);
            //传感器总体增益K
            /************************************************************************/
            /*                                                                      */
            /************************************************************************/
            int K_Count= Calc1.saturation;
            int K_Start=0;
            double[] K_delta = new double[K_Count];
            double[] K_miu = new double[K_Count];
            for (int i = 0; i < K_Count; i++)
            {
                K_delta[i] = Calc1.R_delta_y[K_Start + i] - Calc1.R_delta_y_dark[K_Start + i];
                K_miu[i] = Calc1.R_miu_y[K_Start + i] - Calc1.R_miu_y_dark[K_Start + i];
            }
            fitret = FittingMultiLine.MultiLine(K_miu, K_delta, K_Count, 1);
            Calc1.R_OverAllGain_K = fitret[1];
            /************************************************************************/
            /*                                                                      */
            /************************************************************************/
            K_Count = Calc1.saturation;
            K_Start = 0;
            K_delta = new double[K_Count];
            K_miu = new double[K_Count];
            for (int i = 0; i < K_Count; i++)
            {
                K_delta[i] = Calc1.G_delta_y[K_Start + i] - Calc1.G_delta_y_dark[K_Start + i];
                K_miu[i] = Calc1.G_miu_y[K_Start + i] - Calc1.G_miu_y_dark[K_Start + i];
            }
            fitret = FittingMultiLine.MultiLine(K_miu, K_delta, K_Count, 1);
            Calc1.G_OverAllGain_K = fitret[1];
            /************************************************************************/
            /*                                                                      */
            /************************************************************************/
            K_Count = Calc1.saturation;
            K_Start = 0;
            K_delta = new double[K_Count];
            K_miu = new double[K_Count];
            for (int i = 0; i < K_Count; i++)
            {
                K_delta[i] = Calc1.B_delta_y[K_Start + i] - Calc1.B_delta_y_dark[K_Start + i];
                K_miu[i] = Calc1.B_miu_y[K_Start + i] - Calc1.B_miu_y_dark[K_Start + i];
            }
            fitret = FittingMultiLine.MultiLine(K_miu, K_delta, K_Count, 1);
            Calc1.B_OverAllGain_K = fitret[1];
            waitProc.SetProcessBar(40);
            this.Invoke((EventHandler)(delegate
            {
                listView1.Items[6].SubItems[1].Text = "完成";
                listView1.Items[6].SubItems[2].Text = Calc1.R_OverAllGain_K.ToString("F4") + "||" + Calc1.G_OverAllGain_K.ToString("F4") + "||" + Calc1.B_OverAllGain_K.ToString("F4");
                textBox1.AppendText("传感器总体增益K计算完成\r\n");
            }));
            //平均暗信号
            /************************************************************************/
            /*                                                                      */
            /************************************************************************/
            double[] miu_y_dark=Calc1.R_miu_y_dark.ToArray();
            double[] t2 = new double[miu_y_dark.Length];
            for (int i = 0; i < miu_y_dark.Length; i++)
            {
                t2[i] = SystemParam.GetTime(i);
            }
            fitret = FittingMultiLine.MultiLine(t2, miu_y_dark, miu_y_dark.Length, 1);
            Calc1.R_AverageDarkSignal_b = fitret[0];
            Calc1.R_AverageDarkSignal_k = fitret[1];
            /************************************************************************/
            /*                                                                      */
            /************************************************************************/
            miu_y_dark = Calc1.G_miu_y_dark.ToArray();
            t2 = new double[miu_y_dark.Length];
            for (int i = 0; i < miu_y_dark.Length; i++)
            {
                t2[i] = SystemParam.GetTime(i);
            }
            fitret = FittingMultiLine.MultiLine(t2, miu_y_dark, miu_y_dark.Length, 1);
            Calc1.G_AverageDarkSignal_b = fitret[0];
            Calc1.G_AverageDarkSignal_k = fitret[1];
            /************************************************************************/
            /*                                                                      */
            /************************************************************************/
            miu_y_dark = Calc1.B_miu_y_dark.ToArray();
            t2 = new double[miu_y_dark.Length];
            for (int i = 0; i < miu_y_dark.Length; i++)
            {
                t2[i] = SystemParam.GetTime(i);
            }
            fitret = FittingMultiLine.MultiLine(t2, miu_y_dark, miu_y_dark.Length, 1);
            Calc1.B_AverageDarkSignal_b = fitret[0];
            Calc1.B_AverageDarkSignal_k = fitret[1];

            this.Invoke((EventHandler)(delegate
            {
                listView1.Items[7].SubItems[1].Text = "完成";
                StringBuilder sb = new StringBuilder();
                sb.Append("ydark=");
                sb.Append(Calc1.R_AverageDarkSignal_k.ToString("F4"));
                if (Calc1.R_AverageDarkSignal_b > 0)
                    sb.Append("*t+");
                else
                    sb.Append("*t");
                sb.Append(Calc1.R_AverageDarkSignal_b.ToString("F4"));

                sb.Append("||");
                
                sb.Append("ydark=");
                sb.Append(Calc1.G_AverageDarkSignal_k.ToString("F4"));
                if (Calc1.G_AverageDarkSignal_b > 0)
                    sb.Append("*t+");
                else
                    sb.Append("*t");
                sb.Append(Calc1.G_AverageDarkSignal_b.ToString("F4"));

                sb.Append("||");
                
                sb.Append("ydark=");
                sb.Append(Calc1.B_AverageDarkSignal_k.ToString("F4"));
                if (Calc1.B_AverageDarkSignal_b > 0)
                    sb.Append("*t+");
                else
                    sb.Append("*t");
                sb.Append(Calc1.B_AverageDarkSignal_b.ToString("F4"));
                listView1.Items[7].SubItems[2].Text = sb.ToString();
                textBox1.AppendText("平均暗信号计算完成\r\n");
            }));
            waitProc.SetProcessBar(60);
            //找出输出饱和点
            double per;
            double t3;
            double y1, y2;
            double diff;
            /************************************************************************/
            /*                                                                      */
            /************************************************************************/            
            for (int i = fitlen; i < Calc1.R_miu_y.Count; i++)
            {
                t3 = SystemParam.GetTime(i);
                y1 = Calc1.R_PhotoelectricResponseCurve_k * t3 + Calc1.R_PhotoelectricResponseCurve_b;
                y2=Calc1.R_miu_y[i]-Calc1.R_miu_y_dark[i];
                diff = y1 - y2;
                diff = Math.Abs(diff);
                per = diff / y1*100;
                if (per > SystemParam.ExposurePointThreshold)//找到了饱和点
                {
                    Calc1.R_SaturatedIndex = i;
                    break;
                }
            }
            Calc1.R_Saturated50Index = Calc1.R_SaturatedIndex / 2;
            /************************************************************************/
            /*                                                                      */
            /************************************************************************/
            for (int i = fitlen; i < Calc1.G_miu_y.Count; i++)
            {
                t3 = SystemParam.GetTime(i);
                y1 = Calc1.G_PhotoelectricResponseCurve_k * t3 + Calc1.G_PhotoelectricResponseCurve_b;
                y2 = Calc1.G_miu_y[i] - Calc1.G_miu_y_dark[i];
                diff = y1 - y2;
                diff = Math.Abs(diff);
                per = diff / y1 * 100;
                if (per > SystemParam.ExposurePointThreshold)//找到了饱和点
                {
                    Calc1.G_SaturatedIndex = i;
                    break;
                }
            }
            Calc1.G_Saturated50Index = Calc1.G_SaturatedIndex / 2;
            /************************************************************************/
            /*                                                                      */
            /************************************************************************/
            for (int i = fitlen; i < Calc1.B_miu_y.Count; i++)
            {
                t3 = SystemParam.GetTime(i);
                y1 = Calc1.B_PhotoelectricResponseCurve_k * t3 + Calc1.B_PhotoelectricResponseCurve_b;
                y2 = Calc1.B_miu_y[i] - Calc1.B_miu_y_dark[i];
                diff = y1 - y2;
                diff = Math.Abs(diff);
                per = diff / y1 * 100;
                if (per > SystemParam.ExposurePointThreshold)//找到了饱和点
                {
                    Calc1.B_SaturatedIndex = i;
                    break;
                }
            }
            Calc1.B_Saturated50Index = Calc1.B_SaturatedIndex / 2;
            this.Invoke((EventHandler)(delegate
            {
                textBox1.AppendText("查找输出饱和点完成\r\n");
                chart1.Series["R_miu"].Points[Calc1.R_SaturatedIndex].IsValueShownAsLabel=true;
                chart1.Series["R_miu"].Points[Calc1.R_SaturatedIndex].MarkerSize = 20;
                chart1.Series["R_miu"].Points[Calc1.R_SaturatedIndex].Label = "饱和点:" + SystemParam.GetTime(Calc1.R_SaturatedIndex).ToString("F2") + "ms";

                chart1.Series["R_miu"].Points[Calc1.R_Saturated50Index].IsValueShownAsLabel = true;
                chart1.Series["R_miu"].Points[Calc1.R_Saturated50Index].MarkerSize = 15;
                chart1.Series["R_miu"].Points[Calc1.R_Saturated50Index].Label = "50%饱和点:" + SystemParam.GetTime(Calc1.R_Saturated50Index).ToString("F2") + "ms";


                chart1.Series["G_miu"].Points[Calc1.G_SaturatedIndex].IsValueShownAsLabel = true;
                chart1.Series["G_miu"].Points[Calc1.G_SaturatedIndex].MarkerSize = 20;
                chart1.Series["G_miu"].Points[Calc1.G_SaturatedIndex].Label = "饱和点:" + SystemParam.GetTime(Calc1.G_SaturatedIndex).ToString("F2") + "ms";

                chart1.Series["G_miu"].Points[Calc1.G_Saturated50Index].IsValueShownAsLabel = true;
                chart1.Series["G_miu"].Points[Calc1.G_Saturated50Index].MarkerSize = 15;
                chart1.Series["G_miu"].Points[Calc1.G_Saturated50Index].Label = "50%饱和点:" + SystemParam.GetTime(Calc1.G_Saturated50Index).ToString("F2") + "ms";



                chart1.Series["B_miu"].Points[Calc1.B_SaturatedIndex].IsValueShownAsLabel = true;
                chart1.Series["B_miu"].Points[Calc1.B_SaturatedIndex].MarkerSize = 20;
                chart1.Series["B_miu"].Points[Calc1.B_SaturatedIndex].Label = "饱和点:" + SystemParam.GetTime(Calc1.B_SaturatedIndex).ToString("F2") + "ms";

                chart1.Series["B_miu"].Points[Calc1.B_Saturated50Index].IsValueShownAsLabel = true;
                chart1.Series["B_miu"].Points[Calc1.B_Saturated50Index].MarkerSize = 15;
                chart1.Series["B_miu"].Points[Calc1.B_Saturated50Index].Label = "50%饱和点:" + SystemParam.GetTime(Calc1.B_Saturated50Index).ToString("F2") + "ms";
                
                
                listView1.Items[8].SubItems[1].Text = "完成";
                listView1.Items[8].SubItems[2].Text = "饱和曝光时间为:" + SystemParam.GetTime(Calc1.R_SaturatedIndex).ToString("F2") + "ms" + "||" + SystemParam.GetTime(Calc1.G_SaturatedIndex).ToString("F2") + "ms" + "||" + SystemParam.GetTime(Calc1.B_SaturatedIndex).ToString("F2") + "ms";
            }));            
            waitProc.SetProcessBar(80);
            //平均暗信号
            /************************************************************************/
            /*                                                                      */
            /************************************************************************/
            Calc1.R_miu_d=new List<double>();
            for (int i = 0; i < Calc1.R_miu_y_dark.Count; i++)
            {
                Calc1.R_miu_d.Add(Calc1.R_miu_y_dark[i] / Calc1.R_OverAllGain_K);
            }
            /************************************************************************/
            /*                                                                      */
            /************************************************************************/
            Calc1.G_miu_d = new List<double>();
            for (int i = 0; i < Calc1.G_miu_y_dark.Count; i++)
            {
                Calc1.G_miu_d.Add(Calc1.G_miu_y_dark[i] / Calc1.G_OverAllGain_K);
            }/************************************************************************/
            /*                                                                      */
            /************************************************************************/
            Calc1.B_miu_d = new List<double>();
            for (int i = 0; i < Calc1.B_miu_y_dark.Count; i++)
            {
                Calc1.B_miu_d.Add(Calc1.B_miu_y_dark[i] / Calc1.B_OverAllGain_K);
            }
            //动态范围
            /************************************************************************/
            /*                                                                      */
            /************************************************************************/
            Calc1.R_DR = Calc1.R_miu_y[Calc1.R_SaturatedIndex] - Calc1.R_miu_y_dark[Calc1.R_SaturatedIndex];
            Calc1.R_DR = Calc1.R_DR / Calc1.R_miu_y_dark[Calc1.R_SaturatedIndex];
            Calc1.R_DR = Math.Log10(Calc1.R_DR);
            Calc1.R_DR = Calc1.R_DR * 20;
            /************************************************************************/
            /*                                                                      */
            /************************************************************************/
            Calc1.G_DR = Calc1.G_miu_y[Calc1.G_SaturatedIndex] - Calc1.G_miu_y_dark[Calc1.G_SaturatedIndex];
            Calc1.G_DR = Calc1.G_DR / Calc1.G_miu_y_dark[Calc1.G_SaturatedIndex];
            Calc1.G_DR = Math.Log10(Calc1.G_DR);
            Calc1.G_DR = Calc1.G_DR * 20;
            /************************************************************************/
            /*                                                                      */
            /************************************************************************/
            Calc1.B_DR = Calc1.B_miu_y[Calc1.B_SaturatedIndex] - Calc1.B_miu_y_dark[Calc1.B_SaturatedIndex];
            Calc1.B_DR = Calc1.B_DR / Calc1.B_miu_y_dark[Calc1.B_SaturatedIndex];
            Calc1.B_DR = Math.Log10(Calc1.B_DR);
            Calc1.B_DR = Calc1.B_DR * 20;
            this.Invoke((EventHandler)(delegate
            {
                listView1.Items[9].SubItems[1].Text = "完成";
                listView1.Items[9].SubItems[2].Text = "动态范围为:" + Calc1.R_DR.ToString("F2") + "dB" + "||" + Calc1.G_DR.ToString("F2") + "dB" + "||" + Calc1.B_DR.ToString("F2") + "dB";
                textBox1.AppendText("动态范围计算完成\r\n");
            }));
            //
            double E = 0;
            for (int i = 0; i < Calc1.EList.Count; i++)
            {
                E += Calc1.EList.Count;
            }
            E = E / Calc1.EList.Count;
            Calc1.R_eta = Calc1.R_PhotoelectricResponseCurve_k / Calc1.R_OverAllGain_K / SystemParam.cmosInfo.Lambda / SystemParam.cmosInfo.PixelArea / E * 6.626 * Math.Pow(10, -34) * 3 * Math.Pow(10, 8);
            Calc1.G_eta = Calc1.G_PhotoelectricResponseCurve_k / Calc1.G_OverAllGain_K / SystemParam.cmosInfo.Lambda / SystemParam.cmosInfo.PixelArea / E * 6.626 * Math.Pow(10, -34) * 3 * Math.Pow(10, 8);
            Calc1.B_eta = Calc1.B_PhotoelectricResponseCurve_k / Calc1.B_OverAllGain_K / SystemParam.cmosInfo.Lambda / SystemParam.cmosInfo.PixelArea / E * 6.626 * Math.Pow(10, -34) * 3 * Math.Pow(10, 8);
            this.Invoke((EventHandler)(delegate
            {
                listView1.Items[10].SubItems[1].Text = "完成";
                listView1.Items[10].SubItems[2].Text = Calc1.R_eta.ToString("F2") + "||" + Calc1.G_eta.ToString("F2") + "||" + Calc1.B_eta.ToString("F2");
            }));
            Calc1.R_FullCapacity = Calc1.R_miu_y[Calc1.R_SaturatedIndex] / Calc1.R_OverAllGain_K;
            Calc1.G_FullCapacity = Calc1.G_miu_y[Calc1.G_SaturatedIndex] / Calc1.G_OverAllGain_K;
            Calc1.B_FullCapacity = Calc1.B_miu_y[Calc1.B_SaturatedIndex] / Calc1.B_OverAllGain_K;
            this.Invoke((EventHandler)(delegate
            {
                listView1.Items[11].SubItems[1].Text = "完成";
                listView1.Items[11].SubItems[2].Text = Calc1.R_FullCapacity.ToString("F2") + "||" + Calc1.G_FullCapacity.ToString("F2") + "||" + Calc1.B_FullCapacity.ToString("F2");
            }));

            waitProc.SetProcessBar(100);
        }
    }    
}
