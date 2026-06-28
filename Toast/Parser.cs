namespace Toast;

public enum Associativity
{
    Left,
    Right,
}

public sealed record OperatorInfo(int Precedence, Associativity Associativity);

public class Parser
{
    private readonly List<Token> _tokens;
    private int _position;

    private readonly Dictionary<string, OperatorInfo> _infixOperators;

    private const int PrefixPrecedence = 9;
    private const int InfixPrecedence = 6;

    public Parser(List<Token> tokens, Dictionary<string, OperatorInfo> infixOperators)
    {
        _tokens = tokens;
        _position = 0;

        var operators = new Dictionary<string, OperatorInfo>();

        foreach (var op in infixOperators)
        {
            if (op.Key is "=" or "+=" or "-=" or "*=" or "/=" or "%=" or "=>" or "->")
            {
                operators[op.Key] = new(1, Associativity.Right);
            }
            else if (op.Key is "&&" or "||")
            {
                operators[op.Key] = new(2, Associativity.Right);
            }
            else
            {
                operators[op.Key] = op.Key[0] switch
                {
                    '.' => new(10, Associativity.Left),
                    '*' or '/' or '%' => new(8, Associativity.Left),
                    '+' or '-' => new(7, Associativity.Left),
                    '<' or '>' => new(5, Associativity.Left),
                    '!' or '=' => new(4, Associativity.Left),
                    '&' or '|' or '^' => new(3, Associativity.Left),
                    '@' or '#' or '$' or '?' or ':' or '~' => new(6, Associativity.Left),
                    _ => new(InfixPrecedence, Associativity.Left),
                };
            }
        }

        _infixOperators = operators;
    }

    public static ProgramNode Parse(List<Token> tokens)
    {
        var filtered = tokens.Where(x => x.Kind != TokenKind.NewLine).ToList();
        var operators = ScanInfixOperators(filtered);

        var parser = new Parser(filtered, operators);
        return parser.ParseProgram();
    }

    private static Dictionary<string, OperatorInfo> ScanInfixOperators(List<Token> tokens)
    {
        var customOperators = new Dictionary<string, OperatorInfo>();
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
                tokens[i].Kind == TokenKind.Identifier
                && tokens[i].Value == "fun"
                && tokens[i + 1].Kind == TokenKind.Symbol
                && tokens[i + 1].Value == "~"
                && tokens[i + 2].Kind == TokenKind.Identifier
            )
            {
                if (scopeDepth == 0)
                {
                    var name = tokens[i + 2].Value!;
                    customOperators[name] = new OperatorInfo(InfixPrecedence, Associativity.Left);
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
                return new GroupNode(expr);
            case TokenKind.LBrace:
                return ParseBlock();
            case TokenKind.LBracket:
                var listNode = ParseList();
                return listNode;
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
        var parameters = new List<ParameterNode>();
        Expect(TokenKind.LParen, "Expected '(' for function parameters.");

        if (!Check(TokenKind.RParen))
        {
            do
            {
                // `~` for parameters not yet implemented per spec, but placeholder logic can be added here
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
        var opInfo = _infixOperators[opToken.Value!];

        var rightAssociativityCorrection = opInfo.Associativity == Associativity.Right ? -1 : 0;
        var right = ParseExpression(opInfo.Precedence + rightAssociativityCorrection);

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

    private static bool IsPrefixOperator(Token token)
    {
        if (token.Kind != TokenKind.Symbol)
        {
            return false;
        }

        var op = token.Value;
        // 임시
        return op is "-" or "+" or "!" or "~" or "*";
    }

    private bool IsInfixOperator(Token token)
    {
        return token.Kind is TokenKind.Symbol or TokenKind.Identifier
            && _infixOperators.ContainsKey(token.Value!);
    }

    private int GetInfixPrecedence(Token token)
    {
        if (IsAtEnd() || !IsInfixOperator(token))
        {
            return 0;
        }

        return _infixOperators.TryGetValue(token.Value!, out var info) ? info.Precedence : 0;
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
