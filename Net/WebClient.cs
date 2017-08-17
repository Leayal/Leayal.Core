using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using Microsoft.IO;
using Leayal.IO;
using System.IO.Compression;

namespace Leayal.Net
{
    public class WebClient : System.Net.WebClient
    {
        private BackgroundWorker worker;
        public WebClient(CookieContainer cookies = null, bool autoRedirect = true) : base()
        {
            base.Encoding = System.Text.Encoding.UTF8;
            this.AutoUserAgent = true;
            this.AutoCredentials = true;
            this.CookieContainer = cookies ?? new CookieContainer();
            this.AutoRedirect = autoRedirect;
            this.UserAgent = "Mozilla/4.0";
            this.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            this.Proxy = null;
            this.TimeOut = 5000;
            this.ReadTimeOut = 1800000;
            this._response = null;
            this.CacheStorage = null;
            this.worker = new BackgroundWorker();
            this.worker.WorkerSupportsCancellation = true;
            this.worker.WorkerReportsProgress = true;
            this.worker.DoWork += this.Worker_DoWork;
            this.worker.RunWorkerCompleted += this.Worker_RunWorkerCompleted;
            this.worker.ProgressChanged += this.Worker_ProgressChanged;
        }

        public WebClient(int iTimeOut, CookieContainer cookies = null, bool autoRedirect = true) : this(cookies, autoRedirect)
        {
            this.TimeOut = iTimeOut;
        }

        public bool AutoCredentials { get; set; }
        public bool AutoUserAgent { get; set; }
        public string UserAgent { get; set; }
        public int TimeOut { get; set; }
        public int ReadTimeOut { get; set; }
        public System.Uri CurrentURL { get; private set; }
        private WebResponse _response;

        public new bool IsBusy { get { return (base.IsBusy || this.worker.IsBusy); } }

        /// Initializes a new instance of the BetterWebClient class.  <pa...

        /// Gets or sets a value indicating whether to automatically redi...
        public bool AutoRedirect { get; set; }

        /// Gets or sets the cookie container. This contains all the cook...
        public CookieContainer CookieContainer { get; set; }

        /// Gets the cookies header (Set-Cookie) of the last request.
        public string Cookies
        {
            get { return GetHeaderValue("Set-Cookie"); }
        }

        public Leayal.Net.CacheStorage CacheStorage { get; set; }

        /// Gets the location header for the last request.
        public string Location
        {
            get { return GetHeaderValue("Location"); }
        }

        /// Gets the status code. When no request is present, <see cref="...
        public HttpStatusCode StatusCode
        {
            get
            {
                var result = HttpStatusCode.Gone;
                if (_response != null && this.IsHTTP())
                {
                    try
                    {
                        var rep = _response as HttpWebResponse;
                        result = rep.StatusCode;
                    }
                    catch
                    { result = HttpStatusCode.Gone; }
                }
                return result;
            }
        }

        /// Gets or sets the setup that is called before the request is d...
        public Action<HttpWebRequest> Setup { get; set; }

        /// Gets the header value.
        public string GetHeaderValue(string headerName)
        {
            if (_response == null)
                return null;
            else
            {
                string result = null;
                result = _response.Headers?[headerName];
                return result;
            }
        }

        public string GetHeaderValue(HttpResponseHeader headerenum)
        {
            if (_response == null)
                return null;
            else
            {
                string result = null;
                result = _response.Headers?[headerenum];
                return result;
            }
        }

        #region "Open"
        public WebRequest CreateRequest(System.Uri url, string _method, WebHeaderCollection _headers, IWebProxy _proxy, int _timeout, System.Net.Cache.RequestCachePolicy _cachePolicy)
        {
            if (this.IsHTTP(url))
            {
                HttpWebRequest request = this.GetWebRequest(url) as HttpWebRequest;
                HttpWebRequestHeaders placeholder = new HttpWebRequestHeaders();

                foreach (string key in _headers.AllKeys)
                    switch (key)
                    {
                        case "Accept":
                            placeholder[HttpRequestHeader.Accept] = _headers[HttpRequestHeader.Accept];
                            _headers.Remove(HttpRequestHeader.Accept);
                            break;
                        case "ContentType":
                            placeholder[HttpRequestHeader.ContentType] = _headers[HttpRequestHeader.ContentType];
                            _headers.Remove(HttpRequestHeader.ContentType);
                            break;
                        case "Expect":
                            placeholder[HttpRequestHeader.Expect] = _headers[HttpRequestHeader.Expect];
                            _headers.Remove(HttpRequestHeader.Expect);
                            break;
                        case "Referer":
                            placeholder[HttpRequestHeader.Referer] = _headers[HttpRequestHeader.Referer];
                            _headers.Remove(HttpRequestHeader.Referer);
                            break;
                        case "TransferEncoding":
                            placeholder[HttpRequestHeader.TransferEncoding] = _headers[HttpRequestHeader.TransferEncoding];
                            _headers.Remove(HttpRequestHeader.TransferEncoding);
                            break;
                        case "UserAgent":
                            placeholder[HttpRequestHeader.UserAgent] = _headers[HttpRequestHeader.UserAgent];
                            _headers.Remove(HttpRequestHeader.UserAgent);
                            break;
                        case "ContentLength":
                            placeholder[HttpRequestHeader.ContentLength] = _headers[HttpRequestHeader.ContentLength];
                            _headers.Remove(HttpRequestHeader.ContentLength);
                            break;
                    }
                request.Headers = _headers;
                request.Proxy = _proxy;
                request.CachePolicy = _cachePolicy;
                request.Timeout = _timeout;
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                request.SendChunked = false;
                if (!string.IsNullOrWhiteSpace(_method))
                    request.Method = _method.ToUpper();
                if (!string.IsNullOrEmpty(placeholder[HttpRequestHeader.Accept]))
                    request.Accept = placeholder[HttpRequestHeader.Accept];
                if (!string.IsNullOrEmpty(placeholder[HttpRequestHeader.ContentType]))
                    request.ContentType = placeholder[HttpRequestHeader.ContentType];
                if (!string.IsNullOrEmpty(placeholder[HttpRequestHeader.Expect]))
                    request.Expect = placeholder[HttpRequestHeader.Expect];
                if (!string.IsNullOrEmpty(placeholder[HttpRequestHeader.Referer]))
                    request.Referer = placeholder[HttpRequestHeader.Referer];
                if (!string.IsNullOrEmpty(placeholder[HttpRequestHeader.TransferEncoding]))
                    request.TransferEncoding = placeholder[HttpRequestHeader.TransferEncoding];
                if (!string.IsNullOrEmpty(placeholder[HttpRequestHeader.UserAgent]))
                    request.UserAgent = placeholder[HttpRequestHeader.UserAgent];
                else
                {
                    string ua = this.GetUserAgent(url);
                    if (!string.IsNullOrEmpty(ua))
                        request.UserAgent = ua;
                }
                request.Credentials = this.GetCredentials(url, null);
                if (!string.IsNullOrEmpty(placeholder[HttpRequestHeader.ContentLength]))
                    request.ContentLength = long.Parse(placeholder[HttpRequestHeader.ContentLength]);

                //request.Headers = _headers;
                return request;
            }
            else
            {
                WebRequest request = this.GetWebRequest(url);
                request.Proxy = _proxy;
                request.CachePolicy = _cachePolicy;
                request.Timeout = _timeout;
                request.Headers = _headers;
                return request;
            }
        }

