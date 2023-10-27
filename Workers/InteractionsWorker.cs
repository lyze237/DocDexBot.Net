using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DocDexBot.Net.Extensions;
using DocDexBot.Net.Options;
using Microsoft.Extensions.Options;

namespace DocDexBot.Net.Workers;

public class InteractionsWorker : BackgroundService
{
    private readonly DocDexDiscordClient client;
    private readonly InteractionService interactionService;
    private readonly DiscordOptions options;
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<InteractionsWorker> logger;

    public InteractionsWorker(DocDexDiscordClient client, IOptions<DiscordOptions> options,
        IServiceProvider serviceProvider, InteractionService interactionService,
        ILogger<InteractionsWorker> logger)
    {
        this.client = client;
        this.options = options.Value;
        this.serviceProvider = serviceProvider;
        this.interactionService = interactionService;
        this.logger = logger;
        
        interactionService.Log += InteractionServiceOnLog;
        client.OnReadyOnce += ClientOnReady;
    }

    private Task InteractionServiceOnLog(LogMessage arg)
    {
        switch (arg.Severity)
        {
            case LogSeverity.Critical:
                logger.LogCritical(arg.Exception, "{Message}", arg.Message);
                break;
            case LogSeverity.Error:
                logger.LogError(arg.Exception, "{Message}", arg.Message);
                break;
            case LogSeverity.Warning:
                logger.LogWarning(arg.Exception, "{Message}", arg.Message);
                break;
            case LogSeverity.Info:
                logger.LogInformation(arg.Exception,"{Message}", arg.Message);
                break;
            case LogSeverity.Verbose:
                logger.LogTrace(arg.Exception,"{Message}", arg.Message);
                break;
            case LogSeverity.Debug:
                logger.LogDebug(arg.Exception,"{Message}", arg.Message);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(arg));
        }

