using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WFNetLib;

namespace udpCCDTest
{
    public class SystemParam
    {
        public static string TempPicPath = System.Windows.Forms.Application.StartupPath + "\\TempPic\\";
        public static string TxtDataPath = System.Windows.Forms.Application.StartupPath + "\\TxtData\\";
        public static string L_LightTempFilePath = System.Windows.Forms.Application.StartupPath + "\\TempData\\L_LightTemp.bin";
        public static string L_DarkTempFilePath = System.Windows.Forms.Application.StartupPath + "\\TempData\\L_DarkTemp.bin";
        public static string ccdParamFilePath = System.Windows.Forms.Application.StartupPath + "\\ccdParamFile\\";
        public static void InitSystemParam()
        {
            iniFileOP.iniFilePath = System.Windows.Forms.Application.StartupPath + "\\Config.ini";            
            SystemParam.CCDModel = iniFileOP.Read("CCD Param", "CCDModel");
            SystemParam.CCD_Sa = double.Parse(iniFileOP.Read("CCD Param", "CCD_Sa"));
            SystemParam.CCD_Sb = double.Parse(iniFileOP.Read("CCD Param", "CCD_Sb"));
            SystemParam.CCD_M = int.Parse(iniFileOP.Read("CCD Param", "CCD_M"));
            SystemParam.CCD_N = int.Parse(iniFileOP.Read("CCD Param", "CCD_N"));
            SystemParam.CCD_Pa = int.Parse(iniFileOP.Read("CCD Param", "CCD_Pa"));
            SystemParam.CCD_Pb = int.Parse(iniFileOP.Read("CCD Param", "CCD_Pb"));
            SystemParam.CCD_Sch = int.Parse(iniFileOP.Read("CCD Param", "CCD_Sch"));
            SystemParam.CCD_ADL = int.Parse(iniFileOP.Read("CCD Param", "CCD_ADL"));
            SystemParam.CCD_phi = int.Parse(iniFileOP.Read("CCD Param", "CCD_phi"));
            SystemParam.CCD_PGA = int.Parse(iniFileOP.Read("CCD Param", "CCD_PGA"));

            SystemParam.NTmin2 = uint.Parse(iniFileOP.Read("Collect Param", "NTmin2"));
            SystemParam.NTmin1 = uint.Parse(iniFileOP.Read("Collect Param", "NTmin1"));
            SystemParam.n = int.Parse(iniFileOP.Read("Collect Param", "n"));
            SystemParam.L = int.Parse(iniFileOP.Read("Collect Param", "L"));
            SystemParam.L_BTM = int.Parse(iniFileOP.Read("Collect Param", "L_BTM"));
            SystemParam.L_TOP = int.Parse(iniFileOP.Read("Collect Param", "L_TOP"));
            SystemParam.NTexp2 = uint.Parse(iniFileOP.Read("Collect Param", "NTexp2"));
            SystemParam.NTdark = uint.Parse(iniFileOP.Read("Collect Param", "NTdark"));
            SystemParam.delta_Tdark = uint.Parse(iniFileOP.Read("Collect Param", "delta_Tdark"));

            SystemParam.OeStep = double.Parse(iniFileOP.Read("Light Param", "OeStep"));
            SystemParam.OeLight = double.Parse(iniFileOP.Read("Light Param", "OeLight"));
            SystemParam.lambda_Oe = int.Parse(iniFileOP.Read("Light Param", "lambda_Oe"));
            SystemParam.Np = double.Parse(iniFileOP.Read("Light Param", "Np"));
            SystemParam.delta_lambda = double.Parse(iniFileOP.Read("Light Param", "delta_lambda"));
            SystemParam.L_lambda = double.Parse(iniFileOP.Read("Light Param", "L_lambda"));
            SystemParam.H_lambda = double.Parse(iniFileOP.Read("Light Param", "H_lambda"));

            string strL2E_lambda = iniFileOP.Read("Light Param", "L2E_lambda");
            string[] lambdaList = strL2E_lambda.Split(',');
            tcpCCS.lambdaList = new int[lambdaList.Length];
            for (int i = 0; i < tcpCCS.lambdaList.Length; i++)
            {
                tcpCCS.lambdaList[i] = int.Parse(lambdaList[i]);
            }

            string strL2E_Max_nit = iniFileOP.Read("Light Param", "L2E_Max_nit");
            string[] Max_nit = strL2E_Max_nit.Split(',');
            tcpCCS.Max_nit = new double[Max_nit.Length];
            for (int i = 0; i < Max_nit.Length; i++)
            {
                tcpCCS.Max_nit[i] = double.Parse(Max_nit[i]);
            }

            string strL2E_a2 = iniFileOP.Read("Light Param", "L2E_a2");
            string strL2E_a1 = iniFileOP.Read("Light Param", "L2E_a1");
            string strL2E_a0 = iniFileOP.Read("Light Param", "L2E_a0");
            string[] a2List = strL2E_a2.Split(',');
            string[] a1List = strL2E_a1.Split(',');
            string[] a0List = strL2E_a0.Split(',');
            tcpCCS.L2E_a2 = new double[tcpCCS.lambdaList.Length];
            tcpCCS.L2E_a1 = new double[tcpCCS.lambdaList.Length];
            tcpCCS.L2E_a0 = new double[tcpCCS.lambdaList.Length];
            for(int i=0;i<tcpCCS.lambdaList.Length;i++)
            {
                //try
                //{
                tcpCCS.L2E_a2[i] = double.Parse(a2List[i]);
                tcpCCS.L2E_a1[i] = double.Parse(a1List[i]);
                tcpCCS.L2E_a0[i] = double.Parse(a0List[i]);
                //}
                //catch
                //{
                //    tcpCCS.L2E_a2[i] = 0;
                //    tcpCCS.L2E_a1[i] = 0;
                //    tcpCCS.L2E_a0[i] = 0;
                //}
            }


            
        }
        public static void SaveSystemParam()
        {
            iniFileOP.Write("CCD Param", "CCDModel", SystemParam.CCDModel);
            //iniFileOP.Write("CCD Param", "DeviceID",SystemParam.DeviceID);
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

            iniFileOP.Write("Collect Param", "NTmin2",SystemParam.NTmin2.ToString());
            iniFileOP.Write("Collect Param", "NTmin1", SystemParam.NTmin1.ToString());
            iniFileOP.Write("Collect Param", "n",SystemParam.n.ToString());
            iniFileOP.Write("Collect Param", "L",SystemParam.L.ToString());
            iniFileOP.Write("Collect Param", "L_BTM",SystemParam.L_BTM.ToString());
            iniFileOP.Write("Collect Param", "L_TOP",SystemParam.L_TOP.ToString());
            iniFileOP.Write("Collect Param", "NTexp2",SystemParam.NTexp2.ToString());
            iniFileOP.Write("Collect Param", "NTdark",SystemParam.NTdark.ToString());
            iniFileOP.Write("Collect Param", "delta_Tdark",SystemParam.delta_Tdark.ToString());

            iniFileOP.Write("Light Param", "OeStep",SystemParam.OeStep.ToString());
            iniFileOP.Write("Light Param", "OeLight", SystemParam.OeLight.ToString());
            //iniFileOP.Write("Light Param", "lambda_Oe",SystemParam.lambda_Oe.ToString());
            iniFileOP.Write("Light Param", "Np",SystemParam.Np.ToString("F2"));
            iniFileOP.Write("Light Param", "delta_lambda",SystemParam.delta_lambda.ToString("F2"));
            iniFileOP.Write("Light Param", "L_lambda",SystemParam.L_lambda.ToString("F1"));
            iniFileOP.Write("Light Param", "H_lambda",SystemParam.H_lambda.ToString("F1"));
        }
        public static string strCCDINIPath;
        public static string CCDModel;
        public static string DeviceID
        {
            get
            {
                return iniFileOP.Read("CCD Param", "DeviceID");
            }
            set
            {
                iniFileOP.Write("CCD Param", "DeviceID", value);
                strCCDINIPath = ccdParamFilePath + value + "_" + lambda_Oe.ToString() + ".ini";
            }
        }
        public static double CCD_Sa;
        public static double CCD_Sb;
        public static int CCD_M;//图像传感器像素 行 数量 ，暂时固定6032
        public static int CCD_N;//图像传感器像素 列 数量 ，暂时固定8640
        public static int CCD_Pa;
        public static int CCD_Pb;
        public static int CCD_Sch;
        public static int CCD_ADL;//图像传感器模数转换宽度
        public static int CCD_phi;//8、 图像传感器工作频率
        public static int CCD_PGA;

