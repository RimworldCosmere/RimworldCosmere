using System.Collections.Generic;
using Cosmere.Core.Def;
using Verse;

namespace Cosmere.Core.Comp.Game;

/// <summary>
///     The world this save is set on. Written during colony creation, before world generation,
///     and read by everything that has to know which shardworld it is running on.
/// </summary>
public class CosmereWorld : GameComponent {
    public CosmereWorldDef? primary;

    /// <summary>
    ///     Per-feature overrides, keyed by CosmereFeatureDef.defName. Only consulted on a
    ///     cross-world save, where the player chooses which phenomena they want rather than the
    ///     world deciding. Absent means "use the def's default".
    /// </summary>
    public Dictionary<string, bool> featureOverrides = [];

    public CosmereWorld(Verse.Game game) { }

    public override void ExposeData() {
        Scribe_Defs.Look(ref primary, "primaryWorld");
        Scribe_Collections.Look(ref featureOverrides, "featureOverrides", LookMode.Value, LookMode.Value);
        featureOverrides ??= [];
    }
}
