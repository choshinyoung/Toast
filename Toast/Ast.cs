namespace Toast;

public abstract record Node;

public sealed record ProgramNode(IReadOnlyList<Node> Statements) : Node;

public sealed record BlockNode(IReadOnlyList<Node> Statements) : Node;

public sealed record ListNode(IReadOnlyList<Node> Items) : Node;

public sealed record FunctionNode(IReadOnlyList<ParameterNode> Parameters, Node Body) : Node;

public sealed record ParameterNode(string Name, TypeNode? Type) : Node;

public sealed record TypeNode(string Name, bool IsArray) : Node;

public sealed record CallNode(Node Callee, IReadOnlyList<Node> Arguments) : Node;

public sealed record GroupNode(Node Expression) : Node;

public sealed record IdentifierNode(string Name) : Node;

// 타입 Enum화
public sealed record LiteralNode(string Kind, object? Value) : Node;
