using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Leayal.Native
{
    public class NativeLibrary : IDisposable
    {
        private IntPtr hModule;
        private Dictionary<string, Delegate> cache_FunctionName;

        public string Filename { get; }

        internal NativeLibrary(string filename, IntPtr pDll)
        {
            this.hModule = pDll;
            this.Filename = filename;
            this.cache_FunctionName = new Dictionary<string, Delegate>();
        }

        private bool _disposed;
        public void Dispose()
        {
            if (this._disposed) return;
            this._disposed = true;

            if (!NativeMethods.FreeLibrary(this.hModule))
                throw new FreeLibraryException("An unknown error has occurred while freeing this library. May be it is still in use.");
            else
                NativeLibraryManager.RemoveFromCache(this);
        }

        public Delegate GetFunction<T>(string functionName)
        {
            if (this._disposed)
                throw new ObjectDisposedException("NativeLibrary");

            if (this.cache_FunctionName.ContainsKey(functionName))
                return this.cache_FunctionName[functionName];

            IntPtr pAddressOfFunctionToCall = NativeMethods.GetProcAddress(this.hModule, functionName);
            if (pAddressOfFunctionToCall == IntPtr.Zero)
                throw new FunctionNotFoundException($"The function '{functionName}' is not found.");
            Delegate result = Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(T));

            this.cache_FunctionName.Add(functionName, result);

            return result;
        }

        /// <summary>
        /// This method is equal to <see cref="Dispose"/>
        /// </summary>
        public void Free()
        {
            this.Dispose();
        }
    }
}
