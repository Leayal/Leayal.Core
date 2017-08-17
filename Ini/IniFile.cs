using Microsoft.VisualBasic;
using System.Linq;
using System.Collections.Concurrent;
using System.Text;
using System;

namespace Leayal.Ini
{
    public sealed class IniFile
    {
        private string sFilename;
        private System.Text.StringBuilder tmpStringBuild;

        private ConcurrentDictionary<string, IniSection> o_Sections;
        #region "Constructors"
        public IniFile(string filePath) : this(filePath, null) { }

        public IniFile(string filePath, IniReadErrorEventHandler IniReadErrorCallback)
        {
            this.o_Sections = new ConcurrentDictionary<string, IniSection>(StringComparer.OrdinalIgnoreCase);
            this.sFilename = filePath;
            this.tmpStringBuild = new System.Text.StringBuilder();
            if (System.IO.File.Exists(filePath))
                using (System.IO.StreamReader theReader = new System.IO.StreamReader(filePath))
                    this.ReadIniFromTextStream(theReader, IniReadErrorCallback);
        }

        public IniFile(System.IO.TextReader Stream, bool CloseAfterRead = true) : this(Stream, null, CloseAfterRead) { }

        public IniFile(System.IO.TextReader Stream, IniReadErrorEventHandler IniReadErrorCallback, bool CloseAfterRead = true) : this()
        {
            this.ReadIniFromTextStream(Stream, IniReadErrorCallback);
            if (CloseAfterRead)
                Stream.Dispose();
        }

        /// <summary>
        /// Create empty INI file in memory.
        /// </summary>
        public IniFile()
        {
            this.o_Sections = new ConcurrentDictionary<string, IniSection>();
            this.sFilename = string.Empty;
            this.tmpStringBuild = new System.Text.StringBuilder();
        }
        #endregion

        #region "Properties"
        public bool IsEmpty
        {
            get
            {
                if (!this.o_Sections.IsEmpty)
                    foreach (var aliwghaliwhg in this.o_Sections.Values.Where((x) => { return !x.IsEmpty; }))
                        return false;
                return true;
            }
        }
        #endregion

        #region "Methods"
        public bool RemoveSection(string section)
        {
            if (this.o_Sections.TryRemove(section, out var sec))
            {
                sec.Clear();
                return true;
            }
            else
                return false;
        }

        public bool RemoveValue(string section, string key)
        {
            if (this.o_Sections.TryGetValue(section, out var val))
                return val.RemoveValue(key);
            else
                return false;
        }

        public string GetValue(string section, string key, string defaultValue)
        {
            if (this.o_Sections.TryGetValue(section, out var _inisection))
                if (_inisection.IniKeyValues.TryGetValue(key, out var _inivalue))
                    return _inivalue.Value;
            return defaultValue;
        }

        public void SetValue(string section, string key, string value)
        {
            IniSection _inisection;
            if (this.o_Sections.TryGetValue(section, out _inisection))
            {
                if (_inisection.IniKeyValues.TryGetValue(key, out var _inikey))
                    _inikey.Value = value;
                else
                    _inisection.IniKeyValues.TryAdd(key, new IniKeyValue(value));
            }
            else
            {
                _inisection = new IniSection();
                this.o_Sections.TryAdd(section, _inisection);
                _inisection.IniKeyValues.TryAdd(key, new IniKeyValue(value));                
            }
        }

        public ConcurrentDictionary<string, IniKeyValue> GetAllValues(string section)
        {
            if (this.o_Sections.TryGetValue(section, out var value))
                return value.IniKeyValues;
            else
                return null;
        }

        public void Save()
        {
            this.Save(System.Text.Encoding.UTF8);
        }

        public void Save(System.Text.Encoding encode)
        {
            if (string.IsNullOrWhiteSpace(this.sFilename))
            {
                return;
            }
            using (System.IO.StreamWriter theWriter = new System.IO.StreamWriter(this.sFilename, false, encode))
            {
                WriteToStream(theWriter);
            }
        }

        public void SaveAs(System.IO.TextWriter textWriter)
        {
            WriteToStream(textWriter);
        }

        public void SaveAs(string newPath)
        {
            this.SaveAs(newPath, System.Text.Encoding.UTF8);
        }

        public void SaveAs(string newPath, System.Text.Encoding encode)
        {
            if (string.IsNullOrWhiteSpace(newPath))
            {
                return;
            }
            this.sFilename = newPath;
            using (System.IO.StreamWriter theWriter = new System.IO.StreamWriter(newPath, false, encode))
            {
                WriteToStream(theWriter);
            }
        }

