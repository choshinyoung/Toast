using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toast.Elements;
using Toast.Exceptions;

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
            var parseResult = ParseLine(line);
            string name = parseResult.name;
            object[] parameters = parseResult.parameters;

            ToastCommand cmd = Commands.Find(c => c.Name == name);
            if (cmd is null)
            {
                throw new Exception($"Couldn't find a command '{cmd}'.");
            }

            object result = cmd.Method.Invoke(cmd.Target, parameters);

            return result;
        }

        public static (string name, object[] parameters) ParseLine(string line)
        {
            Element[] elements = ToastParser.ParseRaw(line);

            if (elements[0] is not Command)
            {
                throw new InvalidCommandLineException(line);
            }

            string name = ((Command)elements[0]).GetValue();
            object[] parameters = elements[1..].Select(e => e.GetValue()).ToArray();

            return (name, parameters);
        }
    }
}
