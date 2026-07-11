namespace Toast;

public class Command
{
    public string Name { get; }
    public Delegate TargetDelegate { get; }
    public int Precedence { get; }
    public bool IsRightAssociative { get; }
    public bool IsPrefix { get; }
    public bool IsInfix { get; }
    public IReadOnlyList<ToastType>? ParameterTypes { get; }

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
        TargetDelegate = targetDelegate;
        Precedence = precedence;
        IsRightAssociative = isRightAssociative;
        IsPrefix = isPrefix;
        IsInfix = isInfix;

        var method = targetDelegate.Method;
        var parameters = method.GetParameters();

        if (parameters.Length > 0 && typeof(Context).IsAssignableFrom(parameters[0].ParameterType))
        {
            // 지연 평가 시그니처 감지: (Context, List<Node>, Toast)
            if (
                parameters.Length == 3
                && typeof(List<Node>).IsAssignableFrom(parameters[1].ParameterType)
                && typeof(Toaster).IsAssignableFrom(parameters[2].ParameterType)
            )
            {
                ParameterTypes = null;
            }
            else
            {
                var types = new List<ToastType>();
                for (int i = 1; i < parameters.Length; i++)
                {
                    types.Add(MapToToastType(parameters[i].ParameterType));
                }
                ParameterTypes = types;
            }
        }
        else
        {
            throw new InvalidOperationException(
                $"Command delegate for '{name}' must have Context as its first parameter."
            );
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
        if (type == typeof(MemoryAddress))
            return ToastType.Identifier;
        if (type == typeof(FunctionValue) || type == typeof(Command))
            return ToastType.Function;
        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            return ToastType.List;
        return ToastType.Any;
    }
}
