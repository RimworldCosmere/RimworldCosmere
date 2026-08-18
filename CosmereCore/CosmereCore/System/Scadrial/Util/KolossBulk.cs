using System.Collections.Concurrent;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Util;

/// <summary>
///     How much bigger than a man this koloss has got.
/// </summary>
/// <remarks>
///     A koloss never stops growing, so its size is not a property of being a koloss - it is a
///     reading of how long this one has been alive. The growth hediff's severity is that reading,
///     and one number drives both what the player sees and what the thing can pick up.
///     <para>
///         Every render node of every pawn asks for this on every frame, so the answer is cached.
///         Growth runs at about two thousandths of a point per day, which is far slower than the
///         refresh below - a koloss cannot change size between two checks.
///     </para>
/// </remarks>
public static class KolossBulk {
    /// <summary>Size the moment the spikes go in. Barely more than the man they were.</summary>
    public const float NewlyMade = 1.05f;

    /// <summary>
    ///     Size when the skin finally fails. A grown koloss stands about twelve feet.
    /// </summary>
    public const float FullyGrown = 1.75f;

    /// <summary>
    ///     Mass a koloss hauls over what its size alone would say, before growth is counted.
    /// </summary>
    /// <remarks>
    ///     Vanilla reads carried mass straight off <c>BodySize</c>, which no gene or hediff can
    ///     reach, so a koloss capped at a colonist's 35kg no matter what its stats said.
    /// </remarks>
    public const float HaulingBuild = 2f;

    private const int RefreshInterval = 250;

    /// <summary>
    ///     Concurrent because portraits render off the main thread. A plain Dictionary here threw
    ///     an exclusive-access exception seventeen hundred times in one session.
    /// </summary>
    private static readonly ConcurrentDictionary<int, float> Cached = new();
    private static int CachedAt = -99999;

    /// <summary>
    ///     1.0 for anything that is not a koloss, so callers can multiply without asking first.
    /// </summary>
    public static float For(Pawn? pawn) {
        if (pawn == null) return 1f;

        int now = Find.TickManager?.TicksGame ?? 0;

        // Wholesale rather than per-entry, so dead and despawned pawns cannot pile up in here.
        if (now - CachedAt >= RefreshInterval) {
            Cached.Clear();
            CachedAt = now;
        }

        if (Cached.TryGetValue(pawn.thingIDNumber, out float known)) return known;

        float size = Measure(pawn);
        Cached[pawn.thingIDNumber] = size;

        return size;
    }

    private static float Measure(Pawn pawn) {
        Verse.Hediff? growth = pawn.health?.hediffSet?.GetFirstHediffOfDef(
            HediffDefOf.Cosmere_Scadrial_Hediff_KolossGrowth
        );
        if (growth == null) return 1f;

        // height tracks width too - scaling them apart made a koloss read as a stretched human, not a bigger one.
        return Mathf.Lerp(NewlyMade, FullyGrown, Mathf.Clamp01(growth.Severity));
    }
}
