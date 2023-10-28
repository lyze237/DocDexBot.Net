using Discord;
using DocDexBot.Net.Api.Models;

namespace DocDexBot.Net.Builders;

public class ClassBuilder : ObjectBuilder
{
    public ClassBuilder(ObjectModel model) : base(model)
    {
    }

    public override string GetTitleName() => 
        $"{Model.Package}.{Model.Name}";

    public override string GetJavaBlockHeader()
    {
        var extends = Model.Metadata.Extensions.Contains("java.lang.Object") ? "" : $" extends {string.Join(", ", RemovePackage(Model.Metadata.Extensions))}";
        var implements = Model.Metadata.Implementations.Count == 0 ? "" : $" implements {string.Join(", ", RemovePackage(Model.Metadata.Implementations))}";
        return $"{string.Join(" ", Model.Modifiers)} class {Model.Name}{extends}{implements} {{";
    }

    public override Color GetEmbedColor() => 
        Color.Blue;
}