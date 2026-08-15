using System.Collections.Generic;
using System.Linq;
using Cosmere.Core.Def;
using Cosmere.System.Scadrial.Hemalurgy;
using Cosmere.System.Scadrial.Hemalurgy.Util;
using Cosmere.System.Scadrial.Util;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Gene;

/// <summary>
///     Holds a koloss to the four spikes that made it.
/// </summary>
/// <remarks>
///     A koloss is four spikes driven in at once, and the gene's own description says so - but
///     nothing recorded them, so a koloss from a raid or the dev menu had no spikes in its body at
///     all. Nothing to pull out, nothing for Ruin to speak through, and nothing for a surgeon to
///     find.
///     <para>
///         Shaped after <see cref="BlessingBound" />, which does the same job for a kandra's pair:
///         seed on add, then re-check slowly, because spikes come out through surgery rather than
///         between two ticks.
///     </para>
/// </remarks>
public class SpikeBound : Verse.Gene {
    /// <summary>Four, driven at once. Fewer and what is left is not a koloss.</summary>
    public const int SpikeCount = 4;

    /// <summary>Weakest a koloss spike comes out. Well above the 0.05 that counts as charged.</summary>
    public const float WeakestCharge = 0.5f;

    /// <summary>Strongest. A spike driven by somebody who knew where to put it.</summary>
    public const float StrongestCharge = 1f;

    /// <summary>
    ///     What the four are made of, which is iron, four times.
    /// </summary>
    /// <remarks>
    ///     Iron steals human strength. Four of them is why a koloss is enormous, and the absence of
    ///     any mental metal among them is why there is so little left inside it.
    /// </remarks>
    private const string Metal = "Iron";

    public override void PostAdd() {
        base.PostAdd();

        if (pawn.health?.hediffSet == null) return;

        // SetXenotype calls AddGene once per gene and nothing dedupes, so a second Become would
        // otherwise drive another four in.
        if (KolossUtility.SpikeCount(pawn) > 0) return;

        BodyPartRecord? core = pawn.RaceProps?.body?.corePart == null
            ? null
            : pawn.health.hediffSet.GetNotMissingParts()
                .FirstOrDefault(p => p.def == pawn.RaceProps.body.corePart.def);

        // stealType is stated rather than left to default. HemalurgicChargeData.isValid needs one
        // of six things to be true, and the only one these satisfy is IsHumanAttribute - which held
        // only because HumanStrength happens to be the first member of the enum. Reorder it and
        // every koloss quietly drops uncharged spikes the make-koloss bill then refuses. Iron
        // steals strength; say so.
        for (int i = 0; i < SpikeCount; i++) {
            HemalurgicImplantUtility.AddToUnifiedHediff(
                pawn,
                new ImplantedSpikeData {
                    metalDefName = Metal,
                    chargeStrength = RolledCharge(),
                    stealType = HemalurgicStealType.HumanStrength,
                },
                core
            );
        }

        HemalurgicImplantUtility.UpdateRuinsInfluence(pawn);
    }

    /// <summary>
    ///     How much strength one spike took out of whoever it went through.
    /// </summary>
    /// <remarks>
    ///     Gaussian rather than flat, because most spikings are ordinary and the good and bad ones
    ///     are the exception. The mean sits at three quarters with the edges about two deviations
    ///     out, so the tails are rare rather than merely less common. Clamped, since Gaussian has
    ///     no bounds and one unlucky roll would otherwise produce a spike below the 0.05 that
    ///     counts as charged at all - a koloss held together by nothing.
    /// </remarks>
    private static float RolledCharge() {
        return Mathf.Clamp(
            Rand.Gaussian((WeakestCharge + StrongestCharge) / 2f, 0.12f),
            WeakestCharge,
            StrongestCharge
        );
    }

    /// <summary>
    ///     Drops the four when the thing they were holding together finally comes apart.
    /// </summary>
    /// <remarks>
    ///     Koloss make new koloss out of the spikes of their dead, and without this a koloss raid
    ///     was pure attrition on the player's spike supply - four went in, nothing ever came back,
    ///     and there was no way to break even on one. Surgery already returns them; this is the
    ///     other way a koloss ends.
    ///     <para>
    ///         They keep whatever charge they had. A spike that has been driven through somebody
    ///         is worth something on the strength of that, and dying does not undo it.
    ///     </para>
    /// </remarks>
    public override void Notify_PawnDied(DamageInfo? dinfo, Verse.Hediff? culprit = null) {
        base.Notify_PawnDied(dinfo, culprit);

        if (pawn.health?.hediffSet?.GetFirstHediffOfDef(
                HemalurgicDefOf.Cosmere_Scadrial_Hediff_HemalurgicSpikes
            ) is not Hemalurgy.Hediff.HemalurgicSpikes set) {
            return;
        }

        Map? map = pawn.MapHeld;
        IntVec3 where = pawn.PositionHeld;
        if (map == null || !where.IsValid) return;

        List<ImplantedSpikeData> falling = [.. set.spikes];
        for (int i = 0; i < falling.Count; i++) {
            Drop(falling[i], map, where);
        }
    }

    private static void Drop(ImplantedSpikeData spike, Map map, IntVec3 where) {
        MetalDef? metal = DefDatabase<MetalDef>.GetNamedSilentFail(spike.metalDefName);
        if (metal?.Item == null) return;

        Verse.Thing made = ThingMaker.MakeThing(
            spike.isThinNeedle
                ? HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicNeedle
                : HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicSpike,
            metal.Item
        );

        if (made.TryGetComp(out Hemalurgy.Comp.Thing.HemalurgicSpike? comp) && comp != null) {
            comp.Charge(new HemalurgicChargeData {
                chargedTick = Find.TickManager?.TicksGame ?? 0,
                stealType = spike.stealType,
                stolenDefName = spike.stolenDefName,
                stolenDefNames = [.. spike.stolenDefNames],
                storedInvestiture = spike.storedInvestiture,
                strength = spike.chargeStrength,
            });
        }

        GenPlace.TryPlaceThing(made, where, map, ThingPlaceMode.Near);
    }

    /// <summary>
    ///     Slow on purpose. A spike leaves through surgery, which is an event.
    /// </summary>
    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        if (!pawn.IsHashIntervalTick(GenTicks.TickLongInterval, delta)) return;

        KolossUtility.ReconcileSpikes(pawn);
    }
}
