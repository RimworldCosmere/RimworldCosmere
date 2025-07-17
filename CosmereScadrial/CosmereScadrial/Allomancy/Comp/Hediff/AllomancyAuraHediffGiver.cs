using System.Collections.Generic;
using System.Linq;
using CosmereFramework.Comp.Map;
using CosmereFramework.Extension;
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
    private readonly List<Pawn> pawnsWithHediff = [];
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

        if (debugMode && Find.Selector.IsSelected(parent.pawn)) {
            CircleRenderer.TryAdd(this, new CircleToRender(parent.pawn, radius, parent.metal.transparentLineColor));
        }

        if (!base.parent.pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond, delta)) {
            return;
        }

        List<Pawn> nearbyPawns = base.parent.pawn.GetCellsAround(radius, true)
            .Select(c => c.GetFirstPawn(base.parent.pawn.Map))
            .Where(p => p != base.parent.pawn)
            .ToList();
        foreach (Pawn? pawn in nearbyPawns) {
            TryAct(pawn);
        }

        foreach (Pawn pawn in pawnsWithHediff.ToList()) {
            if (nearbyPawns.Contains(pawn)) continue;
            AllomanticHediff? hediff = HediffUtility.GetOrAddHediff(Pawn, pawn, ability, props);
            if (hediff != null) {
                hediff.RemoveSource(ability);
            }

            pawnsWithHediff.Remove(pawn);
        }
    }

    public override void CompPostPostRemoved() {
        base.CompPostPostRemoved();
        CircleRenderer.TryRemove(this);
    }

    public override void CompPostTick(ref float severityAdjustment) {
        base.CompPostTick(ref severityAdjustment);

        CreateMote()?.Maintain();
        if (mote != null) {
            mote.Graphic.drawSize = new Vector2(moteScale, moteScale);
        }
    }

    private Mote? CreateMote() {
        if (props.moteDef == null) return null;

        if (mote?.Destroyed == false) {
            return mote;
        }

        return mote ??= MoteMaker.MakeAttachedOverlay(
            base.parent.pawn,
            props.moteDef,
            Vector3.zero,
            moteScale
        );
    }

    private void TryAct(Pawn? target) {
        if (target?.mindState == null || target.Dead || ability == null) {
            return;
        }

        AllomanticHediff? hediff = HediffUtility.GetOrAddHediff(Pawn, target, ability, props);
        if (hediff == null) return;
        pawnsWithHediff.Add(target);

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