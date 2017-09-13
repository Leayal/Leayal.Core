using System.Diagnostics;
using Microsoft.VisualBasic.Devices;
using Microsoft.VisualBasic.ApplicationServices;
using System.Reflection;
using Microsoft.IO;

namespace Leayal
{
    public static class AppInfo
    {
        internal static RecyclableMemoryStreamManager _MemoryStreamManager;
        public static RecyclableMemoryStreamManager MemoryStreamManager
        {
            get
            {
                if (_MemoryStreamManager == null)
                    _MemoryStreamManager = new RecyclableMemoryStreamManager();
                return _MemoryStreamManager;
            }
        }
        private static ComputerInfo _compInfo = new ComputerInfo();
        public static ComputerInfo ComputerInfo
        { get { return _compInfo; } }
        private static AssemblyInfo _entryassemblyInfo;
        public static AssemblyInfo EntryAssemblyInfo
        {
            get
            {
                if (_entryassemblyInfo == null)
                    _entryassemblyInfo = new AssemblyInfo(EntryAssembly);
                return _entryassemblyInfo;
            }
        }
        private static string _processFullpath;
        public static string ProcessFullpath
        {
            get
            {
                if (string.IsNullOrEmpty(_processFullpath))
                    GetProcessInfo();
                return _processFullpath;
            }
        }
        private static int _currentprocessID = -1;
        public static int CurrentProcessID
        {
            get
            {
                if (_currentprocessID == -1)
                    GetProcessInfo();
                return _currentprocessID;
            }
        }
        private static void GetProcessInfo()
        {
            using (Process proc = Process.GetCurrentProcess())
            {
                _currentprocessID = proc.Id;
                _processFullpath = proc.MainModule.FileName;
            }
        }
        private static Assembly _entryAssembly;
        public static Assembly EntryAssembly
        {
            get
            {
                if (_entryAssembly == null)
                    _entryAssembly = Assembly.GetEntryAssembly();
                return _entryAssembly;
            }
        }
    }
}
