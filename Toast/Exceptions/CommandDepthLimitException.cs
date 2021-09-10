using System;

namespace Toast.Exceptions
{
    public class CommandDepthLimitException : Exception
    {
        public CommandDepthLimitException() : base($"Too many commands executed recursive and reached its limit.") { }
    }
}
