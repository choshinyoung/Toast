namespace Toast.SystemModules;

public partial class DefaultModule
{
    [ToastCommand(
        "==",
        Precedence = 4,
        Description = "Equality operator, checks if two values are equal."
    )]
    public static BoolValue Equal(ToastValue left, ToastValue right) => new(Equals(left, right));

    [ToastCommand(
        "!=",
        Precedence = 4,
        Description = "Inequality operator, checks if two values are not equal."
    )]
    public static BoolValue NotEqual(ToastValue left, ToastValue right) =>
        new(!Equals(left, right));

    [ToastCommand("<", Precedence = 5, Description = "Less than comparison operator.")]
    public static BoolValue LessThan(NumberValue left, NumberValue right) =>
        new(left.Value < right.Value);

    [ToastCommand(">", Precedence = 5, Description = "Greater than comparison operator.")]
    public static BoolValue GreaterThan(NumberValue left, NumberValue right) =>
        new(left.Value > right.Value);

    [ToastCommand("<=", Precedence = 5, Description = "Less than or equal comparison operator.")]
    public static BoolValue LessThanOrEqual(NumberValue left, NumberValue right) =>
        new(left.Value <= right.Value);

    [ToastCommand(">=", Precedence = 5, Description = "Greater than or equal comparison operator.")]
    public static BoolValue GreaterThanOrEqual(NumberValue left, NumberValue right) =>
        new(left.Value >= right.Value);

    [ToastCommand(
        "is",
        Precedence = 6,
        IsInfix = true,
        Description = "Checks whether a value is compatible with the specified type."
    )]
    public static BoolValue Is(Context context, ToastValue left, ToastValue right)
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

            if (context.HasVariable(typeName) && context.GetValue(typeName) is TypeValue resolvedTv)
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
                new TypeError("Right side of 'is' must evaluate to a type, identifier, or string.")
            );
        }

        return new BoolValue(CheckIsCompatible(context, left, typeVal));
    }

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
}
