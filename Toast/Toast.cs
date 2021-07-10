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

        public object Execute(string line)
        {
            string[] parameters = line.Split();
            string name = parameters[0];
            parameters = parameters[1..];

            ToastCommand cmd = Commands.Find(c => c.Name == name);
            if (cmd is null)
            {
                throw new CommandNotFoundException(name);
            }

            object result = cmd.Method.Invoke(cmd.Target, parameters);

            return result;
        }

        public class CommandNotFoundException : Exception
        {
            public CommandNotFoundException()
            {

            }

            public CommandNotFoundException(string cmd) : base($"Couldn't find a command '{cmd}'.")
            {
                
            }
        }
    }
}
