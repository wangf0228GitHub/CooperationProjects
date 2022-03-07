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
        List<double> Collect_DarkI_miu;
        List<double> Collect_DarkI_delta;
        bool CCDParamTest_DarkI()
        {
            exposureChart.ChartAreas[0].AxisX.Title = "曝光时间";
            if (double.IsNaN(ccdParamTestResult.K))
            {
                if (!CCDParamTest_K())
                    return false;
            }
            chartHide();
            exposureChart.Visible = true;
            Collect_DarkI_miu = new List<double>();
            Collect_DarkI_delta = new List<double>();
            ParamTestWaitingProc = new WaitingProc();

            WaitingProcFunc wpf = null;
            textBox1.AppendText("测量暗电流，暗场并按改变曝光时间方式采集图像\r\n");
  
            ParamTestWaitingProcTitle = "测量暗电流，暗场并按改变曝光时间方式采集图像中";
            ParamTestWaitingProcMax = 15;
            ParamTestWaitingProc.MaxProgress = ParamTestWaitingProcMax;

            wpf = new WaitingProcFunc(WaitingCollect_DarkI);
            if (!ParamTestWaitingProc.Execute(wpf, ParamTestWaitingProcTitle, WaitingType.With_ConfirmCancel, "是否取消？"))
            {
                textBox1.AppendText("用户终止自动测试\r\n");
                return false;
            }
            double[] texp = new double[Collect_DarkI_miu.Count];
            for (int i = 0; i < Collect_DarkI_miu.Count; i++)
            {
                double NTexp = SystemParam.NTdark + i * SystemParam.delta_Tdark;
                texp[i] = NTexp / 20 / 1000000;
            }
            double[] fitret = FittingMultiLine.MultiLine(texp, Collect_DarkI_miu.ToArray(), Collect_DarkI_miu.Count, 1);
            ccdParamTestResult.miu_I_miu = fitret[1] / ccdParamTestResult.K;

            chart3.Series[0].Points.Clear();
            chart3.Series[1].Points.Clear();
            chart3.ChartAreas[0].AxisY.Title = "暗场均值";
            chart3.ChartAreas[0].AxisX.Title = "曝光时间(s)";
            for (int i = 0; i < Collect_DarkI_miu.Count; i++)
            {
                chart3.Series[0].Points.AddXY(texp[i], Collect_DarkI_miu[i]);
                chart3.Series[1].Points.AddXY(texp[i], texp[i] * fitret[1] + fitret[0]);
            }
            chart3.SaveImage(SystemParam.TempPicPath + "DarkI1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);


            fitret = FittingMultiLine.MultiLine(texp, Collect_DarkI_delta.ToArray(), Collect_DarkI_delta.Count, 1);
            ccdParamTestResult.miu_I_delta = fitret[1] / ccdParamTestResult.K / ccdParamTestResult.K;

            chart3.Series[0].Points.Clear();
            chart3.Series[1].Points.Clear();
            chart3.ChartAreas[0].AxisY.Title = "暗场方差";
            chart3.ChartAreas[0].AxisX.Title = "曝光时间(s)";
            for (int i = 0; i < Collect_DarkI_miu.Count; i++)
            {
                chart3.Series[0].Points.AddXY(texp[i], Collect_DarkI_delta[i]);
                chart3.Series[1].Points.AddXY(texp[i], texp[i] * fitret[1] + fitret[0]);
            }
            chart3.SaveImage(SystemParam.TempPicPath + "DarkI2.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

            ccdParamTestResult.bDarkICurve = true;
            return true;
        }
        void WaitingCollect_DarkI(object LockWatingThread)
        {
            double miu = 0;
            double miuCC;
            double delta;
            uint Tex;
            //double E;
            this.Invoke((EventHandler)(delegate
            {
                WFGlobal.WaitMS(1);
                exposureChart.Visible = true;
                exposureChart.BringToFront();
                exposureChart.Series["miu"].Points.Clear();
                exposureChart.Series["delta"].Points.Clear();
                tcpCCS.LightSet(SystemParam.lambda_Oe, 0.0);//暗场
            }));
            Tex = (uint)SystemParam.NTdark;
            string[] strDatas = null;
            if (bTestMode == 2)
                strDatas = File.ReadAllLines(strDarkILogPath + SystemParam.lambda_Oe.ToString() + ".txt");
            else
            {
                if (!UDPProc.CollectImage(this, Tex, 2))
                {
                    ParamTestWaitingProc.ExitWatting();
                    return;
                }
            }
            //明场
            for (int i=0;i<15;i++)
            {
                this.Invoke((EventHandler)(delegate
                {
                    if (bTestMode == 2)
                    {
                        string[] dataLine = strDatas[i].Split(',');
                        miu = double.Parse(dataLine[0]);
                        delta = double.Parse(dataLine[1]);
                    }
                    else
                    {
                        if (bTestMode == 1)
                        {
                                                        
                        }
                        else// (bTestMode == 0)
                        {
                            if (!UDPProc.CollectImage(this, Tex, 2))
                            {
                                ParamTestWaitingProc.ExitWatting();
                                return;
                            }
                            //UDPProc.ccdImageList[0].save(SystemParam.TempPicPath + "DarkI" + Tex.ToString("") + "_0.bin");
                            //UDPProc.ccdImageList[1].save(SystemParam.TempPicPath + "DarkI" + Tex.ToString("") + "_1.bin");
                        }
                        ccdImage.Calc_miu_delta(UDPProc.ccdImageList[0], UDPProc.ccdImageList[1], out miu, out delta, out miuCC);

                        string strLog = miu.ToString("F6") + "," + delta.ToString("F6");
                        TextLog.AddTextLog(strLog, strDarkILogPath + SystemParam.lambda_Oe.ToString() + ".txt", false);
                    }
                    exposureChart.Series["miu"].Points.AddXY(SystemParam.GetTime(Tex), miu);
                    exposureChart.Series["delta"].Points.AddXY(SystemParam.GetTime(Tex), delta);                    
                    Collect_DarkI_miu.Add(miu);
                    Collect_DarkI_delta.Add(delta);                    
                }));                
                if (ParamTestWaitingProc.HasBeenCancelled())
                {
                    return;
                }
                Tex += (uint)SystemParam.delta_Tdark;
                ParamTestWaitingProc.SetProcessBarPerformStep();
                ParamTestWaitingProc.SetTitle(ParamTestWaitingProcTitle + ":" + (i+1).ToString() + "/" + ParamTestWaitingProcMax.ToString());
                WFGlobal.WaitMS(1);
            }
        }
    }
}
