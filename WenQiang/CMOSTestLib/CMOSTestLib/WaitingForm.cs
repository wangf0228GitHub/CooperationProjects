using System;
using System.Windows;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace CMOSTestLib
{
    public partial class WaitingForm : Form
    {
//         public WaitingForm()
//         {
//             InitializeComponent();
//         }
        public bool bCancelled;
        public ManualResetEvent ReadyEvent;
        public WaitingType type;
        public object LockWatingThread;
        bool CloseSelf = false;
        /************************************************************************/
        /* 外部设定鼠标样式                                                     */
        /************************************************************************/
        private delegate void SetCursorStyleDelegate(Cursor c);
        private void SetCursorStyleDelegateProc(Cursor c)
        {
            this.Cursor = c;
        }        
        public void ExternSetCursorStyle(Cursor c)
        {
            while (!this.IsHandleCreated) ;
            this.Invoke(new SetCursorStyleDelegate(SetCursorStyleDelegateProc), c);
        }
        /************************************************************************/
        /* 外部设定窗体名称                                                     */
        /************************************************************************/
        private delegate void SetTitleDelegate(string str);
        private void SetTitleDelegateProc(string str)
        {
            this.Text = str;
        }
        public void ExternSetTitle(string str)
        {
            while (!this.IsHandleCreated) ;
            this.Invoke(new SetTitleDelegate(SetTitleDelegateProc), str);
        }
        /************************************************************************/
        /* 外部设定进度条范围                                                       */
        /************************************************************************/
        private delegate void SetProcessBarRangeDelegate(int min,int max);
        private void SetProcessBarRangeDelegateProc(int min, int max)
        {
            progressBar1.Minimum= min;
            progressBar1.Maximum = max;            
        }
        public void ExternSetProcessBarRange(int min, int max)
        {
            while (!this.IsHandleCreated) ;
            this.Invoke(new SetProcessBarRangeDelegate(SetProcessBarRangeDelegateProc), min,max);
        }
        /************************************************************************/
        /* 外部设定进度条                                                       */
        /************************************************************************/
        private delegate void SetProcessBarDelegate(int i);
        private void SetProcessBarDelegateProc(int i)
        {
            if(i>progressBar1.Maximum)
                progressBar1.Value = progressBar1.Maximum;
            else if (i < progressBar1.Minimum)
                    progressBar1.Value = progressBar1.Minimum;
            else
                progressBar1.Value = i;
        }
        public void ExternSetProcessBar(int i)
        {
            while (!this.IsHandleCreated) ;
            try
            {
                this.Invoke(new SetProcessBarDelegate(SetProcessBarDelegateProc), i);
            }
            catch { }
        }
        /************************************************************************/
        /* 外部设定进度条递增                                                       */
        /************************************************************************/
        private delegate void SetProcessBarPerformStepDelegate();
        private void SetProcessBarPerformStepDelegateProc()
        {
            progressBar1.PerformStep();
        }
        public void ExternSetProcessBarPerformStep()
        {
            while (!this.IsHandleCreated) ;
            this.Invoke(new SetProcessBarPerformStepDelegate(SetProcessBarPerformStepDelegateProc));
        }
        /************************************************************************/
        /* 外部关闭窗体                                                         */
        /************************************************************************/
        private delegate void CloseDelegate();      
        private void CloseDelegateProc()
        {
            CloseSelf = true;
            this.Close();
            
        }
        public void ExternClose()
        {
            while (!this.IsHandleCreated)
            {
                if (this.bCancelled)
                    return;
            }
            this.Invoke(new CloseDelegate(CloseDelegateProc));
        }
        
        public WaitingForm(WaitingType type)
        {
            InitializeComponent();
            LockWatingThread = new object();
            ReadyEvent = new ManualResetEvent(false);
            this.type = type;
            if (type == WaitingType.None)
            {
                //button1.Visible = false;
                //progressBar1.Width = 530;
            }
            bCancelled = false;
        }
        private string _ConfirmPrompt;

        public string ConfirmPrompt
        {
            get { return _ConfirmPrompt; }
            set { _ConfirmPrompt = value; }
        }

        private void WaitingForm_Shown(object sender, EventArgs e)
        {
            ReadyEvent.Set();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            switch (type)
            {
                case WaitingType.None:
                    break;
                case WaitingType.WithCancel:
                    bCancelled = true;
                    break;

                case WaitingType.With_ConfirmCancel:
                    lock (LockWatingThread)
                    {
                        if(MessageBox.Show(ConfirmPrompt, "waring", MessageBoxButtons.YesNo)==DialogResult.Yes)                       
                        {
                            bCancelled = true;
                        } 
                    }
                    break;
            }
        }

        private void WaitingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (button1.Visible || CloseSelf)
            //    return;
            if (CloseSelf || bCancelled)
                return;
            lock (LockWatingThread)
            {
                if (MessageBox.Show("是否终止当前测试", "警告", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    this.Text += "*****正在终止当前测试";
                    bCancelled = true;
                    e.Cancel = false;
                }
                else
                    e.Cancel = true;
            }
            //e.Cancel = true;
        }        
    }

}
