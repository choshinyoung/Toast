using System.Linq.Expressions;

namespace Toast;

public class Command
{
    public string Name { get; }
    public Func<Context, object?[], object?> TargetDelegate { get; }
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
        bool isInfix = false
    )
    {
        Name = name;
        TargetDelegate = CompileDelegate(targetDelegate);
        Precedence = precedence;
        IsRightAssociative = isRightAssociative;
        IsPrefix = isPrefix;
        IsInfix = isInfix;

        var method = targetDelegate.Method;
        var parameters = method.GetParameters();

        if (parameters.Length > 0 && typeof(Context).IsAssignableFrom(parameters[0].ParameterType))
        {
            var types = new List<ToastType>();
            var isLazy = new List<bool>();
            for (int i = 1; i < parameters.Length; i++)
            {
                var paramType = parameters[i].ParameterType;
                bool lazy = typeof(Node).IsAssignableFrom(paramType);
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

    private static Func<Context, object?[], object?> CompileDelegate(Delegate del)
    {
        var method = del.Method;
        var target = del.Target;
        var parameters = method.GetParameters();

        if (
            del is Func<Context, object?[], object?> fastFunc
            && parameters.Length == 2
            && parameters[1].ParameterType == typeof(object?[])
        )
        {
            return fastFunc;
        }

        var contextParam = Expression.Parameter(typeof(Context), "context");
        var argsParam = Expression.Parameter(typeof(object?[]), "args");

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
            var block = Expression.Block(call, Expression.Constant(null, typeof(object)));
            return Expression
                .Lambda<Func<Context, object?[], object?>>(block, contextParam, argsParam)
                .Compile();
        }
        else
        {
            var castResult = Expression.Convert(call, typeof(object));
            return Expression
                .Lambda<Func<Context, object?[], object?>>(castResult, contextParam, argsParam)
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
        if (type == typeof(string))
            return ToastType.String;
        if (type == typeof(int))
            return ToastType.Integer;
        if (type == typeof(double) || type == typeof(float))
            return ToastType.Float;
        if (type == typeof(bool))
            return ToastType.Boolean;
        if (type == typeof(IdentifierNode))
            return ToastType.Identifier;
        if (type == typeof(FunctionValue) || type == typeof(Command))
            return ToastType.Function;
        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            return ToastType.List;
        return ToastType.Any;
    }
}
