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
        WaitingProc exposureVerifyWaitingProc;
        void ExposureVerify()
        {
            UIHide();
            exposureVerifyChart.Visible = true;
            exposureVerifyChart.Dock = DockStyle.Fill;
            exposureVerifyListView.Visible = true;
            exposureVerifyListView.Dock = DockStyle.Fill;
            exposureVerifyListView.Items[1].SubItems[1].Text = SystemParam.NTmin2.ToString() + "(" + SystemParam.GetTime2(0, ccdParamTestResult.NTexp2).ToString("F3") + "ms)"; ; ;
            exposureVerifyListView.Items[2].SubItems[1].Text = ccdParamTestResult.OeLight.ToString("F2") + "(" + tcpCCS.LX2Per(ccdParamTestResult.OeLight).ToString("F6") + "%)";
            exposureVerifyListView.Items[3].SubItems[1].Text = SystemParam.n.ToString();
            exposureVerifyListView.Items[4].SubItems[1].Text = "0";

            textBox1.AppendText("开始系统稳定性及曝光步距验证\r\n");
            exposureVerifyWaitingProc = new WaitingProc();
            exposureVerifyWaitingProc.MaxProgress = SystemParam.n;
            WaitingProcFunc wpf = new WaitingProcFunc(WaitingExposureVerify);
            if (!exposureVerifyWaitingProc.Execute(wpf, "系统稳定性及曝光步距验证", WaitingType.With_ConfirmCancel, "是否取消？"))
            {
                textBox1.AppendText("用户终止自动测试\r\n");
                return;
            }

        }
        void WaitingExposureVerify(object LockWatingThread)
        {
            while (true)
            {
                double miu;
                double delta;
                double miuCC;
                this.Invoke((EventHandler)(delegate
                {
                    exposureVerifyChart.Series["miu"].Points.Clear();
                    exposureVerifyChart.Series["delta"].Points.Clear();
                    exposureVerifyChart.Series["miuCC"].Points.Clear();
                    tcpCCS.LightSet(SystemParam.lambda_Oe, ccdParamTestResult.OeLight);
                    for (int step = 0; step < SystemParam.n; step++)
                    {
                        WFGlobal.WaitMS(1);
                        uint ex = SystemParam.NTmin2 + (uint)step * ccdParamTestResult.NTexp2;                        
                        exposureVerifyListView.Items[4].SubItems[1].Text = step.ToString();
                        if (!UDPProc.CollectImage(this, ex, 2))
                        {
                            exposureVerifyWaitingProc.ExitWatting();
                            return;
                        }
                        //UDPProc.ccdImageList[0].save(SystemParam.TempPicPath + "Ev" + step.ToString() + "_0.bin");
                        //UDPProc.ccdImageList[1].save(SystemParam.TempPicPath + "Ev" + step.ToString() + "_1.bin");
                        ccdImage.Calc_miu_delta(UDPProc.ccdImageList[0], UDPProc.ccdImageList[1], out miu, out delta, out miuCC);
                        exposureVerifyChart.Series["miu"].Points.AddXY(step, miu);
                        exposureVerifyChart.Series["delta"].Points.AddXY(step, delta);
                        exposureVerifyChart.Series["miuCC"].Points.AddXY(step, miuCC);
                        if (exposureVerifyWaitingProc.HasBeenCancelled())
                        {
                            return;
                        }
                        exposureVerifyWaitingProc.SetProcessBarPerformStep();
                    }                    
                }));
                if (exposureVerifyWaitingProc.HasBeenCancelled())
                {
                    return;
                }
                if (MessageBox.Show("测试完毕，是否修改要修改参数", "测试完成", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    FormParam f = new FormParam();
                    f.ShowDialog();
                }
                else
                    return;
            }
        }
    }
}
