using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DocDexBot.Net;
using DocDexBot.Net.Api;
using DocDexBot.Net.Options;
using DocDexBot.Net.Workers;
using Microsoft.Extensions.Options;
using RestSharp;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<DiscordOptions>(context.Configuration.GetSection("Discord"));
        services.Configure<ApiOptions>(context.Configuration.GetSection("Api"));
        
        services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
        {
            AlwaysDownloadUsers = true,
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers | GatewayIntents.MessageContent,
            MessageCacheSize = 10000
        }));
        
        services.AddSingleton(s =>
        {
            var interactionService = new InteractionService(s.GetService<DiscordSocketClient>(), new InteractionServiceConfig
            {
                UseCompiledLambda = true,
                DefaultRunMode = RunMode.Async
            });
            
            return interactionService;
        });

        services.AddSingleton<IDocDexApiClient, DocDexApiClient>();
        
        services.AddSingleton<DocDexDiscordClient>();
        
        services.AddHostedService<DiscordClientWorker>();
        services.AddHostedService<InteractionsWorker>();
    })
    .Build();

host.Run();