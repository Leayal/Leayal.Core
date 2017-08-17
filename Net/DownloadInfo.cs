using System;
namespace Leayal.Net
{
    public class DownloadInfo
    {
        public System.Uri URL
        { get; private set; }
        public string Filename
        { get; private set; }

        public DownloadInfo(System.Uri uUrl, string sFilename)
        {
            this.URL = uUrl;
            this.Filename = sFilename;
        }
    }
}
