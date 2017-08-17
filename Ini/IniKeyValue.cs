namespace Leayal.Ini
{
    public sealed class IniKeyValue
    {
        private bool m_IsComment;
        public bool IsComment
        {
            get { return this.m_IsComment; }
            set { this.m_IsComment = value; }
        }

        private string m_Value;
        public string Value
        {
            get { return this.m_Value; }
            set { this.m_Value = value; }
        }

        public IniKeyValue(string Value) : this(Value, false)
        {
        }

        public IniKeyValue(string Value, bool IsComment)
        {
            this.m_IsComment = IsComment;
            this.m_Value = Value;
        }
    }
}
