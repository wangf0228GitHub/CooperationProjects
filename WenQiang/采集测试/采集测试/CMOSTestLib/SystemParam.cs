using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace CMOSTestLib
{
    public class CMOSInfo
    {
        //public int RowPixels;
        //public int ColPixels;
        //public int PixelDepth;
        public int PixelArea;
        public double Ts;//最小曝光时间,ns
        public int Lambda;
        public int bRGB;
        public int RGB1;
        public int RGB2;
        public int RGB3;
        public int RGB4;
        //public string ccfPath;
    }
    public class EInfo
    {
        public double Rf;
        public double rho;
        public double S;
    }
    public class SystemParam
    {
        public static int BaudRate = 9600;
        public static string ccfPath;
        public static string TempPicPath= System.Windows.Forms.Application.StartupPath + "\\TempPic\\";
        public static string TxtDataPath = System.Windows.Forms.Application.StartupPath + "\\TxtData\\";
        public static string TxtCustomDarkDataPath = System.Windows.Forms.Application.StartupPath + "\\CustomDarkData\\";
        public static string TxtCustomDarkDataFormat = "{0,-15}{1,-15}{2,-15}";
        /************************************************************************/
        /*                                                                      */
        /************************************************************************/
        public static string TxtDataFormat = "{0,-15}{1,-15}{2,-15}{3,-15}{4,-15}";
        public static string TxtDataTitleFormat = "{0,-11}{1,-11}{2,-11}{3,-11}{4,-11}";
        public static string TxtDataFormat_RGB = "{0,-15}{1,-15}{2,-15}{3,-15}{4,-15}{5,-15}{6,-15}{7,-15}{8,-15}{9,-15}{10,-15}{11,-15}{12,-15}";
        public static string TxtDataTitleFormat_RGB = "{0,-11}{1,-11}{2,-11}{3,-11}{4,-11}{5,-11}{6,-11}{7,-11}{8,-11}{9,-11}{10,-11}{11,-11}{12,-11}";
        /************************************************************************/
        /*                                                                      */
        /************************************************************************/
        public static string TxtCustomDarkDataTitleFormat = "{0,-11}{1,-11}{2,-11}";
        public static uint eStart=4000;
        public static uint eStep=4000;
        public static int ExposureTest_Ns=150;
        public static double ExposurePointThreshold=5;

        public static double Ts;//最小曝光时间,ns为单位

        public static int PicDelay;
		public static int WaitTimeOut;

        public static CMOSInfo cmosInfo;

        public static int ByteLen4Pic;
        public static int Pixel4Pic;        
        public static EInfo eInfo;

        public static ushort L = 200;
        public static ushort Step2_len = 20;
        public static double DarkPointPer=50;
        public static double LightPointPer = 50;

        public static string DeviceID;

        public static byte[] ReadTempFile(int Len, int index,string fileName)
        {
            Stream stream = File.OpenRead(fileName);
            long x = stream.Seek((long)Len * (long)index, SeekOrigin.Begin);
            byte[] ret = new byte[Len];
            stream.Read(ret, 0, Len);
            stream.Close();
            return ret;
        }
        public static void WriteTempFile(byte[] pBuf, int index, string fileName)
        {
            Stream stream = File.OpenWrite(fileName);
            long x = stream.Seek((long)pBuf.Length * (long)index, SeekOrigin.Begin);
            stream.Write(pBuf, 0, pBuf.Length);
            stream.Flush();
            stream.Close();
        }
        public static void CreateBINFile(byte[] pBuf, string fileName)
        {
            FileStream fs = null;
            try
            {
                FileInfo f = new FileInfo(fileName);
                if (!Directory.Exists(f.DirectoryName))
                    Directory.CreateDirectory(f.DirectoryName);
                fs = new FileStream(fileName, FileMode.Create);                
                fs.SetLength(pBuf.Length); //设置文件大小  
            }
            catch (Exception ex)
            {
                if (fs != null)
                {
                    fs.Close();
                    File.Delete(fileName); //注意，若由fs.SetLength方法产生了异常，同样会执行删除命令，请慎用overwrite:true参数，或者修改删除文件代码。  
                }
                MessageBox.Show("创建临时文件出错！" + ex.Message);
                throw ex;
            }
            finally
            {
                if (fs != null) fs.Close();
            }
            Stream stream = File.OpenWrite(fileName);
            long x = stream.Seek(0, SeekOrigin.Begin);
            stream.Write(pBuf, 0, pBuf.Length);
            stream.Flush();
            stream.Close();
        }
        public static void CreateTempFile(int row, int col, int BytesPerPixel, int count, string fileName)
        {
            FileStream fs = null;
            try
            {
                FileInfo f = new FileInfo(fileName);
                if (!Directory.Exists(f.DirectoryName))
                    Directory.CreateDirectory(f.DirectoryName);
                fs = new FileStream(fileName, FileMode.Create);
                Int64 len = (Int64)row * (Int64)col * (Int64)BytesPerPixel * (Int64)count;
                fs.SetLength(len); //设置文件大小  
            }
            catch (Exception ex)
            {
                if (fs != null)
                {
                    fs.Close();
                    File.Delete(fileName); //注意，若由fs.SetLength方法产生了异常，同样会执行删除命令，请慎用overwrite:true参数，或者修改删除文件代码。  
                }
                MessageBox.Show("创建临时文件出错！" + ex.Message);
                throw ex;
            }
            finally
            {
                if (fs != null) fs.Close();
            }
        }
        public static double GetTime(int index)
        {
            double ret = SystemParam.eStart + index * SystemParam.eStep;
            ret = ret * SystemParam.Ts;
            return ret;
        }
    }
}
