using System.Linq.Expressions;

namespace Toast;

public class Command
{
    public string Name { get; }
    public Func<Context, ToastObject[], ToastObject> TargetDelegate { get; }
    public int Precedence { get; }
    public bool IsRightAssociative { get; }
    public bool IsPrefix { get; }
    public bool IsInfix { get; }
    public IReadOnlyList<ToastType> ParameterTypes { get; }
    public IReadOnlyList<bool> IsParameterLazy { get; }
    public int ParameterCount => ParameterTypes.Count;

    public Command(
        string name,
        Delegate targetDelegate,
        int precedence = 0,
        bool isRightAssociative = false,
        bool isPrefix = false,
        bool isInfix = false,
        IReadOnlyList<ToastType>? parameterTypes = null,
        IReadOnlyList<bool>? isParameterLazy = null
    )
    {
        Name = name;
        Precedence = precedence;
        IsRightAssociative = isRightAssociative;
        IsPrefix = isPrefix;
        IsInfix = isInfix;

        var method = targetDelegate.Method;
        var parameters = method.GetParameters();

        TargetDelegate = CompileDelegate(targetDelegate);

        if (parameterTypes != null && isParameterLazy != null)
        {
            ParameterTypes = parameterTypes;
            IsParameterLazy = isParameterLazy;
        }
        else if (
            parameters.Length > 0
            && typeof(Context).IsAssignableFrom(parameters[0].ParameterType)
        )
        {
            var types = new List<ToastType>();
            var isLazy = new List<bool>();
            for (int i = 1; i < parameters.Length; i++)
            {
                var paramType = parameters[i].ParameterType;
                bool lazy = paramType == typeof(AstNodeValue);
                isLazy.Add(lazy);
                types.Add(MapToToastType(paramType));
            }
            ParameterTypes = types;
            IsParameterLazy = isLazy;
        }
        else
        {
            throw new InvalidOperationException(
                $"Command delegate for '{name}' must have Context as its first parameter."
            );
        }
    }

    private static Func<Context, ToastObject[], ToastObject> CompileDelegate(Delegate del)
    {
        var method = del.Method;
        var target = del.Target;
        var parameters = method.GetParameters();

        if (
            del is Func<Context, ToastObject[], ToastObject> fastFunc
            && parameters.Length == 2
            && parameters[1].ParameterType == typeof(ToastObject[])
        )
        {
            return fastFunc;
        }

        var contextParam = Expression.Parameter(typeof(Context), "context");
        var argsParam = Expression.Parameter(typeof(ToastObject[]), "args");

        var callArgs = new List<Expression> { contextParam };

        for (int i = 1; i < parameters.Length; i++)
        {
            var paramType = parameters[i].ParameterType;
            var arrayIndex = Expression.ArrayIndex(argsParam, Expression.Constant(i - 1));
            var cast = Expression.Convert(arrayIndex, paramType);
            callArgs.Add(cast);
        }

        Expression call;
        if (target != null)
        {
            call = Expression.Call(Expression.Constant(target), method, callArgs);
        }
        else
        {
            call = Expression.Call(method, callArgs);
        }

        if (method.ReturnType == typeof(void))
        {
            var block = Expression.Block(
                call,
                Expression.Constant(NullValue.Instance, typeof(ToastObject))
            );
            return Expression
                .Lambda<Func<Context, ToastObject[], ToastObject>>(block, contextParam, argsParam)
                .Compile();
        }
        else
        {
            var castResult = Expression.Convert(call, typeof(ToastObject));
            return Expression
                .Lambda<Func<Context, ToastObject[], ToastObject>>(
                    castResult,
                    contextParam,
                    argsParam
                )
                .Compile();
        }
    }

    public static Command CreateOperator(
        string name,
        Delegate targetDelegate,
        int precedence,
        bool isRightAssociative = false,
        bool isPrefix = false
    )
    {
        return new Command(
            name,
            targetDelegate,
            precedence: precedence,
            isRightAssociative: isRightAssociative,
            isPrefix: isPrefix,
            isInfix: !isPrefix
        );
    }

    public static Command CreateFunction(
        string name,
        Delegate targetDelegate,
        int precedence = 0,
        bool isRightAssociative = false,
        bool isPrefix = false,
        bool isInfix = false
    )
    {
        return new Command(
            name,
            targetDelegate,
            precedence: precedence,
            isRightAssociative: isRightAssociative,
            isPrefix: isPrefix,
            isInfix: isInfix
        );
    }

    private static ToastType MapToToastType(Type type)
    {
        if (type == typeof(StringValue))
            return ToastType.String;
        if (type == typeof(NumberValue))
            return ToastType.Number;
        if (type == typeof(BoolValue))
            return ToastType.Boolean;
        if (type == typeof(ListValue))
            return ToastType.List;
        if (type == typeof(ObjectValue))
            return ToastType.Object;
        if (type == typeof(FunctionValue) || type == typeof(CommandValue))
            return ToastType.Function;
        if (type == typeof(IdentifierValue))
            return ToastType.Identifier;
        if (type == typeof(NullValue))
            return ToastType.Null;
        if (type == typeof(AstNodeValue))
            return ToastType.Any;
        if (type == typeof(ReferenceValue))
            return ToastType.Reference;
        if (type == typeof(ToastObject))
            return ToastType.Any;

        if (type == typeof(string))
            return ToastType.String;
        if (type == typeof(int) || type == typeof(double) || type == typeof(float))
            return ToastType.Number;
        if (type == typeof(bool))
            return ToastType.Boolean;
        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            return ToastType.List;
        return ToastType.Any;
    }
}
