using Cosmere.Core.Ability;
using Cosmere.Resources;
using Cosmere.Scadrial.Allomancy.Hediff;
using Cosmere.Scadrial.Def;
using Cosmere.Scadrial.Gene;
using RimWorld;
using Verse;

namespace Cosmere.Scadrial.Allomancy.Ability;

public abstract class AbstractAllomancyAbility : AbstractAbility<Allomancer, AllomanticHediff> {
    private const int DURALUMIN_BURN_POWER = 10;

    protected int flareStartTick = -1;


    protected AbstractAllomancyAbility() { }
    protected AbstractAllomancyAbility(Pawn pawn) : base(pawn) { }
    protected AbstractAllomancyAbility(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }
    protected AbstractAllomancyAbility(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    protected AbstractAllomancyAbility(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(
        pawn,
        sourcePrecept,
        def
    ) { }

    public override Allomancer gene => cachedGene ??= pawn.genes.GetAllomanticGeneForMetal(metal)!;
    public float flareDuration => flareStartTick < 0 ? 0 : Find.TickManager.TicksGame - flareStartTick;

    public new AllomanticAbilityDef def {
        get => (AllomanticAbilityDef)base.def;
        set => base.def = value;
    }

    public bool atLeastBurning => status.power >= 1;
    public MetallicArtsMetalDef metal => def.metal;

    public override float GetDesiredBurnRateForStatus(Status? desiredStatus) {
        return status.power == DURALUMIN_BURN_POWER ? 0.00000000001f : base.GetDesiredBurnRateForStatus(desiredStatus);
    }

    public override float GetStrength(Status? desiredStatus = null) {
        float statusValue = status.power == DURALUMIN_BURN_POWER
            ? pawn.GetAllomanticReservePercent(MetalDefOf.Duralumin) * 10f
            : base.GetStrength(desiredStatus);

        return statusValue * pawn.GetRawAllomanticPower();
    }

    protected override void OnStatusChanged(Status oldStatus, Status newStatus) {
        gene.UpdateBurnSource((def, GetDesiredBurnRateForStatus(newStatus)));
        base.OnStatusChanged(oldStatus, newStatus);
    }

    protected override void OnEnable() { }

    protected override void OnDisable() {
        nextStatus = null;
    }

    protected override void OnPowerUp() { }

    protected override void OnPowerDown() { }

    public new bool GizmosVisible() {
        if (!base.GizmosVisible()) return false;

        if (!def.defName.Contains("Cosmere_Scadrial_Ability_Compound")) return true;

        return pawn.genes.HasAllomanticGeneForMetal(metal) && pawn.genes.HasFeruchemicGeneForMetal(metal);
    }


    protected void ApplyDrag(Pawn? targetPawn, float severity) {
        if (targetPawn == null || def.dragHediff == null || severity < def.minSeverityForDrag) return;

        Verse.Hediff? drag = targetPawn.health.GetOrAddHediff(def.dragHediff);
        drag.Severity = severity;
        //Logger.Warning($"Applying {def.dragHediff.defName} drag to {targetPawn.NameFullColored} with Severity={severity}");
    }

    protected void RemoveDrag(Pawn? targetPawn) {
        Verse.Hediff? drag = targetPawn?.health.hediffSet.GetFirstHediffOfDef(def.dragHediff);
        if (drag == null) return;

        float existingSeverity = drag.Severity;
        targetPawn!.health.RemoveHediff(drag);
        flareStartTick -= (int)(existingSeverity * 3000f);
    }

    public override void ExposeData() {
        base.ExposeData();

        Scribe_Values.Look(ref flareStartTick, "flareStartTick", -1);
    }
}