using System;

namespace Toast.Exceptions
{
    public class ParameterConvertException : Exception
    {
        public ParameterConvertException() { }

        public ParameterConvertException(Type from, Type to) : base($"Cannot convert {from.Name} to {to.Name}.") { }
    }
}
