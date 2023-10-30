using Discord;
using Discord.Interactions;
using DocDexBot.Net.Api;
using DocDexBot.Net.Api.Parsers;
using DocDexBot.Net.Extensions;
using DocDexBot.Net.Interactions.AutocompleteHandlers;

namespace DocDexBot.Net.Interactions;

public class WikiSearchInteraction : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordTextFixer discordTextFixer;
    private readonly IWikiApiClient wikiApiClient;
    private readonly ILogger<WikiSearchInteraction> logger;
    
    public WikiSearchInteraction(IDiscordTextFixer discordTextFixer, IWikiApiClient wikiApiClient, ILogger<WikiSearchInteraction> logger)
    {
        this.discordTextFixer = discordTextFixer;
        this.wikiApiClient = wikiApiClient;
        this.logger = logger;
    }

    [SlashCommand("wiki", "Searches through the libGDX Wiki")]
    public async Task Search([Summary("Page"), Autocomplete(typeof(WikiSearchAutocompleteHandler))] string pageNumberString, [Summary("Section"), Autocomplete(typeof(WikiSearchSectionAutocompleteHandler))] string section)
    {
        var pageNumber = Convert.ToInt32(pageNumberString);

        var isHeader = !section.Contains('~');
        var sectionSplit = section.Split("~");
        var sectionId = isHeader ? null : sectionSplit[1];
        var sectionAnchor = isHeader ? "" : $"#{sectionId}";
        var sectionIndex = isHeader ? null : sectionSplit[2];
        var sectionHeader = isHeader ? null : sectionSplit[3];
        
        var entry = (await wikiApiClient.GetMainWikiPageLinks())[pageNumber];
        var entryHref = entry.Attributes["href"].Value;

        var mdPage = await wikiApiClient.GetMarkdownPage(entryHref);

        var lines = mdPage.Text.Split("\n");
        var currentTitle = -1;
        var tripleLine = 0;

        var text = "";
        var title = "";
        foreach (var line in lines)
        {
            if (line.StartsWith("---"))
            {
                tripleLine++;
                continue;
            }

            if (tripleLine == 1 && line.StartsWith("title:"))
                title = line.Split("title:")[1].Trim();

            // ignore header
            if (tripleLine < 2)
                continue;
            
            if (line.StartsWith($"{new string('#', Convert.ToInt32(sectionHeader))} "))
                currentTitle++;

            switch (isHeader)
            {
                case true when tripleLine == 2 && currentTitle == -1:
                case false when currentTitle == Convert.ToInt32(sectionIndex):
                    text += line + "\n";
                    break;
            }
        }

        var embed = new EmbedBuilder()
            .WithTitle(title.Trim('"'))
            .WithUrl(wikiApiClient.GetWikiUrl().GetAbsoluteUrlString($"{entryHref}{sectionAnchor}"))
            .WithColor(Color.DarkMagenta)
            .WithDescription(discordTextFixer.ParseMd(wikiApiClient.GetWikiUrl(), text).SubstringIgnoreError(3072, true));
        
        await RespondAsync("", embed: embed.Build());
    }
}
