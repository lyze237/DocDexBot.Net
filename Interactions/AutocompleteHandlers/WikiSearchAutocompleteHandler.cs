using Discord;
using Discord.Interactions;
using DocDexBot.Net.Api;

namespace DocDexBot.Net.Interactions.AutocompleteHandlers;

public class WikiSearchAutocompleteHandler : AutocompleteHandler
{
    private readonly IWikiApiClient wikiApiClient;
    private readonly ILogger<WikiSearchAutocompleteHandler> logger;

    public WikiSearchAutocompleteHandler(IWikiApiClient wikiApiClient, ILogger<WikiSearchAutocompleteHandler> logger)
    {
        this.wikiApiClient = wikiApiClient;
        this.logger = logger;
    }

    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        var query = autocompleteInteraction.Data.Current.Value as string;
        logger.LogInformation("Listing Wiki Entries for {query}", query);

        var links = await wikiApiClient.GetMainWikiPageLinks();

        return AutocompletionResult.FromSuccess(links
            .Select((l, i) => new AutocompleteResult(l.InnerText, i.ToString()))
            .Where(l => string.IsNullOrWhiteSpace(query) || l.Name.ToLower().Contains(query.ToLower()))
            .Take(25));
    }
}