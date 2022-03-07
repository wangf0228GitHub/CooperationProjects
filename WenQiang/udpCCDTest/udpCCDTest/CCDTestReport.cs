using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WFOffice;

namespace udpCCDTest
{
    public partial class FormMain : Form
    {
        public void MakeReport(string filename)
        {
            WordReport report = new WordReport();
            report.CreateNewDocument(System.Windows.Forms.Application.StartupPath + "\\测试报告模板.dot");
            if(ExType==1)
                report.InsertValue("报告类型", "1");
            else
                report.InsertValue("报告类型", "2");
            report.InsertValue("报告生成时间", DateTime.Now.ToString("yyyy.MM.dd   HH:mm"));
            report.InsertValue("芯片编号", SystemParam.DeviceID);
            report.InsertValue("器件型号", SystemParam.CCDModel);
            report.InsertValue("lambda", SystemParam.lambda_Oe.ToString());
            report.InsertValue("PGA", SystemParam.GetPGA().ToString("F2"));
            int phi=0;
            switch (SystemParam.CCD_phi)
            {
                case 0:
                    phi = 30;
                    break;
                case 1:
                    phi = 40;
                    break;
                case 2:
                    phi = 50;
                    break;
                case 3:
                    phi = 60;
                    break;
                case 4:
                    phi = 72;
                    break;
                case 5:
                    phi = 75;
                    break;
                case 6:
                    phi = 80;
                    break;
            }
            report.InsertValue("phi", phi.ToString());
            report.InsertValue("饱和输出", ccdParamTestResult.miu_sat.ToString("F2"));
            report.InsertValue("系统增益", ccdParamTestResult.K.ToString("F3"));
            if (double.IsNaN(ccdParamTestResult.eta))
                report.InsertValue("量子效率", "未测试");
            else
                report.InsertValue("量子效率", ccdParamTestResult.eta.ToString("F3"));

            if (double.IsNaN(ccdParamTestResult.SNRMax))
                report.InsertValue("SNRMax", "未测试");
            else
                report.InsertValue("SNRMax", ccdParamTestResult.SNRMax.ToString("F2"));

            if (double.IsNaN(ccdParamTestResult.DR))
                report.InsertValue("动态范围", "未测试");
            else
                report.InsertValue("动态范围", ccdParamTestResult.DR.ToString("F2"));

            if (double.IsNaN(ccdParamTestResult.FWC))
                report.InsertValue("满阱容量", "未测试");
            else
                report.InsertValue("满阱容量", ccdParamTestResult.FWC.ToString("F2"));

            if (double.IsNaN(ccdParamTestResult.miu_I_miu))
                report.InsertValue("暗电流1", "未测试");
            else
                report.InsertValue("暗电流1", ccdParamTestResult.miu_I_miu.ToString("F2"));
            if (double.IsNaN(ccdParamTestResult.miu_I_delta))
                report.InsertValue("暗电流2", "未测试");
            else
                report.InsertValue("暗电流2", ccdParamTestResult.miu_I_delta.ToString("F2"));
            if (double.IsNaN(ccdParamTestResult.LE))
                report.InsertValue("线性误差LE", "未测试");
            else
                report.InsertValue("线性误差LE", ccdParamTestResult.LE.ToString("F2"));

            if (double.IsNaN(ccdParamTestResult.DSNU))
                report.InsertValue("DSUN", "未测试");
            else
                report.InsertValue("DSUN", ccdParamTestResult.DSNU.ToString("F2"));
            if (double.IsNaN(ccdParamTestResult.PRNU))
                report.InsertValue("PRUN", "未测试");
            else
                report.InsertValue("PRUN", ccdParamTestResult.PRNU.ToString("F6"));
            if (double.IsNaN(ccdParamTestResult.FPN))
                report.InsertValue("FPN", "未测试");
            else
                report.InsertValue("FPN", ccdParamTestResult.FPN.ToString("F2"));
            if (double.IsNaN(ccdParamTestResult.delta_raed))
                report.InsertValue("读出噪声", "未测试");
            else
                report.InsertValue("读出噪声", ccdParamTestResult.delta_raed.ToString("F2"));


            int curveIndex = 1;

            if (ccdParamTestResult.bmiuCurve)
            {
                report.InsertPicture("图"+curveIndex.ToString(), SystemParam.TempPicPath + "K_1.jpg", 300, 122);
                report.InsertValue("图" + curveIndex.ToString()+"图题", "明场测试曲线");
                curveIndex++;
            }

            if (ccdParamTestResult.betaCurve)
            {
                report.InsertPicture("图" + curveIndex.ToString(), SystemParam.TempPicPath + "etaCurve.jpg", 300, 122);
                report.InsertValue("图" + curveIndex.ToString() + "图题", "量子效率曲线");
                curveIndex++;
            }

            if (ccdParamTestResult.bSNRCurve)
            {
                report.InsertPicture("图" + curveIndex.ToString(), SystemParam.TempPicPath + "SNR.jpg", 300, 122);
                report.InsertValue("图" + curveIndex.ToString() + "图题", "信噪比SNR曲线");
                curveIndex++;
            }
            if (ccdParamTestResult.bDarkICurve)
            {
                report.InsertPicture("图" + curveIndex.ToString(), SystemParam.TempPicPath + "DarkI1.jpg", 300, 122);
                report.InsertValue("图" + curveIndex.ToString() + "图题", "暗信号均值曲线");
                curveIndex++;

                report.InsertPicture("图" + curveIndex.ToString(), SystemParam.TempPicPath + "DarkI2.jpg", 300, 122);
                report.InsertValue("图" + curveIndex.ToString() + "图题", "暗信号方差曲线");
                curveIndex++;
            }

            if (ccdParamTestResult.bPGACurve)
            {
                report.InsertPicture("图" + curveIndex.ToString(), SystemParam.TempPicPath + "PGA.jpg", 300, 122);
                report.InsertValue("图" + curveIndex.ToString() + "图题", "增益曲线");
                curveIndex++;
            }
            report.SaveDocument(filename);
        }
        public void MakeReport3T(string filename)
        {
            WordReport report = new WordReport();
            report.CreateNewDocument(System.Windows.Forms.Application.StartupPath + "\\3T测试报告模板.dot");
            if (ExType == 1)
                report.InsertValue("报告类型", "1");
            else
                report.InsertValue("报告类型", "2");
            report.InsertValue("报告生成时间", DateTime.Now.ToString("yyyy.MM.dd   HH:mm"));
            report.InsertValue("芯片编号", SystemParam.DeviceID);
            report.InsertValue("器件型号", SystemParam.CCDModel);
            report.InsertValue("系统增益", ccdParamTestResult.K.ToString("F2"));
            report.InsertValue("lambda", SystemParam.lambda_Oe.ToString());
            report.InsertValue("PGA", SystemParam.GetPGA().ToString("F2"));
            int phi = 0;
            switch (SystemParam.CCD_phi)
            {
                case 0:
                    phi = 30;
                    break;
                case 1:
                    phi = 40;
                    break;
                case 2:
                    phi = 50;
                    break;
                case 3:
                    phi = 60;
                    break;
                case 4:
                    phi = 72;
                    break;
                case 5:
                    phi = 75;
                    break;
                case 6:
                    phi = 80;
                    break;
            }
            report.InsertValue("phi", phi.ToString());
            for (int i=0;i<3;i++)
            {
                report.InsertValue("T" + (i + 1).ToString(), ccd3TTestResult.T[i].ToString("F1"));
                if (ccd3TTestResult.miu_I_miu == null)
                {
                    report.InsertValue("暗电流1_T"+(i+1).ToString(), "未测试");
                    report.InsertValue("暗电流2_T" + (i + 1).ToString(), "未测试");
                }
                else
                {
                    report.InsertValue("暗电流1_T" + (i + 1).ToString(), ccd3TTestResult.miu_I_miu[i].ToString("F2"));
                    report.InsertValue("暗电流2_T" + (i + 1).ToString(), ccd3TTestResult.miu_I_delta[i].ToString("F2"));
                }

                if(ccd3TTestResult.DSNU==null)
                {
                    report.InsertValue("DSUN_T" + (i + 1).ToString(), "未测试");
                }
                else
                {
                    report.InsertValue("DSUN_T" + (i + 1).ToString(), ccd3TTestResult.DSNU[i].ToString("F2"));
                }

                if (ccd3TTestResult.FPN == null)
                {
                    report.InsertValue("FPN_T" + (i + 1).ToString(), "未测试");
                }
                else
                {
                    report.InsertValue("FPN_T" + (i + 1).ToString(), ccd3TTestResult.FPN[i].ToString("F2"));
                }

                if (ccd3TTestResult.delta_raed == null)
                {
                    report.InsertValue("读出噪声_T" + (i + 1).ToString(), "未测试");
                }
                else
                {
                    report.InsertValue("读出噪声_T" + (i + 1).ToString(), ccd3TTestResult.delta_raed[i].ToString("F2"));
                }

                if (ccdParamTestResult.bDarkICurve)
                {
                    report.InsertPicture("图1_T" + (i+1).ToString(), SystemParam.TempPicPath + "DarkI1_T" + (i + 1).ToString() + ".jpg", 300, 122);
                    report.InsertValue("图1图题_T" + (i + 1).ToString(), "暗信号均值曲线");

                    report.InsertPicture("图2_T" + (i + 1).ToString(), SystemParam.TempPicPath + "DarkI2_T" + (i + 1).ToString() + ".jpg", 300, 122);
                    report.InsertValue("图2图题_T" + (i + 1).ToString(), "暗信号均值曲线");
                }
            }            
            report.SaveDocument(filename);
        }
    }
}
