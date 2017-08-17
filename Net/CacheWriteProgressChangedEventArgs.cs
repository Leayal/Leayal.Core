using System;

namespace Leayal.Net
{
    public class CacheWriteProgressChangedEventArgs : EventArgs
    {
        public long BytesReceived { get; }
        public bool Cancel { get; set; }
        public CacheWriteProgressChangedEventArgs(long bytes) : base()
        {
            this.BytesReceived = bytes;
            this.Cancel = false;
        }
    }
}
