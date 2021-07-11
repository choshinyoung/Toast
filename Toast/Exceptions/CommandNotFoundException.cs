using System;

namespace Toast.Exceptions
{
    public class CommandNotFoundException : Exception
    {
        public CommandNotFoundException() { }

        public CommandNotFoundException(string cmd) : base($"Couldn't find a command '{cmd}'.") { }
    }
}
