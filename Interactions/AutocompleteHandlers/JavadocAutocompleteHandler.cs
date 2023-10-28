using Discord;
using Discord.Interactions;
using DocDexBot.Net.Api;
using Microsoft.Extensions.Caching.Memory;

namespace DocDexBot.Net.Interactions.AutocompleteHandlers;

public class JavadocAutocompleteHandler : AutocompleteHandler
{
    private readonly IDocDexApiClient apiClient;
    private readonly IMemoryCache cache;
    private readonly ILogger<JavadocAutocompleteHandler> logger;

    public JavadocAutocompleteHandler(IDocDexApiClient apiClient, IMemoryCache cache, ILogger<JavadocAutocompleteHandler> logger)
    {
        this.apiClient = apiClient;
        this.cache = cache;
        this.logger = logger;
    }
        
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        var query = autocompleteInteraction.Data.Current.Value as string;
        logger.LogInformation("Listing JavaDocs for {javadoc}", query);

        var result = (await cache.GetOrCreateAsync("javadocs", async _ => await apiClient.GetJavaDocs()))!;
            
        if (!string.IsNullOrWhiteSpace(query))
            result = result.Where(r => r.Names.Any(n => n.ToLower().Contains(query.ToLower()))).ToArray();
            
        return AutocompletionResult.FromSuccess(result.Select(o => new AutocompleteResult(o.Names.First(), o.Names.First())).Take(25));
    }
}