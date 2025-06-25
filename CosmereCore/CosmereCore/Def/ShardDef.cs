using System.Collections.Generic;

namespace CosmereCore.Def;

public class ShardDef : Verse.Def {
    public List<ShardDef> mutuallyExclusiveWith = new List<ShardDef>();
    public string? planet;
}