using Toast;

while (true)
{
    var input = Console.ReadLine()!;
    var tokens = Lexer.Tokenize(input);

    foreach (var token in tokens)
    {
        Console.Write($"({token.Kind}, {token.Value}) ");
    }
    Console.WriteLine();

    var ast = Parser.Parse(tokens);

    Console.WriteLine(TreePrinter.Print(ast));
}
