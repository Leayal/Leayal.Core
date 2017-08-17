using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Leayal
{
    public static class ByteHelper
    {
        public static string ToHexString(this byte[] bytes)
        {
            string result = null;
            using (BytesConverter bc = new BytesConverter())
                result = bc.ToHexString(bytes);
            return result;
        }
    }
}
