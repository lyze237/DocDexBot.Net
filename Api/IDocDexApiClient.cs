using DocDexBot.Net.Api.Models;

namespace DocDexBot.Net.Api;

public interface IDocDexApiClient
{
    Task<Javadoc[]> GetJavaDocs();
}