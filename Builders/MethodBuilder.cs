using Discord;
using DocDexBot.Net.Api.Models;

namespace DocDexBot.Net.Builders;

public class MethodBuilder : ObjectBuilder
{
    public MethodBuilder(ObjectModel model) : base(model)
    {
    }

    public override string GetTitleName() => 
        $"{Model.Package}.{Model.Metadata.Owner}#{Model.Name}";

    public override string GetJavaBlockHeader() => 
        $"{Model.Metadata.Returns} {Model.Name}({string.Join(", ", Model.Metadata.Parameters)}) {{";

    public override Color GetEmbedColor() => 
        Color.Purple;
}