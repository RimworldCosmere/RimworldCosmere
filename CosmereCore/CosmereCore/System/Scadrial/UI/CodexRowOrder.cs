namespace Cosmere.System.Scadrial.UI;

/// <summary>
///     Row order for the Codex metal tables.
/// </summary>
/// <remarks>
///     Sixteen metals in def order made "which am I savant in" a question you had to read the
///     whole table to answer. Savant stage outranks time worked: a pawn who reached Savant on one
///     metal wants that line first even when another metal has more hours on it.
/// </remarks>
public static class CodexRowOrder {
    /// <summary>Descending savant stage, then descending ticks worked.</summary>
    public static int Compare(int stageA, int ticksA, int stageB, int ticksB) {
        if (stageA != stageB) return stageB.CompareTo(stageA);

        return ticksB.CompareTo(ticksA);
    }
}
