using System;

namespace Toast.Exceptions
{
    public class InvalidCommandParameterException : Exception
    {
        public InvalidCommandParameterException() { }

        public InvalidCommandParameterException(string cmd) : base($"A command '{cmd}' cannot be used here.") { }
    }
}
