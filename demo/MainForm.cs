using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ScpiTransport;

namespace ScpiTransport.Demo
{
    /// <summary>
    /// 演示如何在 WinForms 环境中复用 ScpiTransport 类库的核心能力，完成示波器的枚举、连接与命令收发。
    /// </summary>
    public partial class MainForm : Form
    {
        /// <summary>
        /// 状态条颜色分级，便于在不同场景下快速区分提示严重程度。
        /// </summary>
        private enum StatusLevel
        {
            Info,
            Success,
            Warning,
            Error,
        }

        /// <summary>
        /// 下拉框中实际绑定的数据项，Display 用于界面展示，Value 用于保存真实对象。
        /// </summary>
        private sealed class DeviceItem
        {
            public DeviceItem(string display, object value)
            {
                Display = display;
                Value = value;
            }

            public string Display { get; }

            public object Value { get; }

            public override string ToString()
            {
                return Display;
            }
        }

        private readonly Timer mi_StatusTimer;
        private ScpiClient.eConnectMode me_CurrentMode = ScpiClient.eConnectMode.USB;
        private ScpiClient mi_Client;

        public MainForm()
        {
            InitializeComponent();

            mi_StatusTimer = new Timer
            {
                Interval = 4000,
            };
            mi_StatusTimer.Tick += OnStatusTimerTick;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // 默认选择 USB 模式，保证初次运行即可看到枚举结果。
            radioUsb.Checked = true;
            UpdateUiForMode();
            RefreshDeviceList();
            UpdateConnectionState();
        }

        private void OnConnectModeChanged(object sender, EventArgs e)
        {
            if (sender is not RadioButton radioButton || !radioButton.Checked)
            {
                return;
            }

            if (ReferenceEquals(radioButton, radioUsb))
            {
                me_CurrentMode = ScpiClient.eConnectMode.USB;
            }
            else if (ReferenceEquals(radioButton, radioVxi))
            {
                me_CurrentMode = ScpiClient.eConnectMode.VXI;
            }
            else if (ReferenceEquals(radioButton, radioTcp))
            {
                me_CurrentMode = ScpiClient.eConnectMode.TCP;
            }

            UpdateUiForMode();
            RefreshDeviceList();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshDeviceList();
        }

