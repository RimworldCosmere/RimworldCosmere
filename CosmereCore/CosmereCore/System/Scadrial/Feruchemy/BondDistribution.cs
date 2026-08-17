using System;
using Cosmere.Core.ShardConnection;

namespace Cosmere.System.Scadrial.Feruchemy;

/// <summary>
///     How one duralumin transfer spreads across a pawn's non-Shard SpiritWeb edges. Verse-free;
///     converts the aggregate ask/result once via <see cref="ConnectionMath.Max" />, unclamped.
/// </summary>
public static class BondDistribution {
    /// <summary>Drains points proportional to each edge's value. Nothing goes below 0.</summary>
    public static (float moved, float[] deltas) Drain(float[] edgeValues, float points) {
        float[] deltas = new float[edgeValues.Length];
        if (points <= 0f || edgeValues.Length == 0) return (0f, deltas);

        float askEdge = points / ConnectionMath.Max;
        float pool = 0f;
        for (int i = 0; i < edgeValues.Length; i++) pool += edgeValues[i];
        if (pool <= 0f) return (0f, deltas);

        float takeEdge = Math.Min(askEdge, pool);
        for (int i = 0; i < edgeValues.Length; i++) deltas[i] = -(edgeValues[i] / pool * takeEdge);

        return (takeEdge * ConnectionMath.Max, deltas);
    }

    /// <summary>Restores points proportional to each edge's headroom. Nothing goes above 1.</summary>
    public static (float moved, float[] deltas) Restore(float[] edgeValues, float points) {
        float[] deltas = new float[edgeValues.Length];
        if (points <= 0f || edgeValues.Length == 0) return (0f, deltas);

        float askEdge = points / ConnectionMath.Max;
        float headroom = 0f;
        for (int i = 0; i < edgeValues.Length; i++) headroom += 1f - edgeValues[i];
        if (headroom <= 0f) return (0f, deltas);

        float giveEdge = Math.Min(askEdge, headroom);
        for (int i = 0; i < edgeValues.Length; i++) deltas[i] = (1f - edgeValues[i]) / headroom * giveEdge;

        return (giveEdge * ConnectionMath.Max, deltas);
    }
}
