using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace CMOSTestLib
{
    public class PixelInfo
    {
        public int row;
        public int col;
        public double y;
    }
    public class Calc2
    {
        public static string TempPicPath_Light = System.Windows.Forms.Application.StartupPath + "\\Temp2L\\";
        public static string TempPicPath_Dark = System.Windows.Forms.Application.StartupPath + "\\Temp2D\\";
        public static double[,] y50;
        public static double[,] y50_dark;

        public static double miu_y50;
        public static double miu_y50_dark;

        public static double[,] delta_y50;
        public static double[,] delta_y50_dark;

        public static double delta_y50_stack;
        public static double delta_y50_dark_stack;

        public static double S2_y50;
        public static double S2_y50_dark;

        public static double DSNU1288;
        public static double PRNU1288;

        public static List<PixelInfo> DarkPoints;
        public static List<PixelInfo> LightPoints;

        public static string LightTempFile = System.Windows.Forms.Application.StartupPath + "\\TempData\\Step2_LightTemp.bin";
        public static string DarkTempFile = System.Windows.Forms.Application.StartupPath + "\\TempData\\Step2_DarkTemp.bin";



        public static double[] R_y50;
        public static double[] R_y50_dark;

        public static double R_miu_y50;
        public static double R_miu_y50_dark;

        public static double[] R_delta_y50;
        public static double[] R_delta_y50_dark;

        public static double R_delta_y50_stack;
        public static double R_delta_y50_dark_stack;

        public static double R_S2_y50;
        public static double R_S2_y50_dark;

        public static double R_DSNU1288;
        public static double R_PRNU1288;

        public static List<PixelInfo> R_DarkPoints;
        public static List<PixelInfo> R_LightPoints;



        public static double[] B_y50;
        public static double[] B_y50_dark;

        public static double B_miu_y50;
        public static double B_miu_y50_dark;

        public static double[] B_delta_y50;
        public static double[] B_delta_y50_dark;

        public static double B_delta_y50_stack;
        public static double B_delta_y50_dark_stack;

        public static double B_S2_y50;
        public static double B_S2_y50_dark;

        public static double B_DSNU1288;
        public static double B_PRNU1288;

        public static List<PixelInfo> B_DarkPoints;
        public static List<PixelInfo> B_LightPoints;



        public static double[] G_y50;
        public static double[] G_y50_dark;

        public static double G_miu_y50;
        public static double G_miu_y50_dark;

        public static double[] G_delta_y50;
        public static double[] G_delta_y50_dark;

        public static double G_delta_y50_stack;
        public static double G_delta_y50_dark_stack;

        public static double G_S2_y50;
        public static double G_S2_y50_dark;

        public static double G_DSNU1288;
        public static double G_PRNU1288;

        public static List<PixelInfo> G_DarkPoints;
        public static List<PixelInfo> G_LightPoints;
    }
}
