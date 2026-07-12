namespace Toast.BuiltIns;

public static class Relational
{
    public static readonly Command Equal = Command.CreateOperator(
        "==",
        (Context context, object? left, object? right) => Equals(left, right),
        precedence: 4
    );

    public static readonly Command NotEqual = Command.CreateOperator(
        "!=",
        (Context context, object? left, object? right) => !Equals(left, right),
        precedence: 4
    );

    public static readonly Command LessThan = Command.CreateOperator(
        "<",
        (Context context, double left, double right) => left < right,
        precedence: 5
    );

    public static readonly Command GreaterThan = Command.CreateOperator(
        ">",
        (Context context, double left, double right) => left > right,
        precedence: 5
    );

    public static readonly Command LessThanOrEqual = Command.CreateOperator(
        "<=",
        (Context context, double left, double right) => left <= right,
        precedence: 5
    );

    public static readonly Command GreaterThanOrEqual = Command.CreateOperator(
        ">=",
        (Context context, double left, double right) => left >= right,
        precedence: 5
    );

    public static readonly Command Is = Command.CreateFunction(
        "is",
        (Context context, object? left, object? right) =>
        {
            var typeStr = right is IdentifierNode typeId ? typeId.Name : right?.ToString();
            return left?.GetType().Name.ToLower() == typeStr?.ToLower();
        },
        precedence: 6,
        isInfix: true
    );

    public static readonly Command As = Command.CreateFunction(
        "as",
        (Context context, object? leftVal, Node targetNode) =>
        {
            var sourceType = Executor.GetToastType(leftVal);

            ToastType targetType;
            if (targetNode is TypeNode typeNode)
            {
                targetType = typeNode.Type;
            }
            else if (targetNode is IdentifierNode idNode)
            {
                targetType = idNode.Name.ToLower() switch
                {
                    "string" => ToastType.String,
                    "integer" => ToastType.Integer,
                    "float" => ToastType.Float,
                    "boolean" => ToastType.Boolean,
                    "list" => ToastType.List,
                    _ => throw new InvalidOperationException(
                        $"Invalid cast target type: {idNode.Name}"
                    ),
                };
            }
            else
            {
                throw new InvalidOperationException("Right side of 'as' must be a type.");
            }

            if (sourceType == targetType)
                return leftVal;

            if (context.Toaster.Converters.TryGetValue((sourceType, targetType), out var converter))
            {
                return converter.ConvertFunc(context, leftVal);
            }

            throw new InvalidOperationException(
                $"No converter registered from {sourceType} to {targetType}."
            );
        },
        precedence: 6,
        isInfix: true
    );

    public static void Register(Toaster toast)
    {
        toast.RegisterCommand(Equal);
        toast.RegisterCommand(NotEqual);
        toast.RegisterCommand(LessThan);
        toast.RegisterCommand(GreaterThan);
        toast.RegisterCommand(LessThanOrEqual);
        toast.RegisterCommand(GreaterThanOrEqual);
        toast.RegisterCommand(Is);
        toast.RegisterCommand(As);
    }
}
