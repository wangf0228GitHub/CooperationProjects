using Opc.Ua;
using Opc.Ua.Client;
using OpcUaHelper;
using OpcUaHelper.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DewePLC
{
    public partial class Form1 : Form
    {
        private OpcUaClient opcUaClient;// = new OpcUaClient();
        Task OpcTask;
        void ConnectOPCServer()
        {
            while (true)
            {
                opcUaClient = new OpcUaClient();
                OpcTask = opcUaClient.ConnectServer("opc.tcp://127.0.0.1:4840");
                try
                {                    
                    OpcTask.Wait();
                    if (OpcTask.IsCompleted)
                    {
                        opcUaClient.AddSubscription("A", "ns=1;s=data/OpcUaServer/channels/1/0_CUR_SCA_VAL", SubCallback1);
                        break;
                    }
                    else
                    {
                        if (MessageBox.Show("无法连接OPC服务器请确保OPC服务器正常工作!", "opc服务器连接失败", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1) == DialogResult.Cancel)
                        {
                            opcUaClient.Disconnect();
                            this.Close();
                            return;
                        }
                    }
                    //Console.WriteLine("taskA Status: {0}", taskA.Status);
                }
                catch (AggregateException ex)
                {
                    if (MessageBox.Show("无法连接OPC服务器，错误为:" + ex.Message + ",请确保OPC服务器正常工作!", "opc服务器连接失败", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1) == DialogResult.Cancel)
                    {
                        opcUaClient.Disconnect();
                        this.Close();
                        return;
                    }
                }
            }
        }
        public void SubCallback1(string key, MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs args)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string, MonitoredItem, MonitoredItemNotificationEventArgs>(SubCallback1), key, monitoredItem, args);
                return;
            }

            if (key == "A")
            {
                // 如果有多个的订阅值都关联了当前的方法，可以通过key和monitoredItem来区分
                MonitoredItemNotification notification = args.NotificationValue as MonitoredItemNotification;
                if (notification != null)
                {
                    try
                    {
                        textBox1.Text = notification.Value.WrappedValue.Value.ToString();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
        }
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            using (FormBrowseServer form = new FormBrowseServer())
            {
                form.ShowDialog();
            }
        }
    }    
}
