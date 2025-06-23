using RimWorld;
using Verse;

namespace CosmereScadrial.Abilities.Allomancy;

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

    public override bool GizmoDisabled(out string reason) {
        bool hasClip = pawn.inventory?.innerContainer.Contains(CoinThingDefOf.Cosmere_Scadrial_Thing_Clip) ?? false;
        if (!hasClip) {
            reason = "CS_NoClipsToThrow".Translate(pawn.Named("PAWN"));
            return true;
        }

        return base.GizmoDisabled(out reason);
    }

    public override bool CanApplyOn(LocalTargetInfo targetInfo) {
        if (!base.CanApplyOn(targetInfo)) return false;
        SkillRecord? shooting = pawn.skills.GetSkill(SkillDefOf.Shooting);
        if (shooting.TotallyDisabled) return false;

        return pawn.inventory?.innerContainer.Contains(CoinThingDefOf.Cosmere_Scadrial_Thing_Clip) ?? false;
    }

    public override AcceptanceReport CanActivate(LocalTargetInfo targetInfo, BurningStatus activationStatus,
        bool ignoreInvestiture = false) {
        AcceptanceReport baseResult = base.CanActivate(targetInfo, activationStatus, ignoreInvestiture);
        if (!baseResult.Accepted) return baseResult;

        SkillRecord? shooting = pawn.skills.GetSkill(SkillDefOf.Shooting);
        if (!shooting.PermanentlyDisabled && !shooting.TotallyDisabled) return true;

        return "CS_CannotShoot".Translate(pawn.Named("PAWN"));
    }

    public override bool Activate(LocalTargetInfo targetInfo, LocalTargetInfo dest) {
        localTarget = targetInfo;

        return base.Activate(localTarget.Value, dest);
    }
}