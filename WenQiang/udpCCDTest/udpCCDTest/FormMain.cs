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
using WFNetLib.Log;

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
            exposureChart.ChartAreas[0].AxisX.Title = "照度";
            CCDParamTestListView.Visible = false;
            CCD3TTestListView.Visible = false;
            ParamTestChart1.Visible = false;
            ParamTestChart2.Visible = false;

            exposureChart.Dock = DockStyle.Fill;
            exposureListView.Dock = DockStyle.Fill;

            exposureVerifyChart.Dock = DockStyle.Fill;
            exposureVerifyListView.Dock = DockStyle.Fill;

            CCDParamTestListView.Dock = DockStyle.Fill;
            CCD3TTestListView.Dock = DockStyle.Fill;
            ParamTestChart1.Dock = DockStyle.Fill;
            ParamTestChart2.Dock = DockStyle.Fill;

        }
        void chartHide()
        {
            exposureChart.Visible = false;
            exposureVerifyChart.Visible = false;
            ParamTestChart1.Visible = false;
            ParamTestChart2.Visible = false;
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
            ccdParamTestResult = new CCDParamTestResult();
            ccd3TTestResult = new CCD3TTestResult();
            DeviceState.fMain = this;
            UDPProc.fMain = this;
            tcpCCS.Connect();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            FormManual f = new FormManual();
            f.ShowDialog();
        }
        
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            
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
//             if (!FileOP.IsExist(SystemParam.strCCDINIPath, FileOPMethod.File))
//             {
//                 FileOP.CopyFile(System.Windows.Forms.Application.StartupPath + "\\ccdParamFileTemplate.ini", SystemParam.strCCDINIPath);
//             }
            if(ExposureTest())
                MessageBox.Show("曝光步距测试完毕");
            else
                MessageBox.Show("曝光步距测试失败");
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            SystemParam.DeviceID = InputBox.ShowInputBox("请设定当前测试器件的芯片编号", SystemParam.DeviceID);
//             iniFileOP.Write("System Run", "DeviceID", SystemParam.DeviceID);
//             SystemParam.strCCDINIPath = SystemParam.ccdParamFilePath + SystemParam.DeviceID + "_" + SystemParam.lambda_Oe.ToString() + ".ini";
//             if (!FileOP.IsExist(SystemParam.strCCDINIPath, FileOPMethod.File))
//             {
//                 FileOP.CopyFile(System.Windows.Forms.Application.StartupPath + "\\ccdParamFileTemplate.ini", SystemParam.strCCDINIPath);
//             }
            CCDTest();
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
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {

                    MakeReport(saveFileDialog1.FileName);
                    textBox1.AppendText("测试报告保存位置为:\r\n");
                    textBox1.AppendText(saveFileDialog1.FileName);
                }
            }
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            FormCCS_Calibration f = new FormCCS_Calibration();
            f.ShowDialog();
        }

        private void CCD3TTestListView_SizeChanged(object sender, EventArgs e)
        {
            CCD3TTestListView.Columns[1].Width = CCD3TTestListView.ClientSize.Width - CCD3TTestListView.Columns[0].Width;
        }

        private void CCD3TTestListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo info = CCD3TTestListView.HitTest(e.X, e.Y);
            if (info.Item != null)
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    MakeReport3T(saveFileDialog1.FileName);
                    textBox1.AppendText("测试报告保存位置为:\r\n");
                    textBox1.AppendText(saveFileDialog1.FileName);
                }
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            tcpCCS.LightSet(SystemParam.lambda_Oe, 0);
            e.Cancel = false;
        }
    }
}
