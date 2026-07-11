namespace Toast;

public enum ToastType
{
    String,
    Integer,
    Float,
    Boolean,
    Array,
    Identifier,
    Function,
    Null,
    Any,
}

public abstract record Node;

public sealed record ProgramNode(IReadOnlyList<Node> Statements) : Node;

public sealed record GroupNode(IReadOnlyList<Node> Items) : Node;

public sealed record BlockNode(IReadOnlyList<Node> Statements) : Node;

public sealed record ListNode(IReadOnlyList<Node> Items) : Node;

public sealed record FunctionNode(IReadOnlyList<ParameterNode> Parameters, Node Body) : Node;

public sealed record ParameterNode(string Name, TypeNode? Type) : Node;

public sealed record TypeNode(ToastType Type, bool IsArray) : Node;

public sealed record CallNode(Node Callee, IReadOnlyList<Node> Arguments) : Node;

public sealed record IdentifierNode(string Name) : Node;

public sealed record LiteralNode(ToastType Type, object? Value) : Node;
