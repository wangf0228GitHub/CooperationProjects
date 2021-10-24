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
        public void LightSet(int lambda,double Oe)
        {
            FormMain.ShowText("设定光源:    波长:" + lambda.ToString()+",照度:"+Oe.ToString("F2"));
            string str = "level:scale 0,0,0,0,0,0,0,0,0,0," + Oe.ToString("F2") + ",0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0";
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
            string sRecieved = Encoding.ASCII.GetString(tempbuffer, 0, length);           
            return sRecieved;
        }
    }
}
