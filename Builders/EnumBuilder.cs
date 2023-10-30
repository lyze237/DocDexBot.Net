using Discord;
using DocDexBot.Net.Api.Models;
using DocDexBot.Net.Api.Parsers;

namespace DocDexBot.Net.Builders;

public class EnumBuilder : ObjectBuilder
{
    public EnumBuilder(ObjectModel model, IDiscordTextFixer discordTextFixer) : base(model, discordTextFixer)
    {
    }

    public override string GetTitleName() => 
        $"{Model.Package}.{Model.Name}";

    public override string GetJavaBlockHeader() => 
        $"{string.Join(" ", Model.Modifiers)} enum {Model.Name} {{";

    public override Color GetEmbedColor() => 
        Color.Orange;
}