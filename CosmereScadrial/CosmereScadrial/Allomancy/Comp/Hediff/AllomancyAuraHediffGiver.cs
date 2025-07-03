using System;
using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Allomancy.Ability;
using CosmereScadrial.Allomancy.Hediff;
using CosmereScadrial.Comp.Hediff;
using CosmereScadrial.Util;
using RimWorld;
using UnityEngine;
using Verse;
using HediffUtility = CosmereScadrial.Util.HediffUtility;
using static CosmereFramework.CosmereFramework;

namespace CosmereScadrial.Allomancy.Comp.Hediff;

public class AllomancyAuraHediffGiverProperties : HediffCompProperties, IMultiTypeHediff {
    public HediffDef? hediff;
    public HediffDef? hediffFriendly;
    public HediffDef? hediffHostile;
    public ThingDef? moteDef;
    public float radius = 12f;
    public string? verb;

    public AllomancyAuraHediffGiverProperties() {
        compClass = typeof(AllomancyAuraHediffGiver);
    }

    public HediffDef? GetHediff() {
        return hediff;
    }

    public HediffDef? GetFriendlyHediff() {
        return hediffFriendly;
    }

    public HediffDef? GetHostileHediff() {
        return hediffHostile;
    }
}

public class AllomancyAuraHediffGiver : HediffComp {
    private Mote? mote;

    private new AllomancyAuraHediffGiverProperties props => (AllomancyAuraHediffGiverProperties)base.props;

    private new AllomanticHediff parent => (AllomanticHediff)base.parent;

    private bool isAtLeastPassive => parent.Severity >= 0.5f;

    private AbstractAbility? ability => parent.sourceAbilities.FirstOrDefault(_ => true);

    private float moteScale =>
        props.moteDef == null ? 1f : MoteUtility.GetMoteSize(props.moteDef, props.radius, parent.Severity);

    public override void CompPostMake() {
        base.CompPostMake();
        CreateMote();
    }

    public override void CompPostTickInterval(ref float severityAdjustment, int delta) {
        if (!isAtLeastPassive) {
            return;
        }

        base.CompPostTickInterval(ref severityAdjustment, delta);
        float radius = props.radius * base.parent.Severity;
        IntVec3 center = base.parent.pawn.Position;
        Map? map = base.parent.pawn.Map;

        if (map == null || !center.IsValid) {
            return;
        }

        if (!base.parent.pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond, delta)) {
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

    public override void CompPostTick(ref float severityAdjustment) {
        base.CompPostTick(ref severityAdjustment);
        if (cosmereSettings.debugMode && Find.Selector.IsSelected(parent.pawn)) {
            GenDraw.DrawCircleOutline(
                parent.pawn.DrawPos,
                props.radius * base.parent.Severity,
                parent.metal.transparentLineColor
            );
        }

        CreateMote()?.Maintain();
        if (mote != null) mote.Scale = moteScale;
    }

    private Mote? CreateMote() {
        if (props.moteDef == null) return null;

        if (mote?.Destroyed == false) {
            return mote;
        }

        /*
        props.moteDef.graphicData.color = parent.metal.color;
        if (parent.metal.colorTwo != null) props.moteDef.graphicData.colorTwo = parent.metal.colorTwo.Value;
        */

        mote = MoteMaker.MakeAttachedOverlay(
            base.parent.pawn,
            props.moteDef,
            Vector3.zero,
            moteScale
        );

        return mote;
    }

    private void TryAct(Pawn? target) {
        if (target?.mindState == null || target.Dead || ability == null) {
            return;
        }

        AllomanticHediff? hediff = HediffUtility.GetOrAddHediff(Pawn, target, ability, props);
        if (hediff == null) return;

        // Reset the Disappears timer
        if (hediff.TryGetComp<DisappearsScaled>(out DisappearsScaled? disappearsComp)) {
            disappearsComp.CompPostMake();
        }

        // Update the mood offset
        if (hediff.TryGetComp<HediffComp_ThoughtSetter>(out HediffComp_ThoughtSetter? thoughtComp)) {
            float offset =
                hediff.ageTicks / (float)GenTicks.SecondsToTicks(60 * 5) * hediff.Severity; // Jumps for every hour
            int newOffset = Mathf.RoundToInt(Mathf.Clamp(offset, 2f, 10f));
            thoughtComp.OverrideMoodOffset(newOffset);
        }

        if (hediff.ageTicks >= GenTicks.TicksPerRealSecond) return;

        MoteMaker.ThrowText(target.DrawPos, target.Map, props.verb, Color.cyan);
    }
}