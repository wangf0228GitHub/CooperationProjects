using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WFNetLib;

namespace udpCCDTest
{
    public partial class FormManual : Form
    {
        public FormManual()
        {
            InitializeComponent();
        }
        int nCount;
        uint Tex;
        int lambda;
        double OePer;
        private void toolStripButton1_Click(object sender, EventArgs e)
        {      
            double d;
            if (!(int.TryParse(tbCount.Text, out nCount) && (double.TryParse(tbTex.Text, out d)) && (int.TryParse(tbLambda.Text, out lambda)) && ((double.TryParse(tbOePer.Text, out OePer)))))
            {
                MessageBox.Show("参数设置有误");
                return;
            }
            tcpCCS.LightSet(lambda, tcpCCS.Per2LX(lambda ,OePer));
            Tex = (uint)trackBar1.Value;// (int)(d / SystemParam.Ts);
            if (!UDPProc.CollectImage(this, Tex, nCount))
                return;
            for (int i = 0; i < UDPProc.ccdImageList.Count; i++)
                UDPProc.ccdImageList[i].save(SystemParam.TempPicPath + "E_" + (i + 1).ToString() + ".bin");
            //             for (ushort g = 0; g < UDPProc.ccdImageList.Count; g++)
            //             {
            Bitmap bitmap = new Bitmap(SystemParam.CCD_N, SystemParam.CCD_M, PixelFormat.Format48bppRgb);
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, SystemParam.CCD_N, SystemParam.CCD_M), ImageLockMode.WriteOnly, PixelFormat.Format48bppRgb);
            int posScan = 0, posReal = 0;
            IntPtr iptr = bmpData.Scan0;
            int scanBytes = bmpData.Stride * SystemParam.CCD_M;
            byte[] pixel = new byte[scanBytes];

            for (int x = 0; x < SystemParam.CCD_M; x++)
            {
                for (int y = 0; y < SystemParam.CCD_N; y++)
                {
                    //short p = (short)0xffff;//(short)BytesOP.MakeShort(UDPProc.ccdImageList.Last().byteList[posReal + 1], UDPProc.ccdImageList.Last().byteList[posReal]);
                    ushort p = BytesOP.MakeShort(UDPProc.ccdImageList.Last().byteList[posReal + 1], UDPProc.ccdImageList.Last().byteList[posReal]);
                    p = (ushort)(p << 4);
                    byte pL = BytesOP.GetLowByte(p);
                    byte pH = BytesOP.GetHighByte(p);
                    pixel[posScan++] = pL;
                    pixel[posScan++] = pH;
                    pixel[posScan++] = pL;
                    pixel[posScan++] = pH;
                    pixel[posScan++] = pL;
                    pixel[posScan++] = pH;
                    posReal += 2;
                }
            }
            System.Runtime.InteropServices.Marshal.Copy(pixel, 0, iptr, scanBytes);
            bitmap.UnlockBits(bmpData);
            Image img = Image.FromHbitmap(bitmap.GetHbitmap());
            pictureBox1.Image = img;
            //                MessageBox.Show(g.ToString());
            //            }

            MessageBox.Show("拍照成功");
        }        
        private void FormManual_Load(object sender, EventArgs e)
        {
            trackBar1.Minimum = (int)SystemParam.NTmin2;
            trackBar1.Value = (int)SystemParam.NTmin2;
            tbTex.Text = (trackBar1.Value * 1000  / SystemParam.Get_phi()).ToString("F3");
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            tbTex.Text = ((double)(trackBar1.Value) * 1000  / SystemParam.Get_phi() ).ToString("F3");
        }
    }
}
