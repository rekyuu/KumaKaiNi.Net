using KumaKaiNi.Core;
using KumaKaiNi.Core.Models;
using Serilog;

namespace KumaKaiNi.Terminal;

internal static class Program
{
    private static KumaClient? _kuma;
    private static CancellationTokenSource? _cts;

    private static int Main()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .CreateLogger();
        
        _cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            _cts.Cancel();
            eventArgs.Cancel = true;

            Environment.Exit(0);
        };
        
        _kuma = new KumaClient();
        _kuma.Responded += OnKumaResponse;
        
        while (true)
        {
            if (_cts.IsCancellationRequested) break;
            
            Log.Verbose("Starting loop");
            
            Console.Write("> ");
            string? message = Console.ReadLine();

            if (string.IsNullOrEmpty(message)) continue;
            if (message == "exit") Environment.Exit(0);

            KumaRequest kumaRequest = new(
                "rekyuu",
                message,
                SourceSystem.Terminal,
                userAuthority: UserAuthority.User,
                channelId: 5862,
                channelIsPrivate: true,
                channelIsNsfw: true);

            _ = _kuma.SendRequest(kumaRequest);
        }

        return 0;
    }

    private static void OnKumaResponse(KumaResponse kumaResponse)
    {
        Log.Verbose("Got response: {Response}", kumaResponse);
    }
}