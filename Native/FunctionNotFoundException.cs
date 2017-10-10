using System;

namespace Leayal.Native
{
    public class FunctionNotFoundException : Exception
    {
        public FunctionNotFoundException() : base() { }
        public FunctionNotFoundException(string message) : base(message) { }
        public FunctionNotFoundException(string message, Exception exception) : base(message, exception) { }
    }
}
