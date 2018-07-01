using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CMOSTestLib;
using WFNetLib;

namespace 采集测试
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            numericUpDown1.Value = SystemParam.ExposureTest_Ns;
            numericUpDown6.Value = (decimal)SystemParam.ExposurePointThreshold;

            numericUpDown2.Value = SystemParam.L;
            numericUpDown3.Value = SystemParam.Step2_len;
            numericUpDown4.Value = (decimal)SystemParam.DarkPointPer;
            numericUpDown5.Value = (decimal)SystemParam.LightPointPer;

            numericUpDown7.Value = Calc1.k1;
            numericUpDown8.Value = Calc1.k2;

            numericUpDown10.Value = (decimal)Calc1.percent_base;
            numericUpDown9.Value = (decimal)Calc1.percent;

            numericUpDown13.Value = int.Parse(iniFileOP.Read("System Setting", "FPN_Per"));
            numericUpDown12.Value = int.Parse(iniFileOP.Read("System Setting", "FPN_Ns"));

            numericUpDown11.Value = int.Parse(iniFileOP.Read("System Setting", "FPN_L"));
            numericUpDown14.Value = int.Parse(iniFileOP.Read("System Setting", "FPN_Len"));

            numericUpDown16.Value = SystemParam.PicDelay;
			numericUpDown21.Value = SystemParam.WaitTimeOut;

            tbPixelArea.Text = SystemParam.cmosInfo.PixelArea.ToString();
            tbLambda.Text = SystemParam.cmosInfo.Lambda.ToString();

            if (SystemParam.cmosInfo.bRGB == 0)
            {
                bRGB.Checked = false;
                RGB1.Visible = false;
                RGB2.Visible = false;
                RGB3.Visible = false;
                RGB4.Visible = false;
            }
            else
            {
                bRGB.Checked = true;
                RGB1.Visible = true;
                RGB2.Visible = true;
                RGB3.Visible = true;
                RGB4.Visible = true;                
            }
            RGB1.SelectedIndex = SystemParam.cmosInfo.RGB1;
            RGB2.SelectedIndex = SystemParam.cmosInfo.RGB2;
            RGB3.SelectedIndex = SystemParam.cmosInfo.RGB3;
            RGB4.SelectedIndex = SystemParam.cmosInfo.RGB4;

            numericUpDown24.Value = (decimal)double.Parse(iniFileOP.Read("E Calc", "Rf"));
            numericUpDown23.Value = (decimal)double.Parse(iniFileOP.Read("E Calc", "rho"));
            numericUpDown22.Value = (decimal)double.Parse(iniFileOP.Read("E Calc", "S"));

            numericUpDown17.Value = (decimal)double.Parse(iniFileOP.Read("System Run", "eStart"));
            numericUpDown18.Value = (decimal)double.Parse(iniFileOP.Read("System Run", "eStep"));

            numericUpDown19.Value = (decimal)double.Parse(iniFileOP.Read("System Run", "BaudRate"));
            tbPath.Text = iniFileOP.Read("CMOS Param", "ccfPath");
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
			/************************************************************************/
			/* 传感器参数                                                           */
			/************************************************************************/			
			SystemParam.cmosInfo.PixelArea = int.Parse(tbPixelArea.Text);
			SystemParam.cmosInfo.Lambda = int.Parse(tbLambda.Text);
			SystemParam.cmosInfo.RGB1 = RGB1.SelectedIndex;
			SystemParam.cmosInfo.RGB2 = RGB2.SelectedIndex;
			SystemParam.cmosInfo.RGB3 = RGB3.SelectedIndex;
			SystemParam.cmosInfo.RGB4 = RGB4.SelectedIndex;			
			SystemParam.ccfPath = tbPath.Text;
			iniFileOP.Write("CMOS Param", "PixelArea", SystemParam.cmosInfo.PixelArea.ToString());
			iniFileOP.Write("CMOS Param", "Lambda", SystemParam.cmosInfo.Lambda.ToString());
			iniFileOP.Write("CMOS Param", "RGB1", SystemParam.cmosInfo.RGB1.ToString());
			iniFileOP.Write("CMOS Param", "RGB2", SystemParam.cmosInfo.RGB2.ToString());
			iniFileOP.Write("CMOS Param", "RGB3", SystemParam.cmosInfo.RGB3.ToString());
			iniFileOP.Write("CMOS Param", "RGB4", SystemParam.cmosInfo.RGB4.ToString());
			iniFileOP.Write("CMOS Param", "ccfPath", tbPath.Text);			
			if (bRGB.Checked)
			{
				SystemParam.cmosInfo.bRGB = 1;
				iniFileOP.Write("CMOS Param", "bRGB", "1");
			}
			else
			{
				SystemParam.cmosInfo.bRGB = 0;
				iniFileOP.Write("CMOS Param", "bRGB", "0");
			}
			/************************************************************************/
			/* 曝光测试																*/
			/************************************************************************/
			SystemParam.ExposureTest_Ns = (int)numericUpDown1.Value;
			Calc1.k1 = (int)numericUpDown7.Value;
			Calc1.k2 = (int)numericUpDown8.Value;
			SystemParam.ExposurePointThreshold = (double)numericUpDown6.Value;

			Calc1.percent_base = (double)numericUpDown10.Value;
			Calc1.percent = (double)numericUpDown9.Value;
			SystemParam.PicDelay = (int)numericUpDown16.Value;
			SystemParam.WaitTimeOut = (int)numericUpDown21.Value;
            iniFileOP.Write("System Setting", "ExposureTest_Ns",SystemParam.ExposureTest_Ns.ToString());
            iniFileOP.Write("System Setting", "k1", Calc1.k1.ToString());
            iniFileOP.Write("System Setting", "k2", Calc1.k2.ToString());
            iniFileOP.Write("System Setting", "percent_base", Calc1.percent_base.ToString());
            iniFileOP.Write("System Setting", "percent", Calc1.percent.ToString());

			iniFileOP.Write("System Setting", "PicDelay", SystemParam.PicDelay.ToString(""));
			iniFileOP.Write("System Setting", "WaitTimeOut", SystemParam.WaitTimeOut.ToString(""));
			/************************************************************************/
			/* 50%曝光测试                                                          */
			/************************************************************************/
			SystemParam.L = (ushort)numericUpDown2.Value;
			SystemParam.Step2_len = (ushort)numericUpDown3.Value;
			SystemParam.DarkPointPer = (double)numericUpDown4.Value;
			SystemParam.LightPointPer = (double)numericUpDown5.Value;
            iniFileOP.Write("System Setting", "L", SystemParam.L.ToString());
            iniFileOP.Write("System Setting", "Step2_len", SystemParam.Step2_len.ToString());
            iniFileOP.Write("System Setting", "DarkPointPer", SystemParam.DarkPointPer.ToString("F1"));
            iniFileOP.Write("System Setting", "LightPointPer", SystemParam.LightPointPer.ToString("F1"));


            iniFileOP.Write("System Setting", "FPN_Per", ((int)(numericUpDown13.Value)).ToString());
            iniFileOP.Write("System Setting", "FPN_NS", ((int)(numericUpDown12.Value)).ToString());
            iniFileOP.Write("System Setting", "FPN_L", ((int)(numericUpDown11.Value)).ToString());
            iniFileOP.Write("System Setting", "FPN_Len", ((int)(numericUpDown14.Value)).ToString());

           			
			/************************************************************************/
			/*  辐照度计算                                                          */
			/************************************************************************/
            SystemParam.eInfo.Rf = (double)numericUpDown24.Value;
            SystemParam.eInfo.rho = (double)numericUpDown23.Value;
            SystemParam.eInfo.S = (double)numericUpDown22.Value;
            iniFileOP.Write("E Calc", "Rf", SystemParam.eInfo.Rf.ToString("F0"));
            iniFileOP.Write("E Calc", "rho", SystemParam.eInfo.rho.ToString("F0"));
            iniFileOP.Write("E Calc", "S", SystemParam.eInfo.S.ToString("F0"));
            

            SystemParam.eStart = (uint)numericUpDown17.Value;
            SystemParam.eStep = (uint)numericUpDown18.Value;
            SystemParam.BaudRate = (int)numericUpDown19.Value;


            iniFileOP.Write("System Run", "eStart", SystemParam.eStart.ToString());
            iniFileOP.Write("System Run", "eStep", SystemParam.eStep.ToString());
            iniFileOP.Write("System Run", "BaudRate", SystemParam.BaudRate.ToString());
			try
			{
				SerialFunc.CloseSerialPort();
				SerialFunc.OpenSerialPort();
			}
			catch
			{				
			}
            
            this.Close();
        }

        private void label28_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void bRGB_CheckedChanged(object sender, EventArgs e)
        {
            if (bRGB.Checked)
            {
                RGB1.Visible = true;
                RGB2.Visible = true;
                RGB3.Visible = true;
                RGB4.Visible = true;                
            }
            else
            {
                RGB1.Visible = false;
                RGB2.Visible = false;
                RGB3.Visible = false;
                RGB4.Visible = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                tbPath.Text = openFileDialog1.FileName;
            }
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }
    }
}
