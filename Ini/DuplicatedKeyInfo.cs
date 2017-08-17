using System.Collections.Generic;
using System.Linq;

namespace Leayal.Ini
{
    public sealed class DuplicatedKeyInfo
    {
        private List<int> innerList;

        public int[] Lines => this.innerList.ToArray();

        public int RepeatedCount => this.innerList.Count;

        public int this[int index] => this.innerList[index];

        public KeyType Type { get; }

        internal void Add(int lineNumber)
        {
            this.innerList.Add(lineNumber);
        }

        internal DuplicatedKeyInfo(KeyType _type)
        {
            this.innerList = new List<int>();
            this.Type = _type;
        }
    }

    public enum KeyType : byte
    {
        Unknown,
        Section,
        KeyValue
    }
}
