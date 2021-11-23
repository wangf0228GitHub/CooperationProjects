using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WFNetLib.Algorithm;

namespace udpCCDTest
{
    public partial class FormMain : Form
    {
        void CCDParamTest_Calc_K()
        {
            int K_Count = Collect_Step_miu.Count * (int)((SystemParam.L_TOP - SystemParam.L_BTM) / 100.0);
            int K_Start = Collect_Step_miu.Count * (int)(SystemParam.L_BTM / 100.0);

            if (ExType == 1)//固定曝光时间，改变光源照度
            {
                double[] K_delta = new double[K_Count];
                double[] K_miu = new double[K_Count];
                chart1.Series[0].Points.Clear();
                chart2.Series[0].Points.Clear();
                chart1.ChartAreas[0].AxisY.Title = "均值";
                chart1.ChartAreas[0].AxisX.Title = "照度";
                chart2.ChartAreas[0].AxisY.Title = "方差";
                chart2.ChartAreas[0].AxisX.Title = "照度";
                for (int i = 0; i < K_Count; i++)
                {
                    K_miu[i] = Collect_Step_miu[K_Start + i];
                    K_delta[i] = Collect_Step_delta[K_Start + i];

                    chart1.Series[0].Points.AddXY(Collect_Step_E[K_Start + i], Collect_Step_miu[K_Start + i]);
                    chart2.Series[0].Points.AddXY(Collect_Step_E[K_Start + i], Collect_Step_delta[K_Start + i]);
                }
                double[] fitret = FittingMultiLine.MultiLine(K_miu, K_delta, K_Count, 1);
                CCDParamTestResult.K = fitret[1];
                chart1.SaveImage(SystemParam.TempPicPath + "K_1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                chart2.SaveImage(SystemParam.TempPicPath + "K_2.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                chart3.Series[0].Points.Clear();
                chart3.Series[1].Points.Clear();
                chart3.ChartAreas[0].AxisY.Title = "方差";
                chart3.ChartAreas[0].AxisX.Title = "均值";
                for (int i = 0; i < K_Count; i++)
                {
                    chart3.Series[0].Points.AddXY(K_miu[i],K_delta[i]);
                    chart3.Series[1].Points.AddXY(K_miu[i], K_miu[i] * fitret[1] + fitret[0]);
                }
                chart3.SaveImage(SystemParam.TempPicPath + "KL.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            else if (ExType == 2)//固定光源照度，改变曝光时间
            {
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
                    K_miu[i] = Collect_Step_miu[K_Start + i];
                    K_delta[i] = Collect_Step_delta[K_Start + i];
                    NTexp[i] = SystemParam.NTmin + SystemParam.NTexp * i;
                    chart1.Series[0].Points.AddXY(NTexp[i], K_miu[i]);
                    chart2.Series[0].Points.AddXY(NTexp[i], K_delta[i]);
                }
                double[] fitret_miu = FittingMultiLine.MultiLine(K_miu, NTexp, K_Count, 1);
                double[] fitret_delta = FittingMultiLine.MultiLine(K_delta, NTexp, K_Count, 1);
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
                    K_miu[i] = Collect_Step_miu_dark[K_Start + i];
                    K_delta[i] = Collect_Step_delta_dark[K_Start + i];
                    chart1.Series[0].Points.AddXY(NTexp[i], K_miu[i]);
                    chart2.Series[0].Points.AddXY(NTexp[i], K_delta[i]);
                }
                double[] fitret_miu_dark = FittingMultiLine.MultiLine(K_miu, NTexp, K_Count, 1);
                double[] fitret_delta_dark = FittingMultiLine.MultiLine(K_delta, NTexp, K_Count, 1);
                CCDParamTestResult.K = (fitret_delta[1] - fitret_delta_dark[1]) / (fitret_miu[1] - fitret_miu_dark[1]);
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
            CCDParamTestResult.bmiuCurve = true;
        }
        void CCDParamTest_Calc_eta()
        {
            int K_Count = Collect_Step_miu.Count * (int)((SystemParam.L_TOP - SystemParam.L_BTM) / 100.0);
            int K_Start = Collect_Step_miu.Count * (int)(SystemParam.L_BTM / 100.0);
            double[] miu_p = new double[K_Count];
            double[] miu_y = new double[K_Count];
            for (int i = 0; i < K_Count; i++)
            {
                miu_y[i] = Collect_Step_miu[K_Start + i];
                double texp;
                if (ExType == 1)//固定曝光时间，改变光源照度
                {
                    texp = SystemParam.GetTime(0);
                    miu_p[i] = 50.34 * SystemParam.CCD_Sa * SystemParam.CCD_Sb * texp * SystemParam.lambda_Oe * Collect_Step_E[K_Start + i];
                }
                else if (ExType == 2)//固定光源照度，改变曝光时间
                {
                    texp = SystemParam.GetTime(i);
                    miu_p[i] = 50.34 * SystemParam.CCD_Sa * SystemParam.CCD_Sb * texp * SystemParam.lambda_Oe * SystemParam.Oe;
                }
            }
            double[] fitret = FittingMultiLine.MultiLine(miu_p, miu_y, K_Count, 1);
            CCDParamTestResult.eta = fitret[1] / CCDParamTestResult.K;

            chart3.Series[0].Points.Clear();
            chart3.Series[1].Points.Clear();
            chart3.ChartAreas[0].AxisY.Title = "均值";
            chart3.ChartAreas[0].AxisX.Title = "光子均值";
            for (int i = 0; i < K_Count; i++)
            {
                chart3.Series[0].Points.AddXY(miu_p[i], miu_y[i]);
                chart3.Series[1].Points.AddXY(miu_p[i], miu_p[i] * fitret[1] + fitret[0]);
            }
            chart3.SaveImage(SystemParam.TempPicPath + "eta.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
        }
        void CCDParamTest_Calc_etaCurve()
        {
            chart1.Series[0].Points.Clear();
            chart1.ChartAreas[0].AxisY.Title = "量子效率";
            chart1.ChartAreas[0].AxisX.Title = "波长";

            CCDParamTestResult.etaCurve = new double[Collect_etaCurve_miu.Count];
            for (int j = 0; j < Collect_etaCurve_miu.Count; j++)
            {
                List<double> miu_y_o = Collect_etaCurve_miu[j];
                int K_Count = Collect_etaCurve_miu.Count * (int)((SystemParam.L_TOP - SystemParam.L_BTM) / 100.0);
                int K_Start = Collect_etaCurve_miu.Count * (int)(SystemParam.L_BTM / 100.0);
                double[] miu_p = new double[K_Count];
                double[] miu_y = new double[K_Count];
                double lambda_Oe = SystemParam.L_lambda + j * SystemParam.delta_lambda;
                for (int i = 0; i < K_Count; i++)
                {
                    miu_y[i] = miu_y_o[K_Start + i];
                    double texp;
                    if (ExType == 1)//固定曝光时间，改变光源照度
                    {
                        texp = 1000 * SystemParam.Np * SystemParam.NTmin / SystemParam.CCD_phi / 1000000;
                        miu_p[i] = 50.34 * SystemParam.CCD_Sa * SystemParam.CCD_Sb * texp * lambda_Oe * Collect_etaCurve_E[K_Start + i];
                    }
                    else if (ExType == 2)//固定光源照度，改变曝光时间
                    {
                        texp = 1000 * SystemParam.Np * (SystemParam.NTmin + SystemParam.NTexp * i) / SystemParam.CCD_phi / 1000000;
                        miu_p[i] = 50.34 * SystemParam.CCD_Sa * SystemParam.CCD_Sb * texp * lambda_Oe * SystemParam.Oe;
                    }
                }
                double[] fitret = FittingMultiLine.MultiLine(miu_p, miu_y, K_Count, 1);
                CCDParamTestResult.etaCurve[j] = fitret[1] / CCDParamTestResult.K;
                chart1.Series[0].Points.AddXY(lambda_Oe, CCDParamTestResult.etaCurve[j]);

                chart3.Series[0].Points.Clear();
                chart3.Series[1].Points.Clear();
                chart3.ChartAreas[0].AxisY.Title = "均值";
                chart3.ChartAreas[0].AxisX.Title = "光子均值";
                for (int i = 0; i < K_Count; i++)
                {
                    chart3.Series[0].Points.AddXY(miu_p[i], miu_y[i]);
                    chart3.Series[1].Points.AddXY(miu_p[i], miu_p[i] * fitret[1] + fitret[0]);
                }
                chart3.SaveImage(SystemParam.TempPicPath + "eta"+j.ToString()+".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            chart1.SaveImage(SystemParam.TempPicPath + "etaCurve.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            CCDParamTestResult.betaCurve = true;
        }
        void CCDParamTest_Calc_SNR()
        {
            double[] miu_p = new double[Collect_Step_miu.Count];
            double[] SNR = new double[Collect_Step_miu.Count];
            if (ExType == 1)//固定曝光时间，改变光源照度
            {
                
                for (int i = 0; i < Collect_Step_miu.Count; i++)
                {
                    miu_p[i] = 50.34 * SystemParam.CCD_Sa * SystemParam.CCD_Sb * SystemParam.GetTime(0) * SystemParam.lambda_Oe * Collect_Step_E[i];
                    SNR[i]= CCDParamTestResult.eta * miu_p[i] / Math.Sqrt(Collect_Step_delta[0] + 1 / 144 / CCDParamTestResult.K / CCDParamTestResult.K + CCDParamTestResult.eta * miu_p[i]);
                }
                CCDParamTestResult.miu_p_min = (Math.Sqrt(Collect_Step_delta[0] / CCDParamTestResult.K / CCDParamTestResult.K + 0.25) + 0.5) / CCDParamTestResult.eta;
                CCDParamTestResult.miu_p_sat = SNR.Last();
            }
            else if (ExType == 2)//固定光源照度，改变曝光时间
            {
                for (int i = 0; i < Collect_Step_miu.Count; i++)
                {
                    miu_p[i] = 50.34 * SystemParam.CCD_Sa * SystemParam.CCD_Sb * SystemParam.GetTime(i) * SystemParam.lambda_Oe * SystemParam.Oe;
                    SNR[i] = CCDParamTestResult.eta * miu_p[i] / Math.Sqrt(Collect_Step_delta_dark[0] + 1 / 144 / CCDParamTestResult.K / CCDParamTestResult.K + CCDParamTestResult.eta * miu_p[i]);
                }
                CCDParamTestResult.miu_p_min = (Math.Sqrt(Collect_Step_delta_dark[0] / CCDParamTestResult.K / CCDParamTestResult.K + 0.25) + 0.5) / CCDParamTestResult.eta;
                CCDParamTestResult.miu_p_sat = SNR.Last();
            }
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            chart1.ChartAreas[0].AxisY.Title = "20lg[SNR]";
            chart1.ChartAreas[0].AxisX.Title = "20lg[光子均值]";
            for (int i = 0; i < Collect_Step_miu.Count; i++)
            {
                chart1.Series[0].Points.AddXY(20 * Math.Log10(miu_p[i]), 20 * Math.Log10(SNR[i]));
                chart1.Series[1].Points.AddXY(20 * Math.Log10(miu_p[i]), 20 * Math.Log10(Math.Sqrt(miu_p[i])));
            }
            chart1.SaveImage(SystemParam.TempPicPath + "SNR.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            CCDParamTestResult.bSNRCurve = true;
        }
        void CCDParamTest_Calc_DR()
        {
            double miu_p_min = (Math.Sqrt(delta_dark / CCDParamTestResult.K / CCDParamTestResult.K + 0.25) + 0.5) / CCDParamTestResult.eta;
            double texp = 1000 * SystemParam.Np * SystemParam.NTmin / SystemParam.CCD_phi / 1000000;
            double miu_p_sat = 50.34 * SystemParam.CCD_Sa * SystemParam.CCD_Sb * texp * SystemParam.lambda_Oe * SystemParam.Osat;
            CCDParamTestResult.DR = 20 * Math.Log10(miu_p_sat / miu_p_min);
        }
        void CCDParamTest_Calc_FWC()
        {
            CCDParamTestResult.FWC = (miu_sat - miu_dark) / CCDParamTestResult.K;
        }
        void CCDParamTest_Calc_DarkI()
        {
            double[] texp = new double[Collect_DarkI_miu.Count];
            for (int i = 0; i < Collect_DarkI_miu.Count; i++)
            {
                double NTexp = SystemParam.NTdark + i * SystemParam.delta_Tdark;
                texp[i] = NTexp / SystemParam.CCD_phi / 1000000;
            }
            double[] fitret = FittingMultiLine.MultiLine(texp, Collect_DarkI_miu.ToArray(), Collect_DarkI_miu.Count, 1);
            CCDParamTestResult.miu_I_miu = fitret[1] / CCDParamTestResult.K;

            chart3.Series[0].Points.Clear();
            chart3.Series[1].Points.Clear();
            chart3.ChartAreas[0].AxisY.Title = "暗场均值";
            chart3.ChartAreas[0].AxisX.Title = "曝光时间";
            for (int i = 0; i < Collect_DarkI_miu.Count; i++)
            {
                chart3.Series[0].Points.AddXY(texp[i], Collect_DarkI_miu[i]);
                chart3.Series[1].Points.AddXY(texp[i], texp[i] * fitret[1] + fitret[0]);
            }
            chart3.SaveImage(SystemParam.TempPicPath + "DarkI1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);


            fitret = FittingMultiLine.MultiLine(texp, Collect_DarkI_delta.ToArray(), Collect_DarkI_delta.Count, 1);
            CCDParamTestResult.miu_I_delta = fitret[1] / CCDParamTestResult.K / CCDParamTestResult.K;

            chart3.Series[0].Points.Clear();
            chart3.Series[1].Points.Clear();
            chart3.ChartAreas[0].AxisY.Title = "暗场方差";
            chart3.ChartAreas[0].AxisX.Title = "曝光时间";
            for (int i = 0; i < Collect_DarkI_miu.Count; i++)
            {
                chart3.Series[0].Points.AddXY(texp[i], Collect_DarkI_delta[i]);
                chart3.Series[1].Points.AddXY(texp[i], texp[i] * fitret[1] + fitret[0]);
            }
            chart3.SaveImage(SystemParam.TempPicPath + "DarkI2.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

            CCDParamTestResult.bDarkICurve = true;
        }
        void CCDParamTest_Calc_LE()
        {
            int Count = Collect_Step_miu.Count * (int)((95 - 5) / 100.0);
            int Start = Collect_Step_miu.Count * (int)(5 / 100.0);
            double[] y = new double[Count];
            double[] H = new double[Count];
            if (ExType == 1)//固定曝光时间，改变光源照度
            {
                for (int i = 0; i < Count; i++)
                {
                    y[i] = Collect_Step_miu[Start + i] - Collect_Step_miu[0];
                    H[i] = 1000 * Collect_Step_E[Start + i] * SystemParam.NTmin / SystemParam.CCD_phi / 1000000;
                }
            }
            else// if (ExType == 2)//固定光源照度，改变曝光时间
            {
                for (int i = 0; i < Count; i++)
                {
                    y[i] = Collect_Step_miu[Start + i] - Collect_Step_miu_dark[i];
                    H[i] = 1000 * Collect_Step_E[Start + i] * (SystemParam.NTmin + i * SystemParam.NTexp) / SystemParam.CCD_phi / 1000000;
                }
            }
            double sum1 = 0, sum2 = 0, sum3 = 0, sum4 = 0, sum5 = 0;
            for (int i = 0; i < Count; i++)
            {
                sum1 += H[i] / y[i] / y[i];
                sum2 += H[i] * H[i] / y[i] / y[i];
                sum3 += 1.0 / y[i] / y[i];
                sum4 += H[i] / y[i];
                sum5 += 1 / y[i];
            }
            double DELTA = sum1 * sum1 - sum2 * sum3;
            double a0 = (sum4 * sum1 - sum2 * sum5) / DELTA;
            double a1 = (sum1 * sum5 - sum4 * sum3) / DELTA;
            double[] delta_y = new double[Count];
            for (int i = 0; i < Count; i++)
            {
                delta_y[i] = 100 * (y[i] - (a0 + a1 * H[i])) / (a0 + a1 * H[i]);
            }
            double sum = 0;
            for (int i = 0; i < Count; i++)
            {
                sum += Math.Abs(delta_y[i]);
            }
            CCDParamTestResult.LE = sum / Count;


            chart3.Series[0].Points.Clear();
            chart3.Series[1].Points.Clear();
            chart3.ChartAreas[0].AxisY.Title = "y";
            chart3.ChartAreas[0].AxisX.Title = "曝光量";
            for (int i = 0; i < Collect_DarkI_miu.Count; i++)
            {
                chart3.Series[0].Points.AddXY(H[i], y[i]);
                chart3.Series[1].Points.AddXY(H[i], H[i] * a1 + a0);
            }
            chart3.SaveImage(SystemParam.TempPicPath + "LE.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
        }
    }
}
