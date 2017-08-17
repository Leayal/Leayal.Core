using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Leayal.IO
{
    public static class PathHelper
    {
        public static string Combine(IEnumerable<string> paths)
        {
            string result = PathTrim(paths.First());
            foreach (string item in paths.Skip(1))
                result = System.IO.Path.Combine(result, PathTrim(item));
            return result;
        }

        public static string Combine(params string[] paths)
        {
            for (int i = 0; i < paths.Length; i++)
                paths[i] = PathTrim(paths[i]);
            return System.IO.Path.Combine(paths);
        }

        public static string Combine(string path1, string path2)
        { return System.IO.Path.Combine(path1.PathTrim(), path2.PathTrim()); }

        public static string PathTrim(this string url)
        {
            url = url.TrimStart('/', ' ');
            url = url.TrimEnd('\\', '/', ' ');
            return url;
        }
    }
}
