using System.Reactive.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;
using DocDexBot.Net.Api;
using DocDexBot.Net.Api.Models;
using DocDexBot.Net.Api.Parsers;
using DocDexBot.Net.Builders;
using DocDexBot.Net.Extensions;
using DocDexBot.Net.Interactions.AutocompleteHandlers;
using Microsoft.Extensions.Caching.Memory;

namespace DocDexBot.Net.Interactions;

public partial class SearchInteraction : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDocDexApiClient apiClient;
    private readonly IDiscordTextFixer discordTextFixer;
    private readonly IMemoryCache cache;
    private readonly ILogger<SearchInteraction> logger;
    
    [GeneratedRegex("(?<search>.+)_(?<index>\\d+)")]
    private static partial Regex QueryRegex();
    private readonly Regex queryRegex = QueryRegex();

    public SearchInteraction(IDocDexApiClient apiClient, IDiscordTextFixer discordTextFixer, IMemoryCache cache, ILogger<SearchInteraction> logger)
    {
        this.apiClient = apiClient;
        this.discordTextFixer = discordTextFixer;
        this.cache = cache;
        this.logger = logger;
    }

    [SlashCommand("searchgdx", "Searches through the libGDX JavaDocs")]
    public async Task Search([Summary("Query"), Autocomplete(typeof(SearchAutocompleteHandler))] string query) =>
        await Search("gdx", query);


    [SlashCommand("search", "Searches through JavaDocs")]
    public async Task Search([Summary("Javadoc"), Autocomplete(typeof(JavadocAutocompleteHandler))] string javadoc, [Summary("Query"), Autocomplete(typeof(SearchAutocompleteHandler))] string query)
    {
        logger.LogInformation("User {user} searched for {query} in {javadoc}", Context.User.Username, query, javadoc);

        var match = queryRegex.Match(query);
        if (!match.Success)
        {
            await RespondAsync("Please select the search query via the autocomplete", ephemeral: true);
            return;
        }
        
        javadoc = string.IsNullOrWhiteSpace(javadoc) ? "gdx" : javadoc;
        var javadocsResult = (await cache.GetOrCreateAsync("javadocs", async _ => await apiClient.GetJavaDocs()))!;
        if (!javadocsResult.Any(j => j.ActualLink.Contains(javadoc)))
        {
            await RespondAsync("Please select the javadoc via the autocomplete", ephemeral: true);
            return;
        }

        var searchString = match.Groups["search"].Value;
        var index = Convert.ToInt32(match.Groups["index"].Value);
        
        var result = await apiClient.Search(string.IsNullOrWhiteSpace(javadoc) ? "gdx" : javadoc, searchString);
        
        var thing = result[index];
        logger.LogInformation("Found {amount} results for {query}; taking first one {thing}", result.Length, query, thing.Name);

        var embed = thing.Object.Type switch
        {
            "METHOD" => new MethodBuilder(thing.Object, discordTextFixer).Build(),
            "CLASS" => new ClassBuilder(thing.Object, discordTextFixer).Build(),
            "INTERFACE" => new InterfaceBuilder(thing.Object, discordTextFixer).Build(),
            "ENUM" => new EnumBuilder(thing.Object, discordTextFixer).Build(),
            "FIELD" => new FieldBuilder(thing.Object, discordTextFixer).Build(),
            "CONSTRUCTOR" => new ConstructorBuilder(thing.Object, discordTextFixer).Build(),
            _ => throw new ArgumentOutOfRangeException($"Type {thing.Object.Type} not supported yet")
        };

        await RespondAsync("", new []{embed.Build()});
    }
}