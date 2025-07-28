using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Extension;

public static class HediffExtension {
    public static bool IsTooOldToHealFullyWithInvestiture(this Verse.Hediff hediff) {
        return hediff.tickAdded == 0 || hediff.ageTicks >= GenTicks.SecondsToTicks(60 * 60 * 6);
    }

    public static bool CanBeHealedByInvestiture(this Verse.Hediff hediff) {
        if (hediff.TryGetComp(out HediffComp_GetsPermanent permanent)) {
            if (permanent.IsPermanent || permanent.PainCategory > 0) return true;
        }

        if (hediff.TryGetComp(out HediffComp_TendDuration tended) && tended.AllowTend) return true;

        if (hediff.ShouldRemove) return false;

        if (!hediff.def.isBad && !hediff.def.IsAddiction) return false;

        return !hediff.IsTooOldToHealFullyWithInvestiture() && !hediff.IsTended();
    }

    public static bool TryHealWithInvestiture(this Verse.Hediff hediff, float healPower) {
        return hediff.TryHealWithInvestiture(hediff.pawn, healPower);
    }

    public static bool TryHealWithInvestiture(this Verse.Hediff hediff, Pawn doctor, float healPower) {
        Pawn? patient = hediff.pawn;

        bool removedPain = false;
        if (hediff.TryGetComp(out HediffComp_GetsPermanent permanent)) {
            if (permanent.IsPermanent) {
                permanent.IsPermanent = false;
                removedPain = true;
            }

            if (permanent.PainCategory > 0) {
                permanent.SetPainCategory(permanent.PainCategory - 1);
                removedPain = true;
            }
        }

        if (hediff.IsTooOldToHealFullyWithInvestiture()) {
            if (hediff.IsTended()) {
                if (removedPain) hediff.pawn.health.Notify_HediffChanged(hediff);
                return removedPain;
            }

            hediff.Tended(.75f, healPower);
        } else {
            hediff.Heal(Mathf.Lerp(0, 3, healPower));
        }

        hediff.pawn.health.Notify_HediffChanged(hediff);

        if (doctor.Faction == Faction.OfPlayer &&
            patient.Faction != doctor.Faction &&
            patient is { IsPrisoner: false, Faction: not null }) {
            ++patient.mindState.timesGuestTendedToByPlayer;
        }

        patient.records.Increment(RimWorld.RecordDefOf.TimesTendedTo);
        doctor.records.Increment(RimWorld.RecordDefOf.TimesTendedOther);
        if (doctor == patient) doctor.mindState.Notify_SelfTended();
        if (!ModsConfig.IdeologyActive || doctor.Ideo == null) return true;

        Precept_Role role = doctor.Ideo.GetRole(doctor);
        foreach (RoleEffect roleEffect in role?.def.roleEffects ?? []) {
            roleEffect.Notify_Tended(doctor, patient);
        }

        return true;
    }
}