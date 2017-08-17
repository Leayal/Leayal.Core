using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Leayal
{
    public static class ProcessHelper
    {
        public static string TableStringToArgs(IEnumerable<string> arg)
        {
            if (arg != null)
            {
                string dun = arg.First();
                if (dun != null)
                {
                    System.Text.StringBuilder builder = new System.Text.StringBuilder();
                    if (!string.IsNullOrWhiteSpace(dun))
                    {
                        if (dun.IndexOf(" ") > -1)
                            builder.AppendFormat("\"{0}\"", dun);
                        else
                            builder.Append(dun);
                    }
                    foreach (string tmp in arg.Skip(1))
                        if (!string.IsNullOrWhiteSpace(tmp))
                        {
                            if (tmp.IndexOf(" ") > -1)
                                builder.AppendFormat(" \"{0}\"", tmp);
                            else
                                builder.Append(" " + tmp);
                        }
                    return builder.ToString();
                }
                else { return string.Empty; }
            }
            else { return string.Empty; }
        }

        public static string TableStringToArgs(params string[] arg)
        {
            if ((arg != null) && (arg.Length > 0))
            {
                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                string tmp;
                for (int i = 0; i < arg.Length; i++)
                {
                    tmp = arg[i];
                    if (!string.IsNullOrWhiteSpace(tmp))
                    {
                        if (i == 0)
                        {
                            if (tmp.IndexOf(" ") > -1)
                                builder.AppendFormat("\"{0}\"", tmp);
                            else
                                builder.Append(tmp);
                        }
                        else
                        {
                            if (tmp.IndexOf(" ") > -1)
                                builder.AppendFormat(" \"{0}\"", tmp);
                            else
                                builder.Append(" " + tmp);
                        }
                    }
                }
                return builder.ToString();
            }
            else { return string.Empty; }
        }

        public static string GetProcessImagePath(int targetID)
        {
            string result = null;

            for (int i = 0; i < 3; i++)
                try
                {
                    // To check if the process is running or not
                    using (Process proc = Process.GetProcessById(targetID))
                    {
                        if (Environment.OSVersion.Version.Major >= 6)
                            result = GetExecutablePathAboveVista(proc.Id);
                        else
                            result = proc.MainModule.FileName;
                    }
                    break;
                }
                catch (Win32Exception)
                {
                    result = null;
                }
            return result;
        }

        public static string GetProcessImagePath(Process target)
        {
            string result = null;

            for (int i = 0; i < 3; i++)
                try
                {
                    if (Environment.OSVersion.Version.Major >= 6)
                        result = GetExecutablePathAboveVista(target.Id);
                    else
                        result = target.MainModule.FileName;
                    break;
                }
                catch (Win32Exception)
                {
                    result = null;
                }
            return result;
        }

        private static string GetExecutablePathAboveVista(int ProcessId)
        {
            var buffer = new StringBuilder(1024);
            IntPtr hprocess = NativeMethods.OpenProcess(0x1000, false, ProcessId);
            if (hprocess != IntPtr.Zero)
            {
                try
                {
                    int size = buffer.Capacity;
                    if (NativeMethods.QueryFullProcessImageName(hprocess, 0, buffer, out size))
                    {
                        return buffer.ToString();
                    }
                }
                finally
                {
                    NativeMethods.CloseHandle(hprocess);
                }
            }
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
}
