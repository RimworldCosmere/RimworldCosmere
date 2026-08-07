using Cosmere.Core.Comp.Game;
using Cosmere.Core.Def;
using Cosmere.Core.Framework;
using Cosmere.Core.Util;
using Verse;

namespace Cosmere.Core.ShardConnection;

/// <summary>
///     Reading and changing how strongly a pawn is tied to a Shard.
/// </summary>
/// <remarks>
///     Storage is the SpiritWeb edge between the pawn and the Shard, which already persists,
///     already gets seeded at pawn generation and already has hemalurgic theft hooked to it.
///     That edge holds the <em>earned</em> portion only. Ancestry and Investiture are recomputed
///     on every read, so removing a gene or killing a spren drops the total on its own without a
///     save migration.
/// </remarks>
public static class ConnectionUtility {
    /// <summary>What the pawn has earned toward this Shard, ignoring who they were born as.</summary>
    public static int Earned(Pawn? pawn, ShardDef? shard) {
        if (pawn == null || shard == null) return 0;

        Cosmere.Core.Entity.Shard? entity = Entity(shard);
        if (entity == null) return 0;

        return ConnectionMath.FromEdge(SpiritWeb.Instance?.GetConnectionValue(pawn, entity) ?? 0f);
    }

    /// <summary>
    ///     The pawn's full strength toward this Shard, including Harmony's implication.
    /// </summary>
    public static int StrengthOf(Pawn? pawn, ShardDef? shard) {
        if (pawn == null || shard == null) return 0;

        int own = Raw(pawn, shard);

        // Harmony holds Ruin and Preservation both, so anyone Connected to Harmony is Connected
        // to each of them at the same value.
        if (shard.defName is "Ruin" or "Preservation") {
            ShardDef? harmony = DefDatabase<ShardDef>.GetNamedSilentFail("Harmony");
            if (harmony != null) return ConnectionMath.WithHarmony(own, Raw(pawn, harmony));

            return own;
        }

        // And the same fact read the other way, so the two directions cannot disagree. Ancestry
        // grants floors from a world's fallback Shards - Ruin and Preservation on Scadrial - so
        // nothing hands a floor to Harmony, and a post-Catacendre native would otherwise read 0
        // to the Shard their own world is held by.
        if (shard.defName == "Harmony") {
            ShardDef? ruin = DefDatabase<ShardDef>.GetNamedSilentFail("Ruin");
            ShardDef? preservation = DefDatabase<ShardDef>.GetNamedSilentFail("Preservation");
            if (ruin == null || preservation == null) return own;

            return ConnectionMath.WithHarmony(
                own,
                ConnectionMath.HarmonyFrom(Raw(pawn, ruin), Raw(pawn, preservation))
            );
        }

        return own;
    }

    public static ConnectionTier TierOf(Pawn? pawn, ShardDef? shard) {
        return ConnectionMath.TierOf(StrengthOf(pawn, shard));
    }

    /// <summary>
    ///     Whether this pawn may burn a god metal of this Shard, or gain a power from it.
    /// </summary>
    /// <remarks>
    ///     Lerasium is exempt and its caller must not ask - burning it is how an unconnected
    ///     person becomes Connected, so gating it would deny it to exactly the people it exists
    ///     for.
    /// </remarks>
    public static bool MayUse(Pawn? pawn, ShardDef? shard) {
        return ConnectionMath.MayUseGodMetal(StrengthOf(pawn, shard));
    }

    /// <summary>Adds to the earned portion. Never moves the recomputed parts.</summary>
    public static void Grant(Pawn? pawn, ShardDef? shard, int amount) {
        if (pawn == null || shard == null || amount == 0) return;

        Cosmere.Core.Entity.Shard? entity = Entity(shard);
        if (entity == null) return;

        int next = ConnectionMath.Clamp(Earned(pawn, shard) + amount);
        SpiritWeb.Instance?.SetConnection(pawn, entity, ConnectionMath.ToEdge(next));
    }

    /// <summary>
    ///     The live Shard object a connection edge points at. Null when that Shard is not
    ///     enabled in this save, which is the right answer: you cannot be tied to a Shard this
    ///     cosmere does not have.
    /// </summary>
    private static Cosmere.Core.Entity.Shard? Entity(ShardDef shard) {
        Comp.Game.Shards? shards = ShardUtility.shards;
        if (shards == null) return null;

        return shards.enabledShards.TryGetValue(shard.defName, out Cosmere.Core.Entity.Shard? entity)
            ? entity
            : null;
    }

    private static int Raw(Pawn pawn, ShardDef shard) {
        return ConnectionMath.Compose(
            AncestryFloor(pawn, shard),
            0, // Residence is Phase 5. Until then nobody naturalises.
            ConnectionInvestitureRegistry.StrengthFor(pawn, shard),
            Earned(pawn, shard)
        );
    }

    /// <summary>
    ///     Whether this pawn was born to a world this Shard belongs to. The xenotype answers
    ///     first; the save's world is the fallback for a pawn whose xenotype says nothing.
    /// </summary>
    private static int AncestryFloor(Pawn pawn, ShardDef shard) {
        CosmereWorldDef? home = WorldUtility.WorldForXenotype(pawn.genes?.Xenotype)
                                ?? WorldUtility.Primary;
        if (home == null) return 0;

        List<ShardDef> ancestral = WorldUtility.AncestryShards(home);
        for (int i = 0; i < ancestral.Count; i++) {
            if (ancestral[i] == shard) return ConnectionMath.AncestryFloor;
        }

        return 0;
    }
}
