using Discord;
using DocDexBot.Net.Api.Models;
using DocDexBot.Net.Api.Parsers;
using DocDexBot.Net.Extensions;

namespace DocDexBot.Net.Builders;

public abstract class ObjectBuilder
{
    private readonly IDiscordTextFixer discordTextFixer;
    
    protected readonly ObjectModel Model;

    public IEnumerable<string> GetTrimmedFields =>
        Model.Metadata.Fields.Select(f => f[(f.LastIndexOf("%", StringComparison.Ordinal) + 1)..]);
    
    public IEnumerable<string> GetTrimmedMethods =>
        Model.Metadata.Methods.Select(m => m[(m.LastIndexOf("#", StringComparison.Ordinal) + 1)..]);

    public IEnumerable<string> GetTrimmedParameterDescriptions =>
        Model.Metadata.ParameterDescriptions.Select(p => $"`{p.Key}` - {p.Value}");

    public string ParsedDescription =>
        discordTextFixer.ParseHtml(new Uri(Model.Link), Model.Description);

    protected ObjectBuilder(ObjectModel model, IDiscordTextFixer discordTextFixer) =>
        (Model, this.discordTextFixer) = (model, discordTextFixer);
    
    public abstract string GetTitleName();
    public abstract string GetJavaBlockHeader();
    public abstract Color GetEmbedColor();

    public EmbedBuilder Build()
    {
        var embed = new EmbedBuilder();
        embed.WithTitle(GetTitleName())
            .WithUrl(Model.Link)
            .WithColor(GetEmbedColor())
            .WithDescription(GenerateEmbedDescription());

        if (Model.Metadata.Methods.Count > 0)
            embed.AddField("Methods", Model.Metadata.Methods.Count, true);
        if (Model.Metadata.Fields.Count > 0)
            embed.AddField("Fields", Model.Metadata.Fields.Count, true);
        if (Model.Metadata.SubClasses.Count > 0)
            embed.AddField("SubClasses", Model.Metadata.SubClasses.Count, true);
        
        return embed;
    }

    private string GenerateEmbedDescription()
    {
        var methods = Model.Metadata.Methods.Count;
        var fields = Model.Metadata.Fields.Count;

        string fullDescription;
        do
        {
            fullDescription = GenerateJavaBlock(methods, fields);
            fullDescription += ParsedDescription.SubstringIgnoreError(2048, true);
            fullDescription += string.Join("\n", GetTrimmedParameterDescriptions).SubstringIgnoreError(1024, true);
            if (!string.IsNullOrWhiteSpace(Model.Metadata.ReturnsDescription))
                fullDescription += string.Join("\n", "Returns " + Model.Metadata.ReturnsDescription).SubstringIgnoreError(128, true);

            fields = Math.Max(fields - 1, 0);
            methods = Math.Max(methods - 1, 0);
        } while (fullDescription.Length > 4096);

        return fullDescription;
    }
    
    public virtual string GenerateJavaBlock(int methods, int fields)
    {
        var str = "```java\n";

        str += GetJavaBlockHeader();
        if (Model.Metadata.Fields.Count > 0)
            str += $"\n\tFields: {string.Join(", ", GetTrimmedFields.Take(fields))}{(fields < Model.Metadata.Fields.Count ? "..." : "")};\n";
        if (Model.Metadata.Methods.Count > 0)
            str += $"\n\tMethods: {string.Join(", ", GetTrimmedMethods.Take(methods))}{(methods < Model.Metadata.Methods.Count ? "..." : "")};\n";
        
        str += "}\n```\n";

        return str;
    }
    
    protected static IEnumerable<string> RemovePackage(IEnumerable<string> names) =>
        names.Select(RemovePackage).ToList();

    protected static string RemovePackage(string name) => 
        name.Contains('.') ? name[(name.LastIndexOf('.') + 1)..] : name;
}