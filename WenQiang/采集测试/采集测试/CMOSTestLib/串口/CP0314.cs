using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WFNetLib.PacketProc;
using WFNetLib;

namespace CMOSTestLib
{
    /// <summary>
    /// 包头
    /// </summary>
    public class CP0314PacketHead
    {
        /// <summary>
        /// 包头大小
        /// </summary>        
        public static Int32 DataLen_SIZE = 1;
        public static Int32 CommandLen_SIZE = 1;
        public static Int32 HEAD_SIZE = 4;
        public byte RxAddr;
        public byte Command;
        public byte Len = 0;
        public byte[] Data;
        public CP0314PacketHead()
        {
            Data = new byte[HEAD_SIZE];
        }
    }
    public class CP0314Packet
    {
        public CP0314PacketHead Header;
        public byte[] Data;
        public int RxCount;
        public byte[] RxList;
        public byte NeedCommand;
        public static Int32 BUFFER_SIZE = 1024;
        public CP0314Packet(byte com)
        {
            NeedCommand = com;

            Header = new CP0314PacketHead();
            RxCount = 0;
        }
        public CP0314Packet()
        {
            NeedCommand = 0xff;
            Header = new CP0314PacketHead();
            RxCount = 0;
        }
        public static byte[] MakeCP0314Packet(byte com, byte b)
        {
            return MakeCP0314Packet(com, new byte[] { b });
        }
        public static byte[] MakeCP0314Packet(byte com, byte[] data)
        {
            byte[] txbuffer;
            if (data == null)
            {
                txbuffer = new byte[CP0314PacketHead.HEAD_SIZE + 2];
            }
            else
            {
                txbuffer = new byte[CP0314PacketHead.HEAD_SIZE + data.Length + 2];
            }
            txbuffer[0] = 0x03;
            txbuffer[1] = 0x14;
            txbuffer[2] = com;
            if(data!=null)
            {
                txbuffer[3] = (byte)data.Length;
                for (int i = 0; i < data.Length; i++)
                {
                    txbuffer[i + CP0314PacketHead.HEAD_SIZE] = data[i];
                }
                txbuffer[CP0314PacketHead.HEAD_SIZE + data.Length] = Verify.GetVerify_byteSum(txbuffer, CP0314PacketHead.HEAD_SIZE + data.Length);
                txbuffer[CP0314PacketHead.HEAD_SIZE + data.Length + 1] = 0x0d;
                return txbuffer;
            }
            else
            {
                txbuffer[3] = 0;
                txbuffer[4] = Verify.GetVerify_byteSum(txbuffer, 4);
                txbuffer[5] = 0x0d;
                return txbuffer;
            }            
        }
        public bool DataPacketed(byte rx)
        {
            //判断读取的字节数+缓冲区已有字节数是否超过缓冲区总大小
            if (RxCount < CP0314PacketHead.HEAD_SIZE)
            {
                Header.Data[RxCount++] = rx;
                if (RxCount == 1)
                {
                    if (Header.Data[0] != 0x03)
                    {
                        RxCount = 0;
                    }
                }
                else if (RxCount == 2)
                {
                    if (Header.Data[1] != 0x14)
                    {
                        RxCount = 0;
                    }
                }
                else if (RxCount == 3)
                {
                    Header.Command = Header.Data[2];
                    if (NeedCommand != 0xff)
                    {
                        if (Header.Command != NeedCommand)
                        {
                            RxCount = 0;
                        }
                    }
                }
                else if (RxCount == 4)
                {
                    Header.Len = Header.Data[3];
                    Data = new byte[Header.Len + 2];
                }
            }
            else
            {
                Data[RxCount - CP0314PacketHead.HEAD_SIZE] = rx;
                RxCount++;
                if (RxCount == (CP0314PacketHead.HEAD_SIZE + Header.Len + 2))
                {
                    if (Data[Data.Length - 1] == 0x0d)
                    {
                        byte s1 = Verify.GetVerify_byteSum(Header.Data);
                        byte s2 = Verify.GetVerify_byteSum(Data, Data.Length - 2);
                        s1 = (byte)(s1 + s2);
                        if (s1 == Data[Data.Length - 2])
                            return true;
                    }
                    else
                    {
                        RxCount = 0;
                        return false;
                    }
                }
            }
            return false;
        }
    }
}
