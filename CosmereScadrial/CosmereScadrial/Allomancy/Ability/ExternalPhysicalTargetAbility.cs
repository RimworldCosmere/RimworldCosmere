using Cosmere.Core.Util;
using RimWorld;
using Verse;

namespace Cosmere.Scadrial.Allomancy.Ability;

public class ExternalPhysicalTargetAbility(Pawn pawn, AbilityDef def) : AllomancyAbility(pawn, def) {
    protected sealed override bool toggleable => false;

    public override bool CanApplyOn(LocalTargetInfo targetInfo) {
        if (targetInfo.Equals(pawn)) return true;

        if (!base.CanApplyOn(targetInfo) ||
            !targetInfo.HasThing ||
            !MetalDetector.IsCapableOfHavingMetal(targetInfo.Thing.def)) {
            return false;
        }

        return MetalDetector.GetMetal(targetInfo.Thing) > 0f;
    }

    public override bool Activate(LocalTargetInfo targetInfo, LocalTargetInfo dest) {
        localTarget = targetInfo;

        return !localTarget.Value.HasThing && base.Activate(localTarget.Value, dest);
    }
}