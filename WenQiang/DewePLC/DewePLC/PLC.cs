using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WFNetLib.PacketProc;
using WFNetLib.TCP;

namespace DewePLC
{
    public partial class Form1 : Form
    {
        TcpModbusPacket ModbusWork(byte[] tx, TcpModbusPacket.FunctionCode type)
        {
            string str;
            TCPSyncClient tcp = new TCPSyncClient();
            tcp.TCPServerName = "192.168.3.250";
            tcp.SaveDataProcessCallback = new SaveDataProcessCallbackDelegate(TcpModbusPacket.SaveDataProcessCallbackProc);
            tcp.TCPServerPort = 502;
            //ShowText("-----------------------"+DateTime.Now.ToLocalTime()+"------------------------");
            if (!tcp.Conn())
            {
                str = "无法连接到服务器:";
                if (tcp.LastException.GetType() == typeof(SocketException))
                {
                    SocketException ex = (SocketException)tcp.LastException;
                    switch (ex.ErrorCode)
                    {
                        case 10061:
                            str += ex.Message;
                            break;
                        case 10060:
                            str += ex.Message;
                            break;
                    }
                }
                else
                    str = "未知错误:" + tcp.LastException.Message;
                ShowText(str);
                return null;
            }
            byte[] txBytes = TcpModbusPacket.MakePacket(0, 0, type, tx);
            ShowText("发送数据:" + WFNetLib.StringFunc.StringsFunction.byteToHexStr(txBytes, " "));
            if (!tcp.SendPacket(txBytes))
            {
                return null;
            }
            object o = tcp.ReceivePacket();
            if (o == null)
            {
                ShowText("未收到需要读取的数据");
                return null;
            }
            TcpModbusPacket tp = (TcpModbusPacket)o;
            ShowText("收到数据:" + WFNetLib.StringFunc.StringsFunction.byteToHexStr(tp.Data, " "));
            tcp.Close();
            return tp;
        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            byte[] tx = new byte[4];
            tx[0] = 0x00;//起始地址
            tx[1] = 0xe9;
            tx[2] = 0;//长度
            tx[3] = 4;
            TcpModbusPacket tp = ModbusWork(tx, TcpModbusPacket.FunctionCode.Read);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            byte[] tx = new byte[200 + 5];
            tx[0] = 0x01;//起始地址
            tx[1] = 0x2c;
            tx[2] = 0;//长度
            tx[3] = 100;
            tx[4] = 200;//写入字节数
            for (int i = 0; i < 200; i++)
                tx[i + 5] = (byte)i;
            //tx[6]=(byte)20;
            ModbusWork(tx, TcpModbusPacket.FunctionCode.Write);
        }
    }
}
