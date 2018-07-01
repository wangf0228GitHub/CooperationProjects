using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WFNetLib;
using CMOSTestLib.SaperaGUI;
using System.Diagnostics;

namespace CMOSTestLib
{
    public class Calc1
    {
        public static string TempPicPath_Light = System.Windows.Forms.Application.StartupPath + "\\Temp1L\\";
        public static string TempPicPath_Dark = System.Windows.Forms.Application.StartupPath + "\\Temp1D\\";
        public static double percent_base;//饱和点在70%±5%定义为曝光适度
        public static double percent;
        public static int k1;//欠曝光校正系数,范围约为500-1800,值变大，饱和点向左移动
        public static int k2;//过曝光校正系数,范围约为30-200,值变大，饱和点向右移动

        public static int p1;//饱和点最小位置
        public static int p2;  //饱和点最大位置
        public static int saturation = 0;  // %饱和点的图片编号       

        public static List<double> TList;
        public static List<double> EList;
        public static List<double> miu_y;
        public static List<double> miu_y_dark;
        public static List<double> delta_y;
        public static List<double> delta_y_dark;
        public static List<double> SNR;
        public static double PhotoelectricResponseCurve_k;//光电响应线性方程:(miu_y-miu_y_dark)~~texp
        public static double PhotoelectricResponseCurve_b;
        public static double OverAllGain_K;//传感器总体增益(delta_y-delta_y_dark)/(miu_y-miu_y_dark)
		public static double AverageDarkSignal_k;//平均暗信号线性方程:miu_y_dark~~texp
        public static double AverageDarkSignal_b;

		public static double AverageDarkSignal_k2;//delta_y_dark~~texp
		public static double AverageDarkSignal_b2;

        public static int SaturatedIndex;//饱和点位置索引
        public static int Saturated50Index;//50%饱和点位置索引
        public static List<double> miu_d;//平均暗信号
        public static double DR;//动态范围DR
        public static double eta;//量子效率
        public static double FullCapacity;//满阱容量
        /************************************************************************/
        /* 彩色的                                                               */
        /************************************************************************/
        public static List<double> R_miu_y;
        public static List<double> R_miu_y_dark;
        public static List<double> R_delta_y;
        public static List<double> R_delta_y_dark;
        public static List<double> R_SNR;
        public static double R_PhotoelectricResponseCurve_k;//光电响应线性方程
        public static double R_PhotoelectricResponseCurve_b;
        public static double R_OverAllGain_K;//传感器总体增益
        public static double R_AverageDarkSignal_k;//平均暗信号线性方程
        public static double R_AverageDarkSignal_b;
        public static int R_SaturatedIndex;//饱和点位置索引
        public static int R_Saturated50Index;//50%饱和点位置索引
        public static List<double> R_miu_d;//平均暗信号
        public static double R_DR;//动态范围DR
        public static double R_eta;//量子效率
        public static double R_FullCapacity;//满阱容量

        public static List<double> G_miu_y;
        public static List<double> G_miu_y_dark;
        public static List<double> G_delta_y;
        public static List<double> G_delta_y_dark;
        public static List<double> G_SNR;
        public static double G_PhotoelectricResponseCurve_k;//光电响应线性方程
        public static double G_PhotoelectricResponseCurve_b;
        public static double G_OverAllGain_K;//传感器总体增益
        public static double G_AverageDarkSignal_k;//平均暗信号线性方程
        public static double G_AverageDarkSignal_b;
        public static int G_SaturatedIndex;//饱和点位置索引
        public static int G_Saturated50Index;//50%饱和点位置索引
        public static List<double> G_miu_d;//平均暗信号
        public static double G_DR;//动态范围DR
        public static double G_eta;//量子效率
        public static double G_FullCapacity;//满阱容量

        public static List<double> B_miu_y;
        public static List<double> B_miu_y_dark;
        public static List<double> B_delta_y;
        public static List<double> B_delta_y_dark;
        public static List<double> B_SNR;
        public static double B_PhotoelectricResponseCurve_k;//光电响应线性方程
        public static double B_PhotoelectricResponseCurve_b;
        public static double B_OverAllGain_K;//传感器总体增益
        public static double B_AverageDarkSignal_k;//平均暗信号线性方程
        public static double B_AverageDarkSignal_b;
        public static int B_SaturatedIndex;//饱和点位置索引
        public static int B_Saturated50Index;//50%饱和点位置索引
        public static List<double> B_miu_d;//平均暗信号
        public static double B_DR;//动态范围DR
        public static double B_eta;//量子效率
        public static double B_FullCapacity;//满阱容量

