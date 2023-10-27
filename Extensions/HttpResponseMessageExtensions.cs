namespace DocDexBot.Net.Extensions;

public static class HttpResponseMessageExtensions
{
    public static async Task<Exception> ToException(this HttpResponseMessage message)
    {
        var content = "<Couldn't read content>";
        try
        {
            content = await message.Content.ReadAsStringAsync();
        }
        catch (Exception)
        {
            // ignored
        }

        return new HttpRequestException($"{message.RequestMessage?.RequestUri}: {message.ReasonPhrase}: {content}", null, message.StatusCode);
    }
}