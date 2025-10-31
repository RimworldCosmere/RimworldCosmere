using Cosmere;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Allomancy.Ability;

public class CoinshotAbility : AllomancyAbility {
    public CoinshotAbility(Pawn pawn) : base(pawn) { }
    public CoinshotAbility(Pawn pawn, AbilityDef def) : base(pawn, def) { }
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