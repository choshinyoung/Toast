using System;
using System.Linq;
using System.Reflection;

namespace Toast
{
    public class ToastConverter
    {
        public MethodInfo Method { get; private set; }

        internal object Target { get; private set; }

        public Type From { get; private set; }

        public Type To { get; private set; }

        public static ToastConverter Create<T, TResult>(Func<ToastContext, T, TResult> method)
        {
            ToastConverter cvt = new ToastConverter()
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
