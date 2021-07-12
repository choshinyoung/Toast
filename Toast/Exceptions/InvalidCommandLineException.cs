using System;

namespace Toast.Exceptions
{
    public class InvalidCommandLineException : Exception
    {
        public InvalidCommandLineException() { }

        public InvalidCommandLineException(string line) : base($"\"{line}\" is not a valid command line.") { }
    }
}
