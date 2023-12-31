﻿using Discord;
using Discord.Interactions;
using DocDexBot.Net.Api;
using DocDexBot.Net.Api.Parsers;
using DocDexBot.Net.Extensions;
using DocDexBot.Net.Interactions.AutocompleteHandlers;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

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
    public async Task Search([Summary("Page"), Autocomplete(typeof(WikiSearchAutocompleteHandler))] string pageNumberString, [Summary("Section"), Autocomplete(typeof(WikiSearchSectionAutocompleteHandler))] string section = "header")
    {
        if (!int.TryParse(pageNumberString, out var pageNumber) || (section != "header" && section.Count(c => c == '~') != 3))
        {
            throw new ArgumentException("You didn't use autocomplete >:C");
        }

        await DeferAsync();
        
        var wikiLinks = await wikiApiClient.GetMainWikiPageWikiLinks();
        var entry = wikiLinks.SelectMany(w => w.GetAllChildren()).ToList()[pageNumber];
        var entryHref = entry.Link!;

        if (section == "header")
        {
            var (_, header) = await wikiApiClient.GetWikiPageSections(entry.Link!);
            section = $"{pageNumberString}~{header}";
        }

        var isHeader = section.Count(c => c == '~') != 3;
        var sectionSplit = section.Split("~");
        var sectionId = isHeader ? null : sectionSplit[1];
        var sectionAnchor = isHeader ? "" : $"#{sectionId}";
        var sectionIndex = isHeader ? null : sectionSplit[2];
        var sectionHeader = isHeader ? sectionSplit[1] : sectionSplit[3];
        

        var mdPage = await wikiApiClient.GetMarkdownPage(entryHref);

        var (title, unparsedText) = FindCorrectSection(discordTextFixer.PreParseMd(mdPage.Text), sectionHeader, isHeader, sectionIndex);
        var (parsedText, images) = discordTextFixer.ParseMd(wikiApiClient.GetWikiUrl(), unparsedText);
        
        var embeds = new List<Embed>();
        
        var embed = new EmbedBuilder()
            .WithTitle(title.Trim('"'))
            .WithUrl(wikiApiClient.GetWikiUrl().GetAbsoluteUrlString($"{entryHref}{sectionAnchor}"))
            .WithColor(Color.DarkMagenta)
            .WithDescription(parsedText.SubstringIgnoreError(512, true));

        if (images.Count > 0)
            embed.WithImageUrl(images.First());
        
        embeds.Add(embed.Build());
        embeds.AddRange(images.Skip(1).Take(4).Select(otherImage => new EmbedBuilder()
            .WithTitle(embed.Title)
            .WithUrl(embed.Url)
            .WithImageUrl(otherImage).Build()));

        await FollowupAsync("", embeds: embeds.ToArray());
    }

    private static (string title, string text) FindCorrectSection(string searchText, string sectionHeader, bool isHeader, string? sectionIndex)
    {
        var lines = searchText.Split("\n");
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

        if (text.Trim().Length == 0)
            return FindCorrectSection(searchText, sectionHeader, false, "0");
        
        return (title, text);
    }
}