        public WebRequest CreateRequest(System.Uri url, WebHeaderCollection _headers, IWebProxy _proxy, int _timeout, System.Net.Cache.RequestCachePolicy _cachePolicy)
        {
            return CreateRequest(url, string.Empty, _headers, _proxy, _timeout, _cachePolicy);
        }

        public WebRequest CreateRequest(System.Uri url, WebHeaderCollection _headers, IWebProxy _proxy, int _timeout)
        {
            return this.CreateRequest(url, _headers, _proxy, _timeout, this.CachePolicy);
        }

        public WebRequest CreateRequest(System.Uri url, WebHeaderCollection _headers, IWebProxy _proxy)
        {
            return this.CreateRequest(url, _headers, _proxy, this.TimeOut, this.CachePolicy);
        }

        public WebRequest CreateRequest(System.Uri url, WebHeaderCollection _headers, int _timeout)
        {
            return this.CreateRequest(url, _headers, null, _timeout, this.CachePolicy);
        }

        public WebRequest CreateRequest(System.Uri url, IWebProxy _proxy, int _timeout)
        {
            return this.CreateRequest(url, this.Headers, _proxy, _timeout, this.CachePolicy);
        }

        public WebRequest CreateRequest(System.Uri url, IWebProxy _proxy)
        {
            return this.CreateRequest(url, this.Headers, _proxy, this.TimeOut, this.CachePolicy);
        }

        public WebRequest CreateRequest(System.Uri url, WebHeaderCollection _headers)
        {
            return this.CreateRequest(url, _headers, null, this.TimeOut, this.CachePolicy);
        }

        public WebRequest CreateRequest(System.Uri url, int _timeout)
        {
            return this.CreateRequest(url, this.Headers, null, _timeout, this.CachePolicy);
        }

        public WebRequest CreateRequest(System.Uri url, string _method, int _timeout)
        {
            return this.CreateRequest(url, _method, this.Headers, null, _timeout, this.CachePolicy);
        }

        public WebRequest CreateRequest(System.Uri url, string _method)
        {
            return this.CreateRequest(url, _method, this.TimeOut);
        }

        public WebRequest CreateRequest(System.Uri url)
        {
            return this.CreateRequest(url, this.TimeOut);
        }

        public WebRequest CreateRequest(string url, string _method, int _timeout)
        {
            return this.CreateRequest(new System.Uri(url), _method, this.Headers, null, _timeout, this.CachePolicy);
        }

        public WebRequest CreateRequest(string url, string _method)
        {
            return this.CreateRequest(new System.Uri(url), _method, this.TimeOut);
        }

        public WebRequest CreateRequest(string url)
        {
            return this.CreateRequest(new System.Uri(url));
        }

        public WebRequest CreateRequest(string url, int _timeout)
        {
            return this.CreateRequest(new System.Uri(url), _timeout);
        }

        public WebRequest CreateRequest(string url, IWebProxy _proxy)
        {
            return this.CreateRequest(new System.Uri(url), _proxy);
        }

        public WebRequest CreateRequest(string url, IWebProxy _proxy, int _timeout)
        {
            return this.CreateRequest(new System.Uri(url), _proxy, _timeout);
        }

        public WebRequest CreateRequest(string url, WebHeaderCollection _headers)
        {
            return this.CreateRequest(new System.Uri(url), _headers);
        }

        public WebRequest CreateRequest(string url, WebHeaderCollection _headers, int _timeout)
        {
            return this.CreateRequest(new System.Uri(url), _headers, null, _timeout, this.CachePolicy);
        }

