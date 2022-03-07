namespace udpCCDTest
{
    partial class FormParamTestChoice
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("不同曝光条件下采集", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("1231", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "饱和光照度",
            ""}, -1, System.Drawing.Color.Empty, System.Drawing.SystemColors.Window, null);
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem(new string[] {
            "饱和均值",
            "",
            ""}, -1, System.Drawing.Color.Empty, System.Drawing.SystemColors.Window, null);
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem(new string[] {
            "转换增益",
            "",
            ""}, -1, System.Drawing.Color.Empty, System.Drawing.SystemColors.Window, null);
            System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem(new string[] {
            "量子效率",
            "",
            ""}, -1, System.Drawing.Color.Empty, System.Drawing.SystemColors.Window, null);
            this.lvTested = new System.Windows.Forms.ListView();
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.rbFixOe = new System.Windows.Forms.RadioButton();
            this.rbFixTime = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rb3T = new System.Windows.Forms.RadioButton();
            this.rbParam = new System.Windows.Forms.RadioButton();
            this.cbParam = new System.Windows.Forms.CheckedListBox();
            this.cb3T = new System.Windows.Forms.CheckedListBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvTested
            // 
            this.lvTested.AutoArrange = false;
            this.lvTested.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader6,
            this.columnHeader7});
            this.lvTested.Cursor = System.Windows.Forms.Cursors.Default;
            this.lvTested.FullRowSelect = true;
            this.lvTested.GridLines = true;
            listViewGroup1.Header = "不同曝光条件下采集";
            listViewGroup1.Name = "listViewGroup1";
            listViewGroup1.Tag = "";
            listViewGroup2.Header = "1231";
            listViewGroup2.Name = "listViewGroup2";
            this.lvTested.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2});
            this.lvTested.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvTested.HideSelection = false;
            listViewItem3.StateImageIndex = 0;
            this.lvTested.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3,
            listViewItem4});
            this.lvTested.Location = new System.Drawing.Point(12, 12);
            this.lvTested.MultiSelect = false;
            this.lvTested.Name = "lvTested";
            this.lvTested.Scrollable = false;
            this.lvTested.ShowGroups = false;
            this.lvTested.Size = new System.Drawing.Size(263, 135);
            this.lvTested.TabIndex = 4;
            this.lvTested.UseCompatibleStateImageBehavior = false;
            this.lvTested.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "已测试内容";
            this.columnHeader6.Width = 80;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "测试结果";
            this.columnHeader7.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader7.Width = 181;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButton3);
            this.groupBox1.Controls.Add(this.rbFixOe);
            this.groupBox1.Controls.Add(this.rbFixTime);
            this.groupBox1.Location = new System.Drawing.Point(326, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(163, 135);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "曝光方式";
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Enabled = false;
            this.radioButton3.Location = new System.Drawing.Point(26, 103);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(107, 16);
            this.radioButton3.TabIndex = 0;
            this.radioButton3.Text = "光脉冲曝光方式";
            this.radioButton3.UseVisualStyleBackColor = true;
            // 
            // rbFixOe
            // 
            this.rbFixOe.AutoSize = true;
            this.rbFixOe.Location = new System.Drawing.Point(26, 66);
            this.rbFixOe.Name = "rbFixOe";
            this.rbFixOe.Size = new System.Drawing.Size(95, 16);
            this.rbFixOe.TabIndex = 0;
            this.rbFixOe.Text = "固定光源照度";
            this.rbFixOe.UseVisualStyleBackColor = true;
            // 
            // rbFixTime
            // 
            this.rbFixTime.AutoSize = true;
            this.rbFixTime.Checked = true;
            this.rbFixTime.Location = new System.Drawing.Point(26, 29);
            this.rbFixTime.Name = "rbFixTime";
            this.rbFixTime.Size = new System.Drawing.Size(95, 16);
            this.rbFixTime.TabIndex = 0;
            this.rbFixTime.TabStop = true;
            this.rbFixTime.Text = "固定曝光时间";
            this.rbFixTime.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rb3T);
            this.groupBox2.Controls.Add(this.rbParam);
            this.groupBox2.Location = new System.Drawing.Point(22, 179);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(139, 103);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "测试内容";
            // 
            // rb3T
            // 
            this.rb3T.AutoSize = true;
            this.rb3T.Location = new System.Drawing.Point(26, 66);
            this.rb3T.Name = "rb3T";
            this.rb3T.Size = new System.Drawing.Size(71, 16);
            this.rb3T.TabIndex = 0;
            this.rb3T.Text = "三温测试";
            this.rb3T.UseVisualStyleBackColor = true;
            this.rb3T.CheckedChanged += new System.EventHandler(this.rb3T_CheckedChanged);
            // 
            // rbParam
            // 
            this.rbParam.AutoSize = true;
            this.rbParam.Checked = true;
            this.rbParam.Location = new System.Drawing.Point(26, 29);
            this.rbParam.Name = "rbParam";
            this.rbParam.Size = new System.Drawing.Size(71, 16);
            this.rbParam.TabIndex = 0;
            this.rbParam.TabStop = true;
            this.rbParam.Text = "参数测试";
            this.rbParam.UseVisualStyleBackColor = true;
            this.rbParam.CheckedChanged += new System.EventHandler(this.rbParam_CheckedChanged);
            // 
            // cbParam
            // 
            this.cbParam.ColumnWidth = 150;
            this.cbParam.FormattingEnabled = true;
            this.cbParam.Items.AddRange(new object[] {
            "曝光测试",
            "转换增益",
            "量子效率",
            "量子效率曲线",
            "信噪比",
            "动态范围",
            "满井容量",
            "暗电流",
            "线性误差",
            "DSNU(暗信号不均匀性)",
            "PRNU(光子响应不均匀性)",
            "FPN",
            "读出噪声",
            "增益曲线"});
            this.cbParam.Location = new System.Drawing.Point(187, 179);
            this.cbParam.MultiColumn = true;
            this.cbParam.Name = "cbParam";
            this.cbParam.Size = new System.Drawing.Size(307, 132);
            this.cbParam.TabIndex = 8;
            this.cbParam.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.cbParam_ItemCheck);
            this.cbParam.Click += new System.EventHandler(this.cbParam_Click);
            // 
            // cb3T
            // 
            this.cb3T.ColumnWidth = 150;
            this.cb3T.FormattingEnabled = true;
            this.cb3T.Items.AddRange(new object[] {
            "暗电流",
            "DSNU(暗信号不均匀性)",
            "FPN",
            "读出噪声"});
            this.cb3T.Location = new System.Drawing.Point(187, 179);
            this.cb3T.MultiColumn = true;
            this.cb3T.Name = "cb3T";
            this.cb3T.Size = new System.Drawing.Size(153, 132);
            this.cb3T.TabIndex = 9;
            this.cb3T.Visible = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(430, 279);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(59, 23);
            this.button1.TabIndex = 10;
            this.button1.Text = "全选";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(103, 340);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(109, 30);
            this.button2.TabIndex = 11;
            this.button2.Text = "开始测试";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button3.Location = new System.Drawing.Point(281, 340);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(109, 30);
            this.button3.TabIndex = 11;
            this.button3.Text = "取消";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // FormParamTestChoice
            // 
            this.AcceptButton = this.button2;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button3;
            this.ClientSize = new System.Drawing.Size(515, 383);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.cb3T);
            this.Controls.Add(this.cbParam);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lvTested);
            this.Name = "FormParamTestChoice";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "参数测试向导";
            this.Load += new System.EventHandler(this.FormParamTestChoice_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvTested;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        public System.Windows.Forms.RadioButton radioButton3;
        public System.Windows.Forms.RadioButton rbFixOe;
        public System.Windows.Forms.RadioButton rbFixTime;
        public System.Windows.Forms.RadioButton rb3T;
        public System.Windows.Forms.RadioButton rbParam;
        public System.Windows.Forms.CheckedListBox cbParam;
        public System.Windows.Forms.CheckedListBox cb3T;
    }
}