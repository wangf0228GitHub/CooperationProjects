using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WFNetLib;
using WFNetLib.Forms;

namespace udpCCDTest
{
    public partial class FormMain : Form
    {
        void UIHide()
        {
            exposureChart.Visible = false;
            exposureListView.Visible = false;

            exposureVerifyChart.Visible = false;
            exposureVerifyListView.Visible = false;

            CCDParamTestListView.Visible = false;
            ParamTestChart1.Visible = false;
            ParamTestChart2.Visible = false;


            exposureChart.Dock = DockStyle.Fill;
            exposureListView.Dock = DockStyle.Fill;

            exposureVerifyChart.Dock = DockStyle.Fill;
            exposureVerifyListView.Dock = DockStyle.Fill;

            CCDParamTestListView.Dock = DockStyle.Fill;
            ParamTestChart1.Dock = DockStyle.Fill;
            ParamTestChart2.Dock = DockStyle.Fill;

        }
        public delegate void _ShowText(string s);
        public void __ShowText(string s)
        {
            this.Invoke((EventHandler)(delegate
            {
                textBox1.AppendText(s + "\r\n");
            }));            
        }
        public static _ShowText ShowText;
        public _tcpCCS tcpCCS; 
        public FormMain()
        {
            InitializeComponent();
            ShowText = new _ShowText(__ShowText);
            string fileName = SystemParam.TempPicPath + "1.tif";
            FileInfo f = new FileInfo(fileName);
            if (!Directory.Exists(f.DirectoryName))
                Directory.CreateDirectory(f.DirectoryName);
            fileName = SystemParam.ccdParamFilePath + "1.tif";
            f = new FileInfo(fileName);
            if (!Directory.Exists(f.DirectoryName))
                Directory.CreateDirectory(f.DirectoryName);
            fileName = SystemParam.TxtDataPath + "1.tif";
            f = new FileInfo(fileName);
            if (!Directory.Exists(f.DirectoryName))
                Directory.CreateDirectory(f.DirectoryName);            
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            FormParam f = new FormParam();
            f.ShowDialog();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            SystemParam.InitSystemParam();
            DeviceState.fMain = this;
            tcpCCS = new _tcpCCS();
            //tcpCCS.Connect();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            FormManual f = new FormManual();
            f.ShowDialog();
        }
        
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            tcpCCS.LightSet(550, 0.5);
        }


        private void btExposureVerify_Click(object sender, EventArgs e)
        {
            ExposureVerify();
        }

        private void exposureVerifyListView_Resize(object sender, EventArgs e)
        {
            exposureVerifyListView.Columns[1].Width = exposureVerifyListView.ClientSize.Width - exposureVerifyListView.Columns[0].Width;
        }

        private void exposureListView_Resize(object sender, EventArgs e)
        {
            exposureListView.Columns[1].Width = exposureListView.ClientSize.Width - exposureListView.Columns[0].Width;
        }

        private void btExposureTest_Click(object sender, EventArgs e)
        {
            SystemParam.DeviceID = InputBox.ShowInputBox("请设定当前测试器件的芯片编号", SystemParam.DeviceID);
            iniFileOP.Write("System Run", "DeviceID", SystemParam.DeviceID);
            strCCDINIPath = SystemParam.ccdParamFilePath + SystemParam.DeviceID + ".ini";
            string Osat = iniFileOP.Read("CCD Param", "Osat", strCCDINIPath);
            if (!FileOP.IsExist(strCCDINIPath, FileOPMethod.File))
            {
                FileOP.CopyFile(System.Windows.Forms.Application.StartupPath + "\\ccdParamFileTemplate.ini", strCCDINIPath);
            }
            ExposureTest();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            SystemParam.DeviceID = InputBox.ShowInputBox("请设定当前测试器件的芯片编号", SystemParam.DeviceID);
            iniFileOP.Write("System Run", "DeviceID", SystemParam.DeviceID);
            strCCDINIPath = SystemParam.ccdParamFilePath + SystemParam.DeviceID + ".ini";
            string Osat = iniFileOP.Read("CCD Param", "Osat", strCCDINIPath);
            if (!FileOP.IsExist(strCCDINIPath, FileOPMethod.File))
            {
                FileOP.CopyFile(System.Windows.Forms.Application.StartupPath + "\\ccdParamFileTemplate.ini", strCCDINIPath);
            }
            CCDParamTest();
        }

        private void CCDParamTestListView_Resize(object sender, EventArgs e)
        {
            CCDParamTestListView.Columns[1].Width = CCDParamTestListView.ClientSize.Width - CCDParamTestListView.Columns[0].Width;
        }

        private void CCDParamTestListView_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void CCDParamTestListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo info = CCDParamTestListView.HitTest(e.X, e.Y);
            if (info.Item != null)
            {
                ResultCurveForm f = new ResultCurveForm();
                for (int i = 0; i < 100; i++)
                {
                    f.chart.Series[0].Points.AddXY(i*10, i * 6);
                }
                f.ShowDialog();
                //MessageBox.Show(info.Item.Text+e.Button.ToString());
            }
        }
    }
}
