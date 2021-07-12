using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toast.Exceptions
{
    class ParameterConvertException : Exception
    {
        public ParameterConvertException() { }

        public ParameterConvertException(Type from, Type to) : base($"Can't convert {from.Name} to {to.Name}.") { }
    }
}
