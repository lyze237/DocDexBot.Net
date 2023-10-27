using System.Text.Json.Serialization;

namespace DocDexBot.Net.Api.Models;

public class Metadata
{
    [JsonPropertyName("extensions")] public IReadOnlyList<string> Extensions { get; set; }

    [JsonPropertyName("implementations")] public IReadOnlyList<string> Implementations { get; set; }

    [JsonPropertyName("all_implementations")] public IReadOnlyList<string> AllImplementations { get; set; }

    [JsonPropertyName("super_interfaces")] public IReadOnlyList<string> SuperInterfaces { get; set; }

    [JsonPropertyName("sub_interfaces")] public IReadOnlyList<string> SubInterfaces { get; set; }

    [JsonPropertyName("sub_classes")] public IReadOnlyList<string> SubClasses { get; set; }

    [JsonPropertyName("implementing_classes")] public IReadOnlyList<string> ImplementingClasses { get; set; }

    [JsonPropertyName("methods")] public IReadOnlyList<string> Methods { get; set; }

    [JsonPropertyName("fields")] public IReadOnlyList<string> Fields { get; set; }

    [JsonPropertyName("owner")] public string Owner { get; set; }

    [JsonPropertyName("parameters")] public IReadOnlyList<string> Parameters { get; set; } = new List<string>();

    [JsonPropertyName("parameter_descriptions")] public Dictionary<string, string> ParameterDescriptions { get; set; } = new();

    [JsonPropertyName("returns")] public string Returns { get; set; }

    [JsonPropertyName("returns_description")] public string ReturnsDescription { get; set; }

    [JsonPropertyName("throws")] public IReadOnlyList<string> Throws { get; set; }
}