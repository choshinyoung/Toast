namespace Toast;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class ToastCommandAttribute(string? name = null) : Attribute
{
    public string? Name { get; } = name;
    public int Precedence { get; set; } = 0;
    public bool IsRightAssociative { get; set; } = false;
    public bool IsPrefix { get; set; } = false;
    public bool IsInfix { get; set; } = false;
    public bool DeclaresMember { get; set; } = false;
    public string Description { get; set; } = "";
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ToastObjectAttribute(string? name = null) : Attribute
{
    public string? Name { get; } = name;
    public string Description { get; set; } = "";
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class ToastConverterAttribute : Attribute
{
    public string? SourceTypeName { get; set; }
    public string? TargetTypeName { get; set; }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ToastTypeAttribute(string? name = null) : Attribute
{
    public string? Name { get; } = name;
    public string Description { get; set; } = "";
}

[AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
public sealed class ToastParameterAttribute : Attribute
{
    public bool IsLazy { get; set; } = false;
    public string Description { get; set; } = "";
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class ToastMemberAttribute(string? typeName = null) : Attribute
{
    public string? TypeName { get; } = typeName;
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ToastModuleAttribute(string? name = null) : Attribute
{
    public string? Name { get; } = name;
    public string Description { get; set; } = "";
}
