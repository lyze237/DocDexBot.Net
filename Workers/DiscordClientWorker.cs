using Discord;
using Discord.WebSocket;
using DocDexBot.Net.Options;
using Microsoft.Extensions.Options;

namespace DocDexBot.Net.Workers;

public class DiscordClientWorker : BackgroundService
{
    private readonly DiscordSocketClient client;
    private readonly IServiceScopeFactory scopeFactory;
    private readonly DiscordOptions options;
    private readonly ILogger<DiscordClientWorker> logger;

    public DiscordClientWorker(DiscordSocketClient client, IServiceScopeFactory scopeFactory, IOptions<DiscordOptions> options, ILogger<DiscordClientWorker> logger)
    {
        this.client = client;
        this.scopeFactory = scopeFactory;
        this.options = options.Value;
        this.logger = logger;

        client.Ready += ClientOnReady;
        client.Log += ClientOnLog;
    }

    private Task ClientOnLog(LogMessage arg)
    {
        switch (arg.Severity)
        {
            case LogSeverity.Critical:
                logger.LogCritical(arg.Exception, arg.Message);
                break;
            case LogSeverity.Error:
                logger.LogError(arg.Exception, arg.Message);
                break;
            case LogSeverity.Warning:
                logger.LogWarning(arg.Exception, arg.Message);
                break;
            case LogSeverity.Info:
                logger.LogInformation(arg.Exception, arg.Message);
                break;
            case LogSeverity.Verbose:
                logger.LogTrace(arg.Exception, arg.Message);
                break;
            case LogSeverity.Debug:
                logger.LogDebug(arg.Exception, arg.Message);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return Task.CompletedTask;
    }

    private async Task ClientOnReady()
    {
        logger.LogInformation("On Ready");
        
        await Task.FromResult(Task.CompletedTask);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Hello");
        
        await client.LoginAsync(TokenType.Bot, options.Token);
        await client.StartAsync();

        await Task.Delay(-1, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Goodbye");

        await client.DisposeAsync();
    }
}