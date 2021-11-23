using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WFNetLib;

namespace udpCCDTest
{
    public partial class FormParamTestChoice : Form
    {
        public FormParamTestChoice()
        {
            InitializeComponent();
        }
        string strCCDINIPath;
        private void FormParamTestChoice_Load(object sender, EventArgs e)
        {
            strCCDINIPath = SystemParam.ccdParamFilePath + SystemParam.DeviceID + ".ini";                    
            string str = iniFileOP.Read("CCD Param", "Osat", strCCDINIPath);
            if (str == "xx")
            {
                lvTested.Items[0].SubItems[1].Text = "未测试";                
                cbParam.SetItemChecked(0, true);
            }
            else
            {
                lvTested.Items[0].SubItems[1].Text = str;
                SystemParam.Osat = int.Parse(str);             
            }

            str = iniFileOP.Read("CCD Param", "miu_sat", strCCDINIPath);
            if (str == "xx")
            {
                lvTested.Items[1].SubItems[1].Text = "未测试";
            }
            else
            {
                lvTested.Items[1].SubItems[1].Text = str;
                SystemParam.miu_sat = double.Parse(str);
            }

            str = iniFileOP.Read("CCD Param", "K", strCCDINIPath);
            if (str == "xx")
            {
                lvTested.Items[2].SubItems[1].Text = "未测试";
                cbParam.SetItemChecked(1, true);
            }
            else
            {
                lvTested.Items[2].SubItems[1].Text = str;
                CCDParamTestResult.K = double.Parse(str);
            }

            str = iniFileOP.Read("CCD Param", "eta", strCCDINIPath);
            if (str == "xx")
            {
                lvTested.Items[3].SubItems[1].Text = "未测试";
            }
            else
            {
                lvTested.Items[3].SubItems[1].Text = str;
                CCDParamTestResult.eta = double.Parse(str);
            }
        }

        private void rb3T_CheckedChanged(object sender, EventArgs e)
        {
            if (lvTested.Items[2].SubItems[1].Text == "未测试")
            {
                rbParam.Checked = true;                
            }
            else
            {
                cb3T.Visible = true;
                cbParam.Visible = false;
            }                
        }

        private void rbParam_CheckedChanged(object sender, EventArgs e)
        {
            cb3T.Visible = false;
            cbParam.Visible = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CheckedListBox cb;
            if (cb3T.Visible == true)
            {
                cb = cb3T;
            }
            else
                cb = cbParam;
            for (int i = 0; i < cb.Items.Count; i++)
                cb.SetItemChecked(i, true);
        }

        private void cbParam_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            
        }

        private void cbParam_Click(object sender, EventArgs e)
        {
            if (lvTested.Items[0].SubItems[1].Text == "未测试")
            {
                if (!cbParam.GetItemChecked(0))
                    cbParam.SetItemChecked(0, true);                
            }
            if (lvTested.Items[2].SubItems[1].Text == "未测试")
            {
                if (!cbParam.GetItemChecked(1))
                    cbParam.SetItemChecked(1, true);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(rbParam.Checked)
            {
                if(cbParam.CheckedItems.Count==0)
                {
                    MessageBox.Show("未选择测试项目!");
                    return;
                }
            }
            else
            {
                if (cb3T.CheckedItems.Count == 0)
                {
                    MessageBox.Show("未选择测试项目!");
                    return;
                }
            }
            this.DialogResult = DialogResult.OK;
        }
    }
}
