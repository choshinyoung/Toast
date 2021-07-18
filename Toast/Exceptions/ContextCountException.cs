using System;

namespace Toast.Exceptions
{
    public class ContextCountException : Exception
    {
        public ContextCountException() : base("A command takes one ToastContext argument.") { }
    }
}
