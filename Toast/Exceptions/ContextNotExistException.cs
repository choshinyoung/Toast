using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toast.Exceptions
{
    public class ContextNotExistException : Exception
    {
        public ContextNotExistException() : base("A command takes one ToastContext argument.") { }
    }
}
