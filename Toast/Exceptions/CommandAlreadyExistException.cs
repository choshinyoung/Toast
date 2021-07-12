using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toast.Exceptions
{
    public class CommandAlreadyExistException : Exception
    {
        public CommandAlreadyExistException() { }
        
        public CommandAlreadyExistException(string cmd) : base($"A command named '{cmd}' is already created.") { }
    }
}
