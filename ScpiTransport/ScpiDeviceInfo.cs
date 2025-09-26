using System;

namespace ScpiTransport
{
    /// <summary>
    /// 表示一个可被 SCPI 会话使用的物理或逻辑设备路径。
    /// </summary>
    public sealed class ScpiDeviceInfo
    {
        /// <summary>
        /// 设备在界面或日志中显示的友好名称。
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// 用于真正打开设备的底层路径，USB 模式为符号链接，其他模式可以是任意标识。
        /// </summary>
        public string DevicePath { get; }

        /// <summary>
        /// 初始化设备信息对象。
        /// </summary>
        /// <param name="displayName">供人阅读的名称，例如序列号。</param>
        /// <param name="devicePath">系统调用所需的完整路径。</param>
        public ScpiDeviceInfo(string displayName, string devicePath)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("displayName 不能为空", nameof(displayName));
            }

            if (string.IsNullOrWhiteSpace(devicePath))
            {
                throw new ArgumentException("devicePath 不能为空", nameof(devicePath));
            }

            DisplayName = displayName;
            DevicePath  = devicePath;
        }

        /// <summary>
        /// 返回用于界面显示的名称，便于直接绑定到列表控件。
        /// </summary>
        public override string ToString()
        {
            return DisplayName;
        }
    }
}
