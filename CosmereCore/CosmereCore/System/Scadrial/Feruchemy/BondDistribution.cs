using System;
using Cosmere.Core.ShardConnection;

namespace Cosmere.System.Scadrial.Feruchemy;

/// <summary>
///     How one duralumin transfer spreads across a pawn's non-Shard SpiritWeb edges.
/// </summary>
/// <remarks>
///     Verse-free so the test host can load it. Runs the split in edge-value floats end to end and
///     converts to points only once, at the boundary - rounding every edge separately would leak
///     charge on every transfer.
/// </remarks>
public static class BondDistribution {
    /// <summary>Drains points proportional to each edge's value. Nothing goes below 0.</summary>
    public static (float moved, float[] deltas) Drain(float[] edgeValues, float points) {
        float[] deltas = new float[edgeValues.Length];
        if (points <= 0f || edgeValues.Length == 0) return (0f, deltas);

        float askEdge = ConnectionMath.ToEdge((int)Math.Round(points));
        float pool = 0f;
        for (int i = 0; i < edgeValues.Length; i++) pool += edgeValues[i];
        if (askEdge <= 0f || pool <= 0f) return (0f, deltas);

        float takeEdge = Math.Min(askEdge, pool);
        for (int i = 0; i < edgeValues.Length; i++) deltas[i] = -(edgeValues[i] / pool * takeEdge);

        return (ConnectionMath.FromEdge(takeEdge), deltas);
    }

    /// <summary>Restores points proportional to each edge's headroom. Nothing goes above 1.</summary>
    public static (float moved, float[] deltas) Restore(float[] edgeValues, float points) {
        float[] deltas = new float[edgeValues.Length];
        if (points <= 0f || edgeValues.Length == 0) return (0f, deltas);

        float askEdge = ConnectionMath.ToEdge((int)Math.Round(points));
        float headroom = 0f;
        for (int i = 0; i < edgeValues.Length; i++) headroom += 1f - edgeValues[i];
        if (askEdge <= 0f || headroom <= 0f) return (0f, deltas);

        float giveEdge = Math.Min(askEdge, headroom);
        for (int i = 0; i < edgeValues.Length; i++) deltas[i] = (1f - edgeValues[i]) / headroom * giveEdge;

        return (ConnectionMath.FromEdge(giveEdge), deltas);
    }
}
