namespace Toast;

public abstract class Command(
    string name,
    Func<Context, List<Node>, Toast, object?> body,
    int precedence = 0,
    bool isRightAssociative = false,
    bool isPrefix = false
)
{
    public string Name { get; } = name;
    public Func<Context, List<Node>, Toast, object?> Body { get; } = body;
    public int Precedence { get; } = precedence;
    public bool IsRightAssociative { get; } = isRightAssociative;
    public bool IsPrefix { get; } = isPrefix;
}

public class OperatorCommand(
    string name,
    Func<Context, List<Node>, Toast, object?> body,
    int precedence = 0,
    bool isRightAssociative = false,
    bool isPrefix = false
) : Command(name, body, precedence, isRightAssociative, isPrefix) { }

public class IdentifierCommand(
    string name,
    Func<Context, List<Node>, Toast, object?> body,
    bool isInfix = false,
    int precedence = 0,
    bool isRightAssociative = false
) : Command(name, body, precedence, isRightAssociative, false)
{
    public bool IsInfix { get; } = isInfix;
}
