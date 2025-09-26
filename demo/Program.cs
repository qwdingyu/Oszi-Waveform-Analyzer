using System;
using System.Windows.Forms;

namespace ScpiTransport.Demo
{
    /// <summary>
    /// 程序入口，用于初始化 WinForms 环境后启动主界面。
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// 由于需要访问串口、网口等资源，整个应用程序以单线程单元模式启动。
        /// </summary>
        [STAThread]
        private static void Main()
        {
#if NET6_0_OR_GREATER
            // .NET 6/7 提供了新的高 DPI 配置 API，优先调用以提升界面显示效果。
            ApplicationConfiguration.Initialize();
#else
            // .NET Framework 4.8 仍旧使用传统 API 启用视觉样式和文本渲染配置。
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
#endif
            Application.Run(new MainForm());
        }
    }
}
