using Discord.Interactions;

namespace DocDexBot.Net.Interactions;

public class PingInteraction : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Ping Pong")]
    public async Task PingPong()
    {
        await RespondAsync("Pong");
    }
}