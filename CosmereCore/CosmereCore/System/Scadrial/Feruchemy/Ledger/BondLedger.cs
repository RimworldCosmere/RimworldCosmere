using System;
using Cosmere.Core.Comp.Game;
using Cosmere.Core.Entity;
using Cosmere.Core.ShardConnection;
using Verse;

namespace Cosmere.System.Scadrial.Feruchemy.Ledger;

// Alias must live inside the namespace body - Cosmere.System.Scadrial.Connection is a
// sibling namespace, and it beats a compilation-unit-scoped alias for the bare name.
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

        // AdjustConnection clamps Connection.Value itself, so read each edge before and
        // after and sum the real differences rather than trusting the computed delta.
        float movedEdge = 0f;
        for (int i = 0; i < edges.Count; i++) {
            if (deltas[i] == 0f) continue;

            float before = edges[i].Value;
            web.AdjustConnection(pawn, FarEnd(pawn, edges[i]), deltas[i]);
            movedEdge += edges[i].Value - before;
        }

        int movedPoints = ConnectionMath.FromEdge(Math.Abs(movedEdge));

        return points < 0 ? -movedPoints : movedPoints;
    }

    private static List<Connection> QualifyingEdges(Pawn pawn) {
        List<Connection> edges = [];
        if (pawn == null) return edges;

        SpiritWeb? web = SpiritWeb.Instance;
        if (web == null) return edges;

        // Collected into a list before anything writes back - GetConnections is a lazy
        // yield over the same dictionary AdjustConnection can insert into.
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
