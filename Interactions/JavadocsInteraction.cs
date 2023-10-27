using Discord.Interactions;
using DocDexBot.Net.Api;

namespace DocDexBot.Net.Interactions;

public class JavadocsInteraction : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDocDexApiClient apiClient;
    private readonly ILogger<JavadocsInteraction> logger;

    public JavadocsInteraction(IDocDexApiClient apiClient, ILogger<JavadocsInteraction> logger)
    {
        this.apiClient = apiClient;
        this.logger = logger;
    }
    
    [SlashCommand("javadocs", "Lists all javaDocs")]
    public async Task JavaDocs()
    {
        logger.LogInformation("Fetching javadocs");
        var javadocs = await apiClient.GetJavaDocs();
        logger.LogInformation("Found {Amount} Javadocs", javadocs.Length);

        var response = "Here's a list of all supported javadocs:\n";
        
        foreach (var javadoc in javadocs) 
            response += $"* [{string.Join(", ", javadoc.Names)}]({javadoc.ActualLink})\n";

        await RespondAsync(response.Trim());
    }
}