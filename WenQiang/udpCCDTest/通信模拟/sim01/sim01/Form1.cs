using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace sim01
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Thread thrRecv;
        UdpClient udpcRecv;
        IPEndPoint deviceIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12032);
        IPEndPoint pcIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12022);
        private void Form1_Load(object sender, EventArgs e)
        {
            udpcRecv = new UdpClient(deviceIP);
            thrRecv = new Thread(ReceiveMessage);
            thrRecv.Start();
        }
        public string byteToHexStr(byte[] bytes, string spilt)
        {
            StringBuilder strB = new StringBuilder();
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    strB.Append(bytes[i].ToString("X2"));
                    strB.Append(spilt);
                }
            }
            return strB.ToString();
        }
        void ReceiveMessage(object obj)
        {            
            UdpClient udpcSend = new UdpClient();
            byte[] tx = new byte[34];
            tx[0] = 0x80;
            
            tx[2] = 0x55;
            tx[3] = 0xaa;
            tx[4] = 0;
            tx[5] = 0x01;
            while (true)
            {
                try
                {
                    byte[] bytRecv = udpcRecv.Receive(ref deviceIP);
                    this.Invoke((EventHandler)(delegate
                    {
                        string str = byteToHexStr(bytRecv, " ");
                        textBox1.AppendText(str + "\r\n");
                    }));
                    tx[1] = bytRecv[0];
                    udpcSend.Send(tx, tx.Length, pcIP);

                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}
