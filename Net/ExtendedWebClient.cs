using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;

namespace Leayal.Net
{
    public class ExtendedWebClient : IDisposable
    {
        private Leayal.Net.WebClient innerWebClient;
        private short retried;
        private string downloadfileLocalPath;
        private System.Threading.SynchronizationContext SyncContext;

        public ExtendedWebClient() : this(new Leayal.Net.WebClient()) { }
        public ExtendedWebClient(Leayal.Net.WebClient baseWebClient)
        {
            this.innerWebClient = baseWebClient;
            this.SyncContext = System.Threading.SynchronizationContext.Current;
            this.innerWebClient.DownloadStringCompleted += InnerWebClient_DownloadStringCompleted;
            this.innerWebClient.DownloadDataCompleted += InnerWebClient_DownloadDataCompleted;
            this.innerWebClient.DownloadFileCompleted += InnerWebClient_DownloadFileCompleted;
            this.innerWebClient.DownloadProgressChanged += InnerWebClient_DownloadProgressChanged;
            this.innerWebClient.DownloadToMemoryCompleted += InnerWebClient_DownloadToMemoryCompleted;
            this.retried = 0;
            this.Retry = 4;
            this.LastURL = null;
            this.downloadfileLocalPath = null;
            this.IsBusy = false;
        }

        private void InnerWebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (e.UserState is DownloadAsyncWrapper)
            { }
            else
            {
                this.DownloadProgressChanged?.Invoke(sender, e);
                //this.OnDownloadProgressChanged(e);
            }
        }

        #region "Properties"
        public bool AutoCredentials { get { return this.innerWebClient.AutoCredentials; } set { this.innerWebClient.AutoCredentials = value; } }
        public ICredentials Credentials { get { return this.innerWebClient.Credentials; } set { this.innerWebClient.Credentials = value; } }
        public bool AutoUserAgent { get { return this.innerWebClient.AutoUserAgent; } set { this.innerWebClient.AutoUserAgent = value; } }
        public Leayal.Net.CacheStorage CacheStorage { get { return this.innerWebClient.CacheStorage; } set { this.innerWebClient.CacheStorage = value; } }
        private short Retry { get; set; }
        public bool IsBusy { get; private set; }
        public System.Uri LastURL { get; private set; }
        public WebHeaderCollection Headers { get { return this.innerWebClient.Headers; } set { this.innerWebClient.Headers = value; } }
        public IWebProxy Proxy { get { return this.innerWebClient.Proxy; } set { this.innerWebClient.Proxy = value; } }
        public System.Net.Cache.RequestCachePolicy CachePolicy { get { return this.innerWebClient.CachePolicy; } set { this.innerWebClient.CachePolicy = value; } }
        public int TimeOut { get { return this.innerWebClient.TimeOut; } set { this.innerWebClient.TimeOut = value; } }
        public string UserAgent { get { return this.innerWebClient.UserAgent; } set { this.innerWebClient.UserAgent = value; } }
        public WebHeaderCollection ResponseHeaders { get { return this.innerWebClient.ResponseHeaders; } }
        #endregion

        #region "Open"
        public WebRequest CreateRequest(System.Uri url, string _method, WebHeaderCollection _headers, IWebProxy _proxy, int _timeout, System.Net.Cache.RequestCachePolicy _cachePolicy)
        {
            return innerWebClient.CreateRequest(url, _method, _headers, _proxy, _timeout, _cachePolicy);
        }

