using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Leayal.Native
{
    public static class NativeLibraryManager
    {
        private static Dictionary<string, NativeLibrary> cache_moduleName;
        public static NativeLibrary LoadLibrary(string filename)
        {
            filename = Path.GetFullPath(filename);
            if (!File.Exists(filename))
                throw new FileNotFoundException("The library file is not existed.", filename);

            IntPtr pDll = NativeMethods.LoadLibrary(filename);
            if (pDll == IntPtr.Zero)
                throw new NativeLibraryLoadException("Failed to load the library.");
            else
            {
                if (cache_moduleName == null)
                    cache_moduleName = new Dictionary<string, NativeLibrary>(StringComparer.OrdinalIgnoreCase);
                else
                {
                    if (cache_moduleName.ContainsKey(filename))
                        return cache_moduleName[filename];
                }
                NativeLibrary result = new NativeLibrary(filename, pDll);
                cache_moduleName.Add(filename, result);
                return result;
            }
        }

        internal static void RemoveFromCache(NativeLibrary lib)
        {
            if (cache_moduleName.ContainsKey(lib.Filename))
                cache_moduleName.Remove(lib.Filename);
        }

        public static void FreeAllLoadedLibraries()
        {
            if (cache_moduleName != null && (cache_moduleName.Count > 0))
            {
                foreach (NativeLibrary lib in cache_moduleName.Values)
                    lib.Free();
                cache_moduleName.Clear();
            }
        }
    }
}
