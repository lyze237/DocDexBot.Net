namespace DocDexBot.Net.Extensions;

public static class StringExtensions
{
    public static string SubstringIgnoreError(this string str, int length, bool append3Dots = false)
    {
        try
        {
            return append3Dots ? $"{str[..(length - 4)]}..." : str[..length];
        }
        catch (Exception)
        {
            return str;
        }
    }
    
    public static string SubstringIgnoreErrorFromBack(this string str, int length, bool append3Dots = false)
    {
        try
        {
            return append3Dots ? $"...{str[^(length - 4)..]}" : str[^length..];
        }
        catch (Exception)
        {
            return str;
        }
    }

    public static string ReplaceViaIndex(this string str, string with, int index, int length) => 
        str[..index] + with + str[(index + length)..];
}