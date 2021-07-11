using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
            var parseResult = ToastParser.ParseRaw(line);
            if (parseResult[0] is not Command)
            {
                throw new InvalidCommandLineException(line);
            }

            ToastCommand cmd = GetCommand(((Command)parseResult[0]).GetValue());

            int index = 0;
            object[] parameters = ExecuteParameter(parseResult, cmd.Parameters.Length, ref index);

            if (++index != parseResult.Length)
            {
                throw new Exception($"Argument is not correct. {index} != {parseResult.Length}");
            }

            return ExecuteCommand(cmd, parameters);
        }
        
        private object[] ExecuteParameter(Element[] elements, int count, ref int index)
        {
            List<object> parameters = new();

            while (parameters.Count < count)
            {
                index++;

                if (elements.Length <= index)
                {
                    throw new Exception();
                }

                Element ele = elements[index];

                switch (ele)
                {
                    case Command c:
                        ToastCommand cmd = GetCommand(c.GetValue());

                        parameters.Add(ExecuteCommand(cmd, ExecuteParameter(elements, cmd.Parameters.Length, ref index)));

                        break;
                    case Number n:
                        parameters.Add(n.GetValue());

                        break;
                    case Text t:
                        parameters.Add(t.GetValue());

                        break;
                    case Group:
                        throw new NotImplementedException();

                        break;
                    default:
                        throw new Exception("Unknown type.");
                }
            }

            return parameters.ToArray();
        }

        private ToastCommand GetCommand(string name)
        {
            ToastCommand cmd = Commands.Find(c => c.Name == name);
            if (cmd is null)
            {
                throw new Exception($"Couldn't find a command '{cmd}'.");
            }

            return cmd;
        }

        private static object ExecuteCommand(ToastCommand cmd, object[] parameters)
        {
            object result = cmd.Method.Invoke(cmd.Target, parameters);

            return result;
        }
    }
}
