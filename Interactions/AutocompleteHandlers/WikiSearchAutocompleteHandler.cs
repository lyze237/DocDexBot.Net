using Discord;
using Discord.Interactions;
using DocDexBot.Net.Api;
using DocDexBot.Net.Extensions;
using HtmlAgilityPack;

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

    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        var query = autocompleteInteraction.Data.Current.Value as string;
        logger.LogInformation("Listing Wiki Entries for {query}", query);

        var wikiLinks = await wikiApiClient.GetMainWikiPageWikiLinks();
        
        return AutocompletionResult.FromSuccess(
            wikiLinks.SelectMany(w => w.GetAllChildren())
            .Select((l, i) => new AutocompleteResult(l.GetFullName().SubstringIgnoreErrorFromBack(100, true), i.ToString()))
            .Where(l => string.IsNullOrWhiteSpace(query) || l.Name.ToLower().Contains(query.ToLower()))
            .Take(25));
    }
}