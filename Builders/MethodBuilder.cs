using Discord;
using DocDexBot.Net.Api.Models;
using DocDexBot.Net.Api.Parsers;

namespace DocDexBot.Net.Builders;

public class MethodBuilder : ObjectBuilder
{
    public MethodBuilder(ObjectModel model, IDiscordTextFixer discordTextFixer) : base(model, discordTextFixer)
    {
    }

    public override string GetTitleName() => 
        $"{Model.Package}.{Model.Metadata.Owner}#{Model.Name}";

    public override string GetJavaBlockHeader() => 
        $"{Model.Metadata.Returns} {Model.Name}({string.Join(", ", Model.Metadata.Parameters)}) {{";

    public override Color GetEmbedColor() => 
        Color.Purple;
}