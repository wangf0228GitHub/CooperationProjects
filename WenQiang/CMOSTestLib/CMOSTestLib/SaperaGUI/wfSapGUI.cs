using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using WFNetLib;
using DALSA.SaperaLT.SapClassBasic;
using System.Runtime.InteropServices;

namespace CMOSTestLib.SaperaGUI
{
    public class wfSapGUI
    {
        public static List<PixelInfo> RPixel;
        public static List<PixelInfo> GPixel;
        public static List<PixelInfo> BPixel;
        public static void GetRGBPixelInfo(int row, int col, int rgb1, int rgb2, int rgb3, int rgb4)
        {
            RPixel = new List<PixelInfo>();
            GPixel = new List<PixelInfo>();
            BPixel = new List<PixelInfo>();
            for (int i = 0; i < (row / 2); i += 2)
            {
                for (int j = 0; j < (col / 2); j += 2)
                {
                    PixelInfo p = new PixelInfo();
                    p.row = i;
                    p.col = j;
                    switch (rgb1)
                    {
                        case 0:
                            RPixel.Add(p);
                            break;
                        case 1:
                            GPixel.Add(p);
                            break;
                        case 2:
                            BPixel.Add(p);
                            break;
                    }
                    p.row = i;
                    p.col = j+1;
                    switch (rgb2)
                    {
                        case 0:
                            RPixel.Add(p);
                            break;
                        case 1:
                            GPixel.Add(p);
                            break;
                        case 2:
                            BPixel.Add(p);
                            break;
                    }
                    p.row = i+1;
                    p.col = j;
                    switch (rgb3)
                    {
                        case 0:
                            RPixel.Add(p);
                            break;
                        case 1:
                            GPixel.Add(p);
                            break;
                        case 2:
                            BPixel.Add(p);
                            break;
                    }
                    p.row = i+1;
                    p.col = j+1;
                    switch (rgb4)
                    {
                        case 0:
                            RPixel.Add(p);
                            break;
                        case 1:
                            GPixel.Add(p);
                            break;
                        case 2:
                            BPixel.Add(p);
                            break;
                    }
                }
            }
        }
        public static Rectangle FitToWindow(Rectangle ViewArea, Rectangle picArea)
        {
            return new Rectangle();
//             decimal WidthScalor = (decimal)(100.0f * ViewArea.Width / picArea.Width);
//             decimal HeightScalor = (decimal)(100.0f * ViewArea.Height / picArea.Height);
//             Rectangle ret = new Rectangle();
//             ret.Width .Value = WidthScalor;
//             NUpDown_height_scalor.Value = HeightScalor;
// 
//             NUpDown_width.Value = Decimal.Floor((decimal)(m_pView.Buffer.Width * (float)NUpDown_width_scalor.Value / 100 + 0.5f));
//             NUpDown_height.Value = Decimal.Floor((decimal)(m_pView.Buffer.Height * (float)NUpDown_height_scalor.Value / 100 + 0.5f));
        }
        public static byte[] ReadPicDatas(SapBuffer Buffers)
        {
            return ReadPicDatas(Buffers, -1);
        }

        public static byte[] ReadPicDatas(SapBuffer Buffers,int index)
        {
            long len = Buffers.Height * Buffers.Width * Buffers.BytesPerPixel;
            long len1 = len / Buffers.BytesPerPixel;
            byte[] pRx1 = new byte[len];
            IntPtr pBuf1 = Marshal.UnsafeAddrOfPinnedArrayElement(pRx1, 0);
            if(index<0)
                Buffers.Read(0, (int)len1, pBuf1);
            else
                Buffers.Read(index, 0, (int)len1, pBuf1);
            return pRx1;
        }
        public static ushort[,] TransPicDatas(byte[] ya, int row, int col, int PixelDepth)
        {
            int k;
            ushort[,] ret = new ushort[row, col];
            for (int m = 0; m < row; m++)
            {
                for (int n = 0; n < col; n++)
                {
                    k = n * 2;
                    ret[m,n] = BytesOP.MakeShort(ya[k + 2 * col * m + 1], ya[k + 2 * col * m]);
                    switch (PixelDepth)
                    {
                        case 8:
                            ret[m, n] &= 0x00ff;
                            break;
                        case 10:
                            ret[m, n] &= 0x03ff;
                            break;
                        case 12:
                            ret[m, n] &= 0x0fff;
                            break;
                        case 14:
                            ret[m, n] &= 0x3fff;
                            break;
                    }
                }
            }
            return ret;
        }
        public static void TransPicDatas(byte[] ya, int row, int col, int PixelDepth, int rgb1, int rgb2, int rgb3, int rgb4, out ushort[] yR, out ushort[] yG, out ushort[] yB)
        {
            ushort[,] ret = TransPicDatas(ya,row,col,PixelDepth);
            List<ushort> R = new List<ushort>();
            List<ushort> G = new List<ushort>();
            List<ushort> B = new List<ushort>();
            for (int i = 0; i < (row / 2); i+=2)
            {
                for (int j = 0; j < (col / 2); j += 2)
                {
                    switch (rgb1)
                    {
                        case 0:
                            R.Add(ret[i, j]);
                            break;
                        case 1:
                            G.Add(ret[i, j]);
                            break;
                        case 2:
                            B.Add(ret[i, j]);
                            break;
                    }
                    switch (rgb2)
                    {
                        case 0:
                            R.Add(ret[i, j+1]);
                            break;
                        case 1:
                            G.Add(ret[i, j+1]);
                            break;
                        case 2:
                            B.Add(ret[i, j+1]);
                            break;
                    }
                    switch (rgb3)
                    {
                        case 0:
                            R.Add(ret[i+1, j]);
                            break;
                        case 1:
                            G.Add(ret[i+1, j]);
                            break;
                        case 2:
                            B.Add(ret[i+1, j]);
                            break;
                    }
                    switch (rgb4)
                    {
                        case 0:
                            R.Add(ret[i+1, j + 1]);
                            break;
                        case 1:
                            G.Add(ret[i+1, j + 1]);
                            break;
                        case 2:
                            B.Add(ret[i+1, j + 1]);
                            break;
                    }
                }
            }
            yR = R.ToArray();
            yG = G.ToArray();
            yB = B.ToArray();
        }
        public static byte[] ReTransPicDatas(ushort[,] ya, int row, int col)
        {
            int k;
            byte[] ret = new byte[row*col*2];
            for (int m = 0; m < row; m++)
            {
                for (int n = 0; n < col; n++)
                {
                    k = n * 2;
                    ret[k + 2 * col * m + 1]=BytesOP.GetHighByte(ya[m,n]);
                    ret[k + 2 * col * m]=BytesOP.GetLowByte(ya[m,n]);
                }
            }
            return ret;
        }
    }
}
