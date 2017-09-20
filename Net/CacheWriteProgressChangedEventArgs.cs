using System;

namespace Leayal.Net
{
    public class CacheWriteProgressChangedEventArgs : EventArgs
    {
        internal void SetBytesReceived(long val)
        {
            this._bytesreceived = val;
        }
        private long _bytesreceived;
        public long BytesReceived => this._bytesreceived;
        public bool Cancel { get; set; }
        public CacheWriteProgressChangedEventArgs(long bytes) : base()
        {
            this._bytesreceived = bytes;
            this.Cancel = false;
        }
    }
}
