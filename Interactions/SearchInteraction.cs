using System.Reactive.Linq;
using System.Runtime.InteropServices.ComTypes;
using Discord;
using Discord.Interactions;
using DocDexBot.Net.Api;
using DocDexBot.Net.Api.Models;
using DocDexBot.Net.Api.Parsers;
using DocDexBot.Net.Extensions;

namespace DocDexBot.Net.Interactions;

public class SearchInteraction : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDocDexApiClient apiClient;
    private readonly HtmlToDiscordParser htmlparser;
    private readonly ILogger<SearchInteraction> logger;

    public SearchInteraction(IDocDexApiClient apiClient, HtmlToDiscordParser htmlparser, ILogger<SearchInteraction> logger)
    {
        this.apiClient = apiClient;
        this.htmlparser = htmlparser;
        this.logger = logger;
    }

    [SlashCommand("searchgdx", "Searches through the libGDX JavaDocs")]
    public async Task Search([Summary("Query"), Autocomplete(typeof(SearchAutocompleteHandler))] string query) =>
        await Search("gdx", query);


    [SlashCommand("search", "Searches through JavaDocs")]
    public async Task Search([Summary("Javadoc"), Autocomplete(typeof(JavadocAutocompleteHandler))] string javadoc, [Summary("Query"), Autocomplete(typeof(SearchAutocompleteHandler))] string query)
    {
        logger.LogInformation("User {user} searched for {query} in {javadoc}", Context.User.Username, query, javadoc);
        
        var result = await apiClient.Search(string.IsNullOrWhiteSpace(javadoc) ? "gdx" : javadoc, query);
        
        var thing = result.First();
        logger.LogInformation("Found {amount} results for {query}; taking first one {thing}", result.Length, query, thing.Name);

        var embed = new EmbedBuilder()
            .WithTitle(GetFullName(thing))
            .WithUrl(thing.Object.Link);

        var description = string.IsNullOrWhiteSpace(thing.Object.Description) ? "" : $"**Description:**\n{htmlparser.ParseDescription(thing)}\n";
        switch (thing.Object.Type)
        {
            case "METHOD":
                var parameters = thing.Object.Metadata.ParameterDescriptions.Count > 0 ? $"**Parameters:**\n{string.Join("\n", thing.Object.Metadata.ParameterDescriptions.Select(p => $"`{p.Key}` - {p.Value}"))}\n" : "";
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
                                      $"{string.Join(" ", thing.Object.Modifiers)} class {thing.Object.Name}{extends}{implements}\n" +
                                      "```\n" +
                                      description);
                break;
            }
            case "FIELD":
                embed.WithDescription("```java\n" +
                                      $"{string.Join("", thing.Object.Modifiers)} {thing.Object.Metadata.Returns} {thing.Object.Name}\n" +
                                      "```\n" +
                                      description);
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
        var name = thing.Object.Type switch
        {
            "METHOD" => $"{package}{thing.Object.Metadata.Owner}#{thing.Object.Name}{(withParameters ? $"({string.Join(", ", thing.Object.Metadata.Parameters)})" : "")}",
            "CLASS" => $"{package}{thing.Object.Name}",
            "FIELD" => $"{package}{thing.Object.Metadata.Owner}%{thing.Object.Name}",
            _ => thing.Name
        };

        return name.SubstringIgnoreError(80, true);
    }

    public class SearchAutocompleteHandler : AutocompleteHandler
    {
        private readonly IDocDexApiClient apiClient;
        private readonly ILogger<SearchAutocompleteHandler> logger;

        public SearchAutocompleteHandler(IDocDexApiClient apiClient, ILogger<SearchAutocompleteHandler> logger)
        {
            this.apiClient = apiClient;
            this.logger = logger;
        }
        
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            var javadoc = autocompleteInteraction.Data.Options.FirstOrDefault(n => n.Name == "javadoc")?.Value as string;
            var query = autocompleteInteraction.Data.Current.Value as string;
            if (string.IsNullOrWhiteSpace(query))
                return AutocompletionResult.FromSuccess();

            var result = await apiClient.Search(string.IsNullOrWhiteSpace(javadoc) ? "gdx" : javadoc, query);
            
            return AutocompletionResult.FromSuccess(result.Select(o => new AutocompleteResult($"{GetFullName(o, true, false)} ({o.Object.Type})", GetFullName(o, true))).Take(25));
        }
    }
    
    public class JavadocAutocompleteHandler : AutocompleteHandler
    {
        private readonly IDocDexApiClient apiClient;
        private readonly ILogger<JavadocAutocompleteHandler> logger;

        public JavadocAutocompleteHandler(IDocDexApiClient apiClient, ILogger<JavadocAutocompleteHandler> logger)
        {
            this.apiClient = apiClient;
            this.logger = logger;
        }
        
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            var result = await apiClient.GetJavaDocs();
            
            var query = autocompleteInteraction.Data.Current.Value as string;
            if (!string.IsNullOrWhiteSpace(query))
                result = result.Where(r => r.Names.Any(n => n.ToLower().Contains(query.ToLower()))).ToArray();
                
            return AutocompletionResult.FromSuccess(result.Select(o => new AutocompleteResult(o.Names.First(), o.Names.First())).Take(25));
        }
    }
}