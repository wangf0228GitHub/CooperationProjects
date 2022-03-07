using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WFNetLib;

namespace udpCCDTest
{
    public partial class FormMain : Form
    {
        List<double> Collect_PGA_miu;
        List<double> Collect_PGA_delta;
        bool CCDParamTest_PGACurve()
        {
            UIHide();
            exposureChart.Visible = true;
            exposureChart.BringToFront();
            exposureChart.ChartAreas[0].AxisX.Title = "增益";
            exposureListView.Visible = true;
            exposureChart.Series["miu"].Points.Clear();
            exposureChart.Series["delta"].Points.Clear();
            int pga = SystemParam.CCD_PGA;
            ParamTestWaitingProc = new WaitingProc();
            WaitingProcFunc wpf = null;

            Collect_PGA_miu = new List<double>();
            Collect_PGA_delta = new List<double>();
            textBox1.AppendText("开始采集增益曲线所用图像\r\n");
            ParamTestWaitingProcTitle = "采集增益曲线所用图像中";
            ParamTestWaitingProcMax = 7;
            ParamTestWaitingProc.MaxProgress = ParamTestWaitingProcMax;//(int)(ccdParamTestResult.Osat / n);
            wpf = new WaitingProcFunc(WaitingCollect_PGA);
            if (!ParamTestWaitingProc.Execute(wpf, ParamTestWaitingProcTitle, WaitingType.With_ConfirmCancel, "是否取消？"))
            {
                textBox1.AppendText("用户终止自动测试\r\n");
                SystemParam.CCD_PGA = pga;
                UDPProc.UDPCommand_01();
                return false;
            }
            chart1.Series[0].Points.Clear();
            chart1.ChartAreas[0].AxisY.Title = "均值";
            chart1.ChartAreas[0].AxisX.Title = "增益";
            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "F2";
            for (int i = 0; i < Collect_PGA_miu.Count; i++)
            {
                SystemParam.CCD_PGA = i;
                chart1.Series[0].Points.AddXY(SystemParam.GetPGA(), Collect_PGA_miu[i]);
            }            
            chart1.SaveImage(SystemParam.TempPicPath + "PGA.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            SystemParam.CCD_PGA = pga;
            UDPProc.UDPCommand_01();
            ccdParamTestResult.bPGACurve = true;
            return true;
        }

        void WaitingCollect_PGA(object LockWatingThread)
        {
            double miu = 0;
            double miuCC = 0;
            double delta = 0;
            this.Invoke((EventHandler)(delegate
            {
                tcpCCS.LightSet(SystemParam.lambda_Oe, ccdParamTestResult.Osat / 10);
                if (!UDPProc.CollectImage(this, ccdParamTestResult.NTmin1, 2))
                {
                    ParamTestWaitingProc.ExitWatting();
                    return;
                }
            }));
            for (int i = 0; i < 7; i++)
            {
                SystemParam.CCD_PGA = i;
                this.Invoke((EventHandler)(delegate
                {
                    if (UDPProc.UDPCommand_01()==null)
                    {
                        ParamTestWaitingProc.ExitWatting();
                        return;
                    }
                    WFGlobal.WaitMS(50);
                    if (!UDPProc.CollectImage(this, ccdParamTestResult.NTmin1, 2))
                    {
                        ParamTestWaitingProc.ExitWatting();
                        return;
                    }
                    ccdImage.Calc_miu_delta(UDPProc.ccdImageList[0], UDPProc.ccdImageList[1], out miu, out delta, out miuCC);
                    exposureChart.Series["miu"].Points.AddXY(SystemParam.GetPGA(), miu);
                    exposureChart.Series["delta"].Points.AddXY(SystemParam.GetPGA(), delta);
                    Collect_PGA_miu.Add(miu);
                    Collect_PGA_delta.Add(delta);                    
                    ParamTestWaitingProc.SetProcessBarPerformStep();
                    ParamTestWaitingProc.SetTitle(ParamTestWaitingProcTitle + ":" + (i+1).ToString() + "/" + ParamTestWaitingProcMax.ToString());
                    WFGlobal.WaitMS(1);
                }));
                if (ParamTestWaitingProc.HasBeenCancelled())
                {
                    return;
                }
            }
        }
    }
}
