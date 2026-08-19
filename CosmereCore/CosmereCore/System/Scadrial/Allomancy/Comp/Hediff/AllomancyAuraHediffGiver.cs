using Cosmere.Core;
using Cosmere.Core.Ability;
using Cosmere.Core.Comp.Map;
using Cosmere.Core.Hediff;
using Cosmere.Core.Util;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Allomancy.Hediff;
using Cosmere.System.Scadrial.Comp.Hediff;
using Cosmere.System.Scadrial.Gene;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using UnityEngine;
using Verse;
using static Cosmere.Core.Mod;

namespace Cosmere.System.Scadrial.Allomancy.Comp.Hediff;

public class AllomancyAuraHediffGiverProperties : HediffCompProperties, IMultiTypeHediff {
    /// <summary>
    ///     Whether the Allomancer is inside their own aura.
    /// </summary>
    /// <remarks>
    ///     A Rioter does not riot themself, so this is off by default. A Smoker is standing in their
    ///     own coppercloud by definition, and leaving them out made the one pawn burning copper the
    ///     only pawn it failed to hide - and stopped two overlapping Smokers from ever holding each
    ///     other's source, which is what made a doubled cloud read as a single one.
    /// </remarks>
    public bool affectsSelf;
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

    /// <summary>
    ///     How far the aura carries, rounded the way GetCellsAround rounds it so that anything
    ///     asking how big the aura is gets the size it actually applied.
    /// </summary>
    public float Reach => Mathf.Round(props.radius * base.parent.Severity);

    /// <summary>Zinc and brass reach for the mind, which is the reach copper shuts.</summary>
    private bool isEmotional =>
        parent.metal == MetallicArtsMetalDefOf.Zinc || parent.metal == MetallicArtsMetalDefOf.Brass;

    /// <summary>
    ///     The first source ability, or none. An explicit first read, not a foreach that returns on
    ///     entry - that reads like a loop but can never take a second pass.
    /// </summary>
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
        float radius = Reach;

        if (debugMode && Find.Selector.IsSelected(parent.pawn)) {
            CircleRenderer.Add(this, new CircleToRender(parent.pawn, radius, parent.metal.transparentLineColor));
        }

        if (!base.parent.pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond, delta)) {
            return;
        }

        HashSet<Pawn> nearbyPawns = [];
        if (props.affectsSelf) {
            nearbyPawns.Add(base.parent.pawn);
        }

        foreach (IntVec3 cell in base.parent.pawn.GetCellsAround(radius, true)) {
            Pawn? cellPawn = cell.GetFirstPawn(base.parent.pawn.Map);
            if (cellPawn != null && cellPawn != base.parent.pawn) {
                nearbyPawns.Add(cellPawn);
            }
        }

        foreach (Pawn pawn in nearbyPawns) {
            Act(pawn);
            TrySeize(pawn);
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

    /// <summary>
    ///     Picks up a koloss the aura happens to sweep, if this Allomancer can manage it.
    /// </summary>
    /// <remarks>
    ///     The deliberate seizure is the act the player reaches for; this is the same act happening
    ///     without ceremony when an emotional Allomancer walks through a pen. Same roster, same
    ///     slot, same threshold - convenience, not an easier route.
    ///     <para>
    ///         Silent on failure. This runs once a second on everything in range, and a koloss the
    ///         Allomancer is too weak to take would otherwise say so once a second forever.
    ///     </para>
    /// </remarks>
    private void TrySeize(Pawn? target) {
        if (target == null || ability == null) return;
        if (!isEmotional) return;
        if (Coppercloud.Hides(Pawn, target, parent.metal)) return;
        if (EmotionalResistance.Of(target) <= 0f) return;
        if (KolossControl.IsHeld(target)) return;

        AllomancyAbility burning = (AllomancyAbility)ability!;
        KolossControl.TryBind(Pawn, target, burning.GetStrength(burning.nextStatus), burning.def.metal, burning.def);
    }

    private void Act(Pawn? target) {
        if (target?.mindState == null || target.Dead || ability == null) {
            return;
        }

        // silent: runs once a second on everyone in range, so a Rioter would narrate this forever
        if (isEmotional && Coppercloud.Hides(Pawn, target, parent.metal)) {
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
