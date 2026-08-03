namespace Cosmere.Core.Investiture;

// Deliberately free of RimWorld and Unity types so the test host can load it.
public static class UpkeepRate {
    public const int TicksPerSecond = 60;
    public const int TicksPerRareInterval = 250;

    public static int TicksFor(UpkeepCadence cadence) {
        return cadence switch {
            UpkeepCadence.PerTick => 1,
            UpkeepCadence.PerRareTick => TicksPerRareInterval,
            _ => TicksPerSecond,
        };
    }

    public static float PerSecond(float ratePerCharge, int ticksPerCharge) {
        if (ticksPerCharge <= 0) return 0f;

        return ratePerCharge * TicksPerSecond / ticksPerCharge;
    }

    // Allomancy spends thousandths of a breath-equivalent unit per charge, which rounds away
    // entirely at the two decimals the strip has room for. A share of the reserve stays legible
    // at that scale and matches the percentage already on the tile.
    public static float ReservePercentPerSecond(
        float ratePerCharge,
        int ticksPerCharge,
        float breathEquivalentUnitsPerReserveUnit,
        float maxReserve
    ) {
        if (breathEquivalentUnitsPerReserveUnit <= 0f || maxReserve <= 0f) return 0f;

        float reservePerSecond = PerSecond(ratePerCharge, ticksPerCharge) / breathEquivalentUnitsPerReserveUnit;

        return reservePerSecond / maxReserve * 100f;
    }
}
