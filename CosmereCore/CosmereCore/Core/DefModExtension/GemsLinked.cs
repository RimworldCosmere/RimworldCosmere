using Cosmere.Core.Def;

namespace Cosmere.Core.DefModExtension;

public class GemsLinked : Verse.DefModExtension {
    // ReSharper disable once InconsistentNaming
    public List<GemDef> Gems => gems ?? (gem != null ? [gem] : []);

    // Both are written by def xml, by field name; nothing in C# assigns them.
#pragma warning disable CS0649, S3459 // Field is never assigned to, and will always have its default value
    private GemDef? gem;
    private List<GemDef>? gems;
#pragma warning restore CS0649, S3459 // Field is never assigned to, and will always have its default value
}
