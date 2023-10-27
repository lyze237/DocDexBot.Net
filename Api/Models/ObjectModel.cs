using System.Text.Json.Serialization;

namespace DocDexBot.Net.Api.Models;

public class ObjectModel
{
    [JsonPropertyName("link")]
    public string Link { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("package")]
    public string Package { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("stripped_description")]
    public string StrippedDescription { get; set; }

    [JsonPropertyName("annotations")]
    public IReadOnlyList<string> Annotations { get; set; }

    [JsonPropertyName("deprecated")]
    public bool Deprecated { get; set; }

    [JsonPropertyName("deprecation_message")]
    public string DeprecationMessage { get; set; }

    [JsonPropertyName("modifiers")]
    public IReadOnlyList<string> Modifiers { get; set; }

    [JsonPropertyName("since")]
    public string Since { get; set; }

    [JsonPropertyName("metadata")]
    public Metadata Metadata { get; set; }
}