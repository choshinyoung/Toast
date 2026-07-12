namespace Toast.BuiltIns;

public static class List
{
    public static readonly Command RangeTo = Command.CreateFunction(
        "to",
        (Context context, int left, int right) =>
        {
            return Enumerable.Range(left, right - left + 1).ToList();
        },
        precedence: 6,
        isInfix: true
    );

    public static readonly Command ListIn = Command.CreateFunction(
        "in",
        (Context context, object? left, System.Collections.IEnumerable right) =>
        {
            foreach (var item in right)
            {
                if (Equals(item, left))
                    return true;
            }
            return false;
        },
        precedence: 6,
        isInfix: true
    );

    public static readonly Command IndexAccess = Command.CreateOperator(
        "#",
        (Context context, System.Collections.IEnumerable list, int index) =>
        {
            if (list is string str)
            {
                return str[index].ToString();
            }
            var items = list.Cast<object?>().ToList();
            return items[index];
        },
        precedence: 10
    );

    public static readonly Command Member = Command.CreateFunction(
        "member",
        (Context context, int index, System.Collections.IEnumerable list) =>
        {
            var items = list.Cast<object?>().ToList();
            return items[index];
        }
    );

    public static readonly Command Len = Command.CreateFunction(
        "len",
        (Context context, System.Collections.IEnumerable list) =>
        {
            if (list is string str)
                return str.Length;
            var items = list.Cast<object?>().ToList();
            return items.Count;
        }
    );

    public static readonly Command IndexOf = Command.CreateFunction(
        "indexOf",
        (Context context, System.Collections.IEnumerable list, object? item) =>
        {
            var items = list.Cast<object?>().ToList();
            return items.IndexOf(item);
        }
    );

    public static readonly Command Filter = Command.CreateFunction(
        "filter",
        (Context context, System.Collections.IEnumerable list, FunctionValue predicate) =>
        {
            var items = list.Cast<object?>().ToList();
            var result = new List<object?>();
            foreach (var item in items)
            {
                var res = predicate.Execute([item]);
                if (res is bool b && b)
                {
                    result.Add(item);
                }
            }
            return result;
        }
    );

    public static readonly Command Map = Command.CreateFunction(
        "map",
        (Context context, System.Collections.IEnumerable list, FunctionValue mapper) =>
        {
            var items = list.Cast<object?>().ToList();
            var result = new List<object?>();
            foreach (var item in items)
            {
                result.Add(mapper.Execute([item]));
            }
            return result;
        }
    );

    public static readonly Command Combine = Command.CreateFunction(
        "combine",
        (
            Context context,
            System.Collections.IEnumerable list1,
            System.Collections.IEnumerable list2
        ) =>
        {
            var items1 = list1.Cast<object?>();
            var items2 = list2.Cast<object?>();
            return items1.Concat(items2).ToList();
        }
    );

    public static readonly Command Append = Command.CreateFunction(
        "append",
        (Context context, System.Collections.IEnumerable list, object? item) =>
        {
            var items = list.Cast<object?>().ToList();
            items.Add(item);
            return items;
        }
    );

    public static readonly Command Remove = Command.CreateFunction(
        "remove",
        (Context context, System.Collections.IEnumerable list, object? item) =>
        {
            var items = list.Cast<object?>().ToList();
            items.Remove(item);
            return items;
        }
    );

    public static readonly Command Sort = Command.CreateFunction(
        "sort",
        (Context context, System.Collections.IEnumerable list) =>
        {
            var items = list.Cast<object?>().ToList();
            items.Sort();
            return items;
        }
    );

    public static readonly Command SortAs = Command.CreateFunction(
        "sortAs",
        (Context context, System.Collections.IEnumerable list, FunctionValue keySelector) =>
        {
            var items = list.Cast<object?>().ToList();
            items.Sort(
                (a, b) =>
                {
                    var ka = keySelector.Execute([a]) as IComparable;
                    var kb = keySelector.Execute([b]) as IComparable;
                    if (ka == null || kb == null)
                        return 0;
                    return ka.CompareTo(kb);
                }
            );
            return items;
        }
    );

    public static readonly Command Shuffle = Command.CreateFunction(
        "shuffle",
        (Context context, System.Collections.IEnumerable list) =>
        {
            var items = list.Cast<object?>().ToList();
            var random = new Random();
            return items.OrderBy(x => random.Next()).ToList();
        }
    );

    public static readonly Command Range = Command.CreateFunction(
        "range",
        (Context context, int start, int count) =>
        {
            return Enumerable.Range(start, count).ToList();
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
