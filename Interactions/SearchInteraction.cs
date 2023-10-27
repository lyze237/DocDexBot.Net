using System.Reactive.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;
using DocDexBot.Net.Api;
using DocDexBot.Net.Api.Models;
using DocDexBot.Net.Api.Parsers;
using DocDexBot.Net.Extensions;
using Microsoft.Extensions.Caching.Memory;

namespace DocDexBot.Net.Interactions;

public partial class SearchInteraction : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDocDexApiClient apiClient;
    private readonly HtmlToDiscordParser htmlparser;
    private readonly IMemoryCache cache;
    private readonly ILogger<SearchInteraction> logger;
    
    [GeneratedRegex("(?<search>.+)_(?<index>\\d+)")]
    private static partial Regex QueryRegex();
    private readonly Regex queryRegex = QueryRegex();

    public SearchInteraction(IDocDexApiClient apiClient, HtmlToDiscordParser htmlparser, IMemoryCache cache, ILogger<SearchInteraction> logger)
    {
        this.apiClient = apiClient;
        this.htmlparser = htmlparser;
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

        var embed = new EmbedBuilder()
            .WithTitle(GetFullName(thing))
            .WithUrl(thing.Object.Link);

        var description = string.IsNullOrWhiteSpace(thing.Object.Description) ? "" : $"**Description:**\n{htmlparser.ParseDescription(thing)}\n\n";
        var parameters = thing.Object.Metadata.ParameterDescriptions.Count > 0 ? $"**Parameters:**\n{string.Join("\n", thing.Object.Metadata.ParameterDescriptions.Select(p => $"`{p.Key}` - {p.Value}"))}\n" : "";
        switch (thing.Object.Type)
        {
            case "METHOD":
                embed.WithDescription("```java\n" +
                                      $"{thing.Object.Metadata.Returns} {thing.Object.Name}({string.Join(", ", thing.Object.Metadata.Parameters)}) {{ }}\n" +
                                      "```\n" +
                                      description +
                                      parameters);
                break;
            case "CLASS":
            {
                var extends = thing.Object.Metadata.Extensions.Contains("java.lang.Object") ? "" : $" extends {string.Join(", ", RemovePackage(thing.Object.Metadata.Extensions))}";
                var implements = thing.Object.Metadata.Implementations.Count == 0 ? "" : $" implements {string.Join(", ", RemovePackage(thing.Object.Metadata.Implementations))}";

                if (thing.Object.Metadata.Methods.Count > 0)
                    embed.AddField("Methods", thing.Object.Metadata.Methods.Count, true);
                if (thing.Object.Metadata.Fields.Count > 0)
                    embed.AddField("Fields", thing.Object.Metadata.Fields.Count, true);
                if (thing.Object.Metadata.SubClasses.Count > 0)
                    embed.AddField("SubClasses", thing.Object.Metadata.SubClasses.Count, true);

                embed.WithDescription("```java\n" +
                                      $"{string.Join(" ", thing.Object.Modifiers)} class {thing.Object.Name}{extends}{implements} {{ }}\n" +
                                      "```\n" +
                                      description);
                break;
            }
            case "FIELD":
                embed.WithDescription("```java\n" +
                                      $"{string.Join(" ", thing.Object.Modifiers)} {thing.Object.Metadata.Returns} {thing.Object.Name};\n" +
                                      "```\n" +
                                      description);
                break;
            case "CONSTRUCTOR":
                embed.WithDescription("```java\n" +
                                      $"{string.Join(" ", thing.Object.Modifiers)}({string.Join(", ", thing.Object.Metadata.Parameters)}) {thing.Object.Name} {{ }}\n" +
                                      "```\n" +
                                      description + 
                                      parameters);
                break;
            default:
                await RespondAsync($"Type {thing.Object.Type} not supported yet");
                return;
        }

        await RespondAsync("", new []{embed.Build()});
    }

    private static IEnumerable<string> RemovePackage(IEnumerable<string> names) =>
        names.Select(RemovePackage).ToList();

    private static string RemovePackage(string name) => 
        name.Contains('.') ? name[(name.LastIndexOf('.') + 1)..] : name;

    private static string GetFullName(SearchResult thing, bool withParameters = false, bool withPackage = true)
    {
        var package = withPackage ? $"{thing.Object.Package}." : "";
        return thing.Object.Type switch
        {
            "METHOD" => $"{package}{thing.Object.Metadata.Owner}#{thing.Object.Name}{(withParameters ? $"({string.Join(", ", thing.Object.Metadata.Parameters)})" : "")}",
            "CLASS" => $"{package}{thing.Object.Name}",
            "FIELD" => $"{package}{thing.Object.Metadata.Owner}%{thing.Object.Name}",
            "CONSTRUCTOR" => $"{package}{thing.Object.Metadata.Owner}#{thing.Object.Name}{(withParameters ? $"({string.Join(", ", thing.Object.Metadata.Parameters)})" : "")}",
            _ => thing.Name
        };
    }

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
            
            return AutocompletionResult.FromSuccess(result!.Select((o, i) => new AutocompleteResult($"{GetFullName(o, true, false).SubstringIgnoreError(75, true)} ({o.Object.Type}{(o.Object.Metadata.Parameters.Count > 0 ? $", {o.Object.Metadata.Parameters.Count} Params" : "")})", $"{query}_{i}")).Take(25));
        }
    }
    
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
}