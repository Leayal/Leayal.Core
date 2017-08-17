using System;
using System.Collections.Concurrent;
using System.IO;
using System.ComponentModel;

namespace Leayal.Log
{
    public class Logger
    {
        internal class LogLine
        {
            public string Message { get; }
            public LogLevel Level { get; }
            public LogLine(string msg, LogLevel _level)
            {
                this.Message = msg;
                this.Level = _level;
            }
        }
        private ConcurrentStack<LogLine> myQueue;
        private BackgroundWorker myWorker;

        public FileInfo LogPath
        { get; private set; }
        public string SeparatorChar
        { get; set; }

        internal Logger(FileInfo pathInfo, string separator, bool appendExisting)
        {
            this.LogPath = pathInfo;
            this.SeparatorChar = separator;
            if (!appendExisting)
                pathInfo.Delete();
            this.myQueue = new ConcurrentStack<LogLine>();
            this.myWorker = new BackgroundWorker();
            this.myWorker.WorkerReportsProgress = false;
            this.myWorker.WorkerSupportsCancellation = false;
            this.myWorker.DoWork += MyWorker_DoWork;
            this.myWorker.RunWorkerCompleted += MyWorker_RunWorkerCompleted;
        }

        internal Logger(FileInfo pathInfo, bool appendExisting) : this(pathInfo, "=", appendExisting) { }

        internal Logger(FileInfo pathInfo) : this(pathInfo, "=", true) { }

        internal Logger(string path, string separator, bool appendExisting) : this(new FileInfo(path), separator, appendExisting) { }

        internal Logger(string path, bool appendExisting) : this(new FileInfo(path), "=", appendExisting) { }

        internal Logger(string path) : this(new FileInfo(path), "=", true) { }

        private void StartLog()
        {
            if (!this.myWorker.IsBusy && !this.myQueue.IsEmpty)
                this.myWorker.RunWorkerAsync();
        }

        private void MyWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null) { }
            else
                this.StartLog();
        }

        private void MyWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            LogLine _LogLine;
            if (this.myQueue.TryPop(out _LogLine))
                if (!string.IsNullOrWhiteSpace(_LogLine.Message))
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.CreateDirectory(this.LogPath.DirectoryName);
                    using (StreamWriter sw = new StreamWriter(this.LogPath.FullName, true, System.Text.Encoding.UTF8))
                    {
                        if (_LogLine.Level != LogLevel.None)
                            sw.Write(_LogLine.Level.ToString() + ": ");
                        sw.WriteLine(_LogLine.Message);
                        if (!string.IsNullOrWhiteSpace(this.SeparatorChar))
                        {
                            for (int i = 0; i < 15; i++)
                                sw.Write(SeparatorChar);
                            sw.WriteLine(SeparatorChar);
                        }
                    }
                }
        }

        public void Print(string msg, LogLevel _level)
        {
            this.myQueue.Push(new LogLine(msg, _level));
            this.StartLog();
        }

        public void Print(string msg)
        {
            this.Print(msg, LogLevel.Info);
        }

        public void Print(Exception ex)
        {
            this.Print(ex, LogLevel.Error);
        }

        public void Print(Exception ex, LogLevel _level)
        {
            if (ex.InnerException != null)
                this.Print(ex.InnerException.ToString(), _level);
            else
                this.Print(ex.ToString(), _level);
        }

        private bool _disposed;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            this.myWorker.Dispose();

        }
    }
}