        public static uint NTmin1;//10、	第1种方式的曝光周期数
        public static uint NTmin2;//10、	第1种方式的最小曝光周期数
        public static int n;//11、	时域测试采样深度
        public static int L;//12、	空域测试采样深度
        public static int L_BTM;//13、	线性区下边界
        public static int L_TOP;//14、	线性区上边界
        public static uint NTexp2;//15、	第2种方式的曝光步距周期数
        public static uint NTdark;//21、	测试暗电流曝光起点周期数
        public static uint delta_Tdark;//22、	测试暗电流曝光递增步距周期数


        public static double OeStep;//16、 第1种方式的光源亮度步距，单位为亮度
        public static double OeLight;//16、 第2种方式的光源亮度，单位为亮度

        public static double Osat;//饱和光照度
        public static double miu_sat;//饱和均值
        public static int lambda_Oe//17、	明场测试光波长
        {
            get
            {
                return int.Parse(iniFileOP.Read("Light Param", "lambda_Oe"));
            }
            set
            {
                iniFileOP.Write("Light Param", "lambda_Oe", value.ToString());
                strCCDINIPath = ccdParamFilePath + DeviceID + "_" + value.ToString() + ".ini";
            }
        }
        public static double Np;//18、	固定曝光时间系数
        public static double delta_lambda;//19、	量子效率曲线波长步进单位
        public static double L_lambda;//20、	光谱区间
        public static double H_lambda;//20、	光谱区间

