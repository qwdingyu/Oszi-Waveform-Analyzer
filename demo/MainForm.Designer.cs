using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ScpiTransport.Demo
{
    partial class MainForm
    {
        private IContainer components = null;

        private ComboBox comboDevices;
        private Button btnRefresh;
        private Button btnInstallDriver;
        private RadioButton radioUsb;
        private RadioButton radioVxi;
        private RadioButton radioTcp;
        private Label lblDevices;
        private Label lblTcpEndpoint;
        private TextBox txtTcpEndpoint;
        private Button btnConnect;
        private Button btnDisconnect;
        private TextBox txtCommand;
        private Button btnSend;
        private TextBox txtResponse;
        private Label lblResponse;
        private Label lblStatus;
        private Label lblMode;
        private Label lblCommand;
        private TextBox txtVxiDeviceName;
        private Label lblVxiDeviceName;

        /// <summary>
        /// 清理当前窗体占用的所有托管资源。
        /// </summary>
        /// <param name="disposing">true 表示同时释放托管资源与非托管资源。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            if (disposing && mi_StatusTimer != null)
            {
                mi_StatusTimer.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        private void InitializeComponent()
        {
            this.components = new Container();
            this.comboDevices = new ComboBox();
            this.btnRefresh = new Button();
            this.btnInstallDriver = new Button();
            this.radioUsb = new RadioButton();
            this.radioVxi = new RadioButton();
            this.radioTcp = new RadioButton();
            this.lblDevices = new Label();
            this.lblTcpEndpoint = new Label();
            this.txtTcpEndpoint = new TextBox();
            this.btnConnect = new Button();
            this.btnDisconnect = new Button();
            this.txtCommand = new TextBox();
            this.btnSend = new Button();
            this.txtResponse = new TextBox();
            this.lblResponse = new Label();
            this.lblStatus = new Label();
            this.lblMode = new Label();
            this.lblCommand = new Label();
            this.txtVxiDeviceName = new TextBox();
            this.lblVxiDeviceName = new Label();
            this.SuspendLayout();
            // 
            // comboDevices
            // 
            this.comboDevices.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboDevices.FormattingEnabled = true;
            this.comboDevices.Location = new Point(120, 72);
            this.comboDevices.Name = "comboDevices";
            this.comboDevices.Size = new Size(360, 23);
            this.comboDevices.TabIndex = 5;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new Point(496, 71);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new Size(96, 25);
            this.btnRefresh.TabIndex = 6;
            this.btnRefresh.Text = "刷新设备";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnInstallDriver
            // 
            this.btnInstallDriver.Location = new Point(608, 71);
            this.btnInstallDriver.Name = "btnInstallDriver";
            this.btnInstallDriver.Size = new Size(120, 25);
            this.btnInstallDriver.TabIndex = 7;
            this.btnInstallDriver.Text = "安装/修复驱动";
            this.btnInstallDriver.UseVisualStyleBackColor = true;
            this.btnInstallDriver.Click += new System.EventHandler(this.btnInstallDriver_Click);
            // 
            // radioUsb
            // 
            this.radioUsb.AutoSize = true;
            this.radioUsb.Location = new Point(120, 40);
            this.radioUsb.Name = "radioUsb";
            this.radioUsb.Size = new Size(101, 19);
            this.radioUsb.TabIndex = 2;
            this.radioUsb.TabStop = true;
            this.radioUsb.Text = "USB 本地连接";
            this.radioUsb.UseVisualStyleBackColor = true;
            this.radioUsb.CheckedChanged += new System.EventHandler(this.OnConnectModeChanged);
            // 
            // radioVxi
            // 
            this.radioVxi.AutoSize = true;
            this.radioVxi.Location = new Point(244, 40);
            this.radioVxi.Name = "radioVxi";
            this.radioVxi.Size = new Size(115, 19);
            this.radioVxi.TabIndex = 3;
            this.radioVxi.TabStop = true;
            this.radioVxi.Text = "VXI-11 网络连接";
            this.radioVxi.UseVisualStyleBackColor = true;
            this.radioVxi.CheckedChanged += new System.EventHandler(this.OnConnectModeChanged);
            // 
            // radioTcp
            // 
            this.radioTcp.AutoSize = true;
            this.radioTcp.Location = new Point(380, 40);
            this.radioTcp.Name = "radioTcp";
            this.radioTcp.Size = new Size(99, 19);
            this.radioTcp.TabIndex = 4;
            this.radioTcp.TabStop = true;
            this.radioTcp.Text = "TCP/IP 直连";
            this.radioTcp.UseVisualStyleBackColor = true;
            this.radioTcp.CheckedChanged += new System.EventHandler(this.OnConnectModeChanged);
            // 
            // lblDevices
            // 
            this.lblDevices.AutoSize = true;
            this.lblDevices.Location = new Point(24, 76);
            this.lblDevices.Name = "lblDevices";
            this.lblDevices.Size = new Size(92, 15);
            this.lblDevices.TabIndex = 1;
            this.lblDevices.Text = "可用设备列表：";
            // 
            // lblTcpEndpoint
            // 
            this.lblTcpEndpoint.AutoSize = true;
            this.lblTcpEndpoint.Location = new Point(24, 112);
            this.lblTcpEndpoint.Name = "lblTcpEndpoint";
            this.lblTcpEndpoint.Size = new Size(92, 15);
            this.lblTcpEndpoint.TabIndex = 8;
            this.lblTcpEndpoint.Text = "TCP 地址端口：";
            // 
            // txtTcpEndpoint
            // 
            this.txtTcpEndpoint.Location = new Point(120, 108);
            this.txtTcpEndpoint.Name = "txtTcpEndpoint";
            this.txtTcpEndpoint.Size = new Size(360, 23);
            this.txtTcpEndpoint.TabIndex = 9;
            this.txtTcpEndpoint.Text = "192.168.0.100:5555";
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new Point(120, 148);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new Size(120, 27);
            this.btnConnect.TabIndex = 12;
            this.btnConnect.Text = "建立连接";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnDisconnect
            // 
            this.btnDisconnect.Location = new Point(256, 148);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new Size(120, 27);
            this.btnDisconnect.TabIndex = 13;
            this.btnDisconnect.Text = "断开连接";
            this.btnDisconnect.UseVisualStyleBackColor = true;
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
            // 
            // txtCommand
            // 
            this.txtCommand.Location = new Point(120, 200);
            this.txtCommand.Name = "txtCommand";
            this.txtCommand.Size = new Size(360, 23);
            this.txtCommand.TabIndex = 15;
            this.txtCommand.Text = "*IDN?";
            // 
            // btnSend
            // 
            this.btnSend.Location = new Point(496, 198);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new Size(96, 27);
            this.btnSend.TabIndex = 16;
            this.btnSend.Text = "发送命令";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // txtResponse
            // 
            this.txtResponse.Location = new Point(24, 264);
            this.txtResponse.Multiline = true;
            this.txtResponse.Name = "txtResponse";
            this.txtResponse.ReadOnly = true;
            this.txtResponse.ScrollBars = ScrollBars.Vertical;
            this.txtResponse.Size = new Size(704, 168);
            this.txtResponse.TabIndex = 18;
            // 
            // lblResponse
            // 
            this.lblResponse.AutoSize = true;
            this.lblResponse.Location = new Point(24, 240);
            this.lblResponse.Name = "lblResponse";
            this.lblResponse.Size = new Size(116, 15);
            this.lblResponse.TabIndex = 17;
            this.lblResponse.Text = "示波器返回内容：";
            // 
            // lblStatus
            // 
            this.lblStatus.BorderStyle = BorderStyle.FixedSingle;
            this.lblStatus.ForeColor = Color.DarkSlateGray;
            this.lblStatus.Location = new Point(24, 448);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new Size(704, 26);
            this.lblStatus.TabIndex = 19;
            this.lblStatus.Text = "准备就绪";
            this.lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblMode
            // 
            this.lblMode.AutoSize = true;
            this.lblMode.Location = new Point(24, 42);
            this.lblMode.Name = "lblMode";
            this.lblMode.Size = new Size(92, 15);
            this.lblMode.TabIndex = 0;
            this.lblMode.Text = "连接方式选择：";
            // 
            // lblCommand
            // 
            this.lblCommand.AutoSize = true;
            this.lblCommand.Location = new Point(24, 204);
            this.lblCommand.Name = "lblCommand";
            this.lblCommand.Size = new Size(92, 15);
            this.lblCommand.TabIndex = 14;
            this.lblCommand.Text = "SCPI 控制命令：";
            // 
            // txtVxiDeviceName
            // 
            this.txtVxiDeviceName.Location = new Point(576, 108);
            this.txtVxiDeviceName.Name = "txtVxiDeviceName";
            this.txtVxiDeviceName.Size = new Size(152, 23);
            this.txtVxiDeviceName.TabIndex = 11;
            this.txtVxiDeviceName.Text = "inst0";
            // 
            // lblVxiDeviceName
            // 
            this.lblVxiDeviceName.AutoSize = true;
            this.lblVxiDeviceName.Location = new Point(496, 112);
            this.lblVxiDeviceName.Name = "lblVxiDeviceName";
            this.lblVxiDeviceName.Size = new Size(76, 15);
            this.lblVxiDeviceName.TabIndex = 10;
            this.lblVxiDeviceName.Text = "VXI 设备名：";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(752, 493);
            this.Controls.Add(this.lblVxiDeviceName);
            this.Controls.Add(this.txtVxiDeviceName);
            this.Controls.Add(this.lblCommand);
            this.Controls.Add(this.lblMode);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblResponse);
            this.Controls.Add(this.txtResponse);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.txtCommand);
            this.Controls.Add(this.btnDisconnect);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.txtTcpEndpoint);
            this.Controls.Add(this.lblTcpEndpoint);
            this.Controls.Add(this.lblDevices);
            this.Controls.Add(this.radioTcp);
            this.Controls.Add(this.radioVxi);
            this.Controls.Add(this.radioUsb);
            this.Controls.Add(this.btnInstallDriver);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.comboDevices);
            this.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "SCPI 连接示例 - Demo";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
