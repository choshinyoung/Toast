namespace Toast;

public class Parser
{
    private readonly List<Token> _tokens;
    private int _position;

    private readonly HashSet<string> _prefixes;
    private readonly Dictionary<string, int> _precedences;

    private const int PrefixPrecedence = 9;
    private const int InfixPrecedence = 6;

    public Parser(List<Token> tokens, List<string> operators, List<string> prefixes)
    {
        _tokens = tokens;
        _position = 0;

        var precedences = new Dictionary<string, int>();

        operators.Add("+");
        operators.Add("*");

        foreach (string op in operators)
        {
            if (op is "=" or "+=" or "-=" or "*=" or "/=" or "%=" or "=>" or "->")
            {
                precedences[op] = 1;
            }
            else if (op is "&&" or "||")
            {
                precedences[op] = 2;
            }
            else
            {
                precedences[op] = op[0] switch
                {
                    '.' => 10,
                    '*' or '/' or '%' => 8,
                    '+' or '-' => 7,
                    '@' or '#' or '$' or '?' or ':' or '~' => 6,
                    '<' or '>' => 5,
                    '!' or '=' => 4,
                    '&' or '|' or '^' => 3,
                    _ => InfixPrecedence,
                };
            }
        }

        _prefixes = [.. prefixes];
        _precedences = precedences;
    }

    public static ProgramNode Parse(List<Token> tokens)
    {
        var filtered = tokens.Where(x => x.Kind != TokenKind.NewLine).ToList();
        var operators = ScanInfixOperators(filtered);

        var parser = new Parser(filtered, operators, []);
        return parser.ParseProgram();
    }

    private static List<string> ScanInfixOperators(List<Token> tokens)
    {
        var customOperators = new List<string>();
        var scopeDepth = 0;

        for (var i = 0; i < tokens.Count - 3; i++)
        {
            if (tokens[i].Kind == TokenKind.LBrace)
            {
                scopeDepth++;
                continue;
            }

            if (tokens[i].Kind == TokenKind.RBrace)
            {
                scopeDepth--;
                continue;
            }

            if (
                tokens[i] is { Kind: TokenKind.Identifier, Value: "fun" }
                && tokens[i + 1] is { Kind: TokenKind.Symbol, Value: "~" }
                && tokens[i + 2] is { Kind: TokenKind.Identifier }
            )
            {
                if (scopeDepth == 0)
                {
                    customOperators.Add(tokens[i + 2].Value!);
                    i += 2;
                }
                else
                {
                    throw new Exception("중위 연산자는 최상위에서 선언되어야 함");
                }
            }
        }

        return customOperators;
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
        }

        return left;
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
                var expr = ParseExpression();
                Expect(TokenKind.RParen, "Expected ')' after expression in group.");
                return expr;
            case TokenKind.LBrace:
                return ParseBlock();
            case TokenKind.LBracket:
                return ParseList();
            default:
                throw new InvalidOperationException(
                    $"Unexpected token '{current.Kind}' ('{current.Value}') for prefix expression."
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

    private bool CanBeArgument(Token token)
    {
        if (
            token.Kind == TokenKind.RParen
            || token.Kind == TokenKind.RBrace
            || token.Kind == TokenKind.RBracket
            || token.Kind == TokenKind.Comma
            || IsInfixOperator(token)
        )
        {
            return false;
        }

        return true;
    }

    private Token Peek() => _tokens[_position];

    private bool IsAtEnd() => _position >= _tokens.Count;

    private Token Previous() => _tokens[_position - 1];

    private Token Consume()
    {
        if (!IsAtEnd())
        {
            _position++;
        }

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
