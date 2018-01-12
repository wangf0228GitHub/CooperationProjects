using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WFNetLib;
using WFOffice2007;


namespace CMOSTestLib
{
    public class TestReport
    {
        public static void MakeReport(string filename)
        {
            WordReport report = new WordReport();
            double d;
            report.CreateNewDocument(System.Windows.Forms.Application.StartupPath + "\\测试报告模板.dot");
            report.InsertValue("报告生成时间", DateTime.Now.ToString("yyyy.MM.dd   HH:mm"));
            report.InsertValue("芯片编号", SystemParam.DeviceID);
            double Temperature;
            double E;
            Temperature = 0;
            for (int j = 0; j < Calc1.TList.Count; j++)
            {
                Temperature += Calc1.TList[j];
            }
            Temperature = Temperature / Calc1.TList.Count;
            report.InsertValue("环境温度", Temperature.ToString("F1"));
            E = 0;
            for (int j = 0; j < Calc1.EList.Count; j++)
            {
                E += Calc1.EList[j];
            }
            E = E / Calc1.EList.Count;
            report.InsertValue("环境光强", E.ToString("F1"));

            report.InsertValue("饱和输出", Calc1.miu_y[Calc1.SaturatedIndex].ToString("F6"));
            report.InsertValue("系统增益", Calc1.OverAllGain_K.ToString("F6"));
            d = (Calc1.AverageDarkSignal_k * 1000) / Calc1.OverAllGain_K;
            report.InsertValue("平均暗电流", d.ToString("F6"));
            report.InsertValue("动态范围", Calc1.DR.ToString("F6"));
            report.InsertValue("量子效率", Calc1.eta.ToString("F6"));
            report.InsertValue("满阱容量", Calc1.FullCapacity.ToString("F6"));
            report.InsertValue("DSUN", Calc2.DSNU1288.ToString("F6"));
            report.InsertValue("PRUN", Calc2.PRNU1288.ToString("F6"));
            report.InsertPicture("光电响应曲线", SystemParam.TempPicPath + "1.JPG", 300, 122);
            report.InsertPicture("平均暗信号", SystemParam.TempPicPath + "2.jpg", 300, 122);
            report.InsertPicture("暗信号均方差", SystemParam.TempPicPath + "3.jpg", 300, 122);
            report.InsertPicture("信噪比", SystemParam.TempPicPath + "4.jpg", 300, 122);


            report.InsertValue("亮点个数", Calc2.LightPoints.Count.ToString());
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Calc2.LightPoints.Count; i++)
            {
                sb.Append(String.Format("[{0,4},{1,4}]", (Calc2.LightPoints[i].col + 1).ToString(), (Calc2.LightPoints[i].row + 1).ToString()));
                if (((i + 1) % 5) == 0)
                {
                    sb.Append("\r\n");
                }
                else
                {
                    sb.Append("\t");
                }
                if (i > 98)
                {
                    report.InsertValue("亮点前100个", "(亮点前100个)");
                    break;
                }
            }
            report.InsertValue("亮点位置", sb.ToString());

