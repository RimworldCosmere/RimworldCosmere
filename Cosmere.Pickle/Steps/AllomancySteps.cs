using System;
using Cosmere.Core.Comp.Game;
using Cosmere.Core.Util;
using Cosmere.Pickle.Lookup;
using Cosmere.System.Scadrial.Allomancy;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Allomancy.Hediff;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Extension;
using Cosmere.System.Scadrial.Gene;
using Cosmere.System.Scadrial.Hemalurgy;
using Cosmere.System.Scadrial.Hemalurgy.Comp.Thing;
using Cosmere.System.Scadrial.Util;
using RimWorks.Pickle;
using RimWorld;
using Verse;
using ShardDef = Cosmere.Core.Def.ShardDef;

namespace Cosmere.Pickle.Steps;

/// <summary>Scadrial's metallic arts: burning a metal, shoving off one, and driving a spike
/// through somebody. The shard overlap step lives here too, because Harmony is its only caller.</summary>
[PickleSteps]
public class AllomancySteps {
    /// <summary>A whole charge off a live donor. The recipes scale this down; nothing here does.</summary>
    private const float FullChargeStrength = 1f;

    /// <summary>Turns a Shard on and leaves the ones it conflicts with running.</summary>
    /// <remarks>Harmony is Ruin and Preservation held together, and the moment of ascension is the
    /// one time all three hold this cosmere at once. The plain enable step resolves that away.</remarks>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="shardName">The Shard def to enable.</param>
    [Given("I enable the shard {string} alongside its conflicts")]
    public void EnableShardWithOverlap(PickleContext ctx, string shardName) {
        Shards shards = RequireShards(ctx);
        ShardDef shard = CosmereLookup.RequireDef<ShardDef>(shardName);

        shards.EnableShard(shard, allowConflicts: true);

        CosmereLookup.AssertThat(
            ctx,
            shards.IsEnabled(shard),
            $"the shard '{shardName}' should be enabled alongside its conflicts",
            () => DescribeShards(shards));
    }

    /// <summary>Lights a metal, the way the gizmo does. Forced: it does not ask whether the
    /// reserve covers the burn, so a scenario can watch the game pull the plug itself.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The Allomancer.</param>
    /// <param name="abilityDefName">The allomantic ability def to light.</param>
    [When("{string} starts burning {string}")]
    public void StartBurning(PickleContext ctx, string nickname, string abilityDefName) {
        AllomancyAbility ability = RequireAbility(ctx, nickname, abilityDefName);
        ability.UpdateStatus(BurningStatus.Burning);

        CosmereLookup.AssertThat(
            ctx,
            ability.status.IsActive,
            $"'{nickname}' should be burning '{abilityDefName}' once it is lit",
            () => DescribeBurn(ability));
    }

    /// <summary>Puts a metal out.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The Allomancer.</param>
    /// <param name="abilityDefName">The allomantic ability def to put out.</param>
    [When("{string} stops burning {string}")]
    public void StopBurning(PickleContext ctx, string nickname, string abilityDefName) {
        AllomancyAbility ability = RequireAbility(ctx, nickname, abilityDefName);
        ability.UpdateStatus(BurningStatus.Off);

        CosmereLookup.AssertThat(
            ctx,
            !ability.status.IsActive,
            $"'{nickname}' should have put '{abilityDefName}' out",
            () => DescribeBurn(ability));
    }

    /// <summary>Asserts a metal is alight.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The Allomancer.</param>
    /// <param name="abilityDefName">The allomantic ability def expected to be alight.</param>
    [Then("{string} is burning {string}")]
    public void AssertBurning(PickleContext ctx, string nickname, string abilityDefName) {
        AllomancyAbility ability = RequireAbility(ctx, nickname, abilityDefName);

        CosmereLookup.AssertThat(
            ctx,
            ability.status.IsActive,
            $"'{nickname}' should be burning '{abilityDefName}'",
            () => DescribeBurn(ability));
    }

