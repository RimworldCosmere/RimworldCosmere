using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Comp.Hediff;

public class StormfathersRegardProperties : HediffCompProperties {
    public StormfathersRegardProperties() {
        compClass = typeof(StormfathersRegard);
    }
}

/// <summary>
///     While the Stormfather is paying attention the storm mends rather than tears. Closes one
///     bleeding wound, or tends one untended injury, per in-game hour.
/// </summary>
public class StormfathersRegard : HediffComp {
    public override void CompPostTickInterval(ref float severityAdjustment, int delta) {
        base.CompPostTickInterval(ref severityAdjustment, delta);
        if (!GenTicks.IsTickIntervalDelta(GenDate.TicksPerHour, delta)) return;

        MendOneWound();
    }

    private void MendOneWound() {
        List<Verse.Hediff>? hediffs = Pawn?.health?.hediffSet?.hediffs;
        if (hediffs == null) return;

        for (int i = 0; i < hediffs.Count; i++) {
            if (hediffs[i] is not Hediff_Injury injury) continue;

            if (injury.Bleeding) {
                injury.Heal(injury.Severity);
                return;
            }

            if (injury.TendableNow(true)) {
                injury.Tended(1f, 1f, 0);
                return;
            }
        }
    }
}
