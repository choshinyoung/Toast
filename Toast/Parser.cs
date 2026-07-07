namespace Toast;

public class Parser(
    List<Token> _tokens,
    Dictionary<string, int> _precedences,
    HashSet<string> _prefixes
)
{
    private int _position = 0;

    private const int PrefixPrecedence = 9;
    private const int InfixPrecedence = 6;

    public static ProgramNode Parse(List<Token> tokens)
    {
        var filtered = tokens.Where(x => x.Kind != TokenKind.NewLine).ToList();

        // 임시
        var operators = new Dictionary<string, int>() { { "+", 7 }, { "*", 8 } };

        var parser = new Parser(filtered, operators, []);
        return parser.ParseProgram();
    }

    public ProgramNode ParseProgram()
    {
        var expressions = new List<Node>();

        while (!IsAtEnd())
        {
            expressions.Add(ParseExpression());
        }

        return new ProgramNode(expressions);
    }

    private Node ParseExpression(int precedence = 0)
    {
        var left = ParsePrefix();

        while (!IsAtEnd() && precedence < GetInfixPrecedence(Peek()))
        {
            left = ParseInfix(left);
        }

        while (!IsAtEnd() && CanBeArgument(Peek()))
        {
            var beforePos = _position;

            if (left is CallNode callNode)
            {
                var arguments = new List<Node>(callNode.Arguments)
                {
                    ParseExpression(GetInfixPrecedence(Peek())),
                };
                left = callNode with { Arguments = arguments };
            }
            else
            {
                var arguments = new[] { ParseExpression(GetInfixPrecedence(Peek())) };
                left = new CallNode(left, arguments);
            }

            if (_position == beforePos)
            {
                throw new InvalidOperationException(
                    $"Parser stuck at position {_position}: no progress while parsing arguments."
                );
            }
        }

        return left;
    }

    private Node ParseGroup()
    {
        var items = new List<Node>();

        while (!Check(TokenKind.RParen))
        {
            items.Add(ParseExpression());

            if (!Match(TokenKind.Comma))
            {
                break;
            }
        }

        Expect(TokenKind.RParen, "Expected ')' after expression in group.");

        if (items.Count == 1)
        {
            return items[0];
        }

        return new GroupNode(items);
    }

    private BlockNode ParseBlock()
    {
        var statements = new List<Node>();

        if (!Check(TokenKind.RBrace))
        {
            do
            {
                statements.Add(ParseExpression());
            } while (!IsAtEnd() && !Check(TokenKind.RBrace));
        }

        Expect(TokenKind.RBrace, "Expected '}' to close block.");

        return new BlockNode(statements);
    }

    public ListNode ParseList()
    {
        var items = new List<Node>();

        while (!Check(TokenKind.RBracket))
        {
            items.Add(ParseExpression());

            if (!Match(TokenKind.Comma))
            {
                break;
            }
        }

        Expect(TokenKind.RBracket, "Expected ']' to close list.");

        return new ListNode(items);
    }

    private Node ParsePrimary(Token current)
    {
        switch (current.Kind)
        {
            case TokenKind.Identifier:
                return new IdentifierNode(current.Value!);
            case TokenKind.Integer:
                return new LiteralNode("Integer", int.Parse(current.Value!));
            case TokenKind.Float:
                return new LiteralNode("Float", double.Parse(current.Value!));
            case TokenKind.String:
                return new LiteralNode("String", current.Value!.Trim('"'));
            case TokenKind.LParen:
                return ParseGroup();
            case TokenKind.LBrace:
                return ParseBlock();
            case TokenKind.LBracket:
                return ParseList();
            default:
                throw new InvalidOperationException(
                    $"Unexpected token '{current.Kind}' ('{current.Value}')."
                );
        }
    }

    private FunctionNode ParseFunctionLiteral()
    {
        var parameters = ParseParameters();
        Expect(TokenKind.Symbol, "=>", "Expected '=>' after parameters to define function body.");
        var body = ParseExpression();

        return new FunctionNode(parameters, body);
    }

    private List<ParameterNode> ParseParameters()
    {
        Expect(TokenKind.LParen, "Expected '(' for function parameters.");

        var parameters = new List<ParameterNode>();

        if (!Check(TokenKind.RParen))
        {
            do
            {
                var name = Expect(TokenKind.Identifier, "Expected parameter name.").Value!;
                parameters.Add(new ParameterNode(name, null));
            } while (Match(TokenKind.Comma));
        }

        Expect(TokenKind.RParen, "Expected ')' after parameters.");

        return parameters;
    }

    private Node ParsePrefix()
    {
        if (IsFunctionLiteral())
        {
            return ParseFunctionLiteral();
        }

        var current = Consume();

        if (IsPrefixOperator(current))
        {
            var op = current.Value!;
            var operand = ParseExpression(PrefixPrecedence);
            return new CallNode(new IdentifierNode(op), [operand]);
        }

        return ParsePrimary(current);
    }

    private CallNode ParseInfix(Node left)
    {
        var opToken = Consume();
        int precedence = _precedences[opToken.Value!];

        var right = ParseExpression(precedence);

        return new CallNode(new IdentifierNode(opToken.Value!), [left, right]);
    }

    private bool IsFunctionLiteral()
    {
        if (IsAtEnd() || Peek().Kind != TokenKind.LParen)
        {
            return false;
        }

        var savedPos = _position;
        try
        {
            _position++;
            var balance = 1;
            while (!IsAtEnd() && balance > 0)
            {
                var current = Peek();
                if (current.Kind == TokenKind.LParen)
                {
                    balance++;
                }
                else if (current.Kind == TokenKind.RParen)
                {
                    balance--;
                }
                _position++;
            }

            return !IsAtEnd() && Match(TokenKind.Symbol, "=>");
        }
        finally
        {
            _position = savedPos;
        }
    }

    private bool IsPrefixOperator(Token token)
    {
        if (token.Kind != TokenKind.Symbol)
        {
            return false;
        }

        return _prefixes.Contains(token.Value!);
    }

    private bool IsInfixOperator(Token token)
    {
        return token.Kind is TokenKind.Symbol or TokenKind.Identifier
            && _precedences.ContainsKey(token.Value!);
    }

    private int GetInfixPrecedence(Token token)
    {
        if (IsAtEnd() || !IsInfixOperator(token))
        {
            return 0;
        }

        return _precedences.GetValueOrDefault(token.Value!, 0);
    }

    private bool CanBeArgument(Token token) =>
        token.Kind != TokenKind.RParen
        && token.Kind != TokenKind.RBrace
        && token.Kind != TokenKind.RBracket
        && token.Kind != TokenKind.Comma
        && !IsInfixOperator(token);

    private Token Peek() => _tokens[_position];

    private bool IsAtEnd() => _position >= _tokens.Count;

    private Token Previous() => _tokens[_position - 1];

    private Token Consume()
    {
        if (IsAtEnd())
        {
            throw new InvalidOperationException("Unexpected end of input.");
        }

        _position++;
        return Previous();
    }

    private bool Check(TokenKind kind) => !IsAtEnd() && Peek().Kind == kind;

    private bool Match(TokenKind kind)
    {
        if (!Check(kind))
        {
            return false;
        }

        _position++;
        return true;
    }

    private bool Match(TokenKind kind, string value)
    {
        if (!Check(kind) || Peek().Value != value)
        {
            return false;
        }

        _position++;
        return true;
    }

    private Token Expect(TokenKind kind, string message)
    {
        if (Check(kind))
        {
            return Consume();
        }

        var current = IsAtEnd() ? "end of file" : $"'{Peek().Value}' ({Peek().Kind})";
        throw new InvalidOperationException($"{message} Found {current}.");
    }

    private void Expect(TokenKind kind, string value, string message)
    {
        if (Match(kind, value))
        {
            return;
        }

        var current = IsAtEnd() ? "end of file" : $"'{Peek().Value}' ({Peek().Kind})";
        throw new InvalidOperationException($"{message} Found {current}.");
    }
}
