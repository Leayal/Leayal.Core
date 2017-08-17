using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Leayal
{
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        internal static extern bool QueryFullProcessImageName(IntPtr hprocess, int dwFlags, StringBuilder lpExeName, out int size);
        [DllImport("kernel32.dll")]
        internal static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr hHandle);
    }
}
