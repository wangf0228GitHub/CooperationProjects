using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WFNetLib;
using WFNetLib.Forms;

namespace udpCCDTest
{
    public partial class FormMain : Form
    {
        WaitingProc exposureWaitingProc;
        uint NT;
        double Imin, Imax;//照度范围
        bool ExposureTest()
        {
            UIHide();
            exposureChart.ChartAreas[0].AxisX.Title = "照度";
            exposureChart.Visible = true;
            exposureChart.Dock = DockStyle.Fill;
            exposureListView.Visible = true;
            exposureListView.Dock = DockStyle.Fill;
            NT = SystemParam.NTmin2;
            exposureListView.Items[1].SubItems[1].Text=(NT.ToString());            

            textBox1.AppendText("开始明场曝光测试\r\n");
            
            exposureWaitingProc = new WaitingProc();
            exposureWaitingProc.MaxProgress = 10;
            WaitingProcFunc wpf = new WaitingProcFunc(WaitingExposureTest);
            if (!exposureWaitingProc.Execute(wpf, "曝光步距测试中", WaitingType.With_ConfirmCancel, "是否取消？"))
            {
                textBox1.AppendText("用户终止自动测试\r\n");
                return false;
            }
            return true;
        }
        void WaitingExposureTest(object LockWatingThread)
        {
            Imin = 0;
            Imax = 100;
            int picIndex = 1;
            while (true)
            {                
                double[] miu = new double[10];
                double[] delta = new double[10];
                double[] miuCC = new double[10];
                exposureWaitingProc.SetProcessBar(0);
                WFGlobal.WaitMS(1);
                this.Invoke((EventHandler)(delegate
                {
                    WFGlobal.WaitMS(1);
                    exposureChart.Series["miu"].Points.Clear();
                    exposureChart.Series["delta"].Points.Clear();
                    double gap = (Imax - Imin) / 9;
                    for (int step = 0; step < 10; step++)
                    {
                        double oe_per = Imin+gap * step;
                        tcpCCS.LightSet(SystemParam.lambda_Oe, tcpCCS.Per2LX(oe_per/100));
                        exposureListView.Items[2].SubItems[1].Text = (Imin.ToString("F2") + "-" + Imax.ToString("F2"));
                        exposureListView.Items[3].SubItems[1].Text = oe_per.ToString("F2");
                        if (!UDPProc.CollectImage(this, NT, 2))
                        {
                            exposureWaitingProc.ExitWatting();
                            return;
                        }
//                         UDPProc.ccdImageList[0].save(SystemParam.TempPicPath + "E" + step.ToString() + "_0.bin");
//                         UDPProc.ccdImageList[1].save(SystemParam.TempPicPath + "E" + step.ToString() + "_1.bin");
                        ccdImage.Calc_miu_delta(UDPProc.ccdImageList[0], UDPProc.ccdImageList[1], out miu[step], out delta[step], out miuCC[step]);
                        exposureChart.Series["miu"].Points.AddXY(oe_per, miu[step]);
                        exposureChart.Series["delta"].Points.AddXY(oe_per, delta[step]);
                        if (exposureWaitingProc.HasBeenCancelled())
                        {
                            return;
                        }
                        exposureWaitingProc.SetProcessBarPerformStep();
                        WFGlobal.WaitMS(1);
                    }
                    exposureChart.SaveImage(SystemParam.TempPicPath + "exposure"+picIndex.ToString()+".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                    picIndex++;
                }));
                if (exposureWaitingProc.HasBeenCancelled())
                {
                    return;
                }                
                // 方差全部相等（且接近0），则在之前照度第一个区间分10个照度等级
                double close0 = 5;
                if (miu.Max()> (SystemParam.GetADMax() / 2) && ((delta[0]<close0 && delta[1]<close0) || (delta[0]>delta[1])))
                {
//                    Imax =Imin+ (Imax - Imin) / 9;
//                     this.Invoke((EventHandler)(delegate
//                     {
//                         textBox1.AppendText("方差全部接近0，在之前照度第一个区间分成10个亮度等级，重新测试\r\n");
//                     }));
                    this.Invoke((EventHandler)(delegate
                    {
                        textBox1.AppendText("单调减，测试完成，找到曝光饱和点\r\n");
                    }));
                    double[] delta_delta = new double[9];
                    for (int i = 0; i < 9; i++)
                    {
                        delta_delta[i] = Math.Abs(delta[i + 1] - delta[i]);
                    }
                    int index;
                    for (index = 0; index < 9; index++)
                    {
                        if (delta_delta[index] < 2)
                            break;
                    }
                    double Isat = Imin + index * (Imax - Imin) / 9;
                    SystemParam.OeStep = tcpCCS.Per2LX(Isat / 100 / (SystemParam.n));// - 5));
                    iniFileOP.Write("Light Param", "OeStep", SystemParam.OeStep.ToString());
                    ccdParamTestResult.OeStep = SystemParam.OeStep;
                    
                    SystemParam.OeLight = tcpCCS.Per2LX(Isat / 100 / 10);
                    iniFileOP.Write("Light Param", "OeLight", SystemParam.OeLight.ToString());
                    ccdParamTestResult.OeLight = SystemParam.OeLight;

                    SystemParam.Osat = tcpCCS.Per2LX(Isat / 100);
                    //iniFileOP.Write("CCD Param", "Osat", SystemParam.Osat.ToString(), strCCDINIPath);
                    ccdParamTestResult.Osat = SystemParam.Osat;

                    SystemParam.miu_sat = miu[index];
                    //iniFileOP.Write("CCD Param", "miu_sat", SystemParam.miu_sat.ToString(), strCCDINIPath);
                    ccdParamTestResult.miu_sat = SystemParam.miu_sat;

                    SystemParam.NTexp2 = (uint)(SystemParam.Np*(10*NT-SystemParam.NTmin2)/ SystemParam.n);
                    iniFileOP.Write("Collect Param", "NTexp2", SystemParam.NTexp2.ToString());
                    ccdParamTestResult.NTexp2= SystemParam.NTexp2;

                    SystemParam.NTmin1 = NT;
                    iniFileOP.Write("Collect Param", "NTmin1", SystemParam.NTmin1.ToString());
                    ccdParamTestResult.NTmin1 = SystemParam.NTmin1;

                    this.Invoke((EventHandler)(delegate
                    {
                        exposureListView.Items[4].SubItems[1].Text = ccdParamTestResult.Osat.ToString("F6")+"("+tcpCCS.LX2Per(ccdParamTestResult.Osat).ToString("F6") + ")";
                        exposureListView.Items[5].SubItems[1].Text = ccdParamTestResult.miu_sat.ToString("F2");
                        exposureListView.Items[6].SubItems[1].Text = ccdParamTestResult.NTexp2.ToString() + "(" + SystemParam.GetTime(ccdParamTestResult.NTexp2).ToString("F3") + "ms)"; ;
                        exposureListView.Items[7].SubItems[1].Text = ccdParamTestResult.OeLight.ToString("F6") + "(" + tcpCCS.LX2Per(ccdParamTestResult.OeLight).ToString("F6") + ")";
                        exposureListView.Items[8].SubItems[1].Text = ccdParamTestResult.OeStep.ToString("F6") + "(" + tcpCCS.LX2Per(ccdParamTestResult.OeStep).ToString("F6") + ")";
                        exposureListView.Items[9].SubItems[1].Text = ccdParamTestResult.NTmin1.ToString() + "(" + SystemParam.GetTime(ccdParamTestResult.NTmin1).ToString("F3") + "ms)";
                    }));
                    //MessageBox.Show("曝光步距测试完毕");
                    return;
                }
                else if(miu.Max() < (SystemParam.GetADMax() / 2))
                {
                    NT = (uint)(NT * SystemParam.GetADMax() / miu.Max() * 1.1);
                    this.Invoke((EventHandler)(delegate
                    {
                        textBox1.AppendText("单调增，按比例倍增最小曝光时间，重新测试\r\n");
                    }));
                }
                //如果方差单调升则修改NT=10*NT_min
                else if (delta[0]<delta[1])//单调增
                {                    
                    bool bMonotonicity = true;
                    for (int i = 0; i < 9; i++)
                    {
                        if (delta[i + 1]<delta[i] )//增减转折
                        {
                            if(((i + 2) >= 10) ||(Math.Abs(delta[i + 2] - delta[i + 1]) < 2))
                            {
                                double gap = (Imax - Imin) / 9;
                                Imax = Imin + gap * (i + 1);
//                                 if (Imax > 1)
//                                     Imax = 1;
                                Imin = Imin + gap * (i);
                                bMonotonicity = false;
                                this.Invoke((EventHandler)(delegate
                                {
                                    textBox1.AppendText("有增有减，在转折取分成10个亮度等级，重新测试\r\n");
                                }));
                                break;
                            }
                            
                        }
                    }
                    if (bMonotonicity)
                    {
                        NT = (uint)(NT * SystemParam.GetADMax()/miu.Max()*1.0);
                        this.Invoke((EventHandler)(delegate
                        {
                            textBox1.AppendText("单调增，按比例倍增最小曝光时间，重新测试\r\n");
                        }));
                        //                         while(true)
                        //                         {
                        //                             string strNTmin = InputBox.ShowInputBox("请重新设定最小曝光周期数", SystemParam.NTmin.ToString());
                        //                             if (!int.TryParse(strNTmin, out SystemParam.NTmin))
                        //                             {
                        //                                 MessageBox.Show("所设定的最小曝光周期数有误！！！");
                        //                             }
                        //                             break;
                        //                         }
                    }                  
                }               
            }
        }
    }
}
