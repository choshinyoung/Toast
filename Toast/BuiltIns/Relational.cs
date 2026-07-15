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
        (Context context, ToastObject left, ToastObject right) =>
        {
            if (right is NullValue)
            {
                return new BoolValue(left is NullValue);
            }

            if (right is not TypeValue typeVal)
            {
                throw new InvalidOperationException(
                    "Right side of 'is' must evaluate to a type or null."
                );
            }

            var targetType = typeVal.TargetType;
            var declaredMembers = typeVal.DeclaredMembers;

            if (declaredMembers.Count == 0)
            {
                return new BoolValue(
                    left.Type.Name.Equals(targetType.Name, StringComparison.OrdinalIgnoreCase)
                );
            }

            if (left is ObjectValue targetObj)
            {
                var objBindings = targetObj.Context.GetBindings();
                foreach (var reqMember in declaredMembers)
                {
                    if (!objBindings.ContainsKey(reqMember))
                    {
                        return new BoolValue(false);
                    }
                }
                return new BoolValue(true);
            }

            return new BoolValue(false);
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
    }
}