    /// <summary>Asserts a metal is out.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The Allomancer.</param>
    /// <param name="abilityDefName">The allomantic ability def expected to be out.</param>
    [Then("{string} is not burning {string}")]
    public void AssertNotBurning(PickleContext ctx, string nickname, string abilityDefName) {
        AllomancyAbility ability = RequireAbility(ctx, nickname, abilityDefName);

        CosmereLookup.AssertThat(
            ctx,
            !ability.status.IsActive,
            $"'{nickname}' should not be burning '{abilityDefName}'",
            () => DescribeBurn(ability));
    }

    /// <summary>Asserts how hard a metal is being burnt: 0 out, 1 burning, 2 flaring, 10 supercharged.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The Allomancer.</param>
    /// <param name="abilityDefName">The allomantic ability def to read.</param>
    /// <param name="power">The burn power expected.</param>
    [Then("{string} burn power for {string} is {int}")]
    public void AssertBurnPower(PickleContext ctx, string nickname, string abilityDefName, int power) {
        AllomancyAbility ability = RequireAbility(ctx, nickname, abilityDefName);

        CosmereLookup.AssertThat(
            ctx,
            ability.status.power == power,
            $"'{nickname}' should be burning '{abilityDefName}' at power {power}",
            () => DescribeBurn(ability));
    }

    /// <summary>Asserts the game would let a burn start, which is the reserve check and the
    /// shard check the gizmo makes.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The Allomancer.</param>
    /// <param name="abilityDefName">The allomantic ability def to ask about.</param>
    [Then("{string} can burn {string}")]
    public void AssertCanBurn(PickleContext ctx, string nickname, string abilityDefName) {
        AllomancyAbility ability = RequireAbility(ctx, nickname, abilityDefName);

        CosmereLookup.AssertThat(
            ctx,
            ability.CanCast.Accepted,
            $"'{nickname}' should be allowed to burn '{abilityDefName}'",
            () => DescribeBurn(ability));
    }

    /// <summary>Asserts the game refuses a burn.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The Allomancer.</param>
    /// <param name="abilityDefName">The allomantic ability def to ask about.</param>
    [Then("{string} cannot burn {string}")]
    public void AssertCannotBurn(PickleContext ctx, string nickname, string abilityDefName) {
        AllomancyAbility ability = RequireAbility(ctx, nickname, abilityDefName);

        CosmereLookup.AssertThat(
            ctx,
            !ability.CanCast.Accepted,
            $"'{nickname}' should be refused the burn of '{abilityDefName}'",
            () => DescribeBurn(ability));
    }

    /// <summary>Lays a nicrosil supercharge on somebody else, the effect the maintain job holds
    /// on its target. Duralumin needs no step: burning it puts the charge on the burner.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The Allomancer burning nicrosil.</param>
    /// <param name="targetNickname">The pawn the charge lands on.</param>
    [When("{string} supercharges {string}")]
    public void Supercharge(PickleContext ctx, string nickname, string targetNickname) {
        AllomancyAbility ability = RequireAbility(ctx, nickname, "Cosmere_Scadrial_Ability_Nicrosil");
        Pawn target = CosmereLookup.RequirePawn(ctx, targetNickname);

        ability.localTarget = target;
        ability.UpdateStatus(BurningStatus.Burning);
        target.GetOrAddHediff(ability, ability.def, ability.pawn);

        CosmereLookup.AssertThat(
            ctx,
            AllomancyUtility.FindSurgeChargeHediff(target) != null,
            $"'{targetNickname}' should be carrying a supercharge once '{nickname}' burns nicrosil at them",
            () => DescribeSurge(target));
    }

    /// <summary>Fires the supercharge, which throws every other metal the pawn is burning up to
    /// power ten. This is the call the cast and maintain jobs make.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn carrying the charge.</param>
    [When("{string} triggers their supercharge")]
    public void TriggerSupercharge(PickleContext ctx, string nickname) {
        RequireSurge(ctx, nickname).Burn();
    }

