using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

namespace Leayal.Net
{
    public class CacheStorage : IDisposable
    {
        private ConcurrentDictionary<string, CacheInfo> innerDict;
        public string StorageLocation { get; }
        public CacheStorage(string path)
        {
            this.StorageLocation = Path.GetFullPath(path);
            this.innerDict = new ConcurrentDictionary<string, CacheInfo>();
            //Set it as 250MB ???
            this.MaximumCacheSize = 250 * 1024 * 1024;
        }

        public long MaximumCacheSize { get; set; }

        public CacheInfo GetCacheFromURL(System.Uri _url)
        {
            if (disposed) throw new ObjectDisposedException("CacheStorage");
            var cacheID = Security.Cryptography.SHA256Wrapper.FromString(_url.AbsoluteUri);
            if (string.IsNullOrWhiteSpace(cacheID)) return null;
            cacheID = cacheID.ToLower();
            if (this.innerDict.ContainsKey(cacheID))
            {
                CacheInfo ci;
                if (this.innerDict.TryGetValue(cacheID, out ci))
                    return ci;
                else
                    return null;
            }
            else
            {
                var cache = GetCacheFromID(cacheID);
                if (this.innerDict.TryAdd(cacheID, cache))
                    this.innerDict[cacheID] = cache;
                //this.innerDict.Add(cacheID, cache);
                return cache;
            }
        }

        private CacheInfo GetCacheFromID(string cacheID)
        {
            var asdasd = CacheInfo.FromFile(Path.Combine(this.StorageLocation, cacheID));
            asdasd.CacheCreateCompleted += CacheInfo_CacheCreateCompleted;
            return asdasd;
        }

        private void CacheInfo_CacheCreateCompleted(object sender, EventArgs e)
        {
            CacheInfo ci = sender as CacheInfo;
            if (ci != null)
                this.Shrink();
        }

        public void Shrink()
        {
            string cacheID = null;
            long maxcacheSize = Math.Max(this.MaximumCacheSize, 10 * 1024 * 1024);
            CacheInfo cache;
            long TotalSize = 0L;
            foreach (string filename in Directory.EnumerateFiles(this.StorageLocation, "*", SearchOption.TopDirectoryOnly))
            {
                cacheID = Path.GetFileName(filename.ToLower());
                if (!this.innerDict.ContainsKey(cacheID))
                {
                    cache = GetCacheFromID(cacheID);
                    if (this.innerDict.TryAdd(cacheID, cache))
                        this.innerDict[cacheID] = cache;
                }
                else
                    cache = this.innerDict[cacheID];
                TotalSize += cache.FileSize;
            }
            if (this.innerDict.Count > 5 && TotalSize > maxcacheSize)
            {
                var ienum = this.innerDict.Values.Where(fi => fi.Exists && !fi.IsInUse).OrderBy(fi => fi.LastModifiedDate);
                if (ienum.Count() < 6) return;
                cache = ienum.First();
                if (cache == null) return;
                try
                { cache.Delete(); }
                catch { }
                cache.Dispose();
                CacheInfo aaaa;
                this.innerDict.TryRemove(cache.ID, out aaaa);
                if (this.innerDict.Count > 5 && TotalSize > 0 && TotalSize > maxcacheSize)
                    this.Shrink();
            }
        }

        private bool disposed;
        public void Dispose()
        {
            if (disposed) return;
            this.disposed = true;
            if (this.innerDict.Count > 0)
                foreach (CacheInfo ci in this.innerDict.Values)
                    ci.Dispose();
            this.innerDict.Clear();
        }
    }
}
