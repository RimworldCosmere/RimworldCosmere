namespace Cosmere.System.Scadrial.Feruchemy;

/// <summary>
///     Charge moved so far this tick, positive into the metalmind and negative out.
/// </summary>
/// <remarks>
///     Draining zeroes it, so a metal that mirrors its charge onto real game state applies
///     each tick's movement exactly once. Deliberately free of RimWorld and Unity types.
/// </remarks>
public sealed class ChargeLedger {
    private float pending;

    public void Stored(float amount) {
        pending += amount;
    }

    public void Tapped(float amount) {
        pending -= amount;
    }

    public float Drain() {
        float moved = pending;
        pending = 0f;

        return moved;
    }
}
