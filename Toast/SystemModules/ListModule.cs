namespace Toast.SystemModules;

public class ListModule : IToastModule
{
    public string Name => "list";
    public string Description => "List functions.";

    public static readonly Command To = Command.CreateFunction(
        "to",
        (Context context, NumberValue left, NumberValue right) =>
        {
            int l = (int)left.Value;
            int r = (int)right.Value;
            var list = new List<ToastValue>();
            for (int i = l; i <= r; i++)
            {
                list.Add(new NumberValue(i));
            }
            return new ListValue(list);
        },
        precedence: 6,
        isInfix: true,
        description: "Generates a list containing an integer range from left to right (inclusive).",
        returnType: ToastType.List
    );
    public static readonly Command In = Command.CreateFunction(
        "in",
        (Context context, ToastValue left, ListValue right) =>
        {
            foreach (var item in right.Elements)
            {
                if (Equals(item, left))
                    return new BoolValue(true);
            }
            return new BoolValue(false);
        },
        precedence: 6,
        isInfix: true,
        description: "Checks if an element is contained in a list.",
        returnType: ToastType.Boolean
    );
    public static readonly Command IndexAccess = Command.CreateOperator(
        "#",
        (Context context, ToastValue left, NumberValue index) =>
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
        },
        precedence: 10,
        description: "Index operator for collections."
    );
    private static readonly Command ListIndex = Command.CreateFunction(
        "#",
        (Context context, ListValue list, NumberValue index) =>
        {
            int idx = (int)index.Value;
            if (idx < 0 || idx >= list.Elements.Count)
            {
                throw new IndexOutOfRangeException(
                    $"Index {idx} is out of range for list of length {list.Elements.Count}."
                );
            }

            if (context.Toaster.Executor.SuppressDereference)
            {
                return new ReferenceValue(new ListIndexAssignTarget(list, idx));
            }

            return list.Elements[idx];
        },
        description: "Retrieves the element at the specified index in the list."
    );
    public static readonly Command IndexOf = Command.CreateFunction(
        "indexOf",
        (Context context, ListValue list, ToastValue item) =>
        {
            return new NumberValue(list.Elements.IndexOf(item));
        },
        description: "Returns the zero-based index of the first occurrence of an item in a list.",
        returnType: ToastType.Number
    );
    public static readonly Command Filter = Command.CreateFunction(
        "filter",
        (Context context, ListValue list, FunctionValue predicate) =>
        {
            var result = new List<ToastValue>();
            foreach (var item in list.Elements)
            {
                var res = predicate.Execute([item]);
                if (res is BoolValue b && b.Value)
                {
                    result.Add(item);
                }
            }
            return new ListValue(result);
        },
        description: "Filters elements in a list based on a predicate function.",
        returnType: ToastType.List
    );
    public static readonly Command Map = Command.CreateFunction(
        "map",
        (Context context, ListValue list, FunctionValue mapper) =>
        {
            var result = new List<ToastValue>();
            foreach (var item in list.Elements)
            {
                result.Add(mapper.Execute([item]));
            }
            return new ListValue(result);
        },
        description: "Transforms each element in a list by applying a mapper function.",
        returnType: ToastType.List
    );
    public static readonly Command Reduce = Command.CreateFunction(
        "reduce",
        (Context context, ListValue list, ToastValue initial, FunctionValue reducer) =>
        {
            var acc = initial;
            foreach (var item in list.Elements)
            {
                acc = reducer.Execute([acc, item]);
            }
            return acc;
        },
        description: "Reduces a list of values to a single accumulated value using a reducer function."
    );
    public static readonly Command Join = Command.CreateFunction(
        "combine",
        (Context context, ListValue list1, ListValue list2) =>
        {
            var result = list1.Elements.Concat(list2.Elements).ToList();
            return new ListValue(result);
        },
        description: "Combines two lists into a single list.",
        returnType: ToastType.List
    );
    public static readonly Command Sort = Command.CreateFunction(
        "sort",
        (Context context, ListValue list) =>
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
                        new TypeError(
                            "Can only sort lists containing only numbers or only strings."
                        )
                    );
                }
            );
            return new ListValue(result);
        },
        description: "Sorts elements in a list in ascending order.",
        returnType: ToastType.List
    );
    public static readonly Command SortAs = Command.CreateFunction(
        "sortAs",
        (Context context, ListValue list, FunctionValue keySelector) =>
        {
            var result = new List<ToastValue>(list.Elements);
            result.Sort(
                (a, b) =>
                {
                    var ka = keySelector.Execute([a]);
                    var kb = keySelector.Execute([b]);

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
        },
        description: "Sorts elements in a list according to a key selector function.",
        returnType: ToastType.List
    );
    public static readonly Command Shuffle = Command.CreateFunction(
        "shuffle",
        (Context context, ListValue list) =>
        {
            var random = new Random();
            var result = list.Elements.OrderBy(x => random.Next()).ToList();
            return new ListValue(result);
        },
        description: "Shuffles the elements in a list into random order.",
        returnType: ToastType.List
    );
    public static readonly Command Add = Command.CreateFunction(
        "add",
        (Context context, ListValue list, ToastValue item) =>
        {
            list.Elements.Add(item);
            return NullValue.Instance;
        },
        description: "Adds an element to the end of the list.",
        returnType: ToastType.Null
    );
    public static readonly Command RemoveAt = Command.CreateFunction(
        "removeAt",
        (Context context, ListValue list, NumberValue index) =>
        {
            int i = (int)index.Value;
            var removed = list.Elements[i];
            list.Elements.RemoveAt(i);
            return removed;
        },
        description: "Removes the element at the specified index of the list."
    );
    public static readonly Command Length = Command.CreateFunction(
        "length",
        (Context context, ListValue list) =>
        {
            return new NumberValue(list.Elements.Count);
        },
        description: "Gets the number of elements in the list.",
        returnType: ToastType.Number
    );

    public static void Register(Toaster toast)
    {
        toast.RegisterCommand(To);
        toast.RegisterCommand(In);
        toast.RegisterCommand(IndexAccess);
        toast.RegisterCommand(Filter);
        toast.RegisterCommand(Map);
        toast.RegisterCommand(Reduce);

        toast.RegisterTypeMember(ToastType.List, "#", new CommandValue(ListIndex));
        toast.RegisterTypeMember(ToastType.List, "add", new CommandValue(Add));
        toast.RegisterTypeMember(ToastType.List, "removeAt", new CommandValue(RemoveAt));
        toast.RegisterTypeMember(ToastType.List, "length", new CommandValue(Length));
        toast.RegisterTypeMember(ToastType.List, "indexOf", new CommandValue(IndexOf));
        toast.RegisterTypeMember(ToastType.List, "join", new CommandValue(Join));
        toast.RegisterTypeMember(ToastType.List, "sort", new CommandValue(Sort));
        toast.RegisterTypeMember(ToastType.List, "sortAs", new CommandValue(SortAs));
        toast.RegisterTypeMember(ToastType.List, "shuffle", new CommandValue(Shuffle));
    }

    public void Load(Toaster toaster, Context callerContext)
    {
        Register(toaster);
    }
}
