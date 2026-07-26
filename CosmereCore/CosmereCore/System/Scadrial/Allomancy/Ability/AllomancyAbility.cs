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
            Allomancer? found = pawn.genes?.GetAllomanticGeneForMetal(metal);
            if (found == null) {
                throw new InvalidOperationException(
                    $"AllomancyAbility on {pawn.LabelShort} could not find Allomancer gene for {metal?.defName}"
                );
            }

            cachedGene = found;
            return cachedGene;
        }
    }

    public float flareDuration => flareStartTick < 0 ? 0 : Find.TickManager.TicksGame - flareStartTick;

    public new AllomanticAbilityDef def {
        get => (AllomanticAbilityDef)base.def;
        set => base.def = value;
    }

    public bool atLeastBurning => status.power >= 1;
    public MetallicArtsMetalDef metal => def.metal;

    public override float GetDesiredBurnRateForStatus(Status? desiredStatus) {
        return status.power == DuraluminBurnPower ? 0.00000000001f : base.GetDesiredBurnRateForStatus(desiredStatus);
    }

    public override float GetStrength(Status? desiredStatus = null) {
        float statusValue = status.power == DuraluminBurnPower
            ? pawn.GetAllomanticReservePercent(MetalDefOf.Duralumin) * 10f
            : base.GetStrength(desiredStatus);

        return statusValue * pawn.GetRawAllomanticPower(metal);
    }

    protected override void OnEnable() {
        base.OnEnable();
        if (def.hediff != null && !def.targetRequired) {
            GetOrAddHediff(pawn);
        }
    }

    protected override void OnDisable() {
        if (def.hediff != null && !def.targetRequired) {
            RemoveHediff(pawn);
        }

        base.OnDisable();
    }

    protected override void OnPowerUp() {
        base.OnPowerUp();
        flareStartTick = Find.TickManager.TicksGame;
        Pawn target = localTarget.HasValue ? localTarget.Value.Pawn ?? pawn : pawn;
        if (target == pawn || def.applyDragOnTarget) {
            RemoveDrag(target);
        }
    }

    protected override void OnPowerDown() {
        base.OnPowerDown();
        if (flareStartTick < 0) return;
        Pawn target = localTarget.HasValue ? localTarget.Value.Pawn ?? pawn : pawn;
        if (target == pawn || def.applyDragOnTarget) {
            ApplyDrag(target, flareDuration / 3000f / 2);
        }

        flareStartTick = -1;
    }

    public override AcceptanceReport CanCast {
        get {
            if (!ShardUtility.AreAnyEnabled(ShardDefOf.Preservation, ShardDefOf.Ruin, ShardDefOf.Harmony)) {
                return "CS_Allomancy_NoShardPresent".Translate();
            }

            return base.CanCast;
        }
    }

    public override bool GizmosVisible() {
        if (!base.GizmosVisible()) return false;
        if (!ShardUtility.AreAnyEnabled(ShardDefOf.Preservation, ShardDefOf.Ruin, ShardDefOf.Harmony)) return false;

        return true;
    }

    protected void ApplyDrag(Pawn? targetPawn, float severity) {
        if (targetPawn == null || def.dragHediff == null || severity < def.minSeverityForDrag) return;

        Verse.Hediff? drag = targetPawn.health.GetOrAddHediff(def.dragHediff);
        drag.Severity = severity;
    }

    protected void RemoveDrag(Pawn? targetPawn) {
        Verse.Hediff? drag = targetPawn?.health.hediffSet.GetFirstHediffOfDef(def.dragHediff);
        if (drag == null) return;

        float existingSeverity = drag.Severity;
        targetPawn!.health.RemoveHediff(drag);
        flareStartTick -= (int)(existingSeverity * 3000f);
    }

    protected AllomanticHediff? GetOrAddHediff(Pawn targetPawn) {
        return HediffUtility.GetOrAddHediff(pawn, targetPawn, this, def.hediff);
    }

    protected void RemoveHediff(Pawn? targetPawn) {
        if (targetPawn == null) return;

        HediffUtility.RemoveHediff(pawn, targetPawn, this, def.hediff);
    }

    public override void ExposeData() {
        base.ExposeData();

        Scribe_Values.Look(ref flareStartTick, "flareStartTick", -1);
    }
}