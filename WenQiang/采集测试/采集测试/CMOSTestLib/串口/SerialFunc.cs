using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Windows.Forms;
using WFNetLib;

namespace CMOSTestLib
{
    public class SerialFunc
    {
        public static string SerialPortName=null;
        private static SerialPort mySerialPort;
        public static bool OpenSerialPort()
        {
            mySerialPort = new SerialPort();
            mySerialPort.PortName = SerialPortName;
            mySerialPort.BaudRate = SystemParam.BaudRate;
            mySerialPort.ReadTimeout = 1000;
            while(true)
            {
                try
                {
                    mySerialPort.Open();
                    break;
                }
                catch (System.Exception ex)
                {
                    if(MessageBox.Show("无法打开系统设定串口:"+ex.Message+"，是否重试?","错误",MessageBoxButtons.RetryCancel)==DialogResult.Cancel)
                        return false;
                }
            }
            return true;
        }
        public static void CloseSerialPort()
        {
            mySerialPort.Close();
            mySerialPort.Dispose();
//             try
//             {
//                 
//             }
//             catch
//             {
//                 
//             }
        }
        public static CMOSInfo SerialCommand1()
        {
            CP0314Packet Rx0314 = new CP0314Packet(1);
            byte[] tx = CP0314Packet.MakeCP0314Packet(1, null);
            mySerialPort.Write(tx, 0, tx.Length);
            while (true)
            {
                try
                {
                    if (Rx0314.DataPacketed((byte)mySerialPort.ReadByte()))
                    {
                        CMOSInfo ret = new CMOSInfo();
                        //ret.RowPixels = BytesOP.MakeShort(Rx0314.Data[0], Rx0314.Data[1]);
                        //ret.ColPixels = BytesOP.MakeShort(Rx0314.Data[2], Rx0314.Data[3]);
                        //ret.PixelDepth = Rx0314.Data[4];
                        //ret.RGB = (RGBType)(BytesOP.MakeShort(Rx0314.Data[5], Rx0314.Data[6]));
                        ret.Ts = BytesOP.MakeShort(Rx0314.Data[7], Rx0314.Data[8]);
                        return ret;
                    }
                }
                catch
                {
                    //MessageBox.Show("读取数据失败");
                    break;
                }
            }
            return null;
        }
        public static EnvironmentInfo SerialCommand2()
        {
            CP0314Packet Rx0314 = new CP0314Packet(2);
            byte[] tx = CP0314Packet.MakeCP0314Packet(2, null);
            mySerialPort.Write(tx, 0, tx.Length);
            while (true)
            {
                try
                {
                    if (Rx0314.DataPacketed((byte)mySerialPort.ReadByte()))
                    {
                        EnvironmentInfo ret = new EnvironmentInfo();
                        ret.Temp = BytesOP.MakeShort(Rx0314.Data[0], Rx0314.Data[1]);
                        ret.E = BytesOP.MakeShort(Rx0314.Data[2], Rx0314.Data[3]);
                        return ret;
                    }
                }
                catch
                {
                    //MessageBox.Show("读取数据失败");
                    break;
                }
            }
            return null;
        }
        public static bool SerialCommand3(ushort Fn,uint nLs)
        {
            byte[] t = new byte[6];
            if (nLs < 0x300)
                nLs = 0x300;
            //if (nLs > 0xffff00)
            //    nLs = 0xffff00; 
            t[0] = BytesOP.GetHighByte(Fn);
            t[1] = BytesOP.GetLowByte(Fn);
            t[2] = BytesOP.GetHighByte(BytesOP.GetHighShort(nLs));
            t[3] = BytesOP.GetLowByte(BytesOP.GetHighShort(nLs));
            t[4] = BytesOP.GetHighByte(BytesOP.GetLowShort(nLs));
            t[5] = BytesOP.GetLowByte(BytesOP.GetLowShort(nLs));
            CP0314Packet Rx0314 = new CP0314Packet(3);
            byte[] tx = CP0314Packet.MakeCP0314Packet(3,t);
            mySerialPort.Write(tx, 0, tx.Length);
            while (true)
            {
                try
                {
                    if (Rx0314.DataPacketed((byte)mySerialPort.ReadByte()))
                    {
                        ushort x = BytesOP.MakeShort(Rx0314.Data[0], Rx0314.Data[1]);
                        if (x == 0x4f4b)
                            return true;
                        else
                            return false;
                    }
                }
                catch
                {
                    //MessageBox.Show("读取数据失败");
                    break;
                }
            }
            return false;
        }
    }    
    public class EnvironmentInfo
    {
        public ushort Temp;
        public ushort E;
    }
}
