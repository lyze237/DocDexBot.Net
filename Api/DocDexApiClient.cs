using DocDexBot.Net.Api.Models;
using DocDexBot.Net.Options;
using Microsoft.Extensions.Options;
using RestSharp;

namespace DocDexBot.Net.Api;

public class DocDexApiClient : IDocDexApiClient, IDisposable
{
    private readonly RestClient client;

    public DocDexApiClient(IOptions<ApiOptions> options) => 
        client = new RestClient(options.Value.Url);

    public async Task<Javadoc[]> GetJavaDocs()
    {
        var response = await client.GetJsonAsync<Javadoc[]>("javadocs");
        return response!;
    }

    public void Dispose() => 
        client.Dispose();
}