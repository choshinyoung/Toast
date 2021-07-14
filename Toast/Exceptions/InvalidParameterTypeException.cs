using System;

namespace Toast.Exceptions
{
    public class InvalidParameterTypeException : Exception
    {
        public InvalidParameterTypeException() { }

        public InvalidParameterTypeException(object obj) : base($"Type {obj.GetType().Name} with value '{obj}' cannot be used as a parameter.") { }
    }
}
