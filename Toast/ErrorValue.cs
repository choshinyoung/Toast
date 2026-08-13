namespace Toast;

public record ErrorValue : ObjectValue
{
    public string ErrorType { get; }
    public string Message { get; }
    public Location Location { get; init; }
    public ToastValue Cause { get; }

    public ErrorValue(
        string errorType,
        string message,
        Location? location = null,
        ToastValue? cause = null
    )
        : base(
            CreateErrorContext(errorType, message, location ?? Location.Unknown, cause),
            ToastType.ErrorValue
        )
    {
        ErrorType = errorType;
        Message = message;
        Location = location ?? Location.Unknown;
        Cause = cause ?? NullValue.Instance;
    }

    public ErrorValue WithLocation(Location location)
    {
        if (Location != Location.Unknown)
        {
            return this;
        }

        return this switch
        {
            SyntaxError s => s with { Location = location },
            TypeError t => t with { Location = location },
            RuntimeError r => r with { Location = location },
            IndexError i => i with { Location = location },
            _ => this with { Location = location },
        };
    }

    public static ErrorValue Create(
        string errorType,
        string message,
        Location? location = null,
        ToastValue? cause = null
    )
    {
        return errorType switch
        {
            "SyntaxError" => new SyntaxError(message, location, cause),
            "TypeError" => new TypeError(message, location, cause),
            "RuntimeError" => new RuntimeError(message, location, cause),
            "IndexError" => new IndexError(message, location, cause),
            _ => new ErrorValue(errorType, message, location, cause),
        };
    }

    private static Context CreateErrorContext(
        string errorType,
        string message,
        Location location,
        ToastValue? cause
    )
    {
        var ctx = new Context(Toaster.Empty);
        ctx.SetValueDirect("errorType", new StringValue(errorType));
        ctx.SetValueDirect("message", new StringValue(message));
        ctx.SetValueDirect("line", new NumberValue(location.Line));
        ctx.SetValueDirect("column", new NumberValue(location.Column));
        ctx.SetValueDirect("cause", cause ?? NullValue.Instance);
        return ctx;
    }

    public override string ToString()
    {
        return $"[{ErrorType}] {Message} (at line {Location.Line}, col {Location.Column})";
    }
}

public record SyntaxError : ErrorValue
{
    public SyntaxError(string message, Location? location = null, ToastValue? cause = null)
        : base("SyntaxError", message, location, cause) { }
}

public record TypeError : ErrorValue
{
    public TypeError(string message, Location? location = null, ToastValue? cause = null)
        : base("TypeError", message, location, cause) { }

    public static TypeError Mismatch(
        ToastType expected,
        ToastType actual,
        string? paramName = null
    ) =>
        new(
            paramName != null
                ? $"Type mismatch: parameter '{paramName}' expects {expected}, but got {actual}."
                : $"Type mismatch: expected {expected}, but got {actual}."
        );

    public static TypeError CannotAssign(
        ToastType valueType,
        string varName,
        ToastType constraint
    ) =>
        new(
            $"Type mismatch: Cannot assign value of type {valueType} to variable '{varName}' which is constrained to {constraint}."
        );

    public static TypeError ArityMismatch(
        string name,
        int expected,
        int actual,
        bool isAtLeast = false
    ) =>
        new(
            isAtLeast
                ? $"Arity mismatch: '{name}' expects at least {expected} arguments, but got {actual}."
                : $"Arity mismatch: '{name}' expects {expected} arguments, but got {actual}."
        );
}

public record RuntimeError : ErrorValue
{
    public RuntimeError(string message, Location? location = null, ToastValue? cause = null)
        : base("RuntimeError", message, location, cause) { }

    public static RuntimeError UndefinedVariable(string name) =>
        new($"Variable '{name}' is not defined.");

    public static RuntimeError AlreadyDefined(string name) =>
        new($"Variable '{name}' is already defined in the current scope.");

    public static RuntimeError PropertyNotDefined(string name) =>
        new($"Property '{name}' is not defined on target object.");
}

public record IndexError : ErrorValue
{
    public IndexError(string message, Location? location = null, ToastValue? cause = null)
        : base("IndexError", message, location, cause) { }

    public static IndexError OutOfRange(int index, int length, string targetName = "list") =>
        new($"Index {index} is out of range for {targetName} of length {length}.");
}
