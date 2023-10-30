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
        await DeferAsync();
        var pageNumber = Convert.ToInt32(pageNumberString);

        var isHeader = section.Count(c => c == '~') != 3;
        var sectionSplit = section.Split("~");
        var sectionId = isHeader ? null : sectionSplit[1];
        var sectionAnchor = isHeader ? "" : $"#{sectionId}";
        var sectionIndex = isHeader ? null : sectionSplit[2];
        var sectionHeader = isHeader ? sectionSplit[1] : sectionSplit[3];
        
        var wikiLinks = await wikiApiClient.GetMainWikiPageWikiLinks();
        var entry = wikiLinks.SelectMany(w => w.GetAllChildren()).ToList()[pageNumber];
        var entryHref = entry.Link!;

        var mdPage = await wikiApiClient.GetMarkdownPage(entryHref);

        var parsed = discordTextFixer.ParseMd(wikiApiClient.GetWikiUrl(), mdPage.Text);
        
        var lines = parsed.md.Split("\n");
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
        
        var embeds = new List<Embed>();
        
        var embed = new EmbedBuilder()
            .WithTitle(title.Trim('"'))
            .WithUrl(wikiApiClient.GetWikiUrl().GetAbsoluteUrlString($"{entryHref}{sectionAnchor}"))
            .WithColor(Color.DarkMagenta)
            .WithDescription(text.SubstringIgnoreError(1024, true));

        if (parsed.images.Count > 0)
            embed.WithImageUrl(parsed.images.First());
        
        embeds.Add(embed.Build());
        embeds.AddRange(parsed.images.Skip(1).Take(4).Select(otherImage => new EmbedBuilder()
            .WithTitle(embed.Title)
            .WithUrl(embed.Url)
            .WithImageUrl(otherImage).Build()));

        await FollowupAsync("", embeds: embeds.ToArray());
    }
}