        public void Close()
        {
            this.tmpStringBuild.Clear();
            this.sFilename = null;
            this.o_Sections.Clear();
            this.tmpStringBuild = null;
            this.o_Sections = null;
        }

        public override string ToString()
        {
            this.tmpStringBuild.Clear();
            if (this.o_Sections.IsEmpty) return string.Empty;
            foreach (var Section_loopVariable in this.o_Sections)
            {
                if (Section_loopVariable.Value.IsComment == false)
                {
                    this.tmpStringBuild.AppendLine("[" + Section_loopVariable.Key + "]");
                }
                else
                {
                    this.tmpStringBuild.AppendLine(";[" + Section_loopVariable.Key + "]");
                }
                foreach (var KeyValue_loopVariable in Section_loopVariable.Value.IniKeyValues)
                {
                    if (Section_loopVariable.Value.IsComment == false)
                    {
                        this.tmpStringBuild.AppendLine(KeyValue_loopVariable.Key + "=" + KeyValue_loopVariable.Value.Value);
                    }
                    else
                    {
                        this.tmpStringBuild.AppendLine(";" + KeyValue_loopVariable.Key + "=" + KeyValue_loopVariable.Value.Value);
                    }
                }
            }
            return this.tmpStringBuild.ToString();
        }
        #endregion

        #region "Properties"
        public System.Collections.ObjectModel.ReadOnlyCollection<string> Sections
        {
            get { return new System.Collections.ObjectModel.ReadOnlyCollection<string>(this.o_Sections.Keys.ToList()); }
        }

        public int SectionCount
        {
            get { return this.o_Sections.Keys.Count; }
        }
        #endregion

        #region "Private Methods"
        private bool CheckSection(string theKey)
        {
            return this.o_Sections.ContainsKey(theKey);
        }

