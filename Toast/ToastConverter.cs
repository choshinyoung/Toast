using System;
using System.Linq;
using System.Reflection;

namespace Toast
{
    public class ToastConverter
    {
        public MethodInfo Method { get; private init; }

        public object Target { get; private init; }

        public Type From { get; private init; }

        public Type To { get; private init; }

        public static ToastConverter Create<T1, T2>(Func<T1, T2> method)
        {
            ToastConverter cvt = new()
            {
                Method = method.Method,
                Target = method.Target,
                From = method.Method.GetParameters().First().ParameterType,
                To = method.Method.ReturnType,
            };

            return cvt;
        }
    }
}
