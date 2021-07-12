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
        
        public ConverterAlreadyExistException(Type from, Type to) : base($"A converter that converts {from.Name} to {to.Name} already exists.") { }
    }
}
