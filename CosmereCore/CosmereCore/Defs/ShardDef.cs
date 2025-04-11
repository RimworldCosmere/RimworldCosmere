using System.Collections.Generic;
using Verse;

namespace CosmereCore.Defs
{
    public class ShardDef : Def
    {
        public List<ShardDef> mutuallyExclusiveWith = new();
        public string? planet;
    }
}