        public static void TestExposureTime(byte[] ya,byte[] yb,int row,int col,int PixelDepth,out double y,out double d)
        {
            ushort[,] picA=wfSapGUI.TransPicDatas(ya,row,col,PixelDepth);
            ushort[,] picB = wfSapGUI.TransPicDatas(yb, row, col,PixelDepth);
            ulong y1 = 0;
            ulong d1 = 0;
            int x;
            for (int m = 0; m < row; m++)
            {
                for (int n = 0; n < col; n++)
                {
                    y1 += (ulong)picA[m, n] + (ulong)picB[m, n];
                    x=picA[m,n]-picB[m,n];
                    x=x*x;
                    d1+=(ulong)x;                    
                }
            }
            y =(double)y1 / 2 / row / col;
            d = (double)d1 / 2 / row /col;
        }
        public static void Get_miu_delta(bool bLight,byte[] ya, byte[] yb, int row, int col, int PixelDepth, int rgb1, int rgb2, int rgb3, int rgb4)
        {
            ushort[] yR1;
            ushort[] yG1;
            ushort[] yB1;
            ushort[] yR2;
            ushort[] yG2;
            ushort[] yB2;
            wfSapGUI.TransPicDatas(ya, row, col, PixelDepth, rgb1, rgb2, rgb3, rgb4, out yR1, out yG1, out yB1);
            wfSapGUI.TransPicDatas(yb, row, col, PixelDepth, rgb1, rgb2, rgb3, rgb4, out yR2, out yG2, out yB2);
            ulong y1 = 0;
            ulong d1 = 0;
            double y, d;
            int x;
            for (int i = 0; i < yR1.Length; i++)
            {
                y1 += (ulong)yR1[i] + (ulong)yR2[i];
                x = yR1[i] -yR2[i];
                x = x * x;
                d1 += (ulong)x;
            }            
            y = (double)y1 / 2 / yR1.Length;
            d = (double)d1 / 2 / yR1.Length;
            if(bLight)
            {
                R_miu_y.Add(y);
                R_delta_y.Add(d);
            }
            else
            {
                R_miu_y_dark.Add(y);
                R_delta_y_dark.Add(d);
            }
            y1 = 0;
            d1 = 0;
            for (int i = 0; i < yG1.Length; i++)
            {
                y1 += (ulong)yG1[i] + (ulong)yG2[i];
                x = yG1[i] - yG2[i];
                x = x * x;
                d1 += (ulong)x;
            }
            y = (double)y1 / 2 / yG1.Length;
            d = (double)d1 / 2 / yG1.Length;
            if (bLight)
            {
                G_miu_y.Add(y);
                G_delta_y.Add(d);
            }
            else
            {
                G_miu_y_dark.Add(y);
                G_delta_y_dark.Add(d);
            }

            y1 = 0;
            d1 = 0;
            for (int i = 0; i < yB1.Length; i++)
            {
                y1 += (ulong)yB1[i] + (ulong)yB2[i];
                x = yB1[i] - yB2[i];
                x = x * x;
                d1 += (ulong)x;
            }
            y = (double)y1 / 2 / yB1.Length;
            d = (double)d1 / 2 / yB1.Length;
            if (bLight)
            {
                B_miu_y.Add(y);
                B_delta_y.Add(d);
            }
            else
            {
                B_miu_y_dark.Add(y);
                B_delta_y_dark.Add(d);
            }
        }

        public static void TestExposureTime1(byte[] ya, int row, int col, int PixelDepth, out double y, out double d)
        {
            ushort[,] picA = wfSapGUI.TransPicDatas(ya, row, col,PixelDepth);
            ulong y1 = 0;
            ulong d1 = 0;
            double x;
            for (int m = 0; m < row; m++)
            {
                for (int n = 0; n < col; n++)
                {
                    y1 += (ulong)picA[m, n];
                    //x = picA[m, n] - picB[m, n];
                    //x = x * x;
                    //d1 += (ulong)x;
                }
            }
            y = (double)y1 / row / col;
            for (int m = 0; m < row; m++)
            {
                for (int n = 0; n < col; n++)
                {
                    x = picA[m, n] - y;
                    x = x * x;
                    d1 += (ulong)x;
                }
            }
            
            d = (double)d1  / row / col;
        }
//         public static bool CheckSaturatedIndex()
//         {
//             int flag = 0;//表示曝光情况，1-欠曝光，2-曝光适中，3-过度曝光
//             if((saturation == 0) ||(saturation > p2 ))
//                 flag = 1;
//             else if( ( saturation >= p1 )&&( saturation <= p2 ) )
//                    flag = 2;
//             else
//                    flag = 3;
// 
// //             %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
// //             %修正曝光步长，步长h、明场均值u、饱和点编号saturation，输出修正步长h_correct
// //             %h_correct表示修正后的曝光步长
// //             %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
//             switch (flag)
//             {
//                 case 1:
//                     SystemParam.eStep = (uint)(SystemParam.eStep * (k1 - miu_y[0]) / (miu_y[(int)(SystemParam.ExposureTest_Ns* percent_base/100)] - miu_y[0]));
//                     break;
//                 case 2:
//                     return true;
//                 case 3:
//                     SystemParam.eStep = (uint)(SystemParam.eStep * saturation/k2);
//                     break;
//             }
//             return false;
//         }

		public static bool CheckSaturatedIndex()
		{
			p1 = (int)((Calc1.percent_base - Calc1.percent) * SystemParam.ExposureTest_Ns / 100);
			p2 = (int)((Calc1.percent_base + Calc1.percent) * SystemParam.ExposureTest_Ns / 100);

			int flag = 0;//表示曝光情况，1-欠曝光，2-曝光适中，3-过度曝光
			if ((saturation == 0) || (saturation > p2))
				flag = 1;
			else if ((saturation >= p1) && (saturation <= p2))
				flag = 2;
			else
				flag = 3;


			switch (flag)
			{
				case 1:
					SystemParam.eStep = (uint)(SystemParam.eStep * Calc1.saturation / (SystemParam.ExposureTest_Ns * (Calc1.percent_base / 100)));
					break;
				case 2:
					return true;
				case 3:
					SystemParam.eStep = (uint)(SystemParam.eStep * Calc1.saturation / (SystemParam.ExposureTest_Ns * (Calc1.percent_base / 100)));
					break;
			}
			return false;
		}

    }
}