        internal WebResponse Open(WebRequest request)
        {
            this.CurrentURL = request.RequestUri;
            return this.GetWebResponse(request);
        }

        public WebResponse Open(System.Uri url, WebHeaderCollection _headers, IWebProxy _proxy, int _timeout, System.Net.Cache.RequestCachePolicy _cachePolicy)
        {
            return this.Open(this.CreateRequest(url, _headers, _proxy, _timeout, _cachePolicy));
        }

        public WebResponse Open(System.Uri url, WebHeaderCollection _headers, IWebProxy _proxy, int _timeout)
        {
            return this.Open(url, _headers, _proxy, _timeout, this.CachePolicy);
        }

        public WebResponse Open(System.Uri url, WebHeaderCollection _headers, IWebProxy _proxy)
        {
            return this.Open(url, _headers, _proxy, this.TimeOut, this.CachePolicy);
        }

        public WebResponse Open(System.Uri url, WebHeaderCollection _headers, int _timeout)
        {
            return this.Open(url, _headers, null, _timeout, this.CachePolicy);
        }

        public WebResponse Open(System.Uri url, IWebProxy _proxy, int _timeout)
        {
            return this.Open(url, this.Headers, _proxy, _timeout, this.CachePolicy);
        }

        public WebResponse Open(System.Uri url, IWebProxy _proxy)
        {
            return this.Open(url, this.Headers, _proxy, this.TimeOut, this.CachePolicy);
        }

        public WebResponse Open(System.Uri url, WebHeaderCollection _headers)
        {
            return this.Open(url, _headers, null, this.TimeOut, this.CachePolicy);
        }

        public WebResponse Open(System.Uri url, int _timeout)
        {
            return this.Open(url, this.Headers, null, _timeout, this.CachePolicy);
        }

        public WebResponse Open(System.Uri url)
        {
            return this.Open(url, this.Headers, null, this.TimeOut, this.CachePolicy);
        }

        public WebResponse Open(string url)
        {
            return this.Open(new System.Uri(url));
        }

        public WebResponse Open(string url, int _timeout)
        {
            return this.Open(new System.Uri(url), _timeout);
        }

        public WebResponse Open(string url, IWebProxy _proxy)
        {
            return this.Open(new System.Uri(url), _proxy);
        }

        public WebResponse Open(string url, IWebProxy _proxy, int _timeout)
        {
            return this.Open(new System.Uri(url), _proxy, _timeout);
        }

        public WebResponse Open(string url, WebHeaderCollection _headers)
        {
            return this.Open(new System.Uri(url), _headers);
        }

        public WebResponse Open(string url, WebHeaderCollection _headers, int _timeout)
        {
            return this.Open(new System.Uri(url), _headers, null, _timeout, this.CachePolicy);
        }
        #endregion

        protected override WebRequest GetWebRequest(System.Uri address)
        {
            this.CurrentURL = address;
            var request = base.GetWebRequest(address);
            if (this.IsHTTP())
            {
                HttpWebRequest httpRequest = request as HttpWebRequest;
                if (request != null)
                {
                    httpRequest.AllowAutoRedirect = AutoRedirect;
                    httpRequest.CookieContainer = CookieContainer;
                    httpRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    httpRequest.Timeout = this.TimeOut;
                    if ((this.Headers != null) && (this.Headers.HasKeys()))
                        httpRequest.Headers = this.Headers;
                    string ua = this.GetUserAgent(address);
                    httpRequest.Credentials = this.GetCredentials(address);
                    if (!string.IsNullOrEmpty(ua))
                        httpRequest.UserAgent = ua;
                    Setup?.Invoke(httpRequest);
                }
                else
                {
                    request.Timeout = this.TimeOut;
                    if ((this.Headers != null) && (this.Headers.HasKeys()))
                        request.Headers = this.Headers;
                }
            }
            else
            {
                request.Timeout = this.TimeOut;
                if ((this.Headers != null) && (this.Headers.HasKeys()))
                    request.Headers = this.Headers;
            }
            return request;
        }

        protected virtual ICredentials GetCredentials(System.Uri address)
        {
            return this.GetCredentials(address, this.Credentials);
        }

        protected virtual ICredentials GetCredentials(System.Uri address, ICredentials defaultvalue)
        {
            return defaultvalue;
        }

