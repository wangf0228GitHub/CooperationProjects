using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WFNetLib;

namespace udpCCDTest
{
    public class ccdImage
    {
        public ushort ImageCount;//此次拍照的总图片数
        public ushort ImageIndex;//此幅照片的序号
        public ushort RowCount;//行总数
        public byte[] byteList;
        public ushort[,] imageData;
        public Boolean[,] rxConfirm;
        public ccdImage()
        {
            byteList = new byte[SystemParam.CCD_M * SystemParam.CCD_N * 2];
            rxConfirm = new Boolean[SystemParam.CCD_M, 12];
            for (int i = 0; i < SystemParam.CCD_M; i++)
            {
                for (int j = 0; j < 12; j++)
                    rxConfirm[i, j] = false;
            }
        }
        public void TransImageDatas()//, int row, int col, int PixelDepth)
        {
            int k;
            imageData = new ushort[SystemParam.CCD_M, SystemParam.CCD_N];
            for (int m = 0; m < SystemParam.CCD_M; m++)
            {
                for (int n = 0; n < SystemParam.CCD_N; n++)
                {
                    k = n * 2;
                    imageData[m, n] = BytesOP.MakeShort(byteList[k + 2 * SystemParam.CCD_N * m + 1], byteList[k + 2 * SystemParam.CCD_N * m]);
                    switch (SystemParam.CCD_ADL)//图像传感器模数转换宽度
                    {
                        case 8:
                            imageData[m, n] &= 0x00ff;
                            break;
                        case 10:
                            imageData[m, n] &= 0x03ff;
                            break;
                        case 12:
                            imageData[m, n] &= 0x0fff;
                            break;
                        case 14:
                            imageData[m, n] &= 0x3fff;
                            break;
                    }
                }
            }
        }
        public void save(string path)
        {
            System.IO.Stream so = new System.IO.FileStream(path, System.IO.FileMode.Create);
            so.Write(byteList, 0, byteList.Length);
            so.Close();
        }
        public static ushort[,] TransImageDatas(byte[] byteList)//, int row, int col, int PixelDepth)
        {
            int k;
            ushort[,]  imageData = new ushort[SystemParam.CCD_M, SystemParam.CCD_N];
            for (int m = 0; m < SystemParam.CCD_M; m++)
            {
                for (int n = 0; n < SystemParam.CCD_N; n++)
                {
                    k = n * 2;
                    imageData[m, n] = BytesOP.MakeShort(byteList[k + 2 * SystemParam.CCD_N * m + 1], byteList[k + 2 * SystemParam.CCD_N * m]);
                    switch (SystemParam.CCD_ADL)//图像传感器模数转换宽度
                    {
                        case 8:
                            imageData[m, n] &= 0x00ff;
                            break;
                        case 10:
                            imageData[m, n] &= 0x03ff;
                            break;
                        case 12:
                            imageData[m, n] &= 0x0fff;
                            break;
                        case 14:
                            imageData[m, n] &= 0x3fff;
                            break;
                    }
                }
            }
            return imageData;
        }
        public static void Calc_miu_delta(ccdImage pic0, ccdImage pic1, out double miu, out double delta,out double miuCC)
        {
            ulong y0 = 0, y1 = 0;
            ulong d=0;
            int x;
            for (int m = 0; m < SystemParam.CCD_M; m++)
            {
                for (int n = 0; n < SystemParam.CCD_N; n++)
                {
                    y0 += (ulong)pic0.imageData[m, n];
                    y1 += (ulong)pic1.imageData[m, n];
                    x = pic0.imageData[m, n] - pic1.imageData[m, n];
                    x = x * x;
                    d += (ulong)x;
                }
            }
            double miu0, miu1;
            miu0 = (double)y0 / SystemParam.CCD_M / SystemParam.CCD_N;
            miu1 = (double)y1 / SystemParam.CCD_M / SystemParam.CCD_N;
            miu = (miu0+miu1) / 2;
            delta = (double)d / 2 / SystemParam.CCD_M / SystemParam.CCD_N;
            miuCC = (miu0 - miu1) * (miu0 - miu1) / 2;
            delta = delta - miuCC;
        }
    }
}
