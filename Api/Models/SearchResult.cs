using System.Text.Json.Serialization;

namespace DocDexBot.Net.Api.Models;

public class SearchResult
{
        public SearchResult(string name, ObjectModel @object)
        {
            Name = name;
            Object = @object;
        }

        [JsonPropertyName("name")]
        public string Name { get; }

        [JsonPropertyName("object")]
        public ObjectModel Object { get; }
}