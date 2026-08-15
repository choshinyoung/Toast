using System.Linq.Expressions;

namespace Toast;

public record CommandParameter(string Name, ToastType Type, bool IsLazy);

public class Command
{
    public string Name { get; }
    public Func<Context, ToastValue[], ToastValue> TargetDelegate { get; }
    public int Precedence { get; }
    public bool IsRightAssociative { get; }
    public bool IsPrefix { get; }
    public bool IsInfix { get; }
    public bool DeclaresMember { get; }
    public string Description { get; init; } = "";
    public ToastType ReturnType { get; init; } = ToastType.Any;
    public IReadOnlyList<CommandParameter> Parameters { get; }
    public int ParameterCount => Parameters.Count;

    public Command(
        string name,
        Delegate targetDelegate,
        int precedence = 0,
        bool isRightAssociative = false,
        bool isPrefix = false,
        bool isInfix = false,
        IReadOnlyList<ToastType>? parameterTypes = null,
        IReadOnlyList<bool>? isParameterLazy = null,
        bool declaresMember = false,
        string? description = null,
        ToastType? returnType = null
    )
    {
        Name = name;
        Precedence = precedence;
        IsRightAssociative = isRightAssociative;
        IsPrefix = isPrefix;
        IsInfix = isInfix;
        DeclaresMember = declaresMember;

        var method = targetDelegate.Method;
        var methodParams = method.GetParameters();

        Description = description ?? "";

        if (returnType != null)
        {
            ReturnType = returnType;
        }
        else if (method.ReturnType == typeof(void) || method.ReturnType == typeof(NullValue))
        {
            ReturnType = ToastType.Null;
        }
        else
        {
            ReturnType = ToastType.TryFromClrType(method.ReturnType) ?? ToastType.Any;
        }

        TargetDelegate = CompileDelegate(targetDelegate);

        var list = new List<CommandParameter>();
        if (parameterTypes != null)
        {
            for (int i = 0; i < parameterTypes.Count; i++)
            {
                var pName =
                    (i + 1 < methodParams.Length)
                        ? (methodParams[i + 1].Name ?? $"arg{i + 1}")
                        : $"arg{i + 1}";
                var pType = parameterTypes[i];
                var pLazy =
                    isParameterLazy != null && i < isParameterLazy.Count && isParameterLazy[i];
                list.Add(new CommandParameter(pName, pType, pLazy));
            }
        }
        else if (
            methodParams.Length > 0
            && typeof(Context).IsAssignableFrom(methodParams[0].ParameterType)
        )
        {
            for (int i = 1; i < methodParams.Length; i++)
            {
                var param = methodParams[i];
                var pName = param.Name ?? $"arg{i}";
                var pType = ToastType.FromClrType(param.ParameterType);
                var pLazy = param.ParameterType == typeof(AstNodeValue);
                list.Add(new CommandParameter(pName, pType, pLazy));
            }
        }
        else
        {
            throw new InvalidOperationException(
                $"Command delegate for '{name}' must have Context as its first parameter."
            );
        }
        Parameters = list;
    }

    public string GetSignature()
    {
        if (IsInfix && Parameters.Count == 2)
        {
            return $"({Parameters[0].Name}: {Parameters[0].Type.Name}) {Name} ({Parameters[1].Name}: {Parameters[1].Type.Name}): {ReturnType.Name}";
        }
        if (IsPrefix && Parameters.Count == 1)
        {
            return $"{Name}({Parameters[0].Name}: {Parameters[0].Type.Name}): {ReturnType.Name}";
        }

        var paramStrs = Parameters.Select(p => $"{p.Name}: {p.Type.Name}");
        return $"{Name}({string.Join(", ", paramStrs)}): {ReturnType.Name}";
    }

    private static Func<Context, ToastValue[], ToastValue> CompileDelegate(Delegate del)
    {
        var method = del.Method;
        var target = del.Target;
        var parameters = method.GetParameters();

        if (
            del is Func<Context, ToastValue[], ToastValue> fastFunc
            && parameters.Length == 2
            && parameters[1].ParameterType == typeof(ToastValue[])
        )
        {
            return fastFunc;
        }

        var contextParam = Expression.Parameter(typeof(Context), "context");
        var argsParam = Expression.Parameter(typeof(ToastValue[]), "args");

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
                Expression.Constant(NullValue.Instance, typeof(ToastValue))
            );
            return Expression
                .Lambda<Func<Context, ToastValue[], ToastValue>>(block, contextParam, argsParam)
                .Compile();
        }
        else
        {
            var castResult = Expression.Convert(call, typeof(ToastValue));
            return Expression
                .Lambda<Func<Context, ToastValue[], ToastValue>>(
                    castResult,
                    contextParam,
                    argsParam
                )
                .Compile();
        }
    }

    public static Command CreateFunction(
        string name,
        Delegate targetDelegate,
        int precedence = 0,
        bool isRightAssociative = false,
        bool isPrefix = false,
        bool isInfix = false,
        IReadOnlyList<ToastType>? parameterTypes = null,
        IReadOnlyList<bool>? isParameterLazy = null,
        bool declaresMember = false,
        string? description = null,
        ToastType? returnType = null
    )
    {
        return new Command(
            name,
            targetDelegate,
            precedence: precedence,
            isRightAssociative: isRightAssociative,
            isPrefix: isPrefix,
            isInfix: isInfix,
            parameterTypes: parameterTypes,
            isParameterLazy: isParameterLazy,
            declaresMember: declaresMember,
            description: description,
            returnType: returnType
        );
    }

    public static Command CreateOperator(
        string name,
        Delegate targetDelegate,
        int precedence,
        bool isInfix = false,
        bool isRightAssociative = false,
        bool isPrefix = false,
        IReadOnlyList<ToastType>? parameterTypes = null,
        IReadOnlyList<bool>? isParameterLazy = null,
        string? description = null,
        ToastType? returnType = null
    )
    {
        return new Command(
            name,
            targetDelegate,
            precedence: precedence,
            isRightAssociative: isRightAssociative,
            isPrefix: isPrefix,
            isInfix: isInfix,
            parameterTypes: parameterTypes,
            isParameterLazy: isParameterLazy,
            description: description,
            returnType: returnType
        );
    }
}
