using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WFNetLib.PacketProc;

namespace 热量表标定
{
    public partial class Form1 : Form
    {
        private void btParamSet_Click(object sender, EventArgs e)
        {
            tbDateTime.Text = DateTime.Now.ToString();
            if (!WFNetLib.WFGlobal.OpenSerialPort(ref serialPort1, "调试串口"))
                return;
            byte[] tx = new byte[16];
            tx[0] = (byte)(DateTime.Now.Year-2000);
            tx[1] = (byte)(DateTime.Now.Month);
            tx[2] = (byte)(DateTime.Now.Day);
            tx[3] = (byte)(DateTime.Now.DayOfWeek);
            tx[4] = (byte)(DateTime.Now.Hour);
            tx[5] = (byte)(DateTime.Now.Minute);
            tx[6] = (byte)(DateTime.Now.Second);
            CP68Packet ret = CP68Packet.CP68ComProc(ref serialPort1, CommandConst.CP68_UserCommand_WriteDeviceParams, tx);
            if (ret != null)
            {
                if (ret.Header.ControlCode == (byte)(CommandConst.CP68_UserCommand_WriteDeviceParams | 0x80))
                    MessageBox.Show("设定成功");
                else
                    MessageBox.Show("设定失败");
            }
            serialPort1.Close();
        }
    }
}
