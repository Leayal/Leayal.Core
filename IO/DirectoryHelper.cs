using System;
using System.IO;

namespace Leayal.IO
{
    public static class DirectoryHelper
    {
        public static bool IsFolderEmpty(this DirectoryInfo df)
        {
            return IsFolderEmpty(df, SearchOption.AllDirectories);
        }

        public static bool IsFolderEmpty(this DirectoryInfo df, SearchOption searchoption)
        {
            bool result = true;
            if (df.Exists)
                foreach (FileInfo str in df.EnumerateFiles("*", searchoption))
                {
                    result = false;
                    break;
                }
            return result;
        }

        public static bool IsFolderEmpty(string path)
        {
            return IsFolderEmpty(path, SearchOption.AllDirectories);
        }

        public static bool IsFolderEmpty(string path, SearchOption searchoption)
        {
            bool result = true;
            if (System.IO.Directory.Exists(path))
                foreach (string str in System.IO.Directory.EnumerateFiles(path, "*", searchoption))
                {
                    result = false;
                    break;
                }
            return result;
        }

        public static void EmptyFolder(string directoryPath)
        {
            foreach (string filename in System.IO.Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
                System.IO.File.Delete(filename);
            foreach (string dirname in System.IO.Directory.EnumerateDirectories(directoryPath, "*", SearchOption.AllDirectories))
                System.IO.Directory.Delete(dirname, true);
        }
        public static void EmptyFolder(this DirectoryInfo directory)
        {
            EmptyFolder(directory.FullName);
        }
    }
}
