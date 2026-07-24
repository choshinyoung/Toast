namespace Toast;

public class Parser(
    List<Token> _tokens,
    Func<Token, (int Precedence, bool IsRight)> _infixResolver,
    Func<Token, bool> _prefixResolver
)
{
    private int _position = 0;
    private int _depth = 0;

    private const int PrefixPrecedence = 9;

    public static ProgramNode Parse(
        List<Token> tokens,
        Func<Token, (int Precedence, bool IsRight)> infixResolver,
        Func<Token, bool> prefixResolver
    )
    {
        var parser = new Parser(tokens, infixResolver, prefixResolver);
        return parser.ParseProgram();
    }

    public ProgramNode ParseProgram()
    {
        var expressions = new List<Node>();

        while (!IsAtEnd())
        {
            MatchWhileNewline();

            if (IsAtEnd())
                break;

            expressions.Add(ParseExpression());

            MatchWhileNewline();
        }

        return new ProgramNode(expressions);
    }

    private Node ParseExpression(int precedence = 0)
    {
        var left = ParsePrefix();

        while (true)
        {
            if (!IsAtEnd() && precedence < GetInfixPrecedence(Peek()))
            {
                left = ParseInfix(left);
                continue;
            }

            if (!IsAtEnd() && precedence < PrefixPrecedence && CanBeArgument(Peek()))
            {
                var beforePos = _position;

                bool isPrefixCall =
                    left is CallNode cn
                    && cn.Callee is IdentifierNode id
                    && IsPrefixOperator(new Token(TokenKind.Symbol, id.Name));
                bool isInfixCall =
                    left is CallNode cn2
                    && cn2.Callee is IdentifierNode id2
                    && IsInfixOperator(new Token(TokenKind.Symbol, id2.Name))
                    && cn2.Arguments.Count >= 2;

                if (Peek().Kind == TokenKind.LParen && !IsFunctionLiteral())
                {
                    Consume();
                    var argList = new List<Node>();
                    while (!Check(TokenKind.RParen) && !IsAtEnd())
                    {
                        argList.Add(ParseExpression());
                        if (!Match(TokenKind.Comma))
                        {
                            break;
                        }
                    }
                    Expect(TokenKind.RParen, "Expected ')' after argument list.");

                    if (left is CallNode callNode && !isPrefixCall && !isInfixCall)
                    {
                        var arguments = new List<Node>(callNode.Arguments);
                        arguments.AddRange(argList);
                        left = callNode with { Arguments = arguments };
                    }
                    else
                    {
                        left = new CallNode(left, argList);
                    }
                }
                else
                {
                    if (left is CallNode callNode && !isPrefixCall && !isInfixCall)
                    {
                        var arguments = new List<Node>(callNode.Arguments)
                        {
                            ParseExpression(PrefixPrecedence),
                        };
                        left = callNode with { Arguments = arguments };
                    }
                    else
                    {
                        var arguments = new[] { ParseExpression(PrefixPrecedence) };
                        left = new CallNode(left, arguments);
                    }
                }

                if (_position == beforePos)
                {
                    throw new InvalidOperationException(
                        $"Parser stuck at position {_position}: no progress while parsing arguments."
                    );
                }
                continue;
            }

            break;
        }

        return left;
    }

    private GroupNode ParseGroup()
    {
        _depth++;
        try
        {
            var items = new List<Node>();

            while (!Check(TokenKind.RParen))
            {
                if (
                    Peek().Kind == TokenKind.Symbol
                    && _position + 1 < _tokens.Count
                    && (
                        _tokens[_position + 1].Kind == TokenKind.RParen
                        || _tokens[_position + 1].Kind == TokenKind.Comma
                    )
                )
                {
                    var opToken = Consume();
                    items.Add(new IdentifierNode(opToken.Value!));
                }
                else
                {
                    items.Add(ParseExpression());
                }

                if (!Match(TokenKind.Comma))
                {
                    break;
                }
            }

            Expect(TokenKind.RParen, "Expected ')' after expression in group.");

            return new GroupNode(items);
        }
        finally
        {
            _depth--;
        }
    }

    private FunctionNode ParseBlock()
    {
        var statements = new List<Node>();

        MatchWhileNewline();

        if (!Check(TokenKind.RBrace))
        {
            do
            {
                statements.Add(ParseExpression());
                MatchWhileNewline();
            } while (!IsAtEnd() && !Check(TokenKind.RBrace));
        }

        Expect(TokenKind.RBrace, "Expected '}' to close block.");

        return new FunctionNode([], statements);
    }

    public ListNode ParseList()
    {
        _depth++;
        try
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
        finally
        {
            _depth--;
        }
    }

    private Node ParsePrimary(Token current)
    {
        return current.Kind switch
        {
            TokenKind.Identifier => new IdentifierNode(current.Value!),
            TokenKind.Integer => new LiteralNode(
                ToastType.Number,
                new NumberValue(double.Parse(current.Value!))
            ),
            TokenKind.Float => new LiteralNode(
                ToastType.Number,
                new NumberValue(double.Parse(current.Value!))
            ),
            TokenKind.String => ParseStringLiteral(current.Value!),
            TokenKind.LParen => ParseGroup(),
            TokenKind.LBrace => ParseBlock(),
            TokenKind.LDoubleBrace => ParseObjectLiteral(),
            TokenKind.LBracket => ParseList(),
            _ => throw new InvalidOperationException(
                $"Unexpected token '{current.Kind}' ('{current.Value}')."
            ),
        };
    }

    private Node ParseStringLiteral(string rawTokenValue)
    {
        string str = rawTokenValue;
        if (
            (str.StartsWith('"') && str.EndsWith('"'))
            || (str.StartsWith('\'') && str.EndsWith('\''))
        )
        {
            str = str[1..^1];
        }

        var parts = new List<Node>();
        var sbStatic = new System.Text.StringBuilder();
        int i = 0;
        int len = str.Length;

        while (i < len)
        {
            if (str[i] == '\\' && i + 1 < len)
            {
                char next = str[i + 1];
                switch (next)
                {
                    case 'n': sbStatic.Append('\n'); break;
                    case '"': sbStatic.Append('"'); break;
                    case '\'': sbStatic.Append('\''); break;
                    case '\\': sbStatic.Append('\\'); break;
                    case '{': sbStatic.Append('{'); break;
                    case '}': sbStatic.Append('}'); break;
                    default:
                        throw new InvalidOperationException(
                            $"Invalid escape sequence '\\{next}' in string literal."
                        );
                }
                i += 2;
                continue;
            }

            if (str[i] == '{')
            {
                if (sbStatic.Length > 0)
                {
                    parts.Add(new LiteralNode(ToastType.String, new StringValue(sbStatic.ToString())));
                    sbStatic.Clear();
                }

                int startExpr = i + 1;
                int depth = 1;
                int j = startExpr;
                bool inString = false;
                char stringQuote = '\0';

                while (j < len && depth > 0)
                {
                    char c = str[j];
                    if (inString)
                    {
                        if (c == '\\' && j + 1 < len)
                        {
                            j += 2;
                            continue;
                        }
                        if (c == stringQuote)
                        {
                            inString = false;
                        }
                    }
                    else
                    {
                        if (c == '"' || c == '\'')
                        {
                            inString = true;
                            stringQuote = c;
                        }
                        else if (c == '{')
                        {
                            depth++;
                        }
                        else if (c == '}')
                        {
                            depth--;
                        }
                    }
                    if (depth > 0) j++;
                }

                if (depth != 0)
                {
                    throw new InvalidOperationException("Unmatched '{' in string interpolation.");
                }

                string exprStr = str[startExpr..j];
                i = j + 1;

                if (string.IsNullOrWhiteSpace(exprStr))
                {
                    parts.Add(new LiteralNode(ToastType.String, new StringValue("")));
                }
                else
                {
                    var tokens = Lexer.Tokenize(exprStr);
                    var programNode = Parse(tokens, _infixResolver, _prefixResolver);
                    if (programNode.Statements.Count == 1)
                    {
                        parts.Add(programNode.Statements[0]);
                    }
                    else if (programNode.Statements.Count > 1)
                    {
                        parts.Add(new GroupNode(programNode.Statements));
                    }
                    else
                    {
                        parts.Add(new LiteralNode(ToastType.String, new StringValue("")));
                    }
                }
                continue;
            }

            sbStatic.Append(str[i]);
            i++;
        }

        if (sbStatic.Length > 0)
        {
            parts.Add(new LiteralNode(ToastType.String, new StringValue(sbStatic.ToString())));
        }

        if (parts.Count == 0)
        {
            return new LiteralNode(ToastType.String, new StringValue(""));
        }

        if (parts.Count == 1 && parts[0] is LiteralNode literalNode)
        {
            return literalNode;
        }

        return new InterpolatedStringNode(parts);
    }

    private ObjectLiteralNode ParseObjectLiteral()
    {
        var statements = new List<Node>();

        MatchWhileNewline();

        if (!Check(TokenKind.RDoubleBrace))
        {
            do
            {
                statements.Add(ParseExpression());
                MatchWhileNewline();
                Match(TokenKind.Comma);
                MatchWhileNewline();
            } while (!IsAtEnd() && !Check(TokenKind.RDoubleBrace));
        }

        Expect(TokenKind.RDoubleBrace, "Expected '}}' to close object literal.");

        return new ObjectLiteralNode(statements);
    }

    private FunctionNode ParseFunctionLiteral()
    {
        List<ParameterNode> parameters;
        if (IsBareFunctionLiteral())
        {
            var name = Consume().Value!;
            parameters = [new ParameterNode(name, null)];
        }
        else
        {
            parameters = ParseParameters();
        }
        Expect(TokenKind.Symbol, "=>", "Expected '=>' after parameters to define function body.");

        if (Match(TokenKind.LBrace))
        {
            var blockFunc = ParseBlock();
            return new FunctionNode(parameters, blockFunc.Statements);
        }
        else
        {
            var body = ParseExpression();
            return new FunctionNode(parameters, [body]);
        }
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
                TypeNode? type = null;
                if (Match(TokenKind.Symbol, ":"))
                {
                    type = ParseType();
                }
                parameters.Add(new ParameterNode(name, type));
            } while (Match(TokenKind.Comma));
        }

        Expect(TokenKind.RParen, "Expected ')' after parameters.");

        return parameters;
    }

    private TypeNode ParseType()
    {
        var typeToken = Expect(TokenKind.Identifier, "Expected type name.");
        ToastType typeEnum = typeToken.Value switch
        {
            "string" => ToastType.String,
            "number" => ToastType.Number,
            "boolean" => ToastType.Boolean,
            "list" => ToastType.List,
            "object" => ToastType.Object,
            _ => new ToastType(typeToken.Value!),
        };

        return new TypeNode(typeEnum);
    }

    private Node ParsePrefix()
    {
        if (IsFunctionLiteral() || IsBareFunctionLiteral())
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
        var (precedence, isRight) = GetInfixInfo(opToken);

        MatchWhileNewline();

        var nextPrecedence = isRight ? precedence - 1 : precedence;
        var right = ParseExpression(nextPrecedence);

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

    private bool IsBareFunctionLiteral()
    {
        if (IsAtEnd() || Peek().Kind != TokenKind.Identifier)
        {
            return false;
        }

        return _position + 1 < _tokens.Count
            && _tokens[_position + 1].Kind == TokenKind.Symbol
            && _tokens[_position + 1].Value == "=>";
    }

    private bool IsPrefixOperator(Token token)
    {
        return _prefixResolver(token);
    }

    private bool IsInfixOperator(Token token)
    {
        return _infixResolver(token).Precedence > 0;
    }

    private (int Precedence, bool IsRight) GetInfixInfo(Token token)
    {
        if (IsAtEnd())
        {
            return (0, false);
        }
        return _infixResolver(token);
    }

    private int GetInfixPrecedence(Token token)
    {
        return GetInfixInfo(token).Precedence;
    }

    private bool CanBeArgument(Token token) =>
        token.Kind != TokenKind.RParen
        && token.Kind != TokenKind.RBrace
        && token.Kind != TokenKind.RDoubleBrace
        && token.Kind != TokenKind.RBracket
        && token.Kind != TokenKind.Comma
        && token.Kind != TokenKind.NewLine
        && !IsInfixOperator(token);

    private void SkipIgnoredNewlines()
    {
        if (_depth > 0)
        {
            while (_position < _tokens.Count && _tokens[_position].Kind == TokenKind.NewLine)
            {
                _position++;
            }
        }
    }

    private Token Peek()
    {
        SkipIgnoredNewlines();
        return _position < _tokens.Count ? _tokens[_position] : _tokens[^1];
    }

    private bool IsAtEnd()
    {
        SkipIgnoredNewlines();
        return _position >= _tokens.Count;
    }

    private Token Consume()
    {
        SkipIgnoredNewlines();
        if (IsAtEnd())
        {
            throw new InvalidOperationException("Unexpected end of file.");
        }

        var token = _tokens[_position];
        _position++;
        return token;
    }

    private bool Check(TokenKind kind) => !IsAtEnd() && Peek().Kind == kind;

    private bool Match(TokenKind kind)
    {
        if (!Check(kind))
        {
            return false;
        }

        Consume();
        return true;
    }

    private bool Match(TokenKind kind, string value)
    {
        if (!Check(kind) || Peek().Value != value)
        {
            return false;
        }

        Consume();
        return true;
    }

    private void MatchWhileNewline()
    {
        while (Match(TokenKind.NewLine)) { }
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
