using System;

namespace Leayal.Native
{
    public class FreeLibraryException : Exception
    {
        public FreeLibraryException() : base() { }
        public FreeLibraryException(string message) : base(message) { }
        public FreeLibraryException(string message, Exception exception) : base(message, exception) { }
    }
}
