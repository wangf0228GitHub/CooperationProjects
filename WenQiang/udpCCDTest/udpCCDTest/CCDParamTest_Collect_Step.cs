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
        List<double> Collect_Step_miu;
        List<double> Collect_Step_delta;
        List<double> Collect_Step_E;//照度

        List<double> Collect_Step_miu_dark;
        List<double> Collect_Step_delta_dark;
        bool CCDParamTest_Collect_Step()
        {
            //UIHide();
            
            Collect_Step_miu = new List<double>();
            Collect_Step_delta = new List<double>();
            Collect_Step_E = new List<double>();
            Collect_Step_miu_dark = new List<double>();
            Collect_Step_delta_dark = new List<double>();
            ParamTestWaitingProc = new WaitingProc();            
            string str ="";
            WaitingProcFunc wpf=null;
            if (ExType == 1)//固定曝光时间，改变光源照度
            {
                textBox1.AppendText("开始固定曝光时间，按逐步改变光源照度方式采集图像\r\n");
                str = "固定曝光时间，按逐步改变光源照度方式采集图像中";
                ParamTestWaitingProc.MaxProgress = SystemParam.n + 1;//(int)(SystemParam.Osat / n);
                wpf = new WaitingProcFunc(WaitingCollect_Step_1);
            }
            else if (ExType == 2)//固定光源照度，改变曝光时间
            {
                textBox1.AppendText("开始固定光源照度，按逐步改变曝光时间方式采集图像\r\n");
                str = "固定光源照度，按逐步改变曝光时间方式采集图像中";
                ParamTestWaitingProc.MaxProgress= 2*(SystemParam.n+1);
                wpf = new WaitingProcFunc(WaitingCollect_Step_2);
            }      
            if (!ParamTestWaitingProc.Execute(wpf, str, WaitingType.With_ConfirmCancel, "是否取消？"))
            {
                textBox1.AppendText("用户终止自动测试\r\n");                
                return false;
            }
            return true;
        }
        
        void WaitingCollect_Step_1(object LockWatingThread)
        {
            double Oe=0;
            double miu;
            double miuCC;
            double delta;
            //double E;
            this.Invoke((EventHandler)(delegate
            {
                WFGlobal.WaitMS(1);
                ParamTestChart1.Visible = true;
                ParamTestChart1.BringToFront();
                ParamTestChart1.Series["miu"].Points.Clear();
                ParamTestChart1.Series["delta"].Points.Clear();
                ParamTestChart1.Series["E"].Points.Clear();
            }));
            for(int i=0;i<(SystemParam.n+1);i++)
            {
                this.Invoke((EventHandler)(delegate
                {
                    tcpCCS.LightSet(SystemParam.lambda_Oe, Oe);

                    if (!UDPProc.CollectImage(this, SystemParam.NTmin1, 2))
                    {
                        ParamTestWaitingProc.ExitWatting();
                        return;
                    }
                    UDPProc.ccdImageList[0].save(SystemParam.TempPicPath + "Step" + i.ToString("") + "_0.bin");
                    UDPProc.ccdImageList[1].save(SystemParam.TempPicPath + "Step" + i.ToString("") + "_1.bin");

                    //读取照片文件
                    //                     UDPProc.ccdImageList = new List<ccdImage>(2);
                    //                     UDPProc.ccdImageList.Add(new ccdImage());
                    //                     UDPProc.ccdImageList.Add(new ccdImage());
                    //                     UDPProc.ccdImageList[0].LoadFile(SystemParam.TempPicPath + "Step" + i.ToString("") + "_0.bin");
                    //                     UDPProc.ccdImageList[1].LoadFile(SystemParam.TempPicPath + "Step" + i.ToString("") + "_1.bin");
                    //                     Collect_Step_E.Add(i*10);

                    ccdImage.Calc_miu_delta(UDPProc.ccdImageList[0], UDPProc.ccdImageList[1], out miu, out delta, out miuCC);
                    ParamTestChart1.Series["miu"].Points.AddXY(Oe, miu);
                    ParamTestChart1.Series["delta"].Points.AddXY(Oe, delta);                    
                    Collect_Step_miu.Add(miu);
                    Collect_Step_delta.Add(delta);

                    DeviceState ds = UDPProc.UDPCommand_04();
                    if (ds == null)
                    {
                        textBox1.AppendText("照度采集失败，测试终止\r\n");
                        ParamTestWaitingProc.ExitWatting();
                        return;
                    }
                    ParamTestChart1.Series["E"].Points.AddXY(Oe, ds.Illuminance);
                    Collect_Step_E.Add(ds.Illuminance);


                    //                     if(miu>=SystemParam.miu_sat)//均值达到本文第二章（曝光步距）所确定的饱和均值
                    //                     {
                    //                         double max = Collect_Step_delta.Max();
                    //                         if(Collect_Step_delta.Last()<max*0.5)//方差由最大峰值迅速下降（超过50%）
                    //                         {
                    //                             return;
                    //                         }
                    //                     }
                    Oe += SystemParam.OeStep;
                    ParamTestWaitingProc.SetProcessBarPerformStep();
                }));
                if (ParamTestWaitingProc.HasBeenCancelled())
                {
                    return;
                }
            }
        }
        void WaitingCollect_Step_2(object LockWatingThread)
        {
            double miu=0;
            double miuCC;
            double delta;
            uint Tex;
            //int stepCount=0;
            //double E;
            this.Invoke((EventHandler)(delegate
            {
                WFGlobal.WaitMS(1);
                ParamTestChart2.Visible = true;
                ParamTestChart2.BringToFront();
                ParamTestChart2.Series["miu"].Points.Clear();
                ParamTestChart2.Series["delta"].Points.Clear();
                ParamTestChart2.Series["miu_dark"].Points.Clear();
                ParamTestChart2.Series["delta_dark"].Points.Clear();
                tcpCCS.LightSet(SystemParam.lambda_Oe, SystemParam.OeLight);
            }));
            Tex = SystemParam.NTmin2;
            //明场
            for (int i = 0; i < (SystemParam.n + 1); i++)//while (true)
            {
                this.Invoke((EventHandler)(delegate
                {
                     //                     exposureListView.Items[2].SubItems[1].Text = (Imin.ToString("F2") + "-" + Imax.ToString("F2"));
                     //                     exposureListView.Items[3].SubItems[1].Text = oe_per.ToString("F2");
                    // if (!UDPProc.CollectImage(this, Tex, 2))
                    //{
                    //    ParamTestWaitingProc.ExitWatting();
                    //    return;
                    //}
                    //UDPProc.ccdImageList[0].save(SystemParam.TempPicPath + "StepL" + i.ToString("") + "_0.bin");
                    //UDPProc.ccdImageList[1].save(SystemParam.TempPicPath + "StepL" + i.ToString("") + "_1.bin");
                    //读取照片文件
                    UDPProc.ccdImageList = new List<ccdImage>(2);
                    UDPProc.ccdImageList.Add(new ccdImage());
                    UDPProc.ccdImageList.Add(new ccdImage());
                    UDPProc.ccdImageList[0].LoadFile(SystemParam.TempPicPath + "StepL" + i.ToString("") + "_0.bin");
                    UDPProc.ccdImageList[1].LoadFile(SystemParam.TempPicPath + "StepL" + i.ToString("") + "_1.bin");
                    Collect_Step_E.Add(i*10);
                    
                    ccdImage.Calc_miu_delta(UDPProc.ccdImageList[0], UDPProc.ccdImageList[1], out miu, out delta, out miuCC);
                    ParamTestChart2.Series["miu"].Points.AddXY(Tex, miu);
                    ParamTestChart2.Series["delta"].Points.AddXY(Tex, delta);
                   
                    Collect_Step_miu.Add(miu);
                    Collect_Step_delta.Add(delta);
//                     DeviceState ds = UDPProc.UDPCommand_04();
//                     if (ds == null)
//                     {
//                         textBox1.AppendText("照度采集失败，测试终止\r\n");
//                         ParamTestWaitingProc.ExitWatting();
//                         return;
//                     }
//                     Collect_Step_E.Add(ds.Illuminance);

                    Tex += SystemParam.NTexp2;
                     //stepCount++;
                     ParamTestWaitingProc.SetProcessBarPerformStep();
                }));
                //                 if (miu >= SystemParam.miu_sat)//均值达到本文第二章（曝光步距）所确定的饱和均值
                //                 {
                //                     double max = Collect_Step_delta.Max();
                //                     if (Collect_Step_delta.Last() < max * 0.5)//方差由最大峰值迅速下降（超过50%）
                //                     {
                //                         break; 
                //                     }
                //                 }
                if (ParamTestWaitingProc.HasBeenCancelled())
                {
                    return;
                }
            }
            this.Invoke((EventHandler)(delegate
            {
                WFGlobal.WaitMS(1);
                tcpCCS.LightSet(SystemParam.lambda_Oe, 0);
            }));
            Tex = SystemParam.NTmin2;
            for (int i = 0; i < (SystemParam.n + 1); i++)
            {
                this.Invoke((EventHandler)(delegate
                {
                    //                     exposureListView.Items[2].SubItems[1].Text = (Imin.ToString("F2") + "-" + Imax.ToString("F2"));
                    //                     exposureListView.Items[3].SubItems[1].Text = oe_per.ToString("F2");
                    //if (!UDPProc.CollectImage(this, Tex, 2))
                    //{
                    //    ParamTestWaitingProc.ExitWatting();
                    //    return;
                    //}
                    //UDPProc.ccdImageList[0].save(SystemParam.TempPicPath + "StepD" + i.ToString("") + "_0.bin");
                    //UDPProc.ccdImageList[1].save(SystemParam.TempPicPath + "StepD" + i.ToString("") + "_1.bin");

                    //读取照片文件
                    UDPProc.ccdImageList = new List<ccdImage>(2);
                    UDPProc.ccdImageList.Add(new ccdImage());
                    UDPProc.ccdImageList.Add(new ccdImage());
                    UDPProc.ccdImageList[0].LoadFile(SystemParam.TempPicPath + "StepD" + i.ToString("") + "_0.bin");
                    UDPProc.ccdImageList[1].LoadFile(SystemParam.TempPicPath + "StepD" + i.ToString("") + "_1.bin");


                    ccdImage.Calc_miu_delta(UDPProc.ccdImageList[0], UDPProc.ccdImageList[1], out miu, out delta, out miuCC);
                    ParamTestChart2.Series["miu_dark"].Points.AddXY(Tex, miu);
                    ParamTestChart2.Series["delta_dark"].Points.AddXY(Tex, delta);
                    
                    Collect_Step_miu_dark.Add(miu);
                    Collect_Step_delta_dark.Add(delta);                    
                }));                
                if (ParamTestWaitingProc.HasBeenCancelled())
                {
                    return;
                }
                Tex += SystemParam.NTexp2;
                ParamTestWaitingProc.SetProcessBarPerformStep();
            }
        }
    }
}
