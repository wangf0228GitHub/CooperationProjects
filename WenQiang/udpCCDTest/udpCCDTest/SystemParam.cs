using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WFNetLib;

namespace udpCCDTest
{
    public class SystemParam
    {
        public static void InitSystemParam()
        {
            iniFileOP.iniFilePath = System.Windows.Forms.Application.StartupPath + "\\Config.ini";
            SystemParam.Ts = 6.25/1000000;
            SystemParam.CCDModel = iniFileOP.Read("CCD Param", "CCDModel");
            SystemParam.DeviceID = iniFileOP.Read("CCD Param", "DeviceID");
            SystemParam.CCD_Sa = int.Parse(iniFileOP.Read("CCD Param", "CCD_Sa"));
            SystemParam.CCD_Sb = int.Parse(iniFileOP.Read("CCD Param", "CCD_Sb"));
            SystemParam.CCD_M = int.Parse(iniFileOP.Read("CCD Param", "CCD_M"));
            SystemParam.CCD_N = int.Parse(iniFileOP.Read("CCD Param", "CCD_N"));
            SystemParam.CCD_Pa = int.Parse(iniFileOP.Read("CCD Param", "CCD_Pa"));
            SystemParam.CCD_Pb = int.Parse(iniFileOP.Read("CCD Param", "CCD_Pb"));
            SystemParam.CCD_Sch = int.Parse(iniFileOP.Read("CCD Param", "CCD_Sch"));
            SystemParam.CCD_ADL = int.Parse(iniFileOP.Read("CCD Param", "CCD_ADL"));
            SystemParam.CCD_phi = int.Parse(iniFileOP.Read("CCD Param", "CCD_phi"));
            SystemParam.CCD_PGA = int.Parse(iniFileOP.Read("CCD Param", "CCD_PGA"));

            SystemParam.NTmin = int.Parse(iniFileOP.Read("Collect Param", "NTmin"));
            SystemParam.n = int.Parse(iniFileOP.Read("Collect Param", "n"));
            SystemParam.L = int.Parse(iniFileOP.Read("Collect Param", "L"));
            SystemParam.L_BTM = int.Parse(iniFileOP.Read("Collect Param", "L_BTM"));
            SystemParam.L_TOP = int.Parse(iniFileOP.Read("Collect Param", "L_TOP"));
            SystemParam.NTexp = int.Parse(iniFileOP.Read("Collect Param", "NTexp"));
            SystemParam.NTdark = int.Parse(iniFileOP.Read("Collect Param", "NTdark"));
            SystemParam.delta_Tdark = int.Parse(iniFileOP.Read("Collect Param", "delta_Tdark"));

            SystemParam.Oe = int.Parse(iniFileOP.Read("Light Param", "Oe"));
            SystemParam.lambda_Oe = int.Parse(iniFileOP.Read("Light Param", "lambda_Oe"));
            SystemParam.Np = int.Parse(iniFileOP.Read("Light Param", "Np"));
            SystemParam.delta_lambda = double.Parse(iniFileOP.Read("Light Param", "delta_lambda"));
            SystemParam.L_lambda = double.Parse(iniFileOP.Read("Light Param", "L_lambda"));
            SystemParam.H_lambda = double.Parse(iniFileOP.Read("Light Param", "H_lambda"));
        }
        public static void SaveSystemParam()
        {
            iniFileOP.Write("CCD Param", "CCDModel", SystemParam.CCDModel);
            iniFileOP.Write("CCD Param", "DeviceID",SystemParam.DeviceID);
            iniFileOP.Write("CCD Param", "CCD_Sa",SystemParam.CCD_Sa.ToString());
            iniFileOP.Write("CCD Param", "CCD_Sb",SystemParam.CCD_Sb.ToString());
            iniFileOP.Write("CCD Param", "CCD_M",SystemParam.CCD_M.ToString());
            iniFileOP.Write("CCD Param", "CCD_N",SystemParam.CCD_N.ToString());
            iniFileOP.Write("CCD Param", "CCD_Pa",SystemParam.CCD_Pa.ToString());
            iniFileOP.Write("CCD Param", "CCD_Pb",SystemParam.CCD_Pb.ToString());
            iniFileOP.Write("CCD Param", "CCD_Sch",SystemParam.CCD_Sch.ToString());
            iniFileOP.Write("CCD Param", "CCD_ADL", SystemParam.CCD_ADL.ToString());
            iniFileOP.Write("CCD Param", "CCD_phi",SystemParam.CCD_phi.ToString());
            iniFileOP.Write("CCD Param", "CCD_PGA",SystemParam.CCD_PGA.ToString());

            iniFileOP.Write("Collect Param", "NTmin",SystemParam.NTmin.ToString());
            iniFileOP.Write("Collect Param", "n",SystemParam.n.ToString());
            iniFileOP.Write("Collect Param", "L",SystemParam.L.ToString());
            iniFileOP.Write("Collect Param", "L_BTM",SystemParam.L_BTM.ToString());
            iniFileOP.Write("Collect Param", "L_TOP",SystemParam.L_TOP.ToString());
            iniFileOP.Write("Collect Param", "NTexp",SystemParam.NTexp.ToString());
            iniFileOP.Write("Collect Param", "NTdark",SystemParam.NTdark.ToString());
            iniFileOP.Write("Collect Param", "delta_Tdark",SystemParam.delta_Tdark.ToString());

            iniFileOP.Write("Light Param", "Oe",SystemParam.Oe.ToString());
            iniFileOP.Write("Light Param", "lambda_Oe",SystemParam.lambda_Oe.ToString());
            iniFileOP.Write("Light Param", "Np",SystemParam.Np.ToString());
            iniFileOP.Write("Light Param", "delta_lambda",SystemParam.delta_lambda.ToString("F2"));
            iniFileOP.Write("Light Param", "L_lambda",SystemParam.L_lambda.ToString("F1"));
            iniFileOP.Write("Light Param", "H_lambda",SystemParam.H_lambda.ToString("F1"));
        }
        public static double Ts;//一个曝光周期的时间大小,ns为单位

        public static string CCDModel;
        public static string DeviceID;
        public static int CCD_Sa;
        public static int CCD_Sb;
        public static int CCD_M;
        public static int CCD_N;
        public static int CCD_Pa;
        public static int CCD_Pb;
        public static int CCD_Sch;
        public static int CCD_ADL;
        public static int CCD_phi;//8、 图像传感器工作频率
        public static int CCD_PGA;

        public static int NTmin;//10、	最小曝光周期数
        public static int n;//11、	时域测试采样深度
        public static int L;//12、	空域测试采样深度
        public static int L_BTM;//13、	线性区下边界
        public static int L_TOP;//14、	线性区上边界
        public static int NTexp;//15、	曝光步距周期数
        public static int NTdark;//21、	测试暗电流曝光起点周期数
        public static int delta_Tdark;//22、	测试暗电流曝光递增步距周期数


        public static int Oe;//16、 明场光照度
        public static int lambda_Oe;//17、	明场测试光波长
        public static int Np;//18、	固定曝光时间系数
        public static double delta_lambda;//19、	量子效率曲线波长步进单位
        public static double L_lambda;//20、	光谱区间
        public static double H_lambda;//20、	光谱区间
    }
}
