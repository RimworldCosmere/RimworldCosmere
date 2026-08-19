using Cosmere.Core.Def;
using Cosmere.Core.ShardConnection;
using Verse;

namespace Cosmere.System.Scadrial.Feruchemy.Ledger;

/// <summary>
///     Duralumin's tie to one Shard, moved through <see cref="ConnectionUtility" />'s held offset.
/// </summary>
public class ShardLedger : IConnectionLedger {
    private readonly ShardDef shard;

    public ShardLedger(ShardDef shard) {
        this.shard = shard;
    }

    public DuraluminLedger Ledger => DuraluminLedger.Shard;

    public float CurrentPoints(Pawn pawn) {
        return ConnectionMath.OffsetCeiling(ConnectionUtility.StrengthOf(pawn, shard));
    }

    public float HeadroomPoints(Pawn pawn) {
        return ConnectionOffsets.Get(pawn, shard);
    }

    public float Move(Pawn pawn, float points) {
        if (pawn == null || points == 0f) return 0f;

        return -ConnectionUtility.AdjustOffset(pawn, shard, -points);
    }
}
