using Discord;
using DocDexBot.Net.Api.Models;
using DocDexBot.Net.Api.Parsers;

namespace DocDexBot.Net.Builders;

public class ConstructorBuilder : ObjectBuilder
{
    public ConstructorBuilder(ObjectModel model, IDiscordTextFixer discordTextFixer) : base(model, discordTextFixer)
    {
    }

    public override string GetTitleName() => 
        $"{Model.Package}{Model.Metadata.Owner}#{Model.Name}";

    public override string GetJavaBlockHeader() => 
        $"{string.Join(" ", Model.Modifiers)}({string.Join(", ", Model.Metadata.Parameters)}) {Model.Name} {{";

    public override Color GetEmbedColor() => 
        Color.Teal;
}