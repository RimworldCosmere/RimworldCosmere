using CosmereScadrial.Utils;
using RimWorld;
using Verse;

namespace CosmereScadrial.Abilities.Allomancy;

public class ExternalPhysicalTargetAbility : AbilityOtherTarget {
    public ExternalPhysicalTargetAbility() { }

    public ExternalPhysicalTargetAbility(Pawn pawn) : base(pawn) { }

    public ExternalPhysicalTargetAbility(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }

    public ExternalPhysicalTargetAbility(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    public ExternalPhysicalTargetAbility(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn,
        sourcePrecept, def) { }

    protected sealed override bool toggleable => false;

    public override bool CanApplyOn(LocalTargetInfo targetInfo) {
        if (!base.CanApplyOn(targetInfo) || !targetInfo.HasThing ||
            !MetalDetector.IsCapableOfHavingMetal(targetInfo.Thing.def)) {
            return false;
        }

        return MetalDetector.GetMetal(targetInfo.Thing) > 0f;
    }

    public override bool Activate(LocalTargetInfo targetInfo, LocalTargetInfo dest) {
        target = targetInfo;

        return !target.HasThing && base.Activate(target, dest);
    }
}