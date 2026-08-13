namespace Toast.Cli;

public static class ScriptRunner
{
    public static int ProcessCode(string code, string sourceName, bool printTokens, bool printAst)
    {
        var toast = new Toaster(useBuiltIn: true);

        try
        {
            if (printTokens)
            {
                var tokens = Lexer.Tokenize(code);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"--- Tokens ({sourceName}) ---");
                Console.ResetColor();
                foreach (var tok in tokens)
                {
                    Console.WriteLine(
                        $"  [{tok.Kind}] \"{tok.Value}\" (Line {tok.Location.Line}, Col {tok.Location.Column})"
                    );
                }
                return 0;
            }

            if (printAst)
            {
                var tokens = Lexer.Tokenize(code);
                var ast = Parser.Parse(tokens, toast.GetInfixInfo, toast.IsPrefix);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"--- AST ({sourceName}) ---");
                Console.ResetColor();
                Console.WriteLine(ast);
                return 0;
            }

            var result = toast.Execute(code);
            if (sourceName == "<eval>" && result is not NullValue)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(result);
                Console.ResetColor();
            }
            return 0;
        }
        catch (ToastException ex)
        {
            ErrorFormatter.PrintError(ex.Error, code, sourceName);
            return 1;
        }
        catch (Exception ex)
        {
            ErrorFormatter.PrintError(new RuntimeError(ex.Message), code, sourceName);
            return 1;
        }
    }

    public static int RunFile(string path, bool printTokens, bool printAst)
    {
        if (!File.Exists(path))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: File '{path}' not found.");
            Console.ResetColor();
            return 1;
        }

        string code = File.ReadAllText(path);
        return ProcessCode(code, path, printTokens, printAst);
    }

    public static int RunStdin()
    {
        string input = Console.In.ReadToEnd();
        return ProcessCode(input, "<stdin>", false, false);
    }
}
