namespace TestClient
{
    partial class FormMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tc_Top = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tb_ConnectNum = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.rtb_Report = new System.Windows.Forms.TabControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.lv_Status = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tb_IPPort = new System.Windows.Forms.TextBox();
            this.lb_IPPort = new System.Windows.Forms.Label();
            this.btn_Connect = new System.Windows.Forms.Button();
            this.label32 = new System.Windows.Forms.Label();
            this.label31 = new System.Windows.Forms.Label();
            this.label30 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.timer_Send = new System.Windows.Forms.Timer(this.components);
            this.tc_Top.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.rtb_Report.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tc_Top
            // 
            this.tc_Top.Controls.Add(this.tabPage1);
            this.tc_Top.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tc_Top.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.tc_Top.Location = new System.Drawing.Point(0, 0);
            this.tc_Top.Name = "tc_Top";
            this.tc_Top.SelectedIndex = 0;
            this.tc_Top.Size = new System.Drawing.Size(955, 661);
            this.tc_Top.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tb_ConnectNum);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.rtb_Report);
            this.tabPage1.Controls.Add(this.tb_IPPort);
            this.tabPage1.Controls.Add(this.lb_IPPort);
            this.tabPage1.Controls.Add(this.btn_Connect);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(947, 635);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "连接管理";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tb_ConnectNum
            // 
            this.tb_ConnectNum.Location = new System.Drawing.Point(482, 9);
            this.tb_ConnectNum.Name = "tb_ConnectNum";
            this.tb_ConnectNum.Size = new System.Drawing.Size(100, 21);
            this.tb_ConnectNum.TabIndex = 33;
            this.tb_ConnectNum.Text = "1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(427, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 32;
            this.label1.Text = "连接数：";
            // 
            // rtb_Report
            // 
            this.rtb_Report.Controls.Add(this.tabPage3);
            this.rtb_Report.Location = new System.Drawing.Point(6, 38);
            this.rtb_Report.Name = "rtb_Report";
            this.rtb_Report.SelectedIndex = 0;
            this.rtb_Report.Size = new System.Drawing.Size(938, 594);
            this.rtb_Report.TabIndex = 31;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.lv_Status);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(930, 568);
            this.tabPage3.TabIndex = 0;
            this.tabPage3.Text = "运行状态";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // lv_Status
            // 
            this.lv_Status.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.lv_Status.GridLines = true;
            this.lv_Status.Location = new System.Drawing.Point(6, 6);
            this.lv_Status.Name = "lv_Status";
            this.lv_Status.Size = new System.Drawing.Size(921, 556);
            this.lv_Status.TabIndex = 23;
            this.lv_Status.UseCompatibleStateImageBehavior = false;
            this.lv_Status.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "状态";
            this.columnHeader1.Width = 900;
            // 
            // tb_IPPort
            // 
            this.tb_IPPort.Location = new System.Drawing.Point(116, 9);
            this.tb_IPPort.Name = "tb_IPPort";
            this.tb_IPPort.Size = new System.Drawing.Size(305, 21);
            this.tb_IPPort.TabIndex = 29;
            this.tb_IPPort.Text = "192.168.1.12:8001";
            // 
            // lb_IPPort
            // 
            this.lb_IPPort.AutoSize = true;
            this.lb_IPPort.Location = new System.Drawing.Point(35, 12);
            this.lb_IPPort.Name = "lb_IPPort";
            this.lb_IPPort.Size = new System.Drawing.Size(77, 12);
            this.lb_IPPort.TabIndex = 28;
            this.lb_IPPort.Text = "服务器地址：";
            // 
            // btn_Connect
            // 
            this.btn_Connect.Location = new System.Drawing.Point(588, 9);
            this.btn_Connect.Name = "btn_Connect";
            this.btn_Connect.Size = new System.Drawing.Size(75, 23);
            this.btn_Connect.TabIndex = 23;
            this.btn_Connect.Text = "(TCP)开 始";
            this.btn_Connect.UseVisualStyleBackColor = true;
            this.btn_Connect.Click += new System.EventHandler(this.btn_Connect_Click);
            // 
            // label32
            // 
            this.label32.Location = new System.Drawing.Point(0, 0);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(100, 23);
            this.label32.TabIndex = 0;
            // 
            // label31
            // 
            this.label31.Location = new System.Drawing.Point(0, 0);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(100, 23);
            this.label31.TabIndex = 0;
            // 
            // label30
            // 
            this.label30.Location = new System.Drawing.Point(0, 0);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(100, 23);
            this.label30.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 23);
            this.label2.TabIndex = 0;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(955, 661);
            this.Controls.Add(this.tc_Top);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.Text = "Console";
            this.tc_Top.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.rtb_Report.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tc_Top;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabControl rtb_Report;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TextBox tb_IPPort;
        private System.Windows.Forms.Label lb_IPPort;
        private System.Windows.Forms.Button btn_Connect;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.ListView lv_Status;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Timer timer_Send;
        private System.Windows.Forms.TextBox tb_ConnectNum;
        private System.Windows.Forms.Label label1;
    }
}

