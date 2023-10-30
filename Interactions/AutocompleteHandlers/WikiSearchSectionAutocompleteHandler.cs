﻿using Discord;
using Discord.Interactions;
using DocDexBot.Net.Api;
using HtmlAgilityPack;

namespace DocDexBot.Net.Interactions.AutocompleteHandlers;

public class WikiSearchSectionAutocompleteHandler : AutocompleteHandler
{
    private readonly IWikiApiClient wikiApiClient;
    private readonly ILogger<WikiSearchSectionAutocompleteHandler> logger;
    
    public WikiSearchSectionAutocompleteHandler(IWikiApiClient wikiApiClient, ILogger<WikiSearchSectionAutocompleteHandler> logger)
    {
        this.wikiApiClient = wikiApiClient;
        this.logger = logger;
    }
    
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        var pageNumberString = autocompleteInteraction.Data.Options.FirstOrDefault(n => n.Name == "page")?.Value as string;
        var pageNumber = Convert.ToInt32(pageNumberString);
        
        var entry = (await wikiApiClient.GetMainWikiPageLinks())[pageNumber];
        
        var (sections, header) = await wikiApiClient.GetWikiPageSections(entry.Attributes["href"].Value);
        
        var section = autocompleteInteraction.Data.Current.Value as string;

        var autocompleteResults = sections
            .Select((s, i) => new AutocompleteResult(s.InnerText, $"{pageNumberString}~{s.Id}~{i}~{header}"))
            .Where(l => string.IsNullOrWhiteSpace(section) || l.Name.ToLower().Contains(section.ToLower()))
            .Take(24)
            .ToList();
        
        autocompleteResults.Insert(0, new AutocompleteResult("Header", pageNumberString));
        return AutocompletionResult.FromSuccess(autocompleteResults);
    }
}
