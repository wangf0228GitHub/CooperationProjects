using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using WFNetLib;

namespace udpCCDTest
{
    public class UDPProc
    {
        public static IPAddress DeviceIP = IPAddress.Parse("127.0.0.1");
        public static int PCDataPort = 12020;
        public static int PCSetPort = 12022;
        public static int DeviceDataPort = 12030;
        public static int DeviceSetPort = 12032;
        static WaitingProc wp;
        public static List<ccdImage> ccdImageList;
        public static List<bool> ccdImageRxConfirm;
        public static IWin32Window owner;
        public static bool CollectImage(IWin32Window _owner, int Tex, int nCount)
        {
            MessageBox.Show(owner, "与采集板通信失败");
            owner = _owner;
            ccdImageList=new List<ccdImage>(nCount);
            ccdImageRxConfirm = new List<bool>(nCount);
            for (int i = 0; i < nCount; i++)
            {
                ccdImageList.Add(new ccdImage());
                ccdImageRxConfirm.Add(false);
            }
            if (UDPProc.UDPCommand_01() == null)
            {
                MessageBox.Show(owner, "与采集板通信失败");
                return false;
            }
            if (UDPCommand_02(Tex, nCount) == null)
            {
                MessageBox.Show(owner,"与采集板通信失败");
                return false;
            }
            wp = new WaitingProc(owner);
            wp.MaxProgress = nCount;
            WaitingProcFunc wpf = new WaitingProcFunc(WaitingImageList);
            wp.Execute(wpf, "图像采集中", WaitingType.None, "是否取消？");
            for (int i = 0; i < nCount; i++)
            {
                if (ccdImageRxConfirm[i] == false)
                    return false;
            }
            return true;
        }
        static void WaitingImageList(object LockWatingThread)
        {
            int retry = 5;
            IPEndPoint dIP = new IPEndPoint(DeviceIP, DeviceDataPort);
            UdpClient udpcSend = new UdpClient();
            UdpClient udpcRecv = new UdpClient(PCDataPort);
            udpcRecv.Client.ReceiveTimeout = 2000;
            ushort ImageCount;
            ushort ImageIndex=0;
            ushort RowCount;
            ushort rowIndex;
            lock (LockWatingThread)
            {
                while (retry != 0)
                {
                    try
                    {
                        while (true)
                        {
                            if (wp.HasBeenCancelled())
                            {
                                return;
                            }
                            byte[] rxList = udpcRecv.Receive(ref dIP);
                            if (rxList[0] != 0x81)
                                continue;
                            ImageCount = BytesOP.MakeShort(rxList[1], rxList[2]);
                            ImageIndex = BytesOP.MakeShort(rxList[3], rxList[4]);
                            RowCount = BytesOP.MakeShort(rxList[5], rxList[6]);
                            rowIndex = BytesOP.MakeShort(rxList[7], rxList[8]);
                            ccdImageList[ImageIndex].rxConfirm[rowIndex, rxList[9]] = true;
                            int dataAddr;
                            if (rxList[9] == 0)//前半行
                                dataAddr = rowIndex * SystemParam.CCD_N * 2;
                            else
                                dataAddr = rowIndex * SystemParam.CCD_N * 2 + SystemParam.CCD_N;
                            Array.Copy(rxList, 10, ccdImageList[ImageIndex].imageData, dataAddr, SystemParam.CCD_N);
                            if (rowIndex == RowCount)//最后一帧
                            {
                                byte[] tx = new byte[34];
                                tx[0] = 0x05;
                                tx[1] = 0x01;//图像接收正确，无需重传
                                for (int i = 0; i < SystemParam.CCD_M; i++)
                                {
                                    if ((ccdImageList[ImageIndex].rxConfirm[i, 0] == false) || ccdImageList[ImageIndex].rxConfirm[i, 1] == false)
                                    {
                                        tx[1] = 0x00;
                                        break;
                                    }
                                }
                                tx[2] = BytesOP.GetLowByte(ImageIndex);
                                tx[3] = BytesOP.GetHighByte(ImageIndex);
                                udpcSend.Send(tx, tx.Length, dIP);
                                if (tx[1] == 0x01)
                                {
                                    wp.SetProcessBarPerformStep();
                                    ccdImageRxConfirm[ImageIndex] = true;
                                    ccdImageList[ImageIndex].ImageCount = ImageCount;
                                    ccdImageList[ImageIndex].ImageIndex = ImageIndex;
                                    ccdImageList[ImageIndex].RowCount = RowCount;
                                    if ((ImageIndex + 1) == ImageCount)
                                    {
                                        return;
                                    }
                                }
                                else
                                {
                                    retry--;
                                    break;
                                }
                            }
                        }

                    }
                    catch //(Exception ex)
                    {
                        //WFNetLib.WFGlobal.WaitMS(1);
                        //MessageBox.Show("读取数据失败");
                        //Application.DoEvents();
                        retry--;
                        continue;
                    }
                }
            }
            MessageBox.Show("采集图像失败：" + (ImageIndex + 1).ToString());
            return;
        }
        public static DeviceState UDPCommand_01()//图像采集板配置帧
        {
            byte[] tx = new byte[34];
            tx[0] = 0x01;
            tx[1] = BytesOP.GetLowByte((ushort)SystemParam.CCD_M);
            tx[2] = BytesOP.GetHighByte((ushort)SystemParam.CCD_M);
            tx[3] = BytesOP.GetLowByte((ushort)SystemParam.CCD_N);
            tx[4] = BytesOP.GetHighByte((ushort)SystemParam.CCD_N);
            tx[5] = (byte)SystemParam.CCD_phi;
            tx[6] = 0x67;
            tx[7] = 0x00;
            tx[8]= (byte)(SystemParam.CCD_PGA+0x0b);
            return udpProc(tx,1000);
        }
        public static DeviceState UDPCommand_02(int Tex,int nCount)//图像采集控制帧
        {
            byte[] tx = new byte[34];
            tx[0] = 0x02;
            tx[1] = BytesOP.GetLowByte(BytesOP.GetLowShort((uint)Tex));
            tx[2] = BytesOP.GetHighByte(BytesOP.GetLowShort((uint)Tex));
            tx[3] = BytesOP.GetLowByte(BytesOP.GetHighShort((uint)Tex));
            tx[4] = BytesOP.GetHighByte(BytesOP.GetHighShort((uint)Tex));
            tx[5] = BytesOP.GetLowByte((ushort)nCount);
            tx[6] = BytesOP.GetHighByte((ushort)nCount);            
            return udpProc(tx,1000);
        }
        public static DeviceState UDPCommand_03(byte xy, byte dir,int nCount)//位移平台控制
        {
            byte[] tx = new byte[34];
            tx[0] = 0x03;
            tx[1] = xy;
            tx[2] = dir;
            tx[3] = BytesOP.GetLowByte((ushort)nCount);
            tx[4] = BytesOP.GetHighByte((ushort)nCount);
            return udpProc(tx,5000);
        }
        public static DeviceState UDPCommand_04()//照度查询帧
        {
            byte[] tx = new byte[34];
            tx[0] = 0x04;
            return udpProc(tx,1000);
        }
        public static DeviceState udpProc(byte[] tx,int timeout)
        {
            int retry = 3;
            IPEndPoint rIP = new IPEndPoint(DeviceIP, DeviceSetPort);
            UdpClient udpcSend = new UdpClient();
            UdpClient udpcRecv = new UdpClient(PCSetPort);
            udpcRecv.Client.ReceiveTimeout = timeout;
            while (retry != 0)
            {
                try
                {
                    udpcSend.Send(tx, tx.Length, rIP);
                    byte[] bytRecv = udpcRecv.Receive(ref rIP);
                    DeviceState ret = new DeviceState(bytRecv);
                    udpcRecv.Close();
                    udpcSend.Close();
                    return ret;
                }
                catch //(Exception ex)
                {
                    //MessageBox.Show("读取数据失败");
                    retry--;
                    continue;
                }
            }
            udpcRecv.Close();
            udpcSend.Close();
            return null;
        }
    }
    public class DeviceState
    {
        public static FormMain fMain;
        public byte CW;//命令字
        public byte rxCW;//板卡上一次接收到的命令字
        public ushort Illuminance;//照度值
        public byte deviceState;//图像采集板自检状态
        public byte CMOSState;//CMOS Sensor状态
        public byte[] all;
        public DeviceState(byte[] _all)
        {
            all = new byte[_all.Length];
            Array.Copy(_all, all, _all.Length);
            CW = all[0];
            rxCW = all[1];
            Illuminance= BytesOP.MakeShort(all[3], all[2]);
            deviceState = all[4];
            CMOSState = all[5];
            fMain.Invoke((EventHandler)(delegate
            {
                fMain.lbDeviceState.Text = deviceState == 0x01 ? "正常" : "异常";
                fMain.lbCMOSState.Text = CMOSState == 0x01 ? "正常" : "异常";
                fMain.lbIlluminance.Text = Illuminance.ToString();

            }));
        }
    }
    public class ccdImage
    {
        public ushort ImageCount;//此次拍照的总图片数
        public ushort ImageIndex;//此幅照片的序号
        public ushort RowCount;//行总数
        public byte[] imageData;
        public Boolean[,] rxConfirm;
        public ccdImage()
        {
            imageData = new byte[SystemParam.CCD_M*SystemParam.CCD_N*2];
            rxConfirm = new Boolean[SystemParam.CCD_M,2];
            for (int i = 0; i < SystemParam.CCD_M; i++)
            {
                rxConfirm[i, 0] = false;
                rxConfirm[i, 1] = false;
            }
        }
    }
}
