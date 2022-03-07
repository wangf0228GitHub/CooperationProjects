using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WFNetLib;

namespace udpCCDTest
{
    public partial class FormMain : Form
    {        
        bool CCDParamTest_DR_FWC()
        {
            if (double.IsNaN(ccdParamTestResult.eta))
            {
                if (!CCDParamTest_eta())
                    return false;
            }
            if(double.IsNaN(ccdParamTestResult.miu_p_sat) || double.IsNaN(ccdParamTestResult.miu_p_min))
            {
                if (!CCDParamTest_SNR())
                    return false;
            }
            //tcpCCS.LightSet(SystemParam.lambda_Oe, ccdParamTestResult.Osat);
            //if (!UDPProc.CollectImage(this, ccdParamTestResult.NTmin1, 2))
            //{
            //    return false;
            //}
            //UDPProc.ccdImageList[0].save(SystemParam.TempPicPath + "Max_0.bin");
            //UDPProc.ccdImageList[1].save(SystemParam.TempPicPath + "Max_1.bin");
            //ccdImage.Calc_miu_delta(UDPProc.ccdImageList[0], UDPProc.ccdImageList[1], out miu_sat, out delta_sat, out miuCC);
            //tcpCCS.LightSet(SystemParam.lambda_Oe, 0.0);
            //if (!UDPProc.CollectImage(this, ccdParamTestResult.NTmin1, 2))
            //{
            //    return false;
            //}
            //UDPProc.ccdImageList[0].save(SystemParam.TempPicPath + "Min_0.bin");
            //UDPProc.ccdImageList[1].save(SystemParam.TempPicPath + "Min_1.bin");
            //ccdImage.Calc_miu_delta(UDPProc.ccdImageList[0], UDPProc.ccdImageList[1], out miu_dark, out delta_dark, out miuCC);
            
            //double miu_p_min = (Math.Sqrt(delta_dark / ccdParamTestResult.K / ccdParamTestResult.K + 0.25) + 0.5) / ccdParamTestResult.eta;
            //double texp = SystemParam.GetTime2(0,ccdParamTestResult.NTexp2);
            //double miu_p_sat = 50.34 * SystemParam.CCD_Sa * SystemParam.CCD_Sb * texp * SystemParam.lambda_Oe * ccdParamTestResult.Osat;
            ccdParamTestResult.DR = 20 * Math.Log10(ccdParamTestResult.miu_p_sat / ccdParamTestResult.miu_p_min);

            ccdParamTestResult.FWC = (ccdParamTestResult.miu_sat - Collect_SNR_miu_dark) / ccdParamTestResult.K;//ccdParamTestResult.miu_p_sat* ccdParamTestResult.eta;
            return true;
        }        
    }
}
