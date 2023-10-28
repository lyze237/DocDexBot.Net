using Discord;
using Discord.Interactions;
using DocDexBot.Net.Api;
using DocDexBot.Net.Api.Models;
using DocDexBot.Net.Extensions;
using Microsoft.Extensions.Caching.Memory;

namespace DocDexBot.Net.Interactions.AutocompleteHandlers;

public class SearchAutocompleteHandler : AutocompleteHandler
{
    private readonly IDocDexApiClient apiClient;
    private readonly IMemoryCache cache;
    private readonly ILogger<SearchAutocompleteHandler> logger;

    public SearchAutocompleteHandler(IDocDexApiClient apiClient, IMemoryCache cache, ILogger<SearchAutocompleteHandler> logger)
    {
        this.apiClient = apiClient;
        this.cache = cache;
        this.logger = logger;
    }
        
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        var javadoc = autocompleteInteraction.Data.Options.FirstOrDefault(n => n.Name == "javadoc")?.Value as string;
        javadoc = string.IsNullOrWhiteSpace(javadoc) ? "gdx" : javadoc;
        var query = autocompleteInteraction.Data.Current.Value as string;
        logger.LogInformation("Searching for {query} in {javadoc}", query, javadoc);
            
        if (string.IsNullOrWhiteSpace(query))
            return AutocompletionResult.FromSuccess();

        var result = await cache.GetOrCreateAsync($"{query}_{javadoc}", async _ => await apiClient.Search(javadoc, query));
            
        return AutocompletionResult.FromSuccess(result!.Select((o, i) => new AutocompleteResult($"{GetFullName(o).SubstringIgnoreError(75, true)} ({o.Object.Type}{(o.Object.Metadata.Parameters.Count > 0 ? $", {o.Object.Metadata.Parameters.Count} Params" : "")})", $"{query}_{i}")).Take(25));
    }
    
    private static string GetFullName(SearchResult thing)
    {
        return thing.Object.Type switch
        {
            "METHOD" => $"{thing.Object.Metadata.Owner}#{thing.Object.Name}({string.Join(", ", thing.Object.Metadata.Parameters)})",
            "CLASS" => $"{thing.Object.Name}",
            "INTERFACE" => $"{thing.Object.Name}",
            "ENUM" => $"{thing.Object.Name}",
            "FIELD" => $"{thing.Object.Metadata.Owner}%{thing.Object.Name}",
            "CONSTRUCTOR" => $"{thing.Object.Metadata.Owner}#{thing.Object.Name}({string.Join(", ", thing.Object.Metadata.Parameters)})",
            _ => thing.Name
        };
    }
}
