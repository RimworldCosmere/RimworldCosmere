using CosmereScadrial.Utils;
using RimWorld;
using Verse;

namespace CosmereScadrial.Abilities.Allomancy {
    public class ExternalPhysicalTargetAbility : AbilityOtherTarget {
        public ExternalPhysicalTargetAbility() { }

        public ExternalPhysicalTargetAbility(Pawn pawn) : base(pawn) { }

        public ExternalPhysicalTargetAbility(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }

        public ExternalPhysicalTargetAbility(Pawn pawn, AbilityDef def) : base(pawn, def) { }

        public ExternalPhysicalTargetAbility(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) { }

        protected override sealed bool toggleable => false;

        public override bool CanApplyOn(LocalTargetInfo target) {
            if (!base.CanApplyOn(target) || !target.HasThing || !MetalDetector.IsCapableOfHavingMetal(target.Thing.def)) return false;

            return MetalDetector.GetMetal(target.Thing) > 0f;
        }

        public override bool Activate(LocalTargetInfo targetInfo, LocalTargetInfo dest) {
            target = targetInfo;

            return !target.HasThing && base.Activate(target, dest);
        }
    }
}