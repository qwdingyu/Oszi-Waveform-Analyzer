using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace ScpiTransport
{
    /// <summary>
    /// 通过 Windows USBTMC 驱动 (ausbtmc.sys) 直接访问示波器的封装。
    /// 该类完全采用重叠 I/O，同步 API 仍然保持非阻塞特性。
    /// </summary>
    internal sealed class UsbTmcDevice : IDisposable
    {
        private static readonly IntPtr InvalidHandleValue = new IntPtr(-1);

        private const int ErrorFileNotFound = 2;
        private const int ErrorGenFailure  = 31;
        private const int ErrorIoPending   = 997;
        private const int WaitTimeout      = 258;

        private IntPtr _handle;
        private NativeOverlapped _overlapped;

        /// <summary>
        /// 构造函数会立即打开 USBTMC 设备句柄并创建事件对象。
        /// </summary>
        /// <param name="device">需要连接的设备信息。</param>
        public UsbTmcDevice(ScpiDeviceInfo device)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            _handle = CreateFileW(
                device.DevicePath,
                FileAccessFlags.GenericRead | FileAccessFlags.GenericWrite,
                FileShareFlags.Read | FileShareFlags.Write,
                IntPtr.Zero,
                CreationDisposition.OpenExisting,
                FileAttributeFlags.Overlapped,
                IntPtr.Zero);

            if (_handle == InvalidHandleValue)
            {
                var error = Marshal.GetLastWin32Error();
                if (error == ErrorFileNotFound)
                {
                    throw new InvalidOperationException("USBTMC 设备不存在或已经被拔出。");
                }

                throw new Win32Exception(error);
            }

            _overlapped.EventHandle = CreateEventW(IntPtr.Zero, true, false, null);
            if (_overlapped.EventHandle == IntPtr.Zero)
            {
                Dispose();
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        /// <summary>
        /// 取消所有挂起的 I/O 操作，避免驱动在下一次访问时仍旧返回超时。
        /// </summary>
        public void CancelTransfer()
        {
            if (_handle != IntPtr.Zero)
            {
                CancelIo(_handle);
            }
        }

        /// <summary>
        /// 发送完整的 USB Bulk OUT 数据包。
        /// </summary>
        /// <param name="packet">需要发送的字节数组。</param>
        /// <param name="timeout">等待驱动完成的超时时间，单位毫秒。</param>
        public void Send(byte[] packet, int timeout)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            CancelTransfer();

            if (!WriteFile(_handle, packet, packet.Length, out var written, ref _overlapped))
            {
                var error = Marshal.GetLastWin32Error();
                if (error != ErrorIoPending)
                {
                    throw new Win32Exception(error);
                }

                if (WaitForSingleObject(_overlapped.EventHandle, timeout) == WaitTimeout)
                {
                    CancelIo(_handle);
                    throw new TimeoutException("发送 USBTMC 数据包超时，设备未在期望时间内响应。");
                }

                if (!GetOverlappedResult(_handle, ref _overlapped, out written, false))
                {
                    error = Marshal.GetLastWin32Error();
                    if (error == ErrorGenFailure)
                    {
                        throw new InvalidOperationException("USB 设备当前不可用，通常发生在系统休眠恢复之后，请重新连接设备。");
                    }

                    throw new Win32Exception(error);
                }
            }

            if (written != packet.Length)
            {
                throw new IOException("写入 USBTMC 数据时发生未知错误，已发送字节数与期望不一致。");
            }
        }

        /// <summary>
        /// 接收 USB Bulk IN 数据包。
        /// </summary>
        /// <param name="buffer">用于承载返回数据的缓冲区。</param>
        /// <param name="timeout">等待设备返回的超时时间。</param>
        /// <returns>实际读取到的字节数。</returns>
        public int Receive(byte[] buffer, int timeout)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (!ReadFile(_handle, buffer, buffer.Length, out var read, ref _overlapped))
            {
                var error = Marshal.GetLastWin32Error();
                if (error != ErrorIoPending)
                {
                    throw new Win32Exception(error);
                }

                if (WaitForSingleObject(_overlapped.EventHandle, timeout) == WaitTimeout)
                {
                    Debug.Print("USB 读取超时");
                    CancelIo(_handle);
                    throw new TimeoutException("等待 USBTMC 设备返回数据超时，可能是命令无效或设备繁忙。");
                }

                if (!GetOverlappedResult(_handle, ref _overlapped, out read, false))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }

            return read;
        }

        /// <summary>
        /// 释放句柄和事件资源。
        /// </summary>
        public void Dispose()
        {
            if (_overlapped.EventHandle != IntPtr.Zero)
            {
                CloseHandle(_overlapped.EventHandle);
                _overlapped.EventHandle = IntPtr.Zero;
            }

            if (_handle != IntPtr.Zero && _handle != InvalidHandleValue)
            {
                CloseHandle(_handle);
                _handle = IntPtr.Zero;
            }
        }

        [Flags]
        private enum FileAccessFlags : uint
        {
            GenericRead  = 0x80000000,
            GenericWrite = 0x40000000,
        }

        [Flags]
        private enum FileShareFlags : uint
        {
            Read  = 0x01,
            Write = 0x02,
        }

        private enum CreationDisposition : uint
        {
            OpenExisting = 3,
        }

        [Flags]
        private enum FileAttributeFlags : uint
        {
            None       = 0,
            Overlapped = 0x40000000,
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateFileW(
            string fileName,
            FileAccessFlags desiredAccess,
            FileShareFlags shareMode,
            IntPtr securityAttributes,
            CreationDisposition creationDisposition,
            FileAttributeFlags flagsAndAttributes,
            IntPtr templateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadFile(
            IntPtr handle,
            byte[] buffer,
            int numberOfBytesToRead,
            out int numberOfBytesRead,
            ref NativeOverlapped overlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteFile(
            IntPtr handle,
            byte[] buffer,
            int numberOfBytesToWrite,
            out int numberOfBytesWritten,
            ref NativeOverlapped overlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetOverlappedResult(
            IntPtr handle,
            ref NativeOverlapped overlapped,
            out int numberOfBytesTransferred,
            bool wait);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int WaitForSingleObject(IntPtr handle, int milliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CancelIo(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateEventW(IntPtr lpEventAttributes, bool manualReset, bool initialState, string name);
    }
}
