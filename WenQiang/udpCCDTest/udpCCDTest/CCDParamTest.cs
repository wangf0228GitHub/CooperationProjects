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
        void CCDParamTest()
        {
            CCDParamTestResult.Reset();
            if (ParamTestList[0])//曝光测试
                ExposureTest();
            UIHide();
            CCDParamTestListView.Visible = true;
            for (int i = 12; i > 1; i--)
            {
                if (!ParamTestList[i])
                    CCDParamTestListView.Items[5 + i].SubItems[1].Text = "不进行测试";
                else
                    CCDParamTestListView.Items[5 + i].SubItems[1].Text = "待测";
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
                if (ParamTestList[10])
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

                TestReport.MakeReport(saveFileDialog1.FileName);
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
            CCD3TTestResult.Reset();            
            UIHide();
            CCD3TTestListView.Visible = true;
            for (int i = 0; i <4; i++)
            {
                if (!ParamTestList[i])
                {
                    CCDParamTestListView.Items[4 + i].SubItems[1].Text = "不进行测试";
                    CCDParamTestListView.Items[4+5 + i].SubItems[1].Text = "不进行测试";
                    CCDParamTestListView.Items[4 +10+ i].SubItems[1].Text = "不进行测试";
                }
                else
                {
                    CCDParamTestListView.Items[4 + i].SubItems[1].Text = "待测";
                    CCDParamTestListView.Items[4 + 5 + i].SubItems[1].Text = "待测";
                    CCDParamTestListView.Items[4 + 10 + i].SubItems[1].Text = "待测";
                }
            }
            
            CCDParamTestListView.Items[1].SubItems[1].Text = CCDParamTestResult.K.ToString();
            CCDParamTestListView.Items[2].SubItems[1].Text = SystemParam.L.ToString();
            if (ParamTestList[0])//暗电流
            {                
                CCD3TTestResult.miu_I_miu = new double[3];
                CCD3TTestResult.miu_I_delta = new double[3];                
            }
            //DSNU
            if (ParamTestList[1] || ParamTestList[2])
            {
                CCD3TTestResult.DSNU = new double[3];
            }
            //FPN
            if (ParamTestList[2])
            {

                CCD3TTestResult.FPN = new double[3];
            }
            //读出噪声
            if (ParamTestList[3])
            {
                CCD3TTestResult.delta_raed = new double[3];
            }

            for (int i = 0; i < 3; i++)
            {
                while(!double.TryParse(InputBox.ShowInputBox("请设定温度" + (i + 1).ToString(), "25"),out CCD3TTestResult.T[i]))
                {
                    MessageBox.Show("设定温度有误，请重新设定");
                }
                if(ParamTestList[0])//暗电流
                {
                    CCDParamTest_Collect_DarkI();
                    CCDParamTest_Calc_DarkI();
                    CCD3TTestListView.Items[4+5*i].SubItems[1].Text = CCDParamTestResult.miu_I_miu.ToString() + "; " + CCDParamTestResult.miu_I_delta.ToString();
                    CCD3TTestResult.miu_I_miu[i] = CCDParamTestResult.miu_I_miu;
                    CCD3TTestResult.miu_I_delta[i] = CCDParamTestResult.miu_I_delta;
                    FileOP.CopyFile(SystemParam.TempPicPath + "DarkI1.jpg", SystemParam.TempPicPath + "DarkI1_T" + (i + 1).ToString() + ".jpg");
                    FileOP.CopyFile(SystemParam.TempPicPath + "DarkI2.jpg", SystemParam.TempPicPath + "DarkI2_T" + (i + 1).ToString() + ".jpg");
                }
                //DSNU,FPN,读出噪声
                if(ParamTestList[1] || ParamTestList[2] || ParamTestList[3])
                {
                    CCDParamTest_Collect_L(false);
                    CCDParamTest_Calc_L_bLight = false;
                    ParamTestWaitingProc = new WaitingProc();
                    WaitingProcFunc wpf = null;
                    ParamTestWaitingProc.MaxProgress = SystemParam.L;
                    wpf = new WaitingProcFunc(CCDParamTest_Calc_L);
                    if (!ParamTestWaitingProc.Execute(wpf, "处理暗场空域测试结果", WaitingType.With_ConfirmCancel, "是否取消？"))
                    {
                        textBox1.AppendText("用户终止自动测试\r\n");
                        return;
                    }
                    //DSNU
                    if(ParamTestList[1] || ParamTestList[2])
                    {
                        CCD3TTestResult.DSNU[i] = Math.Sqrt(CCDParamTestResult.L_S_y_dark) / CCDParamTestResult.K;
                        CCD3TTestListView.Items[4 + 5 * i+1].SubItems[1].Text = CCD3TTestResult.DSNU[i].ToString();
                    }
                    //FPN
                    if(ParamTestList[2])
                    {
                        CCDParamTest_Collect_MinMax();
                        CCDParamTest_Calc_FWC();
                        CCD3TTestResult.FPN[i] = CCD3TTestResult.DSNU[i] / CCDParamTestResult.FWC;
                        CCD3TTestListView.Items[4 + 5 * i + 2].SubItems[1].Text = CCD3TTestResult.FPN[i].ToString();
                    }
                    //读出噪声
                    if(ParamTestList[3])
                    {
                        CCD3TTestResult.delta_raed[i] = Math.Sqrt(CCDParamTestResult.delta_mid) / CCDParamTestResult.K;
                        CCD3TTestListView.Items[4 + 5 * i + 3].SubItems[1].Text = CCD3TTestResult.delta_raed[i].ToString();
                    }
                }
            }            
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                TestReport.MakeReport3T(saveFileDialog1.FileName);
                textBox1.AppendText("自动测试结束,测试报告保存位置为:\r\n");
                textBox1.AppendText(saveFileDialog1.FileName);
            }
            else
            {
                textBox1.AppendText("自动测试结束,用户未保存报告\r\n");
            }
            MessageBox.Show("三温测试完成");
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

        public static bool bmiuCurve;
        public static bool bDarkICurve;
        public static bool bSNRCurve;
        public static bool betaCurve;
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

            bmiuCurve = false;
            bDarkICurve = false;
            bSNRCurve = false;
            betaCurve = false;  
        }
    }

    public class CCD3TTestResult
    {
        public static double[] T;
        public static double[] miu_I_delta;
        public static double[] miu_I_miu;
        public static double[] L_miu_y;
        public static double[] L_S_y;
        public static double[] L_miu_y_dark;
        public static double[] L_S_y_dark;

        public static double[] DSNU;
        public static double[] FPN;
        public static double[] delta_raed;
        public static double[] delta_mid;
                
        public static bool bDarkICurve;

        public static void Reset()
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
