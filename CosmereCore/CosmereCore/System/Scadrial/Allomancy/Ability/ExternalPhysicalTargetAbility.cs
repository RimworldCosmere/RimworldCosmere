using Cosmere.Core.Util;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Allomancy.Ability;

public class ExternalPhysicalTargetAbility : AllomancyAbility {
    public ExternalPhysicalTargetAbility(Pawn pawn) : base(pawn) { }
    public ExternalPhysicalTargetAbility(Pawn pawn, AbilityDef def) : base(pawn, def) { }
    protected sealed override bool toggleable => false;

    public override bool CanApplyOn(LocalTargetInfo target) {
        if (target.Equals(pawn)) return true;

        if (!base.CanApplyOn(target) ||
            !target.HasThing ||
            !MetalDetector.IsCapableOfHavingMetal(target.Thing.def)) {
            return false;
        }

        return MetalDetector.GetMetal(target.Thing) > 0f;
    }

    public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest) {
        localTarget = target;

        return base.Activate(localTarget.Value, dest);
    }
}