namespace DocDexBot.Net.Extensions;

public static class TimeSpanExtensions
{
    public static string ToFormattedString(this TimeSpan ts)
    {
        const string separator = ", ";

        if (ts.TotalMilliseconds < 1) { return "No time"; }

        return string.Join(separator, new[]
        {
            ts.Days > 0 ? ts.Days + (ts.Days > 1 ? " days" : " day") : null,
            ts.Hours > 0 ? ts.Hours + (ts.Hours > 1 ? " hours" : " hour") : null,
            ts.Minutes > 0 ? ts.Minutes + (ts.Minutes > 1 ? " minutes" : " minute") : null,
            ts.Seconds > 0 ? ts.Seconds + (ts.Seconds > 1 ? " seconds" : " second") : null,
            ts.Milliseconds > 0 ? ts.Milliseconds + (ts.Milliseconds > 1 ? " milliseconds" : " millisecond") : null,
        }.Where(t => t != null));
    }
}