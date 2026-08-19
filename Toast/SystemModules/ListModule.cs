namespace Toast.SystemModules;

[ToastModule("list", "List functions and operators.")]
public class ListModule : IToastModule
{
    [ToastCommand(
        "to",
        "Generates a list containing an integer range from left to right (inclusive).",
        Precedence = 6,
        IsInfix = true
    )]
    public static ListValue To(NumberValue left, NumberValue right)
    {
        int l = (int)left.Value;
        int r = (int)right.Value;
        var list = new List<ToastValue>();
        for (int i = l; i <= r; i++)
        {
            list.Add(new NumberValue(i));
        }
        return new ListValue(list);
    }

    [ToastCommand(
        "in",
        "Checks if an element is contained in a list.",
        Precedence = 6,
        IsInfix = true
    )]
    public static BoolValue In(ToastValue left, ListValue right)
    {
        foreach (var item in right.Elements)
        {
            if (Equals(item, left))
                return new BoolValue(true);
        }
        return new BoolValue(false);
    }

    [ToastCommand(
        "#",
        "Accesses member by index from an ObjectValue.",
        Precedence = 14,
        IsInfix = true
    )]
    public static ToastValue IndexAccess(Context context, ToastValue left, NumberValue index)
    {
        if (left is ObjectValue objVal)
        {
            if (
                objVal.Context.GetBindings().TryGetValue("#", out var memberBinding)
                && memberBinding.Value is CommandValue indexCmd
            )
            {
                return indexCmd.Command.TargetDelegate(context, [index]);
            }
            throw new ToastException(
                new TypeError($"Type '{left.Type}' does not support '#' indexing.")
            );
        }
        throw new ToastException(new TypeError("Can only index ObjectValue types with '#'."));
    }

    private static ToastValue InvokeCallable(
        Context context,
        ToastValue callable,
        params ToastValue[] args
    )
    {
        if (callable is FunctionValue funcVal)
        {
            return funcVal.Execute([.. args]);
        }
        if (callable is CommandValue cmdVal)
        {
            return cmdVal.Command.TargetDelegate(context, args);
        }
        throw new ToastException(new TypeError("Target is not a callable function."));
    }

    [ToastCommand("filter", "Filters elements of a list using a predicate function.")]
    public static ListValue Filter(Context context, ListValue list, ToastValue predicate)
    {
        var result = new List<ToastValue>();
        foreach (var item in list.Elements)
        {
            var res = InvokeCallable(context, predicate, item);
            if (res is BoolValue b && b.Value)
            {
                result.Add(item);
            }
        }
        return new ListValue(result);
    }

    [ToastCommand("map", "Transforms each element of a list using a mapper function.")]
    public static ListValue Map(Context context, ListValue list, ToastValue mapper)
    {
        var result = new List<ToastValue>();
        foreach (var item in list.Elements)
        {
            result.Add(InvokeCallable(context, mapper, item));
        }
        return new ListValue(result);
    }

    [ToastCommand(
        "reduce",
        "Reduces elements in a list to a single value using an accumulator function."
    )]
    public static ToastValue Reduce(
        Context context,
        ListValue list,
        ToastValue initial,
        ToastValue reducer
    )
    {
        var acc = initial;
        foreach (var item in list.Elements)
        {
            acc = InvokeCallable(context, reducer, acc, item);
        }
        return acc;
    }

    [ToastCommand("sort", "Sorts elements in a list in ascending order.")]
    public static ListValue Sort(ListValue list)
    {
        var result = new List<ToastValue>(list.Elements);
        result.Sort(
            (a, b) =>
            {
                if (a is NumberValue na && b is NumberValue nb)
                    return na.Value.CompareTo(nb.Value);
                if (a is StringValue sa && b is StringValue sb)
                    return string.Compare(sa.Value, sb.Value, StringComparison.Ordinal);
                throw new ToastException(
                    new TypeError("Can only sort lists containing only numbers or only strings.")
                );
            }
        );
        return new ListValue(result);
    }

    [ToastCommand("sortAs", "Sorts elements in a list according to a key selector function.")]
    public static ListValue SortAs(Context context, ListValue list, ToastValue keySelector)
    {
        var result = new List<ToastValue>(list.Elements);
        result.Sort(
            (a, b) =>
            {
                var ka = InvokeCallable(context, keySelector, a);
                var kb = InvokeCallable(context, keySelector, b);

                if (ka is NumberValue na && kb is NumberValue nb)
                    return na.Value.CompareTo(nb.Value);
                if (ka is StringValue sa && kb is StringValue sb)
                    return string.Compare(sa.Value, sb.Value, StringComparison.Ordinal);
                throw new ToastException(
                    new TypeError("Sorted keys must be comparable numbers or strings.")
                );
            }
        );
        return new ListValue(result);
    }

    [ToastType("list", "List type")]
    public static class ListType
    {
        [ToastCommand("#", "Gets or references an element in a list by index.")]
        public static ToastValue ListIndex(Context context, ListValue list, NumberValue index)
        {
            int idx = (int)index.Value;
            if (idx < 0 || idx >= list.Elements.Count)
            {
                throw new ToastException(IndexError.OutOfRange(idx, list.Elements.Count, "list"));
            }

            if (context.Toaster.Executor.SuppressDereference)
            {
                return new ReferenceValue(new ListIndexAssignTarget(list, idx));
            }

            return list.Elements[idx];
        }

        [ToastCommand("indexOf", "Finds the first index of an element in a list.")]
        public static NumberValue IndexOf(ListValue list, ToastValue target)
        {
            for (int i = 0; i < list.Elements.Count; i++)
            {
                if (Equals(list.Elements[i], target))
                    return new NumberValue(i);
            }
            return new NumberValue(-1);
        }

        [ToastCommand("join", "Combines two lists into a single list.")]
        public static ListValue Join(ListValue list1, ListValue list2)
        {
            var result = list1.Elements.Concat(list2.Elements).ToList();
            return new ListValue(result);
        }

        [ToastCommand("shuffle", "Shuffles the elements in a list into random order.")]
        public static ListValue Shuffle(ListValue list)
        {
            var result = list.Elements.OrderBy(_ => System.Random.Shared.Next()).ToList();
            return new ListValue(result);
        }

        [ToastCommand("add", "Adds an element to the end of the list.")]
        public static ToastValue Add(ListValue list, ToastValue item)
        {
            list.Elements.Add(item);
            return NullValue.Instance;
        }

        [ToastCommand("removeAt", "Removes the element at the specified index of the list.")]
        public static ToastValue RemoveAt(ListValue list, NumberValue index)
        {
            int i = (int)index.Value;
            var removed = list.Elements[i];
            list.Elements.RemoveAt(i);
            return removed;
        }

        [ToastCommand("length", "Gets the number of elements in the list.")]
        public static NumberValue Length(ListValue list)
        {
            return new NumberValue(list.Elements.Count);
        }
    }
}
