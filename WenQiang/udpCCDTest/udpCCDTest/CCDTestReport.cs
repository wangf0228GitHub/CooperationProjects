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




            if (double.IsNaN(CCDParamTestResult.eta))
                report.InsertValue("量子效率", "未测试");
            else
                report.InsertPicture("光电响应曲线", SystemParam.TempPicPath + "1.JPG", 300, 122);
            if (double.IsNaN(CCDParamTestResult.eta))
                report.InsertValue("量子效率", "未测试");
            else
                report.InsertPicture("平均暗信号", SystemParam.TempPicPath + "2.jpg", 300, 122);
            if (double.IsNaN(CCDParamTestResult.eta))
                report.InsertValue("量子效率", "未测试");
            else
                report.InsertPicture("暗信号均方差", SystemParam.TempPicPath + "3.jpg", 300, 122);
            if (double.IsNaN(CCDParamTestResult.eta))
                report.InsertValue("量子效率", "未测试");
            else
                report.InsertPicture("信噪比", SystemParam.TempPicPath + "4.jpg", 300, 122);

            report.SaveDocument(filename);
        }        
    }
}
