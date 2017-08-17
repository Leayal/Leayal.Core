using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Management;
using System.IO;

namespace Leayal.WMI
{
    /// <summary>
    /// May require Administration or elevated access.
    /// </summary>
    public class ProcessesWatcher : IDisposable
    {

        private static ProcessesWatcher _instance;
        public static ProcessesWatcher Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ProcessesWatcher();
                return _instance;
            }
        }

        public static ProcessWatcher GetWatcher(string processPath)
        {
            return Instance.GetWatcherEx(processPath);
        }

        private ConcurrentDictionary<string, ProcessWatcher> myDict;
        public ProcessesWatcher()
        {
            this.myDict = new ConcurrentDictionary<string, ProcessWatcher>(StringComparer.OrdinalIgnoreCase);
        }

        public ProcessWatcher GetWatcherEx(string processPath)
        {
            ProcessWatcher functionReturnValue = null;
            if (!this.myDict.IsEmpty)
            {
                if (this.myDict.TryGetValue(processPath, out functionReturnValue))
                    return functionReturnValue;
            }
            if (functionReturnValue == null)
            {
                functionReturnValue = new ProcessWatcher(processPath);
                this.myDict.TryAdd(processPath.ToLower(), functionReturnValue);
            }
            return functionReturnValue;
        }

        public void RemoveWatcherEx(string processPath)
        {
            if (!this.myDict.IsEmpty)
            {
                if (this.myDict.TryRemove(processPath, out var sumthin))
                    sumthin.Dispose();
            }
        }

        public void Dispose()
        {
            foreach (var asd in myDict.Values)
                asd.Dispose();
            this.myDict.Clear();
            this.myDict = null;
        }
    }

    /// <summary>
    /// May require Administration or elevated access. This class should be disposed if you're finished with it.
    /// </summary>
    public class ProcessWatcher : IDisposable
    {
        public bool IsRunning
        {
            get
            {
                if ((this.ProcessInstance == null))
                {
                    return false;
                }
                else
                {
                    return (!this.ProcessInstance.HasExited);
                }
            }
        }
        private Process withEventsField__ProcessInstance;
        private Process _ProcessInstance
        {
            get { return withEventsField__ProcessInstance; }
            set
            {
                if (withEventsField__ProcessInstance != null)
                {
                    withEventsField__ProcessInstance.Exited -= _ProcessInstance_Exited;
                }
                withEventsField__ProcessInstance = value;
                if (withEventsField__ProcessInstance != null)
                {
                    withEventsField__ProcessInstance.Exited += _ProcessInstance_Exited;
                }
            }
        }
        public Process ProcessInstance
        {
            get { return this._ProcessInstance; }
        }
        private string _ProcessPath;
        public string ProcessPath
        {
            get { return this._ProcessPath; }
        }

        private ManagementEventWatcher withEventsField_processStartEvent;
        private ManagementEventWatcher processStartEvent
        {
            get { return withEventsField_processStartEvent; }
            set
            {
                if (withEventsField_processStartEvent != null)
                {
                    withEventsField_processStartEvent.EventArrived -= processStartEvent_EventArrived;
                }
                withEventsField_processStartEvent = value;
                if (withEventsField_processStartEvent != null)
                {
                    withEventsField_processStartEvent.EventArrived += processStartEvent_EventArrived;
                }
            }

        }
        public ProcessWatcher(string processPath) : this(processPath, null) { }

        public ProcessWatcher(string processPath, EventHandler processLaunchedHandler)
        {
            this._ProcessPath = processPath;
            if (processLaunchedHandler != null)
                this.ProcessLaunched += processLaunchedHandler;
            Process myprocess = FindProcess(processPath);
            this.processStartEvent = new ManagementEventWatcher("SELECT ProcessID FROM Win32_ProcessStartTrace WHERE ProcessName = '" + Path.GetFileName(this.ProcessPath) + "'");
            if (myprocess != null)
            {
                this.SetProcess(myprocess);
                this.processStartEvent.Stop();
            }
            else
            {
                this._ProcessInstance = null;
                this.processStartEvent.Start();
            }
        }

        private Process FindProcess(string filename)
        {
            Process functionReturnValue = null;
            functionReturnValue = null;
            Process[] myList = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(filename));
            if (myList != null && myList.Length > 0)
            {
                if (Path.IsPathRooted(filename))
                {
                    for (int i = 0; i <= myList.Length - 1; i++)
                    {
                        if (Leayal.StringHelper.IsEqual(Leayal.ProcessHelper.GetProcessImagePath(myList[i]), filename, true))
                            functionReturnValue = myList[i];
                        else
                            myList[i].Close();
                    }
                }
                else
                {
                    for (int i = 0; i <= myList.Length - 1; i++)
                    {
                        if (Leayal.ProcessHelper.GetProcessImagePath(myList[i]).EndsWith(filename, StringComparison.OrdinalIgnoreCase))
                            functionReturnValue = myList[i];
                        else
                            myList[i].Close();
                    }
                }
                myList = null;
            }
            return functionReturnValue;
        }


        public void SetPath(string str)
        {
            string asdasdasd = Path.GetFileName(str);

            if ((Path.GetFileName(this.ProcessPath).ToLower() != asdasdasd))
            {
                if ((this.processStartEvent != null))
                {
                    this.processStartEvent.Stop();
                    this.processStartEvent.Dispose();
                }
                this.processStartEvent = new ManagementEventWatcher("SELECT ProcessID FROM Win32_ProcessStartTrace WHERE ProcessName = '" + asdasdasd + "'");
            }

            Process myprocess = FindProcess(str);
            this._ProcessPath = str;
            if ((myprocess != null))
            {
                this.SetProcess(myprocess);
                this.processStartEvent.Stop();
            }
            else
            {
                this._ProcessInstance = null;
                this.processStartEvent.Start();
            }
        }

        private void SetProcess(Process myprocess)
        {
            this._ProcessInstance = myprocess;
            this._ProcessInstance.EnableRaisingEvents = true;
            this.OnProcessLaunched(EventArgs.Empty);
        }

        private void processStartEvent_EventArrived(object sender, EventArrivedEventArgs e)
        {
            try
            {
                int myProcID = Convert.ToInt32(e.NewEvent.Properties["ProcessID"].Value);
                string pathString = Leayal.ProcessHelper.GetProcessImagePath(myProcID);
                if (!string.IsNullOrEmpty(pathString))
                {
                    if ((pathString.ToLower() == this.ProcessPath.ToLower()))
                    {
                        this.SetProcess(Process.GetProcessById(myProcID));
                        this.processStartEvent.Stop();
                    }
                }
            }
            catch (ArgumentException)
            {
                if ((this._ProcessInstance != null))
                    this._ProcessInstance.Close();
                this._ProcessInstance = null;
            }
        }

        private void _ProcessInstance_Exited(object sender, EventArgs e)
        {
            if ((this._ProcessInstance != null))
                this._ProcessInstance.Close();
            this._ProcessInstance = null;
            this.processStartEvent.Start();
            this.OnProcessExited(EventArgs.Empty);
        }

        public event EventHandler ProcessExited;
        protected virtual void OnProcessExited(EventArgs e)
        {
            this.ProcessExited?.Invoke(this, e);
        }
        public event EventHandler ProcessLaunched;
        protected virtual void OnProcessLaunched(EventArgs e)
        {
            this.ProcessLaunched?.Invoke(this, e);
        }

        public void Dispose()
        {
            if ((this._ProcessInstance != null))
            {
                this._ProcessInstance.EnableRaisingEvents = false;
                this._ProcessInstance.Close();
            }
            this._ProcessInstance = null;
            if ((this.processStartEvent != null))
            {
                this.processStartEvent.Stop();
                this.processStartEvent.Dispose();
            }
            this.processStartEvent = null;
        }
    }
}
