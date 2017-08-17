using System;
using System.IO;
using System.Net;

namespace Leayal.Net
{
    public class CacheResponse : WebResponse
    {
        internal static CacheResponse From(CacheInfo info, HttpWebResponse rep)
        {
            return new CacheResponse(info, rep);
        }

        internal static CacheResponse From(CacheStorage storage, HttpWebResponse rep)
        {
            return From(storage.GetCacheFromURL(rep.ResponseUri), rep);
        }

        private CacheInfo innerInfo;
        private WebHeaderCollection _header;
        private CookieCollection m_cookies;
        private HttpWebResponse resp;
        private Stream respStream;

        internal CacheResponse(CacheInfo info, HttpWebResponse head) : base()
        {
            this.innerInfo = info;
            this.resp = head;
            this._header = head.Headers;
            this.m_cookies = head.Cookies;
            if (this._header == null)
                this._header = new WebHeaderCollection();
            this._header.Remove(HttpResponseHeader.ContentType);
            this._header.Remove(HttpResponseHeader.ContentEncoding);
            this._header.Set(HttpResponseHeader.ContentLength, info.FileSize.ToString());
        }

        public override Stream GetResponseStream()
        {
            if (_disposed) throw new ObjectDisposedException("CacheResponse");
            if (respStream == null)
                this.respStream = this.innerInfo.OpenRead();
            return this.respStream;
        }

        public DateTime LastModified => innerInfo.LastModifiedDate;

        public override long ContentLength => this.innerInfo.FileSize;
        public override string ContentType => System.Net.Mime.MediaTypeNames.Application.Octet;

        private bool _disposed;
        public override void Close()
        {
            if (_disposed) return;
            _disposed = true;
            base.Close();
            if (this.respStream != null)
                this.respStream.Dispose();
        }

        public override WebHeaderCollection Headers { get { return this._header; } }

        public override bool IsFromCache => true;
        public override bool IsMutuallyAuthenticated => false;
        public string ContentEncoding=>null;
        public string Method => resp.Method;
        public CookieCollection Cookies => this.m_cookies;
        public string Server => resp.Server;
        public string StatusDescription => resp.StatusDescription;
        public HttpStatusCode StatusCode => resp.StatusCode;
        public override Uri ResponseUri => resp.ResponseUri;
    }
}
