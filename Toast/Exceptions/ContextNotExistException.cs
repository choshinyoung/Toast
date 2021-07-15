using System;

namespace Toast.Exceptions
{
    public class ContextNotExistException : Exception
    {
        public ContextNotExistException() : base("A command takes one ToastContext argument.") { }
    }
}
