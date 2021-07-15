using System;

namespace Toast.Exceptions
{
    public class FunctionParameterLengthException : Exception
    {
        public FunctionParameterLengthException() { }

        public FunctionParameterLengthException(string func, int given, int expected) : base($"Function '{func}' need {expected} parameters but {given} given.") { }
    }
}
