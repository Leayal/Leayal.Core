using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Leayal.Net
{
    public class DownloadFileProgressChangedEventArgs : System.EventArgs
    {
        public int CurrentFileIndex { get; }
        public int TotalFileCount { get; }
        public int Percent { get; }
        public DownloadFileProgressChangedEventArgs(int current, int total)
        {
            this.CurrentFileIndex = current;
            this.TotalFileCount = total;
            this.Percent = (int)((CurrentFileIndex * 100) / TotalFileCount);
        }
    }
}
