using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WFNetLib;
using WFNetLib.Algorithm;
using WFNetLib.Log;

namespace udpCCDTest
{
    public partial class FormMain : Form
    {
        string strStepLogPath1 = SystemParam.TxtDataPath + "step1_";
        string strStepLogPath2L = SystemParam.TxtDataPath + "step2L_";
        string strStepLogPath2D = SystemParam.TxtDataPath + "step2D_";

        string strSNRLogPath = SystemParam.TxtDataPath + "SNR";
        string strDarkILogPath = SystemParam.TxtDataPath + "DarkI_";
        bool CCDParamTest_K()
        {
            if (!CCDParamTest_Collect_Step())
            {
                MessageBox.Show("曲线采集失败");
                return false;
            }
            if (ExType == 1)//固定曝光时间，改变光源照度
            {
                int nsat = Collect_Step_miu1.Count;
                for (int i = 0; i < Collect_Step_miu1.Count; i++)
                {
                    if (Collect_Step_miu1[i] >= ccdParamTestResult.miu_sat)
                    {
                        nsat = i;
                        break;
                    }
                }            
                int K_Count = (int)(nsat * (SystemParam.L_TOP - SystemParam.L_BTM) / 100.0);
                int K_Start = (int)(nsat * SystemParam.L_BTM / 100.0);
                double[] K_delta = new double[K_Count];
                double[] K_miu = new double[K_Count];
                chart1.Series[0].Points.Clear();
                chart2.Series[0].Points.Clear();
                chart1.ChartAreas[0].AxisY.Title = "均值";
                chart1.ChartAreas[0].AxisX.Title = "照度(uW/cm2)";
                chart2.ChartAreas[0].AxisY.Title = "方差";
                chart2.ChartAreas[0].AxisX.Title = "照度(uW/cm2)";
                chart1.ChartAreas[0].AxisX.LabelStyle.Format = "F3";
                for (int i = 0; i < K_Count; i++)
                {
                    K_miu[i] = Collect_Step_miu1[K_Start + i];
                    K_delta[i] = Collect_Step_delta1[K_Start + i];
                }
                double[] fitret = FittingMultiLine.MultiLine(K_miu, K_delta, K_Count, 1);
                ccdParamTestResult.K = fitret[1];
                for (int i = 0; i < Collect_Step_E1.Count; i++)
                {
                    chart1.Series[0].Points.AddXY(Collect_Step_E1[i], Collect_Step_miu1[i]);
                    chart2.Series[0].Points.AddXY(Collect_Step_E1[i], Collect_Step_delta1[i]);
                }
                chart1.SaveImage(SystemParam.TempPicPath + "K_1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                chart2.SaveImage(SystemParam.TempPicPath + "K_2.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                chart3.Series[0].Points.Clear();
                chart3.Series[1].Points.Clear();
                chart3.ChartAreas[0].AxisY.Title = "方差";
                chart3.ChartAreas[0].AxisX.Title = "均值";
                for (int i = 0; i < K_Count; i++)
                {
                    chart3.Series[0].Points.AddXY(K_miu[i], K_delta[i]);
                    chart3.Series[1].Points.AddXY(K_miu[i], K_miu[i] * fitret[1] + fitret[0]);
                }
                chart3.SaveImage(SystemParam.TempPicPath + "KL.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            else if (ExType == 2)//固定光源照度，改变曝光时间
            {
                int nsat = Collect_Step_miu2.Count;
                for (int i = 0; i < Collect_Step_miu2.Count; i++)
                {
                    if (Collect_Step_miu2[i] >= ccdParamTestResult.miu_sat)
                    {
                        nsat = i;
                        break;
                    }
                }
                int K_Count = (int)(nsat * (SystemParam.L_TOP - SystemParam.L_BTM) / 100.0);
                int K_Start = (int)(nsat * SystemParam.L_BTM / 100.0);
                double[] K_delta = new double[K_Count];
                double[] K_miu = new double[K_Count];
                double[] NTexp = new double[K_Count];
                chart1.Series[0].Points.Clear();
                chart2.Series[0].Points.Clear();
                chart1.ChartAreas[0].AxisY.Title = "均值";
                chart1.ChartAreas[0].AxisX.Title = "曝光时间";
                chart2.ChartAreas[0].AxisY.Title = "方差";
                chart2.ChartAreas[0].AxisX.Title = "曝光时间";
                for (int i = 0; i < K_Count; i++)
                {
                    K_miu[i] = Collect_Step_miu2[K_Start + i];
                    K_delta[i] = Collect_Step_delta2[K_Start + i];
                    NTexp[i] = SystemParam.NTmin2 + ccdParamTestResult.NTexp2 * i;
                }
                double[] fitret_miu = FittingMultiLine.MultiLine(NTexp, K_miu, K_Count, 1);
                double[] fitret_delta = FittingMultiLine.MultiLine(NTexp, K_delta, K_Count, 1);
                for (int i = 0; i < Collect_Step_miu2.Count; i++)
                {
                    chart1.Series[0].Points.AddXY(SystemParam.GetTime2(i, ccdParamTestResult.NTexp2), Collect_Step_miu2[i]);
                    chart2.Series[0].Points.AddXY(SystemParam.GetTime2(i,ccdParamTestResult.NTexp2), Collect_Step_delta2[i]);
                }
                chart1.SaveImage(SystemParam.TempPicPath + "K_1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                chart2.SaveImage(SystemParam.TempPicPath + "K_2.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                chart3.Series[0].Points.Clear();
                chart3.Series[1].Points.Clear();
                chart3.ChartAreas[0].AxisY.Title = "均值";
                chart3.ChartAreas[0].AxisX.Title = "曝光时间";
                for (int i = 0; i < K_Count; i++)
                {
                    chart3.Series[0].Points.AddXY(NTexp[i], K_miu[i]);
                    chart3.Series[1].Points.AddXY(NTexp[i], NTexp[i] * fitret_miu[1] + fitret_miu[0]);
                }
                chart3.SaveImage(SystemParam.TempPicPath + "KL1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                chart3.Series[0].Points.Clear();
                chart3.Series[1].Points.Clear();
                chart3.ChartAreas[0].AxisY.Title = "方差";
                chart3.ChartAreas[0].AxisX.Title = "曝光时间";
                for (int i = 0; i < K_Count; i++)
                {
                    chart3.Series[0].Points.AddXY(NTexp[i], K_delta[i]);
                    chart3.Series[1].Points.AddXY(NTexp[i], NTexp[i] * fitret_delta[1] + fitret_delta[0]);
                }
                chart3.SaveImage(SystemParam.TempPicPath + "KL2.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);


                chart1.Series[0].Points.Clear();
                chart2.Series[0].Points.Clear();
                chart1.ChartAreas[0].AxisY.Title = "暗场均值";
                chart1.ChartAreas[0].AxisX.Title = "曝光时间";
                chart2.ChartAreas[0].AxisY.Title = "暗场方差";
                chart2.ChartAreas[0].AxisX.Title = "曝光时间";
                for (int i = 0; i < K_Count; i++)
                {
                    K_miu[i] = Collect_Step_miu_dark2[K_Start + i];
                    K_delta[i] = Collect_Step_delta_dark2[K_Start + i];
                    chart1.Series[0].Points.AddXY(NTexp[i], K_miu[i]);
                    chart2.Series[0].Points.AddXY(NTexp[i], K_delta[i]);
                }
                double[] fitret_miu_dark = FittingMultiLine.MultiLine(NTexp, K_miu, K_Count, 1);
                double[] fitret_delta_dark = FittingMultiLine.MultiLine(NTexp, K_delta, K_Count, 1);
                ccdParamTestResult.K = (fitret_delta[1] - fitret_delta_dark[1]) / (fitret_miu[1] - fitret_miu_dark[1]);
                chart1.SaveImage(SystemParam.TempPicPath + "K_3.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                chart2.SaveImage(SystemParam.TempPicPath + "K_4.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);


                chart3.Series[0].Points.Clear();
                chart3.Series[1].Points.Clear();
                chart3.ChartAreas[0].AxisY.Title = "暗场均值";
                chart3.ChartAreas[0].AxisX.Title = "曝光时间";
                for (int i = 0; i < K_Count; i++)
                {
                    chart3.Series[0].Points.AddXY(NTexp[i], K_miu[i]);
                    chart3.Series[1].Points.AddXY(NTexp[i], NTexp[i] * fitret_miu_dark[1] + fitret_miu_dark[0]);
                }
                chart3.SaveImage(SystemParam.TempPicPath + "KL3.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                chart3.Series[0].Points.Clear();
                chart3.Series[1].Points.Clear();
                chart3.ChartAreas[0].AxisY.Title = "暗场方差";
                chart3.ChartAreas[0].AxisX.Title = "曝光时间";
                for (int i = 0; i < K_Count; i++)
                {
                    chart3.Series[0].Points.AddXY(NTexp[i], K_delta[i]);
                    chart3.Series[1].Points.AddXY(NTexp[i], NTexp[i] * fitret_delta_dark[1] + fitret_delta_dark[0]);
                }
                chart3.SaveImage(SystemParam.TempPicPath + "KL4.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            ccdParamTestResult.bmiuCurve = true;
            return true;
        }

        List<double> Collect_Step_miu1;
        List<double> Collect_Step_delta1;
        List<double> Collect_Step_E1;//照度

        List<double> Collect_Step_miu2;
        List<double> Collect_Step_delta2;
        List<double> Collect_Step_E2;//照度

        List<double> Collect_Step_miu_dark2;
        List<double> Collect_Step_delta_dark2;
        bool CCDParamTest_Collect_Step()
        {
            if (ExType == 1)
                return CCDParamTest_Collect_Step1();
            else
                return CCDParamTest_Collect_Step2();
        }
        bool CCDParamTest_Collect_Step1()
        {
            ParamTestWaitingProc = new WaitingProc();            
            WaitingProcFunc wpf=null;
            if (Collect_Step_miu1 != null && Collect_Step_miu1.Count == (SystemParam.n + 1))
                return true;
            chartHide();
            ParamTestChart1.Visible = true;
            Collect_Step_miu1 = new List<double>();
            Collect_Step_delta1 = new List<double>();
            Collect_Step_E1 = new List<double>();
            ParamTestWaitingProcTitle = "采集信噪比计算所用图像中";
            textBox1.AppendText("开始固定曝光时间，按逐步改变光源照度方式采集图像\r\n");
            ParamTestWaitingProcTitle = "固定曝光时间，按逐步改变光源照度方式采集图像中";
            ParamTestWaitingProcMax = SystemParam.n + 1;
            ParamTestWaitingProc.MaxProgress = ParamTestWaitingProcMax;//(int)(ccdParamTestResult.Osat / n);
            wpf = new WaitingProcFunc(WaitingCollect_Step_1);
            if (!ParamTestWaitingProc.Execute(wpf, ParamTestWaitingProcTitle, WaitingType.With_ConfirmCancel, "是否取消？"))
            {
                textBox1.AppendText("用户终止自动测试\r\n");                
                return false;
            }
            return true;
        }
        bool CCDParamTest_Collect_Step2()
        {
            ParamTestWaitingProc = new WaitingProc();
            WaitingProcFunc wpf = null;
            if (Collect_Step_miu2 != null && Collect_Step_miu2.Count == (SystemParam.n + 1))
                return true;
            chartHide();
            ParamTestChart1.Visible = true;
            Collect_Step_miu2 = new List<double>();
            Collect_Step_delta2 = new List<double>();
            Collect_Step_E2 = new List<double>();
            Collect_Step_miu_dark2 = new List<double>();
            Collect_Step_delta_dark2 = new List<double>();
            textBox1.AppendText("开始固定光源照度，按逐步改变曝光时间方式采集图像\r\n");
            ParamTestWaitingProcTitle = "固定光源照度，按逐步改变曝光时间方式采集图像中";
            ParamTestWaitingProcMax = 2 * (SystemParam.n + 1);
            ParamTestWaitingProc.MaxProgress = ParamTestWaitingProcMax;
            wpf = new WaitingProcFunc(WaitingCollect_Step_2);
            if (!ParamTestWaitingProc.Execute(wpf, ParamTestWaitingProcTitle, WaitingType.With_ConfirmCancel, "是否取消？"))
            {
                textBox1.AppendText("用户终止自动测试\r\n");
                return false;
            }
            return true;
        }
        void WaitingCollect_Step_1(object LockWatingThread)
        {
            double Oe=0;
            double miu;
            double miuCC;
            double delta;
            double E = 0;
            //double E;
            this.Invoke((EventHandler)(delegate
            {
                WFGlobal.WaitMS(1);
                ParamTestChart1.Visible = true;
                ParamTestChart1.BringToFront();
                ParamTestChart1.Series["miu"].Points.Clear();
                ParamTestChart1.Series["delta"].Points.Clear();
                ParamTestChart1.Series["E"].Points.Clear();
            }));
            string[] strDatas=null;
            if (bTestMode == 2)
            {
                strDatas = File.ReadAllLines(strStepLogPath1 + SystemParam.lambda_Oe.ToString() + ".txt");
            }
            else
            {
                if (!UDPProc.CollectImage(this, ccdParamTestResult.NTmin1, 2))
                {
                    ParamTestWaitingProc.ExitWatting();
                    return;
                }
            }
            for (int i=0;i<(SystemParam.n+1);i++)
            {
                this.Invoke((EventHandler)(delegate
                {
                    tcpCCS.LightSet(SystemParam.lambda_Oe, Oe);
                    if(bTestMode==2)
                    {
                        string[] dataLine = strDatas[i].Split(',');
                        miu = double.Parse(dataLine[0]);
                        delta = double.Parse(dataLine[1]);
                        E = double.Parse(dataLine[2]);                        
                    }
                    else
                    {
                        if (bTestMode == 1)
                        {
                            //读取照片文件
                            UDPProc.ccdImageList = new List<ccdImage>(2);
                            UDPProc.ccdImageList.Add(new ccdImage());
                            UDPProc.ccdImageList.Add(new ccdImage());
                            UDPProc.ccdImageList[0].LoadFile(SystemParam.TempPicPath + "Step" + i.ToString("") + "_0.bin");
                            UDPProc.ccdImageList[1].LoadFile(SystemParam.TempPicPath + "Step" + i.ToString("") + "_1.bin");
                            //Collect_Step_E1.Add(i * 10);
                        }
                        else// (bTestMode == 0)
                        {
                            if (!UDPProc.CollectImage(this, ccdParamTestResult.NTmin1, 2))
                            {
                                ParamTestWaitingProc.ExitWatting();
                                return;
                            }
                            //UDPProc.ccdImageList[0].save(SystemParam.TempPicPath + "Step" + i.ToString("") + "_0.bin");
                            //UDPProc.ccdImageList[1].save(SystemParam.TempPicPath + "Step" + i.ToString("") + "_1.bin");
                        }
                        ccdImage.Calc_miu_delta(UDPProc.ccdImageList[0], UDPProc.ccdImageList[1], out miu, out delta, out miuCC);
                        //                         DeviceState ds = UDPProc.UDPCommand_04();
                        //                         if (ds == null)
                        //                         {
                        //                             textBox1.AppendText("照度采集失败，测试终止\r\n");
                        //                             ParamTestWaitingProc.ExitWatting();
                        //                             return;
                        //                         }
                        //                         E = ds.Illuminance;                        
                        E = Oe;
                        string strLog = miu.ToString("F6") + "," + delta.ToString("F6") + "," + E.ToString("F6");
                        TextLog.AddTextLog(strLog, strStepLogPath1 + SystemParam.lambda_Oe.ToString() + ".txt", false);
                    }
                    ParamTestChart1.Series["miu"].Points.AddXY(Oe, miu);
                    ParamTestChart1.Series["delta"].Points.AddXY(Oe, delta);
                    ParamTestChart1.Series["E"].Points.AddXY(Oe, E);
                    Collect_Step_miu1.Add(miu);
                    Collect_Step_delta1.Add(delta);
                    Collect_Step_E1.Add(E);

                    //                     if(miu>=SystemParam.miu_sat)//均值达到本文第二章（曝光步距）所确定的饱和均值
                    //                     {
                    //                         double max = Collect_Step_delta.Max();
                    //                         if(Collect_Step_delta.Last()<max*0.5)//方差由最大峰值迅速下降（超过50%）
                    //                         {
                    //                             return;
                    //                         }
                    //                     }
                    Oe += ccdParamTestResult.OeStep;
                    ParamTestWaitingProc.SetProcessBarPerformStep();
                    ParamTestWaitingProc.SetTitle(ParamTestWaitingProcTitle + ":" + (i+1).ToString() + "/" + ParamTestWaitingProcMax.ToString());
                }));
                if (ParamTestWaitingProc.HasBeenCancelled())
                {
                    return;
                }
            }
        }
        void WaitingCollect_Step_2(object LockWatingThread)
        {
            double miu=0;
            double miuCC;
            double delta;
            double E;
            int perIndex = 0;
            uint Tex;
            DeviceState ds;
            string strLog;
            //int stepCount=0;
            //double E;
            this.Invoke((EventHandler)(delegate
            {
                WFGlobal.WaitMS(1);
                ParamTestChart2.Visible = true;
                ParamTestChart2.BringToFront();
                ParamTestChart2.Series["miu"].Points.Clear();
                ParamTestChart2.Series["delta"].Points.Clear();
                ParamTestChart2.Series["miu_dark"].Points.Clear();
                ParamTestChart2.Series["delta_dark"].Points.Clear();
                tcpCCS.LightSet(SystemParam.lambda_Oe, ccdParamTestResult.OeLight);
            }));
            Tex = SystemParam.NTmin2;
            string[] strDatas=null;
            if (bTestMode == 2)
            {
                strDatas = File.ReadAllLines(strStepLogPath2L + SystemParam.lambda_Oe.ToString() + ".txt");
            }
            else
            {
                if (!UDPProc.CollectImage(this, Tex, 2))
                {
                    ParamTestWaitingProc.ExitWatting();
                    return;
                }
            }
            //明场
            for (int i = 0; i < (SystemParam.n + 1); i++)//while (true)
            {
                this.Invoke((EventHandler)(delegate
                {
                    if (bTestMode == 2)
                    {
                        string[] dataLine = strDatas[i].Split(',');
                        miu = double.Parse(dataLine[0]);
                        delta = double.Parse(dataLine[1]);
                        E = double.Parse(dataLine[2]);
                    }
                    else
                    {
                        if (bTestMode == 1)
                        {
                            //读取照片文件
                            UDPProc.ccdImageList = new List<ccdImage>(2);
                            UDPProc.ccdImageList.Add(new ccdImage());
                            UDPProc.ccdImageList.Add(new ccdImage());
                            UDPProc.ccdImageList[0].LoadFile(SystemParam.TempPicPath + "StepL" + i.ToString("") + "_0.bin");
                            UDPProc.ccdImageList[1].LoadFile(SystemParam.TempPicPath + "StepL" + i.ToString("") + "_1.bin");
                        }
                        else// (bTestMode == 0)
                        {
                            if (!UDPProc.CollectImage(this, Tex, 2))
                            {
                                ParamTestWaitingProc.ExitWatting();
                                return;
                            }
                            //UDPProc.ccdImageList[0].save(SystemParam.TempPicPath + "StepL" + i.ToString("") + "_0.bin");
                            //UDPProc.ccdImageList[1].save(SystemParam.TempPicPath + "StepL" + i.ToString("") + "_1.bin");
                        }
                        ccdImage.Calc_miu_delta(UDPProc.ccdImageList[0], UDPProc.ccdImageList[1], out miu, out delta, out miuCC);
                        //                         ds = UDPProc.UDPCommand_04();
                        //                         if (ds == null)
                        //                         {
                        //                             textBox1.AppendText("照度采集失败，测试终止\r\n");
                        //                             ParamTestWaitingProc.ExitWatting();
                        //                             return;
                        //                         }
                        //                         E = ds.Illuminance;
                        E = ccdParamTestResult.OeLight;
                        strLog = miu.ToString("F6") + "," + delta.ToString("F6") + "," + E.ToString("F6");
                        TextLog.AddTextLog(strLog, strStepLogPath2L + SystemParam.lambda_Oe.ToString() + ".txt", false);
                    }                    
                    ParamTestChart2.Series["miu"].Points.AddXY(SystemParam.GetTime(Tex), miu);
                    ParamTestChart2.Series["delta"].Points.AddXY(SystemParam.GetTime(Tex), delta);                   
                    Collect_Step_miu2.Add(miu);
                    Collect_Step_delta2.Add(delta);
                    Collect_Step_E2.Add(E);
                    Tex += ccdParamTestResult.NTexp2;
                    //stepCount++;
                    perIndex++;
                    ParamTestWaitingProc.SetTitle(ParamTestWaitingProcTitle + ":" + perIndex.ToString() + "/" + ParamTestWaitingProcMax.ToString());
                    WFGlobal.WaitMS(1);
                    ParamTestWaitingProc.SetProcessBarPerformStep();
                }));
                //                 if (miu >= SystemParam.miu_sat)//均值达到本文第二章（曝光步距）所确定的饱和均值
                //                 {
                //                     double max = Collect_Step_delta.Max();
                //                     if (Collect_Step_delta.Last() < max * 0.5)//方差由最大峰值迅速下降（超过50%）
                //                     {
                //                         break; 
                //                     }
                //                 }
                if (ParamTestWaitingProc.HasBeenCancelled())
                {
                    return;
                }
            }
            this.Invoke((EventHandler)(delegate
            {
                WFGlobal.WaitMS(1);
                tcpCCS.LightSet(SystemParam.lambda_Oe, 0);
            }));
            Tex = SystemParam.NTmin2;
            if (bTestMode == 2)
                strDatas = File.ReadAllLines(strStepLogPath2D + SystemParam.lambda_Oe.ToString() + ".txt");
            else
            {
                if (!UDPProc.CollectImage(this, Tex, 2))
                {
                    ParamTestWaitingProc.ExitWatting();
                    return;
                }
            }
            for (int i = 0; i < (SystemParam.n + 1); i++)
            {
                this.Invoke((EventHandler)(delegate
                {
                    if (bTestMode == 2)
                    {
                        string[] dataLine = strDatas[i].Split(',');
                        miu = double.Parse(dataLine[0]);
                        delta = double.Parse(dataLine[1]);
                        //E = double.Parse(dataLine[2]);
                    }
                    else
                    {
                        if (bTestMode == 1)
                        {
                            //读取照片文件
                            UDPProc.ccdImageList = new List<ccdImage>(2);
                            UDPProc.ccdImageList.Add(new ccdImage());
                            UDPProc.ccdImageList.Add(new ccdImage());
                            UDPProc.ccdImageList[0].LoadFile(SystemParam.TempPicPath + "StepD" + i.ToString("") + "_0.bin");
                            UDPProc.ccdImageList[1].LoadFile(SystemParam.TempPicPath + "StepD" + i.ToString("") + "_1.bin");
                        }
                        else// (bTestMode == 0)
                        {
                            if (!UDPProc.CollectImage(this, Tex, 2))
                            {
                                ParamTestWaitingProc.ExitWatting();
                                return;
                            }
                            //UDPProc.ccdImageList[0].save(SystemParam.TempPicPath + "StepD" + i.ToString("") + "_0.bin");
                            //UDPProc.ccdImageList[1].save(SystemParam.TempPicPath + "StepD" + i.ToString("") + "_1.bin");
                        }
                        ccdImage.Calc_miu_delta(UDPProc.ccdImageList[0], UDPProc.ccdImageList[1], out miu, out delta, out miuCC);
                        strLog = miu.ToString("F6") + "," + delta.ToString("F6");
                        TextLog.AddTextLog(strLog, strStepLogPath2D + SystemParam.lambda_Oe.ToString() + ".txt", false);
                    }
                    ParamTestChart2.Series["miu_dark"].Points.AddXY(SystemParam.GetTime(Tex), miu);
                    ParamTestChart2.Series["delta_dark"].Points.AddXY(SystemParam.GetTime(Tex), delta);
                    Collect_Step_miu_dark2.Add(miu);
                    Collect_Step_delta_dark2.Add(delta);
                    Tex += ccdParamTestResult.NTexp2;
                    //stepCount++;
                    perIndex++;
                    ParamTestWaitingProc.SetTitle(ParamTestWaitingProcTitle + ":" + perIndex.ToString() + "/" + ParamTestWaitingProcMax.ToString());
                    WFGlobal.WaitMS(1);
                    ParamTestWaitingProc.SetProcessBarPerformStep();
                }));
            }
        }
    }
}
