﻿using DocDexBot.Net.Interactions;
using HtmlAgilityPack;

namespace DocDexBot.Net.Api;

public interface IWikiApiClient
{
    public Task<HtmlDocument> GetMainWikiPage();
    public Task<HtmlNode[]> GetMainWikiPageAnchors();
    public Task<HtmlNode[]> GetMainWikiPageList();
    public Task<WikiLink[]> GetMainWikiPageWikiLinks();

    public Task<HtmlDocument> GetWikiPage(string href);
    public Task<(HtmlNode[] sections, int header)> GetWikiPageSections(string href);
    public Task<HtmlDocument> GetMarkdownPage(string mainWikiHref);

    public Uri GetWikiUrl();
    public Uri GetSourceWikiUrl();
}