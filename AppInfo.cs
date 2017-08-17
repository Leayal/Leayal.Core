using System.Diagnostics;
using Microsoft.VisualBasic.Devices;
using Microsoft.VisualBasic.ApplicationServices;
using System.Reflection;
using Microsoft.IO;

namespace Leayal
{
    public static class AppInfo
    {
        private static System.Threading.SynchronizationContext _mainsynchronizationcontext = System.Threading.SynchronizationContext.Current;
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
        public static System.Threading.SynchronizationContext MainSynchronizationContext { get { return _mainsynchronizationcontext; } }
        private static ComputerInfo _compInfo = new ComputerInfo();
        public static ComputerInfo ComputerInfo
        { get { return _compInfo; } }
        private static AssemblyInfo _entryassemblyInfo = new AssemblyInfo(CurrentAssembly);
        public static AssemblyInfo AssemblyInfo
        { get { return _entryassemblyInfo; } }

        private static AssemblyInfo _currentAssemblyInfo = new AssemblyInfo(Assembly.GetExecutingAssembly());
        public static AssemblyInfo CurrentAssemblyInfo
        { get { return _currentAssemblyInfo; } }

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
        private static Assembly _currentAssembly;
        public static Assembly CurrentAssembly
        {
            get
            {
                if (_currentAssembly == null)
                    _currentAssembly = Assembly.GetEntryAssembly();
                return _currentAssembly;
            }
        }
    }
}
