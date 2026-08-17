using System;
using Cosmere.Core.Def;
using Cosmere.Core.ShardConnection;
using Verse;

namespace Cosmere.System.Scadrial.Feruchemy.Ledger;

/// <summary>
///     Duralumin's tie to one Shard, sitting on top of what <see cref="ConnectionUtility" /> already
///     tracks as earned.
/// </summary>
public class ShardLedger : IConnectionLedger {
    private readonly ShardDef shard;

    public ShardLedger(ShardDef shard) {
        this.shard = shard;
    }

    public DuraluminLedger Ledger => DuraluminLedger.Shard;

    public float CurrentPoints(Pawn pawn) {
        return ConnectionUtility.Earned(pawn, shard);
    }

    public float HeadroomPoints(Pawn pawn) {
        return ConnectionMath.Max - ConnectionUtility.Earned(pawn, shard);
    }

    public float Move(Pawn pawn, float points) {
        if (pawn == null || points == 0f) return 0f;

        // Grant returns void and ConnectionMath clamps, so read Earned before and after instead of trusting the ask.
        int before = ConnectionUtility.Earned(pawn, shard);
        ConnectionUtility.Grant(pawn, shard, (int)Math.Round(points));
        int after = ConnectionUtility.Earned(pawn, shard);

        return after - before;
    }
}
