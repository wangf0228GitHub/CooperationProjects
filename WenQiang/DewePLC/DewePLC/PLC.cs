﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        TCPSyncClient tcpPLC;
        public void PLCConnect()
        {
            tcpPLC = new TCPSyncClient();
            tcpPLC.TCPServerName = "127.0.0.1";// "192.168.3.250";
            tcpPLC.SaveDataProcessCallback = new SaveDataProcessCallbackDelegate(TcpModbusPacket.SaveDataProcessCallbackProc);
            tcpPLC.TCPServerPort = 502;
            while(true)
            {
                if (tcpPLC.Conn())
                    break;
                if (MessageBox.Show("无法连接连接到PLC,是否重试!", "PLC连接失败", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1) == DialogResult.Cancel)
                {
                    this.Close();
                    return;
                }
            }
            ShowText("成功连接到PLC");
        }
        TcpModbusPacket ModbusWork(byte[] tx, TcpModbusPacket.FunctionCode type)
        {
            byte[] txBytes = TcpModbusPacket.MakePacket(0, 0, type, tx);
            ShowText("发送数据:" + WFNetLib.StringFunc.StringsFunction.byteToHexStr(txBytes, " "));
            if (!tcpPLC.SendPacket(txBytes))
            {
                tcpPLC.Close();
                PLCConnect();
                return null;
            }
            object o = tcpPLC.ReceivePacket();
            if (o == null)
            {
                ShowText("未收到需要读取的数据");
                return null;
            }
            TcpModbusPacket tp = (TcpModbusPacket)o;
            ShowText("收到数据:" + WFNetLib.StringFunc.StringsFunction.byteToHexStr(tp.Data, " "));
            return tp;
        }
        public TcpModbusPacket ReadPLC()
        {
            byte[] tx = new byte[4];
            tx[0] = 0x00;//起始地址
            tx[1] = 100;
            tx[2] = 0;//长度
            tx[3] = 1;
            TcpModbusPacket tp = ModbusWork(tx, TcpModbusPacket.FunctionCode.Read);
            return tp;
        }
        public void SetMotor(double rev,double torque)
        {
            Debug.Assert(rev>0);
            Debug.Assert(torque>0);
            if (torque > 28.6)
                torque = 28.6;
            int retry = 3;
            while(true)
            {
                byte[] tx = new byte[8 + 5];
                tx[0] = 0x00;//起始地址
                tx[1] = 0x00;
                tx[2] = 0;//长度
                tx[3] = 4;
                tx[4] = 8;//写入字节数
                rev = rev * 360 * 1000;
                if (!bForeward)
                {
                    rev = rev * -1;
                    torque = torque * -1;
                }
                byte[] x = BitConverter.GetBytes((int)rev);
                tx[5] = x[1];
                tx[6] = x[0];
                tx[7] = x[3];
                tx[8] = x[2];
                torque = torque / 28.6 * 1000;
                this.Invoke((EventHandler)(delegate
                {
                    listView2.Items[7].SubItems[1].Text = ((int)torque).ToString();
                }));
                x = BitConverter.GetBytes((int)torque);
                tx[9] = x[1];
                tx[10] = x[0];
                tx[11] = x[3];
                tx[12] = x[2];
                TcpModbusPacket tp= ModbusWork(tx, TcpModbusPacket.FunctionCode.Write);
                if (tp != null)
                    return;
                retry --;
                if(retry==0)
                {
                    if (MessageBox.Show("无法连接连接到PLC,是否重试!", "PLC连接失败", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1) == DialogResult.Cancel)
                    {
                        this.Close();
                        return;
                    }
                    retry = 3;
                }
            }
            
        }
        void SetAllTime(int ms1,int ms2)
        {
            int retry = 3;
            while (true)
            {
                byte[] tx = new byte[8 + 5];
                tx[0] = 0x00;//起始地址
                tx[1] = 20;
                tx[2] = 0;//长度
                tx[3] = 4;
                tx[4] = 8;//写入字节数            
                byte[] x = BitConverter.GetBytes(ms1);
                tx[5] = x[1];
                tx[6] = x[0];
                tx[7] = x[3];
                tx[8] = x[2];
                x = BitConverter.GetBytes(ms2);
                tx[9] = x[1];
                tx[10] = x[0];
                tx[11] = x[3];
                tx[12] = x[2];
                TcpModbusPacket tp = ModbusWork(tx, TcpModbusPacket.FunctionCode.Write);
                if (tp != null)
                    return;
                retry--;
                if (retry == 0)
                {
                    if (MessageBox.Show("无法连接连接到PLC,是否重试!", "PLC连接失败", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1) == DialogResult.Cancel)
                    {
                        this.Close();
                        return;
                    }
                    retry = 3;
                }
            }
        }
        //         void SetTorque(int torque)
        //         {
        //             byte[] tx = new byte[4 + 5];
        //             tx[0] = 0x00;//起始地址
        //             tx[1] = 0x02;
        //             tx[2] = 0;//长度
        //             tx[3] = 2;
        //             tx[4] = 4;//写入字节数
        //             //torque = torque * 360 * 1000;
        //             if (!bForeward)
        //                 torque = torque * -1;
        //             byte[] x = BitConverter.GetBytes(torque);
        //             tx[5] = x[1];
        //             tx[6] = x[0];
        //             tx[7] = x[3];
        //             tx[8] = x[2];
        //             ModbusWork(tx, TcpModbusPacket.FunctionCode.Write);
        //         }
    }
}
