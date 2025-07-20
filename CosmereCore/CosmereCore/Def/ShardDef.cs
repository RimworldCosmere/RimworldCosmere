using System.Collections.Generic;

namespace Cosmere.Core.Def;

public class ShardDef : Verse.Def {
    public List<ShardDef> mutuallyExclusiveWith = new List<ShardDef>();
    public string? planet;
}