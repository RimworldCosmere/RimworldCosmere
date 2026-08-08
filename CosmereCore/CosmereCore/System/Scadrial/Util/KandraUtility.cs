using System.Collections.Generic;
using Cosmere.Core;
using Cosmere.System.Scadrial.Hemalurgy;
using Cosmere.System.Scadrial.Hemalurgy.Hediff;
using Cosmere.System.Scadrial.Kandra;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Util;

/// <summary>
///     The two spikes that make a mistwraith into a kandra, and what happens when they come out.
/// </summary>
/// <remarks>
///     A kandra is not born and cannot be bred. Someone takes a mistwraith and drives in a pair
///     of hemalurgic spikes, and the thing wakes up with a mind. Which pair decides what kind of
///     mind: Presence holds it together, Potency makes it strong, Stability makes it calm,
///     Awareness makes it see. The Contract calls all four Blessings.
/// </remarks>
public static class KandraUtility {
    public const string KandraXenotype = "Cosmere_Scadrial_Xenotype_Kandra";

    public static bool IsKandra(Pawn? pawn) {
        return pawn?.genes?.Xenotype?.defName == KandraXenotype;
    }

    /// <summary>Every Blessing there is, in the order the Contract ranks them.</summary>
    public static IReadOnlyList<HediffDef> Blessings => blessings ??= BuildBlessings();

    private static List<HediffDef>? blessings;

    private static List<HediffDef> BuildBlessings() {
        List<HediffDef> found = [];
        HediffDef?[] candidates = [
            HediffDefOf.Cosmere_Scadrial_Hediff_BlessingOfPresence,
            HediffDefOf.Cosmere_Scadrial_Hediff_BlessingOfPotency,
            HediffDefOf.Cosmere_Scadrial_Hediff_BlessingOfStability,
            HediffDefOf.Cosmere_Scadrial_Hediff_BlessingOfAwareness,
        ];

        for (int i = 0; i < candidates.Length; i++) {
            if (candidates[i] != null) found.Add(candidates[i]!);
        }

        return found;
    }

    public static Hediff? BlessingOn(Pawn? pawn) {
        HediffSet? set = pawn?.health?.hediffSet;
        if (set == null) return null;

        IReadOnlyList<HediffDef> all = Blessings;
        for (int i = 0; i < all.Count; i++) {
            Hediff? found = set.GetFirstHediffOfDef(all[i]);
            if (found != null) return found;
        }

        return null;
    }

    public static bool HasBlessing(Pawn? pawn) {
        return BlessingOn(pawn) != null;
    }

    /// <summary>A Blessing is two spikes. Fewer than two and there is no Blessing left.</summary>
    public const int SpikesPerBlessing = 2;

    /// <summary>
    ///     Which metal and which theft each Blessing is made of.
    /// </summary>
    /// <remarks>
    ///     Both spikes in a pair are the same metal, so a hemalurgist looking at a kandra sees a
    ///     matched set rather than the scavenged mess most spiked people carry.
    /// </remarks>
    private static (string metal, HemalurgicStealType steal)? RecipeFor(HediffDef blessing) {
        if (blessing == HediffDefOf.Cosmere_Scadrial_Hediff_BlessingOfPresence) {
            return (MetalDefOf.Copper.defName, HemalurgicStealType.MentalFortitude);
        }

        if (blessing == HediffDefOf.Cosmere_Scadrial_Hediff_BlessingOfPotency) {
            return (MetalDefOf.Iron.defName, HemalurgicStealType.HumanStrength);
        }

        if (blessing == HediffDefOf.Cosmere_Scadrial_Hediff_BlessingOfStability) {
            return (MetalDefOf.Zinc.defName, HemalurgicStealType.EmotionalFortitude);
        }

        if (blessing == HediffDefOf.Cosmere_Scadrial_Hediff_BlessingOfAwareness) {
            return (MetalDefOf.Tin.defName, HemalurgicStealType.HumanSenses);
        }

        return null;
    }

    public static int SpikeCount(Pawn? pawn) {
        Hediff? spiked = pawn?.health?.hediffSet?.GetFirstHediffOfDef(
            Hemalurgy.HemalurgicDefOf.Cosmere_Scadrial_Hediff_HemalurgicSpikes
        );

        return spiked is HemalurgicSpikes set ? set.spikeCount : 0;
    }

