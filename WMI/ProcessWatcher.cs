using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Management;
using System.IO;
using System.Collections.Generic;

namespace Leayal.WMI
{
    /// <summary>
    /// May require Administration or elevated access.
    /// </summary>
    public sealed class ProcessWatcherManager : IDisposable
    {

        private static ProcessWatcherManager _instance;
        public static ProcessWatcherManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ProcessWatcherManager();
                return _instance;
            }
        }

        public static ProcessWatcher GetWatcher(string processPath)
        {
            return Instance.GetWatcherEx(processPath);
        }

        private ConcurrentDictionary<string, ProcessWatcher> myDict;
        public ProcessWatcherManager()
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
    /// Provides a process listener that will raise even whenever any processes match the search. May require Administration or elevated access. This class should be disposed if you're finished with it.
    /// </summary>
    public sealed class ProcessesWatcher : IDisposable
    {
        private bool _islistening, isProcessNameOnly;
        public bool IsListening => this._islistening;
        public bool HasProcesses => (this.ProcessesCount != 0);
        public int ProcessesCount => this.processList.Count;

        private Dictionary<Process, string> processList;

        public IEnumerable<Process> Processes => this.processList.Keys;

        private string _ProcessPath;
        public string ProcessPath
        {
            get { return this._ProcessPath; }
        }

        private ManagementEventWatcher processStartEvent;

        private void CreateManagementEventWatcher(string processPath)
        {
            if (this.processStartEvent != null)
            {
                this.processStartEvent.EventArrived -= processStartEvent_EventArrived;
                this.processStartEvent.Dispose();
            }
            this._ProcessPath = processPath;
            this.processStartEvent = new ManagementEventWatcher("SELECT ProcessID FROM Win32_ProcessStartTrace WHERE ProcessName = '" + Path.GetFileName(processPath) + "'");
            if (this.processStartEvent != null)
                this.processStartEvent.EventArrived += processStartEvent_EventArrived;
        }

        public ProcessesWatcher(string processPath)
        {
            this._islistening = false;
            this.isProcessNameOnly = (processPath.IsEqual(Path.GetFileName(processPath), true));
            this._ProcessPath = processPath;
            this.processList = new Dictionary<Process, string>();
            this.CreateManagementEventWatcher(processPath);
        }

        /// <summary>
        /// Start listening for process launching. This method will raise the event <see cref="ProcessLaunched"/> if it find any processes which are running before <see cref="ProcessesWatcher"/> listens.
        /// </summary>
        public void StartListen()
        {
            if (this.IsListening)
                throw new InvalidOperationException();

            this._islistening = true;

            Process[] myList = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(this.ProcessPath));
            if (myList != null && myList.Length > 0)
            {
                string currentprocessPath;
                if (!this.isProcessNameOnly)
                {
                    for (int i = 0; i <= myList.Length - 1; i++)
                    {
                        currentprocessPath = ProcessHelper.GetProcessImagePath(myList[i]);
                        if (currentprocessPath.IsEqual(this.ProcessPath, true))
                            this.ProcessAdd(myList[i], currentprocessPath);
                        else
                            myList[i].Dispose();
                    }
                }
                else
                {
                    for (int i = 0; i <= myList.Length - 1; i++)
                    {
                        currentprocessPath = ProcessHelper.GetProcessImagePath(myList[i]);
                        if (currentprocessPath.EndsWith(this.ProcessPath, StringComparison.OrdinalIgnoreCase))
                            this.ProcessAdd(myList[i], currentprocessPath);
                        else
                            myList[i].Dispose();
                    }
                }
                myList = null;
            }
            
            this.processStartEvent.Start();
        }

        public void StopListen()
        {
            if (!this.IsListening)
                throw new InvalidOperationException();

            this.processStartEvent.Stop();
            this.Cleanup();

            this._islistening = false;
        }

        private void ProcessAdd(Process proc, string fullfilepath)
        {
            this.processList.Add(proc, fullfilepath);
            proc.EnableRaisingEvents = true;
            proc.Exited += this.Proc_Exited;
        }

        private void Proc_Exited(object sender, EventArgs e)
        {
            Process proc = sender as Process;
            if (proc != null)
            {
                string fetch_before_removing = this.processList[proc];
                this.ProcessRemove(proc);
                this.OnProcessExited(new ProcessEventArgs(proc, fetch_before_removing));
            }
        }

        private void ProcessRemove(Process proc)
        {
            proc.Exited -= this.Proc_Exited;
            proc.Dispose();
            this.processList.Remove(proc);
        }

        private void Cleanup()
        {
            foreach (Process proc in this.processList.Keys)
                proc.Dispose();
            this.processList.Clear();
        }

        private void processStartEvent_EventArrived(object sender, EventArrivedEventArgs e)
        {
            try
            {
                int myProcID = Convert.ToInt32(e.NewEvent.Properties["ProcessID"].Value);
                string pathString = Leayal.ProcessHelper.GetProcessImagePath(myProcID);
                if (!string.IsNullOrEmpty(pathString))
                {
                    if (this.isProcessNameOnly)
                    {
                        if (pathString.EndsWith(this.ProcessPath, StringComparison.OrdinalIgnoreCase))
                        {
                            Process proc = Process.GetProcessById(myProcID);
                            this.ProcessAdd(proc, pathString);
                            this.OnProcessLaunched(new ProcessEventArgs(proc, pathString));
                        }
                    }
                    else
                    {
                        if (pathString.IsEqual(this.ProcessPath, true))
                        {
                            Process proc = Process.GetProcessById(myProcID);
                            this.ProcessAdd(proc, pathString);
                            this.OnProcessLaunched(new ProcessEventArgs(proc, pathString));
                        }
                    }
                }
            }
            catch (ArgumentException)
            { }
        }
        /// <summary>
        /// Will be raised whenever a process, which is in matched process list, exits.
        /// </summary>
        public event EventHandler<ProcessEventArgs> ProcessExited;
        private void OnProcessExited(ProcessEventArgs e)
        {
            this.ProcessExited?.Invoke(this, e);
        }
        /// <summary>
        /// Will be raised whenver any process match the search starting up.
        /// </summary>
        public event EventHandler<ProcessEventArgs> ProcessLaunched;
        private void OnProcessLaunched(ProcessEventArgs e)
        {
            this.ProcessLaunched?.Invoke(this, e);
        }

        public void Dispose()
        {
            if ((this.processStartEvent != null))
            {
                this.StopListen();
                this.processStartEvent.Dispose();
            }
            this.processStartEvent = null;
        }
    }

    /// <summary>
    /// Provides a process listener that will raise even whenever a process match the search. May require Administration or elevated access. This class should be disposed if you're finished with it.
    /// </summary>
    public sealed class ProcessWatcher : IDisposable
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
        private void OnProcessExited(EventArgs e)
        {
            this.ProcessExited?.Invoke(this, e);
        }
        public event EventHandler ProcessLaunched;
        private void OnProcessLaunched(EventArgs e)
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
    
    public class ProcessEventArgs : EventArgs
    {
        public Process Process { get; }
        public string ProcessFullFilename { get; }

        internal ProcessEventArgs(Process proc, string fullfilename) : base()
        {
            this.Process = proc;
            this.ProcessFullFilename = fullfilename;
        }
    }
}
