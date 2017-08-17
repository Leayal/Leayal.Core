using System.Collections.Generic;
using System.Net;
using System;

namespace Leayal.Net
{
    internal class HttpWebRequestHeaders : IDisposable
    {
        Dictionary<HttpRequestHeader, string> innerDictionary;
        public HttpWebRequestHeaders()
        {
            this.innerDictionary = new Dictionary<HttpRequestHeader, string>();
        }

        public string this[HttpRequestHeader key]
        {
            get
            {
                if (!this.ContainsKey(key))
                    return null;
                else
                {
                    if (string.IsNullOrEmpty(this.innerDictionary[key]))
                        return null;
                    else
                        return this.innerDictionary[key];
                }

            }

            set
            {
                if (!this.ContainsKey(key))
                    this.Add(key, value);
                else
                    this.innerDictionary[key] = value;
            }
        }

        public int Count { get { return this.innerDictionary.Count; } }

        public ICollection<HttpRequestHeader> Keys { get { return this.innerDictionary.Keys; } }

        public ICollection<string> Values { get { return this.innerDictionary.Values; } }

        public void Add(HttpRequestHeader key, string value) { this.innerDictionary.Add(key, value); }

        public void Clear() { this.innerDictionary.Clear(); }

        public bool ContainsKey(HttpRequestHeader key) { return this.innerDictionary.ContainsKey(key); }

        private bool _disposed;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            this.Clear();
        }

        public IEnumerator<KeyValuePair<HttpRequestHeader, string>> GetEnumerator() { return this.GetEnumerator(); }

        public bool Remove(HttpRequestHeader key) { return this.innerDictionary.Remove(key); }

        public bool TryGetValue(HttpRequestHeader key, out string value) { return this.innerDictionary.TryGetValue(key, out value); }
    }
}
