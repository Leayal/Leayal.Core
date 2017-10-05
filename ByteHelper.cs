using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Leayal
{
    public static class ByteHelper
    {
        internal static readonly char[] Spacing = { ' ', ControlChars.Tab };

        public static string ToHexString(this byte[] bytes)
        {
            string result = null;
            using (BytesConverter bc = new BytesConverter())
                result = bc.ToHexString(bytes);
            return result;
        }

        /// <summary>
        /// Alternative of <see cref="Array.Resize{T}(ref T[], int)"/>. Create a new <see cref="byte"/> array that holding the existing bytes while appending null bytes to fit the fixed length.
        /// </summary>
        /// <param name="source">Source byte array to copy from</param>
        /// <param name="length">The new fixed length.</param>
        /// <returns>Byte array with new fixed length that holding the old data</returns>
        public static byte[] FixLength(byte[] source, int length)
        {
            byte[] result = new byte[length];
            unsafe
            {
                fixed (byte* src = source, dest = result)
                {
                    for (int i = 0; i < source.Length; i++)
                        dest[i] = src[i];
                }
            }
            return result;
        }

        public static byte[] FromHexString(string hexString)
        {
            hexString = hexString.Remove(Spacing);
            if ((hexString.Length % 2) > 0)
                throw new ArgumentException("Invalid hex string");
            string[] strs = hexString.Chop(2);
            byte[] bytes = new byte[strs.Length];
            byte parsing;
            for (int i = 0; i < bytes.Length; i++)
                if (byte.TryParse(strs[i], System.Globalization.NumberStyles.HexNumber, null, out parsing))
                    bytes[i] = parsing;
                else
                    throw new InvalidHexString($"'{strs[i]}' is not a hex.");
            return bytes;
        }
    }

    public class InvalidHexString : Exception
    {
        internal InvalidHexString() : base() { }
        internal InvalidHexString(string message) : base(message) { }
        internal InvalidHexString(string message, Exception innerException) : base(message, innerException) { }
    }
}