    /// <summary>
    ///     Wakes a mistwraith up as a kandra.
    /// </summary>
    /// <remarks>
    ///     Skills do not carry across. Whatever the donor knew went into the spikes as raw
    ///     capacity, not as training, so the new kandra starts at nothing and learns the way
    ///     anyone does.
    /// </remarks>
    public static void Become(Pawn pawn, HediffDef blessing) {
        if (pawn.genes == null) return;

        XenotypeDef? kandra = DefDatabase<XenotypeDef>.GetNamedSilentFail(KandraXenotype);
        if (kandra == null) {
            Cosmere.Core.Logger.Warning("KandraUtility: the kandra xenotype is missing.");
            return;
        }

        pawn.genes.SetXenotype(kandra);

        Hediff? mistwraith = pawn.health?.hediffSet?.GetFirstHediffOfDef(
            HediffDefOf.Cosmere_Scadrial_Hediff_Mistwraith
        );
        if (mistwraith != null) pawn.health?.RemoveHediff(mistwraith);

        GiveBlessing(pawn, blessing);

        // The kandra's own body is whatever it woke up in. Recording it now means Revert always
        // has somewhere to go back to, even if the first thing it does is eat a corpse.
        pawn.TryGetComp<CompKandraForms>()?.RememberTrueBody();
    }

    /// <summary>
    ///     Adds the Blessing and the pair of spikes that carry it, without touching the xenotype.
    /// </summary>
    /// <remarks>
    ///     Split out from <see cref="Become" /> because a gene's PostAdd runs inside the xenotype
    ///     being applied. Calling SetXenotype from in there sets the whole thing going again.
    /// </remarks>
    public static void GiveBlessing(Pawn pawn, HediffDef blessing) {
        if (BlessingOn(pawn) == null) pawn.health?.AddHediff(blessing);

        DriveSpikes(pawn, blessing);
    }

    /// <summary>Puts the pair in, so the rest of hemalurgy can see them.</summary>
    /// <remarks>
    ///     Ruin reaches people through their spikes, and bronze finds them. A kandra whose
    ///     Blessing was only a hediff would be invisible to both, which is backwards - being
    ///     spiked is the single most important thing about what a kandra is.
    /// </remarks>
    private static void DriveSpikes(Pawn pawn, HediffDef blessing) {
        (string metal, HemalurgicStealType steal)? recipe = RecipeFor(blessing);
        if (recipe == null || pawn.health == null) return;

        Hediff? existing = pawn.health.hediffSet?.GetFirstHediffOfDef(
            Hemalurgy.HemalurgicDefOf.Cosmere_Scadrial_Hediff_HemalurgicSpikes
        );
        if (existing is not HemalurgicSpikes set) {
            set = (HemalurgicSpikes)pawn.health.AddHediff(
                Hemalurgy.HemalurgicDefOf.Cosmere_Scadrial_Hediff_HemalurgicSpikes
            );
        }

        for (int i = set.spikeCount; i < SpikesPerBlessing; i++) {
            set.AddSpike(
                new ImplantedSpikeData {
                    metalDefName = recipe.Value.metal,
                    stealType = recipe.Value.steal,
                    chargeStrength = 1f,
                }
            );
        }
    }

    /// <summary>
    ///     Pulls the spikes. What is left keeps breathing and stops being a person.
    /// </summary>
    /// <remarks>
    ///     The disguise drops first. A mistwraith cannot hold a borrowed face, so anyone who
    ///     thought they were talking to a friend finds out at the same moment the kandra does.
    /// </remarks>
    public static void RevertToMistwraith(Pawn pawn) {
        Hediff? blessing = BlessingOn(pawn);
        if (blessing != null) pawn.health?.RemoveHediff(blessing);

        KandraShapeshift.Revert(pawn);

        if (pawn.health?.hediffSet?.GetFirstHediffOfDef(
                HediffDefOf.Cosmere_Scadrial_Hediff_Mistwraith
            ) == null) {
            pawn.health?.AddHediff(HediffDefOf.Cosmere_Scadrial_Hediff_Mistwraith);
        }

        if (pawn.skills?.skills != null) {
            for (int i = 0; i < pawn.skills.skills.Count; i++) {
                pawn.skills.skills[i].Level = 0;
                pawn.skills.skills[i].passion = Passion.None;
            }
        }

        Messages.Message(
            "CS_KandraLostBlessing".Translate(pawn.NameShortColored.Named("PAWN")),
            pawn,
            MessageTypeDefOf.NegativeEvent,
            false
        );
    }
}
