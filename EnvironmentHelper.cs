using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Leayal
{
    /// <summary>
    /// This class was a mistake.
    /// </summary>
    public static class EnvironmentHelper
    {
        /*
        private static MyTotalPATHManager _path = new MyTotalPATHManager();
        public static IPATH PATH => _path;

        public interface IPATH
        {
            PathManager this[EnvironmentVariableTarget target] { get; }
        }

        class MyTotalPATHManager : IPATH
        {
            private Dictionary<EnvironmentVariableTarget, PathManager> dict;
            internal MyTotalPATHManager()
            {
                this.dict = new Dictionary<EnvironmentVariableTarget, PathManager>(3);
                foreach (EnvironmentVariableTarget target in Enum.GetValues(typeof(EnvironmentVariableTarget)))
                    this.dict.Add(target, new PathManager(target));
            }

            public PathManager this[EnvironmentVariableTarget scope] => this.dict[scope];
        }
        //*/

        public class PathManager : IEnumerable<string>
        {
            EnvironmentVariableTarget scope;
            List<string> innerVariable;
            StringBuilder sb;
            bool cansave;

            internal PathManager(EnvironmentVariableTarget target)
            {
                this.cansave = false;
                this.scope = target;
            }
            
            public void Refresh()
            {
                string pathEnvir = Environment.GetEnvironmentVariable("PATH", this.scope);
                string[] splitted = pathEnvir.Split(';');
                if (this.innerVariable == null || this.innerVariable.Capacity < splitted.Length)
                    this.innerVariable = new List<string>(splitted);
                else
                {
                    this.innerVariable.Clear();
                    this.innerVariable.AddRange(splitted);
                }
                pathEnvir = null;
                splitted = null;
            }

            public IEnumerator<string> GetEnumerator()
            {
                this.Refresh();
                return this.innerVariable.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                this.Refresh();
                return this.innerVariable.GetEnumerator();
            }
            
            public int Count => this.innerVariable.Count;

            public void Remove(params string[] value)
            {
                this.Refresh();

                bool found = false;
                int huh;
                for (int i = 0; i < value.Length; i++)
                {
                    huh = this.InnerIndexOf(value[i]);
                    if (huh != -1)
                    {
                        this.innerVariable.RemoveAt(huh);
                        found = true;
                    }
                }

                if (!found)
                    return;

                this.cansave = true;
                this.Save();
            }

            public override string ToString()
            {
                if (this.sb == null)
                    this.sb = new StringBuilder();
                else
                    this.sb.Clear();
                for (int i = 0; i < this.innerVariable.Count; i++)
                {
                    if (i == 0)
                        this.sb.Append(this.innerVariable[i]);
                    else
                        this.sb.AppendFormat(";{0}", this.innerVariable[i]);
                }
                return this.sb.ToString();
            }

            public bool Contains(string value)
            {
                return (this.IndexOf(value) != -1);
            }

            public int IndexOf(string value)
            {
                this.Refresh();
                return this.InnerIndexOf(value);
            }

            private int InnerIndexOf(string value)
            {
                for (int i = 0; i < this.innerVariable.Count; i++)
                {
                    if (this.innerVariable[i].IsEqual(value, true))
                        return i;
                    else
                    {
                        if (Environment.ExpandEnvironmentVariables(this.innerVariable[i]).IsEqual(value, true))
                            return i;
                    }
                }
                return -1;
            }

            public void Add(params string[] value)
            {
                this.Refresh();
                int huh; bool newthing = false;
                for (int i = 0; i < value.Length; i++)
                {
                    huh = this.InnerIndexOf(value[i]);
                    if (huh == -1)
                    {
                        this.innerVariable.Add(value[i]);
                        newthing = true;
                    }
                }

                if (!newthing)
                    return;

                this.cansave = true;
                this.Save();
            }

            private void Save()
            {
                if (!this.cansave) return;
                this.cansave = false;

                Environment.SetEnvironmentVariable("PATH", this.ToString(), this.scope);
            }
        }
    }
}
