using System.Collections.Generic;
using Cosmere.Core.Def;
using Verse;

namespace Cosmere.Core.Util;

/// <summary>
///     Decides which world gets to name a generated pawn's xenotype.
/// </summary>
/// <remarks>
///     Every shard's xenotype patch injects At.Return on the same method at the same Priority and
///     overwrites the answer unconditionally once its gate passes. On a single-world save that is
///     fine - only one gate can pass. On the cross-world sentinel every gate passes, so whichever
///     patch Concord happened to compose outermost decided the xenotype of every pawn in the game,
///     and nothing said so.
///     <para>
///         The draw has to happen once per pawn, not once per patch: two patches each rolling
///         their own would still collide, just less predictably. Core takes the draw at the head
///         of generation and every shard reads the same answer.
///     </para>
/// </remarks>
public static class XenotypeArbiter {
    [global::System.ThreadStatic]
    private static CosmereWorldDef? chosen;

    /// <summary>Takes the draw for one pawn. Called from the head of xenotype generation.</summary>
    public static void Draw() {
        chosen = null;

        CosmereWorldDef? primary = WorldUtility.Primary;
        if (primary?.crossWorld != true) return;

        List<CosmereWorldDef> candidates = [];
        List<CosmereWorldDef> worlds = DefDatabase<CosmereWorldDef>.AllDefsListForReading;
        for (int i = 0; i < worlds.Count; i++) {
            if (worlds[i].crossWorld) continue;
            if (worlds[i].xenotypes.Count == 0) continue;
            candidates.Add(worlds[i]);
        }

        if (candidates.Count == 0) return;

        chosen = candidates[Rand.Range(0, candidates.Count)];
    }

    /// <summary>Whether <paramref name="world" />'s patch may set the xenotype for this pawn.</summary>
    public static bool MayAnswer(CosmereWorldDef? world) {
        if (world == null) return false;
        if (!WorldUtility.IsActive(world)) return false;

        // A single-world save has already been decided by IsActive - only one world can be
        // active, so there is nothing to arbitrate.
        CosmereWorldDef? primary = WorldUtility.Primary;
        if (primary?.crossWorld != true) return true;

        return chosen == world;
    }
}
