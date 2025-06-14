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

        protected sealed override bool toggleable => false;

        public override bool CanApplyOn(LocalTargetInfo targetInfo) {
            if (!base.CanApplyOn(targetInfo)) return false;
            var shooting = pawn.skills.GetSkill(SkillDefOf.Shooting);
            if (shooting.TotallyDisabled) return false;

            return pawn.inventory?.innerContainer.Contains(CoinThingDefOf.Cosmere_Clip) ?? false;
        }

        public override bool CanActivate(LocalTargetInfo targetInfo, BurningStatus activationStatus,
            out string reason, bool ignoreInvestiture) {
            if (!base.CanActivate(targetInfo, activationStatus, out reason, true)) return false;


            var shooting = pawn.skills.GetSkill(SkillDefOf.Shooting);
            if (!shooting.PermanentlyDisabled && !shooting.TotallyDisabled) return true;

            reason = "MenuCannotShoot".Translate();
            return false;
        }

        public override bool Activate(LocalTargetInfo targetInfo, LocalTargetInfo dest) {
            target = targetInfo;
            UpdateStatus(shouldFlare ? BurningStatus.Flaring : BurningStatus.Burning);

            return base.Activate(target, dest);
        }
    }
}