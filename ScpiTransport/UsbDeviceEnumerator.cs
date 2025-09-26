using System;
using System.Collections.Generic;

#if NET48 || NET7_0_WINDOWS
using System.Management;
#endif

namespace ScpiTransport
{
    /// <summary>
    /// 提供对符合 USB Test and Measurement Class (USBTMC) 设备的扫描能力。
    /// 该实现直接依赖 WMI，因此仅在 Windows 平台可用。
    /// </summary>
    public static class UsbDeviceEnumerator
    {
        /// <summary>
        /// USBTMC 设备在系统中的 Class GUID，Rigol、Keysight 等厂商都遵循该值。
        /// </summary>
        public const string UsbTmcClassGuid = "{A9FDBB24-128A-11D5-9961-00108335E361}";

        /// <summary>
        /// 扫描当前操作系统中所有已连接的 USBTMC 设备，并返回可用于建立 SCPI 会话的路径。
        /// </summary>
        /// <returns>可枚举的 <see cref="ScpiDeviceInfo"/> 列表。</returns>
        /// <exception cref="PlatformNotSupportedException">当运行在非 Windows 平台时抛出。</exception>
        public static IReadOnlyList<ScpiDeviceInfo> Enumerate()
        {
            var devices = new List<ScpiDeviceInfo>();

#if NET48 || NET7_0_WINDOWS
            using (var managementClass = new ManagementClass("Win32_PnPEntity"))
            {
                foreach (ManagementObject instance in managementClass.GetInstances())
                {
                    var classGuid = instance.GetPropertyValue("ClassGuid")?.ToString();
                    if (!UsbTmcClassGuid.Equals(classGuid, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var deviceId = instance.GetPropertyValue("PnpDeviceID")?.ToString();
                    if (string.IsNullOrWhiteSpace(deviceId))
                    {
                        continue;
                    }

                    var symbolicLink = @"\\?\" + deviceId.Replace('\\', '#') + "#" + UsbTmcClassGuid;
                    var parts        = deviceId.Split('\\');
                    var serial       = parts.Length > 0 ? parts[parts.Length - 1].ToUpperInvariant() : deviceId;

                    devices.Add(new ScpiDeviceInfo(serial, symbolicLink));
                }
            }
#else
            throw new PlatformNotSupportedException("当前平台不支持 USBTMC 枚举，仅 Windows 提供该能力。");
#endif

            return devices;
        }
    }
}
