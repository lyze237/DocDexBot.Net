namespace DocDexBot.Net.Extensions;

public static class ClassExtensions
{
    public static void PrintAllProperties(this Type type, object obj)
    {
        foreach (var property in type.GetProperties())
        {
            if (property.Name.Contains("Token"))
                continue;
            
            Console.WriteLine($"{property.Name}: {property.GetValue(obj)}");
        }
    }
}