        public static double Get_phi()
        {
            return 20000000;
//             double phi = 0;
//             switch (CCD_phi)
//             {
//                 case 0:
//                     phi = 30 * 1000000;
//                     break;
//                 case 1:
//                     phi = 40 * 1000000;
//                     break;
//                 case 2:
//                     phi = 50 * 1000000;
//                     break;
//                 case 3:
//                     phi = 60 * 1000000;
//                     break;
//                 case 4:
//                     phi = 72 * 1000000;
//                     break;
//                 case 5:
//                     phi = 75 * 1000000;
//                     break;
//                 case 6:
//                     phi = 80 * 1000000;
//                     break;
//             }
//             return phi;
        }
        public static int GetADL()
        {
            int ad = 8;
            switch (SystemParam.CCD_ADL)//图像传感器模数转换宽度
            {
                case 0:
                    ad = 8;
                    break;
                case 1:
                    ad = 10;
                    break;
                case 2:
                    ad = 12;
                    break;
                case 3:
                    ad = 16;
                    break;
            }
            return ad;
        }
        public static double GetADMax()
        {
            int ad = 8;
            switch (SystemParam.CCD_ADL)//图像传感器模数转换宽度
            {
                case 0:
                    ad = 8;
                    break;
                case 1:
                    ad = 10;
                    break;
                case 2:
                    ad = 12;
                    break;
                case 3:
                    ad = 16;
                    break;
            }
            return Math.Pow(2,ad);
        }

        public static double GetPGA()
        {
            double pga= 3.5;
            switch (SystemParam.CCD_PGA + 0x0b)
            {
                case 0x0b:
                    pga = 3.5;
                    break;
                case 0x0c:
                    pga = 3.75;
                    break;
                case 0x0d:
                    pga = 4;
                    break;
                case 0x0e:
                    pga = 4.25;
                    break;
                case 0x0f:
                    pga = 4.5;
                    break;
                case 0x10:
                    pga = 4.75;
                    break;
                case 0x11:
                    pga = 5;
                    break;
            }
            return pga;
        }
        public static double GetTime1()
        {            
            return 1000 * ((NTmin1) / Get_phi());
        }

        public static double GetTime2(int i,uint ntexp2)
        {
            return 1000 * ((NTmin2 + i * ntexp2) / Get_phi());
        }

        public static double GetTime(uint tex)
        {
            return 1000  * (tex / Get_phi());
        }
    }
}
