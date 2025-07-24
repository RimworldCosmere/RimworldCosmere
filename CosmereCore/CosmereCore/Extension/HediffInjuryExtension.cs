using Verse;

namespace Cosmere.Core.Extension;

public static class HediffInjuryExtension {
    public static bool CanBeHealedWithInvestiture(this Hediff_Injury injury) {
        if (injury.tickAdded != 0 && injury.ageTicks < GenTicks.SecondsToTicks(60 * 60 * 6)) {
            return true;
        }

        return (injury.CanHealNaturally() || injury.CanHealFromTending()) && !injury.IsPermanent();
    }
}