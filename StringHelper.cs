using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Leayal
{
    public static class StringHelper
    {
        internal readonly static SortedDictionary<char, int> charint = createcharint();
        private unsafe static SortedDictionary<char, int> createcharint()
        {
            SortedDictionary<char, int> result = new SortedDictionary<char, int>();
            for (int i = 0; i < 10; i++)
                result.Add(i.ToString()[0], i);
            return result;
        }

        public static bool IsEqual(this string s, string str)
        {
            return IsEqual(s, str, false);
        }

        public static bool IsEqual(this string s, string str, bool ignoreCase)
        {
            if (s == null)
            {
                if (str == null)
                    return true;
                else
                    return false;
            }
            else
            {
                if (str == null)
                    return false;
                else
                {
                    if (s.Length == str.Length)
                        return (string.Compare(s, str, ignoreCase) == 0);
                    else
                        return false;
                }
            }
        }

        public unsafe static string[] ToStringArray(this string str)
        {
            string[] result = new string[str.Length];
            fixed (char* c = str)
                for (int i = 0; i < str.Length; i++)
                    result[i] = new string(c[i], 1);
            return result;
        }

        public unsafe static int ToInt(this string str)
        {
            return ToInt(str, true);
        }

        public static int ToInt(this string str, bool thrownOnError)
        {
            if (thrownOnError)
            {
                int y = 0, pow = 0;
                unsafe
                {
                    fixed (char* c = str)
                        for (int i = str.Length - 1; i >= 0; i--)
                        {
                            if (pow > 0)
                                y += (int)Math.Pow(10, pow) * charint[c[i]];
                            else
                                y += charint[c[i]];
                            pow += 1;
                        }
                }
                return y;
            }
            else
            {
                int y = 0, pow = 0;
                unsafe
                {
                    fixed (char* c = str)
                        for (int i = str.Length - 1; i >= 0; i--)
                            if (charint.ContainsKey(c[i]))
                            {
                                if (pow > 0)
                                    y += (int)Math.Pow(10, pow) * charint[c[i]];
                                else
                                    y += charint[c[i]];
                                pow += 1;
                            }
                }
                return y;
            }
        }
    }
}
