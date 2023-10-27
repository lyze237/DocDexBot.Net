﻿namespace DocDexBot.Net.Extensions;

public static class StringExtensions
{
    public static string SubstringIgnoreError(this string str, int length, bool append3Dots = false)
    {
        try
        {
            if (append3Dots)
                return str[..(length - 4)] + "...";
            
            return str[..length];
        }
        catch (Exception)
        {
            return str;
        }
    }
}