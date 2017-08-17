using System;

namespace Leayal
{
    public static class NumberHelper
    {
        public static int Parse(string str)
        {
            return StringHelper.ToInt(str);
        }

        /// <summary>
        /// Try parse a int from a string. Return True if the string is fully parsed.
        /// </summary>
        /// <param name="str">String. The string which will be parsed.</param>
        /// <param name="result">Out int.</param>
        /// <returns>Bool. True if the string is fully parsed.</returns>
        public unsafe static bool TryParse(string str, out int result)
        {
            result = 0;
            if (string.IsNullOrWhiteSpace(str)) return false;
            int pow = 0;
            bool returnResult = true;
            fixed (char* c = str)
                for (int i = str.Length - 1; i >= 0; i--)
                    if (StringHelper.charint.ContainsKey(c[i]))
                    {
                        if (pow > 0)
                            result += (int)Math.Pow(10, pow) * StringHelper.charint[c[i]];
                        else
                            result += StringHelper.charint[c[i]];
                        pow += 1;
                    }
                    else
                        returnResult = false;
            return returnResult;
        }
    }
}
