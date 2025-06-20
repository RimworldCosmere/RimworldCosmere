using System;
using System.Collections.Generic;
using System.Linq;
using CosmereFramework.Utils;
using CosmereScadrial.Comps.Hediffs;
using CosmereScadrial.Utils;
using RimWorld;
using UnityEngine;
using Verse;
using HediffUtility = CosmereScadrial.Utils.HediffUtility;
using static CosmereFramework.CosmereFramework;

namespace CosmereScadrial.Abilities.Allomancy.Hediffs.Comps;

public class AllomancyAuraHediffGiverProperties : HediffCompProperties, MultiTypeHediff {
    public HediffDef hediff;
    public HediffDef hediffFriendly;
    public HediffDef hediffHostile;
    public float radius = 12f;
    public string verb;

    public AllomancyAuraHediffGiverProperties() {
        compClass = typeof(AllomancyAuraHediffGiver);
    }

    public HediffDef getHediff() {
        return hediff;
    }

    public HediffDef getFriendlyHediff() {
        return hediffFriendly;
    }

    public HediffDef getHostileHediff() {
        return hediffHostile;
    }
}

public class AllomancyAuraHediffGiver : HediffComp {
    private Mote mote;

    private new AllomancyAuraHediffGiverProperties props => (AllomancyAuraHediffGiverProperties)base.props;

    private new AllomanticHediff parent => base.parent as AllomanticHediff;

    private bool isAtLeastPassive => parent.Severity >= 0.5f;

    private AbstractAbility ability => parent.sourceAbilities.First(_ => true);

    private float moteScale =>
        MoteUtility.GetMoteSize(ThingDefOf.Cosmere_Mote_Allomancy_Aura, props.radius, parent.Severity);

    public override void CompPostMake() {
        base.CompPostMake();
        CreateMote();
    }

    public override void CompPostTick(ref float severityAdjustment) {
        if (!isAtLeastPassive) {
            return;
        }

        base.CompPostTick(ref severityAdjustment);
        float radius = props.radius * base.parent.Severity;
        IntVec3 center = base.parent.pawn.Position;
        Map? map = base.parent.pawn.Map;

        if (map == null || !center.IsValid) {
            return;
        }

        if (CosmereSettings.debugMode) {
            GenDraw.DrawCircleOutline(parent.pawn.DrawPos, radius, parent.metal.TransparentLineColor);
        }

        CreateMote()?.Maintain();
        mote.Scale = moteScale;
        // mote.instanceColor = parent.metal.color;

        if (!base.parent.pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond)) {
            return;
        }

        IEnumerable<Pawn> nearbyPawns = GenRadial
            .RadialCellsAround(center, Math.Min(radius, GenRadial.MaxRadialPatternRadius), true)
            .Select(cell => cell.GetFirstPawn(map))
            .Where(p => p != null && p != base.parent.pawn);

        foreach (Pawn? pawn in nearbyPawns) {
            TryAct(pawn);
        }
    }

    private Mote CreateMote() {
        if (mote?.Destroyed == false) {
            return mote;
        }

        ThingDef moteDef = ThingDefOf.Cosmere_Mote_Allomancy_Aura;
        moteDef.graphicData.color = parent.metal.color;

        mote = MoteMaker.MakeAttachedOverlay(
            base.parent.pawn,
            moteDef,
            Vector3.zero,
            moteScale
        );

        return mote;
    }

    private void TryAct(Pawn target) {
        if (target?.mindState == null || target.Dead) {
            return;
        }

        AllomanticHediff hediff = HediffUtility.GetOrAddHediff(Pawn, target, ability, props);

        // Reset the Disappears timer
        if (hediff.TryGetComp<DisappearsScaled>(out DisappearsScaled? disappearsComp)) {
            disappearsComp.CompPostMake();
        }

        // Update the mood offset
        if (hediff.TryGetComp<HediffComp_ThoughtSetter>(out HediffComp_ThoughtSetter? thoughtComp)) {
            float offset = hediff.ageTicks / TickUtility.Minutes(5) * hediff.Severity; // Jumps for every hour
            int newOffset = Mathf.RoundToInt(Mathf.Clamp(offset, 2f, 10f));
            thoughtComp.OverrideMoodOffset(newOffset);
        }

        if (hediff.ageTicks >= GenTicks.TicksPerRealSecond) return;

        MoteMaker.ThrowText(target.DrawPos, target.Map, props.verb, Color.cyan);
    }
}