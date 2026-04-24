using Verse;

namespace Cosmere.System.Roshar.Damage.Worker;

public class SoulDamage : DamageWorker_AddInjury {
    protected override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn) {
        return pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, BodyPartDepth.Outside);
    }

    protected override void ApplySpecialEffectsToPart(
        Pawn pawn,
        float totalDamage,
        DamageInfo dinfo,
        DamageResult result
    ) {
        if (dinfo.HitPart == null || pawn.health.hediffSet.PartIsMissing(dinfo.HitPart)) {
            return;
        }

        Verse.Hediff hediff = HediffMaker.MakeHediff(RimWorld.HediffDefOf.MissingBodyPart, pawn, dinfo.HitPart);
        hediff.Severity = dinfo.HitPart.def.GetMaxHealth(pawn) + 1;
        pawn.health.AddHediff(hediff);

        if (hediff is Hediff_MissingPart missingPart) {
            missingPart.IsFresh = false;
        }

        result.AddPart(pawn, dinfo.HitPart);
        result.wounded = true;
        result.totalDamageDealt += dinfo.HitPart.def.GetMaxHealth(pawn);
    }
}