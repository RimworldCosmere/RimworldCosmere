using System;
using Cosmere.Core.Comp.Game;
using Cosmere.Core.Entity;
using Cosmere.Core.ShardConnection;
using Verse;

namespace Cosmere.System.Scadrial.Feruchemy.Ledger;

// Must sit here, below the namespace - a sibling Cosmere.System.Scadrial.Connection wins over an outer-scoped alias.
using Connection = Cosmere.Core.Comp.Game.Connection;

/// <summary>
///     Duralumin's tie to a pawn's non-Shard SpiritWeb edges - Nahel bonds among them, reached only
///     through <see cref="SpiritWeb" /> so this file never names a Roshar type.
/// </summary>
public class BondLedger : IConnectionLedger {
    public DuraluminLedger Ledger => DuraluminLedger.Bonds;

    public float CurrentPoints(Pawn pawn) {
        List<Connection> edges = QualifyingEdges(pawn);
        float points = 0f;
        for (int i = 0; i < edges.Count; i++) points += ConnectionMath.FromEdge(edges[i].Value);

        return points;
    }

    public float HeadroomPoints(Pawn pawn) {
        List<Connection> edges = QualifyingEdges(pawn);
        float headroom = 0f;
        for (int i = 0; i < edges.Count; i++) headroom += ConnectionMath.Max - ConnectionMath.FromEdge(edges[i].Value);

        return headroom;
    }

    public float Move(Pawn pawn, float points) {
        if (pawn == null || points == 0f) return 0f;

        SpiritWeb? web = SpiritWeb.Instance;
        if (web == null) return 0f;

        List<Connection> edges = QualifyingEdges(pawn);
        if (edges.Count == 0) return 0f;

        float[] values = new float[edges.Count];
        for (int i = 0; i < edges.Count; i++) values[i] = edges[i].Value;

        (_, float[] deltas) = points < 0
            ? BondDistribution.Drain(values, -points)
            : BondDistribution.Restore(values, points);

        // AdjustConnection clamps Connection.Value, so sum real before/after differences, not the ask.
        float movedEdge = 0f;
        for (int i = 0; i < edges.Count; i++) {
            if (deltas[i] == 0f) continue;

            float before = edges[i].Value;
            web.AdjustConnection(pawn, FarEnd(pawn, edges[i]), deltas[i]);
            movedEdge += edges[i].Value - before;
        }

        // N edges, not one - scale by Max directly; FromEdge clamps to a single edge's 0-100 range.
        int movedPoints = (int)Math.Round(Math.Abs(movedEdge) * ConnectionMath.Max);

        return points < 0 ? -movedPoints : movedPoints;
    }

    private static List<Connection> QualifyingEdges(Pawn pawn) {
        List<Connection> edges = [];
        if (pawn == null) return edges;

        SpiritWeb? web = SpiritWeb.Instance;
        if (web == null) return edges;

        // Materialized before any write - GetConnections yields lazily over the dictionary AdjustConnection inserts into.
        foreach (Connection connection in web.GetConnections(pawn)) {
            if (FarEnd(pawn, connection) is Shard) continue;
            edges.Add(connection);
        }

        return edges;
    }

    private static ILoadReferenceable FarEnd(Pawn pawn, Connection connection) {
        return connection.ObjectOne.GetUniqueLoadID() == pawn.GetUniqueLoadID()
            ? connection.ObjectTwo
            : connection.ObjectOne;
    }
}
