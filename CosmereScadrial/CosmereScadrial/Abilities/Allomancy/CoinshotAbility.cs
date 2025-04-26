using RimWorld;
using Verse;

namespace CosmereScadrial.Abilities.Allomancy {
    public class CoinshotAbility : AbilityOtherTarget {
        public CoinshotAbility() {
            status = BurningStatus.Off;
        }

        public CoinshotAbility(Pawn pawn) : base(pawn) {
            status = BurningStatus.Off;
        }

        public CoinshotAbility(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) {
            status = BurningStatus.Off;
        }

        public CoinshotAbility(Pawn pawn, AbilityDef def) : base(pawn, def) {
            status = BurningStatus.Off;
        }

        public CoinshotAbility(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) {
            status = BurningStatus.Off;
        }

        protected override sealed bool toggleable => false;

        public override bool CanApplyOn(LocalTargetInfo target) {
            if (!base.CanApplyOn(target)) return false;

            return pawn.inventory?.innerContainer.Contains(CoinThingDefOf.Cosmere_Clip) ?? false;
        }

        public override bool Activate(LocalTargetInfo targetInfo, LocalTargetInfo dest, bool flare) {
            target = targetInfo;
            UpdateStatus(flare ? BurningStatus.Flaring : BurningStatus.Burning);

            return base.Activate(target, dest, flare);
        }
    }
}