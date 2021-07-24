using System;

namespace Toast.Exceptions
{
    public class InvalidCommandNodeException : Exception
    {
        public InvalidCommandNodeException() { }

        public InvalidCommandNodeException(string cmd, string node) : base($"A command node '{node}' cannot be used as a parameter of command '{cmd}'.") { }
    }
}