        return Task.CompletedTask;
    }

    private async Task ClientOnReady()
    {
        try
        {
            //await client.Guild.DeleteApplicationCommandsAsync();
            
            await interactionService.AddModulesAsync(typeof(Program).Assembly, serviceProvider);
            await interactionService.RegisterCommandsToGuildAsync(client.Guild.Id);
            
            client.Client.InteractionCreated += interaction => _ = ClientOnInteractionCreated(interaction);

            interactionService.SlashCommandExecuted += (info, context, result) => client.RegisterEvent(async () => await InteractionServiceOnSlashCommandExecuted(info, context, result));
            interactionService.ContextCommandExecuted += (info, context, result) => client.RegisterEvent(async () => await InteractionServiceOnContextCommandExecuted(info, context, result));
            interactionService.ComponentCommandExecuted += (info, context, result) => client.RegisterEvent(async () => await InteractionServiceOnComponentCommandExecuted(info, context, result));
        }
        catch (Exception e)
        {
            await client.LogError("Interaction ready error", e);
        }
    }

    private async Task InteractionServiceOnComponentCommandExecuted(ComponentCommandInfo command, IInteractionContext context, IResult result)
    {
        logger.LogInformation("User {User} finished component command {Command} {Result}", context.User, command?.Name, result.IsSuccess ? "successfully" : $"with error {result.Error} and reason {result.ErrorReason}");
        
        var embed = new EmbedBuilder()
            .WithAuthor(context.User)
            .WithCurrentTimestamp()
            .WithColor(Color.Green)
            .WithDescription($"**User {context.User?.Mention} executed component command {command?.Name}**");

        if (!result.IsSuccess)
        {
            embed.WithColor(Color.Red);
            
            embed.AddField("Error", result.Error);
            embed.AddField("Reason", result.ErrorReason);
            
            switch (result.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    await context!.Interaction.RespondOrFollowupAsync($"Unmet Precondition: {result.ErrorReason}", ephemeral: true);
                    break;
                case InteractionCommandError.UnknownCommand:
                    await context!.Interaction.RespondOrFollowupAsync("Unknown command", ephemeral: true);
                    break;
                case InteractionCommandError.BadArgs:
                    await context!.Interaction.RespondOrFollowupAsync("Invalid number or arguments", ephemeral: true);
                    break;
                case InteractionCommandError.Exception:
                    await context!.Interaction.RespondOrFollowupAsync($"Command exception: {result.ErrorReason}", ephemeral: true);
                    break;
                case InteractionCommandError.Unsuccessful:
                    await context!.Interaction.RespondOrFollowupAsync("Command could not be executed", ephemeral: true);
                    break;
                case InteractionCommandError.ConvertFailed:
                    await context!.Interaction.RespondOrFollowupAsync("Command could not be converted", ephemeral: true);
                    break;
                case InteractionCommandError.ParseFailed:
                    await context!.Interaction.RespondOrFollowupAsync("Command could not be parsed", ephemeral: true);
                    break;
                default:
                    break;
            }
        }

        await client.LogChannel.SendMessageAsync("", embed: embed.Build());
    }

    private async Task InteractionServiceOnContextCommandExecuted(ContextCommandInfo command, IInteractionContext context, IResult result)
    {
        logger.LogInformation("User {User} finished context command {Command} {Result}", context.User, command?.Name, result.IsSuccess ? "successfully" : $"with error {result.Error} and reason {result.ErrorReason}");
        
        var embed = new EmbedBuilder()
            .WithAuthor(context.User)
            .WithCurrentTimestamp()
            .WithColor(Color.Green)
            .WithDescription($"**User {context.User?.Mention} executed context command {command?.Name}**");
        
        if (!result.IsSuccess)
        {
            embed.WithColor(Color.Red);
            
            embed.AddField("Error", result.Error);
            embed.AddField("Reason", result.ErrorReason);
            
            switch (result.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    await context.Interaction.RespondOrFollowupAsync($"Unmet Precondition: {result.ErrorReason}", ephemeral: true);
                    break;
                case InteractionCommandError.UnknownCommand:
                    await context.Interaction.RespondOrFollowupAsync("Unknown command", ephemeral: true);
                    break;
                case InteractionCommandError.BadArgs:
                    await context.Interaction.RespondOrFollowupAsync("Invalid number or arguments", ephemeral: true);
                    break;
                case InteractionCommandError.Exception:
                    await context.Interaction.RespondOrFollowupAsync($"Command exception: {result.ErrorReason}", ephemeral: true);
                    break;
                case InteractionCommandError.Unsuccessful:
                    await context.Interaction.RespondOrFollowupAsync("Command could not be executed", ephemeral: true);
                    break;
                case InteractionCommandError.ConvertFailed:
                    await context.Interaction.RespondOrFollowupAsync("Command could not be converted", ephemeral: true);
                    break;
                case InteractionCommandError.ParseFailed:
                    await context.Interaction.RespondOrFollowupAsync("Command could not be parsed", ephemeral: true);
                    break;
                default:
                    break;
            }
        }
        
        await client.LogChannel.SendMessageAsync("", embed: embed.Build());
    }

    private async Task InteractionServiceOnSlashCommandExecuted(SlashCommandInfo command, IInteractionContext context, IResult result)
    {
        logger.LogInformation("User {User} finished slash command {Command} {Result}", context.User, command?.Name, result.IsSuccess ? "successfully" : $"with error {result.Error} and reason {result.ErrorReason}");
        
        var embed = new EmbedBuilder()
            .WithAuthor(context.User)
            .WithCurrentTimestamp()
            .WithColor(Color.Green)
            .WithDescription($"**User {context.User?.Mention} executed slash command {command?.Name}**");

        if (!result.IsSuccess)
        {
            embed.WithColor(Color.Red);

            embed.AddField("Error", result.Error);
            embed.AddField("Reason", result.ErrorReason);
            switch (result.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    await context.Interaction.RespondOrFollowupAsync($"Unmet Precondition: {result.ErrorReason}",
                        ephemeral: true);
                    break;
                case InteractionCommandError.UnknownCommand:
                    await context.Interaction.RespondOrFollowupAsync("Unknown command", ephemeral: true);
                    break;
                case InteractionCommandError.BadArgs:
                    await context.Interaction.RespondOrFollowupAsync("Invalid number or arguments", ephemeral: true);
                    break;
                case InteractionCommandError.Exception:
                    await context.Interaction.RespondOrFollowupAsync($"Command exception: {result.ErrorReason}", ephemeral: true);
                    break;
                case InteractionCommandError.Unsuccessful:
                    await context.Interaction.RespondOrFollowupAsync("Command could not be executed", ephemeral: true);
                    break;
                case InteractionCommandError.ConvertFailed:
                    await context.Interaction.RespondOrFollowupAsync("Command could not be converted", ephemeral: true);
                    break;
                case InteractionCommandError.ParseFailed:
                    await context.Interaction.RespondOrFollowupAsync("Command could not be parsed", ephemeral: true);
                    break;
                default:
                    break;
            }
        }

        await client.LogChannel.SendMessageAsync("", embed: embed.Build());
    }

    private async Task ClientOnInteractionCreated(SocketInteraction arg)
    {
        logger.LogInformation("{User} message interaction created in {Channel}", arg.User, arg.Channel);
        
        try
        {
            var ctx = new SocketInteractionContext(client.Client, arg);
            await interactionService.ExecuteCommandAsync(ctx, serviceProvider);
        }
        catch (Exception e)
        {
            await client.LogError("Unknown exception in interaction handler", e);

            if (arg.Type == InteractionType.ApplicationCommand)
            {
                var response = await arg.GetOriginalResponseAsync();
                await response.DeleteAsync();
            }
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(-1, stoppingToken);
    }
}