        private void btnInstallDriver_Click(object sender, EventArgs e)
        {
            try
            {
                string s_Installer = ResolveDriverInstallerPath();
                if (string.IsNullOrEmpty(s_Installer) || !File.Exists(s_Installer))
                {
                    ShowStatus("未找到驱动安装程序，请确认 Driver 目录已复制到输出文件夹。", StatusLevel.Error);
                    MessageBox.Show(this, "未找到 dpinst-amd64.exe，请检查 demo 项目的输出目录是否包含 Driver 子目录。", "驱动文件缺失", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                ProcessStartInfo k_Info = new ProcessStartInfo(s_Installer)
                {
                    UseShellExecute = true,
                    Verb = "runas",
                };
                Process.Start(k_Info);
                ShowStatus("已启动 Windows 驱动安装向导，如提示请允许获取管理员权限。", StatusLevel.Info);
            }
            catch (Exception ex)
            {
                ShowStatus("调用驱动安装向导失败：" + ex.Message, StatusLevel.Error);
                MessageBox.Show(this, ex.Message, "安装驱动时发生异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                DisconnectInternal();

                mi_Client = new ScpiClient();

                switch (me_CurrentMode)
                {
                    case ScpiClient.eConnectMode.USB:
                        ConnectUsb();
                        break;
                    case ScpiClient.eConnectMode.VXI:
                        ConnectVxi();
                        break;
                    case ScpiClient.eConnectMode.TCP:
                        ConnectTcp();
                        break;
                }

                ShowStatus("连接已建立，设备可以接收 SCPI 指令。", StatusLevel.Success);
            }
            catch (Exception ex)
            {
                DisconnectInternal();
                ShowStatus("建立连接失败：" + ex.Message, StatusLevel.Error);
                MessageBox.Show(this, ex.ToString(), "连接失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UpdateConnectionState();
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            DisconnectInternal();
            ShowStatus("连接已断开。", StatusLevel.Info);
            UpdateConnectionState();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (mi_Client == null)
            {
                ShowStatus("尚未建立连接，请先连接到示波器。", StatusLevel.Warning);
                return;
            }

            string s_Command = txtCommand.Text.Trim();
            if (string.IsNullOrEmpty(s_Command))
            {
                ShowStatus("请输入有效的 SCPI 命令。", StatusLevel.Warning);
                return;
            }

            try
            {
                bool b_ExpectResponse = s_Command.EndsWith("?", StringComparison.Ordinal);
                if (b_ExpectResponse)
                {
                    string s_Response = mi_Client.SendStringCommand(s_Command);
                    txtResponse.Text = s_Response;
                    ShowStatus("命令执行成功并收到响应。", StatusLevel.Success);
                }
                else
                {
                    mi_Client.SendOpcCommand(s_Command);
                    txtResponse.Text = "命令执行完成，设备未返回数据。";
                    ShowStatus("命令执行完成。", StatusLevel.Success);
                }
            }
            catch (Exception ex)
            {
                ShowStatus("命令执行失败：" + ex.Message, StatusLevel.Error);
                MessageBox.Show(this, ex.ToString(), "SCPI 命令失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            DisconnectInternal();
        }

        private void RefreshDeviceList()
        {
            comboDevices.BeginUpdate();
            comboDevices.Items.Clear();
            try
            {
                if (me_CurrentMode == ScpiClient.eConnectMode.USB)
                {
                    Cursor currentCursor = Cursor.Current;
                    Cursor.Current = Cursors.WaitCursor;
                    try
                    {
                        IReadOnlyList<ScpiDeviceInfo> i_Devices = UsbDeviceEnumerator.Enumerate();
                        foreach (ScpiDeviceInfo device in i_Devices)
                        {
                            comboDevices.Items.Add(new DeviceItem(device.DisplayName, device));
                        }

                        if (comboDevices.Items.Count > 0)
                        {
                            comboDevices.SelectedIndex = 0;
                            ShowStatus($"已发现 {comboDevices.Items.Count} 台 USB 示波器设备。", StatusLevel.Success);
                        }
                        else
                        {
                            ShowStatus("未检测到 USBTMC 设备，请确认仪器已接入电脑。", StatusLevel.Warning);
                        }
                    }
                    finally
                    {
                        Cursor.Current = currentCursor;
                    }
                }
                else if (me_CurrentMode == ScpiClient.eConnectMode.VXI)
                {
                    using (Vxi11Client vxi11 = new Vxi11Client())
                    {
                        IReadOnlyList<string> endpoints = vxi11.EnumerateDevices(true);
                        foreach (string endpoint in endpoints)
                        {
                            comboDevices.Items.Add(new DeviceItem(endpoint, endpoint));
                        }
                    }

                    if (comboDevices.Items.Count > 0)
                    {
                        comboDevices.SelectedIndex = 0;
                        ShowStatus($"收到 {comboDevices.Items.Count} 个 VXI-11 广播响应。", StatusLevel.Success);
                    }
                    else
                    {
                        ShowStatus("未收到任何 VXI-11 广播响应，可手动输入 IP 地址。", StatusLevel.Warning);
                    }
                }
                else
                {
                    ShowStatus("请在下方输入 TCP 服务器地址和端口，例如 192.168.0.100:5555。", StatusLevel.Info);
                }
            }
            catch (PlatformNotSupportedException ex)
            {
                ShowStatus("当前系统不支持所选的通信方式：" + ex.Message, StatusLevel.Error);
                MessageBox.Show(this, ex.Message, "平台限制", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                ShowStatus("枚举设备时发生异常：" + ex.Message, StatusLevel.Error);
                MessageBox.Show(this, ex.ToString(), "枚举失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                comboDevices.EndUpdate();
            }
        }

        private void UpdateUiForMode()
        {
            bool b_Usb = me_CurrentMode == ScpiClient.eConnectMode.USB;
            bool b_Vxi = me_CurrentMode == ScpiClient.eConnectMode.VXI;
            bool b_Tcp = me_CurrentMode == ScpiClient.eConnectMode.TCP;

            comboDevices.Visible = b_Usb || b_Vxi;
            lblDevices.Visible = comboDevices.Visible;
            btnRefresh.Visible = comboDevices.Visible;
            btnInstallDriver.Visible = b_Usb;
            if (b_Usb)
            {
                btnInstallDriver.Enabled = File.Exists(ResolveDriverInstallerPath());
            }

            lblTcpEndpoint.Visible = b_Tcp;
            txtTcpEndpoint.Visible = b_Tcp;

            lblVxiDeviceName.Visible = b_Vxi;
            txtVxiDeviceName.Visible = b_Vxi;

            comboDevices.DropDownStyle = b_Vxi ? ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList;

            if (b_Tcp)
            {
                comboDevices.Items.Clear();
            }
        }

        private void UpdateConnectionState()
        {
            bool b_IsConnected = mi_Client != null;
            btnConnect.Enabled = !b_IsConnected;
            btnDisconnect.Enabled = b_IsConnected;
            btnSend.Enabled = b_IsConnected;
        }

        private void ConnectUsb()
        {
            if (comboDevices.SelectedItem is not DeviceItem item || item.Value is not ScpiDeviceInfo deviceInfo)
            {
                throw new InvalidOperationException("请先在列表中选择一台 USB 示波器设备。");
            }

            mi_Client.ConnectUsb(deviceInfo);
        }

        private void ConnectVxi()
        {
            string s_Endpoint = comboDevices.Text.Trim();
            if (string.IsNullOrEmpty(s_Endpoint))
            {
                throw new InvalidOperationException("请输入 VXI-11 设备的 IP 地址或选择广播结果。");
            }

            string s_DeviceName = txtVxiDeviceName.Text.Trim();
            if (string.IsNullOrEmpty(s_DeviceName))
            {
                throw new InvalidOperationException("VXI-11 设备名不能为空，例如 inst0。");
            }

            mi_Client.ConnectVxi(s_Endpoint, s_DeviceName);
        }

        private void ConnectTcp()
        {
            string s_Endpoint = txtTcpEndpoint.Text.Trim();
            if (string.IsNullOrEmpty(s_Endpoint))
            {
                throw new InvalidOperationException("请输入有效的 TCP 地址，格式如 192.168.0.100:5555。");
            }

            mi_Client.ConnectTcp(s_Endpoint);
        }

        private void DisconnectInternal()
        {
            if (mi_Client != null)
            {
                mi_Client.Dispose();
                mi_Client = null;
            }
        }

        private void ShowStatus(string s_Message, StatusLevel e_Level)
        {
            lblStatus.Text = s_Message;
            switch (e_Level)
            {
                case StatusLevel.Success:
                    lblStatus.ForeColor = Color.SeaGreen;
                    break;
                case StatusLevel.Warning:
                    lblStatus.ForeColor = Color.DarkOrange;
                    break;
                case StatusLevel.Error:
                    lblStatus.ForeColor = Color.Firebrick;
                    break;
                default:
                    lblStatus.ForeColor = Color.DarkSlateGray;
                    break;
            }

            mi_StatusTimer.Stop();
            mi_StatusTimer.Start();
        }

        private void OnStatusTimerTick(object sender, EventArgs e)
        {
            mi_StatusTimer.Stop();
            lblStatus.Text = "准备就绪";
            lblStatus.ForeColor = Color.DarkSlateGray;
        }

        private static string ResolveDriverInstallerPath()
        {
            string baseDirectory = AppContext.BaseDirectory;
            string installerPath = Path.Combine(baseDirectory, "Driver", "dpinst-amd64.exe");
            return installerPath;
        }
    }
}
