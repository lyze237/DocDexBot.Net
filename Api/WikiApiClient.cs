using DocDexBot.Net.Extensions;
using DocDexBot.Net.Interactions;
using DocDexBot.Net.Interactions.AutocompleteHandlers;
using DocDexBot.Net.Options;
using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace DocDexBot.Net.Api;

public class WikiApiClient : IWikiApiClient
{
    private readonly HtmlWeb webClient = new();
    
    private readonly IMemoryCache cache;
    
    private readonly Uri wikiUrl;
    private readonly Uri sourceWikiUrl;

    public WikiApiClient(IMemoryCache cache, IOptions<ApiOptions> apiOptions)
    {
        this.cache = cache;
        
        wikiUrl = new Uri(apiOptions.Value.WikiUrl);
        sourceWikiUrl = new Uri(apiOptions.Value.SourceWikiUrl);
    }

    public async Task<HtmlDocument> GetMainWikiPage() =>
        (await cache.GetOrCreateAsync("wikiurl", async _ => await webClient.LoadFromWebAsync(wikiUrl.ToString())))!;
    
    public async Task<HtmlNode[]> GetMainWikiPageAnchors() =>
        (await GetMainWikiPage()).DocumentNode.SelectNodes("//section[contains(@class, 'page__content')]//li/a")!.ToArray();

    public async Task<HtmlNode[]> GetMainWikiPageList() =>
        (await GetMainWikiPage()).DocumentNode.SelectNodes("//section[contains(@class, 'page__content')]/ul/li")!.ToArray();

    public async Task<WikiLink[]> GetMainWikiPageWikiLinks() =>
        (await GetMainWikiPageList())
        .Select(MapHtmlNodeToWikiLink)
        .ToArray();

    private WikiLink MapHtmlNodeToWikiLink(HtmlNode node)
    {
        var link = new WikiLink(node.SelectSingleNode("a")?.GetDirectInnerText() ?? node.GetDirectInnerText(), node.SelectSingleNode("a")?.Attributes["href"].Value);
        
        link.AddChildren(node.SelectNodes("ul/li")?.Select(MapHtmlNodeToWikiLink) ?? new List<WikiLink>());

        return link;
    }
    
    public async Task<HtmlDocument> GetWikiPage(string href) => 
        (await cache.GetOrCreateAsync($"wiki_{href}_html", async _ => await webClient.LoadFromWebAsync(wikiUrl.GetAbsoluteUrlString(href))))!;
    
    public async Task<(HtmlNode[] sections, int header)> GetWikiPageSections(string href)
    {
        var doc = (await GetWikiPage(href)).DocumentNode;

        for (var headers = 1; headers <= 6; headers++)
        {
            var hs = doc.SelectNodes($"//section[contains(@class, 'page__content')]/h{headers}");
            if (hs is { Count: > 0 })
                return (hs.ToArray(), headers);
        }

        return (Array.Empty<HtmlNode>(), 1);
    }

    public async Task<HtmlDocument> GetMarkdownPage(string mainWikiHref) =>
        (await cache.GetOrCreateAsync($"wiki_{mainWikiHref}_md", async _ => await webClient.LoadFromWebAsync(sourceWikiUrl.GetAbsoluteUrlString(mainWikiHref[1..] + ".md"))))!;

    public Uri GetWikiUrl() => 
        wikiUrl;

    public Uri GetSourceWikiUrl() =>
        sourceWikiUrl;
}