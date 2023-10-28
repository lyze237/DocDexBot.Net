using Discord;
using DocDexBot.Net.Api.Models;

namespace DocDexBot.Net.Builders;

public class InterfaceBuilder : ObjectBuilder
{
    public InterfaceBuilder(ObjectModel model) : base(model)
    {
    }

    public override string GetTitleName() => 
        $"{Model.Package}.{Model.Name}";

    public override string GetJavaBlockHeader() => 
        $"{string.Join(" ", Model.Modifiers)} interface {Model.Name} {{";

    public override Color GetEmbedColor() => 
        Color.Green;
}