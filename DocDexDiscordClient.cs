using System.Diagnostics;
using Discord;
using Discord.WebSocket;
using DocDexBot.Net.Extensions;
using DocDexBot.Net.Options;
using Microsoft.Extensions.Options;

namespace DocDexBot.Net;

public class DocDexDiscordClient
{
    public readonly ILogger<DocDexDiscordClient> logger;
    private readonly DiscordOptions options;

    public delegate Task OnReadyOnceDelegate();

    public event OnReadyOnceDelegate? OnReadyOnce;

    public DiscordSocketClient Client { get; }

    public SocketGuild Guild { get; set; }
    public SocketTextChannel LogChannel { get; set; }

    public DocDexDiscordClient(DiscordSocketClient client, IOptions<DiscordOptions> options,
        ILogger<DocDexDiscordClient> logger)
    {
        Client = client;
        this.logger = logger;
        this.options = options.Value;

        logger.LogInformation("Initializing discord client");
        Client.Ready += ClientOnReady;
    }

    private async Task ClientOnReady()
    {
        logger.LogInformation("On Ready");
        Client.Ready -= ClientOnReady;

        Guild = Client.GetGuild(options.Guild);
        LogChannel = (SocketTextChannel) await Client.GetChannelAsync(options.LogChannel);
        
        logger.LogInformation("Calling ready once event");
        OnReadyOnce?.Invoke();
    }
    
        public Task RegisterEvent(Func<Task> func)
    {
        Task.Run(async () =>
        {
            try
            {
                await func();
            }
            catch (Exception e)
            {
                await LogError("Unhandled exception", e);
            }
        });

        return Task.CompletedTask;
    }

    public async Task LogError(string message, Exception? exception = null)
    {
        logger.LogError(exception, "{Message}", message);

        try
        {
            var embed = new EmbedBuilder()
                .WithColor(Color.Red)
                .WithCurrentTimestamp()
                .WithTitle("Unhandled Exception")
                .WithDescription(message);

            if (exception != null)
            {
                embed.AddField("Message", exception.Message.SubstringIgnoreError(1024, true));

                var stack = new StackTrace(exception, true);
                for (var i = 0; i < stack.FrameCount && i < 3; i++)
                {
                    var frame = stack.GetFrame(i);
                    var method = frame?.GetMethod()?.ToString() ?? $"No Method {i}";
                    var file =
                        $"{frame?.GetFileName() ?? "No File"}{frame?.GetFileLineNumber()}#{frame?.GetFileColumnNumber()}";
                    embed.AddField(method.SubstringIgnoreError(256, true), file.SubstringIgnoreError(1024, true));
                }

                if (exception.InnerException != null)
                {
                    embed.AddField("Inner Message", exception.InnerException.Message.SubstringIgnoreError(1024, true));
                    
                    var innerStack = new StackTrace(exception.InnerException, true);
                    for (var i = 0; i < innerStack.FrameCount && i < 3; i++)
                    {
                        var frame = innerStack.GetFrame(i);
                        var method = frame?.GetMethod()?.ToString() ?? $"No Method {i}";
                        var file = $"{frame?.GetFileName() ?? "No File"}{frame?.GetFileLineNumber()}#{frame?.GetFileColumnNumber()}";
                        embed.AddField(method.SubstringIgnoreError(256, true), file.SubstringIgnoreError(1024, true));
                    }
                }
            }

            await LogChannel.SendMessageAsync("", embed: embed.Build());
        }
        catch (Exception)
        {
            // ignored
        }
    }
}