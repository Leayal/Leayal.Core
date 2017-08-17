using System;
using System.Management;
using System.Diagnostics;

namespace Leayal.WMI
{
    public static class ProcessParent
    {
        private static int cacheGetParentProcessID = -2;
        public static int GetParentProcessID()
        {
            if (cacheGetParentProcessID == -2)
                using (ManagementObjectSearcher search = new ManagementObjectSearcher("root\\CIMV2", string.Format("SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {0}", AppInfo.CurrentProcess.Id)))
                using (var results = search.Get().GetEnumerator())
                {
                    if (results.MoveNext())
                        cacheGetParentProcessID = Convert.ToInt32(results.Current["ParentProcessId"]);
                    else
                        cacheGetParentProcessID = -1;
                }
            return cacheGetParentProcessID;
        }

        /// <summary>
        /// Return the <see cref="System.Diagnostics.Process"/> which started current process. Or null if parent process is not found or already closed.
        /// </summary>
        /// <returns></returns>
        public static Process GetParentProcess()
        {
            int id = GetParentProcessID();
            if (id > -1)
            {
                try { return Process.GetProcessById(id); }
                catch (ArgumentException)
                {
                    // Parent process has closed before this GetProcessById is called.
                    return null;
                }
            }
            else
                return null;
        }
    }
}
