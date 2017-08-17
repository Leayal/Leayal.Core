using System;

namespace Leayal.Ini
{
    public delegate void IniReadErrorEventHandler(object sender, IniReadErrorEventArgs e);
    public sealed class IniReadErrorEventArgs : EventArgs
    {
        public Exception Error { get; }
        public DuplicatedKeyCollection DuplicatedKeys { get; }
        internal IniReadErrorEventArgs(DuplicatedKeyCollection keyCollection, Exception ex) : base()
        {
            this.DuplicatedKeys = keyCollection;
            this.Error = ex;
        }

        internal IniReadErrorEventArgs(DuplicatedKeyCollection keyCollection) : this(keyCollection, null) { }
        internal IniReadErrorEventArgs(Exception ex) : this(null, ex) { }
    }
}
