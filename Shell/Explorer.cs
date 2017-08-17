using System.IO;

namespace Leayal.Shell
{
    public static class Explorer
    {
        public static void ShowAndHighlightItem(string path)
        {
            ShowAndHighlightItem(path, true);
        }

        public static void ShowAndHighlightItem(string path, bool throwOnNotFound)
        {
            if (throwOnNotFound)
                if (!Directory.Exists(path) && !File.Exists(path))
                    throw new DirectoryNotFoundException($"'{path}' is not existed.");
            using (System.Diagnostics.Process.Start("explorer.exe", $"/select,{path}")) { }
        }

        public static void OpenFolder(string path)
        {
            OpenFolder(path, true);
        }

        public static void OpenFolder(string path, bool throwOnNotFound)
        {
            if (throwOnNotFound && !Directory.Exists(path))
                throw new DirectoryNotFoundException($"'{path}' is not existed.");
            using (System.Diagnostics.Process.Start(path)) { }
        }
    }
}
