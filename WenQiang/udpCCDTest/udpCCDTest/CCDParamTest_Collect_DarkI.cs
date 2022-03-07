using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WFNetLib;

namespace udpCCDTest
{
    public partial class FormMain : Form
    {
        List<double> Collect_DarkI_miu;
        List<double> Collect_DarkI_delta;
        void CCDParamTest_Collect_DarkI()
        {
            UIHide();
            ParamTestChart1.Visible = true;
            ParamTestChart1.Dock = DockStyle.Fill;

            Collect_DarkI_miu = new List<double>();
            Collect_DarkI_delta = new List<double>();
            ParamTestWaitingProc = new WaitingProc();
            string str = "";
            WaitingProcFunc wpf = null;
            textBox1.AppendText("测量暗电流，暗场并按逐步改变曝光时间方式采集图像\r\n");
            str = "测量暗电流，暗场并按逐步改变曝光时间方式采集图像中";
            ParamTestWaitingProc.MaxProgress = 16;
            wpf = new WaitingProcFunc(WaitingCollect_DarkI);
            if (!ParamTestWaitingProc.Execute(wpf, str, WaitingType.With_ConfirmCancel, "是否取消？"))
            {
                textBox1.AppendText("用户终止自动测试\r\n");
                return;
            }
        }
        void WaitingCollect_DarkI(object LockWatingThread)
        {
            double miu = 0;
            double miuCC;
            double delta;
            uint Tex;
            //double E;
            this.Invoke((EventHandler)(delegate
            {
                WFGlobal.WaitMS(1);
                exposureChart.Series["miu"].Points.Clear();
                exposureChart.Series["delta"].Points.Clear();
                tcpCCS.LightSet(SystemParam.lambda_Oe, 0.0);//暗场
            }));
            Tex = (uint)SystemParam.NTdark;
            //明场
            for(int i=0;i<16;i++)
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
                    UDPProc.ccdImageList[0].save(SystemParam.TempPicPath + "DarkI" + Tex.ToString("") + "_0.bin");
                    UDPProc.ccdImageList[1].save(SystemParam.TempPicPath + "DarkI" + Tex.ToString("") + "_1.bin");
                    ccdImage.Calc_miu_delta(UDPProc.ccdImageList[0], UDPProc.ccdImageList[1], out miu, out delta, out miuCC);
                    exposureChart.Series["miu"].Points.AddXY(Tex, miu);
                    exposureChart.Series["delta"].Points.AddXY(Tex, delta);                    
                    Collect_DarkI_miu.Add(miu);
                    Collect_DarkI_delta.Add(delta);
                }));                
                if (exposureWaitingProc.HasBeenCancelled())
                {
                    return;
                }
                Tex += (uint)SystemParam.delta_Tdark;
                ParamTestWaitingProc.SetProcessBarPerformStep();
            }
        }
    }
}
