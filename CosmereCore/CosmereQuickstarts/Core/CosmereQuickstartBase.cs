using System.Text;
using Cosmere.Core.Util;
using Verse;

namespace Cosmere.Core.Quickstart;

/// <summary>
///     Adds the two things a Cosmere quickstart needs that the generic base cannot know about:
///     which Shards the game comes up with, and which era the world sits in.
/// </summary>
public abstract class CosmereQuickstartBase : AbstractQuickstart {
    /// <summary>
    ///     Def names, not ShardDefs: Radiant grants need Honor even from Scadrial code, and the
    ///     Roshar ShardDefOf lives in a namespace Scadrial must not import.
    /// </summary>
    public virtual IReadOnlyList<string> shards => [];

    /// <summary>
    ///     Overrides whatever era the scenario declares. Null defers to the scenario, which is
    ///     what every quickstart that isn't deliberately cross-shard wants.
    /// </summary>
    public virtual string? era => null;

    /// <summary>
    ///     A WorldGenStep that reads the world - Scadrial's Ashmounts, for one - runs inside
    ///     GenerateWorld, so the world has to be committed before that call.
    /// </summary>
    public override void PreGenerateWorld() {
        WorldUtility.SeedFromScenario();
    }

    /// <summary>
    ///     Runs after the scenario's own PostIdeoChosen. Any earlier and PreConfigure drops a
    ///     shard like Ruin or Preservation straight back off again.
    /// </summary>
    public override void PostConfigured() {
        CosmereQuicktest.EnableShards(shards);
    }

    public override TaggedString GetDescription() {
        StringBuilder builder = new StringBuilder(base.GetDescription());
        builder.AppendLine(
            "CC_Quickstart_Shards".Translate().Colorize(ColoredText.TipSectionTitleColor) +
            (shards.Count > 0 ? string.Join(", ", shards) : "None")
        );

        return builder.ToString();
    }
}
