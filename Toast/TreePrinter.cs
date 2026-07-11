using System.Text;

namespace Toast;

public static class TreePrinter
{
    public static string Print(Node node)
    {
        var builder = new StringBuilder();
        WriteNode(builder, node, string.Empty, true);
        return builder.ToString();
    }

    private static void WriteNode(StringBuilder builder, Node node, string indent, bool last)
    {
        builder.Append(indent);
        builder.Append(last ? "└─ " : "├─ ");
        builder.AppendLine(Label(node));

        var children = Children(node).ToArray();
        var nextIndent = indent + (last ? "   " : "│  ");

        for (var index = 0; index < children.Length; index++)
        {
            WriteNode(builder, children[index], nextIndent, index == children.Length - 1);
        }
    }

    private static string Label(Node node) =>
        node switch
        {
            ProgramNode program => $"Program ({program.Statements.Count} statements)",
            GroupNode group => $"Group ({group.Items.Count} items)",
            BlockNode block => $"Block ({block.Statements.Count} statements)",
            ListNode list => $"List ({list.Items.Count} items)",
            FunctionNode function => $"Function ({function.Parameters.Count} params)",
            CallNode call => $"Call {Describe(call.Callee)}",
            IdentifierNode identifier => $"Identifier {identifier.Name}",
            LiteralNode literal => $"Literal {literal.Type}: {literal.Value}",
            ParameterNode parameter => parameter.Type is null
                ? $"Parameter {parameter.Name}"
                : $"Parameter {parameter.Name}: {DescribeType(parameter.Type)}",
            TypeNode type => $"Type {DescribeType(type)}",
            _ => node.GetType().Name,
        };

    private static string Describe(Node node) =>
        node switch
        {
            IdentifierNode identifier => identifier.Name,
            TypeNode type => DescribeType(type),
            _ => node.GetType().Name,
        };

    private static string DescribeType(TypeNode type) =>
        type.IsArray ? $"{type.Type}[]" : type.Type.ToString();

    private static IEnumerable<Node> Children(Node node) =>
        node switch
        {
            ProgramNode program => program.Statements,
            GroupNode group => group.Items,
            BlockNode block => block.Statements,
            ListNode list => list.Items,
            FunctionNode function => function.Parameters.Cast<Node>().Concat([function.Body]),
            CallNode call => new[] { call.Callee }.Concat(call.Arguments),
            ParameterNode parameter => parameter.Type is null ? [] : [parameter.Type],
            _ => [],
        };
}
