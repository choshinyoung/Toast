using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toast
{
    public class Toast
    {
        public List<ToastCommand> Commands = new();

        public Action<ToastCommand> AddCommand;

        public Toast()
        {
            AddCommand = Commands.Add;
        }
    }
}