        protected virtual string GetUserAgent(System.Uri address)
        {
            if (!string.IsNullOrEmpty(UserAgent))
                return this.UserAgent;
            else
                return null;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            this._response = base.GetWebResponse(request);
            string lastmod = this._response.Headers[HttpResponseHeader.LastModified];
            if ((this._response.Headers != null) && (this._response.Headers.HasKeys()))
            {
                this.ResponseHeaders.Clear();
                foreach (string s in this._response.Headers.AllKeys)
                    this.ResponseHeaders[s] = this._response.Headers.Get(s);
                if (!string.IsNullOrEmpty(lastmod))
                    this.ResponseHeaders.Add(HttpResponseHeader.LastModified, lastmod);
            }

            HttpWebResponse myRep = this._response as HttpWebResponse;
            if (this.CacheStorage != null && myRep != null)
                if (!request.RequestUri.IsFile && !request.RequestUri.IsLoopback && request.RequestUri.IsAbsoluteUri)
                {
                    if (!string.IsNullOrWhiteSpace(lastmod))
                    {
                        DateTime remoteLastModified = DateTime.MinValue;
                        if (lastmod.EndsWith("UTC"))
                            remoteLastModified = DateTime.ParseExact(lastmod, "ddd, dd MMM yyyy HH:mm:ss 'UTC'", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AdjustToUniversal);
                        else if (lastmod.EndsWith("GMT"))
                            remoteLastModified = DateTime.ParseExact(lastmod, "ddd, dd MMM yyyy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AdjustToUniversal);
                        else
                            remoteLastModified = DateTime.ParseExact(lastmod, "ddd, dd MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AdjustToUniversal);
                        CacheInfo _cacheinfo = this.CacheStorage.GetCacheFromURL(request.RequestUri);
                        if (remoteLastModified != DateTime.MinValue)
                        {
                            if (_cacheinfo.Exists && remoteLastModified == _cacheinfo.LastModifiedDate)
                            {
                                //System.Windows.Forms.MessageBox.Show(lastmod + "\n\n" + remoteLastModified.ToString() + "\n\n" + _cacheinfo.LastModifiedDate.ToString());
                                request.Abort();
                                this._response = CacheResponse.From(_cacheinfo, myRep);
                                myRep.Close();
                            }
                            else
                            {
                                using (Stream s = myRep.GetResponseStream())
                                {
                                    GZipStream wrapperstream = s as GZipStream;
                                    if (wrapperstream != null)
                                    {
                                        if (wrapperstream.BaseStream.CanTimeout)
                                            wrapperstream.BaseStream.ReadTimeout = this.ReadTimeOut;
                                    }
                                    else
                                    {
                                        if (s.CanTimeout)
                                            s.ReadTimeout = this.ReadTimeOut;
                                    }
                                    if (this._response.ContentLength > 0)
                                        _cacheinfo.CreateFromStream(s, remoteLastModified, (sender, e) =>
                                        {
                                            e.Cancel = this.worker.CancellationPending;
                                            this.OnDownloadProgressChanged(this.GetDownloadProgressChangedEventArgs(null, e.BytesReceived, this._response.ContentLength));
                                        });
                                    else
                                        _cacheinfo.CreateFromStream(s, remoteLastModified);
                                }
                                if (this.worker.CancellationPending)
                                {
                                    this._response.Close();
                                    throw new WebException("User cancelled the request.", WebExceptionStatus.RequestCanceled);
                                }
                                this._response = CacheResponse.From(_cacheinfo, myRep);
                                myRep.Close();
                            }
                        }
                    }
                }

            return this._response;
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            Console.WriteLine("Noooooo~! You can't be here. It must be wrong so that you can reach here.");

            //DownloadDataCompletedEventArgs(byte[] result, System.Exception exception, bool cancelled, object userToken);
            this._response = base.GetWebResponse(request, result);
            string lastmod = this._response.Headers[HttpResponseHeader.LastModified];
            if ((this._response.Headers != null) && (this._response.Headers.HasKeys()))
            {
                this.ResponseHeaders.Clear();
                foreach (string s in this._response.Headers.AllKeys)
                    this.ResponseHeaders[s] = this._response.Headers.Get(s);
                if (!string.IsNullOrEmpty(lastmod))
                    this.ResponseHeaders.Add(HttpResponseHeader.LastModified, lastmod);
            }

            if (this.CacheStorage != null && this.IsHTTP())
                if (!request.RequestUri.IsFile && !request.RequestUri.IsLoopback && request.RequestUri.IsAbsoluteUri)
                {
                    if (this._response != null && !string.IsNullOrWhiteSpace(lastmod))
                    {
                        DateTime remoteLastModified = DateTime.MinValue;
                        if (lastmod.EndsWith("UTC"))
                            remoteLastModified = DateTime.ParseExact(lastmod, "ddd, dd MMM yyyy HH:mm:ss 'UTC'", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AdjustToUniversal);
                        else if (lastmod.EndsWith("GMT"))
                            remoteLastModified = DateTime.ParseExact(lastmod, "ddd, dd MMM yyyy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AdjustToUniversal);
                        else
                            remoteLastModified = DateTime.ParseExact(lastmod, "ddd, dd MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AdjustToUniversal);
                        CacheInfo _cacheinfo = this.CacheStorage.GetCacheFromURL(request.RequestUri);
                        if (remoteLastModified != DateTime.MinValue)
                        {
                            if (remoteLastModified == _cacheinfo.LastModifiedDate)
                            {
                                //System.Windows.Forms.MessageBox.Show(lastmod + "\n\n" + remoteLastModified.ToString() + "\n\n" + _cacheinfo.LastModifiedDate.ToString());
                                request.Abort();
                                this._response.Close();
                                var filerequest = FileWebRequest.Create(_cacheinfo.LocalURI);
                                filerequest.Proxy = null;
                                filerequest.AuthenticationLevel = System.Net.Security.AuthenticationLevel.None;
                                filerequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                                this._response = filerequest.GetResponse();
                            }
                            else
                            {
                                using (Stream s = this._response.GetResponseStream())
                                {
                                    if (this._response.ContentLength > 0)
                                        _cacheinfo.CreateFromStream(s, remoteLastModified, (sender, e) => {
                                            e.Cancel = this.worker.CancellationPending;
                                            this.OnDownloadProgressChanged(this.GetDownloadProgressChangedEventArgs(null, e.BytesReceived, this._response.ContentLength));
                                        });
                                    else
                                        _cacheinfo.CreateFromStream(s, remoteLastModified);
                                }
                                if (this.worker.CancellationPending)
                                {
                                    this._response.Close();
                                    throw new WebException("User cancelled the request.", WebExceptionStatus.RequestCanceled);
                                }
                                var filerequest = FileWebRequest.Create(_cacheinfo.LocalURI);
                                filerequest.Proxy = null;
                                filerequest.AuthenticationLevel = System.Net.Security.AuthenticationLevel.None;
                                filerequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                                this._response = filerequest.GetResponse();
                            }
                        }
                    }
                }
            return this._response;
        }

        private bool cancelling;
        public new void CancelAsync()
        {
            base.CancelAsync();
            this.worker.CancelAsync();
            this.cancelling = true;
        }

        public new void DownloadFile(string address, string filename)
        {
            this.DownloadFile(new System.Uri(address), filename);
        }

        public new void DownloadFile(System.Uri address, string filename)
        {
            this.cancelling = false;
            WebRequest req = this.GetWebRequest(address);
            bool fromCache = (this.CacheStorage != null);
            WebResponse myRespfile = this.GetWebResponse(req);
            using (Stream networkStream = myRespfile.GetResponseStream())
            {
                if (!(networkStream is CacheStream))
                {
                    GZipStream gzstream = networkStream as GZipStream;
                    if (gzstream != null)
                    {
                        if (gzstream.BaseStream.CanTimeout)
                            gzstream.BaseStream.ReadTimeout = this.ReadTimeOut;
                    }
                    else
                    {
                        if (networkStream.CanTimeout)
                            networkStream.ReadTimeout = this.ReadTimeOut;
                    }
                }
                using (FileStream localfile = File.Create(filename))
                {
                    byte[] arr = new byte[1024];
                    int readbyte = networkStream.Read(arr, 0, arr.Length);
                    while (readbyte > 0)
                    {
                        if (this.cancelling)
                            break;
                        localfile.Write(arr, 0, readbyte);
                        readbyte = networkStream.Read(arr, 0, arr.Length);
                    }
                    localfile.Flush();
                }
            }
        }

        public void DownloadToMemoryAsync(System.Uri address, string filename)
        {
            this.DownloadToMemoryAsync(address, filename, null);
        }

        public void DownloadToMemoryAsync(System.Uri address, string filename, object UserToken)
        {
            if (this.IsBusy)
                throw new InvalidOperationException("The web client is working");
            this.CurrentTask = Task.DownloadToMemory;
            this.innerusertoken = UserToken;
            this.worker.RunWorkerAsync(new filerequestmeta(address, filename));
        }

        public IO.RecyclableMemoryStream DownloadToMemory(System.Uri address, string tag)
        {
            this.cancelling = false;
            IO.RecyclableMemoryStream result = null;
            WebRequest req = this.GetWebRequest(address);
            bool fromCache = (this.CacheStorage != null);
            WebResponse myRespfile = this.GetWebResponse(req);
            using (Stream networkStream = myRespfile.GetResponseStream())
            {
                if (!(networkStream is CacheStream))
                {
                    GZipStream gzstream = networkStream as GZipStream;
                    if (gzstream != null)
                    {
                        if (gzstream.BaseStream.CanTimeout)
                            gzstream.BaseStream.ReadTimeout = this.ReadTimeOut;
                    }
                    else
                    {
                        if (networkStream.CanTimeout)
                            networkStream.ReadTimeout = this.ReadTimeOut;
                    }
                }
                IO.RecyclableMemoryStream localfile = new IO.RecyclableMemoryStream(tag);
                {
                    byte[] arr = new byte[1024];
                    int readbyte = networkStream.Read(arr, 0, arr.Length);
                    while (readbyte > 0)
                    {
                        if (this.cancelling)
                            break;
                        localfile.Write(arr, 0, readbyte);
                        readbyte = networkStream.Read(arr, 0, arr.Length);
                    }
                    localfile.Flush();
                    localfile.Position = 0;
                }
                result = localfile;
            }
            return result;
        }

        public new void DownloadFileAsync(System.Uri address, string filename)
        {
            this.DownloadFileAsync(address, filename, null);
        }

        public new void DownloadFileAsync(System.Uri address, string filename, object UserToken)
        {
            if (this.IsBusy)
                throw new InvalidOperationException("");
            this.CurrentTask = Task.DownloadFile;
            this.innerusertoken = UserToken;
            this.worker.RunWorkerAsync(new filerequestmeta(address, filename));
        }

        public new string DownloadString(string address)
        {
            return this.DownloadString(new System.Uri(address));
        }

        public new string DownloadString(System.Uri address)
        {
            this.cancelling = false;
            WebRequest req = this.GetWebRequest(address);
            bool fromCache = (this.CacheStorage != null);
            WebResponse myRespstr = this.GetWebResponse(req);
            string stringresult = null;
            using (Stream networkStream = myRespstr.GetResponseStream())
            {
                if (!(networkStream is CacheStream))
                {
                    GZipStream gzstream = networkStream as GZipStream;
                    if (gzstream != null)
                    {
                        if (gzstream.BaseStream.CanTimeout)
                            gzstream.BaseStream.ReadTimeout = this.ReadTimeOut;
                    }
                    else
                    {
                        if (networkStream.CanTimeout)
                            networkStream.ReadTimeout = this.ReadTimeOut;
                    }
                }
                using (Microsoft.IO.RecyclableMemoryStream localfile = new Microsoft.IO.RecyclableMemoryStream(AppInfo.MemoryStreamManager))
                {
                    long totalread = 0;
                    byte[] arr = new byte[1024];
                    int readbyte = networkStream.Read(arr, 0, arr.Length);
                    while (readbyte > 0)
                    {
                        if (this.cancelling)
                            break;
                        localfile.Write(arr, 0, readbyte);
                        totalread += readbyte;
                        if (myRespstr.ContentLength > 0)
                            this.worker.ReportProgress(1, new DownloadProgressChangedStruct(null, totalread, myRespstr.ContentLength));
                        readbyte = networkStream.Read(arr, 0, arr.Length);
                    }
                    localfile.Flush();
                    localfile.Position = 0;
                    using (StreamReader sr = new StreamReader(localfile, this.Encoding))
                        stringresult = sr.ReadToEnd();
                }
            }
            return stringresult;
        }

        public new void DownloadStringAsync(System.Uri address)
        {
            this.DownloadStringAsync(address, null);
        }

        public new void DownloadStringAsync(System.Uri address, object UserToken)
        {
            if (this.IsBusy)
                throw new InvalidOperationException("");
            this.CurrentTask = Task.DownloadString;
            this.innerusertoken = UserToken;
            this.worker.RunWorkerAsync(new requestmeta(address));
        }

        public new byte[] DownloadData(string address)
        {
            return this.DownloadData(new System.Uri(address));
        }

        public new byte[] DownloadData(System.Uri address)
        {
            this.cancelling = false;
            WebRequest req = this.GetWebRequest(address);
            bool fromCache = (this.CacheStorage != null);
            WebResponse myRespdata = this.GetWebResponse(req);
            byte[] dataresult = null;
            using (Stream networkStream = myRespdata.GetResponseStream())
            {
                if (!(networkStream is CacheStream))
                {
                    GZipStream gzstream = networkStream as GZipStream;
                    if (gzstream != null)
                    {
                        if (gzstream.BaseStream.CanTimeout)
                            gzstream.BaseStream.ReadTimeout = this.ReadTimeOut;
                    }
                    else
                    {
                        if (networkStream.CanTimeout)
                            networkStream.ReadTimeout = this.ReadTimeOut;
                    }
                }
                using (Microsoft.IO.RecyclableMemoryStream localfile = new Microsoft.IO.RecyclableMemoryStream(AppInfo.MemoryStreamManager))
                {
                    long totalread = 0;
                    byte[] arr = new byte[1024];
                    int readbyte = networkStream.Read(arr, 0, arr.Length);
                    while (readbyte > 0)
                    {
                        if (this.cancelling)
                            break;
                        localfile.Write(arr, 0, readbyte);
                        totalread += readbyte;
                        readbyte = networkStream.Read(arr, 0, arr.Length);
                    }
                    localfile.Flush();
                    /*byte[] bytes = localfile.GetBuffer();
                    dataresult = new byte[localfile.Length];
                    for (int i = 0; i < localfile.Length; i++)
                        dataresult[i] = bytes[i];//*/
                    dataresult = localfile.ToArray();
                }
            }
            return dataresult;
        }

        public new void DownloadDataAsync(System.Uri address)
        {
            this.DownloadDataAsync(address, null);
        }

        public new void DownloadDataAsync(System.Uri address, object UserToken)
        {
            if (this.IsBusy)
                throw new InvalidOperationException("");
            this.CurrentTask = Task.DownloadData;
            this.innerusertoken = UserToken;
            this.worker.RunWorkerAsync(new requestmeta(address));
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            bool fromCache = (this.CacheStorage != null);
            switch (this.CurrentTask)
            {
                case Task.DownloadFile:
                    var _filerequestmeta = e.Argument as filerequestmeta;
                    WebRequest myReqfile = this.GetWebRequest(_filerequestmeta.URL);
                    WebResponse myRespfile = this.GetWebResponse(myReqfile);
                    using (Stream networkStream = myRespfile.GetResponseStream())
                    {
                        if (!(networkStream is CacheStream))
                        {
                            GZipStream gzstream = networkStream as GZipStream;
                            if (gzstream != null)
                            {
                                if (gzstream.BaseStream.CanTimeout)
                                    gzstream.BaseStream.ReadTimeout = this.ReadTimeOut;
                            }
                            else
                            {
                                if (networkStream.CanTimeout)
                                    networkStream.ReadTimeout = this.ReadTimeOut;
                            }
                        }
                        using (FileStream localfile = File.Create(_filerequestmeta.Filename))
                        {
                            long totalread = 0;
                            byte[] arr = new byte[1024];
                            int readbyte = networkStream.Read(arr, 0, arr.Length);
                            while (readbyte > 0)
                            {
                                if (this.worker.CancellationPending)
                                {
                                    e.Cancel = true;
                                    break;
                                }
                                localfile.Write(arr, 0, readbyte);
                                totalread += readbyte;
                                if (myRespfile.ContentLength > 0)
                                    this.worker.ReportProgress(1, new DownloadProgressChangedStruct(null, totalread, myRespfile.ContentLength));
                                readbyte = networkStream.Read(arr, 0, arr.Length);
                            }
                            localfile.Flush();
                        }
                    }
                    break;
                case Task.DownloadData:
                    var _requestmetadata = e.Argument as requestmeta;
                    WebRequest myReqdata = this.GetWebRequest(_requestmetadata.URL);
                    WebResponse myRespdata = this.GetWebResponse(myReqdata);
                    byte[] dataresult = null;
                    using (Stream networkStream = myRespdata.GetResponseStream())
                    {
                        if (!(networkStream is CacheStream))
                        {
                            GZipStream gzstream = networkStream as GZipStream;
                            if (gzstream != null)
                            {
                                if (gzstream.BaseStream.CanTimeout)
                                    gzstream.BaseStream.ReadTimeout = this.ReadTimeOut;
                            }
                            else
                            {
                                if (networkStream.CanTimeout)
                                    networkStream.ReadTimeout = this.ReadTimeOut;
                            }
                        }
                        using (Microsoft.IO.RecyclableMemoryStream localfile = new Microsoft.IO.RecyclableMemoryStream(AppInfo.MemoryStreamManager))
                        {
                            long totalread = 0;
                            byte[] arr = new byte[1024];
                            int readbyte = networkStream.Read(arr, 0, arr.Length);
                            while (readbyte > 0)
                            {
                                if (this.worker.CancellationPending)
                                {
                                    e.Cancel = true;
                                    break;
                                }
                                localfile.Write(arr, 0, readbyte);
                                totalread += readbyte;
                                if (myRespdata.ContentLength > 0)
                                    this.worker.ReportProgress(1, new DownloadProgressChangedStruct(null, totalread, myRespdata.ContentLength));
                                readbyte = networkStream.Read(arr, 0, arr.Length);
                            }
                            localfile.Flush();
                            dataresult = localfile.ToArray();
                        }
                    }
                    e.Result = dataresult;
                    break;
                case Task.DownloadString:
                    var _requestmetastring = e.Argument as requestmeta;
                    WebRequest myReqstr = this.GetWebRequest(_requestmetastring.URL);
                    WebResponse myRespstr = this.GetWebResponse(myReqstr);
                    string stringresult = null;
                    using (Stream networkStream = myRespstr.GetResponseStream())
                    {
                        if (!(networkStream is CacheStream))
                        {
                            GZipStream gzstream = networkStream as GZipStream;
                            if (gzstream != null)
                            {
                                if (gzstream.BaseStream.CanTimeout)
                                    gzstream.BaseStream.ReadTimeout = this.ReadTimeOut;
                            }
                            else
                            {
                                if (networkStream.CanTimeout)
                                    networkStream.ReadTimeout = this.ReadTimeOut;
                            }
                        }

                        using (Microsoft.IO.RecyclableMemoryStream localfile = new Microsoft.IO.RecyclableMemoryStream(AppInfo.MemoryStreamManager))
                        {
                            long totalread = 0;
                            byte[] arr = new byte[1024];
                            int readbyte = networkStream.Read(arr, 0, arr.Length);
                            while (readbyte > 0)
                            {
                                if (this.worker.CancellationPending)
                                {
                                    e.Cancel = true;
                                    break;
                                }
                                localfile.Write(arr, 0, readbyte);
                                totalread += readbyte;
                                if (myRespstr.ContentLength > 0)
                                    this.worker.ReportProgress(1, new DownloadProgressChangedStruct(null, totalread, myRespstr.ContentLength));
                                readbyte = networkStream.Read(arr, 0, arr.Length);
                            }
                            localfile.Flush();
                            localfile.Position = 0;
                            using (StreamReader sr = new StreamReader(localfile, this.Encoding))
                                stringresult = sr.ReadToEnd();
                        }
                    }
                    e.Result = stringresult;
                    break;
                case Task.DownloadToMemory:
                    var _requestmetamemory = e.Argument as filerequestmeta;
                    WebRequest myReqmem = this.GetWebRequest(_requestmetamemory.URL);
                    WebResponse myRespmem = this.GetWebResponse(myReqmem);
                    IO.RecyclableMemoryStream memresult = null;
                    using (Stream networkStream = myRespmem.GetResponseStream())
                    {
                        if (!(networkStream is CacheStream))
                        {
                            GZipStream gzstream = networkStream as GZipStream;
                            if (gzstream != null)
                            {
                                if (gzstream.BaseStream.CanTimeout)
                                    gzstream.BaseStream.ReadTimeout = this.ReadTimeOut;
                            }
                            else
                            {
                                if (networkStream.CanTimeout)
                                    networkStream.ReadTimeout = this.ReadTimeOut;
                            }
                        }

                        IO.RecyclableMemoryStream localfile = new IO.RecyclableMemoryStream(_requestmetamemory.Filename);
                        long totalread = 0;
                        byte[] arr = new byte[1024];
                        int readbyte = networkStream.Read(arr, 0, arr.Length);
                        while (readbyte > 0)
                        {
                            if (this.worker.CancellationPending)
                            {
                                e.Cancel = true;
                                break;
                            }
                            localfile.Write(arr, 0, readbyte);
                            totalread += readbyte;
                            if (myRespmem.ContentLength > 0)
                                this.worker.ReportProgress(1, new DownloadProgressChangedStruct(null, totalread, myRespmem.ContentLength));
                            readbyte = networkStream.Read(arr, 0, arr.Length);
                        }
                        localfile.Flush();
                        localfile.Position = 0;
                        memresult = localfile;
                    }
                    e.Result = memresult;
                    break;
            }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case 1:
                    DownloadProgressChangedStruct progressmeta = e.UserState as DownloadProgressChangedStruct;
                    if (progressmeta != null)
                        this.OnDownloadProgressChanged(this.GetDownloadProgressChangedEventArgs(progressmeta.UserToken, progressmeta.BytesReceived, progressmeta.TotalBytesToReceive));
                    break;
            }
        }

        private object innerusertoken;

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //Do not put the Task=None below this switch, otherwise:
            //The Completed method set the task, then the task again being set to None, which cause DoWork do nothing
            var suchConditionRace = this.CurrentTask;
            this.CurrentTask = Task.None;
            switch (suchConditionRace)
            {
                case Task.DownloadFile:
                    this.OnDownloadFileCompleted(new AsyncCompletedEventArgs(e.Error, e.Cancelled, innerusertoken));
                    break;
                case Task.DownloadString:
                    if (e.Error != null || e.Cancelled)
                        this.OnDownloadStringCompleted(GetDownloadStringCompletedEventArgs(null, e.Error, e.Cancelled, innerusertoken));
                    else
                        this.OnDownloadStringCompleted(GetDownloadStringCompletedEventArgs(e.Result as string, e.Error, e.Cancelled, innerusertoken));
                    break;
                case Task.DownloadData:
                    if (e.Error != null || e.Cancelled)
                        this.OnDownloadDataCompleted(GetDownloadDataCompletedEventArgs(null, e.Error, e.Cancelled, innerusertoken));
                    else
                        this.OnDownloadDataCompleted(GetDownloadDataCompletedEventArgs(e.Result as byte[], e.Error, e.Cancelled, innerusertoken));
                    break;
                case Task.DownloadToMemory:
                    if (e.Error != null || e.Cancelled)
                        this.OnDownloadToMemoryCompleted(new DownloadToMemoryCompletedEventArgs(null, e.Error, e.Cancelled, innerusertoken));
                    else
                        this.OnDownloadToMemoryCompleted(new DownloadToMemoryCompletedEventArgs(e.Result as IO.RecyclableMemoryStream, e.Error, e.Cancelled, innerusertoken));
                    break;
            }
        }

        private Task CurrentTask = Task.None;
        private enum Task : byte
        {
            None,
            DownloadFile,
            DownloadData,
            DownloadString,
            DownloadToMemory
        }

        private class filerequestmeta : requestmeta
        {
            public string Filename { get; }
            public filerequestmeta(Uri _uri, string _filename) : base(_uri)
            {
                this.Filename = _filename;
            }
        }

        private class dataresultmeta : resultmeta
        {
            public byte[] Result { get; }
            public dataresultmeta(byte[] _data, object _usertoken) : base(_usertoken)
            {
                this.Result = _data;
            }
        }

        private class stringresultmeta : resultmeta
        {
            public string Result { get; }
            public stringresultmeta(string _str, object _usertoken) : base(_usertoken)
            {
                this.Result = _str;
            }
        }

        private class resultmeta
        {
            public object UserToken { get; }
            public resultmeta(object _usertoken)
            {
                this.UserToken = _usertoken;
            }
        }

        private class requestmeta
        {
            public Uri URL { get; }
            public requestmeta(Uri _uri)
            {
                this.URL = _uri;
            }
        }

        private bool IsHTTP()
        {
            return this.IsHTTP(this.CurrentURL);
        }

        private bool IsHTTP(System.Uri url)
        {
            if (url == null)
                return false;
            else
            {
                if ((url.Scheme == System.Uri.UriSchemeHttp) || (url.Scheme == System.Uri.UriSchemeHttps))
                    return true;
                else
                    return false;
            }
        }

        public event EventHandler<DownloadToMemoryCompletedEventArgs> DownloadToMemoryCompleted;
        protected virtual void OnDownloadToMemoryCompleted(DownloadToMemoryCompletedEventArgs e)
        {
            //if (this.DownloadToMemoryCompleted!= null)
            this.DownloadToMemoryCompleted?.Invoke(this, e);
        }

        protected DownloadDataCompletedEventArgs GetDownloadDataCompletedEventArgs(byte[] bytes, System.Exception ex, bool cancelled, object usertoken)
        {
            return (DownloadDataCompletedEventArgs)Activator.CreateInstance(typeof(DownloadDataCompletedEventArgs),
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null, new object[] { bytes, ex, cancelled, usertoken }, null);
        }

        protected DownloadStringCompletedEventArgs GetDownloadStringCompletedEventArgs(string str, System.Exception ex, bool cancelled, object usertoken)
        {
            return (DownloadStringCompletedEventArgs)Activator.CreateInstance(typeof(DownloadStringCompletedEventArgs),
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null, new object[] { str, ex, cancelled, usertoken }, null);
        }

        private class DownloadProgressChangedStruct
        {
            public long BytesReceived { get; }
            public long TotalBytesToReceive { get; }
            public object UserToken { get; }
            public DownloadProgressChangedStruct(object _userToken, long _bytesReceived, long _totalBytesToReceive)
            {
                this.UserToken = _userToken;
                this.TotalBytesToReceive = _totalBytesToReceive;
                this.BytesReceived = _bytesReceived;
            }
        }

        protected DownloadProgressChangedEventArgs GetDownloadProgressChangedEventArgs(object userToken, long bytesReceived, long totalBytesToReceive)
        {
            return this.GetDownloadProgressChangedEventArgs((int)((100F * bytesReceived) / totalBytesToReceive), userToken, bytesReceived, totalBytesToReceive);
        }

        protected DownloadProgressChangedEventArgs GetDownloadProgressChangedEventArgs(int progressPercentage, object userToken, long bytesReceived, long totalBytesToReceive)
        {
            return (DownloadProgressChangedEventArgs)Activator.CreateInstance(typeof(DownloadProgressChangedEventArgs),
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null, new object[] { progressPercentage, userToken, bytesReceived, totalBytesToReceive }, null);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                this.worker.Dispose();
            }
        }
    }
}
