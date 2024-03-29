﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WFNetLib;

namespace udpCCDTest
{
    public partial class FormMain : Form
    {
        //         List<List<double>> Collect_etaCurve_miu;
        //         List<double> Collect_etaCurve_delta;
        //         List<double> Collect_etaCurve_E;//照度
        // 
        //         List<List<double>> Collect_etaCurve_miu_dark;
        //         List<double> Collect_etaCurve_delta_dark;
        bool CCDParamTest_Collect_etaCurve()
        {
            UIHide();
            List<int> lambdaList = new List<int>(tcpCCS.lambdaList);
            lambdaList.Sort();
            CCDParamTestResult.etaCurve = new double[lambdaList.Count, 2];
            for (int i = 0; i < lambdaList.Count; i++)
            {
                SystemParam.lambda_Oe = lambdaList[i];
                if (!ExposureTest())
                    return false;
                if (!CCDParamTest_Collect_Step())
                    return false;
                CCDParamTest_Calc_K();
                CCDParamTest_Calc_eta();
                CCDParamTestResult.etaCurve[i, 0] = SystemParam.lambda_Oe;
                CCDParamTestResult.etaCurve[i, 1] = CCDParamTestResult.eta;
            }
            return true;
        }
        /* 
                     ParamTestChart1.Visible = true;
                     ParamTestChart1.Dock = DockStyle.Fill;

                     Collect_etaCurve_miu = new List<List<double>>();
                     Collect_etaCurve_delta = new List<double>();
                     Collect_etaCurve_E = new List<double>();
                     Collect_etaCurve_miu_dark = new List<List<double>>();
                     Collect_etaCurve_delta_dark = new List<double>();
                     ParamTestWaitingProc = new WaitingProc();
                     string str = "";
                     WaitingProcFunc wpf = null;
                     if (ExType == 1)//固定曝光时间，改变光源照度
                     {
                         textBox1.AppendText("开始固定曝光时间，按逐步改变光源照度方式采集图像\r\n");
                         str = "固定曝光时间，按逐步改变光源照度方式采集图像中";
                         ParamTestWaitingProc.MaxProgress = (int)(SystemParam.Osat / SystemParam.OeStep);
                         wpf = new WaitingProcFunc(WaitingCollect_etaCurve_1);
                     }
                     else if (ExType == 2)//固定光源照度，改变曝光时间
                     {
                         textBox1.AppendText("开始固定光源照度，按逐步改变曝光时间方式采集图像\r\n");
                         str = "固定光源照度，按逐步改变曝光时间方式采集图像中";
                         ParamTestWaitingProc.MaxProgress = (int)(2 * SystemParam.Osat / SystemParam.OeStep);
                         wpf = new WaitingProcFunc(WaitingCollect_etaCurve_2);
                     }
                     if (!ParamTestWaitingProc.Execute(wpf, str, WaitingType.With_ConfirmCancel, "是否取消？"))
                     {
                         textBox1.AppendText("用户终止自动测试\r\n");
                         return;
                     }
                }

                void WaitingCollect_etaCurve_1(object LockWatingThread)
                {
                    double Oe = 0;
                    double miu=0;
                    double miuCC;
                    double delta;
                    //double E;
                    for(double lamba=SystemParam.L_lambda;lamba<SystemParam.H_lambda;lamba+=SystemParam.delta_lambda)
                    {
                        List<double> eta_miu = new List<double>();
                        Collect_etaCurve_delta = new List<double>();
                        this.Invoke((EventHandler)(delegate
                        {
                            WFGlobal.WaitMS(1);
                            ParamTestChart1.Series["miu"].Points.Clear();
                            ParamTestChart1.Series["delta"].Points.Clear();
                            ParamTestChart1.Series["E"].Points.Clear();
                        }));
                        while (true)
                        {
                            this.Invoke((EventHandler)(delegate
                            {
                                tcpCCS.LightSet((int)lamba, Oe);
                                //                     exposureListView.Items[2].SubItems[1].Text = (Imin.ToString("F2") + "-" + Imax.ToString("F2"));
                                //                     exposureListView.Items[3].SubItems[1].Text = oe_per.ToString("F2");
                                if (!UDPProc.CollectImage(this, SystemParam.NTmin1, 2))
                                {
                                    ParamTestWaitingProc.ExitWatting();
                                    return;
                                }
                                UDPProc.ccdImageList[0].save(SystemParam.TempPicPath + "E" + Oe.ToString("") + "_0.bin");
                                UDPProc.ccdImageList[1].save(SystemParam.TempPicPath + "E" + Oe.ToString("") + "_1.bin");
                                ccdImage.Calc_miu_delta(UDPProc.ccdImageList[0], UDPProc.ccdImageList[1], out miu, out delta, out miuCC);
                                ParamTestChart1.Series["miu"].Points.AddXY(Oe, miu);
                                ParamTestChart1.Series["delta"].Points.AddXY(Oe, delta);
                                DeviceState ds = UDPProc.UDPCommand_04();
                                if (ds == null)
                                {
                                    textBox1.AppendText("照度采集失败，测试终止\r\n");
                                    ParamTestWaitingProc.ExitWatting();
                                    return;
                                }
                                ParamTestChart1.Series["E"].Points.AddXY(Oe, ds.Illuminance);
                                eta_miu.Add(miu);
                                Collect_etaCurve_delta.Add(delta);
                                Collect_etaCurve_E.Add(ds.Illuminance);                        
                                Oe += SystemParam.OeStep;
                                ParamTestWaitingProc.SetProcessBarPerformStep();
                            }));
                            if (miu >= SystemParam.miu_sat)//均值达到本文第二章（曝光步距）所确定的饱和均值
                            {
                                double max = Collect_Step_delta.Max();
                                if (Collect_Step_delta.Last() < max * 0.5)//方差由最大峰值迅速下降（超过50%）
                                {
                                    Collect_etaCurve_miu.Add(eta_miu);
                                    break;
                                }
                            }                    
                            if (exposureWaitingProc.HasBeenCancelled())
                            {
                                return;
                            }
                        }
                    }

                }
                void WaitingCollect_etaCurve_2(object LockWatingThread)
                {
                    double miu = 0;
                    double miuCC;
                    double delta;
                    uint Tex;
                    int stepCount = 0;
                    for (double lamba = SystemParam.L_lambda; lamba < SystemParam.H_lambda; lamba += SystemParam.delta_lambda)
                    {
                        stepCount = 0;
                        List<double> eta_miu = new List<double>();
                        Collect_etaCurve_delta = new List<double>();
                        this.Invoke((EventHandler)(delegate
                        {
                            WFGlobal.WaitMS(1);
                            ParamTestChart2.Series["miu"].Points.Clear();
                            ParamTestChart2.Series["delta"].Points.Clear();
                            ParamTestChart2.Series["miu_dark"].Points.Clear();
                            ParamTestChart2.Series["delta_dark"].Points.Clear();
                            tcpCCS.LightSet((int)lamba, SystemParam.OeLight);
                        }));
                        Tex = SystemParam.NTmin2;
                        //明场
                        while (true)
                        {
                            this.Invoke((EventHandler)(delegate
                            {
                                //                     exposureListView.Items[2].SubItems[1].Text = (Imin.ToString("F2") + "-" + Imax.ToString("F2"));
                                //                     exposureListView.Items[3].SubItems[1].Text = oe_per.ToString("F2");
                                if (!UDPProc.CollectImage(this, Tex, 2))
                                {
                                    ParamTestWaitingProc.ExitWatting();
                                    return;
                                }
                                UDPProc.ccdImageList[0].save(SystemParam.TempPicPath + "ET" + Tex.ToString("") + "_0.bin");
                                UDPProc.ccdImageList[1].save(SystemParam.TempPicPath + "ET" + Tex.ToString("") + "_1.bin");
                                ccdImage.Calc_miu_delta(UDPProc.ccdImageList[0], UDPProc.ccdImageList[1], out miu, out delta, out miuCC);
                                ParamTestChart2.Series["miu"].Points.AddXY(Tex, miu);
                                ParamTestChart2.Series["delta"].Points.AddXY(Tex, delta);
                                DeviceState ds = UDPProc.UDPCommand_04();
                                if (ds == null)
                                {
                                    textBox1.AppendText("照度采集失败，测试终止\r\n");
                                    ParamTestWaitingProc.ExitWatting();
                                    return;
                                }
                                eta_miu.Add(miu);
                                Collect_etaCurve_delta.Add(delta);
                                Collect_etaCurve_E.Add(ds.Illuminance);
                                Tex += SystemParam.NTexp2;
                                stepCount++;
                                ParamTestWaitingProc.SetProcessBarPerformStep();
                            }));
                            if (miu >= SystemParam.miu_sat)//均值达到本文第二章（曝光步距）所确定的饱和均值
                            {
                                double max = Collect_etaCurve_delta.Max();
                                if (Collect_etaCurve_delta.Last() < max * 0.5)//方差由最大峰值迅速下降（超过50%）
                                {
                                    Collect_etaCurve_miu.Add(eta_miu);
                                    break;
                                }
                            }
                            if (exposureWaitingProc.HasBeenCancelled())
                            {
                                return;
                            }                    
                        }
                        this.Invoke((EventHandler)(delegate
                        {
                            WFGlobal.WaitMS(1);
                            tcpCCS.LightSet(SystemParam.lambda_Oe, 0.0);
                        }));
                        Tex = SystemParam.NTmin2;
                        List<double> eta_miu_dark = new List<double>();
                        for (int i = 0; i < stepCount; i++)
                        {
                            this.Invoke((EventHandler)(delegate
                            {
                                //                     exposureListView.Items[2].SubItems[1].Text = (Imin.ToString("F2") + "-" + Imax.ToString("F2"));
                                //                     exposureListView.Items[3].SubItems[1].Text = oe_per.ToString("F2");
                                if (!UDPProc.CollectImage(this, Tex, 2))
                                {
                                    ParamTestWaitingProc.ExitWatting();
                                    return;
                                }
                                UDPProc.ccdImageList[0].save(SystemParam.TempPicPath + "EDT" + Tex.ToString("") + "_0.bin");
                                UDPProc.ccdImageList[1].save(SystemParam.TempPicPath + "EDT" + Tex.ToString("") + "_1.bin");
                                ccdImage.Calc_miu_delta(UDPProc.ccdImageList[0], UDPProc.ccdImageList[1], out miu, out delta, out miuCC);
                                ParamTestChart2.Series["miu_dark"].Points.AddXY(Tex, miu);
                                ParamTestChart2.Series["delta_dark"].Points.AddXY(Tex, delta);
                                eta_miu_dark.Add(miu);
                                Collect_etaCurve_delta_dark.Add(delta);
                            }));                    
                            if (exposureWaitingProc.HasBeenCancelled())
                            {
                                return;
                            }
                            Tex += SystemParam.NTexp2;
                            ParamTestWaitingProc.SetProcessBarPerformStep();
                        }
                        Collect_etaCurve_miu_dark.Add(eta_miu_dark);
                    }

                }
*/
    }
}
