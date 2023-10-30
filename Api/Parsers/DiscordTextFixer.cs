using System.Text.RegularExpressions;
using DocDexBot.Net.Extensions;

namespace DocDexBot.Net.Api.Parsers;

public partial class DiscordTextFixer : IDiscordTextFixer
{
    public string ParseHtml(Uri baseUri, string html)
    {
        var matches = AnchorRegex().Matches(html);
        for (var index = matches.Count - 1; index >= 0; index--)
        {
            var match = matches[index];

            var href = match.Groups["href"].Value;
            var uri = new Uri(baseUri, href);

            html = html.ReplaceViaIndex($"[{match.Groups["innerhtml"].Value}]({uri.AbsoluteUri})", match.Index, match.Length);
        }

        html = html.Replace("*", "\\*").Replace("`", "\\`");
        
        html = CodeRegex().Replace(html, "`${innerhtml}`");
        html = BoldRegex().Replace(html, "**${innerhtml}**");
        html = ItalicsRegex().Replace(html, "*${innerhtml}*");
        html = ParagraphRegex().Replace(html, "${innerhtml}\n");

        html = html.Replace("<ul>", "").Replace("</ul>", "\n");
        html = ListRegex().Replace(html, "* ${innerhtml}");

        html = CleanupMultilinesRegex().Replace(html, "\n");
        
        return html.Trim();
    }

    public (string md, List<string> images) ParseMd(Uri baseUri, string md)
    {
        var images = new List<string>();
        var matches = MdLinkRegex().Matches(md);
        for (var i = matches.Count - 1; i >= 0; i--)
        {
            var match = matches[i];

            var href = match.Groups["link"].Value;
            var uri = new Uri(baseUri, href);

            if (match.Groups["isimage"].Success) 
                images.Add(uri.ToString());

            md = md.ReplaceViaIndex($"[{match.Groups["text"].Value}]({uri.AbsoluteUri})", match.Index, match.Length);
        }

        md = MdFixStrangeTitle().Replace(md, "## ${title}");
        md = MdIncludeRegex().Replace(md, "");
        md = MdNoticeRegex().Replace(md, "");
        md = MdHeaderRegex().Replace(md, "### ");

        md = CleanupMultilinesRegex().Replace(md, "\n");
        return (md, images);
    }

    [GeneratedRegex("<a(\\s*)href=\\\"(?<href>.*?)\\\".*?>(?<innerhtml>.*?)<\\/a>")]
    private static partial Regex AnchorRegex();
    
    [GeneratedRegex("<code>(?<innerhtml>.*?)</code>")]
    private static partial Regex CodeRegex();
    
    [GeneratedRegex("<b>(?<innerhtml>.*?)</b>")]
    private static partial Regex BoldRegex();
    
    [GeneratedRegex("<i>(?<innerhtml>.*?)</i>")]
    private static partial Regex ItalicsRegex();
    
    [GeneratedRegex("<p>(?<innerhtml>.*?)</p>")]
    private static partial Regex ParagraphRegex();
    
    [GeneratedRegex("\\s?<li>(?<innerhtml>.*?)</li>")]
    private static partial Regex ListRegex();
    [GeneratedRegex(@"(^\p{Zs}*(\r\n|\n)){2,}", RegexOptions.Multiline)]
    private static partial Regex CleanupMultilinesRegex();

    [GeneratedRegex(@"(?<isimage>!)?\[(?<text>.*?)\]\((?<link>.*?)\)")]
    private static partial Regex MdLinkRegex();

    [GeneratedRegex("{% .*? %}")]
    private static partial Regex MdIncludeRegex();

    [GeneratedRegex(@"{: \..*?}")]
    private static partial Regex MdNoticeRegex();
    
    [GeneratedRegex(@"^(#){4,} ", RegexOptions.Multiline)]
    private static partial Regex MdHeaderRegex();
    [GeneratedRegex("^(?<title>.*?)\\n-----$", RegexOptions.Multiline)]
    private static partial Regex MdFixStrangeTitle();
}