using System;
using System.Management;
using System.Diagnostics;

namespace Leayal.WMI
{
    public static class ProcessParent
    {
        public static int GetParentProcessID(int processID)
        {
            int result = -1;
            using (ManagementObjectSearcher search = new ManagementObjectSearcher("root\\CIMV2", string.Format("SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {0}", processID)))
            using (var results = search.Get().GetEnumerator())
                if (results.MoveNext())
                    result = Convert.ToInt32(results.Current["ParentProcessId"]);
            return result;
        }

        public static int GetParentProcessID(Process process)
        {
            return GetParentProcessID(process.Id);
        }

        /// <summary>
        /// Return the <see cref="System.Diagnostics.Process"/> which started current process. Or null if parent process is not found or already closed.
        /// </summary>
        /// <returns></returns>
        public static Process GetParentProcess(Process process)
        {
            int id = GetParentProcessID(process);
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

        private static int cacheCurrentProcessParentID = -2;
        public static int GetCurrentProcessParentID()
        {
            if (cacheCurrentProcessParentID == -2)
                cacheCurrentProcessParentID = GetParentProcessID(AppInfo.CurrentProcessID);
            return cacheCurrentProcessParentID;
        }
    }
}
