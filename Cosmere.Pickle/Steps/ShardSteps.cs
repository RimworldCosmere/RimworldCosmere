using System;
using Cosmere.Core.Comp.Game;
using Cosmere.Core.Def;
using Cosmere.Core.ShardConnection;
using Cosmere.Core.Util;
using Cosmere.Pickle.Lookup;
using RimWorks.Pickle;
using Verse;

namespace Cosmere.Pickle.Steps;

/// <summary>Which Shards hold this cosmere, and how strongly a pawn is tied to one. Almost
/// every Cosmere feature reads both before it does anything, so these run first.</summary>
[PickleSteps]
public class ShardSteps {
    /// <summary>How far a reading may sit from the asserted strength and still count.</summary>
    // The earned portion round-trips through a 0..1 edge, so a point of rounding is normal.
    private const int DefaultTolerance = 1;

    /// <summary>Turns a Shard on, the way picking a world does.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="shardName">The Shard def to enable.</param>
    [Given("I enable the shard {string}")]
    public void EnableShard(PickleContext ctx, string shardName) {
        Shards shards = CosmereLookup.RequireShards(ctx);
        ShardDef shard = CosmereLookup.RequireDef<ShardDef>(shardName);

        // Conflicts respected, the way picking a world does it. Overlap is the ascension beat.
        ShardUtility.Enable(shard);

        CosmereLookup.AssertThat(
            ctx,
            shards.IsEnabled(shard),
            $"the shard '{shardName}' should be enabled",
            () => CosmereLookup.DescribeShards(shards));
    }

    /// <summary>Asserts a Shard holds this cosmere.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="shardName">The Shard def expected to be on.</param>
    [Then("the shard {string} is enabled")]
    public void AssertShardEnabled(PickleContext ctx, string shardName) {
        Shards shards = CosmereLookup.RequireShards(ctx);
        ShardDef shard = CosmereLookup.RequireDef<ShardDef>(shardName);

        CosmereLookup.AssertThat(
            ctx,
            shards.IsEnabled(shard),
            $"the shard '{shardName}' should be enabled",
            () => CosmereLookup.DescribeShards(shards));
    }

    /// <summary>Asserts a Shard does not hold this cosmere.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="shardName">The Shard def expected to be off.</param>
    [Then("the shard {string} is disabled")]
    public void AssertShardDisabled(PickleContext ctx, string shardName) {
        Shards shards = CosmereLookup.RequireShards(ctx);
        ShardDef shard = CosmereLookup.RequireDef<ShardDef>(shardName);

        CosmereLookup.AssertThat(
            ctx,
            !shards.IsEnabled(shard),
            $"the shard '{shardName}' should be disabled",
            () => CosmereLookup.DescribeShards(shards));
    }

    /// <summary>Ties a pawn to a Shard, topping them up to a strength.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn to tie.</param>
    /// <param name="shardName">The Shard to tie them to.</param>
    /// <param name="strength">The strength to reach, on the 0 to 100 scale.</param>
    // Never lowers anyone, the same rule GrantFromMetal follows.
    [Given("{string} has a connection to the shard {string} of {int}")]
    public void GrantConnection(PickleContext ctx, string nickname, string shardName, int strength) {
        Shards shards = CosmereLookup.RequireShards(ctx);
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        ShardDef shard = CosmereLookup.RequireDef<ShardDef>(shardName);

        ctx.Require(
            strength >= 0 && strength <= ConnectionMath.Max,
            $"a connection runs 0 to {ConnectionMath.Max}; this step asked for {strength}");

        // A disabled Shard has no entity to hang the edge on, so the grant is swallowed whole.
        ctx.Require(
            shards.IsEnabled(shard),
            $"the shard '{shardName}' is off, so a connection to it cannot be granted. {CosmereLookup.DescribeShards(shards)}");

        int shortfall = strength - ConnectionUtility.StrengthOf(pawn, shard);
        if (shortfall > 0) {
            ConnectionUtility.Grant(pawn, shard, shortfall);
        }

        CosmereLookup.AssertThat(
            ctx,
            ConnectionUtility.StrengthOf(pawn, shard) >= strength,
            $"'{nickname}' should reach {strength} toward '{shardName}'",
            () => CosmereLookup.DescribeConnection(pawn, shard));
    }

    /// <summary>Asserts a pawn's strength toward a Shard, within a point of rounding.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn to read.</param>
    /// <param name="shardName">The Shard to read against.</param>
    /// <param name="strength">The strength expected.</param>
    [Then("{string} connection to the shard {string} is {int}")]
    public void AssertConnection(PickleContext ctx, string nickname, string shardName, int strength) {
        AssertConnectionWithin(ctx, nickname, shardName, strength, DefaultTolerance);
    }

    /// <summary>Asserts a pawn's strength toward a Shard, within a stated tolerance.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn to read.</param>
    /// <param name="shardName">The Shard to read against.</param>
    /// <param name="strength">The strength expected.</param>
    /// <param name="tolerance">How far the reading may sit from it.</param>
    [Then("{string} connection to the shard {string} is {int} within {int}")]
    public void AssertConnectionWithin(
        PickleContext ctx,
        string nickname,
        string shardName,
        int strength,
        int tolerance) {
        CosmereLookup.RequireShards(ctx);
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        ShardDef shard = CosmereLookup.RequireDef<ShardDef>(shardName);
        int actual = ConnectionUtility.StrengthOf(pawn, shard);

        CosmereLookup.AssertThat(
            ctx,
            Math.Abs(actual - strength) <= tolerance,
            $"'{nickname}' connection to '{shardName}' should be {strength} give or take {tolerance}",
            () => CosmereLookup.DescribeConnection(pawn, shard));
    }

    /// <summary>Asserts which band a pawn's tie to a Shard falls in.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn to read.</param>
    /// <param name="shardName">The Shard to read against.</param>
    /// <param name="tierName">The tier expected: None, Touched, Bonded, Invested or Ascendant.</param>
    [Then("{string} connection tier to the shard {string} is {word}")]
    public void AssertConnectionTier(PickleContext ctx, string nickname, string shardName, string tierName) {
        CosmereLookup.RequireShards(ctx);
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        ShardDef shard = CosmereLookup.RequireDef<ShardDef>(shardName);
        ConnectionTier wanted = RequireTier(tierName);

        CosmereLookup.AssertThat(
            ctx,
            ConnectionUtility.TierOf(pawn, shard) == wanted,
            $"'{nickname}' tier toward '{shardName}' should be {wanted}",
            () => CosmereLookup.DescribeConnection(pawn, shard));
    }

    private static ConnectionTier RequireTier(string tierName) {
        if (Enum.TryParse(tierName, ignoreCase: true, out ConnectionTier tier)) {
            return tier;
        }

        throw new InvalidOperationException(
            $"'{tierName}' is not a connection tier. try one of: {string.Join(", ", Enum.GetNames(typeof(ConnectionTier)))}");
    }
}
