using Discord;
using DocDexBot.Net.Api.Models;

namespace DocDexBot.Net.Builders;

public class ConstructorBuilder : ObjectBuilder
{
    public ConstructorBuilder(ObjectModel model) : base(model)
    {
    }

    public override string GetTitleName() => 
        $"{Model.Package}{Model.Metadata.Owner}#{Model.Name}";

    public override string GetJavaBlockHeader() => 
        $"{string.Join(" ", Model.Modifiers)}({string.Join(", ", Model.Metadata.Parameters)}) {Model.Name} {{";

    public override Color GetEmbedColor() => 
        Color.Teal;
}