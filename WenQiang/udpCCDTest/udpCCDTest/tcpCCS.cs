using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WFNetLib;
using WFNetLib.TCP;

namespace udpCCDTest
{
    public class tcpCCS
    {
        static TCPSyncClient CCS;
        public static double[] L2E_a2;
        public static double[] L2E_a1;
        public static double[] L2E_a0;

        public static int[] lambdaList;
        public static double[] Max_nit;
        public static double oldOe;
        public static void Connect()
        {
            CCS = new TCPSyncClient();
            CCS.TCPServerName = "127.0.0.1";// "192.168.3.250";
            CCS.SaveDataProcessCallback = new SaveDataProcessCallbackDelegate(SaveDataProcessCallbackProc);
            CCS.TCPServerPort = 3434;
            while (true)
            {
                if (CCS.Conn())
                    break;
                if (MessageBox.Show("无法连接到CCS,是否重试!", "CCS连接失败", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1) == DialogResult.Cancel)
                {
                    CCS.Close();
                    CCS = null;
                    FormMain.ShowText("无法连接到CCS");
                    return;
                }
            }
            FormMain.ShowText("成功连接到CCS");
        }
        public static void LightSet(int lambda,double Oe)
        {            
            //return;
            string str;// = "level:scale 0,0,0,0,0,0,0,0,0,0," + Oe.ToString("F2") + ",0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0";
            int ledIndex = Get_lambadIndex(lambda);
            str = "level:scale ";
            for (int i = 0; i<29;i++)
            {
                if (i == ledIndex)
                    str += LX2Per(lambda,Oe).ToString("F4")+",";
                else
                    str += "0,";
            }
            FormMain.ShowText("光源设定:    波长:" + lambda.ToString() + ",  相对照度:" + LX2Per(lambda, Oe).ToString("F4"));
            str = str.Substring(0, str.Length - 1);
            Byte[] byteDateLine = Encoding.ASCII.GetBytes(str.ToCharArray());
            int value = byteDateLine.Length;
            byte[] src = new byte[4];
            src[0] = (byte)((value >> 24) & 0xFF);
            src[1] = (byte)((value >> 16) & 0xFF);
            src[2] = (byte)((value >> 8) & 0xFF);//高8位
            src[3] = (byte)(value & 0xFF);//低位
            CCS.SendPacket(src);
            CCS.SendPacket(byteDateLine);
            object o = CCS.ReceivePacket();
            if (o == null)
            {
                FormMain.ShowText("未收到光源反馈");
                return;
            }
            else
            {
                //FormMain.ShowText("光源反馈:"+(string)o);
            }
            if(oldOe==0 && Oe != 0)//暗场变明场
            {
                WFGlobal.WaitMS(3000);
            }
            else if(Oe==0 && oldOe!=0)//明变暗
            {
                WFGlobal.WaitMS(3000);
            }
            else
            {
                WFGlobal.WaitMS(700);
            }
            oldOe = Oe;
        }
        public static object SaveDataProcessCallbackProc(byte[] tempbuffer, ref byte[] buffer, ref int dataOffset, int length)
        {
            if (length <= 4)
                return null;
            string sRecieved = Encoding.ASCII.GetString(tempbuffer, 0, length);           
            return sRecieved;
        }

        public static int Get_lambadIndex(int lambda)
        {
            for (int i = 1; i < lambdaList.Length; i++)
            {
                if (lambda == lambdaList[i])
                {
                    return i;
                }
            }
            return 0;
        }
        public static double LX2Per(double lx)
        {
            int lambda = SystemParam.lambda_Oe;
            double ret = lx / tcpCCS.Max_nit[Get_lambadIndex(lambda)];
            double per = lx / tcpCCS.Max_nit[Get_lambadIndex(lambda)];
            switch (lambda)
            {
                case 521:
                    ret = -0.4955 * per * per + 1.655 * per - 0.01793;
                    ret = ret / tcpCCS.Max_nit[Get_lambadIndex(lambda)];
                    if (ret < 0)
                    {
                        ret = 0;
                    }
                    break;
                case 634:
                    ret = -0.2413 * per * per + 1.091 * per - 0.02007;
                    ret = ret / tcpCCS.Max_nit[Get_lambadIndex(lambda)];
                    if (ret < 0)
                    {
                        ret = 0;
                    }
                    break;
                case 854:
                    ret = -0.3012 * per * per + 9.666 * per - 0.1707;
                    ret = ret / tcpCCS.Max_nit[Get_lambadIndex(lambda)];
                    if (ret < 0)
                    {
                        ret = 0;
                    }
                    break;
            }
            return ret;
        }

        public static double LX2Per(int lambda, double lx)
        {
            double ret = lx / tcpCCS.Max_nit[Get_lambadIndex(lambda)];
            double per = lx / tcpCCS.Max_nit[Get_lambadIndex(lambda)];
            switch (lambda)
            {
                case 521:
                    ret = -0.4955 * per * per + 1.655 * per - 0.01793;
                    ret = ret / tcpCCS.Max_nit[Get_lambadIndex(lambda)];
                    if (ret < 0)
                    {
                        ret = 0;
                    }

                    break;
                case 634:
                    
                     ret = -0.2413 * per * per + 1.091 * per - 0.02007;
                     ret = ret / tcpCCS.Max_nit[Get_lambadIndex(lambda)];

                  
                    if (ret < 0)
                    {
                        ret = 0;
                    }
                    break;
                case 854:
                    ret = -0.3012 * per * per + 9.666 * per - 0.1707;
                    ret = ret / tcpCCS.Max_nit[Get_lambadIndex(lambda)];
                    if (ret < 0)
                    {
                        ret = 0;
                    }

                    
                    break;
            }
            if (ret > 1)
                ret = 1;
            return ret;
        }
        public static double Per2LX(double per)
        {
            return (per * tcpCCS.Max_nit[Get_lambadIndex(SystemParam.lambda_Oe)]);            
        }

        public static double Per2LX(int lambda,double per)
        {
            return (per * tcpCCS.Max_nit[Get_lambadIndex(lambda)]);
        }
    }
}
