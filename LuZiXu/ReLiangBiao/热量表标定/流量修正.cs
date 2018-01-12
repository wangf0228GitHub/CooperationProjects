using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WFNetLib.PacketProc;
using LinearAlgebra;
using LinearAlgebra.LinearEquations;

namespace 热量表标定
{
    public partial class Form1 : Form
    {
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                if (WFNetLib.WFGlobal.OpenSerialPort(ref serialPort1, "调试串口"))
                {
                    checkBox2.Text = "停止采集";
                    btQCalc.Enabled = false;
                    timer2.Enabled = true;
                }
                else
                    checkBox2.Checked = false;
            }
            else
            {
                serialPort1.Close();
                checkBox2.Text = "开始采集";
                btQCalc.Enabled = true;
                timer2.Enabled = false;
            }
        }
        float press, QFlow;
        private void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Enabled = false;
            CP68Packet ret = CP68Packet.CP68ComProc(ref serialPort1, CommandConst.CP68_UserCommand_ReadPress, null);
            this.Invoke((EventHandler)(delegate
            {
                if (ret != null)
                {
                    press = BitConverter.ToSingle(ret.Data, 0);
                    QFlow = BitConverter.ToSingle(ret.Data, 4);
                    listView2.Items[0].SubItems[1].Text = press.ToString("F2");
                    listView2.Items[1].SubItems[1].Text = QFlow.ToString("F2");

                    progressBar1.PerformStep();
                    if (progressBar1.Value == progressBar1.Maximum)
                        progressBar1.Value = progressBar1.Minimum;
                }
            }));
            timer2.Enabled = true;
        }
        private void btQ1_Click(object sender, EventArgs e)
        {
            listView2.Items[3].SubItems[1].Text = press.ToString("F2");
        }

        private void btQ2_Click(object sender, EventArgs e)
        {
            listView2.Items[4].SubItems[1].Text = press.ToString("F2");
        }

        private void btQ3_Click(object sender, EventArgs e)
        {
            listView2.Items[5].SubItems[1].Text = press.ToString("F2");
        }

        private void btQ4_Click(object sender, EventArgs e)
        {
            listView2.Items[6].SubItems[1].Text = press.ToString("F2");
        }
        private void btQCalc_Click(object sender, EventArgs e)
        {
            double x1, x2, x3, x4;
            x1 = double.Parse(listView2.Items[3].SubItems[1].Text);
            x2 = double.Parse(listView2.Items[4].SubItems[1].Text);
            x3 = double.Parse(listView2.Items[5].SubItems[1].Text);
            x4 = double.Parse(listView2.Items[6].SubItems[1].Text);
            double[] x = new double[25]{
                                    1,0,0,0,0,
                                    1,x1,x1*x1,x1*x1*x1,x1*x1*x1*x1,
                                    1,x2,x2*x2,x2*x2*x2,x2*x2*x2*x2,
                                    1,x3,x3*x3,x3*x3*x3,x3*x3*x3*x3,
                                    1,x4,x4*x4,x4*x4*x4,x4*x4*x4*x4,
                                    };
            double[] Q = new double[5] { 0,70, 350, 3500, 7000 };
            Matrix A = Matrix.Create(5, 5, x);
            GaussElimination ge = new GaussElimination(A, Q);
//             double[] Q1 = new double[4];
//             Q1[0] = ge.X[0] + ge.X[1] * x1 + ge.X[2] * x1 * x1 + ge.X[3] * x1 * x1 * x1;
//             Q1[1] = ge.X[0] + ge.X[1] * x2 + ge.X[2] * x2 * x2 + ge.X[3] * x2 * x2 * x2;
//             Q1[2] = ge.X[0] + ge.X[1] * x3 + ge.X[2] * x3 * x3 + ge.X[3] * x3 * x3 * x3;
//             Q1[3] = ge.X[0] + ge.X[1] * x4 + ge.X[2] * x4 * x4 + ge.X[3] * x4 * x4 * x4;
            if (!WFNetLib.WFGlobal.OpenSerialPort(ref serialPort1, "调试串口"))
                return;
            byte[] a0B = BitConverter.GetBytes(((float)ge.X[1]));
            byte[] a1B = BitConverter.GetBytes(((float)ge.X[2]));
            byte[] a2B = BitConverter.GetBytes(((float)ge.X[3]));
            byte[] a3B = BitConverter.GetBytes(((float)ge.X[4]));

            byte[] tx = new byte[16];
            for (int i = 0; i < 4; i++)
            {
                tx[i] = a0B[i];
                tx[4 + i] = a1B[i];
                tx[8 + i] = a2B[i];
                tx[12 + i] = a3B[i];
            }
            CP68Packet ret = CP68Packet.CP68ComProc(ref serialPort1, CommandConst.CP68_UserCommand_WriteQCalc, tx);
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
