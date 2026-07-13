namespace Toast.BuiltIns;

public static class Relational
{
    public static readonly Command Equal = Command.CreateOperator(
        "==",
        (Context context, ToastObject left, ToastObject right) =>
            new BoolValue(Equals(left, right)),
        precedence: 4
    );

    public static readonly Command NotEqual = Command.CreateOperator(
        "!=",
        (Context context, ToastObject left, ToastObject right) =>
            new BoolValue(!Equals(left, right)),
        precedence: 4
    );

    public static readonly Command LessThan = Command.CreateOperator(
        "<",
        (Context context, NumberValue left, NumberValue right) =>
            new BoolValue(left.Value < right.Value),
        precedence: 5
    );

    public static readonly Command GreaterThan = Command.CreateOperator(
        ">",
        (Context context, NumberValue left, NumberValue right) =>
            new BoolValue(left.Value > right.Value),
        precedence: 5
    );

    public static readonly Command LessThanOrEqual = Command.CreateOperator(
        "<=",
        (Context context, NumberValue left, NumberValue right) =>
            new BoolValue(left.Value <= right.Value),
        precedence: 5
    );

    public static readonly Command GreaterThanOrEqual = Command.CreateOperator(
        ">=",
        (Context context, NumberValue left, NumberValue right) =>
            new BoolValue(left.Value >= right.Value),
        precedence: 5
    );

    public static readonly Command Is = Command.CreateFunction(
        "is",
        (Context context, ToastObject left, AstNodeValue rightNode) =>
        {
            if (rightNode.Node is IdentifierNode right)
            {
                if (left is ObjectValue objVal && objVal.CustomType != null)
                {
                    return new BoolValue(
                        objVal.CustomType.Name.Equals(
                            right.Name,
                            StringComparison.OrdinalIgnoreCase
                        )
                    );
                }
                return new BoolValue(
                    left.Type.Name.Equals(right.Name, StringComparison.OrdinalIgnoreCase)
                );
            }
            throw new InvalidOperationException(
                "Right side of 'is' must be a type name (identifier)."
            );
        },
        precedence: 6,
        isInfix: true
    );

    public static readonly Command As = Command.CreateFunction(
        "as",
        (Context context, ToastObject leftVal, AstNodeValue targetNode) =>
        {
            var sourceType = leftVal.Type;

            ToastType targetType;
            if (targetNode.Node is TypeNode typeNode)
            {
                targetType = typeNode.Type;
            }
            else if (targetNode.Node is IdentifierNode idNode)
            {
                targetType = idNode.Name.ToLower() switch
                {
                    "string" => ToastType.String,
                    "integer" => ToastType.Number,
                    "float" => ToastType.Number,
                    "double" => ToastType.Number,
                    "number" => ToastType.Number,
                    "boolean" => ToastType.Boolean,
                    "list" => ToastType.List,
                    "object" => ToastType.Object,
                    _ => new ToastType(idNode.Name),
                };
            }
            else
            {
                throw new InvalidOperationException("Right side of 'as' must be a type.");
            }

            if (sourceType == targetType)
                return leftVal;

            if (
                context.Toaster.TryConvert(
                    leftVal,
                    sourceType,
                    targetType,
                    context,
                    out var converted
                )
            )
            {
                return converted;
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
