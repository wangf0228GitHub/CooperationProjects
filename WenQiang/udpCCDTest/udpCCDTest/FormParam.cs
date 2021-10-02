using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace udpCCDTest
{
    public partial class FormParam : Form
    {
        public FormParam()
        {
            InitializeComponent();
        }

        private void FormParam_Load(object sender, EventArgs e)
        {
            tbCCDModel.Text = SystemParam.CCDModel;
            nCCD_Sa.Value = SystemParam.CCD_Sa;
            nCCD_Sb.Value = SystemParam.CCD_Sb;
            nCCD_M.Value = SystemParam.CCD_M;
            nCCD_N.Value = SystemParam.CCD_N;
            nCCD_Pa.Value = SystemParam.CCD_Pa;
            nCCD_Pb.Value = SystemParam.CCD_Pb;
            nCCD_Sch.Value = SystemParam.CCD_Sch;
            cbCCD_ADL.SelectedIndex = SystemParam.CCD_ADL;
            cbCCD_phi.SelectedIndex = SystemParam.CCD_phi;
            cbCCD_PGA.SelectedIndex = SystemParam.CCD_PGA;

            nNTmin.Value = SystemParam.NTmin;
            nn.Value = SystemParam.n;
            nL.Value = SystemParam.L;
            nL_BTM.Value = SystemParam.L_BTM;
            nL_TOP.Value = SystemParam.L_TOP;
            nNTexp.Value = SystemParam.NTexp;
            nNTdark.Value = SystemParam.NTdark;
            ndelta_Tdark.Value = SystemParam.delta_Tdark;

            nOe.Value = SystemParam.Oe;
            nlambda_Oe.Value = SystemParam.lambda_Oe;
            nNp.Value = SystemParam.Np;
            ndelta_lambda.Value = (decimal)SystemParam.delta_lambda;
            nL_lambda.Value = (decimal)SystemParam.L_lambda;
            nH_lambda.Value = (decimal)SystemParam.H_lambda;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            SystemParam.CCDModel = tbCCDModel.Text;
            SystemParam.CCD_Sa = (int)nCCD_Sa.Value;
            SystemParam.CCD_Sb = (int)nCCD_Sb.Value;
            SystemParam.CCD_M = (int)nCCD_M.Value;
            SystemParam.CCD_N = (int)nCCD_N.Value;
            SystemParam.CCD_Pa = (int)nCCD_Pa.Value;
            SystemParam.CCD_Pb = (int)nCCD_Pb.Value;
            SystemParam.CCD_Sch = (int)nCCD_Sch.Value;
            SystemParam.CCD_ADL = cbCCD_ADL.SelectedIndex;
            SystemParam.CCD_phi = cbCCD_phi.SelectedIndex;
            SystemParam.CCD_PGA = cbCCD_PGA.SelectedIndex;

            SystemParam.NTmin = (int)nNTmin.Value;
            SystemParam.n = (int)nn.Value;
            SystemParam.L = (int)nL.Value;
            SystemParam.L_BTM = (int)nL_BTM.Value;
            SystemParam.L_TOP = (int)nL_TOP.Value;
            SystemParam.NTexp = (int)nNTexp.Value;
            SystemParam.NTdark = (int)nNTdark.Value;
            SystemParam.delta_Tdark = (int)ndelta_Tdark.Value;

            SystemParam.Oe = (int)nOe.Value;
            SystemParam.lambda_Oe = (int)nlambda_Oe.Value;
            SystemParam.Np = (int)nNp.Value;
            SystemParam.delta_lambda = (double)ndelta_lambda.Value;
            SystemParam.L_lambda = (double)nL_lambda.Value;
            SystemParam.H_lambda = (double)nH_lambda.Value;
            SystemParam.SaveSystemParam();
            this.DialogResult = DialogResult.OK;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SystemParam.CCD_M = (int)nCCD_M.Value;
            SystemParam.CCD_N = (int)nCCD_N.Value;
            SystemParam.CCD_phi = cbCCD_phi.SelectedIndex;
            SystemParam.CCD_PGA = cbCCD_PGA.SelectedIndex;
            DeviceState ds= UDPProc.UDPCommand_01();
            if(ds==null)
            {
                MessageBox.Show("设置失败");
            }
            else
            {
                MessageBox.Show("设置成功");
            }
        }
    }
}
