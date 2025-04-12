using CosmereScadrial.Comps;
using CosmereScadrial.Utils;
using RimWorld;
using Verse;

namespace CosmereScadrial.Abilities {
    public class CompProperties_AbilityBurnMetal : CompProperties_AbilityEffect {
        public HediffDef applyHediff;
        public float beuCost;
        public float hediffSeverity = 1f;
        public string metal;

        public CompProperties_AbilityBurnMetal() {
            compClass = typeof(CompAbilityEffect_BurnMetal);
        }
    }

    public class CompAbilityEffect_BurnMetal : CompAbilityEffect {
        public new CompProperties_AbilityBurnMetal Props {
            get => (CompProperties_AbilityBurnMetal)props;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
            var pawn = parent.pawn;

            if (AllomancyUtility.TryBurnMetalForInvestiture(pawn, Props.metal, Props.beuCost)) {
                if (Props.applyHediff == null || pawn.health.hediffSet.GetFirstHediffOfDef(Props.applyHediff) != null) return;

                var hediff = HediffMaker.MakeHediff(Props.applyHediff, pawn);
                hediff.Severity = Props.hediffSeverity;
                pawn.health.AddHediff(hediff);
                return;
            }

            Messages.Message($"{pawn.NameShortColored} lacks enough {Props.metal} reserve to burn.", pawn, MessageTypeDefOf.RejectInput);
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest) {
            if (!base.CanApplyOn(target, dest)) return false;

            var comp = parent.pawn.GetComp<CompScadrialInvestiture>();
            if (comp == null) return false;

            comp.metalReserves.TryGetValue(Props.metal, out var available);

            return available >= AllomancyUtility.GetMetalNeededForBEU(Props.beuCost);
        }
    }
}