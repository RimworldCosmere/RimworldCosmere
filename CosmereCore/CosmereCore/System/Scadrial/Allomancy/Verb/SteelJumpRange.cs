using System;

namespace Cosmere.System.Scadrial.Allomancy.Verb;

public static class SteelJumpRange {
    private const float BaselineMass = 60f;
    private const float MinMassFactor = 0.5f;

    // The targeter asks for the range before anything has lit the metal, so an idle ability
    // reports power 0 and sizes the jump to nothing. QueueCastingJob defaults to 1; match it.
    public static int PowerFor(int? nextPower, int currentPower) {
        return nextPower ?? Math.Max(currentPower, 1);
    }

    // rawPower is a modifier around 1, not a factor: it floors at 0.1, so multiplying the
    // def's range by it directly left a fresh Misting jumping two tiles out of a stated twelve.
    public static float For(float baseRange, int power, float rawPower, float mass) {
        float massFactor = Math.Max(mass / BaselineMass, MinMassFactor);

        return baseRange * power * (0.5f + rawPower / 2f) / massFactor;
    }
}
