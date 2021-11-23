using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WFNetLib.TCP;

namespace udpCCDTest
{
    public class tcpCCS
    {
        static TCPSyncClient CCS;
        public static double[] L2E_a2;
        public static double[] L2E_a1;
        public static double[] L2E_a0;
        
        public static int[] lambdaList = new int[30]{
            375,385,395,405,420,435,
            450,460,470,490,505,520,
            590,620,630,645,660,680,
            700,720,740,760,780,810,
            830,850,880,910,940,980
        };
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
        public static void LightSet(int lambda, int Oe)
        {
            LightSet(lambda, tcpCCS.LX2Per(Oe));
        }
        public static void LightSet(int lambda,double Oe)
        {
            FormMain.ShowText("设定光源:    波长:" + lambda.ToString()+",照度:"+Oe.ToString("F2"));
            return;
            string str;// = "level:scale 0,0,0,0,0,0,0,0,0,0," + Oe.ToString("F2") + ",0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0";
            int ledIndex = 0;
            if (lambda < lambdaList[1])
                ledIndex = 0;
            else if (lambda >= lambdaList.Last())
                ledIndex = lambdaList.Length - 1;
            else
            {
                for (int i = 1; i < lambdaList.Length; i++)
                {
                    if (lambda < lambdaList[i])
                    {
                        ledIndex = i - 1;
                        break;
                    }
                }
            }
            str = "level:scale ";
            for (int i = 0; i<29;i++)
            {
                if (i == ledIndex)
                    str += Oe.ToString("F2")+",";
                else
                    str += "0,";
            }
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
                FormMain.ShowText("光源反馈:"+(string)o);
            }
        }
        public static object SaveDataProcessCallbackProc(byte[] tempbuffer, ref byte[] buffer, ref int dataOffset, int length)
        {
            if (length <= 4)
                return null;
            string sRecieved = Encoding.ASCII.GetString(tempbuffer, 0, length);           
            return sRecieved;
        }

        public static double LX2Per(int lx)
        {
            return 0.5;
        }

        public static int Per2LX(double per)
        {
            return 5;
        }
    }
}
