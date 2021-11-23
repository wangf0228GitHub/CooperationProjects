using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WFOffice;

namespace udpCCDTest
{
    public class TestReport
    {
        public static void MakeReport(string filename)
        {
            WordReport report = new WordReport();
            report.CreateNewDocument(System.Windows.Forms.Application.StartupPath + "\\测试报告模板.dot");
            report.InsertValue("报告生成时间", DateTime.Now.ToString("yyyy.MM.dd   HH:mm"));
            report.InsertValue("芯片编号", SystemParam.DeviceID);
            
            report.InsertValue("饱和输出", SystemParam.miu_sat.ToString("F6"));
            report.InsertValue("系统增益", CCDParamTestResult.K.ToString("F6"));
            if (double.IsNaN(CCDParamTestResult.eta))
                report.InsertValue("量子效率", "未测试");
            else
                report.InsertValue("量子效率", CCDParamTestResult.eta.ToString("F6"));

            if (double.IsNaN(CCDParamTestResult.DR))
                report.InsertValue("动态范围", "未测试");
            else
                report.InsertValue("动态范围", CCDParamTestResult.DR.ToString("F6"));

            if (double.IsNaN(CCDParamTestResult.FWC))
                report.InsertValue("满阱容量", "未测试");
            else
                report.InsertValue("满阱容量", CCDParamTestResult.FWC.ToString("F6"));

            if (double.IsNaN(CCDParamTestResult.miu_I_miu))
                report.InsertValue("暗电流1", "未测试");
            else
                report.InsertValue("暗电流1", CCDParamTestResult.miu_I_miu.ToString("F6"));
            if (double.IsNaN(CCDParamTestResult.miu_I_delta))
                report.InsertValue("暗电流2", "未测试");
            else
                report.InsertValue("暗电流2", CCDParamTestResult.miu_I_delta.ToString("F6"));
            if (double.IsNaN(CCDParamTestResult.LE))
                report.InsertValue("线性误差LE", "未测试");
            else
                report.InsertValue("线性误差LE", CCDParamTestResult.LE.ToString("F6"));

            if (double.IsNaN(CCDParamTestResult.DSNU))
                report.InsertValue("DSUN", "未测试");
            else
                report.InsertValue("DSUN", CCDParamTestResult.DSNU.ToString("F6"));
            if (double.IsNaN(CCDParamTestResult.PRNU))
                report.InsertValue("PRUN", "未测试");
            else
                report.InsertValue("PRUN", CCDParamTestResult.PRNU.ToString("F6"));
            if (double.IsNaN(CCDParamTestResult.FPN))
                report.InsertValue("FPN", "未测试");
            else
                report.InsertValue("FPN", CCDParamTestResult.FPN.ToString("F6"));
            if (double.IsNaN(CCDParamTestResult.delta_raed))
                report.InsertValue("读出噪声", "未测试");
            else
                report.InsertValue("读出噪声", CCDParamTestResult.delta_raed.ToString("F6"));


            int curveIndex = 1;

            if (CCDParamTestResult.bmiuCurve)
            {
                report.InsertPicture("图"+curveIndex.ToString(), SystemParam.TempPicPath + "K_1.jpg", 300, 122);
                report.InsertValue("图" + curveIndex.ToString()+"图题", "明场测试曲线");
                curveIndex++;
            }

            if (CCDParamTestResult.betaCurve)
            {
                report.InsertPicture("图" + curveIndex.ToString(), SystemParam.TempPicPath + "etaCurve.jpg", 300, 122);
                report.InsertValue("图" + curveIndex.ToString() + "图题", "量子效率曲线");
                curveIndex++;
            }

            if (CCDParamTestResult.bSNRCurve)
            {
                report.InsertPicture("图" + curveIndex.ToString(), SystemParam.TempPicPath + "SNR.jpg", 300, 122);
                report.InsertValue("图" + curveIndex.ToString() + "图题", "信噪比SNR曲线");
                curveIndex++;
            }
            if (CCDParamTestResult.bDarkICurve)
            {
                report.InsertPicture("图" + curveIndex.ToString(), SystemParam.TempPicPath + "DarkI1.jpg", 300, 122);
                report.InsertValue("图" + curveIndex.ToString() + "图题", "暗信号均值曲线");
                curveIndex++;

                report.InsertPicture("图" + curveIndex.ToString(), SystemParam.TempPicPath + "DarkI2.jpg", 300, 122);
                report.InsertValue("图" + curveIndex.ToString() + "图题", "暗信号方差曲线");
                curveIndex++;
            }
            report.SaveDocument(filename);
        }
        public static void MakeReport3T(string filename)
        {
            WordReport report = new WordReport();
            report.CreateNewDocument(System.Windows.Forms.Application.StartupPath + "\\3T测试报告模板.dot");
            report.InsertValue("报告生成时间", DateTime.Now.ToString("yyyy.MM.dd   HH:mm"));
            report.InsertValue("芯片编号", SystemParam.DeviceID);
            report.InsertValue("系统增益", CCDParamTestResult.K.ToString("F6"));
            for(int i=0;i<3;i++)
            {
                if(CCD3TTestResult.delta_mid==null)
                {
                    report.InsertValue("暗电流1_T"+(i+1).ToString(), "未测试");
                    report.InsertValue("暗电流2_T" + (i + 1).ToString(), "未测试");
                }
                else
                {
                    report.InsertValue("暗电流1_T" + (i + 1).ToString(), CCD3TTestResult.miu_I_miu[i].ToString("F6"));
                    report.InsertValue("暗电流2_T" + (i + 1).ToString(), CCD3TTestResult.miu_I_delta[i].ToString("F6"));
                }

                if(CCD3TTestResult.DSNU==null)
                {
                    report.InsertValue("DSNU_T" + (i + 1).ToString(), "未测试");
                }
                else
                {
                    report.InsertValue("DSNU_T" + (i + 1).ToString(), CCD3TTestResult.DSNU[i].ToString("F6"));
                }

                if (CCD3TTestResult.FPN == null)
                {
                    report.InsertValue("FPN_T" + (i + 1).ToString(), "未测试");
                }
                else
                {
                    report.InsertValue("FPN_T" + (i + 1).ToString(), CCD3TTestResult.DSNU[i].ToString("F6"));
                }

                if (CCD3TTestResult.delta_raed == null)
                {
                    report.InsertValue("读出噪声_T" + (i + 1).ToString(), "未测试");
                }
                else
                {
                    report.InsertValue("读出噪声_T" + (i + 1).ToString(), CCD3TTestResult.DSNU[i].ToString("F6"));
                }

                if (CCDParamTestResult.bDarkICurve)
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
