using RimWorld;
using Verse;

namespace CosmereScadrial.Allomancy.Ability;

public class CoinshotAbility : AbilityOtherTarget {
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

    public override bool GizmoDisabled(out string reason) {
        bool hasClip = pawn.inventory?.innerContainer.Contains(ThingDefOf.Cosmere_Scadrial_Thing_Clip) ?? false;
        if (hasClip) return base.GizmoDisabled(out reason);

        reason = "CS_NoClipsToThrow".Translate(pawn.Named("PAWN"));
        return true;
    }

    public override bool CanApplyOn(LocalTargetInfo targetInfo) {
        if (!base.CanApplyOn(targetInfo)) return false;
        SkillRecord? shooting = pawn.skills.GetSkill(RimWorld.SkillDefOf.Shooting);
        if (shooting.TotallyDisabled) return false;

        return pawn.inventory?.innerContainer.Contains(ThingDefOf.Cosmere_Scadrial_Thing_Clip) ?? false;
    }

    public override bool Activate(LocalTargetInfo targetInfo, LocalTargetInfo dest) {
        localTarget = targetInfo;

        return base.Activate(localTarget.Value, dest);
    }
}