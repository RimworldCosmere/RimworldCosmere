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

    private AllomancyAuraHediffGiverProperties Props => (AllomancyAuraHediffGiverProperties)props;

    private AllomanticHediff Parent => (AllomanticHediff)parent;

    private bool IsAtLeastPassive => Parent.Severity >= 0.5f;

    /// <summary>
    ///     Gets how far the aura carries, rounded the way GetCellsAround rounds it so that anything
    ///     asking how big the aura is gets the size it actually applied.
    /// </summary>
    public float Reach => Mathf.Round(Props.radius * parent.Severity);

    /// <summary>Gets a value indicating whether this aura reaches for the mind. Zinc and brass do, and that is the reach copper shuts.</summary>
    private bool IsEmotional =>
        Parent.Metal == MetallicArtsMetalDefOf.Zinc || Parent.Metal == MetallicArtsMetalDefOf.Brass;

    /// <summary>
    ///     Gets the first source ability, or none. An explicit first read, not a foreach that returns on
    ///     entry - that reads like a loop but can never take a second pass.
    /// </summary>
    private IAbility<Allomancer, AllomanticHediff>? Ability {
        get {
            using HashSet<IAbility<Allomancer, IHediff<Allomancer>>>.Enumerator e =
                Parent.SourceAbilities.GetEnumerator();

            return e.MoveNext() ? (IAbility<Allomancer, AllomanticHediff>?)e.Current : null;
        }
    }

    private float MoteScale =>
        Props.moteDef == null ? 1f : MoteUtility.GetMoteSize(Props.moteDef, Props.radius, Parent.Severity);

    public override void CompPostMake() {
        base.CompPostMake();
        CreateMote();
    }

    public override void CompPostTickInterval(ref float severityAdjustment, int delta) {
        if (!IsAtLeastPassive) {
            return;
        }

        base.CompPostTickInterval(ref severityAdjustment, delta);
        float radius = Reach;

        if (DebugMode && Find.Selector.IsSelected(Parent.pawn)) {
            CircleRenderer.Add(this, new CircleToRender(Parent.pawn, radius, Parent.Metal.TransparentLineColor));
        }

        if (!parent.pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond, delta)) {
            return;
        }

        HashSet<Pawn> nearbyPawns = [];
        if (Props.affectsSelf) {
            nearbyPawns.Add(parent.pawn);
        }

        foreach (IntVec3 cell in parent.pawn.GetCellsAround(radius, true)) {
            Pawn? cellPawn = cell.GetFirstPawn(parent.pawn.Map);
            if (cellPawn != null && cellPawn != parent.pawn) {
                nearbyPawns.Add(cellPawn);
            }
        }

        foreach (Pawn pawn in nearbyPawns) {
            Act(pawn);
            TrySeize(pawn);
        }

        ReleasePawnsOutOfReach(radius);
    }

    /// <summary>Drops the source from everyone who walked out of the aura, and forgets them.</summary>
    private void ReleasePawnsOutOfReach(float radius) {
        List<Pawn> toRemove = [];
        foreach (Pawn pawn in pawnsWithHediff) {
            if (StillInReach(pawn, radius)) continue;
            if (Ability != null) {
                AllomanticHediff? hediff = pawn.GetOrAddHediff(
                    (AllomancyAbility)Ability,
                    Props,
                    Pawn
                );
                hediff?.RemoveSource(Ability);
            }

            toRemove.Add(pawn);
        }

        for (int i = 0; i < toRemove.Count; i++) {
            pawnsWithHediff.Remove(toRemove[i]);
        }
    }

    /// <summary>
    ///     Measured, not read off the sweep above. The sweep sees one pawn per cell, so a pawn
    ///     sharing a cell reads as gone there and would lose a source it never walked out of.
    /// </summary>
    private bool StillInReach(Pawn other, float radius) {
        Pawn self = parent.pawn;
        if (other == self) return Props.affectsSelf;
        if (!other.Spawned || other.Map != self.Map) return false;

        return self.DistanceTo(other.Position) <= radius;
    }

    public override void CompPostPostRemoved() {
        base.CompPostPostRemoved();
        CircleRenderer.Remove(this);
    }

    public override void CompPostTick(ref float severityAdjustment) {
        base.CompPostTick(ref severityAdjustment);

        CreateMote()?.Maintain();
        if (mote != null) {
            mote.Graphic.drawSize = new Vector2(MoteScale, MoteScale);
        }
    }

    private Mote? CreateMote() {
        if (Props.moteDef == null) return null;

        if (mote?.Destroyed == false) {
            return mote;
        }

        return mote ??= MoteMaker.MakeAttachedOverlay(
            parent.pawn,
            Props.moteDef,
            Vector3.zero,
            MoteScale
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
        if (target == null || Ability == null) return;
        if (!IsEmotional) return;
        if (Coppercloud.Hides(Pawn, target, Parent.Metal)) return;
        if (EmotionalResistance.Of(target) <= 0f) return;
        if (KolossControl.IsHeld(target)) return;

        AllomancyAbility burning = (AllomancyAbility)Ability;
        KolossControl.TryBind(Pawn, target, burning.GetStrength(burning.NextStatus), burning.Def.metal, burning.Def);
    }

    private void Act(Pawn? target) {
        if (target?.mindState == null || target.Dead || Ability == null) {
            return;
        }

        // silent: runs once a second on everyone in range, so a Rioter would narrate this forever
        if (IsEmotional && Coppercloud.Hides(Pawn, target, Parent.Metal)) {
            return;
        }

        AllomanticHediff? hediff = target.GetOrAddHediff((AllomancyAbility)Ability, Props, Pawn);
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

        MoteMaker.ThrowText(target.DrawPos, target.Map, Props.verb, Color.cyan);
    }
}
