using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WFNetLib;
using WFNetLib.Algorithm;

namespace udpCCDTest
{
    public partial class FormCCS_Calibration : Form
    {
        public FormCCS_Calibration()
        {
            InitializeComponent();
        }

        private void FormCCS_Calibration_Load(object sender, EventArgs e)
        {            
            for (int i=0;i<tcpCCS.lambdaList.Length;i++)
            {
                cbLambda.Items.Add(tcpCCS.lambdaList[i].ToString() + " nm");
            }
            cbLambda.SelectedIndex = 0;
        }
        string aFormat = "F3";
        double step;
        int lambadIndex;
        WaitingProc ccsWaitingProc;
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if(cbLambda.SelectedIndex==0)
            {
                MessageBox.Show("请选择所需校准的光波长!!");
                return;
            }
            if(!double.TryParse(tbStep.Text,out step) || step>=100)
            {
                MessageBox.Show("校准步长输入有误，请重新输入!!");
                return;
            }
            step = step / 100;
            lambadIndex = cbLambda.SelectedIndex - 2;
            a2 = new List<double>();
            a1 = new List<double>();
            a0 = new List<double>();
            if (cbLambda.SelectedIndex != 1)
            {
                chart.Series[0].Points.Clear();
                chart.Series[1].Points.Clear();
                lambadIndex = cbLambda.SelectedIndex - 2;
                ccsWaitingProc = new WaitingProc();
                ccsWaitingProc.MaxProgress = (int)(1 / step) + 1;
                WaitingProcFunc wpf = new WaitingProcFunc(WaitingCCS);
                if (!ccsWaitingProc.Execute(wpf, "光源波长"+tcpCCS.lambdaList[lambadIndex].ToString()+" nm校准中", WaitingType.With_ConfirmCancel, "是否取消？"))
                {
                    return;
                }
                tcpCCS.L2E_a2[lambadIndex] = a2[0];
                tcpCCS.L2E_a1[lambadIndex] = a1[0];
                tcpCCS.L2E_a0[lambadIndex] = a0[0];
            }
            else //全部
            {
                for(int i=0;i<tcpCCS.lambdaList.Length;i++)
                {
                    chart.Series[0].Points.Clear();
                    chart.Series[1].Points.Clear();
                    lambadIndex = cbLambda.SelectedIndex - 2;
                    ccsWaitingProc = new WaitingProc();
                    ccsWaitingProc.MaxProgress = (int)(1 / step) + 1;
                    WaitingProcFunc wpf = new WaitingProcFunc(WaitingCCS);
                    if (!ccsWaitingProc.Execute(wpf, "光源波长" + tcpCCS.lambdaList[lambadIndex].ToString() + " nm校准中", WaitingType.With_ConfirmCancel, "是否取消？"))
                    {
                        return;
                    }
                    MessageBox.Show("波长" + tcpCCS.lambdaList[lambadIndex].ToString() + " nm校准完成");                 
                }
                for (int i = 0; i < tcpCCS.lambdaList.Length; i++)
                {
                    tcpCCS.L2E_a2[i] = a2[i];
                    tcpCCS.L2E_a1[i] = a1[i];
                    tcpCCS.L2E_a0[i] = a0[i];
                }
            }
            string stra2 = "";
            string stra1 = "";
            string stra0 = "";
            for (int i=0; i < tcpCCS.lambdaList.Length;i++)
            {
                stra2 += tcpCCS.L2E_a2[i].ToString(aFormat)+",";
                stra1 += tcpCCS.L2E_a1[i].ToString(aFormat)+",";
                stra0 += tcpCCS.L2E_a1[i].ToString(aFormat)+",";
            }
            stra2 = stra2.Substring(0, stra2.Length - 1);
            stra1 = stra1.Substring(0, stra1.Length - 1);
            stra0 = stra0.Substring(0, stra0.Length - 1);
            iniFileOP.Write("Light Param", "L2E_a2", stra2);
            iniFileOP.Write("Light Param", "L2E_a1", stra1);
            iniFileOP.Write("Light Param", "L2E_a0", stra0);
            MessageBox.Show("校准完成!!!");
        }
        List<double> a2;
        List<double> a1;
        List<double> a0;
        void WaitingCCS(object LockWatingThread)
        {
            List<double> OeList = new List<double>();
            List<double> EList = new List<double>();
            for (double Oe = 0; Oe < 1; Oe += step)
            {
                OeList.Add(Oe);
                tcpCCS.LightSet(tcpCCS.lambdaList[lambadIndex], Oe);
                WFGlobal.WaitMS(100);
                DeviceState ds = UDPProc.UDPCommand_04();
                if (ds == null)
                {
                    MessageBox.Show("照度采集失败，测试终止\r\n");
                    ccsWaitingProc.ExitWatting();
                    return;
                }
                EList.Add(ds.Illuminance);
                this.Invoke((EventHandler)(delegate
                {
                    chart.Series[0].Points.AddXY(Oe, ds.Illuminance);
                }));
                if (ccsWaitingProc.HasBeenCancelled())
                {
                    return;
                }
            }
            double[] fitret = FittingMultiLine.MultiLine(OeList.ToArray(), EList.ToArray(), OeList.Count, 2);
            a2.Add(double.Parse(fitret[2].ToString(aFormat)));
            a1.Add(double.Parse(fitret[1].ToString(aFormat)));
            a0.Add(double.Parse(fitret[0].ToString(aFormat)));
            this.Invoke((EventHandler)(delegate
            {
                for (double Oe = 0; Oe < 1; Oe += step)
                {
                    chart.Series[1].Points.AddXY(Oe, a2.Last() * Oe * Oe + a1.Last() * Oe + a0.Last());
                }
                chart.SaveImage(SystemParam.TempPicPath + "ccs_"+tcpCCS.lambdaList[lambadIndex].ToString()+".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            }));
        }
    }
}
