using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WFNetLib;
using WFNetLib.Algorithm;
using WFNetLib.Forms;

namespace udpCCDTest
{
    public partial class FormMain : Form
    {
        WaitingProc ParamTestWaitingProc;
        string strCCDINIPath;
        int ExType;//曝光方式
        bool[] ParamTestList;
        int TestType;//测试内容：参数测试 or 三温测试
        void CCDParamTest()
        {
            textBox1.AppendText("-------------------------------------------------\r\n");            
            FormParamTestChoice f = new FormParamTestChoice();
            if (f.ShowDialog()!=DialogResult.OK)
                return;
            if (f.rbFixTime.Checked)
                ExType = 1;
            else if (f.rbFixOe.Checked)
                ExType = 2;
            else
                ExType = 2;
            
            if (f.rbParam.Checked)
            {
                TestType = 0;//参数测试
                ParamTestList = new bool[f.cbParam.Items.Count];
                for(int i=0;i<f.cbParam.Items.Count;i++)
                {
                    ParamTestList[i] = f.cbParam.GetItemChecked(i);
                }
            }
            else
            {
                TestType = 1;//三温测试
                ParamTestList = new bool[f.cb3T.Items.Count];
                for (int i = 0; i < f.cb3T.Items.Count; i++)
                {
                    ParamTestList[i] = f.cb3T.GetItemChecked(i);
                }
            }
            CCDParamTestResult.Reset();
            UIHide();
            CCDParamTestListView.Visible = true;
            for (int i = 12; i > 1; i--)
            {
                if (!ParamTestList[i])
                    CCDParamTestListView.Items[5 + i].SubItems[1].Text = "不进行测试";
            }
            return;
            if (TestType == 0)//参数测试
            {
                if (ParamTestList[0])//曝光测试
                    ExposureTest();
                UIHide();
                CCDParamTestListView.Visible = true;
                for (int i = 12; i > 1; i--)
                {
                    if (!ParamTestList[i])
                        CCDParamTestListView.Items[5 + i].SubItems[1].Text = "不进行测试";
                }

                if (ExType == 1)
                {
                    ParamTestChart1.Visible = true;
                    CCDParamTestListView.Items[1].SubItems[1].Text = "固定曝光时间";
                }
                else
                {
                    ParamTestChart2.Visible = true;
                    CCDParamTestListView.Items[1].SubItems[1].Text = "固定光源照度";
                }
                CCDParamTestListView.Items[2].SubItems[1].Text = SystemParam.miu_sat.ToString();
                CCDParamTestListView.Items[3].SubItems[1].Text = SystemParam.Osat.ToString();
                CCDParamTestListView.Items[4].SubItems[1].Text = SystemParam.n.ToString();
                CCDParamTestListView.Items[5].SubItems[1].Text = SystemParam.L.ToString();
                //转换增益，量子效率，信噪比曲线，线性误差
                if (ParamTestList[1] || ParamTestList[2] || ParamTestList[4] || ParamTestList[8])
                {
                    CCDParamTest_Collect_Step();
                    if (ParamTestList[1])//转换增益
                    {
                        CCDParamTest_Calc_K();
                        CCDParamTestListView.Items[5 + 1].SubItems[1].Text = CCDParamTestResult.K.ToString();
                    }
                    if (ParamTestList[2])//量子效率
                    {
                        CCDParamTest_Calc_eta();
                        CCDParamTestListView.Items[5 + 2].SubItems[1].Text = CCDParamTestResult.eta.ToString();
                    }
                    if (ParamTestList[4])//信噪比
                    {
                        CCDParamTest_Calc_SNR();
                        CCDParamTestListView.Items[5 + 4].SubItems[1].Text = CCDParamTestResult.miu_p_min.ToString();
                    }
                    if (ParamTestList[8])//线性误差
                    {
                        CCDParamTest_Calc_LE();
                        CCDParamTestListView.Items[5 + 8].SubItems[1].Text = CCDParamTestResult.LE.ToString();
                    }
                }
                //量子效率曲线
                if (ParamTestList[3])
                {
                    CCDParamTest_Collect_etaCurve();
                    CCDParamTest_Calc_etaCurve();
                    CCDParamTestListView.Items[5 + 3].SubItems[1].Text = "测试完成";//CCDParamTestResult.etaCurve.ToString();
                }
                //动态范围、满阱容量、FPN
                if (ParamTestList[5] || ParamTestList[6] || ParamTestList[11])
                {
                    CCDParamTest_Collect_MinMax();
                    if (ParamTestList[5])//动态范围
                    {
                        CCDParamTest_Calc_DR();
                        CCDParamTestListView.Items[5 + 5].SubItems[1].Text = CCDParamTestResult.DR.ToString();
                    }
                    if (ParamTestList[6] || ParamTestList[11])//满阱容量或FPN
                    {
                        CCDParamTest_Calc_FWC();
                        CCDParamTestListView.Items[5 + 6].SubItems[1].Text = CCDParamTestResult.FWC.ToString();
                    }
                }
                //暗电流
                if (ParamTestList[7])
                {
                    CCDParamTest_Collect_DarkI();
                    CCDParamTest_Calc_DarkI();
                    CCDParamTestListView.Items[5 + 7].SubItems[1].Text = CCDParamTestResult.miu_I_miu.ToString() + "; " + CCDParamTestResult.miu_I_delta.ToString();
                }
                //DSNU,PRNU,读出噪声
                if (ParamTestList[9] || ParamTestList[10] || ParamTestList[11] || ParamTestList[12])
                {
                    CCDParamTest_Collect_L(false);
                    if (ParamTestList[6])
                        CCDParamTest_Collect_L(true);

                    CCDParamTest_Calc_L_bLight = false;
                    ParamTestWaitingProc = new WaitingProc();
                    WaitingProcFunc wpf = null;
                    ParamTestWaitingProc.MaxProgress = SystemParam.L;
                    wpf = new WaitingProcFunc(CCDParamTest_Calc_L);
                    if (!ParamTestWaitingProc.Execute(wpf, "处理空域测试结果", WaitingType.With_ConfirmCancel, "是否取消？"))
                    {
                        textBox1.AppendText("用户终止自动测试\r\n");
                        return;
                    }
                    if (ParamTestList[9])//DSNU
                    {
                        CCDParamTestResult.DSNU = Math.Sqrt(CCDParamTestResult.L_S_y_dark) / CCDParamTestResult.K;
                        CCDParamTestListView.Items[5 + 9].SubItems[1].Text = CCDParamTestResult.DSNU.ToString();
                    }
                    if (ParamTestList[11])//FPN
                    {
                        CCDParamTestResult.DSNU = Math.Sqrt(CCDParamTestResult.L_S_y_dark) / CCDParamTestResult.K;
                        CCDParamTestResult.FPN = CCDParamTestResult.DSNU / CCDParamTestResult.FWC;
                        CCDParamTestListView.Items[5 + 11].SubItems[1].Text = CCDParamTestResult.FPN.ToString();
                    }
                    if (ParamTestList[12])//读出噪声
                    {
                        CCDParamTestResult.delta_raed = Math.Sqrt(CCDParamTestResult.delta_mid) / CCDParamTestResult.K;
                        CCDParamTestListView.Items[5 + 12].SubItems[1].Text = CCDParamTestResult.delta_raed.ToString();
                    }
                    if (ParamTestList[10])//PRNU
                    {
                        CCDParamTest_Calc_L_bLight = true;
                        ParamTestWaitingProc = new WaitingProc();
                        wpf = null;
                        ParamTestWaitingProc.MaxProgress = SystemParam.L;
                        wpf = new WaitingProcFunc(CCDParamTest_Calc_L);
                        if (!ParamTestWaitingProc.Execute(wpf, "处理空域测试结果", WaitingType.With_ConfirmCancel, "是否取消？"))
                        {
                            textBox1.AppendText("用户终止自动测试\r\n");
                            return;
                        }
                        CCDParamTestResult.PRNU = Math.Sqrt(CCDParamTestResult.L_S_y - CCDParamTestResult.L_S_y_dark) / (CCDParamTestResult.L_miu_y - CCDParamTestResult.L_miu_y_dark);
                        CCDParamTestListView.Items[5 + 10].SubItems[1].Text = CCDParamTestResult.PRNU.ToString();
                    }
                }
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
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
                    textBox1.AppendText("自动测试结束,测试报告保存位置为:\r\n");
                    textBox1.AppendText(saveFileDialog1.FileName);
                }
                else
                {
                    textBox1.AppendText("自动测试结束,用户未保存报告\r\n");
                }                
                MessageBox.Show("CMOS测试完成");
            }
        }
        void CCDParamTest_Calc_K()
        {
            int K_Count = Collect_Step_miu.Count*(int)((SystemParam.L_TOP-SystemParam.L_BTM)/100.0);
            int K_Start = Collect_Step_miu.Count* (int)(SystemParam.L_BTM/100.0);                      
            if (ExType == 1)//固定曝光时间，改变光源照度
            {
                double[] K_delta = new double[K_Count];
                double[] K_miu = new double[K_Count];
                for (int i = 0; i < K_Count; i++)
                {
                    K_miu[i] = Collect_Step_miu[K_Start + i];
                    K_delta[i] = Collect_Step_delta[K_Start + i];
                }
                double[] fitret = FittingMultiLine.MultiLine(K_miu, K_delta, K_Count, 1);
                CCDParamTestResult.K = fitret[1];
            }
            else if (ExType == 2)//固定光源照度，改变曝光时间
            {
                double[] K_delta = new double[K_Count];
                double[] K_miu = new double[K_Count];
                double[] NTexp = new double[K_Count];
                for (int i = 0; i < K_Count; i++)
                {
                    K_miu[i] = Collect_Step_miu[K_Start + i];
                    K_delta[i] = Collect_Step_delta[K_Start + i];
                    NTexp[i] = SystemParam.NTmin + SystemParam.NTexp * i;
                }
                double[] fitret_miu = FittingMultiLine.MultiLine(K_miu, NTexp, K_Count, 1);
                double[] fitret_delta = FittingMultiLine.MultiLine(K_delta, NTexp, K_Count, 1);
                for (int i = 0; i < K_Count; i++)
                {
                    K_miu[i] = Collect_Step_miu_dark[K_Start + i];
                    K_delta[i] = Collect_Step_delta_dark[K_Start + i];
                }
                double[] fitret_miu_dark = FittingMultiLine.MultiLine(K_miu, NTexp, K_Count, 1);
                double[] fitret_delta_dark = FittingMultiLine.MultiLine(K_delta, NTexp, K_Count, 1);
                CCDParamTestResult.K = (fitret_delta[1]-fitret_delta_dark[1]) / (fitret_miu[1]-fitret_miu_dark[1]);
            }
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
                    texp = 1000 * SystemParam.Np * SystemParam.NTmin / SystemParam.CCD_phi / 1000000;
                    miu_p[i] = 50.34 * SystemParam.CCD_Sa * SystemParam.CCD_Sb * texp * SystemParam.lambda_Oe * Collect_Step_E[K_Start + i];
                }
                else if (ExType == 2)//固定光源照度，改变曝光时间
                {
                    texp = 1000 * SystemParam.Np * (SystemParam.NTmin+SystemParam.NTexp*i) / SystemParam.CCD_phi / 1000000;
                    miu_p[i] = 50.34 * SystemParam.CCD_Sa * SystemParam.CCD_Sb * texp * SystemParam.lambda_Oe * SystemParam.Oe;
                }
            }            
            double[] fitret = FittingMultiLine.MultiLine(miu_p, miu_y, K_Count, 1);
            CCDParamTestResult.eta = fitret[1] / CCDParamTestResult.K;
        }
        void CCDParamTest_Calc_etaCurve()
        {
            CCDParamTestResult.etaCurve = new double[Collect_etaCurve_miu.Count];
            for (int j=0;j< Collect_etaCurve_miu.Count;j++)
            {
                List<double> miu_y_o = Collect_etaCurve_miu[j];
                int K_Count = Collect_etaCurve_miu.Count * (int)((SystemParam.L_TOP - SystemParam.L_BTM) / 100.0);
                int K_Start = Collect_etaCurve_miu.Count * (int)(SystemParam.L_BTM / 100.0);
                double[] miu_p = new double[K_Count];
                double[] miu_y = new double[K_Count];                
                for (int i = 0; i < K_Count; i++)
                {
                    miu_y[i] = miu_y_o[K_Start + i];                    
                    double texp;
                    if (ExType == 1)//固定曝光时间，改变光源照度
                    {
                        texp = 1000 * SystemParam.Np * SystemParam.NTmin / SystemParam.CCD_phi / 1000000;
                        miu_p[i] = 50.34 * SystemParam.CCD_Sa * SystemParam.CCD_Sb * texp * SystemParam.lambda_Oe * Collect_etaCurve_E[K_Start + i];
                    }
                    else if (ExType == 2)//固定光源照度，改变曝光时间
                    {
                        texp = 1000 * SystemParam.Np * (SystemParam.NTmin + SystemParam.NTexp * i) / SystemParam.CCD_phi / 1000000;
                        miu_p[i] = 50.34 * SystemParam.CCD_Sa * SystemParam.CCD_Sb * texp * SystemParam.lambda_Oe * SystemParam.Oe;
                    }
                }
                double[] fitret = FittingMultiLine.MultiLine(miu_p, miu_y, K_Count, 1);
                CCDParamTestResult.etaCurve[j] = fitret[1] / CCDParamTestResult.K;
            }
        }
        void CCDParamTest_Calc_SNR()
        {            
            if (ExType == 1)//固定曝光时间，改变光源照度
            {
                CCDParamTestResult.miu_p_min = (Math.Sqrt(Collect_Step_delta[0]/CCDParamTestResult.K / CCDParamTestResult.K+0.25)+0.5)/CCDParamTestResult.eta;
                double texp = 1000 * SystemParam.Np * SystemParam.NTmin / SystemParam.CCD_phi / 1000000;
                double miu_p = 50.34 * SystemParam.CCD_Sa * SystemParam.CCD_Sb * texp * SystemParam.lambda_Oe * Collect_Step_E.Last();
                CCDParamTestResult.miu_p_sat = CCDParamTestResult.eta * miu_p / Math.Sqrt(Collect_Step_delta[0] + 1 / 144 / CCDParamTestResult.K / CCDParamTestResult.K + CCDParamTestResult.eta * miu_p);
            }
            else if (ExType == 2)//固定光源照度，改变曝光时间
            {
                CCDParamTestResult.miu_p_min = (Math.Sqrt(Collect_Step_delta_dark[0] / CCDParamTestResult.K / CCDParamTestResult.K + 0.25) + 0.5) / CCDParamTestResult.eta;
                double texp = 1000 * SystemParam.Np * (SystemParam.NTmin+(Collect_Step_delta.Count-1)*SystemParam.NTexp) / SystemParam.CCD_phi / 1000000;
                double miu_p = 50.34 * SystemParam.CCD_Sa * SystemParam.CCD_Sb * texp * SystemParam.lambda_Oe * SystemParam.Oe;
                CCDParamTestResult.miu_p_sat = CCDParamTestResult.eta * miu_p / Math.Sqrt(Collect_Step_delta_dark[0] + 1 / 144 / CCDParamTestResult.K / CCDParamTestResult.K + CCDParamTestResult.eta * miu_p);
            }
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
            CCDParamTestResult.FWC = (miu_sat-miu_dark)/CCDParamTestResult.K;
        }
        void CCDParamTest_Calc_DarkI()
        {
            double[] texp = new double[Collect_DarkI_miu.Count];
            for(int i=0;i< Collect_DarkI_miu.Count;i++)
            {
                double NTexp = SystemParam.NTdark + i * SystemParam.delta_Tdark;
                texp[i] = NTexp / SystemParam.CCD_phi / 1000000;
            }
            double[] fitret = FittingMultiLine.MultiLine(texp, Collect_DarkI_miu.ToArray(), Collect_DarkI_miu.Count, 1);
            CCDParamTestResult.miu_I_miu= fitret[1] / CCDParamTestResult.K;
            fitret = FittingMultiLine.MultiLine(texp, Collect_DarkI_delta.ToArray(), Collect_DarkI_delta.Count, 1);
            CCDParamTestResult.miu_I_delta = fitret[1] / CCDParamTestResult.K / CCDParamTestResult.K;
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
                    H[i] = 1000 * Collect_Step_E[Start + i] * (SystemParam.NTmin+i*SystemParam.NTexp) / SystemParam.CCD_phi / 1000000;
                }
            }
            double sum1=0, sum2=0, sum3=0,sum4=0,sum5=0;
            for (int i = 0; i < Count; i++)
            {
                sum1 += H[i] / y[i] / y[i];
                sum2 += H[i] * H[i] / y[i] / y[i];
                sum3 += 1.0 / y[i] / y[i];
                sum4 += H[i] / y[i];
                sum5 += 1 / y[i];
            }
            double DELTA = sum1*sum1-sum2*sum3;
            double a0 = (sum4 * sum1 - sum2 * sum5) / DELTA;
            double a1 = (sum1 * sum5 - sum4 * sum3) / DELTA;
            double[] delta_y = new double[Count];
            for (int i = 0; i < Count; i++)
            {
                delta_y[i] = 100 * (y[i]-(a0+a1*H[i])) / (a0+a1*H[i]);
            }
            double sum = 0;
            for (int i = 0; i < Count; i++)
            {
                sum += Math.Abs(delta_y[i]);
            }
            CCDParamTestResult.LE = sum / Count;
        }
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
            ParamTestWaitingProc.SetTitle("相同曝光条件下数据处理---计算暗场像素点均值");
            ParamTestWaitingProc.SetProcessBar(0);
            ParamTestWaitingProc.SetProcessBarRange(0, SystemParam.L);
            for (int i = 0; i < SystemParam.L; i++)
            {
                byte[] p = ReadTempFile(2*SystemParam.CCD_M*SystemParam.CCD_N, i, SystemParam.L_DarkTempFilePath);
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
                CCDParamTestResult.L_miu_y = miu_y;
            else
                CCDParamTestResult.L_miu_y_dark = miu_y;
            /************************************************************************/
            /* S_y_measured^2                                                       */
            /************************************************************************/
            double S_y_measured = 0;
            for (int m = 0; m < SystemParam.CCD_M; m++)
            {
                for (int n = 0; n < SystemParam.CCD_N; n++)
                {
                    S_y_measured += Math.Pow((y[m, n]- CCDParamTestResult.L_miu_y_dark),2);
                }
            }
            S_y_measured = S_y_measured / SystemParam.CCD_M / SystemParam.CCD_N;
            /************************************************************************/
            /* delta_s^2                                                            */
            /************************************************************************/
            ParamTestWaitingProc.SetTitle("相同曝光条件下数据处理---计算暗场像素点方差");
            ParamTestWaitingProc.SetProcessBar(0);
            ParamTestWaitingProc.SetProcessBarRange(0, SystemParam.L);
            double[,] delta_s=new double[SystemParam.CCD_M, SystemParam.CCD_N];
            for (int i = 0; i < SystemParam.L; i++)
            {
                byte[] p = ReadTempFile(2 * SystemParam.CCD_M * SystemParam.CCD_N, i, SystemParam.L_DarkTempFilePath);
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
            if(!CCDParamTest_Calc_L_bLight)//暗场，计算方差中值
            {
                for (int m = 0; m < SystemParam.CCD_M; m++)
                {
                    for (int n = 0; n < SystemParam.CCD_N; n++)
                    {
                        if (max < delta_s[m, n])
                            max = delta_s[m, n];
                    }
                }
                CCDParamTestResult.delta_mid = max / 2;
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
                CCDParamTestResult.L_S_y = S_y_measured - delta_y_stack / SystemParam.L;
            else
                CCDParamTestResult.L_S_y_dark = S_y_measured - delta_y_stack / SystemParam.L;
        }
    }
    public class CCDParamTestResult
    {
        public static double K;
        public static double eta;
        public static double[] etaCurve;

        public static double miu_p_min;
        public static double miu_p_sat;

        public static double DR;

        public static double FWC;

        public static double miu_I_delta;
        public static double miu_I_miu;

        public static double LE;
        
        public static double L_miu_y;
        public static double L_S_y;
        public static double L_miu_y_dark;
        public static double L_S_y_dark;

        public static double DSNU;
        public static double PRNU;
        public static double FPN;
        public static double delta_raed;
        public static double delta_mid;

        public static bool miucurve;
        public static bool DarkIcurve;
        public static bool SNRcurve;
        public static void Reset()
        {
            K = double.NaN;
            eta = double.NaN;
            etaCurve = null;

            miu_p_min = double.NaN;
            miu_p_sat = double.NaN;

            DR = double.NaN;

            FWC = double.NaN;

            miu_I_delta = double.NaN;
            miu_I_miu = double.NaN;

            LE = double.NaN;

            L_miu_y = double.NaN;
            L_S_y = double.NaN;
            L_miu_y_dark = double.NaN;
            L_S_y_dark = double.NaN;

            DSNU = double.NaN;
            PRNU = double.NaN;
            FPN = double.NaN;
            delta_raed = double.NaN;
            delta_mid = double.NaN;

            miucurve = false;
            DarkIcurve = false;
            SNRcurve = false;
        }
    }
}
