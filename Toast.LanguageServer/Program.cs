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
                    x.AddLanguageProtocolLogging().SetMinimumLevel(LogLevel.Error)
                )
                .WithHandler<TextDocumentHandler>()
                .WithHandler<HoverHandler>()
                .WithHandler<CompletionHandler>()
                .OnRequest(
                    "shutdown",
                    async token =>
                    {
                        await Console.Out.FlushAsync(token);
                        return (object?)null;
                    }
                )
                .OnNotification(
                    "exit",
                    (CancellationToken token) =>
                    {
                        Environment.Exit(0);
                        return Task.CompletedTask;
                    }
                )
        );

        await server.WaitForExit;
        return 0;
    }
}
