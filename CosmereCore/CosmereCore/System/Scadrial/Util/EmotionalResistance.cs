using Cosmere.System.Scadrial.Gene;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Util;

/// <summary>
///     How hard a thing is to seize with Emotional Allomancy, and how hard an Allomancer pushes.
/// </summary>
/// <remarks>
///     Binding and holding are different acts and only one of them is expensive. Getting hold of a
///     koloss in the first place is a contest the Allomancer can lose; keeping it afterwards is a
///     bill they pay. This is the contest half.
///     <para>
///         Resistance belongs to the creature, and in both cases it is a reading of age. A koloss
///         that has been growing for twenty years is genuinely harder to take than one spiked this
///         morning, and a first generation kandra has nine hundred years of being itself to argue
///         with. One rule for the player to learn: the older it is, the more it takes.
///     </para>
/// </remarks>
public static class EmotionalResistance {
    /// <summary>What a freshly spiked koloss asks for.</summary>
    private const float KolossFloor = 1f;

    /// <summary>What one about to split its skin asks for.</summary>
    private const float KolossCeiling = 5f;

    /// <summary>What a tenth generation kandra asks for, at about forty years old.</summary>
    private const float KandraFloor = 1.5f;

    /// <summary>What a first generation asks for, at about a thousand.</summary>
    private const float KandraCeiling = 7f;

    /// <summary>Years a kandra has to reach before it resists like a first generation.</summary>
    private const float KandraOldestYears = 1000f;

    /// <summary>
    ///     Zero for anything that cannot be bound at all, so callers can test the number rather
    ///     than the species.
    /// </summary>
    public static float Of(Pawn? pawn) {
        if (pawn == null || pawn.Dead) return 0f;

        Verse.Hediff? growth = pawn.health?.hediffSet?.GetFirstHediffOfDef(
            HediffDefOf.Cosmere_Scadrial_Hediff_KolossGrowth
        );
        if (growth != null) {
            return Mathf.Lerp(KolossFloor, KolossCeiling, Mathf.Clamp01(growth.Severity));
        }

        if (KandraOf(pawn) is { } kandra) {
            float aged = Mathf.Clamp01(kandra.ChronologicalYears / KandraOldestYears);

            return Mathf.Lerp(KandraFloor, KandraCeiling, aged);
        }

        return 0f;
    }

    /// <summary>
    ///     Whether this Allomancer is pushing hard enough right now to take that creature.
    /// </summary>
    /// <remarks>
    ///     Reach comes from the ability's own GetStrength, which already folds in AllomanticPower,
    ///     the skill, savant stage and flaring - and already gives a duralumin burn its tenfold
    ///     spike, because GetStrength reads the duralumin reserve directly when the burn is powered
    ///     that way. Nothing extra is needed to make duralumin the way you exceed your reach; it
    ///     already is.
    /// </remarks>
    public static bool CanSeize(float reach, Pawn? target) {
        float needed = Of(target);

        return needed > 0f && reach >= needed;
    }

    private static KandraHeritage? KandraOf(Pawn pawn) {
        return pawn.genes?.GetGene(
            DefDatabase<GeneDef>.GetNamedSilentFail("Cosmere_Scadrial_Gene_KandraHeritage")
        ) as KandraHeritage;
    }
}
