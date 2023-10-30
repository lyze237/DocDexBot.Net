using Discord;
using DocDexBot.Net.Api.Models;
using DocDexBot.Net.Api.Parsers;

namespace DocDexBot.Net.Builders;

public class InterfaceBuilder : ObjectBuilder
{
    public InterfaceBuilder(ObjectModel model, IDiscordTextFixer discordTextFixer) : base(model, discordTextFixer)
    {
    }

    public override string GetTitleName() => 
        $"{Model.Package}.{Model.Name}";

    public override string GetJavaBlockHeader() => 
        $"{string.Join(" ", Model.Modifiers)} interface {Model.Name} {{";

    public override Color GetEmbedColor() => 
        Color.Green;
}