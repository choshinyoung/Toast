using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toast.Exceptions;
using Toast.Nodes;

namespace Toast
{
    internal class ToastExecutor
    {
        public static object Execute(Toaster toaster, INode node)
        {
            switch (node)
            {
                case CommandNode c:
                    List<object> parameters = new();

                    foreach (INode n in c.Parameters)
                    {
                        parameters.Add(Execute(toaster, n));
                    }

                    return toaster.ExecuteCommand(c.Command, parameters.ToArray());
                case VariableNode v:
                    return v;
                case FunctionNode f:
                    return f;
                case ListNode l:
                    List<object> list = new();

                    foreach (INode n in l.Value)
                    {
                        list.Add(Execute(toaster, n));
                    }

                    return list.ToArray();
                case ValueNode v:
                    return v.Value;
                default:
                    throw new InvalidParameterTypeException();
            }
        }
    }
}
