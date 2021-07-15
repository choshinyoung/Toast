using System;

namespace Toast.Exceptions
{
    public class CommandAlreadyExistException : Exception
    {
        public CommandAlreadyExistException() { }
        
        public CommandAlreadyExistException(string cmd) : base($"A command named '{cmd}' is already created.") { }
    }
}
