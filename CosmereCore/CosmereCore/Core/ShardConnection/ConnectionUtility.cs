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
///     <para>
///         <see cref="ConnectionOffsets" /> holds the other half of the story: how much of the
///         composed total is currently somewhere other than the pawn, taken off on every read.
///     </para>
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

        return StrengthFrom(pawn, shard, false);
    }

    /// <summary>
    ///     The same reading, counting what is held elsewhere as though the pawn still carried it.
    /// </summary>
    /// <remarks>
    ///     What a top-up measures against. Setting a tie aside must not make a pawn eligible for a
    ///     grant they already took, which is what measuring against the reduced reading allowed.
    /// </remarks>
    public static int StrengthBeforeOffset(Pawn? pawn, ShardDef? shard) {
        if (pawn == null || shard == null) return 0;

        return StrengthFrom(pawn, shard, true);
    }

    /// <summary>The one Harmony body. Both public readings differ only in which base they compose from.</summary>
    private static int StrengthFrom(Pawn pawn, ShardDef shard, bool ignoreHeld) {
        int own = Carried(pawn, shard, ignoreHeld);

        // Harmony holds Ruin and Preservation both, so a Harmony Connection implies the same to each.
        if (shard.defName is "Ruin" or "Preservation") {
            ShardDef? harmony = DefDatabase<ShardDef>.GetNamedSilentFail("Harmony");
            if (harmony != null) return ConnectionMath.WithHarmony(own, Carried(pawn, harmony, ignoreHeld));

            return own;
        }

        // Inverse of above: nothing hands Harmony a floor directly, so it derives from Ruin and Preservation.
        if (shard.defName == "Harmony") {
            ShardDef? ruin = DefDatabase<ShardDef>.GetNamedSilentFail("Ruin");
            ShardDef? preservation = DefDatabase<ShardDef>.GetNamedSilentFail("Preservation");
            if (ruin == null || preservation == null) return own;

            return ConnectionMath.WithHarmony(
                own,
                ConnectionMath.HarmonyFrom(
                    Carried(pawn, ruin, ignoreHeld),
                    Carried(pawn, preservation, ignoreHeld)
                )
            );
        }

        return own;
    }

    /// <summary>What the pawn carries toward this Shard, or would carry with everything taken back.</summary>
    private static int Carried(Pawn pawn, ShardDef shard, bool ignoreHeld) {
        return ignoreHeld ? Composed(pawn, shard) : Raw(pawn, shard);
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

    /// <summary>
    ///     Whether this pawn may burn a god metal, by the Shards it is made of.
    /// </summary>
    /// <remarks>
    ///     A metal that grants Connection is never gated: lerasium and its alloys are how an
    ///     unconnected person becomes Connected at all, so requiring Connection first would deny
    ///     them to exactly the people they exist for.
    ///     <para>
    ///         Otherwise a tie to any one of its Shards is enough. Leratium is Preservation and
    ///         Ruin both, and someone who holds only one half can still swallow it.
    ///     </para>
    /// </remarks>
    public static bool MayUseMetal(Pawn? pawn, MetalDef? metal) {
        if (metal is not { godMetal: true }) return true;
        if (metal.shards.Count == 0) return true;

        for (int i = 0; i < metal.shards.Count; i++) {
            if (metal.shards[i].grant > 0) return true;
            if (MayUse(pawn, metal.shards[i].shard)) return true;
        }

        return false;
    }

    /// <summary>The first Shard this pawn is not Connected enough to, for the refusal message.</summary>
    public static ShardDef? FirstUnreachedShard(Pawn? pawn, MetalDef? metal) {
        if (metal == null) return null;

        for (int i = 0; i < metal.shards.Count; i++) {
            if (!MayUse(pawn, metal.shards[i].shard)) return metal.shards[i].shard;
        }

        return null;
    }

    /// <summary>
    ///     Burning a metal ties the drinker to each Shard in it, as deeply as that metal reaches.
    /// </summary>
    /// <remarks>
    ///     Call this <em>after</em> the metal's powers are granted, not before. The grant tops the
    ///     pawn up to a total, and a Mistborn gene is worth Investiture on its own - topping up
    ///     first and adding the gene second put a lerasium drinker at 100 rather than 80.
    ///     <para>
    ///         Never lowers anyone. A pawn already deeper than the metal reaches keeps what they
    ///         had.
    ///     </para>
    /// </remarks>
    public static void GrantFromMetal(Pawn? pawn, MetalDef? metal) {
        if (pawn == null || metal == null) return;

        for (int i = 0; i < metal.shards.Count; i++) {
            ShardGrant entry = metal.shards[i];
            if (entry.grant <= 0) continue;

            int shortfall = entry.grant - StrengthBeforeOffset(pawn, entry.shard);
            if (shortfall > 0) Grant(pawn, entry.shard, shortfall);
        }
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
    ///     Moves how much of this pawn's tie is held elsewhere, and reports how much actually moved.
    /// </summary>
    /// <remarks>
    ///     Bounded below by nothing held and above by <see cref="ConnectionMath.OffsetCeiling" />, so
    ///     the last point of a tie can never leave and a stripped pawn can always take theirs back.
    /// </remarks>
    public static float AdjustOffset(Pawn? pawn, ShardDef? shard, float delta) {
        if (pawn == null || shard == null || delta == 0f) return 0f;

        float had = ConnectionOffsets.Get(pawn, shard);
        float ceiling = ConnectionMath.OffsetCeiling(Composed(pawn, shard));
        ConnectionOffsets.Set(pawn, shard, ConnectionMath.ClampOffset(had, delta, ceiling));

        // Read back rather than trust the ask: outside a running game the store swallows the write.
        return ConnectionOffsets.Get(pawn, shard) - had;
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
        int held = (int)global::System.Math.Round(ConnectionOffsets.Get(pawn, shard));

        // Whatever is held elsewhere is not part of what this pawn currently carries.
        return ConnectionMath.Clamp(Composed(pawn, shard) - held);
    }

    /// <summary>The four parts of the tie, before anything held elsewhere comes off the total.</summary>
    private static int Composed(Pawn pawn, ShardDef shard) {
        return ConnectionMath.Compose(
            AncestryFloor(pawn, shard),
            GameComponentCache<ResidenceTracker>.Get()?.StrengthFor(pawn, shard) ?? 0,
            ConnectionInvestitureRegistry.StrengthFor(pawn, shard),
            Earned(pawn, shard)
        );
    }

    /// <summary>
    ///     Whether this pawn was born to a world this Shard belongs to.
    /// </summary>
    /// <remarks>
    ///     The xenotype is the only thing that grants ancestry, on any save. Two earlier rules
    ///     both handed a floor to people who had not earned one: falling back to the save's world
    ///     made every baseliner stepping out of a drop pod a native, and the cross-world sentinel
    ///     handed 30 to all of them at once, so a Crashlanded colony of baseliners burned atium on
    ///     day one.
    ///     <para>
    ///         An off-worlder starts at nothing wherever they are, and earns it by living there.
    ///         Picking a world at world generation says where the colony is, not who the colonists
    ///         are.
    ///     </para>
    /// </remarks>
    private static int AncestryFloor(Pawn pawn, ShardDef shard) {
        CosmereWorldDef? home = WorldUtility.WorldForXenotype(pawn.genes?.Xenotype);
        if (home == null) return 0;

        List<ShardDef> ancestral = WorldUtility.AncestryShards(home);
        for (int i = 0; i < ancestral.Count; i++) {
            if (ancestral[i] == shard) return ConnectionMath.AncestryFloor;
        }

        return 0;
    }
}
