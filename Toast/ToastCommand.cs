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
        public string Name { get; private init; }

        public MethodInfo Method { get; private init; }

        public object Target { get; private init; }

        public Type[] Parameters { get; private init; }

        public Type Return { get; private init; }

        private static ToastCommand Create(string name, MethodInfo method, object target)
        {
            ToastCommand cmd = new()
            {
                Name = name,
                Method = method,
                Target = target,
                Parameters = method.GetParameters().Select(p => p.ParameterType).ToArray(),
                Return = method.ReturnType,
            };

            return cmd;
        }

        public static ToastCommand Create(string name, Action method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1>(string name, Action<T1> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2>(string name, Action<T1, T2> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, T3>(string name, Action<T1, T2, T3> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, T3, T4>(string name, Action<T1, T2, T4, T4> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, T3, T4, T5>(string name, Action<T1, T2, T3, T4, T5> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, T3, T4, T5, T6>(string name, Action<T1, T2, T3, T4, T5, T6> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, T3, T4, T5, T6, T7>(string name, Action<T1, T2, T3, T4, T5, T6, T7> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, T3, T4, T5, T6, T7, T8>(string name, Action<T1, T2, T3, T4, T5, T6, T8> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string name, Action<T1, T2, T3, T4, T5, T6, T8, T9> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string name, Action<T1, T2, T3, T4, T5, T6, T8, T9, T10> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<TResult>(string name, Func<TResult> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, TResult>(string name, Func<T1, TResult> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, TResult>(string name, Func<T1, T2, TResult> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, T3, TResult>(string name, Func<T1, T2, T3, TResult> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, T3, T4, TResult>(string name, Func<T1, T2, T4, T4, TResult> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, T3, T4, T5, TResult>(string name, Func<T1, T2, T3, T4, T5, TResult> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, T3, T4, T5, T6, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, TResult> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, T3, T4, T5, T6, T7, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T7, TResult> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T8, TResult> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T8, T9, TResult> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T8, T9, T10, TResult> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> method)
        {
            return Create(name, method.Method, method.Target);
        }

        public static ToastCommand Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> method)
        {
            return Create(name, method.Method, method.Target);
        }
    }
}
