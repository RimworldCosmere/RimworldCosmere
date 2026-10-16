namespace Cosmere.Core.Def;

public class ShardDef : Verse.Def {
    public List<ShardDef> mutuallyExclusiveWith = [];
    public string? planet;
}
