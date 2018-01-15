using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CMOSTestLib;
using CMOSTestLib.SaperaGUI;
using System.Threading;

namespace 采集测试
{
    public partial class Form1 : Form
    {        
        private void TestStep2(int number, bool trash)
        {
            String str;
            if (trash)
            {
                str = String.Format("Frames acquired in trash buffer: {0}", number * m_Buffers.Count);
                this.StatusLabelInfoTrash.Text = str;
                bTrask = true;
            }
            else
            {
                rxFrame++;
                lock (oWaitMsTime)
                {
                    WaitMsTime = 0;
                }
                //textBox1.AppendText((m_Buffers.Index + 1).ToString() + "\r\n");
                str = String.Format("Frames acquired :{0}", number * m_Buffers.Count);
                this.StatusLabelInfo.Text = str;
            }
        }

        void 第二步明场采集(object LockWatingThread)
        {
            byte[] yByteArray;
            rxFrame = 0;
            uint ls =SystemParam.eStart + (uint)Calc1.Saturated50Index * SystemParam.eStep;
            Calc2.y50 = new double[m_Buffers.Height, m_Buffers.Width];

            int nCount=SystemParam.L/SystemParam.Step2_len;
            this.Invoke((EventHandler)(delegate
            {
                textBox1.AppendText("开始50%曝光明场采集\r\n");
            }));
            //NopCam((ushort)(SystemParam.Step2_len + 1), ls);
//             waitProc.SetProcessBar(SystemParam.Step2_len);
            int saveindex = 0;
            for (int i = 0; i < nCount; i++)
            {
                if (waitProc.HasBeenCancelled())
                {
                    return;
                }
                SerialFunc.SerialCommand3((ushort)(SystemParam.Step2_len + CamEx), ls);
                if (!WaitCam(SystemParam.Step2_len + CamEx))
                {
                    i--;
                    continue;
                }
                for (int j = 0; j < SystemParam.Step2_len; j++)
                {
                    saveindex++;
                    m_Buffers.Save(Calc2.TempPicPath_Light + saveindex.ToString() + ".bmp", "-format bmp", j + CamEx, 0);
                    Thread.Sleep(SystemParam.PicDelay);
                }
                for (int j = 0; j < SystemParam.Step2_len;j++ )
                {
                    yByteArray = wfSapGUI.ReadPicDatas(m_Buffers, j + CamEx);
                    SystemParam.WriteTempFile(yByteArray, i * SystemParam.Step2_len + j, Calc2.LightTempFile);
                }
                this.Invoke((EventHandler)(delegate
                {
                    listView1.Items[12].SubItems[1].Text = (((double)(i + 1) * SystemParam.Step2_len) * 100 / SystemParam.L).ToString("F1") + "%";
                }));
                waitProc.SetProcessBar((int)((i + 1) * SystemParam.Step2_len));
            }
            if (nCount * SystemParam.Step2_len < SystemParam.L)//还有一个
            {
                while(true)
                {
                    int left=SystemParam.L - nCount * SystemParam.Step2_len;
                    rxFrame = 0;
                    SerialFunc.SerialCommand3((ushort)(SystemParam.Step2_len + CamEx), ls);
                    if (!WaitCam(SystemParam.Step2_len + CamEx))
                    {                        
                        continue;
                    }
                    for (int j = 0; j < SystemParam.Step2_len; j++)
                    {
                        saveindex++;
                        m_Buffers.Save(Calc2.TempPicPath_Light + saveindex.ToString() + ".bmp", "-format bmp", j + CamEx, 0);
                        Thread.Sleep(SystemParam.PicDelay);
                    }
                    for (int j = 0; j < left; j++)
                    {
                        yByteArray = wfSapGUI.ReadPicDatas(m_Buffers, j + CamEx);
                        SystemParam.WriteTempFile(yByteArray, nCount * SystemParam.Step2_len + j,Calc2.LightTempFile);
                    }
                    break;
                }                              
            }
            waitProc.SetProcessBar(100);  
            this.Invoke((EventHandler)(delegate
            {
                listView1.Items[12].SubItems[1].Text = "完成";
                textBox1.AppendText("50%曝光明场采集完成\r\n");
            }));
        }
        void 第二步暗场采集(object LockWatingThread)
        {
            byte[] yByteArray;
            rxFrame = 0;
            uint ls = SystemParam.eStart + (uint)Calc1.Saturated50Index * SystemParam.eStep;

            int nCount = SystemParam.L / SystemParam.Step2_len;
            this.Invoke((EventHandler)(delegate
            {
                textBox1.AppendText("开始50%曝光暗场采集\r\n");
            }));
            //NopCam((ushort)(SystemParam.Step2_len + 1), ls);
//             waitProc.SetProcessBar(SystemParam.Step2_len);
            int saveindex = 0;
            for (int i = 0; i < nCount; i++)
            {
                if (waitProc.HasBeenCancelled())
                {
                    return;
                }
                SerialFunc.SerialCommand3((ushort)(SystemParam.Step2_len+CamEx), ls);
                if (!WaitCam(SystemParam.Step2_len + CamEx))
                {
                    i--;
                    continue;
                }
                for (int j = 0; j < SystemParam.Step2_len; j++)
                {
                    saveindex++;
                    m_Buffers.Save(Calc2.TempPicPath_Dark + saveindex.ToString() + ".bmp", "-format bmp", j + CamEx, 0);
                    Thread.Sleep(SystemParam.PicDelay);
                }
                for (int j = 0; j < SystemParam.Step2_len; j++)
                {
                    yByteArray = wfSapGUI.ReadPicDatas(m_Buffers, j + CamEx);
                    SystemParam.WriteTempFile(yByteArray, i * SystemParam.Step2_len + j, Calc2.DarkTempFile);
                }
                waitProc.SetProcessBar((int)((i + 1) * SystemParam.Step2_len));
                this.Invoke((EventHandler)(delegate
                {
                    listView1.Items[13].SubItems[1].Text = (((double)(i + 1) * SystemParam.Step2_len) * 100 / SystemParam.L).ToString("F1") + "%";
                }));
            }
            if (nCount * SystemParam.Step2_len < SystemParam.L)//还有一个
            {
                while (true)
                {
                    int left = SystemParam.L - nCount * SystemParam.Step2_len;
                    SerialFunc.SerialCommand3((ushort)(SystemParam.Step2_len + CamEx), ls);
                    if (!WaitCam(SystemParam.Step2_len + CamEx))
                    {
                        continue;
                    }
                    for (int j = 0; j < left; j++)
                    {
                        saveindex++;
                        m_Buffers.Save(Calc2.TempPicPath_Dark + saveindex.ToString() + ".bmp", "-format bmp", j + CamEx, 0);
                        Thread.Sleep(SystemParam.PicDelay);
                    }
                    for (int j = 0; j < left; j++)
                    {
                        yByteArray = wfSapGUI.ReadPicDatas(m_Buffers, j + CamEx);
                        SystemParam.WriteTempFile(yByteArray, nCount * SystemParam.Step2_len + j, Calc2.DarkTempFile);
                    }
                    break;
                }
            }
            waitProc.SetProcessBar(100);
            this.Invoke((EventHandler)(delegate
            {
                listView1.Items[13].SubItems[1].Text = "完成";
                textBox1.AppendText("50%曝光暗场采集完成\r\n");
            }));
        }
        void 第二步数据处理(object LockWatingThread)
        {
            int count = SystemParam.L * 4;
            /************************************************************************/
            /* y50                                                                  */
            /************************************************************************/
            waitProc.SetTitle("相同曝光条件下数据处理---计算明场像素点均值");
            Calc2.y50 = new double[m_Buffers.Height, m_Buffers.Width];
            for (int i = 0; i < SystemParam.L; i++)
            {
                byte[] p = SystemParam.ReadTempFile(SystemParam.ByteLen4Pic, i,Calc2.LightTempFile);
                ushort[,] pic = wfSapGUI.TransPicDatas(p, m_Buffers.Height, m_Buffers.Width,SystemParam.cmosInfo.PixelDepth);
                for (int m = 0; m < m_Buffers.Height; m++)
                {
                    for (int n = 0; n < m_Buffers.Width; n++)
                    {
                        Calc2.y50[m, n] += pic[m, n];
                    }
                }
                if (waitProc.HasBeenCancelled())
                {
                    return;
                }
                waitProc.SetProcessBar((int)((i + 1)));
                this.Invoke((EventHandler)(delegate
                {
                    listView1.Items[14].SubItems[1].Text = (((double)(i+1)) * 100 / count).ToString("F1") + "%";
                }));
            }
            for (int m = 0; m < m_Buffers.Height; m++)
            {
                for (int n = 0; n < m_Buffers.Width; n++)
                {
                    Calc2.y50[m, n] = Calc2.y50[m, n] / SystemParam.L;
                }                
            }
            /************************************************************************/
            /* y50_dark                                                             */
            /************************************************************************/
            waitProc.SetTitle("相同曝光条件下数据处理---计算暗场像素点均值");
            waitProc.SetProcessBar(0);
            Calc2.y50_dark = new double[m_Buffers.Height, m_Buffers.Width];
            for (int i = 0; i < SystemParam.L; i++)
            {
                byte[] p = SystemParam.ReadTempFile(SystemParam.ByteLen4Pic, i ,Calc2.DarkTempFile);
                ushort[,] pic = wfSapGUI.TransPicDatas(p, m_Buffers.Height, m_Buffers.Width,SystemParam.cmosInfo.PixelDepth);
                for (int m = 0; m < m_Buffers.Height; m++)
                {
                    for (int n = 0; n < m_Buffers.Width; n++)
                    {
                        Calc2.y50_dark[m, n] += pic[m, n];
                    }
                }
                if (waitProc.HasBeenCancelled())
                {
                    return;
                }
                waitProc.SetProcessBar((int)((i + 1)));
                this.Invoke((EventHandler)(delegate
                {
                    listView1.Items[14].SubItems[1].Text = (((double)(i+1)+SystemParam.L) * 100 / count).ToString("F1") + "%";
                }));
            }
            for (int m = 0; m < m_Buffers.Height; m++)
            {
                for (int n = 0; n < m_Buffers.Width; n++)
                {
                    Calc2.y50_dark[m, n] = Calc2.y50_dark[m, n] / SystemParam.L;
                }
            }
            /************************************************************************/
            /* miu_y50                                                              */
            /************************************************************************/
            Calc2.miu_y50 = 0;
            for (int m = 0; m < m_Buffers.Height; m++)
            {
                for (int n = 0; n < m_Buffers.Width; n++)
                {
                    Calc2.miu_y50 += Calc2.y50[m, n];
                }
            }
            Calc2.miu_y50 = Calc2.miu_y50 / m_Buffers.Height / m_Buffers.Width;

            /************************************************************************/
            /* miu_y50_dark                                                         */
            /************************************************************************/
            Calc2.miu_y50_dark = 0;
            for (int m = 0; m < m_Buffers.Height; m++)
            {
                for (int n = 0; n < m_Buffers.Width; n++)
                {
                    Calc2.miu_y50_dark += Calc2.y50_dark[m, n];
                }
            }
            Calc2.miu_y50_dark = Calc2.miu_y50_dark / m_Buffers.Height / m_Buffers.Width;

            /************************************************************************/
            /* delta_y50                                                            */
            /************************************************************************/
            waitProc.SetTitle("相同曝光条件下数据处理---计算明场像素点方差");
            waitProc.SetProcessBar(0);
            Calc2.delta_y50 = new double[m_Buffers.Height, m_Buffers.Width];
            for (int i = 0; i < SystemParam.L; i++)
            {
                byte[] p = SystemParam.ReadTempFile(SystemParam.ByteLen4Pic, i,Calc2.LightTempFile);
                ushort[,] pic = wfSapGUI.TransPicDatas(p, m_Buffers.Height, m_Buffers.Width, SystemParam.cmosInfo.PixelDepth);
                for (int m = 0; m < m_Buffers.Height; m++)
                {
                    for (int n = 0; n < m_Buffers.Width; n++)
                    {
                        Calc2.delta_y50[m, n] += (pic[m, n] - Calc2.y50[m, n]) * (pic[m, n] - Calc2.y50[m, n]);
                    }
                }
                if (waitProc.HasBeenCancelled())
                {
                    return;
                }
                waitProc.SetProcessBar((int)((i + 1)));
                this.Invoke((EventHandler)(delegate
                {
                    listView1.Items[14].SubItems[1].Text = (((double)(i + 1) + SystemParam.L*2) * 100 / count).ToString("F1") + "%";
                }));
            }
            for (int m = 0; m < m_Buffers.Height; m++)
            {
                for (int n = 0; n < m_Buffers.Width; n++)
                {
                    Calc2.delta_y50[m, n] = Calc2.delta_y50[m, n] / (SystemParam.L - 1);
                }
            }
            /************************************************************************/
            /* delta_y50_dark                                                       */
            /************************************************************************/
            waitProc.SetTitle("相同曝光条件下数据处理---计算暗场像素点方差");
            waitProc.SetProcessBar(0);
            Calc2.delta_y50_dark = new double[m_Buffers.Height, m_Buffers.Width];
            for (int i = 0; i < SystemParam.L; i++)
            {
                byte[] p = SystemParam.ReadTempFile(SystemParam.ByteLen4Pic, i,Calc2.DarkTempFile);
                ushort[,] pic = wfSapGUI.TransPicDatas(p, m_Buffers.Height, m_Buffers.Width, SystemParam.cmosInfo.PixelDepth);
                for (int m = 0; m < m_Buffers.Height; m++)
                {
                    for (int n = 0; n < m_Buffers.Width; n++)
                    {
                        Calc2.delta_y50_dark[m, n] += (pic[m, n] - Calc2.y50_dark[m, n]) * (pic[m, n] - Calc2.y50_dark[m, n]);
                    }
                }
                if (waitProc.HasBeenCancelled())
                {
                    this.Invoke((EventHandler)(delegate
                    {
                        textBox1.AppendText("用户终止自动测试\r\n");
                        return;
                    }));
                }
                waitProc.SetProcessBar((int)((i + 1)));
                this.Invoke((EventHandler)(delegate
                {
                    listView1.Items[14].SubItems[1].Text = (((double)(i + 1) + SystemParam.L*3) * 100 / count).ToString("F1") + "%";
                }));
            }
            for (int m = 0; m < m_Buffers.Height; m++)
            {
                for (int n = 0; n < m_Buffers.Width; n++)
                {
                    Calc2.delta_y50_dark[m, n] = Calc2.delta_y50_dark[m, n] / (SystemParam.L - 1);
                }
            }

            /************************************************************************/
            /* delta_y50_stack                                                             */
            /************************************************************************/
            waitProc.SetTitle("相同曝光条件下数据处理---计算PRUN、DSUN、亮点、暗点");
            waitProc.SetProcessBar(0);
            waitProc.SetProcessBarRange(0,7);
            Calc2.delta_y50_stack = 0;
            for (int m = 0; m < m_Buffers.Height; m++)
            {
                for (int n = 0; n < m_Buffers.Width; n++)
                {
                    Calc2.delta_y50_stack += Calc2.delta_y50[m, n];
                }
            }
            Calc2.delta_y50_stack = Calc2.delta_y50_stack / m_Buffers.Height / m_Buffers.Width;
            waitProc.SetProcessBarPerformStep();            
            /************************************************************************/
            /* delta_y50_dark_stack                                                         */
            /************************************************************************/
            Calc2.delta_y50_dark_stack = 0;
            for (int m = 0; m < m_Buffers.Height; m++)
            {
                for (int n = 0; n < m_Buffers.Width; n++)
                {
                    Calc2.delta_y50_dark_stack += Calc2.delta_y50_dark[m, n];
                }
            }
            Calc2.delta_y50_dark_stack = Calc2.delta_y50_dark_stack / m_Buffers.Height / m_Buffers.Width;
            waitProc.SetProcessBarPerformStep();
            /************************************************************************/
            /* S2_y50                                                               */
            /************************************************************************/
            Calc2.S2_y50 = 0;
            for (int m = 0; m < m_Buffers.Height; m++)
            {
                for (int n = 0; n < m_Buffers.Width; n++)
                {
                    Calc2.S2_y50 += (Calc2.y50[m, n] - Calc2.miu_y50) * (Calc2.y50[m, n] - Calc2.miu_y50);
                }
            }
            Calc2.S2_y50 = Calc2.S2_y50 / (m_Buffers.Height * m_Buffers.Width - 1);
            Calc2.S2_y50 = Calc2.S2_y50 - Calc2.delta_y50_stack / SystemParam.L;
            waitProc.SetProcessBarPerformStep();
            /************************************************************************/
            /* S2_y50_dark                                                          */
            /************************************************************************/
            Calc2.S2_y50_dark = 0;
            for (int m = 0; m < m_Buffers.Height; m++)
            {
                for (int n = 0; n < m_Buffers.Width; n++)
                {
                    Calc2.S2_y50_dark += (Calc2.y50_dark[m, n] - Calc2.miu_y50_dark) * (Calc2.y50_dark[m, n] - Calc2.miu_y50_dark);
                }
            }
            Calc2.S2_y50_dark = Calc2.S2_y50_dark / (m_Buffers.Height * m_Buffers.Width - 1);
            Calc2.S2_y50_dark = Calc2.S2_y50_dark - Calc2.delta_y50_dark_stack / SystemParam.L;
            waitProc.SetProcessBarPerformStep();
            /************************************************************************/
            /* DSNU1288 ,PRNU1288                                                   */
            /************************************************************************/
            Calc2.DSNU1288 = Math.Sqrt(Calc2.S2_y50_dark) / Calc1.OverAllGain_K;

            Calc2.PRNU1288 = Math.Sqrt(Math.Abs(Calc2.S2_y50 - Calc2.S2_y50_dark)) / (Calc2.miu_y50 - Calc2.miu_y50_dark);
            this.Invoke((EventHandler)(delegate
            {
                listView1.Items[14].SubItems[1].Text = "完成";
                listView1.Items[15].SubItems[1].Text = "完成";

                listView1.Items[14].SubItems[2].Text = Calc2.DSNU1288.ToString("F2");
                listView1.Items[15].SubItems[2].Text = Calc2.PRNU1288.ToString("F4");
                textBox1.AppendText("DSNU1288 ,PRNU1288计算完成\r\n");
            }));
            waitProc.SetProcessBarPerformStep();
            /************************************************************************/
            /* 暗点查找                                                           */
            /************************************************************************/
            double dP = Calc2.miu_y50 * SystemParam.DarkPointPer / 100;
            Calc2.DarkPoints = new List<PixelInfo>();
            for (int m = 0; m < m_Buffers.Height; m++)
            {
                for (int n = 0; n < m_Buffers.Width; n++)
                {
                    if (Calc2.y50[m, n] < dP)
                    {
                        PixelInfo p = new PixelInfo();
                        p.row = m;
                        p.col = n;
                        p.y = Calc2.y50[m, n];
                        Calc2.DarkPoints.Add(p);
                    }
                }
            }
            waitProc.SetProcessBarPerformStep();
            this.Invoke((EventHandler)(delegate
            {
                listView1.Items[16].SubItems[1].Text = "完成";
                textBox1.AppendText("暗点查找完成\r\n");
            }));
            /************************************************************************/
            /* 明点查找                                                           */
            /************************************************************************/
            double lP = Calc2.miu_y50_dark * (100+SystemParam.LightPointPer) / 100;
            Calc2.LightPoints = new List<PixelInfo>();
            for (int m = 0; m < m_Buffers.Height; m++)
            {
                for (int n = 0; n < m_Buffers.Width; n++)
                {
                    if (Calc2.y50_dark[m, n] > lP)
                    {
                        PixelInfo p = new PixelInfo();
                        p.row = m;
                        p.col = n;
                        p.y = Calc2.y50_dark[m, n];
                        Calc2.LightPoints.Add(p);
                    }
                }
            }
            waitProc.SetProcessBarPerformStep();
            this.Invoke((EventHandler)(delegate
            {
                listView1.Items[17].SubItems[1].Text = "完成";
                textBox1.AppendText("亮点查找完成\r\n");
            }));
        }
        void RGB_第二步数据处理(object LockWatingThread)
        {
            int count = SystemParam.L * 4;
            ushort[] yR;
            ushort[] yG;
            ushort[] yB;
            byte[] p = SystemParam.ReadTempFile(SystemParam.ByteLen4Pic, 0, Calc2.LightTempFile);
            wfSapGUI.TransPicDatas(p, m_Buffers.Height, m_Buffers.Width, SystemParam.cmosInfo.PixelDepth,
                SystemParam.cmosInfo.RGB1, SystemParam.cmosInfo.RGB2, SystemParam.cmosInfo.RGB3, SystemParam.cmosInfo.RGB4,
                out yR, out yG, out yB);
            Calc2.R_y50 = new double[yR.Length];
            Calc2.G_y50 = new double[yG.Length];
            Calc2.B_y50 = new double[yB.Length];
            Calc2.R_y50_dark = new double[yR.Length];
            Calc2.G_y50_dark = new double[yG.Length];
            Calc2.B_y50_dark = new double[yB.Length];
            Calc2.R_delta_y50 = new double[yR.Length];
            Calc2.G_delta_y50 = new double[yG.Length];
            Calc2.B_delta_y50 = new double[yB.Length];
            Calc2.R_delta_y50_dark = new double[yR.Length];
            Calc2.G_delta_y50_dark = new double[yG.Length];
            Calc2.B_delta_y50_dark = new double[yB.Length];
            /************************************************************************/
            /* y50                                                                  */
            /************************************************************************/
            waitProc.SetTitle("相同曝光条件下数据处理---计算明场像素点均值");
            for (int i = 0; i < SystemParam.L; i++)
            {
                p = SystemParam.ReadTempFile(SystemParam.ByteLen4Pic, i, Calc2.LightTempFile);
                wfSapGUI.TransPicDatas(p, m_Buffers.Height, m_Buffers.Width, SystemParam.cmosInfo.PixelDepth, 
                    SystemParam.cmosInfo.RGB1, SystemParam.cmosInfo.RGB2, SystemParam.cmosInfo.RGB3, SystemParam.cmosInfo.RGB4,
                    out yR, out yG, out yB);
                for (int j = 0; j < yR.Length; j++)
                {
                    Calc2.R_y50[j] += yR[j];
                }
                for (int j = 0; j < yG.Length; j++)
                {
                    Calc2.G_y50[j] += yG[j];
                }
                for (int j = 0; j < yB.Length; j++)
                {
                    Calc2.B_y50[j] += yB[j];
                }
                if (waitProc.HasBeenCancelled())
                {
                    return;
                }
                waitProc.SetProcessBar((int)((i + 1)));
                this.Invoke((EventHandler)(delegate
                {
                    listView1.Items[14].SubItems[1].Text = (((double)(i + 1)) * 100 / count).ToString("F1") + "%";
                }));
            }
            for (int j = 0; j < Calc2.R_y50.Length; j++)
            {
                Calc2.R_y50[j] = Calc2.R_y50[j]/SystemParam.L;
            }
            for (int j = 0; j < Calc2.G_y50.Length; j++)
            {
                Calc2.G_y50[j] = Calc2.G_y50[j] / SystemParam.L;
            }
            for (int j = 0; j < Calc2.B_y50.Length; j++)
            {
                Calc2.B_y50[j] = Calc2.B_y50[j] / SystemParam.L;
            }            
            /************************************************************************/
            /* y50_dark                                                             */
            /************************************************************************/
            waitProc.SetTitle("相同曝光条件下数据处理---计算暗场像素点均值");
            waitProc.SetProcessBar(0);
            Calc2.y50_dark = new double[m_Buffers.Height, m_Buffers.Width];
            for (int i = 0; i < SystemParam.L; i++)
            {
                p = SystemParam.ReadTempFile(SystemParam.ByteLen4Pic, i, Calc2.DarkTempFile);
                wfSapGUI.TransPicDatas(p, m_Buffers.Height, m_Buffers.Width, SystemParam.cmosInfo.PixelDepth,
                    SystemParam.cmosInfo.RGB1, SystemParam.cmosInfo.RGB2, SystemParam.cmosInfo.RGB3, SystemParam.cmosInfo.RGB4,
                    out yR, out yG, out yB);
                for (int j = 0; j < yR.Length; j++)
                {
                    Calc2.R_y50_dark[j] += yR[j];
                }
                for (int j = 0; j < yG.Length; j++)
                {
                    Calc2.G_y50_dark[j] += yG[j];
                }
                for (int j = 0; j < yB.Length; j++)
                {
                    Calc2.B_y50_dark[j] += yB[j];
                }
                if (waitProc.HasBeenCancelled())
                {
                    return;
                }
                waitProc.SetProcessBar((int)((i + 1)));
                this.Invoke((EventHandler)(delegate
                {
                    listView1.Items[14].SubItems[1].Text = (((double)(i + 1) + SystemParam.L) * 100 / count).ToString("F1") + "%";
                }));
            }
            for (int j = 0; j < Calc2.R_y50.Length; j++)
            {
                Calc2.R_y50_dark[j] = Calc2.R_y50_dark[j] / SystemParam.L;
            }
            for (int j = 0; j < Calc2.G_y50.Length; j++)
            {
                Calc2.G_y50_dark[j] = Calc2.G_y50_dark[j] / SystemParam.L;
            }
            for (int j = 0; j < Calc2.B_y50.Length; j++)
            {
                Calc2.B_y50_dark[j] = Calc2.B_y50_dark[j] / SystemParam.L;
            }    
            /************************************************************************/
            /* miu_y50                                                              */
            /************************************************************************/
            Calc2.R_miu_y50 = 0;
            for (int j = 0; j < Calc2.R_y50.Length; j++)
            {
                Calc2.R_miu_y50 += Calc2.R_y50[j];
            }
            Calc2.R_miu_y50 = Calc2.R_miu_y50 / Calc2.R_y50.Length;

            Calc2.G_miu_y50 = 0;
            for (int j = 0; j < Calc2.G_y50.Length; j++)
            {
                Calc2.G_miu_y50 += Calc2.G_y50[j];
            }
            Calc2.G_miu_y50 = Calc2.G_miu_y50 / Calc2.G_y50.Length;

            Calc2.B_miu_y50 = 0;
            for (int j = 0; j < Calc2.B_y50.Length; j++)
            {
                Calc2.B_miu_y50 += Calc2.B_y50[j];
            }
            Calc2.B_miu_y50 = Calc2.B_miu_y50 / Calc2.B_y50.Length;
            /************************************************************************/
            /* miu_y50_dark                                                         */
            /************************************************************************/
            Calc2.R_miu_y50_dark = 0;
            for (int j = 0; j < Calc2.R_y50_dark.Length; j++)
            {
                Calc2.R_miu_y50_dark += Calc2.R_y50_dark[j];
            }
            Calc2.R_miu_y50_dark = Calc2.R_miu_y50_dark / Calc2.R_y50_dark.Length;

            Calc2.G_miu_y50_dark = 0;
            for (int j = 0; j < Calc2.G_y50_dark.Length; j++)
            {
                Calc2.G_miu_y50_dark += Calc2.G_y50_dark[j];
            }
            Calc2.G_miu_y50_dark = Calc2.G_miu_y50_dark / Calc2.G_y50_dark.Length;

            Calc2.B_miu_y50_dark = 0;
            for (int j = 0; j < Calc2.B_y50_dark.Length; j++)
            {
                Calc2.B_miu_y50_dark += Calc2.B_y50_dark[j];
            }
            Calc2.B_miu_y50_dark = Calc2.B_miu_y50_dark / Calc2.B_y50_dark.Length;

            /************************************************************************/
            /* delta_y50                                                            */
            /************************************************************************/
            waitProc.SetTitle("相同曝光条件下数据处理---计算明场像素点方差");
            waitProc.SetProcessBar(0);
            for (int i = 0; i < SystemParam.L; i++)
            {
                p = SystemParam.ReadTempFile(SystemParam.ByteLen4Pic, i, Calc2.LightTempFile);
                wfSapGUI.TransPicDatas(p, m_Buffers.Height, m_Buffers.Width, SystemParam.cmosInfo.PixelDepth,
                    SystemParam.cmosInfo.RGB1, SystemParam.cmosInfo.RGB2, SystemParam.cmosInfo.RGB3, SystemParam.cmosInfo.RGB4,
                    out yR, out yG, out yB);
                for (int j = 0; j < yR.Length; j++)
                {
                    Calc2.R_delta_y50[j] += (yR[j] - Calc2.R_y50[j]) * (yR[j] - Calc2.R_y50[j]);
                }
                for (int j = 0; j < yG.Length; j++)
                {
                    Calc2.G_delta_y50[j] += (yG[j] - Calc2.G_y50[j]) * (yG[j] - Calc2.G_y50[j]);
                }
                for (int j = 0; j < yB.Length; j++)
                {
                    Calc2.B_delta_y50[j] += (yB[j] - Calc2.B_y50[j]) * (yB[j] - Calc2.B_y50[j]);
                }                
                if (waitProc.HasBeenCancelled())
                {
                    return;
                }
                waitProc.SetProcessBar((int)((i + 1)));
                this.Invoke((EventHandler)(delegate
                {
                    listView1.Items[14].SubItems[1].Text = (((double)(i + 1) + SystemParam.L * 2) * 100 / count).ToString("F1") + "%";
                }));
            }
            for (int j = 0; j < Calc2.R_y50.Length; j++)
            {
                Calc2.R_delta_y50[j] = Calc2.R_delta_y50[j] / SystemParam.L;
            }
            for (int j = 0; j < Calc2.G_y50.Length; j++)
            {
                Calc2.G_delta_y50[j] = Calc2.G_delta_y50[j] / SystemParam.L;
            }
            for (int j = 0; j < Calc2.B_y50.Length; j++)
            {
                Calc2.B_delta_y50[j] = Calc2.B_delta_y50[j] / SystemParam.L;
            }    
            /************************************************************************/
            /* delta_y50_dark                                                       */
            /************************************************************************/
            waitProc.SetTitle("相同曝光条件下数据处理---计算暗场像素点方差");
            waitProc.SetProcessBar(0);
            Calc2.delta_y50_dark = new double[m_Buffers.Height, m_Buffers.Width];
            for (int i = 0; i < SystemParam.L; i++)
            {
                p = SystemParam.ReadTempFile(SystemParam.ByteLen4Pic, i, Calc2.DarkTempFile);
                wfSapGUI.TransPicDatas(p, m_Buffers.Height, m_Buffers.Width, SystemParam.cmosInfo.PixelDepth,
                    SystemParam.cmosInfo.RGB1, SystemParam.cmosInfo.RGB2, SystemParam.cmosInfo.RGB3, SystemParam.cmosInfo.RGB4,
                    out yR, out yG, out yB);
                for (int j = 0; j < yR.Length; j++)
                {
                    Calc2.R_delta_y50_dark[j] += (yR[j] - Calc2.R_y50_dark[j]) * (yR[j] - Calc2.R_y50_dark[j]);
                }
                for (int j = 0; j < yG.Length; j++)
                {
                    Calc2.G_delta_y50_dark[j] += (yG[j] - Calc2.G_y50_dark[j]) * (yG[j] - Calc2.G_y50_dark[j]);
                }
                for (int j = 0; j < yB.Length; j++)
                {
                    Calc2.B_delta_y50_dark[j] += (yB[j] - Calc2.B_y50_dark[j]) * (yB[j] - Calc2.B_y50_dark[j]);
                } 
                if (waitProc.HasBeenCancelled())
                {
                    this.Invoke((EventHandler)(delegate
                    {
                        textBox1.AppendText("用户终止自动测试\r\n");
                        return;
                    }));
                }
                waitProc.SetProcessBar((int)((i + 1)));
                this.Invoke((EventHandler)(delegate
                {
                    listView1.Items[14].SubItems[1].Text = (((double)(i + 1) + SystemParam.L * 3) * 100 / count).ToString("F1") + "%";
                }));
            }
            for (int j = 0; j < Calc2.R_y50_dark.Length; j++)
            {
                Calc2.R_delta_y50_dark[j] = Calc2.R_delta_y50_dark[j] / SystemParam.L;
            }
            for (int j = 0; j < Calc2.G_y50_dark.Length; j++)
            {
                Calc2.G_delta_y50_dark[j] = Calc2.G_delta_y50_dark[j] / SystemParam.L;
            }
            for (int j = 0; j < Calc2.B_y50_dark.Length; j++)
            {
                Calc2.B_delta_y50_dark[j] = Calc2.B_delta_y50_dark[j] / SystemParam.L;
            }    

            /************************************************************************/
            /* delta_y50_stack                                                             */
            /************************************************************************/
            waitProc.SetTitle("相同曝光条件下数据处理---计算PRUN、DSUN、亮点、暗点");
            waitProc.SetProcessBar(0);
            waitProc.SetProcessBarRange(0, 7);
            Calc2.R_delta_y50_stack = 0;
            for (int j = 0; j < Calc2.R_y50.Length; j++)
            {
                Calc2.R_delta_y50_stack += Calc2.R_delta_y50[j];
            }
            Calc2.R_delta_y50_stack = Calc2.R_delta_y50_stack / Calc2.R_y50.Length;

            Calc2.G_delta_y50_stack = 0;
            for (int j = 0; j < Calc2.G_y50.Length; j++)
            {
                Calc2.G_delta_y50_stack += Calc2.G_delta_y50[j];
            }
            Calc2.G_delta_y50_stack = Calc2.G_delta_y50_stack / Calc2.G_y50.Length;

            Calc2.B_delta_y50_stack = 0;
            for (int j = 0; j < Calc2.B_y50.Length; j++)
            {
                Calc2.B_delta_y50_stack += Calc2.B_delta_y50[j];
            }
            Calc2.B_delta_y50_stack = Calc2.B_delta_y50_stack / Calc2.B_y50.Length;
            waitProc.SetProcessBarPerformStep();
            /************************************************************************/
            /* delta_y50_dark_stack                                                         */
            /************************************************************************/
            Calc2.R_delta_y50_dark_stack = 0;
            for (int j = 0; j < Calc2.R_y50_dark.Length; j++)
            {
                Calc2.R_delta_y50_dark_stack += Calc2.R_delta_y50_dark[j];
            }
            Calc2.R_delta_y50_dark_stack = Calc2.R_delta_y50_dark_stack / Calc2.R_y50_dark.Length;

            Calc2.G_delta_y50_dark_stack = 0;
            for (int j = 0; j < Calc2.G_y50_dark.Length; j++)
            {
                Calc2.G_delta_y50_dark_stack += Calc2.G_delta_y50_dark[j];
            }
            Calc2.G_delta_y50_dark_stack = Calc2.G_delta_y50_dark_stack / Calc2.G_y50_dark.Length;

            Calc2.B_delta_y50_dark_stack = 0;
            for (int j = 0; j < Calc2.B_y50_dark.Length; j++)
            {
                Calc2.B_delta_y50_dark_stack += Calc2.B_delta_y50_dark[j];
            }
            Calc2.B_delta_y50_dark_stack = Calc2.B_delta_y50_dark_stack / Calc2.B_y50_dark.Length;
            waitProc.SetProcessBarPerformStep();
            /************************************************************************/
            /* S2_y50                                                               */
            /************************************************************************/
            Calc2.R_S2_y50 = 0;
            for (int j = 0; j < Calc2.R_y50.Length; j++)
            {
                Calc2.R_S2_y50 += (Calc2.R_y50[j] - Calc2.R_miu_y50) * (Calc2.R_y50[j] - Calc2.R_miu_y50);
            }
            Calc2.R_S2_y50 = Calc2.R_S2_y50 / (Calc2.R_y50.Length - 1);
            Calc2.R_S2_y50 = Calc2.R_S2_y50 - Calc2.R_delta_y50_stack / SystemParam.L;

            Calc2.G_S2_y50 = 0;
            for (int j = 0; j < Calc2.G_y50.Length; j++)
            {
                Calc2.G_S2_y50 += (Calc2.G_y50[j] - Calc2.G_miu_y50) * (Calc2.G_y50[j] - Calc2.G_miu_y50);
            }
            Calc2.G_S2_y50 = Calc2.G_S2_y50 / (Calc2.G_y50.Length - 1);
            Calc2.G_S2_y50 = Calc2.G_S2_y50 - Calc2.G_delta_y50_stack / SystemParam.L;

            Calc2.B_S2_y50 = 0;
            for (int j = 0; j < Calc2.B_y50.Length; j++)
            {
                Calc2.B_S2_y50 += (Calc2.B_y50[j] - Calc2.B_miu_y50) * (Calc2.B_y50[j] - Calc2.B_miu_y50);
            }
            Calc2.B_S2_y50 = Calc2.B_S2_y50 / (Calc2.B_y50.Length - 1);
            Calc2.B_S2_y50 = Calc2.B_S2_y50 - Calc2.B_delta_y50_stack / SystemParam.L;
            waitProc.SetProcessBarPerformStep();
            /************************************************************************/
            /* S2_y50_dark                                                          */
            /************************************************************************/
            Calc2.R_S2_y50_dark = 0;
            for (int j = 0; j < Calc2.R_y50_dark.Length; j++)
            {
                Calc2.R_S2_y50_dark += (Calc2.R_y50_dark[j] - Calc2.R_miu_y50_dark) * (Calc2.R_y50_dark[j] - Calc2.R_miu_y50_dark);
            }
            Calc2.R_S2_y50_dark = Calc2.R_S2_y50_dark / (Calc2.R_y50_dark.Length - 1);
            Calc2.R_S2_y50_dark = Calc2.R_S2_y50_dark - Calc2.R_delta_y50_dark_stack / SystemParam.L;

            Calc2.G_S2_y50_dark = 0;
            for (int j = 0; j < Calc2.G_y50_dark.Length; j++)
            {
                Calc2.G_S2_y50_dark += (Calc2.G_y50_dark[j] - Calc2.G_miu_y50_dark) * (Calc2.G_y50_dark[j] - Calc2.G_miu_y50_dark);
            }
            Calc2.G_S2_y50_dark = Calc2.G_S2_y50_dark / (Calc2.G_y50_dark.Length - 1);
            Calc2.G_S2_y50_dark = Calc2.G_S2_y50_dark - Calc2.G_delta_y50_dark_stack / SystemParam.L;

            Calc2.B_S2_y50_dark = 0;
            for (int j = 0; j < Calc2.B_y50_dark.Length; j++)
            {
                Calc2.B_S2_y50_dark += (Calc2.B_y50_dark[j] - Calc2.B_miu_y50_dark) * (Calc2.B_y50_dark[j] - Calc2.B_miu_y50_dark);
            }
            Calc2.B_S2_y50_dark = Calc2.B_S2_y50_dark / (Calc2.B_y50_dark.Length - 1);
            Calc2.B_S2_y50_dark = Calc2.B_S2_y50_dark - Calc2.B_delta_y50_dark_stack / SystemParam.L;
            waitProc.SetProcessBarPerformStep();
            /************************************************************************/
            /* DSNU1288 ,PRNU1288                                                   */
            /************************************************************************/
            Calc2.R_DSNU1288 = Math.Sqrt(Calc2.R_S2_y50_dark) / Calc1.R_OverAllGain_K;
            Calc2.G_DSNU1288 = Math.Sqrt(Calc2.G_S2_y50_dark) / Calc1.G_OverAllGain_K;
            Calc2.B_DSNU1288 = Math.Sqrt(Calc2.B_S2_y50_dark) / Calc1.B_OverAllGain_K;

            Calc2.R_PRNU1288 = Math.Sqrt(Math.Abs(Calc2.R_S2_y50 - Calc2.R_S2_y50_dark)) / (Calc2.R_miu_y50 - Calc2.R_miu_y50_dark);
            Calc2.G_PRNU1288 = Math.Sqrt(Math.Abs(Calc2.G_S2_y50 - Calc2.G_S2_y50_dark)) / (Calc2.G_miu_y50 - Calc2.G_miu_y50_dark);
            Calc2.B_PRNU1288 = Math.Sqrt(Math.Abs(Calc2.B_S2_y50 - Calc2.B_S2_y50_dark)) / (Calc2.B_miu_y50 - Calc2.B_miu_y50_dark);
            this.Invoke((EventHandler)(delegate
            {
                listView1.Items[14].SubItems[1].Text = "完成";
                listView1.Items[15].SubItems[1].Text = "完成";

                listView1.Items[14].SubItems[2].Text = Calc2.R_DSNU1288.ToString("F2") + "||" + Calc2.G_DSNU1288.ToString("F2") + "||" + Calc2.B_DSNU1288.ToString("F2");
                listView1.Items[15].SubItems[2].Text = Calc2.R_PRNU1288.ToString("F4") + "||" + Calc2.G_PRNU1288.ToString("F4") + "||" + Calc2.B_PRNU1288.ToString("F4");
                textBox1.AppendText("DSNU1288 ,PRNU1288计算完成\r\n");
            }));
            waitProc.SetProcessBarPerformStep();
            /************************************************************************/
            /* 暗点查找                                                           */
            /************************************************************************/
            double dP = Calc2.R_miu_y50 * SystemParam.DarkPointPer / 100;
            Calc2.R_DarkPoints = new List<PixelInfo>();
            for (int j = 0; j < Calc2.R_y50.Length; j++)
            {
                if (Calc2.R_y50[j] < dP)
                {
                    PixelInfo pD = wfSapGUI.RPixel[j];                    
                    pD.y = Calc2.R_y50[j];
                    Calc2.R_DarkPoints.Add(pD);
                }
            }

            dP = Calc2.G_miu_y50 * SystemParam.DarkPointPer / 100;
            Calc2.G_DarkPoints = new List<PixelInfo>();
            for (int j = 0; j < Calc2.G_y50.Length; j++)
            {
                if (Calc2.G_y50[j] < dP)
                {
                    PixelInfo pD = wfSapGUI.GPixel[j];
                    pD.y = Calc2.G_y50[j];
                    Calc2.G_DarkPoints.Add(pD);
                }
            }

            dP = Calc2.B_miu_y50 * SystemParam.DarkPointPer / 100;
            Calc2.B_DarkPoints = new List<PixelInfo>();
            for (int j = 0; j < Calc2.B_y50.Length; j++)
            {
                if (Calc2.B_y50[j] < dP)
                {
                    PixelInfo pD = wfSapGUI.BPixel[j];
                    pD.y = Calc2.B_y50[j];
                    Calc2.B_DarkPoints.Add(pD);
                }
            }
            waitProc.SetProcessBarPerformStep();
            this.Invoke((EventHandler)(delegate
            {
                listView1.Items[16].SubItems[1].Text = "完成";
                textBox1.AppendText("暗点查找完成\r\n");
            }));
            /************************************************************************/
            /* 明点查找                                                           */
            /************************************************************************/
            double lP = Calc2.R_miu_y50_dark * (100 + SystemParam.LightPointPer) / 100;
            Calc2.R_LightPoints = new List<PixelInfo>();
            for (int j = 0; j < Calc2.R_y50.Length; j++)
            {
                if (Calc2.R_y50_dark[j] >lP)
                {
                    PixelInfo pD = wfSapGUI.RPixel[j];
                    pD.y = Calc2.R_y50_dark[j];
                    Calc2.R_LightPoints.Add(pD);
                }
            }

            lP = Calc2.G_miu_y50_dark * (100 + SystemParam.LightPointPer) / 100;
            Calc2.G_LightPoints = new List<PixelInfo>();
            for (int j = 0; j < Calc2.G_y50.Length; j++)
            {
                if (Calc2.G_y50_dark[j] > lP)
                {
                    PixelInfo pD = wfSapGUI.GPixel[j];
                    pD.y = Calc2.G_y50_dark[j];
                    Calc2.G_LightPoints.Add(pD);
                }
            }

            lP = Calc2.B_miu_y50_dark * (100 + SystemParam.LightPointPer) / 100;
            Calc2.B_LightPoints = new List<PixelInfo>();
            for (int j = 0; j < Calc2.B_y50.Length; j++)
            {
                if (Calc2.B_y50_dark[j] > lP)
                {
                    PixelInfo pD = wfSapGUI.BPixel[j];
                    pD.y = Calc2.B_y50_dark[j];
                    Calc2.B_LightPoints.Add(pD);
                }
            }
            waitProc.SetProcessBarPerformStep();
            this.Invoke((EventHandler)(delegate
            {
                listView1.Items[17].SubItems[1].Text = "完成";
                textBox1.AppendText("亮点查找完成\r\n");
            }));
        }
    }
}
