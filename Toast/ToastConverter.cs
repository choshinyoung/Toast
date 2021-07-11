using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Toast
{
    public class ToastConverter
    {
        public string Name { get; private init; }

        public MethodInfo Method { get; private init; }

        public object Target { get; private init; }

        public Type From { get; private init; }

        public Type To { get; private init; }

        public static ToastConverter Create<T1, T2>(string name, Func<T1, T2> method)
        {
            ToastConverter cvt = new()
            {
                Name = name,
                Method = method.Method,
                Target = method.Target,
                From = method.Method.GetParameters().First().ParameterType,
                To = method.Method.ReturnType,
            };

            return cvt;
        }
    }
}
