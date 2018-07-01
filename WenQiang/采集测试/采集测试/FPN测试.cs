using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CMOSTestLib;
using CMOSTestLib.SaperaGUI;
using DALSA.SaperaLT.SapClassBasic;
using WFNetLib.Algorithm;
using WFNetLib;
using System.Threading;
using WFNetLib.Log;
using System.IO;

namespace 采集测试
{
    public partial class Form1 : Form
    {
        private void FPNTest(int number, bool trash)
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
                    WaitMsTime=0;
                }
                //textBox1.AppendText((m_Buffers.Index + 1).ToString() + "\r\n");
                str = String.Format("Frames acquired :{0}", number * m_Buffers.Count);
                this.StatusLabelInfo.Text = str;
            }
        }
        public static string FPNFile_Path = System.Windows.Forms.Application.StartupPath + "\\FPN数据\\";
        int FPN_Per;
        int FPN_Ns;
        int FPN_L;
        int FPN_Len;
        private void FPN测试_Click(object sender, EventArgs e)
        {
            int saveindex = 0;
            string fileName = FPNFile_Path + "TempPic\\Light\\" + saveindex.ToString() + ".bmp";
            FileInfo f = new FileInfo(fileName);
            if (!Directory.Exists(f.DirectoryName))
                Directory.CreateDirectory(f.DirectoryName);
            fileName = FPNFile_Path + "TempData\\Light\\" + "1.tif";
            f = new FileInfo(fileName);
            if (!Directory.Exists(f.DirectoryName))
                Directory.CreateDirectory(f.DirectoryName);
            fileName = FPNFile_Path + "TempPic\\Dark\\" + "1.tif";
            f = new FileInfo(fileName);
            if (!Directory.Exists(f.DirectoryName))
                Directory.CreateDirectory(f.DirectoryName);
            fileName = FPNFile_Path + "TempData\\Dark\\" + "1.tif";
            f = new FileInfo(fileName);
            if (!Directory.Exists(f.DirectoryName))
                Directory.CreateDirectory(f.DirectoryName);

            iniFileOP.iniFilePath = System.Windows.Forms.Application.StartupPath + "\\Config.ini";
            FPN_Per = int.Parse(iniFileOP.Read("System Setting", "FPN_Per"));
            FPN_Ns = int.Parse(iniFileOP.Read("System Setting", "FPN_Ns"));
            FPN_L = int.Parse(iniFileOP.Read("System Setting", "FPN_L"));
            FPN_Len = int.Parse(iniFileOP.Read("System Setting", "FPN_Len"));
            MessageBox.Show("请转入明场，点击确定继续");
            textBox1.AppendText("-------------------------------------------------\r\n");
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            chart1.Series[2].Points.Clear();
            toolStrip1.Enabled = false;            
            Calc1.p1 = (int)((Calc1.percent_base - Calc1.percent) * SystemParam.ExposureTest_Ns / 100);
            Calc1.p2 = (int)((Calc1.percent_base + Calc1.percent) * SystemParam.ExposureTest_Ns / 100);
            SystemParam.cmosInfo = SerialFunc.SerialCommand1();
            if (SystemParam.cmosInfo == null)
            {
                MessageBox.Show("与采集器通信失败");
                this.Close();
            }            
            //第一步、采集图像
            testStep = 1;
            InitCam(2 + CamEx);
            StatusLabelInfoTrash.Text = "";
            m_Xfer.Grab();
            waitProc = new CMOSTestLib.WaitingProc();
            waitProc.MaxProgress = SystemParam.ExposureTest_Ns;
            CMOSTestLib.WaitingProcFunc wpf = new CMOSTestLib.WaitingProcFunc(曝光测试);
            if (!waitProc.Execute(wpf, "曝光时间测试", CMOSTestLib.WaitingType.None, ""))
            {
                textBox1.AppendText("用户终止自动测试\r\n");
                toolStrip1.Enabled = true;
                return;
            }
            testStep = 3;
            bLight = true;
            InitCam(FPN_Len + +CamEx);
            m_Xfer.Grab();
            waitProc = new CMOSTestLib.WaitingProc();
            waitProc.MaxProgress = FPN_L;
            wpf = new CMOSTestLib.WaitingProcFunc(FPNTest_Light);
            if (!waitProc.Execute(wpf, "FPN明场采集", CMOSTestLib.WaitingType.None, ""))
            {
                textBox1.AppendText("用户终止自动测试\r\n");
                toolStrip1.Enabled = true;
                return;
            }
            MessageBox.Show("请转入暗场，点击确定继续");
            bLight = false;
            waitProc = new CMOSTestLib.WaitingProc();
            waitProc.MaxProgress = FPN_L;
            wpf = new CMOSTestLib.WaitingProcFunc(FPNTest_Light);
            if (!waitProc.Execute(wpf, "FPN暗场采集", CMOSTestLib.WaitingType.None, ""))
            {
                textBox1.AppendText("用户终止自动测试\r\n");
                toolStrip1.Enabled = true;
                return;
            }


            waitProc = new CMOSTestLib.WaitingProc();
            waitProc.MaxProgress = 2*FPN_L*FPN_Ns;
            wpf = new CMOSTestLib.WaitingProcFunc(FPNTest_ProcData);
            if (!waitProc.Execute(wpf, "FPN数据处理", CMOSTestLib.WaitingType.None, ""))
            {
                textBox1.AppendText("用户终止自动测试\r\n");
                toolStrip1.Enabled = true;
                return;
            }
            MessageBox.Show("FPN测试完成");
        }
        bool bLight;
        void FPNTest_Light(object LockWatingThread)
        {
            byte[] yByteArray;
            uint lsup = SystemParam.eStart + (uint)(Calc1.saturation * FPN_Per / 100) * SystemParam.eStep;
            for (int k = 0; k < FPN_L;k++ )
            {
                rxFrame = 0;
                uint ls = (uint)(lsup/FPN_L*(k+1));
                Calc2.y50 = new double[m_Buffers.Height, m_Buffers.Width];

                int nCount = FPN_Ns / FPN_Len;
                this.Invoke((EventHandler)(delegate
                {
                    if(bLight)
                        textBox1.AppendText("开始" + (FPN_Per / FPN_L*(k + 1)).ToString("F2") + "%曝光明场采集\r\n");
                    else
                        textBox1.AppendText("开始" + (FPN_Per / FPN_L*(k + 1)).ToString("F2") + "%曝光暗场采集\r\n");
                    toolStripLabel3.Text = (ls * SystemParam.Ts).ToString("F2") + " ms";
                }));
                NopCam((ushort)(FPN_Len + +CamEx), ls);

                int saveindex = 0;
                for (int i = 0; i < nCount; i++)
                {
                    if (waitProc.HasBeenCancelled())
                    {
                        return;
                    }
                    SerialFunc.SerialCommand3((ushort)(FPN_Len + +CamEx), ls);
                    if (!WaitCam(FPN_Len+CamEx))
                    {
                        i--;
                        continue;
                    }
                    if(bLight)
                    {
                        for (int j = 1; j < FPN_Len + 1; j++)
                        {
                            saveindex++;
                            m_Buffers.Save(FPNFile_Path + "TempPic\\Light\\" + saveindex.ToString() + ".bmp", "-format bmp", j, 0);
                            Thread.Sleep(SystemParam.PicDelay);
                        }
                        for (int j = 1; j < FPN_Len + 1; j++)
                        {
                            yByteArray = wfSapGUI.ReadPicDatas(m_Buffers, j);
                            SystemParam.WriteTempFile(yByteArray, i * FPN_Len + j - 1, FPNFile_Path + "TempData\\Light\\" + (k + 1).ToString() + "bin");
                        }
                    }
                    else
                    {
                        for (int j = 1; j < FPN_Len + 1; j++)
                        {
                            saveindex++;
                            m_Buffers.Save(FPNFile_Path + "TempPic\\Dark\\" + saveindex.ToString() + ".bmp", "-format bmp", j, 0);
                            Thread.Sleep(SystemParam.PicDelay);
                        }
                        for (int j = 1; j < FPN_Len + 1; j++)
                        {
                            yByteArray = wfSapGUI.ReadPicDatas(m_Buffers, j);
                            SystemParam.WriteTempFile(yByteArray, i * FPN_Len + j - 1, FPNFile_Path + "TempData\\Dark\\" + (k + 1).ToString() + "bin");
                        }
                    }
                }
                if (nCount * FPN_Len < FPN_L)//还有一个
                {
                    while (true)
                    {
                        int left = FPN_L - nCount * FPN_Len;
                        rxFrame = 0;
                        SerialFunc.SerialCommand3((ushort)(FPN_Len + +CamEx), ls);
                        if (!WaitCam(SystemParam.Step2_len +CamEx))
                        {
                            continue;
                        }
                        if (bLight)
                        {
                            for (int j = 1; j < left + 1; j++)
                            {
                                saveindex++;
                                m_Buffers.Save(FPNFile_Path + "TempPic\\Light\\" + saveindex.ToString() + ".bmp", "-format bmp", j, 0);
                                Thread.Sleep(SystemParam.PicDelay);
                            }
                            for (int j = 1; j < left + 1; j++)
                            {
                                yByteArray = wfSapGUI.ReadPicDatas(m_Buffers, j);
                                SystemParam.WriteTempFile(yByteArray, nCount * FPN_Len + j - 1, FPNFile_Path + "TempData\\Light\\" + (k + 1).ToString() + "bin");
                            }
                        }
                        else
                        {
                            for (int j = 1; j < left + 1; j++)
                            {
                                saveindex++;
                                m_Buffers.Save(FPNFile_Path + "TempPic\\Dark\\" + saveindex.ToString() + ".bmp", "-format bmp", j, 0);
                                Thread.Sleep(SystemParam.PicDelay);
                            }
                            for (int j = 1; j < left + 1; j++)
                            {
                                yByteArray = wfSapGUI.ReadPicDatas(m_Buffers, j);
                                SystemParam.WriteTempFile(yByteArray, nCount * FPN_Len + j - 1, FPNFile_Path + "TempData\\Dark\\" + (k + 1).ToString() + "bin");
                            }
                        }
                        break;
                    }
                }
                waitProc.SetProcessBar(k+1);
            }
            waitProc.SetProcessBar(FPN_L);
            this.Invoke((EventHandler)(delegate
            {
                if(bLight)
                    textBox1.AppendText("FPN明场采集完成\r\n");
                else
                    textBox1.AppendText("FPN暗场采集完成\r\n");
            }));
        }
        ushort[,] FPN_y;
        byte[] ysave;
        //ushort uy;
        List<double> miu_y, miu_ydark;
        void FPNTest_ProcData(object LockWatingThread)
        {
            string strName =FPNFile_Path+ "FPN_L" + FPN_L.ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bin";
            /************************************************************************/
            /* FPN_y                                                                */
            /************************************************************************/
            miu_y = new List<double>();
            for (int i = 0; i < FPN_L; i++)
            {
                FPN_y = new ushort[m_Buffers.Height, m_Buffers.Width];
                ysave = new byte[SystemParam.ByteLen4Pic];
                for (int j = 0; j < FPN_Ns;j++ )
                {
                    byte[] p = SystemParam.ReadTempFile(SystemParam.ByteLen4Pic, j, FPNFile_Path + "TempData\\Light\\" + (i + 1).ToString() + "bin");
                    ushort[,] pic = wfSapGUI.TransPicDatas(p, m_Buffers.Height, m_Buffers.Width,m_Buffers.PixelDepth);
                    for (int m = 0; m < m_Buffers.Height; m++)
                    {
                        for (int n = 0; n < m_Buffers.Width; n++)
                        {
                            FPN_y[m, n] += pic[m, n];
                        }
                    }
                    if (waitProc.HasBeenCancelled())
                    {
                        return;
                    }
                    waitProc.SetProcessBarPerformStep();
                }
                double miu = 0;
                for (int m = 0; m < m_Buffers.Height; m++)
                {
                    for (int n = 0; n < m_Buffers.Width; n++)
                    {
                        FPN_y[m, n] = (ushort)(FPN_y[m, n] / FPN_Ns);
                        miu += FPN_y[m, n];
                    }
                }
                ysave = wfSapGUI.ReTransPicDatas(FPN_y, m_Buffers.Height, m_Buffers.Width);
                SystemParam.WriteTempFile(ysave,i, strName);
                miu = miu / m_Buffers.Height / m_Buffers.Width;
                miu_y.Add(miu);
            }            
            
            this.Invoke((EventHandler)(delegate
            {
                textBox1.AppendText("FPN明场数据处理完成\r\n");
            }));

            strName = FPNFile_Path + "FPN_D" + FPN_L.ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bin";
            /************************************************************************/
            /* FPN_y                                                                */
            /************************************************************************/
            miu_ydark = new List<double>();
            for (int i = 0; i < FPN_L; i++)
            {
                FPN_y = new ushort[m_Buffers.Height, m_Buffers.Width];
                ysave = new byte[SystemParam.ByteLen4Pic];
                for (int j = 0; j < FPN_Ns; j++)
                {
                    byte[] p = SystemParam.ReadTempFile(SystemParam.ByteLen4Pic, j, FPNFile_Path + "TempData\\Dark\\" + (i + 1).ToString() + "bin");
                    ushort[,] pic = wfSapGUI.TransPicDatas(p, m_Buffers.Height, m_Buffers.Width,m_Buffers.PixelDepth);
                    for (int m = 0; m < m_Buffers.Height; m++)
                    {
                        for (int n = 0; n < m_Buffers.Width; n++)
                        {
                            FPN_y[m, n] += pic[m, n];
                        }
                    }
                    if (waitProc.HasBeenCancelled())
                    {
                        return;
                    }
                    waitProc.SetProcessBarPerformStep();
                }
                double miu = 0;
                for (int m = 0; m < m_Buffers.Height; m++)
                {
                    for (int n = 0; n < m_Buffers.Width; n++)
                    {
                        FPN_y[m, n] = (ushort)(FPN_y[m, n] / FPN_Ns);
                        miu += FPN_y[m, n];
                    }
                }
                ysave = wfSapGUI.ReTransPicDatas(FPN_y, m_Buffers.Height, m_Buffers.Width);
                SystemParam.WriteTempFile(ysave, i, strName);
                miu = miu / m_Buffers.Height / m_Buffers.Width;
                miu_ydark.Add(miu);
            }

            this.Invoke((EventHandler)(delegate
            {
                textBox1.AppendText("FPN暗场数据处理完成\r\n");
            }));

            TextLog.AddTextLog("--------------" + DateTime.Now.ToString() + "----------------", FPNFile_Path + "FPN" + FPN_L.ToString()+"_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt", false);
            TextLog.AddTextLog(String.Format("{0,-11}{1,-11}{2,-11}", "曝光时间", "明场均值", "暗场均值"), FPNFile_Path + "FPN" + FPN_L.ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt", false);
            uint lsup = SystemParam.eStart + (uint)(Calc1.saturation * FPN_Per / 100) * SystemParam.eStep;
            for (int i = 0; i < FPN_L; i++)
            {
                uint ls = (uint)(lsup / FPN_L * (i + 1));
                double t = ((double)(ls)) * SystemParam.Ts;
                TextLog.AddTextLog(String.Format("{0,-15}{1,-15}{2,-15}", t.ToString("F3"), miu_y[i].ToString("F6"), miu_ydark[i].ToString("F6")), FPNFile_Path + "FPN" + FPN_L.ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt", false);
            }
        }
    }
}
