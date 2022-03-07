using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace udpCCDTest
{
    public partial class FormMain : Form
    {
        bool CCDParamTest_Calc_L_bLight;
        void CCDParamTest_Calc_L(object LockWatingThread)
        {
            //int count = SystemParam.L * 4;
            string file;
            if (CCDParamTest_Calc_L_bLight)
                file = SystemParam.L_LightTempFilePath;
            else
                file = SystemParam.L_DarkTempFilePath;
            double[,] y = new double[SystemParam.CCD_M, SystemParam.CCD_N];
            /************************************************************************/
            /* <y>                                                                  */
            /************************************************************************/
            ParamTestWaitingProc.SetTitle("stack:计算暗场均值");
            ParamTestWaitingProc.SetProcessBar(0);
            ParamTestWaitingProc.SetProcessBarRange(0, SystemParam.L);
            for (int i = 0; i < SystemParam.L; i++)
            {
                byte[] p = ReadTempFile(2 * SystemParam.CCD_M * SystemParam.CCD_N, i, file);
                ushort[,] pic = ccdImage.TransImageDatas(p);
                for (int m = 0; m < SystemParam.CCD_M; m++)
                {
                    for (int n = 0; n < SystemParam.CCD_N; n++)
                    {
                        y[m, n] += pic[m, n];
                    }
                }
                if (ParamTestWaitingProc.HasBeenCancelled())
                {
                    return;
                }
                ParamTestWaitingProc.SetProcessBarPerformStep();
                this.Invoke((EventHandler)(delegate
                {
                    //listView1.Items[14].SubItems[1].Text = (((double)(i + 1)) * 100 / count).ToString("F1") + "%";
                }));
            }
            for (int m = 0; m < SystemParam.CCD_M; m++)
            {
                for (int n = 0; n < SystemParam.CCD_N; n++)
                {
                    y[m, n] = y[m, n] / SystemParam.L;
                }
            }
            /************************************************************************/
            /* miu_y                                                                */
            /************************************************************************/
            double miu_y = 0;
            for (int m = 0; m < SystemParam.CCD_M; m++)
            {
                for (int n = 0; n < SystemParam.CCD_N; n++)
                {
                    miu_y += y[m, n];
                }
            }
            miu_y = miu_y / SystemParam.CCD_M / SystemParam.CCD_N;
            if (CCDParamTest_Calc_L_bLight)
                ccdParamTestResult.L_miu_y = miu_y;
            else
                ccdParamTestResult.L_miu_y_dark = miu_y;
            /************************************************************************/
            /* S_y_measured^2                                                       */
            /************************************************************************/
            double S_y_measured = 0;
            for (int m = 0; m < SystemParam.CCD_M; m++)
            {
                for (int n = 0; n < SystemParam.CCD_N; n++)
                {
                    S_y_measured += Math.Pow((y[m, n] - ccdParamTestResult.L_miu_y_dark), 2);
                }
            }
            S_y_measured = S_y_measured / SystemParam.CCD_M / SystemParam.CCD_N;
            /************************************************************************/
            /* delta_s^2                                                            */
            /************************************************************************/
            ParamTestWaitingProc.SetTitle("stack:计算暗场方差");
            ParamTestWaitingProc.SetProcessBar(0);
            ParamTestWaitingProc.SetProcessBarRange(0, SystemParam.L);
            double[,] delta_s = new double[SystemParam.CCD_M, SystemParam.CCD_N];
            for (int i = 0; i < SystemParam.L; i++)
            {
                byte[] p = ReadTempFile(2 * SystemParam.CCD_M * SystemParam.CCD_N, i, file);
                ushort[,] pic = ccdImage.TransImageDatas(p);
                for (int m = 0; m < SystemParam.CCD_M; m++)
                {
                    for (int n = 0; n < SystemParam.CCD_N; n++)
                    {
                        delta_s[m, n] += Math.Pow(pic[m, n] - y[m, n], 2);
                    }
                }
                if (ParamTestWaitingProc.HasBeenCancelled())
                {
                    return;
                }
                ParamTestWaitingProc.SetProcessBarPerformStep();
                this.Invoke((EventHandler)(delegate
                {
                    //listView1.Items[14].SubItems[1].Text = (((double)(i + 1)) * 100 / count).ToString("F1") + "%";
                }));
            }
            for (int m = 0; m < SystemParam.CCD_M; m++)
            {
                for (int n = 0; n < SystemParam.CCD_N; n++)
                {
                    delta_s[m, n] = delta_s[m, n] / SystemParam.L;
                }
            }
            double max = 0;
            List<double> deltas = new List<double>();
            if (!CCDParamTest_Calc_L_bLight)//暗场，计算方差中值
            {
                for (int m = 0; m < SystemParam.CCD_M; m++)
                {
                    for (int n = 0; n < SystemParam.CCD_N; n++)
                    {
                        deltas.Add(delta_s[m, n]);
//                         if (max < delta_s[m, n])
//                             max = delta_s[m, n];
                    }
                }
                deltas.Sort();
                //deltas.Reverse();
                ccdParamTestResult.delta_mid = deltas[SystemParam.CCD_M* SystemParam.CCD_N/2];//max / 2;
                int s = SystemParam.CCD_M * SystemParam.CCD_N / 2 - SystemParam.L;
                double deltasum = 0;
                for(int i= s;i<s+(2*SystemParam.L+1);i++)
                {
                    deltasum += Math.Sqrt(deltas[i]);
                }
                ccdParamTestResult.delta_mid_avr = deltasum/ (2 * SystemParam.L + 1);
            }

            /************************************************************************/
            /* delta_y_stack^2                                                      */
            /************************************************************************/
            double delta_y_stack = 0;
            for (int m = 0; m < SystemParam.CCD_M; m++)
            {
                for (int n = 0; n < SystemParam.CCD_N; n++)
                {
                    delta_y_stack += Math.Pow(delta_s[m, n], 2);
                }
            }
            delta_y_stack = delta_y_stack / SystemParam.CCD_M / SystemParam.CCD_N;
            /************************************************************************/
            /* delta_s_stack^2                                                      */
            /************************************************************************/
            if (CCDParamTest_Calc_L_bLight)
                ccdParamTestResult.L_S_y = S_y_measured - delta_y_stack / SystemParam.L;
            else
                ccdParamTestResult.L_S_y_dark = S_y_measured - delta_y_stack / SystemParam.L;
        }
    }
}
