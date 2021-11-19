using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WFNetLib.TCP;

namespace udpCCDTest
{
    public class _tcpCCS
    {
        TCPSyncClient CCS;
        public void Connect()
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
        public void LightSet(int lambda, int Oe)
        {
            LightSet(lambda, _tcpCCS.LX2Per(Oe));
        }
        public void LightSet(int lambda,double Oe)
        {
            FormMain.ShowText("设定光源:    波长:" + lambda.ToString()+",照度:"+Oe.ToString("F2"));
            return;
            string str;// = "level:scale 0,0,0,0,0,0,0,0,0,0," + Oe.ToString("F2") + ",0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0";
            int ledIndex = 0;
            if (lambda < 385)
                ledIndex = 0;
            else if (lambda < 395)
                ledIndex = 1;
            else if (lambda < 405)
                ledIndex = 2;
            else if (lambda < 420)
                ledIndex = 3;
            else if (lambda < 435)
                ledIndex = 4;
            else if (lambda < 450)
                ledIndex = 5;
            else if (lambda < 460)
                ledIndex = 6;
            else if (lambda < 470)
                ledIndex = 7;
            else if (lambda < 490)
                ledIndex = 8;
            else if (lambda < 505)
                ledIndex = 9;
            else if (lambda < 520)
                ledIndex = 10;
            else if (lambda < 590)
                ledIndex = 11;
            else if (lambda < 620)
                ledIndex = 12;
            else if (lambda < 630)
                ledIndex = 13;
            else if (lambda < 645)
                ledIndex = 14;
            else if (lambda < 660)
                ledIndex = 15;
            else if (lambda < 680)
                ledIndex = 16;
            else if (lambda < 700)
                ledIndex = 17;
            else if (lambda < 720)
                ledIndex = 18;
            else if (lambda < 740)
                ledIndex = 19;
            else if (lambda < 760)
                ledIndex = 20;
            else if (lambda < 780)
                ledIndex = 21;
            else if (lambda < 810)
                ledIndex = 22;
            else if (lambda < 830)
                ledIndex = 23;
            else if (lambda < 850)
                ledIndex = 24;
            else if (lambda < 880)
                ledIndex = 25;
            else if (lambda < 910)
                ledIndex = 26;
            else if (lambda < 940)
                ledIndex = 27;
            else if (lambda < 980)
                ledIndex = 28;
            else
                ledIndex = 29;
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
        public object SaveDataProcessCallbackProc(byte[] tempbuffer, ref byte[] buffer, ref int dataOffset, int length)
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
