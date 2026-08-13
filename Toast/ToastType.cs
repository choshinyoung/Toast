namespace Toast;

public record ToastType(string Name)
{
    public static class Names
    {
        public const string String = "string";
        public const string Number = "number";
        public const string Boolean = "boolean";
        public const string List = "list";
        public const string Object = "object";
        public const string Function = "function";
        public const string Identifier = "identifier";
        public const string Null = "null";
        public const string Any = "any";
        public const string Reference = "reference";
        public const string Type = "type";
    }

    public static readonly ToastType String = new(Names.String);
    public static readonly ToastType Number = new(Names.Number);
    public static readonly ToastType Boolean = new(Names.Boolean);
    public static readonly ToastType List = new(Names.List);
    public static readonly ToastType Object = new(Names.Object);
    public static readonly ToastType Function = new(Names.Function);
    public static readonly ToastType Identifier = new(Names.Identifier);
    public static readonly ToastType Null = new(Names.Null);
    public static readonly ToastType Any = new(Names.Any);
    public static readonly ToastType Reference = new(Names.Reference);
    public static readonly ToastType Type = new(Names.Type);

    public static readonly HashSet<string> BuiltInTypeNames =
    [
        Names.Number,
        Names.String,
        Names.Boolean,
        Names.List,
        Names.Object,
        Names.Any,
        Names.Null,
        Names.Function,
    ];

    public static ToastType FromName(string typeName)
    {
        return typeName switch
        {
            Names.String => String,
            Names.Number => Number,
            Names.Boolean => Boolean,
            Names.List => List,
            Names.Object => Object,
            Names.Function => Function,
            Names.Identifier => Identifier,
            Names.Null => Null,
            Names.Any => Any,
            Names.Reference => Reference,
            Names.Type => Type,
            _ => new ToastType(typeName),
        };
    }

    public override string ToString() => Name;
}
