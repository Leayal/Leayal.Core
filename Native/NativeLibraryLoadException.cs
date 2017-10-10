using System;

namespace Leayal.Native
{
    public class NativeLibraryLoadException : Exception
    {
        public NativeLibraryLoadException() : base() { }
        public NativeLibraryLoadException(string message) : base(message) { }
        public NativeLibraryLoadException(string message, Exception exception) : base(message, exception) { }
    }
}
