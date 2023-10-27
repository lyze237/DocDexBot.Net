using DocDexBot.Net.Api.Models;

namespace DocDexBot.Net.Api;

public interface IDocDexApiClient
{
    Task<Javadoc[]> GetJavaDocs();

    Task<SearchResult[]> Search(string javadoc, string query);
}