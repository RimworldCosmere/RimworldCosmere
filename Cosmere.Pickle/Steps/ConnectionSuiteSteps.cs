using System;
using System.Threading.Tasks;
using Cosmere.Core.Def;
using Cosmere.Core.Investiture;
using Cosmere.Core.ShardConnection;
using Cosmere.Core.Util;
using Cosmere.Pickle.Lookup;
using RimWorks.Pickle;
using RimWorks.Pickle.Runtime;
using RimWorld;
using Verse;
using InvestitureHolder = Cosmere.Core.Comp.Thing.InvestitureHolder;

namespace Cosmere.Pickle.Steps;

/// <summary>What the shared steps do not reach: the parts a Connection total is built from,
/// and how a holder moves Investiture between things.</summary>
[PickleSteps]
public class ConnectionSuiteSteps {
    private const int TicksPerYield = 60;

    private const float WaitTimeoutSeconds = 120f;

    /// <summary>Asserts a pawn's tie to a Shard clears a floor, naming no figure a tunable
    /// decides.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn to read.</param>
    /// <param name="shardName">The Shard to read against.</param>
    /// <param name="bound">The lower bound, exclusive.</param>
    [Then("{string} connection to the shard {string} is above {int}")]
    public void AssertConnectionAbove(PickleContext ctx, string nickname, string shardName, int bound) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        ShardDef shard = CosmereLookup.RequireDef<ShardDef>(shardName);

