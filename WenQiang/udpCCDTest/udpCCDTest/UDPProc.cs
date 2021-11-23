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
        public static object oImage=new object();
        public static bool CollectImage(IWin32Window _owner, int Tex, int nCount)
        {
            owner = _owner;            
            if (UDPProc.UDPCommand_01() == null)
            {
                MessageBox.Show(owner, "与采集板通信失败");
                return false;
            }
            while(true)
            {
                ccdImageList = new List<ccdImage>(nCount);
                ccdImageRxConfirm = new List<bool>(nCount);
                for (int i = 0; i < nCount; i++)
                {
                    ccdImageList.Add(new ccdImage());
                    ccdImageRxConfirm.Add(false);
                }
                if (UDPCommand_02(Tex, nCount) == null)
                {
                    MessageBox.Show(owner, "与采集板通信失败");
                    return false;
                }
                if(nCount<3)
                {
                    wp = null;
                    WaitingImageList(oImage);
                }
                else
                {
                    wp = new WaitingProc(owner);
                    wp.MaxProgress = nCount;
                    WaitingProcFunc wpf = new WaitingProcFunc(WaitingImageList);
                    wp.Execute(wpf, "图像采集中", WaitingType.None, "是否取消？");
                }
                bool bok = true;
                for (int i = 0; i < nCount; i++)
                {
                    if (ccdImageRxConfirm[i] == false)
                    {
                        bok = false;
                        break;                        
                    }
                }
                if(bok)
                    return true;
                else
                {
                    if (MessageBox.Show("图像采集失败，是否重试", "图像采集失败", MessageBoxButtons.RetryCancel) == DialogResult.Retry)
                        return false;
                }
            }
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
                            if(wp!=null)
                            {
                                if (wp.HasBeenCancelled())
                                {
                                    return;
                                }
                            }
                            byte[] rxList = udpcRecv.Receive(ref dIP);
                            if (rxList[0] != 0x81)
                                continue;
                            ImageCount = BytesOP.MakeShort(rxList[1], rxList[2]);
                            ImageIndex = BytesOP.MakeShort(rxList[3], rxList[4]);
                            RowCount = BytesOP.MakeShort(rxList[5], rxList[6]);
                            rowIndex = BytesOP.MakeShort(rxList[7], rxList[8]);
                            //确认收到了某行
                            ccdImageList[ImageIndex].rxConfirm[rowIndex, rxList[9]] = true;
                            int dataAddr= rowIndex * SystemParam.CCD_N * 2+1440*rxList[9];
                            Array.Copy(rxList, 10, ccdImageList[ImageIndex].byteList, dataAddr, 1440);
                            if (rowIndex == RowCount && rxList[9]==11)//最后一帧
                            {
                                byte[] tx = new byte[34];
                                tx[0] = 0x05;
                                tx[1] = 0x01;//图像接收正确，无需重传
                                for (int i = 0; i < SystemParam.CCD_M; i++)
                                {
                                    for(int j=0;j<12;j++)
                                    {
                                        if (ccdImageList[ImageIndex].rxConfirm[i, j] == false)
                                        {
                                            tx[1] = 0x00;
                                            break;
                                        }
                                    }
                                    if(tx[1] == 0x00)
                                        break;
                                }
                                tx[2] = BytesOP.GetLowByte(ImageIndex);
                                tx[3] = BytesOP.GetHighByte(ImageIndex);
                                udpcSend.Send(tx, tx.Length, dIP);
                                if (tx[1] == 0x01)
                                {
                                    if (wp != null)
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
}
