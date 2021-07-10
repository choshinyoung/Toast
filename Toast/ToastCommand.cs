using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Toast
{
    public class ToastCommand
    {
        public string Name;

        public ParameterInfo[] Parameters;
        public ParameterInfo Return;

        public MethodInfo Method;

        private void Init(string name, MethodInfo method)
        {
            Parameters = method.GetParameters();
            Return = method.ReturnParameter;

            Name = name;
        }

        public ToastCommand(string name, Action method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Action<object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Action<object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Action<object, object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Action<object, object, object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Action<object, object, object, object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Action<object, object, object, object, object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Action<object, object, object, object, object, object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Action<object, object, object, object, object, object, object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Action<object, object, object, object, object, object, object, object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Action<object, object, object, object, object, object, object, object, object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Action<object, object, object, object, object, object, object, object, object, object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Action<object, object, object, object, object, object, object, object, object, object, object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Action<object, object, object, object, object, object, object, object, object, object, object, object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Action<object, object, object, object, object, object, object, object, object, object, object, object, object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Action<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Action<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Func<object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Func<object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Func<object, object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Func<object, object, object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Func<object, object, object, object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Func<object, object, object, object, object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Func<object, object, object, object, object, object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Func<object, object, object, object, object, object, object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Func<object, object, object, object, object, object, object, object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Func<object, object, object, object, object, object, object, object, object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Func<object, object, object, object, object, object, object, object, object, object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Func<object, object, object, object, object, object, object, object, object, object, object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Func<object, object, object, object, object, object, object, object, object, object, object, object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object> method)
        {
            Init(name, method.Method);
        }

        public ToastCommand(string name, Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object> method)
        {
            Init(name, method.Method);
        }
    }
}
