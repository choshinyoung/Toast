using System;

namespace Toast.Exceptions
{
    public class CannotRemoveCommandException : Exception
    {
        public CannotRemoveCommandException() { }

        public CannotRemoveCommandException(string cmd) : base($"Cannot remove command '{cmd}' because there is no such command.") { }
    }
}
