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
        bool CCDParamTest_SNR()
        {
            if (!CCDParamTest_Collect_SNR())
            {
                MessageBox.Show("曲线采集失败");
                return false;
            }
            if (double.IsNaN(ccdParamTestResult.K))
            {
                if (!CCDParamTest_K())
                    return false;
            }
            if (double.IsNaN(ccdParamTestResult.eta))
            {
                if (!CCDParamTest_eta())
                    return false;
            }
//             double[] miu_p;
//             double[] SNR;
//             int count = 2 * SystemParam.n;
//             miu_p = new double[count];
//             SNR = new double[count];
            if(ExType == 1)
                ccdParamTestResult.miu_p_sat = 50.34 * SystemParam.CCD_Sa * SystemParam.CCD_Sb * SystemParam.GetTime(ccdParamTestResult.NTmin1) * SystemParam.lambda_Oe / 1000 * ccdParamTestResult.Osat;
            else
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
                ccdParamTestResult.miu_p_sat = 50.34 * SystemParam.CCD_Sa * SystemParam.CCD_Sb * SystemParam.GetTime2(nsat,ccdParamTestResult.NTexp2) * SystemParam.lambda_Oe / 1000 * ccdParamTestResult.OeLight;
            }
            ccdParamTestResult.SNRMax = 20 * Math.Log10(ccdParamTestResult.eta * ccdParamTestResult.miu_p_sat / Math.Sqrt(Collect_SNR_delta_dark + ccdParamTestResult.eta * ccdParamTestResult.miu_p_sat));
            chart4.Series[0].Points.Clear();
            chart4.Series[1].Points.Clear();
            chart4.ChartAreas[0].AxisY.Title = "SNR";
            chart4.ChartAreas[0].AxisX.Title = "光子均值lg()";
            chart4.ChartAreas[0].AxisX.LabelStyle.Format = "F0";
            ccdParamTestResult.miu_p_min = (Math.Sqrt(Collect_SNR_delta_dark / ccdParamTestResult.K / ccdParamTestResult.K + 0.25) + 0.5) / ccdParamTestResult.eta;
            double miu_p_step = ccdParamTestResult.miu_p_sat*0.1 / SystemParam.n;
            double miu_p=1, SNR;
            //for (int i = 0; i < SystemParam.n; i++)
            while(miu_p< ccdParamTestResult.miu_p_sat * 0.1)
            {                
                //SNR[i] = Math.Abs((Collect_SNR_miu[i + 1] - Collect_SNR_miu[0])/ccdParamTestResult.K) / Math.Sqrt(Math.Pow(Collect_SNR_delta[0], 2) + ccdParamTestResult.eta * (miu_p[i] - miu_p_NTmin));
                SNR = ccdParamTestResult.eta * miu_p/ Math.Sqrt(Collect_SNR_delta_dark + ccdParamTestResult.eta * miu_p);
                //SNR[i] = ccdParamTestResult.eta * miu_p[i] / Math.Sqrt(Collect_SNR_delta[0] + 1 / 144 / ccdParamTestResult.K / ccdParamTestResult.K + ccdParamTestResult.eta * miu_p[i]);
                chart4.Series[0].Points.AddXY(miu_p,SNR);//(Math.Pow(miu_p[i], 20), Math.Pow(SNR[i], 20));// 20 * Math.Log10(miu_p[i]), 20 * Math.Log10(SNR[i]));
                chart4.Series[1].Points.AddXY(miu_p, Math.Sqrt(miu_p));
                miu_p += 5;
            }
            miu_p_step = ccdParamTestResult.miu_p_sat * 0.9 / SystemParam.n/2;
            for (int i = 0; i < 2*SystemParam.n; i++)
            {                
                //SNR[i] = Math.Abs((Collect_SNR_miu[i + 1] - Collect_SNR_miu[0])/ccdParamTestResult.K) / Math.Sqrt(Math.Pow(Collect_SNR_delta[0], 2) + ccdParamTestResult.eta * (miu_p[i] - miu_p_NTmin));
                SNR = ccdParamTestResult.eta * miu_p / Math.Sqrt(Collect_SNR_delta_dark + ccdParamTestResult.eta * miu_p);
                //SNR[i] = ccdParamTestResult.eta * miu_p[i] / Math.Sqrt(Collect_SNR_delta[0] + 1 / 144 / ccdParamTestResult.K / ccdParamTestResult.K + ccdParamTestResult.eta * miu_p[i]);
                chart4.Series[0].Points.AddXY(miu_p, SNR);//(Math.Pow(miu_p[i], 20), Math.Pow(SNR[i], 20));// 20 * Math.Log10(miu_p[i]), 20 * Math.Log10(SNR[i]));
                chart4.Series[1].Points.AddXY(miu_p, Math.Sqrt(miu_p));
                miu_p +=  miu_p_step;
            }
            chart4.SaveImage(SystemParam.TempPicPath + "SNR.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            ccdParamTestResult.bSNRCurve = true;
            return true;
        }
        double Collect_SNR_miu_dark = 0;
        double Collect_SNR_delta_dark = 0;
        bool CCDParamTest_Collect_SNR()
        {
            ParamTestWaitingProc = new WaitingProc();
            WaitingProcFunc wpf = null;
            textBox1.AppendText("开始采集信噪比计算所用图像\r\n");
            ParamTestWaitingProcTitle = "采集信噪比计算所用图像中";
            ParamTestWaitingProcMax= 10;
            ParamTestWaitingProc.MaxProgress = ParamTestWaitingProcMax;//(int)(ccdParamTestResult.Osat / n);
            wpf = new WaitingProcFunc(WaitingCollect_SNR);
            if (!ParamTestWaitingProc.Execute(wpf, ParamTestWaitingProcTitle, WaitingType.With_ConfirmCancel, "是否取消？"))
            {
                textBox1.AppendText("用户终止自动测试\r\n");
                return false;
            }
            return true;
        }
        void WaitingCollect_SNR(object LockWatingThread)
        {
            double miu = 0;
            double miuCC = 0;
            double delta = 0;
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
                tcpCCS.LightSet(SystemParam.lambda_Oe, 0);
            }));
            if (!UDPProc.CollectImage(this, SystemParam.NTmin2, 2))
            {
                ParamTestWaitingProc.ExitWatting();
                return;
            }
            double miu_sum = 0;
            double delta_sum = 0;
            for (int i = 0; i < 10; i++)
            {
                this.Invoke((EventHandler)(delegate
                {
                    if (!UDPProc.CollectImage(this, SystemParam.NTmin2, 2))
                    {
                        ParamTestWaitingProc.ExitWatting();
                        return;
                    }
                    ccdImage.Calc_miu_delta(UDPProc.ccdImageList[0], UDPProc.ccdImageList[1], out miu, out delta, out miuCC);

                    ParamTestChart1.Series["miu"].Points.AddXY(i, miu);
                    ParamTestChart1.Series["delta"].Points.AddXY(i, delta);
                    ParamTestChart1.Series["E"].Points.AddXY(i, E);

                    miu_sum += miu;
                    delta_sum += delta;
                    ParamTestWaitingProc.SetProcessBarPerformStep();
                    ParamTestWaitingProc.SetTitle(ParamTestWaitingProcTitle + ":" + (i + 1).ToString() + "/" + ParamTestWaitingProcMax.ToString());
                    WFGlobal.WaitMS(1);
                }));
                if (ParamTestWaitingProc.HasBeenCancelled())
                {
                    return;
                }
            }
            Collect_SNR_miu_dark = miu_sum / 10;
            Collect_SNR_delta_dark = delta_sum / 10;
        }
    }
}
