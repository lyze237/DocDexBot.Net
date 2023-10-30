using System.Text.RegularExpressions;
using DocDexBot.Net.Extensions;

namespace DocDexBot.Net.Api.Parsers;

public partial class HtmlToDiscordParser
{
    public string ParseDescription(Uri baseUri, string description)
    {
        var matches = AnchorRegex().Matches(description);
        for (var index = matches.Count - 1; index >= 0; index--)
        {
            var match = matches[index];

            var href = match.Groups["href"].Value;
            var uri = new Uri(baseUri, href);

            description = description.ReplaceViaIndex($"[{match.Groups["innerhtml"].Value}]({uri.AbsoluteUri})", match.Index, match.Length);
        }

        description = description.Replace("*", "\\*").Replace("`", "\\`");
        
        description = CodeRegex().Replace(description, "`${innerhtml}`");
        description = BoldRegex().Replace(description, "**${innerhtml}**");
        description = ItalicsRegex().Replace(description, "*${innerhtml}*");
        description = ParagraphRegex().Replace(description, "${innerhtml}\n");

        description = description.Replace("<ul>", "").Replace("</ul>", "\n");
        description = ListRegex().Replace(description, "* ${innerhtml}");

        Console.WriteLine(description);
        
        description = CleanupMultilinesRegex().Replace(description, "\n");
        
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
    [GeneratedRegex(@"(^\p{Zs}*(\r\n|\n)){2,}", RegexOptions.Multiline)]
    private static partial Regex CleanupMultilinesRegex();
}