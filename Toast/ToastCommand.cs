using System;
using System.Linq;
using System.Reflection;
using Toast.Exceptions;

namespace Toast
{
    public class ToastCommand
    {
        public string Name { get; private init; }

        public MethodInfo Method { get; private init; }

        internal object Target { get; private init; }

        public Type[] Parameters { get; private init; }

        public Type Return { get; private init; }

        public int NamePosition { get; private init; }

        public int Priority { get; private init; }
        
        private static ToastCommand Create(string name, MethodInfo method, object target, int priority)
        {
            var parameters = method.GetParameters().ToList();

            if (parameters.Count(p => p.ParameterType.IsAssignableTo(typeof(ToastContext))) != 1)
            {
                throw new ContextCountException();
            }

            int contextIndex = parameters.FindIndex(p => p.ParameterType.IsAssignableTo(typeof(ToastContext)));
            parameters.RemoveAt(contextIndex);

            ToastCommand cmd = new()
            {
                Name = name,
                Method = method,
                Target = target,
                Parameters = parameters.Select(p => p.ParameterType).ToArray(),
                Return = method.ReturnType,
                NamePosition = contextIndex,
                Priority = priority,
            };

            return cmd;
        }

        public static ToastCommand CreateAction<T1>(string name, Action<T1> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateAction<T1, T2>(string name, Action<T1, T2> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateAction<T1, T2, T3>(string name, Action<T1, T2, T3> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateAction<T1, T2, T3, T4>(string name, Action<T1, T2, T4, T4> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateAction<T1, T2, T3, T4, T5>(string name, Action<T1, T2, T3, T4, T5> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateAction<T1, T2, T3, T4, T5, T6>(string name, Action<T1, T2, T3, T4, T5, T6> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateAction<T1, T2, T3, T4, T5, T6, T7>(string name, Action<T1, T2, T3, T4, T5, T6, T7> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateAction<T1, T2, T3, T4, T5, T6, T7, T8>(string name, Action<T1, T2, T3, T4, T5, T6, T8> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string name, Action<T1, T2, T3, T4, T5, T6, T8, T9> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string name, Action<T1, T2, T3, T4, T5, T6, T8, T9, T10> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateFunc<T1, TResult>(string name, Func<T1, TResult> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateFunc<T1, T2, TResult>(string name, Func<T1, T2, TResult> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateFunc<T1, T2, T3, TResult>(string name, Func<T1, T2, T3, TResult> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateFunc<T1, T2, T3, T4, TResult>(string name, Func<T1, T2, T4, T4, TResult> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateFunc<T1, T2, T3, T4, T5, TResult>(string name, Func<T1, T2, T3, T4, T5, TResult> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateFunc<T1, T2, T3, T4, T5, T6, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, TResult> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateFunc<T1, T2, T3, T4, T5, T6, T7, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T7, TResult> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T8, TResult> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T8, T9, TResult> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T8, T9, T10, TResult> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }

        public static ToastCommand CreateFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> method, int priority = 0)
        {
            return Create(name, method.Method, method.Target, priority);
        }
    }
}
