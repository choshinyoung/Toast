namespace Toast;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class ToastCommandAttribute(string? name = null, string description = "") : Attribute
{
    public string? Name { get; } = name;
    public string Description { get; set; } = description;
    public int Precedence { get; set; } = 0;
    public bool IsRightAssociative { get; set; } = false;
    public bool IsPrefix { get; set; } = false;
    public bool IsInfix { get; set; } = false;
    public bool DeclaresMember { get; set; } = false;
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ToastObjectAttribute(string? name = null, string description = "") : Attribute
{
    public string? Name { get; } = name;
    public string Description { get; set; } = description;
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class ToastConverterAttribute : Attribute
{
    public string? SourceTypeName { get; set; }
    public string? TargetTypeName { get; set; }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ToastTypeAttribute(string? name = null, string description = "") : Attribute
{
    public string? Name { get; } = name;
    public string Description { get; set; } = description;
}

[AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
public sealed class ToastParameterAttribute(string description = "") : Attribute
{
    public string Description { get; set; } = description;
    public bool IsLazy { get; set; } = false;
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class ToastMemberAttribute(string? typeName = null) : Attribute
{
    public string? TypeName { get; } = typeName;
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ToastModuleAttribute(string? name = null, string description = "") : Attribute
{
    public string? Name { get; } = name;
    public string Description { get; set; } = description;
}
