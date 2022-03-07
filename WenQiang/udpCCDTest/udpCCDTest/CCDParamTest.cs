using System;
using System.Collections.Generic;
using System.IO;
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
        int bTestMode = 0 ;//0:udp,1:bin,2:txt
        string ParamTestWaitingProcTitle;
        int ParamTestWaitingProcMax;
        WaitingProc ParamTestWaitingProc;
        int ExType;//曝光方式
        bool[] ParamTestList;
        int TestType;//测试内容：参数测试 or 三温测试
        void CCDTest()
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
                      
            if (TestType == 0)//参数测试
            {
                CCDParamTest();
            }
            else
            {
                CCD3TTest();
            }
        }
        CCDParamTestResult ccdParamTestResult;
        CCD3TTestResult ccd3TTestResult;
        void CCDParamTest()
        {
            UIHide();
            CCDParamTestListView.Visible = true;
            for (int i = 13; i >=1; i--)
            {
                if (!ParamTestList[i])
                    CCDParamTestListView.Items[5 + i].SubItems[1].Text = "未选测试";
                else
                    CCDParamTestListView.Items[5 + i].SubItems[1].Text = "待测";
            }
            CCDParamTestListView.Visible = true;
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
            CCDParamTestListView.Items[2].SubItems[1].Text = ccdParamTestResult.miu_sat.ToString();
            CCDParamTestListView.Items[3].SubItems[1].Text = ccdParamTestResult.Osat.ToString();
            CCDParamTestListView.Items[4].SubItems[1].Text = SystemParam.n.ToString();
            CCDParamTestListView.Items[5].SubItems[1].Text = SystemParam.L.ToString();
            ParamTestReset();
            //量子效率曲线
            if (ParamTestList[3])
            {
                bool betaCurve = CCDParamTest_etaCurve();
                if (!betaCurve)
                {
                    MessageBox.Show("量子效率曲线测试失败");
                    return;
                }
                CCDParamTestListView.Items[5 + 3].SubItems[1].Text = "测试完成";//ccdParamTestResult.etaCurve.ToString();
            }
            else
                etaCurve = null;

            if (ParamTestList[0] || double.IsNaN(ccdParamTestResult.Osat))//曝光测试
                if (!ExposureTest())
                    return;
            CCDParamTestListView.Items[2].SubItems[1].Text = ccdParamTestResult.miu_sat.ToString();
            CCDParamTestListView.Items[3].SubItems[1].Text = ccdParamTestResult.Osat.ToString();
            if (ParamTestList[13])
            {
                bool bPGACurve = CCDParamTest_PGACurve();
                if (!bPGACurve)
                {
                    MessageBox.Show("增益曲线测试失败");
                    return;
                }
                CCDParamTestListView.Items[5 + 13].SubItems[1].Text = "测试完成";//ccdParamTestResult.etaCurve.ToString();
            }            
            CCDParamTestListView.Visible = true;
            //转换增益，量子效率，信噪比曲线，线性误差
            if (ParamTestList[1])//转换增益
            {
                if (!CCDParamTest_K())
                    return;
                
            }
            CCDParamTestListView.Items[5 + 1].SubItems[1].Text = ccdParamTestResult.K.ToString("F3");
            //             else
            //             {
            //                 ccdParamTestResult.K = double.NaN;
            //             }
            if (ParamTestList[2])//量子效率
            {
                if (!CCDParamTest_eta())
                    return;               
            }
            CCDParamTestListView.Items[5 + 2].SubItems[1].Text = ccdParamTestResult.eta.ToString("F3");
            //             else
            //             {
            //                 ccdParamTestResult.eta = double.NaN;
            //             }
            if (ParamTestList[4])//信噪比
            {
                if (!CCDParamTest_SNR())
                    return;
                CCDParamTestListView.Items[5 + 4].SubItems[1].Text = ccdParamTestResult.SNRMax.ToString("F3")
                    +"____miupsat:"+ ccdParamTestResult.miu_p_sat.ToString("F3")
                    +"___miupmin:" + ccdParamTestResult.miu_p_min.ToString("F3")
                    + "___deltadarkmin:" + Collect_SNR_delta_dark.ToString("F3");
            }
            else
            {
                ccdParamTestResult.miu_p_min = double.NaN;
                ccdParamTestResult.miu_p_sat = double.NaN;
            }
            //动态范围、满阱容量、FPN
            if (ParamTestList[5] || ParamTestList[6])
            {
                if (!CCDParamTest_DR_FWC())
                    return;
                CCDParamTestListView.Items[5 + 6].SubItems[1].Text = ccdParamTestResult.FWC.ToString("F3");
                CCDParamTestListView.Items[5 + 5].SubItems[1].Text = ccdParamTestResult.DR.ToString("F3");
            }
            else
            {
                ccdParamTestResult.FWC=double.NaN;
                ccdParamTestResult.DR=double.NaN;
            }
            if (ParamTestList[7])//暗电流
            {
                if (!CCDParamTest_DarkI())
                    return;
                CCDParamTestListView.Items[5 + 7].SubItems[1].Text = ccdParamTestResult.miu_I_miu.ToString("F2") + "; " + ccdParamTestResult.miu_I_delta.ToString("F2");
            }
            else
            {
                ccdParamTestResult.miu_I_miu = double.NaN;
                ccdParamTestResult.miu_I_delta = double.NaN; ;
            }
            if (ParamTestList[8])//线性误差
            {
                if(!CCDParamTest_LE())
                {
                    return;
                }
                CCDParamTestListView.Items[5 + 8].SubItems[1].Text = ccdParamTestResult.LE.ToString("F3");
            }
            else
            {
                ccdParamTestResult.LE = double.NaN;
            }
            //DSNU,PRNU,读出噪声
            if (ParamTestList[9] || ParamTestList[10] || ParamTestList[11] || ParamTestList[12])
            {
                if(bTestMode==0)
                {
                    if (!CCDParamTest_Collect_L(false))
                        return;
                    if (ParamTestList[10])
                        if (!CCDParamTest_Collect_L(true))
                            return;
                }

                CCDParamTest_Calc_L_bLight = false;
                ParamTestWaitingProc = new WaitingProc();
                WaitingProcFunc wpf = null;
                ParamTestWaitingProc.MaxProgress = SystemParam.L;
                wpf = new WaitingProcFunc(CCDParamTest_Calc_L);
                if (!ParamTestWaitingProc.Execute(wpf, "处理空域数据", WaitingType.With_ConfirmCancel, "是否取消？"))
                {
                    textBox1.AppendText("用户终止自动测试\r\n");
                    return;
                }
                if (ParamTestList[9])//DSNU
                {
                    ccdParamTestResult.DSNU = Math.Sqrt(ccdParamTestResult.L_S_y_dark) / ccdParamTestResult.K;
                    CCDParamTestListView.Items[5 + 9].SubItems[1].Text = ccdParamTestResult.DSNU.ToString("F3");
                }
                else
                {
                    ccdParamTestResult.DSNU = double.NaN;
                }
                if (ParamTestList[11])//FPN
                {
                    if(double.IsNaN(ccdParamTestResult.FWC))
                    {
                        if (!CCDParamTest_DR_FWC())
                            return;
                    }
                    ccdParamTestResult.DSNU = Math.Sqrt(ccdParamTestResult.L_S_y_dark) / ccdParamTestResult.K;
                    ccdParamTestResult.FPN = 100*ccdParamTestResult.DSNU / ccdParamTestResult.FWC;
                    CCDParamTestListView.Items[5 + 11].SubItems[1].Text = ccdParamTestResult.FPN.ToString("F3");
                }
                else
                {
                    ccdParamTestResult.FPN = double.NaN;
                }
                if (ParamTestList[12])//读出噪声
                {
                    ccdParamTestResult.delta_raed = Math.Sqrt(ccdParamTestResult.delta_mid) / ccdParamTestResult.K;
                    ccdParamTestResult.delta_raed_avr = ccdParamTestResult.delta_mid_avr / ccdParamTestResult.K;
                    CCDParamTestListView.Items[5 + 12].SubItems[1].Text = ccdParamTestResult.delta_raed.ToString("F3")+"___"+ccdParamTestResult.delta_raed_avr.ToString("F3");
                }
                else
                {
                    ccdParamTestResult.delta_raed = double.NaN;
                }
                if (ParamTestList[10])//PRNU
                {
                    CCDParamTest_Calc_L_bLight = true;
                    ParamTestWaitingProc = new WaitingProc();
                    wpf = null;
                    ParamTestWaitingProc.MaxProgress = SystemParam.L;
                    wpf = new WaitingProcFunc(CCDParamTest_Calc_L);
                    if (!ParamTestWaitingProc.Execute(wpf, "处理空域数据", WaitingType.With_ConfirmCancel, "是否取消？"))
                    {
                        textBox1.AppendText("用户终止自动测试\r\n");
                        return;
                    }
                    ccdParamTestResult.PRNU = Math.Sqrt(ccdParamTestResult.L_S_y - ccdParamTestResult.L_S_y_dark) / (ccdParamTestResult.L_miu_y - ccdParamTestResult.L_miu_y_dark);
                    CCDParamTestListView.Items[5 + 10].SubItems[1].Text = ccdParamTestResult.PRNU.ToString("F3");
                }
                else
                {
                    ccdParamTestResult.PRNU = double.NaN;
                }
            }
            tcpCCS.LightSet(SystemParam.lambda_Oe,0);
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {

                MakeReport(saveFileDialog1.FileName);
                textBox1.AppendText("自动测试结束,测试报告保存位置为:\r\n");
                textBox1.AppendText(saveFileDialog1.FileName);
            }
            else
            {
                textBox1.AppendText("自动测试结束,用户未保存报告\r\n");
            }
            MessageBox.Show("参数测试完成");
        }
        void CCD3TTest()
        {
            ccd3TTestResult.Reset();            
            UIHide();
            CCD3TTestListView.Visible = true;
            CCD3TTestListView.Items[1].SubItems[1].Text = ccdParamTestResult.K.ToString();
            CCD3TTestListView.Items[2].SubItems[1].Text = SystemParam.L.ToString();
            for (int i = 0; i <4; i++)
            {
                if (!ParamTestList[i])
                {
                    CCDParamTestListView.Items[4 + i].SubItems[1].Text = "未选测试";
                    CCDParamTestListView.Items[4+5 + i].SubItems[1].Text = "未选测试";
                    CCDParamTestListView.Items[4 +10+ i].SubItems[1].Text = "未选测试";
                }
                else
                {
                    CCDParamTestListView.Items[4 + i].SubItems[1].Text = "待测";
                    CCDParamTestListView.Items[4 + 5 + i].SubItems[1].Text = "待测";
                    CCDParamTestListView.Items[4 + 10 + i].SubItems[1].Text = "待测";
                }
            }
            
            CCDParamTestListView.Items[1].SubItems[1].Text = ccdParamTestResult.K.ToString();
            CCDParamTestListView.Items[2].SubItems[1].Text = SystemParam.L.ToString();
            if (ParamTestList[0])//暗电流
            {                
                ccd3TTestResult.miu_I_miu = new double[3];
                ccd3TTestResult.miu_I_delta = new double[3];                
            }
            //DSNU
            if (ParamTestList[1] || ParamTestList[2])
            {
                ccd3TTestResult.DSNU = new double[3];
            }
            //FPN
            if (ParamTestList[2])
            {
                ccd3TTestResult.FPN = new double[3];
            }
            //读出噪声
            if (ParamTestList[3])
            {
                ccd3TTestResult.delta_raed = new double[3];
            }

            for (int i = 0; i < 3; i++)
            {
                while(!double.TryParse(InputBox.ShowInputBox("请设定温度" + (i + 1).ToString(), "25"),out ccd3TTestResult.T[i]))
                {
                    MessageBox.Show("设定温度有误，请重新设定");
                }
                CCD3TTestListView.Items[3 + 5 * i].SubItems[1].Text = ccd3TTestResult.T[i].ToString();
                if (ParamTestList[0])//暗电流
                {
                    if (!CCDParamTest_DarkI())
                        return;
                    CCD3TTestListView.Items[4+5*i].SubItems[1].Text = ccdParamTestResult.miu_I_miu.ToString("F2") + "; " + ccdParamTestResult.miu_I_delta.ToString("F2");
                    ccd3TTestResult.miu_I_miu[i] = ccdParamTestResult.miu_I_miu;
                    ccd3TTestResult.miu_I_delta[i] = ccdParamTestResult.miu_I_delta;
                    FileOP.CopyFile(SystemParam.TempPicPath + "DarkI1.jpg", SystemParam.TempPicPath + "DarkI1_T" + (i + 1).ToString() + ".jpg");
                    FileOP.CopyFile(SystemParam.TempPicPath + "DarkI2.jpg", SystemParam.TempPicPath + "DarkI2_T" + (i + 1).ToString() + ".jpg");
                }
                //DSNU,FPN,读出噪声
                if(ParamTestList[1] || ParamTestList[2] || ParamTestList[3])
                {
                    if (bTestMode == 0)
                    {
                        if (!CCDParamTest_Collect_L(false))
                            return;
                    }
                    CCDParamTest_Calc_L_bLight = false;
                    ParamTestWaitingProc = new WaitingProc();
                    WaitingProcFunc wpf = null;
                    ParamTestWaitingProc.MaxProgress = SystemParam.L;
                    wpf = new WaitingProcFunc(CCDParamTest_Calc_L);
                    if (!ParamTestWaitingProc.Execute(wpf, "处理暗场空域数据", WaitingType.With_ConfirmCancel, "是否取消？"))
                    {
                        textBox1.AppendText("用户终止自动测试\r\n");
                        return;
                    }
                    //DSNU
                    if(ParamTestList[1] || ParamTestList[2])
                    {
                        ccd3TTestResult.DSNU[i] = Math.Sqrt(ccdParamTestResult.L_S_y_dark) / ccdParamTestResult.K;
                        CCD3TTestListView.Items[4 + 5 * i+1].SubItems[1].Text = ccd3TTestResult.DSNU[i].ToString();
                    }
                    //FPN
                    if(ParamTestList[2])
                    {
                        if (double.IsNaN(ccdParamTestResult.FWC))
                        {
                            if (!CCDParamTest_DR_FWC())
                                return;
                        }
                        ccd3TTestResult.FPN[i] = 100*ccd3TTestResult.DSNU[i] / ccdParamTestResult.FWC;
                        CCD3TTestListView.Items[4 + 5 * i + 2].SubItems[1].Text = ccd3TTestResult.FPN[i].ToString();
                    }
                    //读出噪声
                    if(ParamTestList[3])
                    {
                        ccd3TTestResult.delta_raed[i] = Math.Sqrt(ccdParamTestResult.delta_mid) / ccdParamTestResult.K;
                        CCD3TTestListView.Items[4 + 5 * i + 3].SubItems[1].Text = ccd3TTestResult.delta_raed[i].ToString();
                    }
                }
            }
            tcpCCS.LightSet(SystemParam.lambda_Oe, 0);
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                MakeReport3T(saveFileDialog1.FileName);
                textBox1.AppendText("自动测试结束,测试报告保存位置为:\r\n");
                textBox1.AppendText(saveFileDialog1.FileName);
            }
            else
            {
                textBox1.AppendText("自动测试结束,用户未保存报告\r\n");
            }
            MessageBox.Show("三温测试完成");
        }  
        void ParamTestReset()
        {
            Collect_Step_miu1=null;
            Collect_Step_delta1 = null;
            Collect_Step_E1 = null;//照度

            Collect_Step_miu2 = null;
            Collect_Step_delta2 = null;
            Collect_Step_E2 = null;//照度

            Collect_Step_miu_dark2 = null;
            Collect_Step_delta_dark2 = null;
        }      
    }
    public class CCDParamTestResult
    {
        public double Osat
        {
            get
            {
                string str = iniFileOP.Read("CCD", "Osat", SystemParam.strCCDINIPath);
                if (str == "xx" || str == "")
                    return double.NaN;
                else
                    return double.Parse(str);
            }
            set
            {
                if (double.IsNaN(value))
                    iniFileOP.Write("CCD", "Osat", "xx", SystemParam.strCCDINIPath);
                else
                    iniFileOP.Write("CCD", "Osat", value.ToString("F6"), SystemParam.strCCDINIPath);
            }
        }
        public double miu_sat
        {
            get
            {
                string str = iniFileOP.Read("CCD", "miu_sat", SystemParam.strCCDINIPath);
                if (str == "xx" || str == "")
                    return double.NaN;
                else
                    return double.Parse(str);
            }
            set
            {
                if (double.IsNaN(value))
                    iniFileOP.Write("CCD", "miu_sat", "xx", SystemParam.strCCDINIPath);
                else
                    iniFileOP.Write("CCD", "miu_sat", value.ToString("F2"), SystemParam.strCCDINIPath);
            }
        }
        public double OeStep
        {
            get
            {
                string str = iniFileOP.Read("CCD", "OeStep", SystemParam.strCCDINIPath);
                if (str == "xx" || str == "")
                    return double.NaN;
                else
                    return double.Parse(str);
            }
            set
            {
                if (double.IsNaN(value))
                    iniFileOP.Write("CCD", "OeStep", "xx", SystemParam.strCCDINIPath);
                else
                    iniFileOP.Write("CCD", "OeStep", value.ToString("F6"), SystemParam.strCCDINIPath);
            }
        }
        public double OeLight
        {
            get
            {
                string str = iniFileOP.Read("CCD", "OeLight", SystemParam.strCCDINIPath);
                if (str == "xx" || str == "")
                    return double.NaN;
                else
                    return double.Parse(str);
            }
            set
            {
                if (double.IsNaN(value))
                    iniFileOP.Write("CCD", "OeLight", "xx", SystemParam.strCCDINIPath);
                else
                    iniFileOP.Write("CCD", "OeLight", value.ToString("F6"), SystemParam.strCCDINIPath);
            }
        }
        public uint NTmin1
        {
            get
            {
                string str = iniFileOP.Read("CCD", "NTmin1", SystemParam.strCCDINIPath);
                if (str == "xx" || str == "")
                    return uint.MaxValue;
                else
                    return uint.Parse(str);
            }
            set
            {
                if (uint.MaxValue==value)
                    iniFileOP.Write("CCD", "NTmin1", "xx", SystemParam.strCCDINIPath);
                else
                    iniFileOP.Write("CCD", "NTmin1", value.ToString(), SystemParam.strCCDINIPath);
            }
        }
        public uint NTexp2
        {
            get
            {
                string str = iniFileOP.Read("CCD", "NTexp2", SystemParam.strCCDINIPath);
                if (str == "xx" || str == "")
                    return uint.MaxValue;
                else
                    return uint.Parse(str);
            }
            set
            {
                if (uint.MaxValue==value)
                    iniFileOP.Write("CCD", "NTexp2", "xx", SystemParam.strCCDINIPath);
                else
                    iniFileOP.Write("CCD", "NTexp2", value.ToString(), SystemParam.strCCDINIPath);
            }
        }

        public double K
        {
            get
            {
                string str = iniFileOP.Read("CCD", "K", SystemParam.strCCDINIPath);
                if (str == "xx" || str == "")
                    return double.NaN;
                else
                    return double.Parse(str);
            }
            set
            {
                if (double.IsNaN(value))
                    iniFileOP.Write("CCD", "K", "xx", SystemParam.strCCDINIPath);
                else
                    iniFileOP.Write("CCD", "K", value.ToString("F6"), SystemParam.strCCDINIPath);
            }
        }
        public double eta
        {
            get
            {
                string str = iniFileOP.Read("CCD", "eta", SystemParam.strCCDINIPath);
                if (str == "xx" || str == "")
                    return double.NaN;
                else
                    return double.Parse(str);
            }
            set
            {
                if (double.IsNaN(value))
                    iniFileOP.Write("CCD", "eta", "xx", SystemParam.strCCDINIPath);
                else
                    iniFileOP.Write("CCD", "eta", value.ToString("F6"), SystemParam.strCCDINIPath);
            }
        }

        public double miu_p_min;
        public double miu_p_sat;
        public double SNRMax;

        public double DR;

        public double FWC;

        public double miu_I_delta;
        public double miu_I_miu;

        public double LE;
        
        public double L_miu_y;
        public double L_S_y;
        public double L_miu_y_dark;
        public double L_S_y_dark;

        public double DSNU;
        public double PRNU;
        public double FPN;
        public double delta_raed;
        public double delta_mid;
        public double delta_raed_avr;
        public double delta_mid_avr;


        public bool bmiuCurve;
        public bool bDarkICurve;
        public bool bSNRCurve;
        public bool betaCurve;
        public bool bPGACurve;
        public CCDParamTestResult()
        {
            Reset();
        }
        public void Reset()
        {
            //K = double.NaN;
            //eta = double.NaN;

            miu_p_min = double.NaN;
            miu_p_sat = double.NaN;
            SNRMax = double.NaN;
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

            bmiuCurve = false;
            bDarkICurve = false;
            bSNRCurve = false;
            betaCurve = false;
            bPGACurve=false;
        }
    }

    public class CCD3TTestResult
    {
        public double[] T;
        public double[] miu_I_delta;
        public double[] miu_I_miu;
        public double[] DSNU;
        public double[] FPN;
        public double[] delta_raed;
                
        public bool bDarkICurve;

        public void Reset()
        {
            T = new double[3];
            miu_I_delta = null;
            miu_I_miu = null;
            FPN = null;
            DSNU = null;
            delta_raed = null;
            bDarkICurve = false;
        }
    }
}
