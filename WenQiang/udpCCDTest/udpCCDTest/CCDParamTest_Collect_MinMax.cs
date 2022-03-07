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
        double miu_dark;
        double miu_sat;
        double delta_dark;
        double delta_sat;
        void CCDParamTest_Collect_MinMax()
        {
            double miuCC;
            tcpCCS.LightSet(SystemParam.lambda_Oe, SystemParam.Osat);
            if (!UDPProc.CollectImage(this, SystemParam.NTmin2, 2))
            {
                return;
            }
            UDPProc.ccdImageList[0].save(SystemParam.TempPicPath + "Max_0.bin");
            UDPProc.ccdImageList[1].save(SystemParam.TempPicPath + "Max_1.bin");
            ccdImage.Calc_miu_delta(UDPProc.ccdImageList[0], UDPProc.ccdImageList[1], out miu_sat, out delta_sat, out miuCC);
            tcpCCS.LightSet(SystemParam.lambda_Oe, 0.0);
            if (!UDPProc.CollectImage(this, SystemParam.NTmin2, 2))
            {
                return;
            }
            UDPProc.ccdImageList[0].save(SystemParam.TempPicPath + "Min_0.bin");
            UDPProc.ccdImageList[1].save(SystemParam.TempPicPath + "Min_1.bin");
            ccdImage.Calc_miu_delta(UDPProc.ccdImageList[0], UDPProc.ccdImageList[1], out miu_dark, out delta_dark, out miuCC);
        }
    }
}
