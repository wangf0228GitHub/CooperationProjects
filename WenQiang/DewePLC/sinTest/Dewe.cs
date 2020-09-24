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

namespace sinTest
{
    public partial class sinTestForm : Form
    {
        private OpcUaClient opcUaClient;// = new OpcUaClient();
        Task OpcTask;
        void ConnectOPCServer()
        {
            while (true)
            {
                opcUaClient = new OpcUaClient();
                OpcTask = opcUaClient.ConnectServer(opcUrl);//("opc.tcp://127.0.0.1:4840");
                try
                {                    
                    OpcTask.Wait();
                    if (OpcTask.IsCompleted)
                    {
                        //opcUaClient.AddSubscription("A", "ns=1;s=data/OpcUaServer/channels/1/0_CUR_SCA_VAL", SubCallback1);
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
        string[] deweNodes;
        double deweNiuZhen, deweNiuJu;
        void ReadDeweData()
        {
            try
            {
                // 因为不能保证读取的节点类型一致，所以只提供统一的DataValue读取，每个节点需要单独解析
                double d1 = 0;
                double d2 = 0;
                int len = 8;
                for (int i=0;i<len;i++)
                {
                    List<float> dewe = opcUaClient.ReadNodes<float>(deweNodes);
                    d1 += dewe[0] * deweNiuJu_k + deweNiuJu_b;
                    d2 += dewe[1] * deweNiuZhen_k + deweNiuZhen_b;
                }
                deweNiuJu = d1/len;
                deweNiuZhen = d2/len;
            }
            catch (Exception ex)
            {
                ShowText("Dewe读取出错:" + ex.Message);
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
