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

        private static Process _currentprocess;
        public static Process CurrentProcess
        {
            get
            {
                if (_currentprocess == null)
                    _currentprocess = Process.GetCurrentProcess();
                return _currentprocess;
            }
        }
        private static string _appFilename;
        public static string ApplicationFilename
        {
            get
            {
                if (string.IsNullOrEmpty(_appFilename))
                    _appFilename = CurrentProcess.MainModule.FileName;
                return _appFilename;
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
