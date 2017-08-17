using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Leayal.Ini
{
    public sealed class DuplicatedKeyCollection
    {
        private Dictionary<string, DuplicatedKeyInfo> dupDict;
        internal DuplicatedKeyCollection()
        {
            this.dupDict = new Dictionary<string, DuplicatedKeyInfo>(StringComparer.OrdinalIgnoreCase);
        }

        public int Count => this.dupDict.Count;

        public IEnumerable<string> Keys => this.dupDict.Keys.AsEnumerable();

        public DuplicatedKeyInfo this[string key] => this.dupDict[key];

        internal void Add(string key, int lineNumber, KeyType type)
        {
            if (this.dupDict.ContainsKey(key))
                this.dupDict[key].Add(lineNumber);
            else
            {
                var dki = new DuplicatedKeyInfo(type);
                dki.Add(lineNumber);
                this.dupDict.Add(key, dki);
            }
        }

        public override string ToString()
        {
            int theMainCount = this.Count;
            if (theMainCount == 0) return string.Empty;
            StringBuilder sb = new StringBuilder();
            if (theMainCount == 1)
                sb.Append("There is " + theMainCount.ToString() + " error.");
            else
                sb.Append("There are " + theMainCount.ToString() + " errors.");
            int[] _lines;
            foreach (var key in this.dupDict)
                if (key.Value.RepeatedCount > 0)
                {
                    _lines = key.Value.Lines;
                    switch (key.Value.Type)
                    {
                        case KeyType.KeyValue:
                            if (key.Value.RepeatedCount == 1)
                                sb.AppendFormat("\r\nThe Keyvalue '{0}' has been repeated 1 time. At line: ", key.Key);
                            else
                                sb.AppendFormat("\r\nThe Keyvalue '{0}' has been repeated {1} times. At lines: ", key.Key, key.Value.RepeatedCount);
                            for (int i = 0; i < _lines.Length; i++)
                            {
                                if (i == 0)
                                    sb.Append(_lines[i]);
                                else
                                    sb.AppendFormat(", {0}", _lines[i]);
                            }
                            break;
                        case KeyType.Section:
                            if (key.Value.RepeatedCount == 1)
                                sb.AppendFormat("\r\nThe Section '{0}' has been repeated 1 time. At line: ", key.Key);
                            else
                                sb.AppendFormat("\r\nThe Section '{0}' has been repeated {1} times. At lines: ", key.Key, key.Value.RepeatedCount);
                            for (int i = 0; i < _lines.Length; i++)
                            {
                                if (i == 0)
                                    sb.Append(_lines[i]);
                                else
                                    sb.AppendFormat(", {0}", _lines[i]);
                            }
                            break;
                        case KeyType.Unknown:
                            if (key.Value.RepeatedCount == 1)
                                sb.AppendFormat("\r\nFound 1 corrupted entry", key.Key);
                            else
                                sb.AppendFormat("\r\nFound " + key.Value.RepeatedCount + " corrupted entries.");
                            for (int i = 0; i < _lines.Length; i++)
                            {
                                if (i == 0)
                                    sb.Append(_lines[i]);
                                else
                                    sb.AppendFormat(", {0}", _lines[i]);
                            }
                            break;
                    }
                }
            return sb.ToString();
        }
    }
}
