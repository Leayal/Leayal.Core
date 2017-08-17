using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Leayal
{
    public static class UriHelper
    {
        internal static readonly Dictionary<string, Protocol> ProtocolDict = createquickprotocoldict();
        private static Dictionary<string, Protocol> createquickprotocoldict()
        {
            Dictionary<string, Protocol> result = new Dictionary<string, Protocol>();
            result.Add(System.Uri.UriSchemeFile, Protocol.File);
            result.Add(System.Uri.UriSchemeFtp, Protocol.Ftp);
            result.Add(System.Uri.UriSchemeGopher, Protocol.Gopher);
            result.Add(System.Uri.UriSchemeHttp, Protocol.Http);
            result.Add(System.Uri.UriSchemeHttps, Protocol.Https);
            result.Add(System.Uri.UriSchemeMailto, Protocol.Mailto);
            result.Add(System.Uri.UriSchemeNetPipe, Protocol.NetPipe);
            result.Add(System.Uri.UriSchemeNetTcp, Protocol.NetTcp);
            result.Add(System.Uri.UriSchemeNews, Protocol.News);
            result.Add(System.Uri.UriSchemeNntp, Protocol.Nntp);
            return result;
        }

        public enum Protocol : byte
        {
            Unknown,
            File,
            Ftp,
            Gopher,
            Http,
            Https,
            Mailto,
            NetPipe,
            NetTcp,
            News,
            Nntp
        }

        public static bool IsHttp(string url)
        {
            switch (GetProtocol(url))
            {
                case Protocol.Http:
                    return true;
                case Protocol.Https:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsHttp(this System.Uri url)
        {
            switch (GetProtocol(url))
            {
                case Protocol.Http:
                    return true;
                case Protocol.Https:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsProtocol(string url, Protocol protocol)
        {
            return ((GetProtocol(url) & protocol) == protocol);
        }

        public static bool IsProtocol(this System.Uri url, Protocol protocol)
        {
            return ((GetProtocol(url) & protocol) == protocol);
        }

        public static Protocol GetProtocol(this System.Uri url)
        {
            return GetProtocolEx(url.Scheme);
        }

        public static Protocol GetProtocol(string url)
        {
            int index = url.IndexOf(System.Uri.SchemeDelimiter);
            if (index > -1)
                return GetProtocolEx(url.Substring(0, index));
            else
                return Protocol.Unknown;
        }

        private static Protocol GetProtocolEx(string protocolText)
        {
            if (ProtocolDict.ContainsKey(protocolText))
                return ProtocolDict[protocolText];
            else
                return Protocol.Unknown;
        }

        public static string URLConcat(string url1, string url2)
        {
            if (string.IsNullOrWhiteSpace(url1) | string.IsNullOrWhiteSpace(url2))
                return string.Empty;
            else
                return url1.URLtrim() + "/" + url2.URLtrim();
        }

        public static string URLConcat(params string[] urls)
        {
            if (urls.Length > 0)
            {
                if (urls.Length == 1)
                {
                    if (string.IsNullOrWhiteSpace(urls[0]))
                        return string.Empty;
                    else
                        return urls[0];
                }
                else
                {
                    int capacity = 0;
                    for (int i = 0; i < urls.Length; i++)
                    {
                        if ((capacity + urls[i].Length) > int.MaxValue)
                            break;
                        capacity += (urls[i].Length + 1);
                    }
                    System.Text.StringBuilder result = new System.Text.StringBuilder(capacity);
                    if (!string.IsNullOrWhiteSpace(urls[0]))
                        result.Append(urls[0]);
                    for (int i = 1; i < urls.Length; i++)
                        if (!string.IsNullOrWhiteSpace(urls[i]))
                            result.Append("/" + urls[i]);
                    return result.ToString();
                }
            }
            else
                return string.Empty;
        }

        public static string URLtrim(this string url)
        { return url.Trim('\\', '/', ' '); }

        public static bool GetResolvedConnecionIPAddress(this System.Uri serverNameOrURL, out IPAddress resolvedIPAddress)
        {
            return GetResolvedConnecionIPAddress(serverNameOrURL.Host, out resolvedIPAddress);
        }

        public static bool GetResolvedConnecionIPAddress(string serverNameOrURL, out IPAddress resolvedIPAddress)
        {
            bool isResolved = false;
            IPHostEntry hostEntry = null;
            IPAddress resolvIP = null;
            try
            {
                if (!IPAddress.TryParse(serverNameOrURL, out resolvIP))
                {
                    hostEntry = Dns.GetHostEntry(serverNameOrURL);
                    if (hostEntry != null && hostEntry.AddressList != null && hostEntry.AddressList.Length > 0)
                    {
                        if (hostEntry.AddressList.Length == 1)
                        {
                            resolvIP = hostEntry.AddressList[0];
                            isResolved = true;
                        }
                        else
                            foreach (IPAddress vars in hostEntry.AddressList)
                                if (vars.AddressFamily == AddressFamily.InterNetwork)
                                {
                                    resolvIP = vars;
                                    isResolved = true;
                                    break;
                                }
                    }
                }
                else
                    isResolved = true;
            }
            catch (System.Exception)
            {
                isResolved = false;
                resolvIP = null;
            }
            finally
            { resolvedIPAddress = resolvIP; }
            return isResolved;
        }
    }
}
