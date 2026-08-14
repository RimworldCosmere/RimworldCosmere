using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cosmere.Core.Util;

/// <summary>
///     Picks head types the way pawn generation does, instead of the way a plain filter does.
/// </summary>
/// <remarks>
///     <c>Skull</c> and <c>Stump</c> are ordinary HeadTypeDefs that happen to carry
///     <c>randomChosen=false</c> and <c>selectionWeight=0</c>. Any code that queries the database
///     and filters on gender alone has both of them in the draw pool, and gender <c>None</c> means
///     they pass every gender check. Sazed came out of the Pre-Catacendre scenario headless that
///     way - nothing removed his head, the scenario handed him the graphic for one that had been.
/// </remarks>
public static class HeadTypeUtility {
    /// <summary>
    ///     Every head the generator would consider for this pawn, in def order.
    /// </summary>
    public static List<HeadTypeDef> Available(Gender gender, Pawn? pawn = null) {
        List<HeadTypeDef> all = DefDatabase<HeadTypeDef>.AllDefsListForReading;
        List<HeadTypeDef> usable = [];

        for (int i = 0; i < all.Count; i++) {
            HeadTypeDef head = all[i];

            if (!head.randomChosen || head.selectionWeight <= 0f) continue;
            if (head.gender != Gender.None && gender != Gender.None && head.gender != gender) continue;
            if (!HasGenesFor(head, pawn)) continue;

            usable.Add(head);
        }

        return usable;
    }

    /// <summary>
    ///     One head, weighted the way the generator weights them, or the fallback if none fit.
    /// </summary>
    public static HeadTypeDef? RandomFor(Gender gender, Pawn? pawn = null, HeadTypeDef? fallback = null) {
        List<HeadTypeDef> usable = Available(gender, pawn);
        if (usable.Count == 0) return fallback;

        float total = 0f;
        for (int i = 0; i < usable.Count; i++) total += usable[i].selectionWeight;

        float roll = Rand.Range(0f, total);
        for (int i = 0; i < usable.Count; i++) {
            roll -= usable[i].selectionWeight;
            if (roll <= 0f) return usable[i];
        }

        return usable[usable.Count - 1];
    }

    private static bool HasGenesFor(HeadTypeDef head, Pawn? pawn) {
        if (head.requiredGenes == null || head.requiredGenes.Count == 0) return true;
        if (pawn?.genes == null) return false;

        for (int i = 0; i < head.requiredGenes.Count; i++) {
            if (!pawn.genes.HasActiveGene(head.requiredGenes[i])) return false;
        }

        return true;
    }
}
