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

        public static OccurenceResult Occurence(this string str, params string[] strings)
        {
            return Occurence(str, StringComparison.Ordinal, strings);
        }

        public static OccurenceResult Occurence(this string str, StringComparison comparison, params string[] strings)
        {
            Dictionary<string, int> result = new Dictionary<string, int>(GetStringComparer(comparison));
            for (int i = 0; i < strings.Length; i++)
                result[strings[i]] = 0;
            int offset;
            int searchIndex;
            for (int i = 0; i < strings.Length; i++)
            {
                offset = 0;
                searchIndex = str.IndexOf(strings[i], offset, comparison);
                while (searchIndex > -1)
                {
                    result[strings[i]]++;
                    offset = searchIndex + 1;
                    searchIndex = str.IndexOf(strings[i], offset, comparison);
                }
            }
            return new OccurenceResult(result);
        }

        public static StringComparer GetStringComparer(StringComparison comparison)
        {
            switch (comparison)
            {
                case StringComparison.CurrentCulture:
                    return StringComparer.CurrentCulture;
                case StringComparison.CurrentCultureIgnoreCase:
                    return StringComparer.CurrentCultureIgnoreCase;
                case StringComparison.InvariantCulture:
                    return StringComparer.InvariantCulture;
                case StringComparison.InvariantCultureIgnoreCase:
                    return StringComparer.InvariantCultureIgnoreCase;
                case StringComparison.OrdinalIgnoreCase:
                    return StringComparer.OrdinalIgnoreCase;
                default:
                    return StringComparer.Ordinal;
            }
        }

        public class OccurenceResult
        {
            private Dictionary<string, int> dict;
            internal OccurenceResult(StringComparison comparison) : this(new Dictionary<string, int>(GetStringComparer(comparison))) { }

            internal OccurenceResult(Dictionary<string, int> dictionary)
            {
                this.dict = dictionary;
            }

            public int this[string key] => this.dict[key];
        }

        public static int Occurence(this char[] chars, char findChar)
        {
            int result = 0;
            for (int i = 0; i < chars.Length; i++)
                if (chars[i] == findChar)
                    result++;
            return result;
        }

        public static int Occurence(this string str, params char[] chars)
        {
            int result = 0;
            int offset;
            int searchIndex;
            for (int i = 0; i < chars.Length; i++)
            {
                offset = 0;
                searchIndex = str.IndexOf(chars[i], offset);
                while (searchIndex > -1)
                {
                    result++;
                    offset = searchIndex + 1;
                    searchIndex = str.IndexOf(chars[i], offset);
                }
            }
            return result;
        }

        /// <summary>
        /// Returns a new string in which all occurrences of a specified Unicode character in this instance are replaced with the specified Unicode character.
        /// </summary>
        /// <param name="str">The source string.</param>
        /// <param name="oldChars">The list of Unicode characters to be replaced.</param>
        /// <param name="newChar">The Unicode character to replace all occurrences of oldChar.</param>
        /// <returns>A string that is equivalent to this instance except that all instances of oldChar are replaced with newChar.</returns>
        public static string Replace(this string str, char[] oldChars, char newChar)
        {
            string result = CreateNullString(str.Length);
            unsafe
            {
                fixed (char* c = str, compare = oldChars, output = result)
                {
                    bool has;
                    for (int i = 0; i < str.Length; i++)
                    {
                        has = false;
                        for (int check = 0; check < oldChars.Length; check++)
                        {
                            if (c[i] == compare[check])
                            {
                                has = true;
                                output[i] = newChar;
                                break;
                            }
                        }
                        if (!has)
                            output[i] = c[i];
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns a new string in which all occurrences of a specified Unicode character in this instance are removed.
        /// </summary>
        /// <param name="str">The source string.</param>
        /// <param name="chars">The list of characters to be removed.</param>
        /// <returns>A string that is equivalent to this instance except that all instances of chars are removed.</returns>
        public static string Remove(this string str, params char[] chars)
        {
            return Remove(str, 0, chars);
        }

        /// <summary>
        /// Returns a new string in which all occurrences starting from given index of a specified Unicode character in this instance are removed.
        /// </summary>
        /// <param name="str">The source string.</param>
        /// <param name="startIndex">Zero-based position to begin deletion.</param>
        /// <param name="chars">The list of characters to be removed.</param>
        /// <returns>A string that is equivalent to this instance except that all instances of chars are removed.</returns>
        public static string Remove(this string str, int startIndex, params char[] chars)
        {
            string result = CreateNullString(str.Length - str.Occurence(chars));
            int index = 0;
            unsafe
            {
                fixed (char* c = str, compare = chars, output = result)
                {
                    bool has;
                    for (int i = startIndex; i < str.Length; i++)
                    {
                        has = false;
                        for (int check = 0; check < chars.Length; check++)
                            if (c[i] == compare[check])
                            {
                                has = true;
                                break;
                            }
                        if (!has)
                        {
                            output[index] = c[i];
                            index++;
                        }
                    }
                }
            }
            return result;
        }

        public static string[] Chop(this string str, int chopSize)
        {
            string[] result;
            int affix = (str.Length % chopSize);
            bool appendone = (affix != 0);
            if (appendone)
                result = new string[(str.Length / chopSize) + 1];
            else
                result = new string[str.Length / chopSize];
            int index = 0, childIndex = 0;
            Collections.FixedArray<char> buffer = new Collections.FixedArray<char>(chopSize);
            unsafe
            {
                fixed (char* c = str)
                {
                    for (int i = 0; i < str.Length; i++)
                    {
                        buffer.Push(c[i]);
                        childIndex++;
                        if (childIndex == chopSize)
                        {
                            childIndex = 0;
                            result[index] = new string(buffer.GetArray());
                            index++;
                        }
                    }
                }
            }
            if (appendone)
            {
                for (int i = affix; i < chopSize; i++)
                    buffer.PushItemOut();
                result[index] = CreateNewString(buffer.GetArray(), false);
            }
            return result;
        }

        public static string CreateNullString(int length)
        {
            return new string('\0', length);
        }

        public static string CreateNewString(char[] chars, bool allowNull)
        {
            if (allowNull)
                return new string(chars);
            else
            {
                /*
                StringBuilder sb = new StringBuilder(Occurence(chars, '\0'));
                for (int i = 0; i < chars.Length; i++)
                    if (chars[i] != '\0')
                        sb.Append(chars[i]);
                return sb.ToString();
                */
                // Use StringBuilder is better than this though.
                string result = CreateNullString(chars.Length - Occurence(chars, '\0'));
                int index = 0;
                unsafe
                {
                    fixed (char* c = result, source = chars)
                    {
                        for (int i = 0; i < chars.Length; i++)
                            if (chars[i] != '\0')
                            {
                                c[index] = source[i];
                                index++;
                            }
                    }
                }
                return result;
            }
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
