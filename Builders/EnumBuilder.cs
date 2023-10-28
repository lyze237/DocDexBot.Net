using Discord;
using DocDexBot.Net.Api.Models;

namespace DocDexBot.Net.Builders;

public class EnumBuilder : ObjectBuilder
{
    public EnumBuilder(ObjectModel model) : base(model)
    {
    }

    public override string GetTitleName() => 
        $"{Model.Package}.{Model.Name}";

    public override string GetJavaBlockHeader() => 
        $"{string.Join(" ", Model.Modifiers)} enum {Model.Name} {{";

    public override Color GetEmbedColor() => 
        Color.Orange;
}