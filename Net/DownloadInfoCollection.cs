using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Leayal.Net
{
    public class DownloadInfoCollection
    {
        private ConcurrentQueue<DownloadInfo> myList;
        public bool IsEmpty
        { get { return this.myList.IsEmpty; } }
        public int Count
        { get; private set; }
        private int _currentItemCount;
        public int CurrentItemCount
        { get { return this._currentItemCount; } }
        public DownloadInfoCollection(IEnumerable<DownloadInfo> list)
        {
            this.myList = new ConcurrentQueue<DownloadInfo>(list);
            this.Count = 0;
            this._currentItemCount = 0;
        }
        public DownloadInfoCollection()
        {
            this.myList = new ConcurrentQueue<DownloadInfo>();
            this.Count = 0;
            this._currentItemCount = 0;
        }

        public void Add(string sUrl, string sFilename)
        {
            this.Add(new DownloadInfo(new System.Uri(sUrl), sFilename));
        }

        public void Add(System.Uri uUrl, string sFilename)
        {
            this.Add(new DownloadInfo(uUrl, sFilename));
        }

        public void Add(DownloadInfo item)
        {
            this.Count++;
            this.myList.Enqueue(item);
        }

        public DownloadInfo TakeFirst()
        {
            if (this.IsEmpty)
                return null;
            else
            {
                DownloadInfo result;
                if (this.myList.TryDequeue(out result))
                {
                    this._currentItemCount++;
                    return result;
                }
                else
                    return null;
            }
        }

        public void Dispose()
        {
            this.myList = null;
        }
    }
}
