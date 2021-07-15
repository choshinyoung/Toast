using System;

namespace Toast.Exceptions
{
    public class CannotRemoveConverterException : Exception
    {
        public CannotRemoveConverterException() { }

        public CannotRemoveConverterException(string cvt) : base($"Cannot remove converter '{cvt}' because there is no such converter.") { }
    }
}