            report.InsertValue("暗点个数", Calc2.DarkPoints.Count.ToString());
            sb = new StringBuilder();
            for (int i = 0; i < Calc2.DarkPoints.Count; i++)
            {
                sb.Append(String.Format("[{0,4},{1,4}]", (Calc2.DarkPoints[i].col + 1).ToString(), (Calc2.DarkPoints[i].row + 1).ToString()));
                if (((i + 1) % 5) == 0)
                {
                    sb.Append("\r\n");
                }
                else
                {
                    sb.Append("\t");
                }
                if (i > 98)
                {
                    report.InsertValue("暗点前100个", "(暗点前100个)");
                    break;
                }
            }
            report.InsertValue("暗点位置", sb.ToString());
            report.SaveDocument(filename); 
        }
        public static void RGBMakeReport(string filename)
        {
            WordReport report = new WordReport();
            double d;
            report.CreateNewDocument(System.Windows.Forms.Application.StartupPath + "\\测试报告模板.dot");
            report.InsertValue("报告生成时间", DateTime.Now.ToString("yyyy.MM.dd   HH:mm"));
            report.InsertValue("芯片编号", SystemParam.DeviceID);
            double Temperature;
            double E;
            Temperature = 0;
            for (int j = 0; j < Calc1.TList.Count; j++)
            {
                Temperature += Calc1.TList[j];
            }
            Temperature = Temperature / Calc1.TList.Count;
            report.InsertValue("环境温度", Temperature.ToString("F1"));
            E = 0;
            for (int j = 0; j < Calc1.EList.Count; j++)
            {
                E += Calc1.EList[j];
            }
            E = E / Calc1.EList.Count;
            report.InsertValue("环境光强", E.ToString("F1"));
            StringBuilder sb;

            report.InsertValue("R_饱和输出", Calc1.R_miu_y[Calc1.R_SaturatedIndex].ToString("F6"));
            report.InsertValue("R_系统增益", Calc1.R_OverAllGain_K.ToString("F6"));
            d = (Calc1.R_AverageDarkSignal_k * 1000) / Calc1.R_OverAllGain_K;
            report.InsertValue("R_平均暗电流", d.ToString("F6"));
            report.InsertValue("R_动态范围", Calc1.R_DR.ToString("F6"));
            report.InsertValue("R_量子效率", Calc1.R_eta.ToString("F6"));
            report.InsertValue("R_满阱容量", Calc1.R_FullCapacity.ToString("F6"));
            report.InsertValue("R_DSUN", Calc2.R_DSNU1288.ToString("F6"));
            report.InsertValue("R_PRUN", Calc2.R_PRNU1288.ToString("F6"));
            report.InsertPicture("R_光电响应曲线", SystemParam.TempPicPath + "1_R.JPG", 300, 122);
            report.InsertPicture("R_平均暗信号", SystemParam.TempPicPath + "2_R.jpg", 300, 122);
            report.InsertPicture("R_暗信号均方差", SystemParam.TempPicPath + "3_R.jpg", 300, 122);
            report.InsertPicture("R_信噪比", SystemParam.TempPicPath + "4_R.jpg", 300, 122);

            report.InsertValue("R_亮点个数", Calc2.R_LightPoints.Count.ToString());
            sb = new StringBuilder();
            for (int i = 0; i < Calc2.R_LightPoints.Count; i++)
            {
                sb.Append(String.Format("[{0,4},{1,4}]", (Calc2.R_LightPoints[i].col + 1).ToString(), (Calc2.R_LightPoints[i].row + 1).ToString()));
                if (((i + 1) % 5) == 0)
                {
                    sb.Append("\r\n");
                }
                else
                {
                    sb.Append("\t");
                }
                if (i > 98)
                {
                    report.InsertValue("R_亮点前100个", "(亮点前100个)");
                    break;
                }
            }
            report.InsertValue("R_亮点位置", sb.ToString());

            report.InsertValue("R_暗点个数", Calc2.R_DarkPoints.Count.ToString());
            sb = new StringBuilder();
            for (int i = 0; i < Calc2.R_DarkPoints.Count; i++)
            {
                sb.Append(String.Format("[{0,4},{1,4}]", (Calc2.R_DarkPoints[i].col + 1).ToString(), (Calc2.R_DarkPoints[i].row + 1).ToString()));
                if (((i + 1) % 5) == 0)
                {
                    sb.Append("\r\n");
                }
                else
                {
                    sb.Append("\t");
                }
                if (i > 98)
                {
                    report.InsertValue("R_暗点前100个", "(暗点前100个)");
                    break;
                }
            }
            report.InsertValue("R_暗点位置", sb.ToString());
            
            report.InsertValue("G_饱和输出", Calc1.G_miu_y[Calc1.G_SaturatedIndex].ToString("F6"));
            report.InsertValue("G_系统增益", Calc1.G_OverAllGain_K.ToString("F6"));
            d = (Calc1.G_AverageDarkSignal_k * 1000) / Calc1.G_OverAllGain_K;
            report.InsertValue("G_平均暗电流", d.ToString("F6"));
            report.InsertValue("G_动态范围", Calc1.G_DR.ToString("F6"));
            report.InsertValue("G_量子效率", Calc1.G_eta.ToString("F6"));
            report.InsertValue("G_满阱容量", Calc1.G_FullCapacity.ToString("F6"));
            report.InsertValue("G_DSUN", Calc2.G_DSNU1288.ToString("F6"));
            report.InsertValue("G_PRUN", Calc2.G_PRNU1288.ToString("F6"));
            report.InsertPicture("G_光电响应曲线", SystemParam.TempPicPath + "1_G.JPG", 300, 122);
            report.InsertPicture("G_平均暗信号", SystemParam.TempPicPath + "2_G.jpg", 300, 122);
            report.InsertPicture("G_暗信号均方差", SystemParam.TempPicPath + "3_G.jpg", 300, 122);
            report.InsertPicture("G_信噪比", SystemParam.TempPicPath + "4_G.jpg", 300, 122);

            report.InsertValue("G_亮点个数", Calc2.G_LightPoints.Count.ToString());
            sb = new StringBuilder();
            for (int i = 0; i < Calc2.G_LightPoints.Count; i++)
            {
                sb.Append(String.Format("[{0,4},{1,4}]", (Calc2.G_LightPoints[i].col + 1).ToString(), (Calc2.G_LightPoints[i].row + 1).ToString()));
                if (((i + 1) % 5) == 0)
                {
                    sb.Append("\r\n");
                }
                else
                {
                    sb.Append("\t");
                }
                if (i > 98)
                {
                    report.InsertValue("G_亮点前100个", "(亮点前100个)");
                    break;
                }
            }
            report.InsertValue("G_亮点位置", sb.ToString());

            report.InsertValue("G_暗点个数", Calc2.G_DarkPoints.Count.ToString());
            sb = new StringBuilder();
            for (int i = 0; i < Calc2.G_DarkPoints.Count; i++)
            {
                sb.Append(String.Format("[{0,4},{1,4}]", (Calc2.G_DarkPoints[i].col + 1).ToString(), (Calc2.G_DarkPoints[i].row + 1).ToString()));
                if (((i + 1) % 5) == 0)
                {
                    sb.Append("\r\n");
                }
                else
                {
                    sb.Append("\t");
                }
                if (i > 98)
                {
                    report.InsertValue("G_暗点前100个", "(暗点前100个)");
                    break;
                }
            }
            report.InsertValue("G_暗点位置", sb.ToString());

            report.InsertValue("B_饱和输出", Calc1.B_miu_y[Calc1.B_SaturatedIndex].ToString("F6"));
            report.InsertValue("B_系统增益", Calc1.B_OverAllGain_K.ToString("F6"));
            d = (Calc1.B_AverageDarkSignal_k * 1000) / Calc1.B_OverAllGain_K;
            report.InsertValue("B_平均暗电流", d.ToString("F6"));
            report.InsertValue("B_动态范围", Calc1.B_DR.ToString("F6"));
            report.InsertValue("B_量子效率", Calc1.B_eta.ToString("F6"));
            report.InsertValue("B_满阱容量", Calc1.B_FullCapacity.ToString("F6"));
            report.InsertValue("B_DSUN", Calc2.B_DSNU1288.ToString("F6"));
            report.InsertValue("B_PRUN", Calc2.B_PRNU1288.ToString("F6"));
            report.InsertPicture("B_光电响应曲线", SystemParam.TempPicPath + "1_B.JPG", 300, 122);
            report.InsertPicture("B_平均暗信号", SystemParam.TempPicPath + "2_B.jpg", 300, 122);
            report.InsertPicture("B_暗信号均方差", SystemParam.TempPicPath + "3_B.jpg", 300, 122);
            report.InsertPicture("B_信噪比", SystemParam.TempPicPath + "4_B.jpg", 300, 122);

            report.InsertValue("B_亮点个数", Calc2.B_LightPoints.Count.ToString());
            sb = new StringBuilder();
            for (int i = 0; i < Calc2.B_LightPoints.Count; i++)
            {
                sb.Append(String.Format("[{0,4},{1,4}]", (Calc2.B_LightPoints[i].col + 1).ToString(), (Calc2.B_LightPoints[i].row + 1).ToString()));
                if (((i + 1) % 5) == 0)
                {
                    sb.Append("\r\n");
                }
                else
                {
                    sb.Append("\t");
                }
                if (i > 98)
                {
                    report.InsertValue("B_亮点前100个", "(亮点前100个)");
                    break;
                }
            }
            report.InsertValue("B_亮点位置", sb.ToString());

            report.InsertValue("B_暗点个数", Calc2.B_DarkPoints.Count.ToString());
            sb = new StringBuilder();
            for (int i = 0; i < Calc2.B_DarkPoints.Count; i++)
            {
                sb.Append(String.Format("[{0,4},{1,4}]", (Calc2.B_DarkPoints[i].col + 1).ToString(), (Calc2.B_DarkPoints[i].row + 1).ToString()));
                if (((i + 1) % 5) == 0)
                {
                    sb.Append("\r\n");
                }
                else
                {
                    sb.Append("\t");
                }
                if (i > 98)
                {
                    report.InsertValue("B_暗点前100个", "(暗点前100个)");
                    break;
                }
            }
            report.InsertValue("B_暗点位置", sb.ToString());
            report.SaveDocument(filename);
        }
    }
}
