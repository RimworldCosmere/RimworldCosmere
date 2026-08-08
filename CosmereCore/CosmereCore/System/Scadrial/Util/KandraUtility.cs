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

    /// <summary>Every Blessing the kandra carries. Kandra can hold more than one.</summary>
    public static List<Hediff> BlessingsOn(Pawn? pawn) {
        List<Hediff> found = [];
        HediffSet? set = pawn?.health?.hediffSet;
        if (set == null) return found;

        IReadOnlyList<HediffDef> all = Blessings;
        for (int i = 0; i < all.Count; i++) {
            Hediff? hediff = set.GetFirstHediffOfDef(all[i]);
            if (hediff != null) found.Add(hediff);
        }

        return found;
    }

    /// <summary>
    ///     The Blessings that still have both their spikes in.
    /// </summary>
    /// <remarks>
    ///     Each Blessing stands on its own pair. Pulling one spike breaks that Blessing and
    ///     leaves the others alone, so a kandra with three of them can lose one and carry on.
    /// </remarks>
    public static List<Hediff> CompleteBlessingsOn(Pawn? pawn) {
        List<Hediff> whole = [];
        List<Hediff> all = BlessingsOn(pawn);

        for (int i = 0; i < all.Count; i++) {
            if (MatchingSpikeCount(pawn, all[i].def) >= SpikesPerBlessing) whole.Add(all[i]);
        }

        return whole;
    }

    public static bool HasBlessing(Pawn? pawn) {
        return BlessingOn(pawn) != null;
    }

    /// <summary>
    ///     True when the kandra has lost a spike and cannot hold a shape any more.
    /// </summary>
    /// <remarks>
    ///     Covers both the one-spike state and the no-spike one. Shapeshifting needs a whole
    ///     mind, so neither of them gets to do it.
    /// </remarks>
    public static bool CanHoldAShape(Pawn? pawn) {
        HediffSet? set = pawn?.health?.hediffSet;
        if (set == null) return false;

        if (set.HasHediff(HediffDefOf.Cosmere_Scadrial_Hediff_HalfBlessed)) return false;
        if (set.HasHediff(HediffDefOf.Cosmere_Scadrial_Hediff_Mistwraith)) return false;

        return true;
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
    ///     How many of the pawn's spikes are the matched pair this Blessing is made of.
    /// </summary>
    /// <remarks>
    ///     A Blessing is two spikes of one metal stealing one thing. Counting every spike in the
    ///     body would let a kandra keep its mind on a scavenged pair of somebody else's, which is
    ///     not what a Blessing is. Anything the kandra picked up elsewhere is ignored here.
    /// </remarks>
    public static int MatchingSpikeCount(Pawn? pawn, HediffDef blessing) {
        (string metal, HemalurgicStealType steal)? recipe = RecipeFor(blessing);
        if (recipe == null) return 0;

        Hediff? spiked = pawn?.health?.hediffSet?.GetFirstHediffOfDef(
            Hemalurgy.HemalurgicDefOf.Cosmere_Scadrial_Hediff_HemalurgicSpikes
        );
        if (spiked is not HemalurgicSpikes set) return 0;

        int matching = 0;
        for (int i = 0; i < set.spikes.Count; i++) {
            if (set.spikes[i].metalDefName != recipe.Value.metal) continue;
            if (set.spikes[i].stealType != recipe.Value.steal) continue;

            matching++;
        }

        return matching;
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
        // This Blessing, not any Blessing. Checking whether the pawn had one at all meant a
        // kandra that already carried Presence could never be given Potency: the hediff was
        // skipped and only the spikes went in.
        if (pawn.health?.hediffSet?.HasHediff(blessing) != true) pawn.health?.AddHediff(blessing);

        DriveSpikes(pawn, blessing);

        // One place decides what the spikes add up to, so adding a second Blessing and repairing
        // a broken one go down the same path.
        ReconcileSpikes(pawn);
    }

    /// <summary>Grey, wet and roughly upright. Placeholder until the art lands.</summary>
    private static readonly UnityEngine.Color MistwraithGrey = new UnityEngine.Color(0.58f, 0.60f, 0.58f);

    /// <summary>
    ///     Drops whatever shape the kandra was holding and leaves the thing underneath.
    /// </summary>
    /// <remarks>
    ///     Holding a face takes a whole mind. With one spike or none there is not enough left to
    ///     do it, so the borrowed body goes whether the player wanted it to or not. The look is a
    ///     recolour for now; a mistwraith needs its own art before this is finished.
    /// </remarks>
    public static void WearMistwraithShape(Pawn pawn) {
        KandraShapeshift.Revert(pawn);

        if (pawn.story == null) return;

        pawn.story.skinColorOverride = MistwraithGrey;
        pawn.story.hairDef = HairDefOf.Bald;
        pawn.Drawer?.renderer?.SetAllGraphicsDirty();
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

        List<ImplantedSpikeData>? salvaged = null;

        Hediff? existing = pawn.health.hediffSet?.GetFirstHediffOfDef(
            Hemalurgy.HemalurgicDefOf.Cosmere_Scadrial_Hediff_HemalurgicSpikes
        );

        // Kandra made before the spikes were attached to a part carry one with no Part, which
        // hides the removal surgery forever. Take it off and let the code below rebuild it,
        // keeping whatever spikes were in it.
        if (existing is HemalurgicSpikes stray && stray.Part == null) {
            List<ImplantedSpikeData> carried = [.. stray.spikes];
            pawn.health.RemoveHediff(stray);
            existing = null;
            salvaged = carried;
        }

        if (existing is not HemalurgicSpikes set) {
            // On the torso, the way the implant surgery does it. Added with no part, the hediff
            // works for everything that only counts spikes, but Recipe_Surgery asks the worker
            // for parts to operate on and it answers with this hediff's Part. Null there means
            // no valid parts, so "remove hemalurgic spike" silently never appears.
            BodyPartRecord? torso = pawn.health.hediffSet?.GetNotMissingParts()
                .FirstOrDefault(part => part.def == BodyPartDefOf.Torso);
            if (torso == null) return;

            set = (HemalurgicSpikes)HediffMaker.MakeHediff(
                Hemalurgy.HemalurgicDefOf.Cosmere_Scadrial_Hediff_HemalurgicSpikes,
                pawn,
                torso
            );
            pawn.health.AddHediff(set, torso);
        }

        if (salvaged != null) {
            for (int i = 0; i < salvaged.Count; i++) set.AddSpike(salvaged[i]);
        }

        // Count this Blessing's own pair, not every spike in the body. A kandra with three
        // Blessings has six spikes in it, and topping up to two total would leave the new one
        // with nothing.
        for (int i = MatchingSpikeCount(pawn, blessing); i < SpikesPerBlessing; i++) {
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
    ///     Brings the kandra's state into line with how many spikes are actually still in it.
    /// </summary>
    /// <remarks>
    ///     Two spikes and it is a kandra. One and it keeps its name but loses the thread of its
    ///     own centuries. None and it is a mistwraith. Called on a slow tick and after any
    ///     surgery, so a hemalurgist pulling a single spike gets the half state rather than
    ///     nothing until the second one comes out.
    /// </remarks>
    public static void ReconcileSpikes(Pawn pawn) {
        if (pawn.health?.hediffSet == null) return;

        // A Blessing whose pair is broken stops being a Blessing, and its stats go with it. The
        // spikes that are left stay in the body; they just do not add up to anything.
        List<Hediff> all = BlessingsOn(pawn);
        for (int i = 0; i < all.Count; i++) {
            if (MatchingSpikeCount(pawn, all[i].def) < SpikesPerBlessing) {
                pawn.health.RemoveHediff(all[i]);
            }
        }

        int whole = CompleteBlessingsOn(pawn).Count;

        if (whole > 0) {
            Remove(pawn, HediffDefOf.Cosmere_Scadrial_Hediff_HalfBlessed);
            Remove(pawn, HediffDefOf.Cosmere_Scadrial_Hediff_Mistwraith);

            CompKandraForms? forms = pawn.TryGetComp<CompKandraForms>();
            KandraForm? worn = forms?.Mind.Shape;

            forms?.Mind.Restore(pawn);

            // A repaired kandra picks its old face back up. Coming out of it grey and nameless
            // would make every recovery feel like a different person walking in.
            if (worn != null) {
                KandraShapeshift.Wear(pawn, worn);
            } else if (forms != null) {
                KandraShapeshift.Revert(pawn);
            }

            return;
        }

        if (SpikeCount(pawn) <= 0) {
            if (!pawn.health.hediffSet.HasHediff(HediffDefOf.Cosmere_Scadrial_Hediff_Mistwraith)) {
                RevertToMistwraith(pawn);
            }

            return;
        }

        // Spikes in the body, but not two of a kind among them.
        if (pawn.health.hediffSet.HasHediff(HediffDefOf.Cosmere_Scadrial_Hediff_HalfBlessed)) return;

        Remove(pawn, HediffDefOf.Cosmere_Scadrial_Hediff_Mistwraith);
        pawn.health.AddHediff(HediffDefOf.Cosmere_Scadrial_Hediff_HalfBlessed);

        // Not enough left to hold a borrowed shape, and the years start going.
        pawn.TryGetComp<CompKandraForms>()?.Mind.Store(pawn);
        KandraMind.Clear(pawn, alsoSkills: false);
        WearMistwraithShape(pawn);

        Messages.Message(
            "CS_Kandra_HalfBlessed".Translate(pawn.NameShortColored.Named("PAWN")),
            pawn,
            MessageTypeDefOf.NegativeEvent,
            false
        );
    }

    private static void Remove(Pawn pawn, HediffDef def) {
        Hediff? found = pawn.health?.hediffSet?.GetFirstHediffOfDef(def);
        if (found != null) pawn.health?.RemoveHediff(found);
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

        Remove(pawn, HediffDefOf.Cosmere_Scadrial_Hediff_HalfBlessed);

        KandraShapeshift.Revert(pawn);

        if (pawn.health?.hediffSet?.GetFirstHediffOfDef(
                HediffDefOf.Cosmere_Scadrial_Hediff_Mistwraith
            ) == null) {
            pawn.health?.AddHediff(HediffDefOf.Cosmere_Scadrial_Hediff_Mistwraith);
        }

        CompKandraForms? forms = pawn.TryGetComp<CompKandraForms>();
        forms?.Mind.Store(pawn);
        KandraMind.Clear(pawn, alsoSkills: true);

        WearMistwraithShape(pawn);

        Messages.Message(
            "CS_KandraLostBlessing".Translate(pawn.NameShortColored.Named("PAWN")),
            pawn,
            MessageTypeDefOf.NegativeEvent,
            false
        );
    }
}
