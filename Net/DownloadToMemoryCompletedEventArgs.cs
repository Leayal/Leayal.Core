using Leayal.IO;
using System;
using System.ComponentModel;

namespace Leayal.Net
{
    public class DownloadToMemoryCompletedEventArgs : AsyncCompletedEventArgs
    {
        public RecyclableMemoryStream Result { get; }
        public string Tag { get; }
        public DownloadToMemoryCompletedEventArgs(RecyclableMemoryStream _result, Exception ex, bool cancelled, object userState) : this(_result, string.Empty, ex, cancelled, userState) { }
        public DownloadToMemoryCompletedEventArgs(RecyclableMemoryStream _result, string _tag, Exception ex, bool cancelled, object userState) : base(ex, cancelled, userState)
        {
            this.Result = _result;
            this.Tag = _tag;
        }
    }
}
