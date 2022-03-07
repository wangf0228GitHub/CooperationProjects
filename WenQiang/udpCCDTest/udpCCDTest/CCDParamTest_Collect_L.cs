using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WFNetLib;

namespace udpCCDTest
{
    public partial class FormMain : Form
    {
//         List<double> Collect_L_miu;
//         List<double> Collect_L_delta;
//         List<double> Collect_L_miu_dark;
//         List<double> Collect_L_delta_dark;
        string L_TempFilePath;
        bool CCDParamTest_Collect_L(bool bLight)
        {
            //UIHide();
            ParamTestChart1.Visible = true;
            ParamTestChart1.Dock = DockStyle.Fill;
            if (bLight)
            {
                textBox1.AppendText("50%饱和空域测试\r\n");
                ParamTestWaitingProcTitle = "50%饱和空域测试中";
                L_TempFilePath = SystemParam.L_LightTempFilePath;
                tcpCCS.LightSet(SystemParam.lambda_Oe, ccdParamTestResult.Osat/2);
            }
            else
            {
                textBox1.AppendText("暗场空域测试\r\n");
                ParamTestWaitingProcTitle = "暗场空域测试中";
                L_TempFilePath = SystemParam.L_DarkTempFilePath;
                tcpCCS.LightSet(SystemParam.lambda_Oe, 0.0);
            }            
            CreateTempFile(SystemParam.L, L_TempFilePath);
            ParamTestWaitingProc = new WaitingProc();            
            WaitingProcFunc wpf = null;
            ParamTestWaitingProcMax = SystemParam.L;
            ParamTestWaitingProc.MaxProgress = ParamTestWaitingProcMax;
            wpf = new WaitingProcFunc(WaitingCollect_L);
            if (!ParamTestWaitingProc.Execute(wpf, ParamTestWaitingProcTitle, WaitingType.With_ConfirmCancel, "是否取消？"))
            {
                textBox1.AppendText("用户终止自动测试\r\n");
                return false;
            }
            return true;
        }
        void WaitingCollect_L(object LockWatingThread)
        {
            uint Tex;
            //double E;
            this.Invoke((EventHandler)(delegate
            {
                WFGlobal.WaitMS(1);
//                 exposureChart.Series["miu"].Points.Clear();
//                 exposureChart.Series["delta"].Points.Clear();
//                 tcpCCS.LightSet(SystemParam.lambda_Oe, 0);//暗场
            }));
            Tex = ccdParamTestResult.NTmin1;
            //else
            {
                if (!UDPProc.CollectImage(this, Tex, 2))
                {
                    ParamTestWaitingProc.ExitWatting();
                    return;
                }
            }
            //明场
            for (int i = 0; i < SystemParam.L; i++)
            {
                this.Invoke((EventHandler)(delegate
                {
                    //                     exposureListView.Items[2].SubItems[1].Text = (Imin.ToString("F2") + "-" + Imax.ToString("F2"));
                    //                     exposureListView.Items[3].SubItems[1].Text = oe_per.ToString("F2");
                    if (!UDPProc.CollectImage(this, Tex, 1))
                    {
                        ParamTestWaitingProc.ExitWatting();
                        return;
                    }
                    WriteTempFile(UDPProc.ccdImageList[0].byteList, i, L_TempFilePath); 
                }));
                if (ParamTestWaitingProc.HasBeenCancelled())
                {
                    return;
                }
                ParamTestWaitingProc.SetProcessBarPerformStep();
                ParamTestWaitingProc.SetTitle(ParamTestWaitingProcTitle + ":" + (i+1).ToString() + "/" + ParamTestWaitingProcMax.ToString());
                WFGlobal.WaitMS(1);
            }
        }
        public byte[] ReadTempFile(int Len, int index, string fileName)
        {
            Stream stream = File.OpenRead(fileName);
            long x = stream.Seek((long)Len * (long)index, SeekOrigin.Begin);
            byte[] ret = new byte[Len];
            stream.Read(ret, 0, Len);
            stream.Close();
            return ret;
        }
        public void WriteTempFile(byte[] pBuf, int index, string fileName)
        {
            Stream stream = File.OpenWrite(fileName);
            long x = stream.Seek((long)pBuf.Length * (long)index, SeekOrigin.Begin);
            stream.Write(pBuf, 0, pBuf.Length);
            stream.Flush();
            stream.Close();
        }        
        public void CreateTempFile(int count, string fileName)
        {
            FileStream fs = null;
            try
            {
                FileInfo f = new FileInfo(fileName);
                if (!Directory.Exists(f.DirectoryName))
                    Directory.CreateDirectory(f.DirectoryName);
                fs = new FileStream(fileName, FileMode.Create);
                Int64 len = (Int64)SystemParam.CCD_M * (Int64)SystemParam.CCD_N * (Int64)2/*BytesPerPixel*/ * (Int64)count;
                fs.SetLength(len); //设置文件大小  
            }
            catch (Exception ex)
            {
                if (fs != null)
                {
                    fs.Close();
                    File.Delete(fileName); //注意，若由fs.SetLength方法产生了异常，同样会执行删除命令，请慎用overwrite:true参数，或者修改删除文件代码。  
                }
                MessageBox.Show("创建临时文件出错！" + ex.Message);
                throw ex;
            }
            finally
            {
                if (fs != null) fs.Close();
            }
        }
    }
}
