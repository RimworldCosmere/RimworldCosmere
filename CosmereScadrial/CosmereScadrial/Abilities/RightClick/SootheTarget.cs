using CosmereScadrial.Defs;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereScadrial.Abilities.RightClick {
    public class SootheTarget(RightClickAbilityDef def) : RightClickAbility(def) {
        public override bool CanUse(Pawn caster, LocalTargetInfo target) {
            if (!target.HasThing || !(target.Thing is Pawn targetPawn)) return false;
            if (targetPawn.Dead || targetPawn.Downed) return false;

            return caster.CanReach(target, PathEndMode.Touch, Danger.Deadly);
        }

        public override void Execute(Pawn caster, LocalTargetInfo target) {
            if (target.Thing is not Pawn targetPawn) return;
            if (targetPawn.Dead) return;

            // Apply Soothe Hediff here
            var hediff = HediffMaker.MakeHediff(HediffDef.Named("Cosmere_SootheHediff"), targetPawn);
            targetPawn.health.AddHediff(hediff);

            MoteMaker.ThrowText(targetPawn.DrawPos, targetPawn.Map, "Soothed", 3.65f);
        }
    }
}