        CosmereLookup.AssertThat(
            ctx,
            ConnectionUtility.StrengthOf(pawn, shard) > bound,
            $"'{nickname}' connection to '{shardName}' should be above {bound}",
            () => CosmereLookup.DescribeConnection(pawn, shard));
    }

    /// <summary>Asserts a pawn's tie to a Shard stays under a ceiling.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn to read.</param>
    /// <param name="shardName">The Shard to read against.</param>
    /// <param name="bound">The upper bound, exclusive.</param>
    [Then("{string} connection to the shard {string} is below {int}")]
    public void AssertConnectionBelow(PickleContext ctx, string nickname, string shardName, int bound) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        ShardDef shard = CosmereLookup.RequireDef<ShardDef>(shardName);

        CosmereLookup.AssertThat(
            ctx,
            ConnectionUtility.StrengthOf(pawn, shard) < bound,
            $"'{nickname}' connection to '{shardName}' should be below {bound}",
            () => CosmereLookup.DescribeConnection(pawn, shard));
    }

    /// <summary>Remembers a tie so a later step can say which way it moved.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn to read.</param>
    /// <param name="shardName">The Shard to read against.</param>
    /// <remarks>A generated colonist rolls a shardworld xenotype, so no scenario may assume a
    /// tie starts at nothing. Recording the baseline is how a direction is asserted instead.</remarks>
    [When("I record {string} connection to the shard {string}")]
    public void RecordConnection(PickleContext ctx, string nickname, string shardName) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        ShardDef shard = CosmereLookup.RequireDef<ShardDef>(shardName);

        Readings(ctx).Taken[(pawn.thingIDNumber, shard.defName)] = ConnectionUtility.StrengthOf(pawn, shard);
    }

    /// <summary>Asserts a tie has grown since it was recorded.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn to read.</param>
    /// <param name="shardName">The Shard to read against.</param>
    [Then("{string} connection to the shard {string} has risen")]
    public void AssertConnectionRose(PickleContext ctx, string nickname, string shardName) {
        AssertMoved(ctx, nickname, shardName, (before, now) => now > before, "should have risen");
    }

    /// <summary>Asserts a tie has shrunk since it was recorded.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn to read.</param>
    /// <param name="shardName">The Shard to read against.</param>
    [Then("{string} connection to the shard {string} has fallen")]
    public void AssertConnectionFell(PickleContext ctx, string nickname, string shardName) {
        AssertMoved(ctx, nickname, shardName, (before, now) => now < before, "should have fallen");
    }

    /// <summary>Asserts a tie is exactly where it was, which is how a change proves it stayed
    /// in its own lane.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn to read.</param>
    /// <param name="shardName">The Shard to read against.</param>
    [Then("{string} connection to the shard {string} has not moved")]
    public void AssertConnectionUnmoved(PickleContext ctx, string nickname, string shardName) {
        AssertMoved(ctx, nickname, shardName, (before, now) => now == before, "should not have moved");
    }

    /// <summary>Asserts the parts the player is shown rebuild the strength the game acts on.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn to read.</param>
    /// <param name="shardName">The Shard to read against.</param>
    /// <remarks>Never reads Harmony, which the breakdown defines as leftover and so absorbs any
    /// gap. The sibling Shards say what that floor should be, so a missed source shows up here.</remarks>
    [Then("{string} connection breakdown to the shard {string} adds up")]
    public void AssertBreakdownAddsUp(PickleContext ctx, string nickname, string shardName) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        ShardDef shard = CosmereLookup.RequireDef<ShardDef>(shardName);
        ConnectionBreakdown parts = ConnectionUtility.BreakdownFor(pawn, shard);

        int carried = CarriedFrom(parts);
        int implied = ImpliedHarmony(pawn, shard);
        int rebuilt = ConnectionMath.WithHarmony(carried, implied);

        CosmereLookup.AssertThat(
            ctx,
            rebuilt == parts.Total,
            $"'{nickname}' breakdown toward '{shardName}' should rebuild the strength the game acts on",
            () => $"the shown parts carry {carried} and the sibling Shards imply a Harmony floor of " +
                $"{implied}, rebuilding to {rebuilt}, but the strength reads {parts.Total}, a gap of " +
                $"{parts.Total - rebuilt}. {CosmereLookup.DescribeConnection(pawn, shard)}");
    }

    /// <summary>Asserts a pawn is tied deeply enough to burn a god metal.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn to read.</param>
    /// <param name="metalName">The metal def to ask about.</param>
    [Then("{string} may burn the god metal {string}")]
    public void AssertMayBurn(PickleContext ctx, string nickname, string metalName) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        MetalDef metal = RequireGodMetal(ctx, metalName);

        CosmereLookup.AssertThat(
            ctx,
            ConnectionUtility.MayUseMetal(pawn, metal),
            $"'{nickname}' should be able to burn '{metalName}'",
            () => DescribeMetal(pawn, metal));
    }

    /// <summary>Asserts a pawn is too far from a god metal's Shards to burn it.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn to read.</param>
    /// <param name="metalName">The metal def to ask about.</param>
    [Then("{string} may not burn the god metal {string}")]
    public void AssertMayNotBurn(PickleContext ctx, string nickname, string metalName) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        MetalDef metal = RequireGodMetal(ctx, metalName);

        CosmereLookup.AssertThat(
            ctx,
            !ConnectionUtility.MayUseMetal(pawn, metal),
            $"'{nickname}' should not be able to burn '{metalName}'",
            () => DescribeMetal(pawn, metal));
    }

    /// <summary>Says which world this save is on, which decides who can naturalise into it.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="worldName">The CosmereWorldDef to make primary.</param>
    [Given("the save's world is {string}")]
    public void SetWorld(PickleContext ctx, string worldName) {
        ctx.Require(Current.Game != null, "no game is loaded, so this save has no world to set");
        CosmereWorldDef world = CosmereLookup.RequireDef<CosmereWorldDef>(worldName);
        WorldUtility.Set(world);

        ctx.Require(
            WorldUtility.Primary == world,
            $"the world would not move to '{worldName}'; it reads " +
            $"'{WorldUtility.Primary?.defName ?? "none"}'");
    }

    /// <summary>Gives a pawn the ancestry of a shardworld, the way being born there does.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn to change.</param>
    /// <param name="xenotypeName">The xenotype def to give them.</param>
    [Given("{string} xenotype is {string}")]
    public void SetXenotype(PickleContext ctx, string nickname, string xenotypeName) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        XenotypeDef xenotype = CosmereLookup.RequireDef<XenotypeDef>(xenotypeName);
        ctx.Require(pawn.genes != null, $"pawn '{nickname}' has no gene tracker, so they have no xenotype");

        Pawn_GeneTracker genes = pawn.genes!;
        genes.SetXenotype(xenotype);

        ctx.Require(
            genes.Xenotype == xenotype,
            $"pawn '{nickname}' would not take the xenotype '{xenotypeName}'; they read " +
            $"'{genes.Xenotype?.defName ?? "none"}'");
    }

    /// <summary>Sets how long a pawn has lived on this world, which is what naturalises an
    /// off-worlder.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn to change.</param>
    /// <param name="years">The years lived here. Residence stops counting at its own cap.</param>
    [Given("{string} has lived on this world for {int} years")]
    public void SetResidence(PickleContext ctx, string nickname, int years) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        ctx.Require(years >= 0 && years <= 100, $"this step takes 0 to 100 years; it was given {years}");

        ResidenceTracker? tracker = GameComponentCache<ResidenceTracker>.Get();
        ctx.Require(tracker != null, "no game is loaded, so nobody has lived anywhere yet");

        int wanted = years * ConnectionMath.TicksPerYear;
        tracker!.AdjustTicks(pawn, wanted - tracker.TicksFor(pawn));

        int capped = Math.Min(wanted, ConnectionMath.TicksToFullResidence);
        ctx.Require(
            tracker.TicksFor(pawn) == capped,
            $"'{nickname}' residence would not move to {capped} ticks; it reads {tracker.TicksFor(pawn)}");
    }

    /// <summary>Charges a thing to the top, the way a highstorm fills a sphere.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="defName">The thing def in the cell.</param>
    /// <param name="x">The cell's x coordinate.</param>
    /// <param name="z">The cell's z coordinate.</param>
    [When("I fill the {string} at \\({int}, {int}\\) with investiture")]
    public void FillThing(PickleContext ctx, string defName, int x, int z) {
        CosmereLookup.RequireThingHolder(ctx, defName, x, z).FillInvestiture();
    }

    /// <summary>Takes a thing out of the colony's reach, so only the clock touches it.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="defName">The thing def in the cell.</param>
    /// <param name="x">The cell's x coordinate.</param>
    /// <param name="z">The cell's z coordinate.</param>
    /// <remarks>Sharing off stops a Radiant drinking it, forbidding stops a hauler moving it off
    /// the cell. Neither touches the drain rate.</remarks>
    [When("I put the {string} at \\({int}, {int}\\) beyond the colony's reach")]
    public void WithholdThing(PickleContext ctx, string defName, int x, int z) {
        InvestitureHolder holder = CosmereLookup.RequireThingHolder(ctx, defName, x, z);
        holder.sharingInvestiture = false;
        holder.parent.SetForbidden(true, false);

        ctx.Require(
            holder.parent.IsForbidden(Faction.OfPlayer),
            $"the {defName} at ({x}, {z}) will not take a forbidden flag, so a hauler can still " +
            "carry it off the cell this scenario reads");
    }

    /// <summary>Takes a thing out of the colony's reach and stops its decay, so only this
    /// scenario's own steps can move it.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="defName">The thing def in the cell.</param>
    /// <param name="x">The cell's x coordinate.</param>
    /// <param name="z">The cell's z coordinate.</param>
    /// <remarks>The save runs unpaused, and a diamond spends half a point every rare tick - a
    /// third of the figure a transfer scenario asserts. Decay is measured on its own elsewhere.</remarks>
    [When("I put the {string} at \\({int}, {int}\\) beyond the colony and the clock")]
    public void StillThing(PickleContext ctx, string defName, int x, int z) {
        WithholdThing(ctx, defName, x, z);
        CosmereLookup.RequireThingHolder(ctx, defName, x, z).drainRate = 0f;
    }

    /// <summary>Asserts a thing holds every drop it has room for.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="defName">The thing def in the cell.</param>
    /// <param name="x">The cell's x coordinate.</param>
    /// <param name="z">The cell's z coordinate.</param>
    [Then("the {string} at \\({int}, {int}\\) is full")]
    public void AssertThingFull(PickleContext ctx, string defName, int x, int z) {
        InvestitureHolder holder = CosmereLookup.RequireThingHolder(ctx, defName, x, z);

        CosmereLookup.AssertThat(
            ctx,
            holder.isFull,
            $"the {defName} at ({x}, {z}) should be full",
            () => CosmereLookup.DescribeHolder(holder));
    }

    /// <summary>Draws Investiture out of one thing and into another, the way a Radiant drinks a
    /// sphere.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="defName">The thing def doing the drawing.</param>
    /// <param name="x">The drawing thing's x coordinate.</param>
    /// <param name="z">The drawing thing's z coordinate.</param>
    /// <param name="amount">How much to ask for. The holders decide how much actually moves.</param>
    /// <param name="sourceDefName">The thing def being drawn from.</param>
    /// <param name="sourceX">The source's x coordinate.</param>
    /// <param name="sourceZ">The source's z coordinate.</param>
    [When("the {string} at \\({int}, {int}\\) absorbs {float} investiture from the {string} at \\({int}, {int}\\)")]
    public void AbsorbFromThing(
        PickleContext ctx,
        string defName,
        int x,
        int z,
        float amount,
        string sourceDefName,
        int sourceX,
        int sourceZ
    ) {
        InvestitureHolder target = CosmereLookup.RequireThingHolder(ctx, defName, x, z);
        InvestitureHolder source = CosmereLookup.RequireThingHolder(ctx, sourceDefName, sourceX, sourceZ);
        ctx.Require(amount > 0f, $"a draw needs a positive amount; it was given {amount:0.###}");

        bool ran = target.AbsorbInvestitureFrom(source.parent, amount, out float _);

        ctx.Require(
            ran,
            $"the {sourceDefName} at ({sourceX}, {sourceZ}) cannot be drawn from at all. {CosmereLookup.DescribeHolder(source)}");
    }

    /// <summary>Runs the clock and asserts a thing bled the Investiture its own decay rate
    /// declares.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="defName">The thing def in the cell.</param>
    /// <param name="x">The cell's x coordinate.</param>
    /// <param name="z">The cell's z coordinate.</param>
    /// <param name="ticks">How many game ticks to run.</param>
    /// <returns>A task that completes when the step finishes. A failed assertion faults it.</returns>
    /// <remarks>A holder spends once a rare interval, so the count of intervals crossed is what
    /// the loss is measured against, give or take one boundary.</remarks>
    [Then("the {string} at \\({int}, {int}\\) loses its declared investiture over {int} ticks", TimeoutSeconds = WaitTimeoutSeconds)]
    public async Task AssertDeclaredDrain(PickleContext ctx, string defName, int x, int z, int ticks) {
        InvestitureHolder holder = CosmereLookup.RequireThingHolder(ctx, defName, x, z);
        ctx.Require(
            ticks >= UpkeepRate.TicksPerRareInterval * 2,
            $"a drain needs at least two rare intervals to be measurable; {ticks} ticks leaves the " +
            $"tolerance wide enough to swallow the whole expected loss");
        ctx.Require(
            holder.drainRate > 0f,
            $"the {defName} at ({x}, {z}) declares no decay, so it can never lose anything. {CosmereLookup.DescribeHolder(holder)}");

        float before = holder.currentInvestitureSelf;
        int startedAt = Find.TickManager.TicksGame;
        float wanted = holder.drainRate * (ticks / (float)UpkeepRate.TicksPerRareInterval);
        ctx.Require(
            before > wanted,
            $"the {defName} at ({x}, {z}) holds {before:0.###}, less than the {wanted:0.###} it would " +
            "spend over this span, so the floor would decide the answer rather than the rate");

        await Advance(ctx, ticks);

        int elapsed = Find.TickManager.TicksGame - startedAt;
        float lost = before - holder.currentInvestitureSelf;
        float expected = holder.drainRate * (elapsed / (float)UpkeepRate.TicksPerRareInterval);
        float slack = holder.drainRate + 0.001f;

        CosmereLookup.AssertThat(
            ctx,
            Math.Abs(lost - expected) <= slack,
            $"the {defName} at ({x}, {z}) should lose about {expected:0.###} investiture over {elapsed} ticks",
            () => $"it lost {lost:0.###}, which is {Math.Abs(lost - expected):0.###} off a declared " +
                $"{holder.drainRate:0.####} per {UpkeepRate.TicksPerRareInterval} ticks. {CosmereLookup.DescribeHolder(holder)}");
    }

    // Fast mode leaves the game paused and drives ticks by hand, the way the built-in wait step does.
    private static async Task Advance(PickleContext ctx, int ticks) {
        int deadline = Find.TickManager.TicksGame + ticks;

        while (Find.TickManager.TicksGame < deadline) {
            if (PickleRunMode.Current != PickleRunMode.Mode.Fast) {
                await ctx.WaitTicks(TicksPerYield);
                continue;
            }

            for (int i = 0; i < TicksPerYield && Find.TickManager.TicksGame < deadline; i++) {
                Find.TickManager.DoSingleTick();
            }

            await ctx.WaitFrames(1);
        }
    }

    private static void AssertMoved(
        PickleContext ctx,
        string nickname,
        string shardName,
        Func<int, int, bool> holds,
        string wanted
    ) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        ShardDef shard = CosmereLookup.RequireDef<ShardDef>(shardName);

        ctx.Require(
            Readings(ctx).Taken.TryGetValue((pawn.thingIDNumber, shard.defName), out int before),
            $"'{nickname}' tie to '{shardName}' was never recorded; run " +
            $"'I record \"{nickname}\" connection to the shard \"{shardName}\"' first");

        int now = ConnectionUtility.StrengthOf(pawn, shard);

        CosmereLookup.AssertThat(
            ctx,
            holds(before, now),
            $"'{nickname}' connection to '{shardName}' {wanted} since it was recorded",
            () => $"it went from {before} to {now}. {CosmereLookup.DescribeConnection(pawn, shard)}");
    }

    // The floor Harmony lends, rebuilt from the sibling Shards rather than read back off the total.
    private static int ImpliedHarmony(Pawn pawn, ShardDef shard) {
        if (shard.defName is "Ruin" or "Preservation") return CarriedToward(pawn, "Harmony");

        if (shard.defName == "Harmony") {
            return ConnectionMath.HarmonyFrom(
                CarriedToward(pawn, "Ruin"),
                CarriedToward(pawn, "Preservation"));
        }

        return 0;
    }

    // A Shard the save never enabled lends nothing, which is what the reading itself does.
    private static int CarriedToward(Pawn pawn, string shardName) {
        ShardDef? shard = DefDatabase<ShardDef>.GetNamedSilentFail(shardName);

        return shard == null ? 0 : CarriedFrom(ConnectionUtility.BreakdownFor(pawn, shard));
    }

    private static int CarriedFrom(ConnectionBreakdown parts) {
        return ConnectionMath.Clamp(
            ConnectionMath.Compose(parts.Ancestry, parts.Residence, parts.Investiture, parts.Earned) - parts.Held);
    }

    private static ConnectionReadings Readings(PickleContext ctx) {
        try {
            return ctx.Get<ConnectionReadings>();
        } catch (InvalidOperationException) {
            ConnectionReadings fresh = new ConnectionReadings();
            ctx.Set(fresh);
            return fresh;
        }
    }

    private static MetalDef RequireGodMetal(PickleContext ctx, string metalName) {
        MetalDef metal = CosmereLookup.RequireDef<MetalDef>(metalName);
        ctx.Require(metal.godMetal, $"'{metalName}' is not a god metal, so Connection never gates it");

        return metal;
    }

    private static string DescribeMetal(Pawn pawn, MetalDef metal) {
        List<string> readings = [];
        for (int i = 0; i < metal.shards.Count; i++) {
            ShardGrant entry = metal.shards[i];
            readings.Add(
                $"{entry.shard.defName}={ConnectionUtility.StrengthOf(pawn, entry.shard)} (grant {entry.grant})");
        }

        string name = pawn.Name?.ToStringShort ?? pawn.LabelShort;
        return $"{name} needs {ConnectionMath.GodMetalThreshold} toward one of {metal.defName}'s shards; " +
            (readings.Count == 0 ? "it names no shards" : string.Join(", ", readings));
    }

    /// <summary>What each recorded pawn read toward each Shard when a step took the reading.</summary>
    private sealed class ConnectionReadings {
        public Dictionary<(int pawnId, string shardName), int> Taken { get; } = [];
    }
}
