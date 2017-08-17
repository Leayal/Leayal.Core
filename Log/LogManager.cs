using System;
using System.Collections.Generic;
using System.IO;
using System.Collections.Concurrent;

namespace Leayal.Log
{
    public static class LogManager
    {
        private static InnerLogManager Default = new InnerLogManager();
        internal sealed class InnerLogManager : IDisposable
        {
            private ConcurrentDictionary<string, Logger> dict_Log;
            private string _defaultPath;
            public string DefaultPath
            {
                get { return this._defaultPath; }
                set
                {
                    if (this._defaultPath != value)
                    {
                        this._defaultPath = value;
                        Logger val;
                        if (this.dict_Log.TryRemove(string.Empty, out val))
                            val.Dispose();
                        val = new Logger(Path.Combine(value, "GeneralErrors.txt"), true);
                        this.dict_Log.TryAdd(string.Empty, val);
                    }
                }
            }

            public InnerLogManager()
            {
                this.dict_Log = new ConcurrentDictionary<string, Logger>();
                this.DefaultPath = AppInfo.AssemblyInfo.DirectoryPath;
            }

            public Logger this[string fullname]
            {
                get
                {
                    Logger result;
                    if (dict_Log.TryGetValue(fullname, out result))
                        return result;
                    else
                        return null;
                }
            }

            public Logger GetLog(FileInfo path, string seperator, bool appendExisting)
            {
                if (!dict_Log.ContainsKey(path.FullName))
                    dict_Log.TryAdd(path.FullName, new Logger(path, seperator, appendExisting));
                return this[path.FullName];
            }

            public void Clear()
            {
                foreach (Logger obj in Default.dict_Log.Values)
                    obj.Dispose();
                this.dict_Log.Clear();
            }

            public void Dispose()
            {
                this.Clear();
            }
        }

        public static string DefaultPath
        {
            get { return Default.DefaultPath; }
            set { Default.DefaultPath = value; }
        }

        public static Logger GeneralLog
        { get { return Default[string.Empty]; } }

        public static Logger GetLog(string path, string seperator)
        {
            return GetLog(new FileInfo(path), seperator, true);
        }

        public static Logger GetLog(string path, string seperator, bool appendExisting)
        {
            return GetLog(new FileInfo(path), seperator, appendExisting);
        }

        public static Logger GetLogDefaultPath(string path, string seperator)
        {
            return GetLog(new FileInfo(Path.Combine(DefaultPath, path)), seperator, true);
        }

        public static Logger GetLogDefaultPath(string path, string seperator, bool appendExisting)
        {
            return GetLog(new FileInfo(Path.Combine(DefaultPath, path)), seperator, appendExisting);
        }

        public static Logger GetLog(string path)
        {
            return GetLog(new FileInfo(path), "=", true);
        }

        public static Logger GetLog(string path, bool appendExisting)
        {
            return GetLog(new FileInfo(path), "=", appendExisting);
        }

        public static Logger GetLogDefaultPath(string path)
        {
            return GetLog(new FileInfo(Path.Combine(DefaultPath, path)), "=", true);
        }

        public static Logger GetLogDefaultPath(string path, bool appendExisting)
        {
            return GetLog(new FileInfo(Path.Combine(DefaultPath, path)), "=", appendExisting);
        }

        public static Logger GetLog(FileInfo path, string seperator, bool appendExisting)
        {
            return Default.GetLog(path, seperator, appendExisting);
        }
    }
}
