using Verse;
using HediffUtility = CosmereScadrial.Utils.HediffUtility;
using Log = CosmereFramework.Log;

namespace CosmereScadrial.Abilities.Allomancy {
    public abstract partial class AbstractAbility {
        protected void GetOrAddHediff(Pawn targetPawn) {
            HediffUtility.GetOrAddHediff(pawn, targetPawn, this, def);
        }

        protected void RemoveHediff(Pawn targetPawn) {
            HediffUtility.RemoveHediff(pawn, targetPawn, this);
        }

        protected void ApplyDrag(Pawn targetPawn, float severity) {
            if (def.dragHediff == null || severity < def.minSeverityForDrag) return;

            Log.Warning(
                $"Applying {def.dragHediff.defName} drag to {targetPawn.NameFullColored} with Severity={severity}");
            var drag = targetPawn.health.GetOrAddHediff(def.dragHediff);
            drag.Severity = severity;
        }

        protected void RemoveDrag(Pawn targetPawn) {
            var drag = targetPawn.health.hediffSet.GetFirstHediffOfDef(def.dragHediff);
            if (drag == null) return;

            var existingSeverity = drag.Severity;
            targetPawn.health.RemoveHediff(drag);
            flareStartTick -= (int)(existingSeverity * 3000f);
        }

        protected virtual void OnEnable() { }

        protected virtual void OnDisable() { }

        protected virtual void OnFlare() { }

        protected virtual void OnDeFlare() { }

        public override void ExposeData() {
            base.ExposeData();

            Scribe_Values.Look(ref flareStartTick, "flareStartTick", -1);
            Scribe_Values.Look(ref status, "status");
            Scribe_TargetInfo.Look(ref target, "target");
        }
    }
}