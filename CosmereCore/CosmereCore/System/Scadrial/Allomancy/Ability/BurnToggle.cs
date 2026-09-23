using Cosmere.Core.Ability;

namespace Cosmere.System.Scadrial.Allomancy.Ability;

public static class BurnToggle {
    /// <summary>
    ///     Flaring steps down to Burning rather than Off. Dropping straight to Off pulls the
    ///     aura hediff and kills the metal lines mid-flare.
    /// </summary>
    public static Status Next(Status current, bool flare) {
        if (flare) return current.power > 1 ? BurningStatus.Burning : BurningStatus.Flaring;

        return current.power >= 1 ? BurningStatus.Off : BurningStatus.Burning;
    }
}
