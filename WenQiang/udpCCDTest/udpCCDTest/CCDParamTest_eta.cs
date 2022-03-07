using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WFNetLib;
using WFNetLib.Algorithm;

namespace udpCCDTest
{
    public partial class FormMain : Form
    {        
        bool CCDParamTest_eta()
        {
            if (!CCDParamTest_Collect_Step())
            {
                MessageBox.Show("曲线采集失败");
                return false;
            }
            if (double.IsNaN(ccdParamTestResult.K))
            {
                if (!CCDParamTest_K())
                    return false;
            }
            int K_Count;
            int K_Start;
            double[] miu_p;
            double[] miu_y;
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
                K_Count = (int)(nsat * (SystemParam.L_TOP - SystemParam.L_BTM) / 100.0);
                K_Start = (int)(nsat * SystemParam.L_BTM / 100.0);
                miu_p = new double[K_Count];
                miu_y = new double[K_Count];
                for (int i = 0; i < K_Count; i++)
                {
                    miu_y[i] = Collect_Step_miu1[K_Start + i];
                    miu_p[i] = 50.34 * SystemParam.CCD_Sa * SystemParam.CCD_Sb * SystemParam.GetTime(ccdParamTestResult.NTmin1) * SystemParam.lambda_Oe / 1000 * Collect_Step_E1[K_Start + i];
                }
            }
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
                K_Count = (int)(nsat * (SystemParam.L_TOP - SystemParam.L_BTM) / 100.0);
                K_Start = (int)(nsat* SystemParam.L_BTM / 100.0);
                miu_p = new double[K_Count];
                miu_y = new double[K_Count];
                double E = 0;
                for (int i = 0; i < K_Count; i++)
                {
                    E+=Collect_Step_E2[K_Start + i];
                }
                E = E / K_Count;
                for (int i = 0; i < K_Count; i++)
                {
                    miu_y[i] = Collect_Step_miu2[K_Start + i];
                    miu_p[i] = 50.34 * SystemParam.CCD_Sa * SystemParam.CCD_Sb * SystemParam.GetTime2(i, ccdParamTestResult.NTexp2) * SystemParam.lambda_Oe / 1000 * E;
                }
            }
            double[] fitret = FittingMultiLine.MultiLine(miu_p, miu_y, K_Count, 1);
            ccdParamTestResult.eta = fitret[1] / ccdParamTestResult.K;
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
            return true;
        }
    }
}
