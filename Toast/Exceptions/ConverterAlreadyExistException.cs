using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toast.Exceptions
{
    public class ConverterAlreadyExistException : Exception
    {
        public ConverterAlreadyExistException() { }
        
        public ConverterAlreadyExistException(string cvt) : base($"A converter named '{cvt}' is already created.") { }
    }
}
