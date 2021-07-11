using System;

namespace Toast.Exceptions
{
    class InvalidParameterTypeException : Exception
    {
        public InvalidParameterTypeException() { }

        public InvalidParameterTypeException(object obj) : base($"Type {obj.GetType().Name} with value '{obj}' can't be used as a parameter.") { }
    }
}
