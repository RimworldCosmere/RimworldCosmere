#nullable enable
using System.Collections.Generic;
using Verse;

namespace CosmereCore.Defs {
    public class ShardDef : Def {
        public List<ShardDef> mutuallyExclusiveWith = new List<ShardDef>();
        public string? planet;
    }
}