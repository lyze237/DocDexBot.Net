using DocDexBot.Net.Api.Models;
using DocDexBot.Net.Options;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Serializers.Json;

namespace DocDexBot.Net.Api;

public class DocDexApiClient : IDocDexApiClient, IDisposable
{
    private readonly RestClient client;
    private readonly ILogger<DocDexApiClient> logger;

    public DocDexApiClient(IOptions<ApiOptions> options, ILogger<DocDexApiClient> logger)
    {
        client = new RestClient(options.Value.Url, configureSerialization: s => s.UseSystemTextJson());
        this.logger = logger;
    }

    public async Task<Javadoc[]> GetJavaDocs()
    {
        logger.LogInformation("Querying JavaDocs api");
        var response = await client.GetJsonAsync<Javadoc[]>("javadocs");
        return response!;
    }

    public async Task<SearchResult[]> Search(string javadoc, string query, int limit = 10)
    {
        logger.LogInformation("Querying Search api with {query} in {javadoc}", query, javadoc);
        
        query = query.Replace("%", "-").Replace("#", "~");
        
        var response = await client.GetJsonAsync<SearchResult[]>($"index?javadoc={javadoc}&limit={limit}&query={query}", new
        {
            javadoc, query, limit
        });

        return response!;
    }

    public void Dispose() => 
        client.Dispose();
}