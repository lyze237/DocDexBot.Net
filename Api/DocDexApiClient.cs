using DocDexBot.Net.Api.Models;
using DocDexBot.Net.Options;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Serializers.Json;

namespace DocDexBot.Net.Api;

public class DocDexApiClient : IDocDexApiClient, IDisposable
{
    private readonly RestClient client;

    public DocDexApiClient(IOptions<ApiOptions> options)
    {
        client = new RestClient(options.Value.Url, configureSerialization: s => s.UseSystemTextJson());
    }

    public async Task<Javadoc[]> GetJavaDocs()
    {
        var response = await client.GetJsonAsync<Javadoc[]>("javadocs");
        return response!;
    }

    public async Task<SearchResult[]> Search(string javadoc, string query)
    {
        query = query.Replace("%", "-").Replace("#", "~");
        
        var response = await client.GetJsonAsync<SearchResult[]>($"index?javadoc={javadoc}&query={query}", new
        {
            javadoc, query
        });

        return response!;
    }

    public void Dispose() => 
        client.Dispose();
}