namespace Toast.BuiltIns;

public static class Relational
{
    public static readonly Command Equal = Command.CreateOperator(
        "==",
        (Context context, ToastValue left, ToastValue right) => new BoolValue(Equals(left, right)),
        precedence: 4
    );

    public static readonly Command NotEqual = Command.CreateOperator(
        "!=",
        (Context context, ToastValue left, ToastValue right) => new BoolValue(!Equals(left, right)),
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
        (Context context, ToastValue left, ToastValue right) =>
        {
            if (right is NullValue)
            {
                return new BoolValue(left is NullValue);
            }

            TypeValue typeVal;
            if (right is TypeValue tv)
            {
                typeVal = tv;
            }
            else if (right is StringValue || right is IdentifierValue)
            {
                var typeName = right.ToString();
                if (typeName == "null")
                {
                    return new BoolValue(left is NullValue);
                }

                if (
                    context.HasVariable(typeName)
                    && context.GetValue(typeName) is TypeValue resolvedTv
                )
                {
                    typeVal = resolvedTv;
                }
                else
                {
                    var targetType = ToastType.FromName(typeName);
                    typeVal = new TypeValue(targetType, null);
                }
            }
            else
            {
                throw new ToastException(
                    new TypeError(
                        "Right side of 'is' must evaluate to a type, identifier, or string."
                    )
                );
            }

            return new BoolValue(CheckIsCompatible(context, left, typeVal));
        },
        precedence: 6,
        isInfix: true
    );

    private static bool CheckIsCompatible(Context context, ToastValue left, TypeValue targetTypeVal)
    {
        var targetType = targetTypeVal.TargetType;
        if (targetType == ToastType.Any)
        {
            return true;
        }

        if (targetType == ToastType.Null)
        {
            return left is NullValue;
        }

        if (left is NullValue)
        {
            return false;
        }

        bool isTypeCompatible = Toaster.IsCompatible(left.Type, targetType, context);
        if (isTypeCompatible)
        {
            return true;
        }

        if (
            left is ObjectValue objVal
            && left is not ErrorValue
            && targetTypeVal.DeclaredMembers.Count > 0
        )
        {
            var bindings = objVal.Context.GetBindings();
            foreach (var reqMember in targetTypeVal.DeclaredMembers)
            {
                if (!bindings.TryGetValue(reqMember, out var binding))
                {
                    return false;
                }

                if (targetTypeVal.MemberTypes.TryGetValue(reqMember, out var expectedMemberType))
                {
                    var actualMemberVal = binding.Value;
                    if (!Toaster.IsCompatible(actualMemberVal.Type, expectedMemberType, context))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        return false;
    }

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
