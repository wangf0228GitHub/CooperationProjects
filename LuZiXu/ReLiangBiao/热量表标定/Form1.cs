using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WFNetLib.PacketProc;
using System.Diagnostics;
using WFNetLib;
using LinearAlgebra.LinearEquations;
using LinearAlgebra;

namespace 热量表标定
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {            
            serialPort1.PortName = "COM19";
            CP68Packet.DeviceType = 0x20;
            CP68Packet.DeviceAddr = new byte[7];
            for(int i=0;i<7;i++)
                CP68Packet.DeviceAddr[i] = (byte)i;
        }        
    }
}
