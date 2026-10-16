using System;
using Cosmere.Core;
using Cosmere.Core.Ability;
using Cosmere.Core.Util;
using Cosmere.System.Scadrial.Allomancy.Hediff;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using Verse;
using HediffUtility = Cosmere.System.Scadrial.Util.HediffUtility;

namespace Cosmere.System.Scadrial.Allomancy.Ability;

public class AllomancyAbility : AbstractAbility<Allomancer, AllomanticHediff> {
    private const int DuraluminBurnPower = 10;

    protected int flareStartTick = -1;

    public AllomancyAbility(Pawn pawn) : base(pawn) { }

    public AllomancyAbility(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    public override Allomancer Gene {
        get {
            if (cachedGene != null) return cachedGene;
            Allomancer? found = pawn.genes?.GetAllomanticGeneForMetal(Metal);
            if (found == null) {
                throw new InvalidOperationException(
                    $"AllomancyAbility on {pawn.LabelShort} could not find Allomancer gene for {Metal?.defName}"
                );
            }

            cachedGene = found;
            return cachedGene;
        }
    }

    public float FlareDuration => flareStartTick < 0 ? 0 : Find.TickManager.TicksGame - flareStartTick;

    public new AllomanticAbilityDef Def {
        get => (AllomanticAbilityDef)base.Def;
        set => base.Def = value;
    }

    public bool AtLeastBurning => status.power >= 1;

    public MetallicArtsMetalDef Metal => Def.metal;

    public override float GetDesiredBurnRateForStatus(Status? desiredStatus) {
        return status.power == DuraluminBurnPower ? 0.00000000001f : base.GetDesiredBurnRateForStatus(desiredStatus);
    }

    public override float GetStrength(Status? desiredStatus = null) {
        float statusValue = status.power == DuraluminBurnPower
            ? pawn.GetAllomanticReservePercent(MetalDefOf.Duralumin) * 10f
            : base.GetStrength(desiredStatus);

        return statusValue * pawn.GetRawAllomanticPower(Metal);
    }

    protected override void OnEnable() {
        base.OnEnable();
        if (Def.hediff != null && !Def.targetRequired) {
            GetOrAddHediff(pawn);
        }
    }

    protected override void OnDisable() {
        if (Def.hediff != null && !Def.targetRequired) {
            RemoveHediff(pawn);
        }

        base.OnDisable();
    }

    protected override void OnPowerUp() {
        base.OnPowerUp();
        flareStartTick = Find.TickManager.TicksGame;
        Pawn target = localTarget.HasValue ? localTarget.Value.Pawn ?? pawn : pawn;
        if (target == pawn || Def.applyDragOnTarget) {
            RemoveDrag(target);
        }
    }

    protected override void OnPowerDown() {
        base.OnPowerDown();
        if (flareStartTick < 0) return;
        Pawn target = localTarget.HasValue ? localTarget.Value.Pawn ?? pawn : pawn;
        if (target == pawn || Def.applyDragOnTarget) {
            ApplyDrag(target, FlareDuration / 3000f / 2);
        }

        flareStartTick = -1;
    }

    /// <summary>
    ///     Gets whether the burn can start, priced at the status the cast would move to so a flare is
    ///     charged as a flare. The base asks only whether the reserve covers one tick, which let the
    ///     wheel start a burn the gizmo refused.
    /// </summary>
    public override AcceptanceReport CanCast {
        get {
            if (!ShardUtility.AreAnyEnabled(ShardDefOf.Preservation, ShardDefOf.Ruin, ShardDefOf.Harmony)) {
                return "CS_Allomancy_NoShardPresent".Translate();
            }

            return Gene.CanBurn(GetDesiredBurnRateForStatus(NextStatus ?? Status.PowerOne));
        }
    }

    public override bool GizmosVisible() {
        if (!base.GizmosVisible()) return false;
        if (!ShardUtility.AreAnyEnabled(ShardDefOf.Preservation, ShardDefOf.Ruin, ShardDefOf.Harmony)) return false;

        return true;
    }

    protected void ApplyDrag(Pawn? targetPawn, float severity) {
        if (targetPawn == null || Def.dragHediff == null || severity < Def.minSeverityForDrag) return;

        Verse.Hediff? drag = targetPawn.health.GetOrAddHediff(Def.dragHediff);
        drag.Severity = severity;
    }

    protected void RemoveDrag(Pawn? targetPawn) {
        Verse.Hediff? drag = targetPawn?.health.hediffSet.GetFirstHediffOfDef(Def.dragHediff);
        if (drag == null) return;

        float existingSeverity = drag.Severity;
        targetPawn!.health.RemoveHediff(drag);
        flareStartTick -= (int)(existingSeverity * 3000f);
    }

    protected AllomanticHediff? GetOrAddHediff(Pawn targetPawn) {
        return HediffUtility.GetOrAddHediff(pawn, targetPawn, this, Def.hediff);
    }

    protected void RemoveHediff(Pawn? targetPawn) {
        if (targetPawn == null) return;

        HediffUtility.RemoveHediff(pawn, targetPawn, this, Def.hediff);
    }

    public override void ExposeData() {
        base.ExposeData();

        Scribe_Values.Look(ref flareStartTick, "flareStartTick", -1);
    }
}
