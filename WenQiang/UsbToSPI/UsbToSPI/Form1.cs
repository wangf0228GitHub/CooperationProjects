using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace UsbToSPI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GY7502_CONFIG_INFO ConfigInfo = new GY7502_CONFIG_INFO();
            ConfigInfo.kFreq = 3;
            ConfigInfo.SpiMode = 2;
            int x = GY7502_USBSPI.GY7502_USBSPI_SetConfig(ref ConfigInfo);
            if (x == 0)
            {
                return;
            }
            if (GY7502_USBSPI.GY7502_USBSPI_Open() == 0)
            {
                return;
            }
            
	
        }
        string[] Files;
        int PickTimes;
        int SaveTimes;
        private void button1_Click_1(object sender, EventArgs e)
        {            
            if(button1.Text=="开始")
            {
                tbLog.Clear();
                if (GY7502_USBSPI.GY7502_USBSPI_Open() == 0)
                {
                    tbLog.AppendText("USB2SPI打开失败\r\n");
                    return;
                }
                tbLog.AppendText("USB2SPI打开成功\r\n");
                GY7502_CONFIG_INFO ConfigInfo = new GY7502_CONFIG_INFO();
                ConfigInfo.kFreq = 1;
                ConfigInfo.SpiMode = 2;
                int x = GY7502_USBSPI.GY7502_USBSPI_SetConfig(ref ConfigInfo);
                if (x == 0)
                {
                    tbLog.AppendText("USB2SPI工作参数设置失败\r\n");
                    return;
                }
                if (x == -1)
                {
                    tbLog.AppendText("USB2SPI工作参数设置失败,尚未打开\r\n");
                    return;
                }
                button1.Text = "停止";
                for (int i = 0; i < 24; i++)
                {
                    FileStream fs = new FileStream(Files[i], FileMode.Create);
                    fs.Close();
                }
                tbHH.ReadOnly = true;
                tbMM.Text = "00";
                tbSS.Text = "00";
                timer1.Enabled = true;
                PickTimes = 0;
                SaveTimes = 0;
            }
            else
            {
                button1.Text = "开始";
                tbHH.ReadOnly = false;
                timer1.Enabled = false;
                GY7502_USBSPI.GY7502_USBSPI_Close();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Files = new string[24];
            for (int i = 0; i < 24; i++)
            {
                Files[i] = System.Windows.Forms.Application.StartupPath + "\\Data\\" + (i+1).ToString()+ ".txt";               
            }
            FileInfo f = new FileInfo(Files[1]);
            if (!Directory.Exists(f.DirectoryName))
                Directory.CreateDirectory(f.DirectoryName);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (tbLog.TextLength > 25000)
                tbLog.Clear();
            PickTimes++;
            SaveTimes++;
            string time;
            if (PickTimes % 2 == 0)
            {
                int ss = Convert.ToInt32(tbSS.Text);
                if (ss == 0)
                {
                    int mm = Convert.ToInt32(tbMM.Text);
                    if (mm == 0)
                    {
                        int hh = Convert.ToInt32(tbHH.Text);
                        if (hh == 0)
                        {
                            timer1.Enabled = false;
                            tbLog.AppendText("老化实验完成");
                            GY7502_USBSPI.GY7502_USBSPI_Close();
                        }
                        else
                        {
                            hh--;
                            tbHH.Text = hh.ToString("D2");
                            tbMM.Text = "59";
                            tbSS.Text = "59";
                        }
                    }
                    else
                    {
                        mm--;
                        tbMM.Text = mm.ToString("D2");
                        tbSS.Text = "59";
                    }
                }
                else
                {
                    ss--;
                    tbSS.Text = ss.ToString("D2");
                }
            }
            time = tbHH.Text + "-" + tbMM.Text + "-" + tbSS.Text+"-"+((PickTimes % 2)+1).ToString();
            GY7502_DATA_INFO spiData = new GY7502_DATA_INFO();
            spiData.Databuffer = new byte[256];
            spiData.ChipSelect = 0;
            spiData.Databuffer[0] = 0xa9;
            spiData.ReadNum = 33;
            spiData.WriteNum = 1;
            int ret=GY7502_USBSPI.GY7502_USBSPI_Read(ref spiData);
             if (ret == 0)
             {
                 tbLog.AppendText(time + ":" + "数据读取失败\r\n");
             }
             else if (ret == -1)
             {
                 tbLog.AppendText(time + ":" + "设备未打开\r\n");
             }
             else if (ret != 33)
             {
                 tbLog.AppendText(time + ":" + "读取数据量不正确\r\n");
             }
             else
             {
                tbLog.AppendText(time + ":" + "正确读取数据"+PickTimes.ToString()+"\r\n");
                textBox1.Text = spiData.Databuffer[0].ToString("X2");
                ushort[] APS = new ushort[24];
                for (int i = 0; i < 8; i++)
                {
                    ushort x1;
                    byte x2;
                    x2 = (byte)(spiData.Databuffer[1 + (i << 2) + 1] >> 6);
                    x1 = (ushort)(spiData.Databuffer[1 + (i << 2)]);
                    x1 = (ushort)(x1 << 2);
                    x1 = (ushort)(x1 | ((ushort)x2));
                    APS[i*3] = x1;
                    x2 = (byte)(spiData.Databuffer[1 + (i << 2) + 1] & 0x3f);
                    x1 = (ushort)x2;
                    x1 = (ushort)(x1 << 4);
                    x2 = (byte)(spiData.Databuffer[1 + (i << 2) + 2] >> 4);
                    x1 = (ushort)(x1 | ((ushort)x2));
                    APS[i * 3 + 1] = x1;
                    x2 = (byte)(spiData.Databuffer[1 + (i << 2) + 2] & 0x0f);
                    x1 = (ushort)x2;
                    x1 = (ushort)(x1 << 6);
                    x2 = (byte)(spiData.Databuffer[1 + (i << 2) + 3] >> 2);
                    x1 = (ushort)(x1 | ((ushort)x2));
                    APS[i * 3 + 2] = x1;
                    x2 = (byte)(spiData.Databuffer[1 + (i << 2) + 3] & 0x03);
                    if (x2 != 0x01)
                    {
                        tbLog.AppendText(time + ":" + "APS" + (i + 1).ToString() + "状态错误\r\n");
                    }
                }
                textBox2.Text = APS[0].ToString();
                textBox3.Text = APS[1].ToString();
                textBox4.Text = APS[2].ToString();
                textBox5.Text = APS[3].ToString();
                textBox6.Text = APS[4].ToString();
                textBox7.Text = APS[5].ToString();
                textBox8.Text = APS[6].ToString();
                textBox9.Text = APS[7].ToString();
                textBox10.Text = APS[8].ToString();
                textBox11.Text = APS[9].ToString();
                textBox12.Text = APS[10].ToString();
                textBox13.Text = APS[11].ToString();
                textBox14.Text = APS[12].ToString();
                textBox15.Text = APS[13].ToString();
                textBox16.Text = APS[14].ToString();
                textBox17.Text = APS[15].ToString();
                textBox18.Text = APS[16].ToString();
                textBox19.Text = APS[17].ToString();
                textBox20.Text = APS[18].ToString();
                textBox21.Text = APS[19].ToString();
                textBox22.Text = APS[20].ToString();
                textBox23.Text = APS[21].ToString();
                textBox24.Text = APS[22].ToString();
                textBox25.Text = APS[23].ToString();
                 if(SaveTimes>=20)
                 {
                     SaveTimes = 0;
                     for (int i = 0; i < 24; i++)
                     {
                         FileStream fs = new FileStream(Files[i], FileMode.Open);
                         StreamWriter sw = new StreamWriter(fs);
                         //开始写入
                         StringBuilder sb = new StringBuilder();
                         sb.AppendFormat("{0,-10}", PickTimes.ToString());
                         sb.AppendFormat("{0,-4}", APS[i].ToString());
                         fs.Seek(0, SeekOrigin.End);
                         sw.WriteLine(sb.ToString());
                         //清空缓冲区
                         sw.Flush();
                         //关闭流
                         sw.Close();
                         fs.Close();/**/
                     }
                 }
            }
        }
    }
}