    /// <summary>Ends the supercharge, which wipes the reserve of every metal it lifted and of
    /// the metal that paid for it.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn carrying the charge.</param>
    [When("{string} spends their supercharge")]
    public void SpendSupercharge(PickleContext ctx, string nickname) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        RequireSurge(ctx, nickname).PostBurn();

        CosmereLookup.AssertThat(
            ctx,
            AllomancyUtility.FindSurgeChargeHediff(pawn) == null,
            $"'{nickname}' should have no supercharge left once it is spent",
            () => DescribeSurge(pawn));
    }

    /// <summary>Asserts a steelpush or ironpull has something to grasp, which is the check
    /// <c>ExternalPhysicalTargetAbility</c> makes before it spends any metal.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The Allomancer.</param>
    /// <param name="abilityDefName">The push or pull ability def.</param>
    /// <param name="defName">The thing def at the cell.</param>
    /// <param name="x">The cell's x coordinate.</param>
    /// <param name="z">The cell's z coordinate.</param>
    [Then("{string} burning {string} can grasp the {string} at \\({int}, {int}\\)")]
    public void AssertCanGrasp(
        PickleContext ctx,
        string nickname,
        string abilityDefName,
        string defName,
        int x,
        int z) {
        AllomancyAbility ability = RequireAbility(ctx, nickname, abilityDefName);
        Verse.Thing thing = RequireThingAt(ctx, defName, x, z);

        CosmereLookup.AssertThat(
            ctx,
            ability.CanApplyOn(new LocalTargetInfo(thing)),
            $"'{nickname}' should find metal to grasp in the '{defName}' at ({x}, {z})",
            () => DescribeShove(ability, thing));
    }

    /// <summary>Asserts a steelpush or ironpull has nothing to grasp.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The Allomancer.</param>
    /// <param name="abilityDefName">The push or pull ability def.</param>
    /// <param name="defName">The thing def at the cell.</param>
    /// <param name="x">The cell's x coordinate.</param>
    /// <param name="z">The cell's z coordinate.</param>
    [Then("{string} burning {string} cannot grasp the {string} at \\({int}, {int}\\)")]
    public void AssertCannotGrasp(
        PickleContext ctx,
        string nickname,
        string abilityDefName,
        string defName,
        int x,
        int z) {
        AllomancyAbility ability = RequireAbility(ctx, nickname, abilityDefName);
        Verse.Thing thing = RequireThingAt(ctx, defName, x, z);

        CosmereLookup.AssertThat(
            ctx,
            !ability.CanApplyOn(new LocalTargetInfo(thing)),
            $"'{nickname}' should find no metal to grasp in the '{defName}' at ({x}, {z})",
            () => DescribeShove(ability, thing));
    }

    /// <summary>Asserts a shove sends the target, not the Allomancer.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The Allomancer.</param>
    /// <param name="abilityDefName">The push or pull ability def.</param>
    /// <param name="defName">The thing def at the cell.</param>
    /// <param name="x">The cell's x coordinate.</param>
    /// <param name="z">The cell's z coordinate.</param>
    [Then("{string} burning {string} shoves the {string} at \\({int}, {int}\\)")]
    public void AssertShovesTarget(
        PickleContext ctx,
        string nickname,
        string abilityDefName,
        string defName,
        int x,
        int z) {
        AllomancyAbility ability = RequireAbility(ctx, nickname, abilityDefName);
        Verse.Thing thing = RequireThingAt(ctx, defName, x, z);

        CosmereLookup.AssertThat(
            ctx,
            !MovesCaster(ability, thing),
            $"the shove from '{nickname}' should send the '{defName}' at ({x}, {z})",
            () => DescribeShove(ability, thing));
    }

    /// <summary>Asserts a shove sends the Allomancer instead, which is what a bolted-down
    /// building does: it reads heavier than the person pushing off it.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The Allomancer.</param>
    /// <param name="abilityDefName">The push or pull ability def.</param>
    /// <param name="defName">The thing def at the cell.</param>
    /// <param name="x">The cell's x coordinate.</param>
    /// <param name="z">The cell's z coordinate.</param>
    [Then("{string} burning {string} is thrown off the {string} at \\({int}, {int}\\)")]
    public void AssertShovesCaster(
        PickleContext ctx,
        string nickname,
        string abilityDefName,
        string defName,
        int x,
        int z) {
        AllomancyAbility ability = RequireAbility(ctx, nickname, abilityDefName);
        Verse.Thing thing = RequireThingAt(ctx, defName, x, z);

        CosmereLookup.AssertThat(
            ctx,
            MovesCaster(ability, thing),
            $"the shove from '{nickname}' should throw them off the '{defName}' at ({x}, {z})",
            () => DescribeShove(ability, thing));
    }

    /// <summary>Lays a bare spike of one metal on a cell. The metal is the spike's stuff, which
    /// is the only place the steal type and the metal filters read it from.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="metalName">The metal thing def the spike is forged from.</param>
    /// <param name="x">The cell's x coordinate.</param>
    /// <param name="z">The cell's z coordinate.</param>
    [Given("a {string} hemalurgic spike is at \\({int}, {int}\\)")]
    public void SpikeIsAt(PickleContext ctx, string metalName, int x, int z) {
        Map map = RequireMap(ctx);
        ThingDef spikeDef = RequireSpikeDef(ctx);
        ThingDef stuff = CosmereLookup.RequireDef<ThingDef>(metalName);
        IntVec3 cell = RequireCell(ctx, map, x, z);

        ctx.Require(
            stuff.IsStuff,
            $"'{metalName}' is not a material, so no spike can be forged from it");

        Verse.Thing spike = ThingMaker.MakeThing(spikeDef, stuff);
        GenSpawn.Spawn(spike, cell, map, WipeMode.Vanish);

        CosmereLookup.AssertThat(
            ctx,
            spike.TryGetComp<HemalurgicSpike>() != null,
            $"the '{metalName}' spike at ({x}, {z}) should carry a hemalurgic spike comp",
            () => $"spawned {spike.LabelCap} of {spike.Stuff?.defName ?? "(no stuff)"}");
    }

    /// <summary>Drives a spike through a donor, taking whatever its metal takes. The donor keeps
    /// their life: the injury is off so later steps can read what the spike left behind.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The one holding the spike.</param>
    /// <param name="x">The spike cell's x coordinate.</param>
    /// <param name="z">The spike cell's z coordinate.</param>
    /// <param name="donorNickname">The donor.</param>
    [When("{string} drives the spike at \\({int}, {int}\\) through {string}")]
    public void DriveSpike(PickleContext ctx, string nickname, int x, int z, string donorNickname) {
        DriveSpikeTakingGene(ctx, nickname, x, z, donorNickname, null);
    }

    /// <summary>Drives a spike through a donor and names the power it takes, which the alloy
    /// metals need: steel takes one Allomantic gene, not all of them.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The one holding the spike.</param>
    /// <param name="x">The spike cell's x coordinate.</param>
    /// <param name="z">The spike cell's z coordinate.</param>
    /// <param name="donorNickname">The donor.</param>
    /// <param name="geneDefName">The gene the spike takes.</param>
    [When("{string} drives the spike at \\({int}, {int}\\) through {string}, taking the gene {string}")]
    public void DriveSpikeForGene(
        PickleContext ctx,
        string nickname,
        int x,
        int z,
        string donorNickname,
        string geneDefName) {
        DriveSpikeTakingGene(ctx, nickname, x, z, donorNickname, CosmereLookup.RequireDef<GeneDef>(geneDefName));
    }

    /// <summary>Asserts a spike holds a charge.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="x">The spike cell's x coordinate.</param>
    /// <param name="z">The spike cell's z coordinate.</param>
    [Then("the spike at \\({int}, {int}\\) is charged")]
    public void AssertSpikeCharged(PickleContext ctx, int x, int z) {
        HemalurgicSpike spike = RequireSpikeAt(ctx, x, z);

        CosmereLookup.AssertThat(
            ctx,
            spike.isCharged,
            $"the spike at ({x}, {z}) should hold a charge",
            () => DescribeSpike(spike));
    }

    /// <summary>Asserts a spike is a bare lump of metal.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="x">The spike cell's x coordinate.</param>
    /// <param name="z">The spike cell's z coordinate.</param>
    [Then("the spike at \\({int}, {int}\\) is not charged")]
    public void AssertSpikeUncharged(PickleContext ctx, int x, int z) {
        HemalurgicSpike spike = RequireSpikeAt(ctx, x, z);

        CosmereLookup.AssertThat(
            ctx,
            !spike.isCharged,
            $"the spike at ({x}, {z}) should hold nothing",
            () => DescribeSpike(spike));
    }

    /// <summary>Asserts what a spike took: a gene def name, or one of the plain names the
    /// charge uses for an attribute, Investiture or Connection.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="x">The spike cell's x coordinate.</param>
    /// <param name="z">The spike cell's z coordinate.</param>
    /// <param name="stolen">The name the charge should carry.</param>
    [Then("the spike at \\({int}, {int}\\) holds {string}")]
    public void AssertSpikeHolds(PickleContext ctx, int x, int z, string stolen) {
        HemalurgicSpike spike = RequireSpikeAt(ctx, x, z);

        CosmereLookup.AssertThat(
            ctx,
            string.Equals(spike.chargeData?.stolenDefName, stolen, StringComparison.Ordinal),
            $"the spike at ({x}, {z}) should hold '{stolen}'",
            () => DescribeSpike(spike));
    }

    /// <summary>Asserts a special filter matches a spike. These workers name what a bill must
    /// NOT take, so a match is a rejection.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="filterDefName">The special filter def.</param>
    /// <param name="x">The spike cell's x coordinate.</param>
    /// <param name="z">The spike cell's z coordinate.</param>
    [Then("the special filter {string} rejects the spike at \\({int}, {int}\\)")]
    public void AssertFilterRejects(PickleContext ctx, string filterDefName, int x, int z) {
        SpecialThingFilterDef filter = CosmereLookup.RequireDef<SpecialThingFilterDef>(filterDefName);
        HemalurgicSpike spike = RequireSpikeAt(ctx, x, z);

        CosmereLookup.AssertThat(
            ctx,
            filter.Worker.Matches(spike.parent),
            $"'{filterDefName}' should reject the spike at ({x}, {z})",
            () => DescribeSpike(spike));
    }

    /// <summary>Asserts a special filter lets a spike through.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="filterDefName">The special filter def.</param>
    /// <param name="x">The spike cell's x coordinate.</param>
    /// <param name="z">The spike cell's z coordinate.</param>
    [Then("the special filter {string} allows the spike at \\({int}, {int}\\)")]
    public void AssertFilterAllows(PickleContext ctx, string filterDefName, int x, int z) {
        SpecialThingFilterDef filter = CosmereLookup.RequireDef<SpecialThingFilterDef>(filterDefName);
        HemalurgicSpike spike = RequireSpikeAt(ctx, x, z);

        CosmereLookup.AssertThat(
            ctx,
            !filter.Worker.Matches(spike.parent),
            $"'{filterDefName}' should allow the spike at ({x}, {z})",
            () => DescribeSpike(spike));
    }

    /// <summary>Asserts which Shard the spikes answer to in this save. Ruin until the
    /// Catacendre, Harmony after it.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="shardName">The Shard def expected.</param>
    [Then("hemalurgy answers to the shard {string}")]
    public void AssertHemalurgicShard(PickleContext ctx, string shardName) {
        Shards shards = RequireShards(ctx);
        ShardDef shard = CosmereLookup.RequireDef<ShardDef>(shardName);

        CosmereLookup.AssertThat(
            ctx,
            string.Equals(HemalurgicShard.Current?.defName, shard.defName, StringComparison.Ordinal),
            $"hemalurgy should answer to '{shardName}'",
            () => $"it answers to {HemalurgicShard.Current?.defName ?? "(nothing)"}; {DescribeShards(shards)}");
    }

    /// <summary>Asserts this many spikes are enough for Ruin to want something.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="spikes">How many spikes the pawn carries.</param>
    [Then("Ruin has a compulsion for {int} spikes")]
    public void AssertCompulsionAvailable(PickleContext ctx, int spikes) {
        CosmereLookup.AssertThat(
            ctx,
            RuinCompulsions.Choose(spikes) != null,
            $"Ruin should have something to ask of somebody carrying {spikes} spikes",
            () => DescribeCompulsions());
    }

    /// <summary>Asserts this many spikes leave Ruin nothing to ask for.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="spikes">How many spikes the pawn carries.</param>
    [Then("Ruin has no compulsion for {int} spikes")]
    public void AssertNoCompulsionAvailable(PickleContext ctx, int spikes) {
        CosmereLookup.AssertThat(
            ctx,
            RuinCompulsions.Choose(spikes) == null,
            $"Ruin should have nothing to ask of somebody carrying {spikes} spikes",
            () => DescribeCompulsions());
    }

    /// <summary>Asserts Ruin has somebody it can still move.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn to ask about.</param>
    [Then("Ruin can move {string}")]
    public void AssertRuinCanMove(PickleContext ctx, string nickname) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);

        CosmereLookup.AssertThat(
            ctx,
            RuinCompulsions.CanBeMoved(pawn),
            $"Ruin should be able to move '{nickname}'",
            () => DescribeMovable(pawn));
    }

    /// <summary>Asserts Ruin has nobody left to move: dead, downed, already gone or locked up.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn to ask about.</param>
    [Then("Ruin cannot move {string}")]
    public void AssertRuinCannotMove(PickleContext ctx, string nickname) {
        Pawn? pawn = PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead
            .FirstOrDefault(p => string.Equals(p.Name?.ToStringShort, nickname, StringComparison.OrdinalIgnoreCase));

        ctx.Require(pawn != null, $"no pawn nicknamed '{nickname}' on any map, alive or dead");

        CosmereLookup.AssertThat(
            ctx,
            !RuinCompulsions.CanBeMoved(pawn!),
            $"Ruin should not be able to move '{nickname}'",
            () => DescribeMovable(pawn!));
    }

    private void DriveSpikeTakingGene(
        PickleContext ctx,
        string nickname,
        int x,
        int z,
        string donorNickname,
        GeneDef? selectedGene) {
        Pawn surgeon = CosmereLookup.RequirePawn(ctx, nickname);
        Pawn donor = CosmereLookup.RequirePawn(ctx, donorNickname);
        HemalurgicSpike spike = RequireSpikeAt(ctx, x, z);

        ctx.Require(
            spike.metal != null,
            $"the spike at ({x}, {z}) has no metal, so it has no steal type. forge it from a metallic arts metal");

        HemalurgicStealType stealType = spike.stealType;
        ctx.Require(
            !HemalurgicConstants.RequiresSelection(stealType) || selectedGene != null,
            $"a {spike.metal!.defName} spike takes a {stealType} power, so the step has to name the gene it takes");

        // injury off on purpose: a full spike kills the donor, and a corpse cannot be read for what it lost.
        HemalurgicChargeUtility.DriveHemalurgicCharge(
            donor,
            surgeon,
            spike,
            stealType,
            selectedGene,
            FullChargeStrength,
            applyInjury: false,
            isThinNeedle: false);

        CosmereLookup.AssertThat(
            ctx,
            spike.isCharged,
            $"the spike at ({x}, {z}) should hold a charge once it is driven through '{donorNickname}'",
            () => DescribeSpike(spike));
    }

    /// <summary>The same two calls <c>CastAllomanticAbilityAtTarget</c> makes to decide which end
    /// of the shove moves. A spawned building is bolted down and reads heavier than the caster.</summary>
    private static bool MovesCaster(AllomancyAbility ability, Verse.Thing thing) {
        return AllomanticShove.MovesCaster(TargetMass(ability, thing), CasterMass(ability));
    }

    private static float CasterMass(AllomancyAbility ability) {
        return ability.pawn.GetStatValue(RimWorld.StatDefOf.Mass) +
            MassUtility.GearAndInventoryMass(ability.pawn) * ability.GetStrength();
    }

    private static float TargetMass(AllomancyAbility ability, Verse.Thing thing) {
        bool anchored = thing.Spawned && thing.def.category == ThingCategory.Building;

        return AllomanticShove.EffectiveTargetMass(
            thing.GetStatValue(RimWorld.StatDefOf.Mass) * thing.stackCount,
            CasterMass(ability),
            anchored);
    }

    private static AllomancyAbility RequireAbility(PickleContext ctx, string nickname, string abilityDefName) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        AbilityDef def = CosmereLookup.RequireDef<AbilityDef>(abilityDefName);
        AllomancyAbility? ability = pawn.GetAllomanticAbility(def);

        ctx.Require(
            ability != null,
            $"'{nickname}' does not hold the allomantic ability '{abilityDefName}'. grant the Misting gene " +
            $"for its metal first. {DescribeHeldAbilities(pawn)}");

        return ability!;
    }

    private static SurgeChargeHediff RequireSurge(PickleContext ctx, string nickname) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        SurgeChargeHediff? surge = AllomancyUtility.FindSurgeChargeHediff(pawn);

        ctx.Require(
            surge != null,
            $"'{nickname}' carries no supercharge. burn duralumin, or have somebody burn nicrosil at them, first");

        return surge!;
    }

    private static HemalurgicSpike RequireSpikeAt(PickleContext ctx, int x, int z) {
        Map map = RequireMap(ctx);
        IntVec3 cell = RequireCell(ctx, map, x, z);
        List<Verse.Thing> things = cell.GetThingList(map);

        for (int i = 0; i < things.Count; i++) {
            HemalurgicSpike? spike = things[i].TryGetComp<HemalurgicSpike>();
            if (spike != null) return spike;
        }

        ctx.Require(false, $"no hemalurgic spike at ({x}, {z}). {DescribeCell(things)}");

        throw new InvalidOperationException("unreachable");
    }

    private static Verse.Thing RequireThingAt(PickleContext ctx, string defName, int x, int z) {
        Map map = RequireMap(ctx);
        IntVec3 cell = RequireCell(ctx, map, x, z);
        ThingDef def = CosmereLookup.RequireDef<ThingDef>(defName);
        List<Verse.Thing> things = cell.GetThingList(map);

        for (int i = 0; i < things.Count; i++) {
            if (things[i].def == def) return things[i];
        }

        ctx.Require(false, $"no '{defName}' at ({x}, {z}). {DescribeCell(things)}");

        throw new InvalidOperationException("unreachable");
    }

    private static ThingDef RequireSpikeDef(PickleContext ctx) {
        ThingDef? def = HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicSpike;

        ctx.Require(def != null, "the hemalurgic spike thing def is missing, so Cosmere.Scadrial is not loaded");

        return def!;
    }

    private static Shards RequireShards(PickleContext ctx) {
        Shards? shards = ShardUtility.shards;

        ctx.Require(
            shards != null,
            "no game is loaded, so this cosmere has no Shards yet. tag the feature @quickstart:<Name> first");

        return shards!;
    }

    private static Map RequireMap(PickleContext ctx) {
        Map? map = Find.CurrentMap;

        ctx.Require(map != null, "no map is loaded, so no step here has anywhere to put a thing");

        return map!;
    }

    private static IntVec3 RequireCell(PickleContext ctx, Map map, int x, int z) {
        IntVec3 cell = new IntVec3(x, 0, z);

        ctx.Require(
            cell.InBounds(map),
            $"cell ({x}, {z}) is outside the map, which is {map.Size.x} by {map.Size.z}");

        return cell;
    }

    private static string DescribeShards(Shards shards) {
        return shards.enabledShards.Count == 0
            ? "no shard holds this cosmere"
            : $"shards on: {string.Join(", ", shards.enabledShards.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase))}";
    }

    // the reserve and the vial are the two things CanCast weighs, so both belong in the failure
    private static string DescribeBurn(AllomancyAbility ability) {
        Allomancer gene = ability.Gene;
        AcceptanceReport report = ability.CanCast;
        string refusal = report.Accepted ? "allowed" : $"refused: {report.Reason}";

        return $"{ability.pawn.LabelShort} burns {ability.metal.defName} at power {ability.status.power} " +
            $"(active={ability.status.IsActive}); reserve={gene.Value:0.###} of {gene.Max:0.###}, " +
            $"rate={gene.BurnRate:0.#####}/charge, vial={ability.pawn.HasVial(ability.metal)}; {refusal}";
    }

    private static string DescribeSurge(Pawn pawn) {
        SurgeChargeHediff? surge = AllomancyUtility.FindSurgeChargeHediff(pawn);
        if (surge == null) return $"{pawn.LabelShort} carries no supercharge";

        return $"{pawn.LabelShort} carries a supercharge, {surge.endInTicks} ticks from spending itself";
    }

    // both masses and the anchor flag, because the anchored surplus is what flips the direction
    private static string DescribeShove(AllomancyAbility ability, Verse.Thing thing) {
        bool anchored = thing.Spawned && thing.def.category == ThingCategory.Building;

        return $"{ability.pawn.LabelShort} reads {CasterMass(ability):0.##}kg against " +
            $"{thing.LabelCap} at {TargetMass(ability, thing):0.##}kg (anchored={anchored}, " +
            $"metal={MetalDetector.GetMetalMass(thing):0.##}kg)";
    }

    private static string DescribeSpike(HemalurgicSpike spike) {
        if (!spike.isCharged) {
            return $"the {spike.parent.Stuff?.defName ?? "(no metal)"} spike holds nothing " +
                $"(steal type {spike.stealType})";
        }

        HemalurgicChargeData charge = spike.chargeData!;

        return $"the {spike.parent.Stuff?.defName ?? "(no metal)"} spike holds {charge.stealType} " +
            $"'{charge.stolenDefName}' at strength {spike.currentStrength:0.##}, " +
            $"investiture {charge.storedInvestiture:0.###}";
    }

    private static string DescribeCompulsions() {
        return "Ruin's compulsions: " + string.Join(
            ", ",
            RuinCompulsions.All.Select(c => $"{c.key} at {c.minimumSpikes}+ ({c.stateDefName})"));
    }

    private static string DescribeMovable(Pawn pawn) {
        return $"{pawn.LabelShort}: dead={pawn.Dead}, downed={pawn.Downed}, " +
            $"mentalState={pawn.InMentalState}, prisoner={pawn.IsPrisoner}, awake={!pawn.Dead && pawn.Awake()}";
    }

    private static string DescribeCell(List<Verse.Thing> things) {
        return things.Count == 0
            ? "the cell is empty"
            : $"the cell holds: {string.Join(", ", things.Select(t => t.def.defName))}";
    }

    private static string DescribeHeldAbilities(Pawn pawn) {
        List<Ability>? held = pawn.abilities?.AllAbilitiesForReading;

        return held == null || held.Count == 0
            ? "the pawn holds no abilities"
            : $"the pawn holds: {string.Join(", ", held.Select(a => a.def.defName).OrderBy(n => n, StringComparer.OrdinalIgnoreCase))}";
    }
}
