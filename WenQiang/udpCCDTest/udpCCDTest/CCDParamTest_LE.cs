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
        bool CCDParamTest_LE()
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
            int Count;
            int Start;

            double[] y;
            double[] H;
            if (ExType == 1)//固定曝光时间，改变光源照度
            {
                
                Start = (int)(Collect_Step_miu1.Count * 5 / 100.0);
                if (Start == 0)
                {
                    Start++;
                }
                Count = Collect_Step_miu1.Count;
                for(int i=Start;i< Collect_Step_miu1.Count;i++)
                {
                    if (Collect_Step_miu1[i] >= ccdParamTestResult.miu_sat)
                    {
                        Count = i;
                        break;
                    }
                }
                Count= (int)((Count-Start)* 0.90);
                y = new double[Count];
                H = new double[Count];
                for (int i = 0; i < Count; i++)
                {
                    y[i] = Collect_Step_miu1[Start + i] - Collect_Step_miu1[0];
                    H[i] = 1000.0 * Collect_Step_E1[Start + i] * ccdParamTestResult.NTmin1 / 20.0 / 1000000.0;
                }
            }
            else// if (ExType == 2)//固定光源照度，改变曝光时间
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
                Count = (int)(nsat * (95 - 5) / 100.0);
                Start = (int)(Collect_Step_miu2.Count * 5 / 100.0);
                if (Start == 0)
                {
                    Start++;
                }
                y = new double[Count];
                H = new double[Count];
                for (int i = 0; i < Count; i++)
                {
                    y[i] = Collect_Step_miu2[Start + i] - Collect_Step_miu_dark2[i];
                    H[i] = 1000 * Collect_Step_E2[Start + i] * (SystemParam.NTmin2 + i * ccdParamTestResult.NTexp2) / 20 / 1000000;
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
            double a1 =(sum1 * sum5 - sum4 * sum3)/DELTA;
            double a0 =(sum4 * sum1-sum2 * sum5)/DELTA;
            double[] delta_y = new double[Count];
            for (int i = 0; i < Count; i++)
            {
                delta_y[i] = 100.0 * (y[i] - (a0 + a1 * H[i])) / (a0 + a1 * H[i]);
            }
            double sum = 0;
            for (int i = 0; i < Count; i++)
            {
                sum += Math.Abs(delta_y[i]);
            }
             ccdParamTestResult.LE = sum / Count;


            chart3.Series[0].Points.Clear();
            chart3.Series[1].Points.Clear();
            chart3.ChartAreas[0].AxisY.Title = "y";
            chart3.ChartAreas[0].AxisX.Title = "曝光量";
            for (int i = 0; i < Count; i++)
            {
                chart3.Series[0].Points.AddXY(H[i], y[i]);
                chart3.Series[1].Points.AddXY(H[i], H[i] * a1 + a0);
            }
            chart3.SaveImage(SystemParam.TempPicPath + "LE.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            return true;
        }
    }
}
