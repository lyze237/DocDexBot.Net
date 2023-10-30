namespace DocDexBot.Net.Api.Parsers;

public interface IDiscordTextFixer
{
    public string ParseHtml(Uri baseUri, string html);
    public (string md, List<string> images) ParseMd(Uri baseUri, string md);
}