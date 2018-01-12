using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WFNetLib.PacketProc;
using WFNetLib;
using LinearAlgebra;
using LinearAlgebra.LinearEquations;

namespace 热量表标定
{
    public partial class Form1 : Form
    {
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                if (WFNetLib.WFGlobal.OpenSerialPort(ref serialPort1, "调试串口"))
                {
                    checkBox1.Text = "停止采集";
                    btTCalc.Enabled = false;
                    timer1.Enabled = true;
                }
                else
                    checkBox1.Checked = false;
            }
            else
            {
                serialPort1.Close();
                checkBox1.Text = "开始采集";
                btTCalc.Enabled = true;
                timer1.Enabled = false;
            }
        }
        ushort t1ad, t2ad, vinad, vrefad;
        float t1v, t2v;
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            CP68Packet ret = CP68Packet.CP68ComProc(ref serialPort1, CommandConst.CP68_UserCommand_ReadAD, null);
            this.Invoke((EventHandler)(delegate
            {
                if (ret != null)
                {
                    t1ad = BytesOP.MakeShort(ret.Data[1], ret.Data[0]);
                    listView1.Items[0].SubItems[1].Text = t1ad.ToString();
                    t2ad = BytesOP.MakeShort(ret.Data[3], ret.Data[2]);
                    listView1.Items[4].SubItems[1].Text = t2ad.ToString();
//                     vinad = BytesOP.MakeShort(ret.Data[5], ret.Data[4]);
//                     listView1.Items[8].SubItems[1].Text = vinad.ToString();
//                     vrefad = BytesOP.MakeShort(ret.Data[7], ret.Data[6]);
//                     listView1.Items[9].SubItems[1].Text = vrefad.ToString();
//                     double power = 3.0 * vrefad/vinad;
//                     listView1.Items[10].SubItems[1].Text = power.ToString("F4");
//                     t1v = (float)(power * t1ad / 4096);
//                     listView1.Items[1].SubItems[1].Text = t1v.ToString("F4");
// 
//                     t2v = (float)(power * t2ad / 4096);
//                     listView1.Items[5].SubItems[1].Text = t2v.ToString("F4");

                    float tin = BitConverter.ToSingle(ret.Data, 8);
                    float tout = BitConverter.ToSingle(ret.Data, 12);
                    listView1.Items[2].SubItems[1].Text = tin.ToString("F4");
                    listView1.Items[6].SubItems[1].Text = tout.ToString("F4");
                    progressBar1.PerformStep();
                    if (progressBar1.Value == progressBar1.Maximum)
                        progressBar1.Value = progressBar1.Minimum;
                }
            }));
            timer1.Enabled = true;
        }
        private void btT1_Click(object sender, EventArgs e)
        {
            
            listView1.Items[12].SubItems[1].Text = t1ad.ToString();
            listView1.Items[13].SubItems[1].Text = t2ad.ToString();
        }

        private void btT2_Click(object sender, EventArgs e)
        {
            listView1.Items[15].SubItems[1].Text = t1ad.ToString();
            listView1.Items[16].SubItems[1].Text = t2ad.ToString();
        } 

        private void button3_Click(object sender, EventArgs e)
        {
            if (!WFNetLib.WFGlobal.OpenSerialPort(ref serialPort1, "调试串口"))
                return;
            float t1 = 0.0f;
            float t2 = 100.0f;
            float adin1 = float.Parse(listView1.Items[12].SubItems[1].Text);
            float adin2 = float.Parse(listView1.Items[15].SubItems[1].Text);

            float adout1 = float.Parse(listView1.Items[13].SubItems[1].Text);
            float adout2 = float.Parse(listView1.Items[16].SubItems[1].Text);

            float kin = (t2 - t1) / (adin2 - adin1);
            float bin = t1 - kin * adin1;

            float kout = (t2 - t1) / (adout2 - adout1);
            float bout = t1 - kout * adout1;

            //             float kin = 1.987f;
            //             float bin = 2.365f;
            //             float kout = -5.687f;
            //             float bout = -7.668f;


            byte[] k1B = BitConverter.GetBytes(kin);
            byte[] b1B = BitConverter.GetBytes(bin);
            byte[] k2B = BitConverter.GetBytes(kout);
            byte[] b2B = BitConverter.GetBytes(bout);

            byte[] tx = new byte[16];
            for (int i = 0; i < 4; i++)
            {
                tx[i] = k1B[i];
                tx[4 + i] = b1B[i];
                tx[8 + i] = k2B[i];
                tx[12 + i] = b2B[i];
            }
            CP68Packet ret = CP68Packet.CP68ComProc(ref serialPort1, CommandConst.CP68_UserCommand_WriteTCalc, tx);
            if (ret != null)
            {
                if (ret.Header.ControlCode == (byte)(CommandConst.CP68_UserCommand_WriteTCalc | 0x80))
                    MessageBox.Show("设定成功");
                else
                    MessageBox.Show("设定失败");
            }
            serialPort1.Close();
        }
    }
}
