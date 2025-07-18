using CosmereScadrial.Allomancy.Hediff;
using Verse;
using HediffUtility = CosmereScadrial.Util.HediffUtility;

namespace CosmereScadrial.Allomancy.Ability;

public abstract partial class AbstractAbility {
    protected AllomanticHediff? GetOrAddHediff(Pawn targetPawn) {
        return HediffUtility.GetOrAddHediff(pawn, targetPawn, this, def);
    }

    protected void RemoveHediff(Pawn? targetPawn) {
        if (targetPawn == null) return;

        HediffUtility.RemoveHediff(pawn, targetPawn, this, def);
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
        Scribe_Values.Look(ref status, "status");

        // Save/load logic
        if (Scribe.mode == LoadSaveMode.Saving) {
            bool localTargetPresent = localTarget.HasValue;
            Scribe_Values.Look(ref localTargetPresent, "targetPresent");
            if (localTargetPresent) {
                LocalTargetInfo temp = localTarget!.Value;
                Scribe_TargetInfo.Look(ref temp, "target");
            }
        } else if (Scribe.mode == LoadSaveMode.LoadingVars) {
            bool localTargetPresent = false;
            Scribe_Values.Look(ref localTargetPresent, "targetPresent");
            if (localTargetPresent) {
                LocalTargetInfo temp = LocalTargetInfo.Invalid;
                Scribe_TargetInfo.Look(ref temp, "target");
                localTarget = temp;
            } else {
                localTarget = null;
            }
        }
    }
}