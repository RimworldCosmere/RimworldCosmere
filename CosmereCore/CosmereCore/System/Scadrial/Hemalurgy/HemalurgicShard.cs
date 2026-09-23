using Cosmere.Core.Def;
using Cosmere.Core.Util;
using Verse;

namespace Cosmere.System.Scadrial.Hemalurgy;

/// <summary>
///     Which Shard is on the other end of a hemalurgic spike in this save.
/// </summary>
/// <remarks>
///     Hemalurgy is Ruin's, and for most of Scadrial's history saying so is correct. After the
///     Catacendre it is not: Harmony holds both Shards, and a spiked pawn in the Alloy era hearing
///     Ruin by name reads as a bug rather than as history.
///     <para>
///         Everything the player sees goes through here so the name follows the save rather than
///         being written into a def.
///     </para>
/// </remarks>
public static class HemalurgicShard {
    /// <summary>
    ///     Harmony where Harmony exists, Ruin otherwise. Ruin is also the fallback for a save
    ///     that somehow has neither, because a spike with nothing behind it is still Ruin's idea.
    /// </summary>
    public static ShardDef? Current {
        get {
            if (ShardUtility.IsEnabled(ShardDefOf.Harmony)) return ShardDefOf.Harmony;

            return ShardDefOf.Ruin;
        }
    }

    /// <summary>The name to put in front of the player. Never empty.</summary>
    public static string Name => Current?.LabelCap.ToString() ?? "Ruin";
}
