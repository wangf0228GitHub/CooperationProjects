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
        int NT;
        double Imin, Imax;//照度范围
        void ExposureTest()
        {
            UIHide();
            exposureChart.Visible = true;
            exposureChart.Dock = DockStyle.Fill;
            exposureListView.Visible = true;
            exposureListView.Dock = DockStyle.Fill;
            exposureListView.Items[1].SubItems[1].Text=(SystemParam.NTmin.ToString());            

            textBox1.AppendText("开始明场曝光测试\r\n");
            NT = SystemParam.NTmin;
            exposureWaitingProc = new WaitingProc();
            exposureWaitingProc.MaxProgress = 10;
            WaitingProcFunc wpf = new WaitingProcFunc(WaitingExposureTest);
            if (!exposureWaitingProc.Execute(wpf, "曝光步距测试中", WaitingType.With_ConfirmCancel, "是否取消？"))
            {
                textBox1.AppendText("用户终止自动测试\r\n");
                return;
            }
            
        }
        void WaitingExposureTest(object LockWatingThread)
        {
            Imin = 0;
            Imax = 100;
            while (true)
            {                
                double[] miu = new double[10];
                double[] delta = new double[10];
                double[] miuCC = new double[10];
                exposureWaitingProc.SetProcessBar(0);
                this.Invoke((EventHandler)(delegate
                {
                    WFGlobal.WaitMS(1);
                    exposureChart.Series["miu"].Points.Clear();
                    exposureChart.Series["delta"].Points.Clear();
                    double gap = (Imax - Imin) / 9;
                    for (int step = 0; step < 10; step++)
                    {
                        double oe_per = Imin+gap * step;
                        tcpCCS.LightSet(SystemParam.lambda_Oe, oe_per);
                        exposureListView.Items[2].SubItems[1].Text = (Imin.ToString("F2") + "-" + Imax.ToString("F2"));
                        exposureListView.Items[3].SubItems[1].Text = oe_per.ToString("F2");
                        if (!UDPProc.CollectImage(this, NT, 2))
                        {
                            exposureWaitingProc.ExitWatting();
                            return;
                        }
                        UDPProc.ccdImageList[0].save(SystemParam.TempPicPath + "E" + step.ToString() + "_0.bin");
                        UDPProc.ccdImageList[1].save(SystemParam.TempPicPath + "E" + step.ToString() + "_1.bin");
                        ccdImage.Calc_miu_delta(UDPProc.ccdImageList[0], UDPProc.ccdImageList[1], out miu[step], out delta[step], out miuCC[step]);
                        exposureChart.Series["miu"].Points.AddXY(oe_per, miu[step]);
                        exposureChart.Series["delta"].Points.AddXY(oe_per, delta[step]);
                        if (exposureWaitingProc.HasBeenCancelled())
                        {
                            return;
                        }
                        exposureWaitingProc.SetProcessBarPerformStep();
                    }                
                }));
                if (exposureWaitingProc.HasBeenCancelled())
                {
                    return;
                }
                // 方差全部相等（且接近0），则在之前照度第一个区间分10个照度等级
                double close0 = 0.01;
                if (delta[0]<close0 && delta[1]<close0)
                {
                    Imax = (Imax - Imin) / 9;
                }
                //如果方差单调升则修改NT=10*NT_min
                else if (delta[0]<delta[1])//单调增
                {                    
                    bool bMonotonicity = true;
                    for (int i = 0; i < 9; i++)
                    {
                        if (delta[i + 1]<delta[i])//增减转折
                        {
                            Imin= (Imax - Imin) * (i+1);
                            bMonotonicity = false;
                            break;
                        }
                    }
                    if (bMonotonicity)
                    {
                        NT = NT * 10;
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
                else//第一个就是减，那应该都是减函数
                {
                    SystemParam.Oe = _tcpCCS.Per2LX( Imin /(SystemParam.n-5));
                    iniFileOP.Write("Light Param", "Oe", SystemParam.Oe.ToString());
                    SystemParam.Osat= _tcpCCS.Per2LX(Imin);
                    iniFileOP.Write("CCD Param", "Osat", SystemParam.Osat.ToString(),strCCDINIPath);
                    SystemParam.miu_sat = miu[0];
                    iniFileOP.Write("CCD Param", "miu_sat", SystemParam.miu_sat.ToString(), strCCDINIPath);
                    SystemParam.NTexp = SystemParam.NTmin * (SystemParam.n - 5) / SystemParam.n;
                    iniFileOP.Write("Collect Param", "NTexp", SystemParam.NTexp.ToString());
                    this.Invoke((EventHandler)(delegate
                    {
                        exposureListView.Items[4].SubItems[1].Text = SystemParam.Oe.ToString("F2");
                        exposureListView.Items[5].SubItems[1].Text = miu[0].ToString("F2");
                        exposureListView.Items[6].SubItems[1].Text = SystemParam.NTexp.ToString("F2");
                    }));                    
                    MessageBox.Show("曝光步距测试完毕");
                    return;
                }                
            }
        }
    }
}
