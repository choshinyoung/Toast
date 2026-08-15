using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Server;
using Toast.LanguageServer.Handlers;

namespace Toast.LanguageServer;

public static class Program
{
    public static async Task Main(string[] args)
    {
        await RunServerAsync();
    }

    public static async Task<int> RunServerAsync()
    {
        var server = await OmniSharp.Extensions.LanguageServer.Server.LanguageServer.From(options =>
            options
                .WithInput(Console.OpenStandardInput())
                .WithOutput(Console.OpenStandardOutput())
                .ConfigureLogging(x =>
                    x.AddLanguageProtocolLogging().SetMinimumLevel(LogLevel.Information)
                )
                .WithHandler<TextDocumentHandler>()
                .WithHandler<HoverHandler>()
                .WithHandler<CompletionHandler>()
                .WithHandler<SemanticTokensHandler>()
        );

        await server.WaitForExit;
        return 0;
    }
}
