namespace Toast.BuiltIns;

public static class List
{
    public static readonly Command RangeTo = Command.CreateFunction(
        "to",
        (Context context, NumberValue left, NumberValue right) =>
        {
            int l = (int)left.Value;
            int r = (int)right.Value;
            var list = new List<ToastObject>();
            for (int i = l; i <= r; i++)
            {
                list.Add(new NumberValue(i));
            }
            return new ListValue(list);
        },
        precedence: 6,
        isInfix: true
    );

    public static readonly Command ListIn = Command.CreateFunction(
        "in",
        (Context context, ToastObject left, ListValue right) =>
        {
            foreach (var item in right.Elements)
            {
                if (Equals(item, left))
                    return new BoolValue(true);
            }
            return new BoolValue(false);
        },
        precedence: 6,
        isInfix: true
    );

    public static readonly Command IndexAccess = Command.CreateOperator(
        "#",
        (Context context, ToastObject list, NumberValue index) =>
        {
            int idx = (int)index.Value;
            if (list is StringValue str)
            {
                return new StringValue(str.Value[idx].ToString());
            }
            if (list is ListValue listVal)
            {
                return listVal.Elements[idx];
            }
            throw new InvalidOperationException("Can only index strings and lists.");
        },
        precedence: 10
    );

    public static readonly Command Member = Command.CreateFunction(
        "member",
        (Context context, NumberValue index, ListValue list) =>
        {
            return list.Elements[(int)index.Value];
        }
    );

    public static readonly Command Len = Command.CreateFunction(
        "len",
        (Context context, ToastObject list) =>
        {
            if (list is StringValue str)
                return new NumberValue(str.Value.Length);
            if (list is ListValue listVal)
                return new NumberValue(listVal.Elements.Count);
            throw new InvalidOperationException("Length is only defined on strings and lists.");
        }
    );

    public static readonly Command IndexOf = Command.CreateFunction(
        "indexOf",
        (Context context, ListValue list, ToastObject item) =>
        {
            return new NumberValue(list.Elements.IndexOf(item));
        }
    );

    public static readonly Command Filter = Command.CreateFunction(
        "filter",
        (Context context, ListValue list, FunctionValue predicate) =>
        {
            var result = new List<ToastObject>();
            foreach (var item in list.Elements)
            {
                var res = predicate.Execute([item]);
                if (res is BoolValue b && b.Value)
                {
                    result.Add(item);
                }
            }
            return new ListValue(result);
        }
    );

    public static readonly Command Map = Command.CreateFunction(
        "map",
        (Context context, ListValue list, FunctionValue mapper) =>
        {
            var result = new List<ToastObject>();
            foreach (var item in list.Elements)
            {
                result.Add(mapper.Execute([item]));
            }
            return new ListValue(result);
        }
    );

    public static readonly Command Combine = Command.CreateFunction(
        "combine",
        (Context context, ListValue list1, ListValue list2) =>
        {
            var result = list1.Elements.Concat(list2.Elements).ToList();
            return new ListValue(result);
        }
    );

    public static readonly Command Append = Command.CreateFunction(
        "append",
        (Context context, ListValue list, ToastObject item) =>
        {
            var result = new List<ToastObject>(list.Elements) { item };
            return new ListValue(result);
        }
    );

    public static readonly Command Remove = Command.CreateFunction(
        "remove",
        (Context context, ListValue list, ToastObject item) =>
        {
            var result = new List<ToastObject>(list.Elements);
            result.Remove(item);
            return new ListValue(result);
        }
    );

    public static readonly Command Sort = Command.CreateFunction(
        "sort",
        (Context context, ListValue list) =>
        {
            var result = new List<ToastObject>(list.Elements);
            result.Sort(
                (a, b) =>
                {
                    if (a is NumberValue na && b is NumberValue nb)
                        return na.Value.CompareTo(nb.Value);
                    if (a is StringValue sa && b is StringValue sb)
                        return string.Compare(sa.Value, sb.Value, StringComparison.Ordinal);
                    throw new InvalidOperationException(
                        "Can only sort lists containing only numbers or only strings."
                    );
                }
            );
            return new ListValue(result);
        }
    );

    public static readonly Command SortAs = Command.CreateFunction(
        "sortAs",
        (Context context, ListValue list, FunctionValue keySelector) =>
        {
            var result = new List<ToastObject>(list.Elements);
            result.Sort(
                (a, b) =>
                {
                    var ka = keySelector.Execute([a]);
                    var kb = keySelector.Execute([b]);

                    if (ka is NumberValue na && kb is NumberValue nb)
                        return na.Value.CompareTo(nb.Value);
                    if (ka is StringValue sa && kb is StringValue sb)
                        return string.Compare(sa.Value, sb.Value, StringComparison.Ordinal);
                    throw new InvalidOperationException(
                        "Sorted keys must be comparable numbers or strings."
                    );
                }
            );
            return new ListValue(result);
        }
    );

    public static readonly Command Shuffle = Command.CreateFunction(
        "shuffle",
        (Context context, ListValue list) =>
        {
            var random = new Random();
            var result = list.Elements.OrderBy(x => random.Next()).ToList();
            return new ListValue(result);
        }
    );

    public static readonly Command Range = Command.CreateFunction(
        "range",
        (Context context, NumberValue start, NumberValue count) =>
        {
            int s = (int)start.Value;
            int c = (int)count.Value;
            var list = new List<ToastObject>();
            for (int i = 0; i < c; i++)
            {
                list.Add(new NumberValue(s + i));
            }
            return new ListValue(list);
        }
    );

    public static void Register(Toaster toast)
    {
        toast.RegisterCommand(RangeTo);
        toast.RegisterCommand(ListIn);
        toast.RegisterCommand(IndexAccess);
        toast.RegisterCommand(Member);
        toast.RegisterCommand(Len);
        toast.RegisterCommand(IndexOf);
        toast.RegisterCommand(Filter);
        toast.RegisterCommand(Map);
        toast.RegisterCommand(Combine);
        toast.RegisterCommand(Append);
        toast.RegisterCommand(Remove);
        toast.RegisterCommand(Sort);
        toast.RegisterCommand(SortAs);
        toast.RegisterCommand(Shuffle);
        toast.RegisterCommand(Range);
    }
}
