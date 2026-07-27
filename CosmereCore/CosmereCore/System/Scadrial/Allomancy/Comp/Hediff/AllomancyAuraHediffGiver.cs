using Cosmere.Core;
using Cosmere.Core.Ability;
using Cosmere.Core.Comp.Map;
using Cosmere.Core.Hediff;
using Cosmere.Core.Util;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Allomancy.Hediff;
using Cosmere.System.Scadrial.Comp.Hediff;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;
using static Cosmere.Core.Mod;

namespace Cosmere.System.Scadrial.Allomancy.Comp.Hediff;

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
    private readonly HashSet<Pawn> pawnsWithHediff = [];
    private Mote? mote;

    private new AllomancyAuraHediffGiverProperties props => (AllomancyAuraHediffGiverProperties)base.props;

    private new AllomanticHediff parent => (AllomanticHediff)base.parent;

    private bool isAtLeastPassive => parent.Severity >= 0.5f;

    // The first source ability, or none. Written as an explicit first read
    // rather than a foreach that returns on entry, which read as a loop but
    // could never take a second pass.
    private IAbility<Allomancer, AllomanticHediff>? ability {
        get {
            using HashSet<IAbility<Allomancer, IHediff<Allomancer>>>.Enumerator e =
                parent.SourceAbilities.GetEnumerator();

            return e.MoveNext() ? (IAbility<Allomancer, AllomanticHediff>?)e.Current : null;
        }
    }

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
            CircleRenderer.Add(this, new CircleToRender(parent.pawn, radius, parent.metal.transparentLineColor));
        }

        if (!base.parent.pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond, delta)) {
            return;
        }

        HashSet<Pawn> nearbyPawns = [];
        foreach (IntVec3 cell in base.parent.pawn.GetCellsAround(radius, true)) {
            Pawn? cellPawn = cell.GetFirstPawn(base.parent.pawn.Map);
            if (cellPawn != null && cellPawn != base.parent.pawn) {
                nearbyPawns.Add(cellPawn);
            }
        }

        foreach (Pawn pawn in nearbyPawns) {
            Act(pawn);
        }

        List<Pawn> toRemove = [];
        foreach (Pawn pawn in pawnsWithHediff) {
            if (nearbyPawns.Contains(pawn)) continue;
            if (ability != null) {
                AllomanticHediff? hediff = (AllomanticHediff?)pawn.GetOrAddHediff(
                    (AllomancyAbility)ability!,
                    props,
                    Pawn
                );
                hediff?.RemoveSource(ability);
            }

            toRemove.Add(pawn);
        }

        for (int i = 0; i < toRemove.Count; i++) {
            pawnsWithHediff.Remove(toRemove[i]);
        }
    }

    public override void CompPostPostRemoved() {
        base.CompPostPostRemoved();
        CircleRenderer.Remove(this);
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

    private void Act(Pawn? target) {
        if (target?.mindState == null || target.Dead || ability == null) {
            return;
        }

        AllomanticHediff? hediff = (AllomanticHediff?)target.GetOrAddHediff((AllomancyAbility)ability!, props, Pawn);
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
