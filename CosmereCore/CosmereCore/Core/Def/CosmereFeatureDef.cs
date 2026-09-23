using System.Collections.Generic;
using Verse;

namespace Cosmere.Core.Def;

/// <summary>
///     One thing a world does: mists, highstorms, spren, ashfall. Named so a save can turn it on
///     or off, and so the conditions for it live in one place instead of being repeated at every
///     call site.
/// </summary>
public class CosmereFeatureDef : Verse.Def {
    /// <summary>The world this belongs to. Null means it belongs to no world in particular.</summary>
    public CosmereWorldDef? world;

    /// <summary>
    ///     At least one of these must exist for the feature to occur. Empty means the feature
    ///     does not depend on any Shard - ashfall is Rashek's engineering, not a Shard's doing.
    /// </summary>
    public List<ShardDef> anyOfShards = [];

    /// <summary>Whether it starts switched on for a save that can choose.</summary>
    public bool enabledByDefault = true;

    public int listOrder;

    public override IEnumerable<string> ConfigErrors() {
        foreach (string error in base.ConfigErrors()) {
            yield return error;
        }

        if (world == null && anyOfShards.Count == 0) {
            yield return "names neither a world nor a Shard, so nothing would ever gate it.";
        }
    }
}
