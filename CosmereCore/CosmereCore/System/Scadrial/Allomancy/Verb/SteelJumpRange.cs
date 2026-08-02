using System;

namespace Cosmere.System.Scadrial.Allomancy.Verb;

public static class SteelJumpRange {
    private const float BaselineMass = 60f;
    private const float MinMassFactor = 0.5f;

    // rawPower is a modifier around 1, not a factor: it floors at 0.1, so multiplying the
    // def's range by it directly left a fresh Misting jumping two tiles out of a stated twelve.
    public static float For(float baseRange, int power, float rawPower, float mass) {
        float massFactor = Math.Max(mass / BaselineMass, MinMassFactor);

        return baseRange * power * (0.5f + rawPower / 2f) / massFactor;
    }
}
