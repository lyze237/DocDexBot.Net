namespace DocDexBot.Net.Extensions;

public static class UriExtensions
{
    public static string GetAbsoluteUrlString(this Uri baseUrl, string url)
    {
        var uri = new Uri(url, UriKind.RelativeOrAbsolute);
        
        if (!uri.IsAbsoluteUri)
            uri = new Uri(baseUrl, uri);
        
        return uri.ToString();
    }
}