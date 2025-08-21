using Verse;

namespace Cosmere.Roshar.Damage.Worker;

public class SoulDamage : DamageWorker_AddInjury {
    protected override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn) {
        // Randomly select a not-missing and outside body part
        return pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, BodyPartDepth.Outside);
    }

    protected override void ApplySpecialEffectsToPart(
        Pawn pawn,
        float totalDamage,
        DamageInfo dinfo,
        DamageResult result
    ) {
        if (dinfo.HitPart != null && !pawn.health.hediffSet.PartIsMissing(dinfo.HitPart)) {
            Verse.Hediff hediff = HediffMaker.MakeHediff(RimWorld.HediffDefOf.MissingBodyPart, pawn, dinfo.HitPart);
            hediff.Severity =
                dinfo.HitPart.def.GetMaxHealth(pawn) + 1; // Set severity beyond max health to ensure removal
            pawn.health.AddHediff(hediff);

            // Prevent bleeding by marking the part as not bleedable
            if (hediff is Hediff_MissingPart missingPart) {
                missingPart.IsFresh = false; // Prevents it from generating bleeding
            }
        }

        List<BodyPartRecord> adjacentParts = dinfo.HitPart?.GetDirectChildParts()
                                                 .Concat(dinfo.HitPart.parent != null ? [dinfo.HitPart.parent] : [])
                                                 .ToList() ??
                                             [];

        foreach (BodyPartRecord unused in adjacentParts.Where(part => !pawn.health.hediffSet.PartIsMissing(part))) {
            FinalizeAndAddInjury(pawn, totalDamage / 2, dinfo, result);
        }
    }
}