        private void ReadIniFromTextStream(System.IO.TextReader Stream, IniReadErrorEventHandler IniReadErrorCallback)
        {
            StringBuilder lineBuffer = new StringBuilder();
            int lineCount = 0;
            string pun = null, anothertmpStr;
            string[] splitBuffer = null;
            IniSection sectionBuffer = null;
            char[] buffer = new char[1];
            char[] spli = new char[] { '=' };
            DuplicatedKeyCollection dkc = new DuplicatedKeyCollection();
            /*try
            {//*/
                int buffercount = Stream.ReadBlock(buffer, 0, 1);
                while (buffercount > 0)
                {
                    switch (buffer[0])
                    {
                        case ControlChars.Lf:
                            lineCount++;
                            pun = lineBuffer.ToString();
                            lineBuffer.Clear();
                            if (!string.IsNullOrWhiteSpace(pun))
                            {
                                // move this line here because no need to check for every read char
                                if (pun.StartsWith("[") && pun.EndsWith("]"))
                                {
                                    anothertmpStr = pun.Substring(1, pun.Length - 2);
                                    sectionBuffer = new IniSection(false);
                                    if (!this.o_Sections.TryAdd(anothertmpStr, sectionBuffer))
                                        dkc.Add(anothertmpStr, lineCount, KeyType.Section);
                                }
                                else if (pun.IndexOf(spli[0]) > -1)
                                {
                                    if (sectionBuffer != null)
                                    {
                                        splitBuffer = pun.Split(spli, 2);
                                        bool isComment = false;
                                        if (splitBuffer[0].StartsWith(";"))
                                        {
                                            isComment = true;
                                            splitBuffer[0] = splitBuffer[0].Remove(0, 1);
                                        }
                                        if (!string.IsNullOrWhiteSpace(splitBuffer[0]))
                                        {
                                            anothertmpStr = splitBuffer[0].Trim();
                                            if (splitBuffer.Length == 1)
                                            {
                                                if (!sectionBuffer.IniKeyValues.TryAdd(anothertmpStr, new IniKeyValue(string.Empty, isComment)))
                                                    dkc.Add(anothertmpStr, lineCount, KeyType.KeyValue);
                                            }
                                            else
                                            {
                                                if (string.IsNullOrEmpty(splitBuffer[1]))
                                                {
                                                    if (!sectionBuffer.IniKeyValues.TryAdd(anothertmpStr, new IniKeyValue(string.Empty, isComment)))
                                                        dkc.Add(anothertmpStr, lineCount, KeyType.KeyValue);
                                                }
                                                else
                                                {
                                                    if (string.IsNullOrWhiteSpace(splitBuffer[1]))
                                                    {
                                                        if (!sectionBuffer.IniKeyValues.TryAdd(anothertmpStr, new IniKeyValue(string.Empty, isComment)))
                                                            dkc.Add(anothertmpStr, lineCount, KeyType.KeyValue);
                                                    }
                                                    else
                                                    {
                                                        if (!sectionBuffer.IniKeyValues.TryAdd(anothertmpStr, new IniKeyValue(this.UnescapeValue(splitBuffer[1].Trim()), isComment)))
                                                            dkc.Add(anothertmpStr, lineCount, KeyType.KeyValue);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    dkc.Add(this.UnescapeValue(pun), lineCount, KeyType.Unknown);
                                }
                            }
                            break;
                        case ControlChars.Cr:
                            break;
                        case ControlChars.NullChar:
                            break;
                        default:
                            lineBuffer.Append(buffer[0]);
                            break;
                    }
                    buffercount = Stream.ReadBlock(buffer, 0, 1);
                }
                pun = lineBuffer.ToString();
                if (!string.IsNullOrWhiteSpace(pun))
                {
                    //This will make sure last line without \n will not be discarded
                    if (pun.StartsWith("[") && pun.EndsWith("]"))
                    {
                        anothertmpStr = pun.Substring(1, lineBuffer.Length - 2);
                        sectionBuffer = new IniSection(false);
                        if (!this.o_Sections.TryAdd(anothertmpStr, sectionBuffer))
                            dkc.Add(anothertmpStr, lineCount, KeyType.Section);
                        lineBuffer.Clear();
                    }
                    else if (pun.IndexOf(spli[0]) > -1)
                    {
                        splitBuffer = pun.Split(spli, 2);
                        anothertmpStr = splitBuffer[0].Trim();
                        // make sure it split just one time
                        if (!sectionBuffer.IniKeyValues.TryAdd(anothertmpStr, new IniKeyValue(this.UnescapeValue(splitBuffer[1].Trim()))))
                            dkc.Add(anothertmpStr, lineCount, KeyType.KeyValue);
                        lineBuffer.Clear();
                    }
                    else
                        dkc.Add(this.UnescapeValue(pun), lineCount, KeyType.Unknown);
                }
                if (dkc.Count > 0)
                    if (IniReadErrorCallback != null)
                        IniReadErrorCallback.Invoke(this, new IniReadErrorEventArgs(dkc));
            /*}
            catch (Exception ex)
            {
                if (IniReadErrorCallback != null)
                {
                    if (dkc.Count > 0)
                        IniReadErrorCallback.Invoke(this, new IniReadErrorEventArgs(dkc, ex));
                    else
                        IniReadErrorCallback.Invoke(this, new IniReadErrorEventArgs(ex));
                }
            }//*/
            spli = null;
            sectionBuffer = null;
            lineBuffer = null;
            splitBuffer = null;
        }



        private void WriteToStream(System.IO.TextWriter theStream)
        {
            if (this.o_Sections.IsEmpty) return;
            IniSection _inisection; IniKeyValue _inikeyvalue;
            foreach (string key in this.o_Sections.Keys)
                if (this.o_Sections.TryGetValue(key, out _inisection))
                {
                    if (_inisection.IsComment)
                        theStream.WriteLine(";[" + key + "]");
                    else
                        theStream.WriteLine("[" + key + "]");
                    foreach (string valueName in _inisection.IniKeyValues.Keys)
                        if (_inisection.IniKeyValues.TryGetValue(valueName, out _inikeyvalue))
                        {
                            if (_inikeyvalue.IsComment)
                                theStream.WriteLine(";" + valueName + "=" + EscapeValue(_inikeyvalue.Value));
                            else
                                theStream.WriteLine(valueName + "=" + EscapeValue(_inikeyvalue.Value));
                        }
                }
            theStream.Flush();
        }

        private string UnescapeValue(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            if (str.IndexOf("\\n") > -1)
                str = str.Replace("\\n", "\n");
            return str;
        }

        private string EscapeValue(string str)
        {
            if (str.IndexOf('\n') > -1)
                str = str.Replace("\n", "\\n");
            if (str.IndexOf('\r') > -1)
                str = str.Replace("\r", string.Empty);
            return str;
        }
        #endregion
    }
}
