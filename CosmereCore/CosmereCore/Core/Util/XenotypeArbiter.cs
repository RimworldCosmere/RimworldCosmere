using System.Collections.Generic;
using Cosmere.Core.Def;
using RimWorld;
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

        // primary can be null while world isn't - IsActive treats a null world as permissive.
        CosmereWorldDef? primary = WorldUtility.Primary;
        if (primary == null) return false;

        if (!WorldUtility.IsActive(world)) return false;

        // non-crossworld means IsActive already picked the one active world; nothing left to arbitrate.
        if (!primary.crossWorld) return true;

        return chosen == world;
    }

    /// <summary>
    ///     Whether the pawn's faction already answers this, in which case no world should.
    /// </summary>
    /// <remarks>
    ///     A faction that declares its own people is more specific than the planet they stand on.
    ///     Without this the arbiter drew a world at random per pawn, so an Alethkar soldier on a
    ///     cross-world save had even odds of coming out Skaa while Alethkar's own set said
    ///     darkeyes and lighteyes.
    /// </remarks>
    public static bool FactionSpeaksForItself(PawnGenerationRequest request) {
        XenotypeSet? set = request.Faction?.def?.xenotypeSet;

        return set is { Count: > 0 };
    }
}
