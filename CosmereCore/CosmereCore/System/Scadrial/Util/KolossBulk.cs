using System.Collections.Concurrent;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Util;

/// <summary>
///     How far along its growth a koloss is, and which body that means it is drawn from.
/// </summary>
/// <remarks>
///     A koloss never stops growing, so its size is not a property of being a koloss - it is a
///     reading of how long this one has been alive. The growth hediff's severity is that reading.
///     <para>
///         Concurrent and cached because CurLifeStage is asked on every render frame and by hunger,
///         health scale and BodySize besides, and portraits render off the main thread. Growth runs
///         at about two thousandths of a point per day, which is far slower than the refresh below.
///     </para>
/// </remarks>
public static class KolossBulk {
    /// <summary>
    ///     Mass a koloss hauls over what its size alone would say, before growth is counted.
    /// </summary>
    /// <remarks>
    ///     Vanilla reads carried mass straight off <c>BodySize</c>, which no gene or hediff can
    ///     reach, so a koloss capped at a colonist's 35kg no matter what its stats said.
    /// </remarks>
    public const float HaulingBuild = 2f;

    private const int RefreshInterval = 250;

    private static readonly ConcurrentDictionary<int, LifeStageDef?> CachedBody = new();
    private static int CachedAt = -99999;

    /// <summary>
    ///     Same answer as <see cref="BodyFor" />, cached.
    /// </summary>
    /// <remarks>
    ///     Pawn_AgeTracker.CurLifeStage is read on every render frame and by hunger, health scale
    ///     and BodySize besides, so the patch that swaps this in cannot walk the hediff list each
    ///     time. Growth moves about two thousandths of a point per day; the refresh below is far
    ///     faster than a koloss can change band.
    /// </remarks>
    public static LifeStageDef? CachedBodyFor(Pawn? pawn) {
        if (pawn == null) return null;

        Refresh();

        if (CachedBody.TryGetValue(pawn.thingIDNumber, out LifeStageDef? known)) return known;

        LifeStageDef? body = BodyFor(pawn);
        CachedBody[pawn.thingIDNumber] = body;

        return body;
    }

    private static void Refresh() {
        int now = Find.TickManager?.TicksGame ?? 0;
        if (now - CachedAt < RefreshInterval) return;

        CachedBody.Clear();
        CachedAt = now;
    }

    /// <summary>
    ///     How far along the eight years this one is, or -1 for anything that is not a koloss.
    /// </summary>
    public static float GrowthOf(Pawn? pawn) {
        Verse.Hediff? growth = pawn?.health?.hediffSet?.GetFirstHediffOfDef(
            HediffDefOf.Cosmere_Scadrial_Hediff_KolossGrowth
        );

        return growth == null ? -1f : Mathf.Clamp01(growth.Severity);
    }

    /// <summary>
    ///     The body a koloss this far along is drawn from, or null for anything that is not one.
    /// </summary>
    /// <remarks>
    ///     Banded rather than smooth because bodyWidth is a def field, and it is bodyWidth that
    ///     decides how large a tile the pawn texture atlas cuts. The bands line up with the growth
    ///     hediff's own stages, which is where the player already sees it change.
    /// </remarks>
    public static LifeStageDef? BodyFor(Pawn? pawn) {
        float along = GrowthOf(pawn);
        if (along < 0f) return null;

        if (along >= 0.80f) return KolossLifeStageDefOf.Cosmere_Scadrial_LifeStage_KolossSplitting;
        if (along >= 0.55f) return KolossLifeStageDefOf.Cosmere_Scadrial_LifeStage_KolossOvergrown;
        if (along >= 0.25f) return KolossLifeStageDefOf.Cosmere_Scadrial_LifeStage_KolossGrown;

        return KolossLifeStageDefOf.Cosmere_Scadrial_LifeStage_KolossYoung;
    }
}
