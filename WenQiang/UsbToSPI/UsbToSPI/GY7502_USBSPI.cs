using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace UsbToSPI
{
    class GY7502_USBSPI
    {
        [DllImport("GY7502_USBSPI.dll", EntryPoint = "GY7502_USBSPI_Open", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int	GY7502_USBSPI_Open();//device open
        [DllImport("GY7502_USBSPI.dll", EntryPoint = "GY7502_USBSPI_Close", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int	GY7502_USBSPI_Close();//device open
        [DllImport("GY7502_USBSPI.dll", EntryPoint = "GY7502_USBSPI_SetConfig", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int GY7502_USBSPI_SetConfig(ref GY7502_CONFIG_INFO pConfigInfo);//device open
        [DllImport("GY7502_USBSPI.dll", EntryPoint = "GY7502_USBSPI_GetConfig", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int GY7502_USBSPI_GetConfig(ref GY7502_CONFIG_INFO pConfigInfo);//device open
        [DllImport("GY7502_USBSPI.dll", EntryPoint = "GY7502_USBSPI_Read", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int GY7502_USBSPI_Read(ref GY7502_DATA_INFO pDataInfo);//device open
        [DllImport("GY7502_USBSPI.dll", EntryPoint = "GY7502_USBSPI_Write", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int GY7502_USBSPI_Write(ref GY7502_DATA_INFO pDataInfo);//device open
        [DllImport("GY7502_USBSPI.dll", EntryPoint = "GY7502_USBSPI_WriteRead", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int GY7502_USBSPI_WriteRead(ref GY7502_DATA_INFO pDataInfo);//device open
        [DllImport("GY7502_USBSPI.dll", EntryPoint = "GY7502_USBSPI_SetIO", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int GY7502_USBSPI_SetIO(ref GY7502_DATA_INFO pDataInfo);//device open
        [DllImport("GY7502_USBSPI.dll", EntryPoint = "GY7502_USBSPI_GetIO", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int GY7502_USBSPI_GetIO(ref GY7502_DATA_INFO pDataInfo);//device open	
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct GY7502_DATA_INFO
    {
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] Databuffer;//buffer to write or read
        public uint WriteNum;//Valid num in Databuffer[]
        public uint ReadNum;// data num to Read
        public byte IoSel;//bit value=1 means selected. bit0 for IO-PORT0, bit1 for IO-PORT1
        public byte IoData;//only the bit will be valid which bit is 1 in IoSel.
        public byte ChipSelect;//Chip Selected,output,  0~CS0, 1~CS1
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] Reserved;//Reserved 
	}
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct GY7502_CONFIG_INFO
    {
        public byte kFreq;//SPI Clock define.Value=0..5
		//0~100kHz,1~200kHz,2~500kHz,3~1000kHz,4~2000kHz,5~6000kHz
        public byte SpiMode;//SPI Mode,to define CKPOL,CKPHA
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] Reserved;//Reserved						
	};
}
