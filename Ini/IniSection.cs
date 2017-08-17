using System.Collections.Concurrent;

namespace Leayal.Ini
{
    public sealed class IniSection
    {
        private bool m_IsComment;
        public bool IsComment
        {
            get { return this.m_IsComment; }
            set { this.m_IsComment = value; }
        }

        private System.Collections.Generic.List<string> m_CommentList;
        public System.Collections.Generic.List<string> CommentList
        {
            get { return this.m_CommentList; }
        }

        public ConcurrentDictionary<string, IniKeyValue> IniKeyValues { get; }

        public IniSection() : this(false) { }

        public IniSection(bool IsComment)
        {
            this.m_IsComment = IsComment;
            this.IniKeyValues = new ConcurrentDictionary<string, IniKeyValue>(System.StringComparer.OrdinalIgnoreCase);
            this.m_CommentList = new System.Collections.Generic.List<string>();
        }

        public bool RemoveValue(string key)
        {
            return this.IniKeyValues.TryRemove(key, out var val);
        }

        public bool IsEmpty { get { return this.IniKeyValues.IsEmpty; } }

        public void Clear()
        {
            this.IniKeyValues.Clear();
            this.m_CommentList.Clear();
        }
    }
}
