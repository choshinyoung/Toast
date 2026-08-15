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
        public const string Error = "Error";
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
    public static readonly ToastType Error = new(Names.Error);

    public static readonly HashSet<string> SystemTypeNames =
    [
        Names.Number,
        Names.String,
        Names.Boolean,
        Names.List,
        Names.Object,
        Names.Function,
        Names.Identifier,
        Names.Null,
        Names.Any,
        Names.Reference,
        Names.Type,
        Names.Error,
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
            Names.Error => Error,
            _ => new ToastType(typeName),
        };
    }

    public static ToastType? TryFromClrType(Type type)
    {
        if (type == typeof(StringValue))
            return String;
        if (type == typeof(NumberValue))
            return Number;
        if (type == typeof(BoolValue))
            return Boolean;
        if (type == typeof(ListValue))
            return List;
        if (type == typeof(ObjectValue))
            return Object;
        if (type == typeof(ErrorValue) || typeof(ErrorValue).IsAssignableFrom(type))
            return Error;
        if (type == typeof(FunctionValue) || type == typeof(CommandValue))
            return Function;
        if (type == typeof(IdentifierValue))
            return Identifier;
        if (type == typeof(NullValue))
            return Null;
        if (type == typeof(AstNodeValue))
            return Any;
        if (type == typeof(ReferenceValue))
            return Reference;

        return null;
    }

    public static ToastType FromClrType(Type type)
    {
        if (!typeof(ToastValue).IsAssignableFrom(type))
        {
            throw new InvalidOperationException(
                $"Command parameter type '{type.Name}' must inherit from ToastValue."
            );
        }

        return TryFromClrType(type) ?? Any;
    }

    public override string ToString() => Name;
}
