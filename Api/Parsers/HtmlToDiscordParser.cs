using System.Security.AccessControl;
using System.Text.RegularExpressions;
using DocDexBot.Net.Api.Models;
using DocDexBot.Net.Extensions;

namespace DocDexBot.Net.Api.Parsers;

public partial class HtmlToDiscordParser
{
    // Yeah yeah I know don't parse html with regex
    private readonly Regex anchorRegex = AnchorRegex();
    private readonly Regex codeRegex = CodeRegex();
    private readonly Regex boldRegex = BoldRegex();
    private readonly Regex italicsRegex = ItalicsRegex();
    private readonly Regex paragraphRegex = ParagraphRegex();
    private readonly Regex listRegex = ListRegex();
    
    public string ParseDescription(SearchResult obj)
    {
        var description = obj.Object.Description;
        
        var matches = anchorRegex.Matches(description);
        for (var index = matches.Count - 1; index >= 0; index--)
        {
            var match = matches[index];

            var href = match.Groups["href"].Value;
            var uri = new Uri(new Uri(obj.Object.Link), href);

            description = description.ReplaceViaIndex($"[{match.Groups["innerhtml"].Value}]({uri.AbsoluteUri})", match.Index, match.Length);
        }

        description = description.Replace("*", "\\*").Replace("`", "\\`");
        
        description = codeRegex.Replace(description, "`${innerhtml}`");
        description = boldRegex.Replace(description, "**${innerhtml}**");
        description = italicsRegex.Replace(description, "*${innerhtml}*");
        description = paragraphRegex.Replace(description, "${innerhtml}\n");

        description = description.Replace("<ul>", "\n").Replace("</ul>", "\n");
        description = listRegex.Replace(description, "* ${innerhtml}");

        Console.WriteLine(description);
        
        return description.Trim();
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
}