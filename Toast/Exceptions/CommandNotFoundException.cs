using System;

namespace Toast.Exceptions
{
    public class CommandNotFoundException : Exception
    {
        public CommandNotFoundException() { }

        public CommandNotFoundException(string cmd) : base($"Cannot find command '{cmd}'.") { }
    }
}
