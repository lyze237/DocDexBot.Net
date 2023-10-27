using Discord;

namespace DocDexBot.Net.Extensions;

public static class DiscordInteractionExtensions
{
    public static async Task RespondOrFollowupAsync(this IDiscordInteraction context, string text, bool ephemeral)
    {
        if (context.HasResponded)
            await context.FollowupAsync(text, ephemeral: ephemeral);

        await context.RespondAsync(text, ephemeral: true);
    }
}