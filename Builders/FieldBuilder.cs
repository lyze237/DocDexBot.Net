using Discord;
using DocDexBot.Net.Api.Models;
using DocDexBot.Net.Api.Parsers;

namespace DocDexBot.Net.Builders;

public class FieldBuilder : ObjectBuilder
{
    public FieldBuilder(ObjectModel model, IDiscordTextFixer discordTextFixer) : base(model, discordTextFixer)
    {
    }

    public override string GetTitleName() =>
        $"{Model.Package}.{Model.Metadata.Owner}%{Model.Name}";

    public override string GetJavaBlockHeader() =>
        $"{string.Join(" ", Model.Modifiers)} {Model.Metadata.Returns} {Model.Name};";

    public override Color GetEmbedColor() =>
        Color.Red;

    public override string GenerateJavaBlock(int methods, int fields)
    {
        var str = "```java\n";

        str += GetJavaBlockHeader();
        
        str += "\n```";

        return str;
    }
}