        public WebRequest CreateRequest(System.Uri url, WebHeaderCollection _headers, IWebProxy _proxy, int _timeout, System.Net.Cache.RequestCachePolicy _cachePolicy)
        {
            return this.CreateRequest(url, string.Empty, _headers, _proxy, _timeout, _cachePolicy);
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

        private WebResponse Open(WebRequest request)
        {
            this.LastURL = request.RequestUri;
            return this.innerWebClient.Open(request);
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

        #region "OpenRead"
        public Stream OpenRead(System.Uri url)
        {
            this.LastURL = url;
            return this.innerWebClient.OpenRead(url);
        }

        public Stream OpenRead(string url)
        {
            return this.OpenRead(new System.Uri(url));
        }
        #endregion

        #region "DownloadToMemory"
        public IO.RecyclableMemoryStream DownloadToMemory(string address, string localpath)
        {
            return this.DownloadToMemory(new System.Uri(address), localpath);
        }

        public IO.RecyclableMemoryStream DownloadToMemory(System.Uri address, string tag)
        {
            IO.RecyclableMemoryStream result = null;
            if (!this.IsBusy)
            {
                OnWorkStarted();
                this.retried = 0;
                this.LastURL = address;
                for (short i = 0; i < Retry; i++)
                    try
                    {
                        result = this.innerWebClient.DownloadToMemory(address, tag);
                        OnWorkFinished();
                        break;
                    }
                    catch (WebException ex)
                    {
                        if (IsHTTP(address))
                        {
                            if (ex.Response is HttpWebResponse)
                                if (!WorthRetry(ex.Response as HttpWebResponse))
                                {
                                    if (result != null)
                                        result.Dispose();
                                    result = null;
                                    OnWorkFinished();
                                    throw ex;
                                }
                        }
                        else
                        {
                            if (result != null)
                                result.Dispose();
                            result = null;
                            OnWorkFinished();
                            throw ex;
                        }
                    }
            }
            return result;
        }

        public void DownloadToMemoryAsync(System.Uri address, string tag)
        {
            this.DownloadToMemoryAsync(address, tag, null);
        }

        public void DownloadToMemoryAsync(System.Uri address, string tag, object userToken)
        {
            if (!this.IsBusy)
            {
                OnWorkStarted();
                this.retried = 0;
                this.LastURL = address;
                this.IsBusy = true;
                this.lasttag = tag;
                this.innerWebClient.DownloadToMemoryAsync(address, tag, userToken);
            }
        }

        string lasttag = null;

        private void InnerWebClient_DownloadToMemoryCompleted(object sender, DownloadToMemoryCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                if (e.Error is WebException)
                {
                    WebException ex = e.Error as WebException;
                    if (IsHTTP(this.LastURL))
                    {
                        if (ex.Response is HttpWebResponse && WorthRetry(ex.Response as HttpWebResponse))
                            this.innerWebClient.DownloadToMemoryAsync(this.LastURL, lasttag, e.UserState);
                        else
                            this.OnDownloadToMemoryCompleted(e);
                    }
                    else
                        this.OnDownloadToMemoryCompleted(e);
                }
                else if (e.Error.InnerException is WebException)
                {
                    WebException ex = e.Error.InnerException as WebException;
                    if (IsHTTP(this.LastURL))
                    {
                        if (ex.Response is HttpWebResponse && WorthRetry(ex.Response as HttpWebResponse))
                            this.innerWebClient.DownloadStringAsync(this.LastURL);
                        else
                            this.OnDownloadToMemoryCompleted(e);
                    }
                    else
                        this.OnDownloadToMemoryCompleted(e);
                }
            }
            else if (e.Cancelled)
            { this.OnDownloadToMemoryCompleted(e); }
            else
            { this.OnDownloadToMemoryCompleted(e); }
        }
        #endregion

        #region "DownloadString"
        public string DownloadString(string address)
        {
            return this.DownloadString(new System.Uri(address));
        }

        public string DownloadString(System.Uri address)
        {
            string result = string.Empty;
            if (!this.IsBusy)
            {
                OnWorkStarted();
                this.retried = 0;
                this.LastURL = address;
                for (short i = 0; i < Retry; i++)
                {
                    try
                    {
                        result = this.innerWebClient.DownloadString(address);
                        OnWorkFinished();
                        break;
                    }
                    catch (WebException ex)
                    {
                        if (IsHTTP(address))
                        {
                            if (ex.Response is HttpWebResponse && !WorthRetry(ex.Response as HttpWebResponse))
                            {
                                OnWorkFinished();
                                throw ex;
                            }
                        }
                        else
                        {
                            OnWorkFinished();
                            throw ex;
                        }
                    }
                }
            }
            return result;
        }

        public void DownloadStringAsync(System.Uri address)
        {
            this.DownloadStringAsync(address, null);
        }

        public void DownloadStringAsync(System.Uri address, object userToken)
        {
            this.retried = 0;
            this.LastURL = address;
            this.IsBusy = true;
            this.innerWebClient.DownloadStringAsync(address, userToken);
        }

        private void InnerWebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                if (e.Error is WebException)
                {
                    WebException ex = e.Error as WebException;
                    if (IsHTTP(this.LastURL))
                    {
                        if (ex.Response is HttpWebResponse && WorthRetry(ex.Response as HttpWebResponse))
                            this.innerWebClient.DownloadStringAsync(this.LastURL, e.UserState);
                        else
                            this.OnDownloadStringCompleted(GetDownloadStringCompletedEventArgs(ex, e.Cancelled, e.Result, e.UserState));
                    }
                    else
                        this.OnDownloadStringCompleted(GetDownloadStringCompletedEventArgs(e.Error, e.Cancelled, e.Result, e.UserState));
                }
                else if (e.Error.InnerException is WebException)
                {
                    WebException ex = e.Error.InnerException as WebException;
                    if (IsHTTP(this.LastURL))
                    {
                        if (ex.Response is HttpWebResponse && WorthRetry(ex.Response as HttpWebResponse))
                            this.innerWebClient.DownloadStringAsync(this.LastURL);
                        else
                            this.OnDownloadStringCompleted(GetDownloadStringCompletedEventArgs(ex, e.Cancelled, e.Result, e.UserState));
                    }
                    else
                        this.OnDownloadStringCompleted(GetDownloadStringCompletedEventArgs(e.Error.InnerException, e.Cancelled, e.Result, e.UserState));
                }
            }
            else if (e.Cancelled)
            { this.OnDownloadStringCompleted(GetDownloadStringCompletedEventArgs(e.Error, e.Cancelled, e.Result, e.UserState)); }
            else
            {
                this.OnDownloadStringCompleted(GetDownloadStringCompletedEventArgs(e.Error, e.Cancelled, e.Result, e.UserState));
            }
        }
        #endregion

        #region "DownloadData"
        public byte[] DownloadData(string address)
        {
            return this.DownloadData(new System.Uri(address));
        }

        public byte[] DownloadData(System.Uri address)
        {
            byte[] result = null;
            if (!this.IsBusy)
            {
                OnWorkStarted();
                this.retried = 0;
                this.LastURL = address;
                for (short i = 0; i < Retry; i++)
                {
                    try
                    {
                        result = this.innerWebClient.DownloadData(address);
                        OnWorkFinished();
                        break;
                    }
                    catch (WebException ex)
                    {
                        if (IsHTTP(address))
                        {
                            if (ex.Response is HttpWebResponse && !WorthRetry(ex.Response as HttpWebResponse))
                            {
                                OnWorkFinished();
                                throw ex;
                            }
                        }
                        else
                        {
                            OnWorkFinished();
                            throw ex;
                        }
                    }
                }
            }
            return result;
        }

        public void DownloadDataAsync(System.Uri address)
        {
            this.DownloadDataAsync(address, null);
        }

        public void DownloadDataAsync(System.Uri address, object userToken)
        {
            if (!this.IsBusy)
            {
                OnWorkStarted();
                this.retried = 0;
                this.LastURL = address;
                this.IsBusy = true;
                this.innerWebClient.DownloadDataAsync(address, userToken);
            }
        }

        private void InnerWebClient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                if (e.Error is WebException)
                {
                    WebException ex = e.Error as WebException;
                    if (IsHTTP(this.LastURL))
                    {
                        if (ex.Response is HttpWebResponse && WorthRetry(ex.Response as HttpWebResponse))
                            this.innerWebClient.DownloadDataAsync(this.LastURL, e.UserState);
                        else
                            this.OnDownloadDataCompleted(GetDownloadDataCompletedEventArgs(ex, e.Cancelled, e.Result, e.UserState));
                    }
                    else
                        this.OnDownloadDataCompleted(GetDownloadDataCompletedEventArgs(e.Error, e.Cancelled, e.Result, e.UserState));
                }
                else if (e.Error.InnerException is WebException)
                {
                    WebException ex = e.Error.InnerException as WebException;
                    if (IsHTTP(this.LastURL))
                    {
                        if (ex.Response is HttpWebResponse && WorthRetry(ex.Response as HttpWebResponse))
                            this.innerWebClient.DownloadStringAsync(this.LastURL);
                        else
                            this.OnDownloadDataCompleted(GetDownloadDataCompletedEventArgs(ex, e.Cancelled, e.Result, e.UserState));
                    }
                    else
                        this.OnDownloadDataCompleted(GetDownloadDataCompletedEventArgs(e.Error.InnerException, e.Cancelled, e.Result, e.UserState));
                }
            }
            else if (e.Cancelled)
            { this.OnDownloadDataCompleted(GetDownloadDataCompletedEventArgs(e.Error, e.Cancelled, e.Result, e.UserState)); }
            else
            { this.OnDownloadDataCompleted(GetDownloadDataCompletedEventArgs(e.Error, e.Cancelled, e.Result, e.UserState)); }
        }
        #endregion

        #region "DownloadFile"
        public bool DownloadFile(string address, string localpath)
        {
            return this.DownloadFile(new System.Uri(address), localpath);
        }

        public bool DownloadFile(System.Uri address, string localpath)
        {
            bool result = false;
            if (!this.IsBusy)
            {
                OnWorkStarted();
                this.retried = 0;
                this.LastURL = address;
                string tmpPath = localpath + ".dtmp";
                for (short i = 0; i < Retry; i++)
                    try
                    {
                        Microsoft.VisualBasic.FileIO.FileSystem.CreateDirectory(Microsoft.VisualBasic.FileIO.FileSystem.GetParentPath(tmpPath));
                        this.innerWebClient.DownloadFile(address, tmpPath);
                        File.Delete(localpath);
                        File.Move(tmpPath, localpath);
                        //File.Move(asd.FullName, localpath);
                        OnWorkFinished();
                        result = true;
                        break;
                    }
                    catch (WebException ex)
                    {
                        if (IsHTTP(address))
                        {
                            if (ex.Response is HttpWebResponse)
                                if (!WorthRetry(ex.Response as HttpWebResponse))
                                {
                                    File.Delete(tmpPath);
                                    OnWorkFinished();
                                    result = false;
                                    throw ex;
                                }
                        }
                        else
                        {
                            File.Delete(tmpPath);
                            OnWorkFinished();
                            result = false;
                            throw ex;
                        }
                    }
            }
            return result;
        }

        public void DownloadFileAsync(System.Uri address, string localPath)
        {
            this.DownloadFileAsync(address, localPath, null);
        }

        public void DownloadFileAsync(System.Uri address, string localPath, object userToken)
        {
            if (!this.IsBusy)
            {
                OnWorkStarted();
                DownloadFileAsyncEx(address, localPath, userToken);
            }
        }

        private void DownloadFileAsyncEx(System.Uri address, string localPath, object userToken)
        {
            this.retried = 0;
            this.downloadfileLocalPath = localPath;
            this.LastURL = address;
            this.IsBusy = true;
            if (File.Exists(localPath))
                File.Open(localPath, FileMode.Open).Close();
            else
                Microsoft.VisualBasic.FileIO.FileSystem.CreateDirectory(Path.GetDirectoryName(localPath));
            this.innerWebClient.DownloadFileAsync(address, localPath + ".dtmp", userToken);
        }

        public void DownloadFileListAsync(Dictionary<System.Uri, string> fileList, object userToken)
        {
            if (fileList.Count > 0)
            {
                DownloadInfoCollection list = new DownloadInfoCollection();
                foreach (var fileNode in fileList)
                    list.Add(new DownloadInfo(fileNode.Key, fileNode.Value));
                this.DownloadFileListAsync(list, userToken);
            }
        }

        public void DownloadFileListAsync(List<DownloadInfo> filelist, object userToken)
        {
            this.DownloadFileListAsync(new DownloadInfoCollection(filelist), userToken);
        }

        public void DownloadFileListAsync(DownloadInfo[] filelist, object userToken)
        {
            this.DownloadFileListAsync(new DownloadInfoCollection(filelist), userToken);
        }

        public void DownloadFileListAsync(DownloadInfoCollection filelist, object userToken)
        {
            if (!this.IsBusy)
            {
                if (!filelist.IsEmpty)
                {
                    DownloadInfo item = filelist.TakeFirst();
                    this.DownloadFileAsyncEx(item.URL, item.Filename, new DownloadAsyncWrapper(filelist, userToken));
                }
            }
        }

        private void InnerWebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            object token = null;
            DownloadAsyncWrapper info = null;
            if (e.UserState != null)
            {
                if (e.UserState is DownloadAsyncWrapper)
                {
                    info = e.UserState as DownloadAsyncWrapper;
                    token = info.userToken;
                }
                else
                    token = e.UserState;
            }
            if (e.Error != null)
            {
                if (e.Error is WebException)
                {
                    WebException ex = e.Error as WebException;
                    if (IsHTTP(this.LastURL))
                    {
                        if (ex.Response is HttpWebResponse && WorthRetry(ex.Response as HttpWebResponse))
                            this.innerWebClient.DownloadFileAsync(this.LastURL, this.downloadfileLocalPath + ".dtmp", e.UserState);
                        else
                            this.SeekActionDerpian(info, e, token);
                    }
                    else
                        this.SeekActionDerpian(info, e, token);
                }
                else if (e.Error.InnerException is WebException)
                {

                    WebException ex = e.Error.InnerException as WebException;
                    if (IsHTTP(this.LastURL))
                    {
                        if (ex.Response is HttpWebResponse && WorthRetry(ex.Response as HttpWebResponse))
                            this.innerWebClient.DownloadStringAsync(this.LastURL);
                        else
                            this.SeekActionDerpian(info, e, token);
                    }
                    else
                        this.SeekActionDerpian(info, e, token);
                }
                else
                {
                    this.SeekActionDerpian(info, e, token);
                }
            }
            else if (e.Cancelled)
            { this.OnDownloadFileCompleted(new AsyncCompletedEventArgs(e.Error, e.Cancelled, token)); }
            else
            {
                try
                {
                    File.Delete(this.downloadfileLocalPath);
                    File.Move(this.downloadfileLocalPath + ".dtmp", this.downloadfileLocalPath);
                }
                catch (Exception ex) { Log.LogManager.GeneralLog.Print(ex); }
                this.SeekActionDerpian(info, e, token);
            }
        }

        private void SeekActionDerpian(DownloadAsyncWrapper info, AsyncCompletedEventArgs e, object token)
        {
            if ((info != null) && (!info.filelist.IsEmpty))
            {
                DownloadInfo item = info.filelist.TakeFirst();
                if (item == null)
                {
                    this.OnDownloadFileCompleted(new AsyncCompletedEventArgs(e.Error, e.Cancelled, token));
                    return;
                }
                this.OnDownloadFileProgressChanged(new DownloadFileProgressChangedEventArgs(info.filelist.CurrentItemCount, info.filelist.Count));
                this.DownloadFileAsyncEx(item.URL, item.Filename, info);
            }
            else
            { this.OnDownloadFileCompleted(new AsyncCompletedEventArgs(e.Error, e.Cancelled, token)); }
        }
        #endregion

        #region "Public Methods"
        public void Dispose()
        {
            this.innerWebClient.Dispose();
        }

        public void CancelAsync()
        {
            this.innerWebClient.CancelAsync();
        }

        public void ClearProgressEvents()
        {
            DownloadProgressChangedEventHandler myDe = this.DownloadProgressChanged;
            while (myDe != null)
            {
                this.DownloadProgressChanged -= myDe;
                myDe = this.DownloadProgressChanged;
            }
            /*
            FieldInfo f1 = typeof(ExtendedWebClient).GetField("DownloadProgressChanged", BindingFlags.Static | BindingFlags.NonPublic);
            object obj = f1.GetValue(this);
            PropertyInfo pi = this.GetType().GetProperty("Events",
                BindingFlags.NonPublic | BindingFlags.Instance);
            EventHandlerList list = (EventHandlerList)pi.GetValue(this, null);
            list.RemoveHandler(obj, list[obj]);
            */
        }
        #endregion

        #region "Private Methods"
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

        private bool WorthRetry(HttpWebResponse rep)
        {
            if (retried > this.Retry)
                return false;
            else
            {
                retried++;
                switch (rep.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return false;
                    case HttpStatusCode.NotAcceptable:
                        return false;
                    case HttpStatusCode.Unauthorized:
                        return false;
                    case HttpStatusCode.ServiceUnavailable:
                        return false;
                    case HttpStatusCode.NotImplemented:
                        return false;
                    case HttpStatusCode.PaymentRequired:
                        return false;
                    case HttpStatusCode.PreconditionFailed:
                        return false;
                    case HttpStatusCode.ProxyAuthenticationRequired:
                        return false;
                    default:
                        return true;
                }
            }
        }
        #endregion

        #region "Events"
        private System.Threading.SynchronizationContext GetSyncContext()
        {
            if (this.SyncContext == null)
                this.SyncContext = System.Threading.SynchronizationContext.Current;
            return this.SyncContext;
        }
        public event EventHandler WorkFinished;
        protected virtual void OnWorkFinished()
        {
            this.IsBusy = false;
            this.GetSyncContext()?.Post(new System.Threading.SendOrPostCallback(delegate { this.WorkFinished?.Invoke(this, EventArgs.Empty); }), null);
        }
        public event EventHandler WorkStarted;
        protected virtual void OnWorkStarted()
        {
            this.IsBusy = true;
            if (WorkStarted != null)
                this.GetSyncContext()?.Post(new System.Threading.SendOrPostCallback(delegate { this.WorkStarted.Invoke(this, EventArgs.Empty); }), null);
        }

        public event DownloadProgressChangedEventHandler DownloadProgressChanged;
        protected void OnDownloadProgressChanged(DownloadProgressChangedEventArgs e)
        {
            if (DownloadProgressChanged != null)
                this.GetSyncContext()?.Post(new System.Threading.SendOrPostCallback(delegate { this.DownloadProgressChanged.Invoke(this, e); }), null);
        }

        public event AsyncCompletedEventHandler DownloadFileCompleted;
        protected virtual void OnDownloadFileCompleted(AsyncCompletedEventArgs e)
        {
            OnWorkFinished();
            if (DownloadFileCompleted != null)
                this.GetSyncContext()?.Post(new System.Threading.SendOrPostCallback(delegate { this.DownloadFileCompleted.Invoke(this, e); }), null);
        }
        
        public event DownloadDataCompletedEventHandler DownloadDataCompleted;
        protected virtual void OnDownloadDataCompleted(DownloadDataCompletedEventArgs e)
        {
            OnWorkFinished();
            if (DownloadDataCompleted != null)
                this.GetSyncContext()?.Post(new System.Threading.SendOrPostCallback(delegate { this.DownloadDataCompleted.Invoke(this, e); }), null);
        }

        public event EventHandler<DownloadToMemoryCompletedEventArgs> DownloadToMemoryCompleted;
        protected virtual void OnDownloadToMemoryCompleted(DownloadToMemoryCompletedEventArgs e)
        {
            OnWorkFinished();
            if (DownloadToMemoryCompleted != null)
                this.GetSyncContext()?.Post(new System.Threading.SendOrPostCallback(delegate { this.DownloadToMemoryCompleted.Invoke(this, e); }), null);
        }

        public event DownloadStringCompletedEventHandler DownloadStringCompleted;
        protected virtual void OnDownloadStringCompleted(DownloadStringCompletedEventArgs e)
        {
            OnWorkFinished();
            if (DownloadStringCompleted != null)
                this.GetSyncContext()?.Post(new System.Threading.SendOrPostCallback(delegate { this.DownloadStringCompleted.Invoke(this, e); }), null);
        }

        public delegate void DownloadFileProgressChangedEventHandler(object sender, DownloadFileProgressChangedEventArgs e);
        public event DownloadFileProgressChangedEventHandler DownloadFileProgressChanged;
        protected void OnDownloadFileProgressChanged(DownloadFileProgressChangedEventArgs e)
        {
            if (DownloadFileProgressChanged != null)
                this.GetSyncContext()?.Post(new System.Threading.SendOrPostCallback(delegate { this.DownloadFileProgressChanged.Invoke(this, e); }), null);
        }
        #endregion

        protected DownloadDataCompletedEventArgs GetDownloadDataCompletedEventArgs(System.Exception ex, bool cancelled, byte[] bytes, object usertoken)
        {
            return (DownloadDataCompletedEventArgs)Activator.CreateInstance(typeof(DownloadDataCompletedEventArgs),
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null, new object[] { bytes, ex, cancelled, usertoken }, null);
        }

        protected DownloadStringCompletedEventArgs GetDownloadStringCompletedEventArgs(System.Exception ex, bool cancelled, string str, object usertoken)
        {
            return (DownloadStringCompletedEventArgs)Activator.CreateInstance(typeof(DownloadStringCompletedEventArgs),
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null, new object[] { str, ex, cancelled, usertoken }, null);
        }

        #region "Private Class"
        private class DownloadAsyncWrapper
        {
            public object userToken { get; }
            public DownloadInfoCollection filelist;
            public DownloadAsyncWrapper(DownloadInfoCollection list, object token)
            {
                this.userToken = token;
                this.filelist = list;
            }
            public DownloadAsyncWrapper(DownloadInfoCollection list) : this(list, null) { }
        }
        #endregion
    }
}
