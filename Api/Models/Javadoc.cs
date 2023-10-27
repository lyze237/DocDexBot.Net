using System.Text.Json.Serialization;

namespace DocDexBot.Net.Api.Models;

public class Javadoc
{
    [JsonPropertyName("names")] public List<string> Names { get; }

    [JsonPropertyName("link")] public string Link { get; }

    [JsonPropertyName("actual_link")] public string ActualLink { get; }

    public Javadoc(List<string> names, string link, string actualLink)
    {
        Names = names;
        Link = link;
        ActualLink = actualLink;
    }
}