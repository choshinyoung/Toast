using System;
using System.Linq;
using System.Reflection;

namespace Toast
{
    public class ToastConverter
    {
        public MethodInfo Method { get; private init; }

        internal object Target { get; private init; }

        public Type From { get; private init; }

        public Type To { get; private init; }

        public static ToastConverter Create<T, TResult>(Func<ToastContext, T, TResult> method)
        {
            ToastConverter cvt = new()
            {
                Method = method.Method,
                Target = method.Target,
                From = method.Method.GetParameters()[1].ParameterType,
                To = method.Method.ReturnType,
            };

            return cvt;
        }
    }
}
