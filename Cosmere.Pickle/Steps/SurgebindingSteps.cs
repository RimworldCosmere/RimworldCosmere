using System;
using System.Threading.Tasks;
using Cosmere.Core.Ability;
using Cosmere.Core.Comp.Game;
using Cosmere.Core.Extension;
using Cosmere.Core.Investiture;
using Cosmere.Core.Quest;
using Cosmere.Core.Quest.Objective;
using Cosmere.Core.Util;
using Cosmere.Pickle.Lookup;
using Cosmere.System.Roshar.Comp.Game;
using Cosmere.System.Roshar.Comp.Map;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Extension;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.Nightwatcher;
using Cosmere.System.Roshar.Surgebinding.Ability;
using Cosmere.System.Roshar.Util;
using RimWorks.Pickle;
using RimWorks.Pickle.Runtime;
using RimWorld;
using UnityEngine;
using Verse;
using Ability = RimWorld.Ability;
using Highstorm = Cosmere.System.Roshar.GameCondition.Highstorm;
using ShardDefOf = Cosmere.System.Roshar.ShardDefOf;

namespace Cosmere.Pickle.Steps;

/// <summary>Surgebinding: the Nahel bond and its Ideals, the surges a bond pays Stormlight for,
/// where a pawn stands when a highstorm arrives, and the Stormfather's capstone.</summary>
[PickleSteps]
public class SurgebindingSteps {
    /// <summary>Drain rates come off a float division, so an equality assert would fail at random.</summary>
    private const float RateTolerance = 0.01f;

    /// <summary>How far out a placement step looks for a cell it can use. Wide, because a shelter
    /// needs five by five of unroofed ground and a built-up colony has little of that.</summary>
    private const float SearchRadius = 30f;

    /// <summary>The darkeyed caste's xenotype, which a pawn is put in to pin a rolled caste down.</summary>
    private const string DarkeyesXenotype = "Cosmere_Roshar_Xenotype_Darkeyes";

    /// <summary>Makes a pawn a darkeyes before anything else touches them, so a scenario about the
    /// caste transition gets the caste it asked for instead of a coin flip.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    [Given("{string} is already darkeyed")]
    public void MakeDarkeyed(PickleContext ctx, string nickname) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        ctx.Require(pawn.genes != null, $"'{nickname}' has no gene tracker, so they have no caste");

        // the vanilla call, which is safe here: it is the first thing done to a freshly made pawn
        pawn.genes!.SetXenotype(CosmereLookup.RequireDef<XenotypeDef>(DarkeyesXenotype));

        CosmereLookup.AssertThat(
            ctx,
            CasteUtility.IsDarkeyes(pawn),
            $"'{nickname}' should be a darkeyes",
            () => DescribeCaste(pawn));
    }

    /// <summary>Asserts a pawn's eyes have lightened, which is what swearing the First Ideal does
    /// to a darkeyes.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    [Then("{string} eyes have lightened")]
    public void AssertLighteyed(PickleContext ctx, string nickname) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);

        CosmereLookup.AssertThat(
            ctx,
            CasteUtility.IsLighteyes(pawn) && !CasteUtility.IsDarkeyes(pawn),
            $"'{nickname}' should be a lighteyes now",
            () => DescribeCaste(pawn));
    }

    /// <summary>Makes a pawn a lighteyes before anything else touches them. Caste is rolled per
    /// pawn, and the first Ideal converts a darkeyes, which is a step these scenarios are not about.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    [Given("{string} is already lighteyed")]
    public void MakeLighteyed(PickleContext ctx, string nickname) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        CasteUtility.DarkeyesToLighteyes(pawn);

        CosmereLookup.AssertThat(
            ctx,
            !CasteUtility.IsDarkeyes(pawn),
            $"'{nickname}' should be a lighteyes",
            () => $"{nickname} is still a darkeyes. {DescribeBonds(pawn)}");
    }

    /// <summary>Grants a Nightwatcher boon the way the encounter does, standard effects and the
    /// boon's own applicator both. The eyes-of-light boon runs the caste transition from here.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn the Nightwatcher answers.</param>
    /// <param name="boonDefName">The NightwatcherBoonDef to grant.</param>
    [When("the Nightwatcher grants {string} the boon {string}")]
    public void GrantBoon(PickleContext ctx, string nickname, string boonDefName) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        NightwatcherBoonDef boon = CosmereLookup.RequireDef<NightwatcherBoonDef>(boonDefName);

        NightwatcherSystem.ApplyBoon(pawn, boon);
    }

    /// <summary>Asserts a pawn carries a Nahel bond to one order.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    /// <param name="orderDefName">The order defName, which is bare: "Windrunner", not a prefixed one.</param>
    [Then("{string} is bonded to the radiant order {string}")]
    public void AssertBonded(PickleContext ctx, string nickname, string orderDefName) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        RadiantOrderDef order = CosmereLookup.RequireDef<RadiantOrderDef>(orderDefName);

        CosmereLookup.AssertThat(
            ctx,
            FindBond(pawn, order) != null,
            $"'{nickname}' should be bonded to the {orderDefName}s",
            () => DescribeBonds(pawn));
    }

    /// <summary>Asserts a pawn carries no Nahel bond to one order.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    /// <param name="orderDefName">The order defName that must be absent.</param>
    [Then("{string} is not bonded to the radiant order {string}")]
    public void AssertNotBonded(PickleContext ctx, string nickname, string orderDefName) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        RadiantOrderDef order = CosmereLookup.RequireDef<RadiantOrderDef>(orderDefName);

        CosmereLookup.AssertThat(
            ctx,
            FindBond(pawn, order) == null,
            $"'{nickname}' should not be bonded to the {orderDefName}s",
            () => DescribeBonds(pawn));
    }

    /// <summary>Moves a bond to an Ideal. The number is the zero-based <c>CurrentIdeal</c> the
    /// game stores, so the Second Ideal is 1 here and reads as 2 on screen.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    /// <param name="orderDefName">The order defName.</param>
    /// <param name="ideal">The zero-based Ideal to move to.</param>
    [When("{string} reaches ideal {int} of the radiant order {string}")]
    public void SetIdeal(PickleContext ctx, string nickname, int ideal, string orderDefName) {
        ctx.Require(ideal is >= 0 and <= 4, $"an Ideal runs 0 to 4 on the stored scale; got {ideal}");

        Surgebinder bond = RequireBond(ctx, nickname, orderDefName);
        bond.CurrentIdeal = ideal;

        CosmereLookup.AssertThat(
            ctx,
            bond.CurrentIdeal == ideal,
            $"'{nickname}' should sit at stored Ideal {ideal}",
            () => DescribeBond(bond));
    }

    /// <summary>Asserts the stored, zero-based Ideal.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    /// <param name="orderDefName">The order defName.</param>
    /// <param name="ideal">The zero-based Ideal expected.</param>
    [Then("{string} stored ideal in the radiant order {string} is {int}")]
    public void AssertStoredIdeal(PickleContext ctx, string nickname, string orderDefName, int ideal) {
        Surgebinder bond = RequireBond(ctx, nickname, orderDefName);

        CosmereLookup.AssertThat(
            ctx,
            bond.CurrentIdeal == ideal,
            $"'{nickname}' stored Ideal should be {ideal}",
            () => DescribeBond(bond));
    }

    /// <summary>Asserts the Ideal the UI shows, which is the stored one plus one. Split from the
    /// stored reading on purpose: confusing the two has already shipped one defect here.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    /// <param name="orderDefName">The order defName.</param>
    /// <param name="ideal">The displayed Ideal expected.</param>
    [Then("{string} ideal in the radiant order {string} reads as {int}")]
    public void AssertDisplayedIdeal(PickleContext ctx, string nickname, string orderDefName, int ideal) {
        Surgebinder bond = RequireBond(ctx, nickname, orderDefName);

        CosmereLookup.AssertThat(
            ctx,
            bond.CurrentIdealDisplay == ideal,
            $"'{nickname}' displayed Ideal should be {ideal}",
            () => DescribeBond(bond));
    }

    /// <summary>Breaks an Ideal, the way a Radiant who fails their oaths loses one.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    /// <param name="orderDefName">The order defName.</param>
    [When("{string} regresses an ideal in the radiant order {string}")]
    public void RegressIdeal(PickleContext ctx, string nickname, string orderDefName) {
        RequireBond(ctx, nickname, orderDefName).RegressIdeal();
    }

    /// <summary>Takes Honor off this cosmere, which is the one gate every bond path checks first.
    /// Re-enable it with the shard step; a feature file that leaves Honor off breaks later scenarios.</summary>
    /// <param name="ctx">The scenario's context.</param>
    [Given("Honor no longer holds this cosmere")]
    public void DisableHonor(PickleContext ctx) {
        Shards? shards = ShardUtility.shards;
        ctx.Require(shards != null, "no game is loaded, so this cosmere has no Shards to switch off");

        ShardUtility.Disable(ShardDefOf.Honor);

        CosmereLookup.AssertThat(
            ctx,
            !ShardUtility.AreAnyEnabled(ShardDefOf.Honor),
            "Honor should no longer hold this cosmere",
            () => CosmereLookup.DescribeShards(shards!));
    }

    /// <summary>Records a broken bond against a pawn, which is what blocks a rebond for sixty days.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    /// <param name="orderDefName">The order defName the bond broke on.</param>
    [Given("{string} has broken a bond to the radiant order {string}")]
    public void RecordBrokenBond(PickleContext ctx, string nickname, string orderDefName) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        RadiantOrderDef order = CosmereLookup.RequireDef<RadiantOrderDef>(orderDefName);
        RadiantTracker tracker = RequireTracker(ctx);

        tracker.RecordBrokenBond(pawn, order.defName);

        CosmereLookup.AssertThat(
            ctx,
            !tracker.CanRebond(pawn, order.defName),
            $"the tracker should now refuse '{nickname}' a {orderDefName} bond",
            () => DescribeBonds(pawn));
    }

    /// <summary>Asks the game for a bond and asserts it says no. Runs the real
    /// <c>TryAddRadiantOrder</c>, so the Honor gate and the rebond check both get their say.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    /// <param name="orderDefName">The order defName to ask for.</param>
    [Then("the game refuses to bond {string} to the radiant order {string}")]
    public void AssertBondRefused(PickleContext ctx, string nickname, string orderDefName) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        RadiantOrderDef order = CosmereLookup.RequireDef<RadiantOrderDef>(orderDefName);
        ctx.Require(pawn.genes != null, $"'{nickname}' has no gene tracker, so no bond can reach them");

        Surgebinder? bond = pawn.genes!.TryAddRadiantOrder(order.GetSurgebindingGene());

        CosmereLookup.AssertThat(
            ctx,
            bond == null && FindBond(pawn, order) == null,
            $"the game should refuse '{nickname}' a {orderDefName} bond",
            () => DescribeBonds(pawn));
    }

    /// <summary>Turns a surge on, the way clicking its gizmo does.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    /// <param name="abilityDefName">The surge ability def.</param>
    [When("{string} starts the surge {string}")]
    public void StartSurge(PickleContext ctx, string nickname, string abilityDefName) {
        SurgebindingAbility surge = RequireSurge(ctx, nickname, abilityDefName);
        surge.UpdateStatus(Active.On);

        CosmereLookup.AssertThat(
            ctx,
            surge.IsActive,
            $"the surge '{abilityDefName}' should be running on '{nickname}'",
            () => DescribeSurges(surge.Gene));
    }

    /// <summary>Turns a surge off.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    /// <param name="abilityDefName">The surge ability def.</param>
    [When("{string} stops the surge {string}")]
    public void StopSurge(PickleContext ctx, string nickname, string abilityDefName) {
        SurgebindingAbility surge = RequireSurge(ctx, nickname, abilityDefName);
        surge.UpdateStatus(Active.Off);

        CosmereLookup.AssertThat(
            ctx,
            !surge.IsActive,
            $"the surge '{abilityDefName}' should be off on '{nickname}'",
            () => DescribeSurges(surge.Gene));
    }

    /// <summary>Asserts what a running surge charges the bond per upkeep charge. This is the rate
    /// the bond registered, not a per-second figure, so the upkeep cadence setting cannot move it.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    /// <param name="abilityDefName">The surge ability def.</param>
    /// <param name="rate">The Stormlight charged per upkeep charge.</param>
    [Then("{string} surge {string} charges {float} stormlight")]
    public void AssertSurgeRate(PickleContext ctx, string nickname, string abilityDefName, float rate) {
        SurgebindingAbility surge = RequireSurge(ctx, nickname, abilityDefName);
        float actual = RateOf(surge.Gene, surge.def);

        CosmereLookup.AssertThat(
            ctx,
            Mathf.Abs(actual - rate) <= RateTolerance,
            $"'{nickname}' surge '{abilityDefName}' should charge {rate:0.###} stormlight per upkeep charge",
            () => DescribeSurges(surge.Gene));
    }

    /// <summary>Asserts a surge is charging the bond nothing, which is how a stopped one reads.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    /// <param name="abilityDefName">The surge ability def.</param>
    [Then("{string} surge {string} charges nothing")]
    public void AssertSurgeIdle(PickleContext ctx, string nickname, string abilityDefName) {
        SurgebindingAbility surge = RequireSurge(ctx, nickname, abilityDefName);

        CosmereLookup.AssertThat(
            ctx,
            RateOf(surge.Gene, surge.def) <= 0f,
            $"'{nickname}' surge '{abilityDefName}' should charge nothing while it is off",
            () => DescribeSurges(surge.Gene));
    }

    /// <summary>Ends any highstorm still blowing, so a scenario that failed halfway through one
    /// cannot leave it running over the next scenario in the same world.</summary>
    /// <param name="ctx">The scenario's context.</param>
    [Given("no storm is running")]
    public void NoStormRunning(PickleContext ctx) {
        Map? map = Find.CurrentMap;
        ctx.Require(map != null, "no current map is loaded, so nothing can be blowing over it");

        List<RimWorld.GameCondition> active = map!.gameConditionManager.ActiveConditions;
        for (int i = active.Count - 1; i >= 0; i--) {
            if (active[i] is Highstorm storm) storm.End();
        }

        CosmereLookup.AssertThat(
            ctx,
            map.gameConditionManager.GetActiveCondition<Highstorm>() == null,
            "no highstorm should be blowing when a scenario starts",
            () => $"still running: {string.Join(", ", active.Select(c => c.def.defName))}");
    }

    /// <summary>Puts a pawn on open ground, with nothing roofed around them, and drops a plasteel
    /// windbreak two cells east so the storm leaves them there.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    /// <remarks><c>Highstorm.MoveItem</c> skips anything with a solid thing within two cells east,
    /// and changes nothing else: the pawn still takes damage and still drinks Stormlight.</remarks>
    [Given("{string} stands in the open")]
    public void StandInTheOpen(PickleContext ctx, string nickname) {
        Pawn pawn = RequireSpawnedPawn(ctx, nickname);
        Map map = pawn.Map;

        IntVec3 cell = FindCell(pawn, c => IsClearOpenGround(c, map) && IsClearOpenGround(Windbreak(c), map));
        ctx.Require(
            cell.IsValid,
            $"no open, unroofed cell within {SearchRadius:0} of '{nickname}' had room for a " +
            $"windbreak two cells east. {DescribeSpot(pawn)}");

        SpawnWall(Windbreak(cell), map);
        Teleport(pawn, cell);

        CosmereLookup.AssertThat(
            ctx,
            !pawn.Position.Roofed(map) && !StormShelterManager.IsInsideShelter(pawn.Position) &&
            pawn.IsBehindSolidThing(IntVec3.East, 2),
            $"'{nickname}' should be standing under open sky, pinned against the storm's push",
            () => DescribeSpot(pawn));
    }

    /// <summary>Roofs a three by three room and walls its east face, which is the direction a
    /// highstorm blows from, then stands the pawn in the middle of it.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    [Given("{string} shelters from the storm")]
    public void ShelterFromTheStorm(PickleContext ctx, string nickname) {
        Pawn pawn = RequireSpawnedPawn(ctx, nickname);
        Map map = pawn.Map;

        IntVec3 cell = FindCell(pawn, c => CanHoldShelter(c, map));
        ctx.Require(
            cell.IsValid,
            $"no cell within {SearchRadius:0} of '{nickname}' could take a shelter. {DescribeSpot(pawn)}");

        for (int z = -1; z <= 1; z++) {
            SpawnWall(new IntVec3(cell.x + 2, 0, cell.z + z), map);
        }

        foreach (IntVec3 roofed in Room(cell)) {
            map.roofGrid.SetRoof(roofed, RimWorld.RoofDefOf.RoofConstructed);
        }

        Teleport(pawn, cell);
        StormShelterManager.RebuildShelterCache(map);

        CosmereLookup.AssertThat(
            ctx,
            StormShelterManager.IsInsideShelter(pawn.Position),
            $"'{nickname}' should be inside a storm shelter",
            () => DescribeSpot(pawn));
    }

    /// <summary>Asserts a pawn is somewhere the storm cannot reach.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    [Then("{string} is sheltered from the storm")]
    public void AssertSheltered(PickleContext ctx, string nickname) {
        Pawn pawn = RequireSpawnedPawn(ctx, nickname);

        CosmereLookup.AssertThat(
            ctx,
            !pawn.ShouldBeMovedByStorm(),
            $"the storm should not reach '{nickname}'",
            () => DescribeSpot(pawn));
    }

    /// <summary>Asserts the storm can reach a pawn where they stand.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    [Then("{string} is exposed to the storm")]
    public void AssertExposed(PickleContext ctx, string nickname) {
        Pawn pawn = RequireSpawnedPawn(ctx, nickname);

        CosmereLookup.AssertThat(
            ctx,
            pawn.ShouldBeMovedByStorm(),
            $"the storm should reach '{nickname}'",
            () => DescribeSpot(pawn));
    }

    /// <summary>Remembers how hurt a pawn is, so a later step can say which way it went.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    [When("I record the health of {string}")]
    public void RecordHealth(PickleContext ctx, string nickname) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        ctx.Set(new HealthReading(pawn));
    }

    /// <summary>Asserts the recorded pawn has taken damage since it was recorded.</summary>
    /// <param name="ctx">The scenario's context.</param>
    [Then("the recorded health has fallen")]
    public void AssertHealthFell(PickleContext ctx) {
        AssertHealthMoved(ctx, (before, now) => now < before - RateTolerance, "should have fallen");
    }

    /// <summary>Asserts the recorded pawn has taken no damage since it was recorded.</summary>
    /// <param name="ctx">The scenario's context.</param>
    [Then("the recorded health is unchanged")]
    public void AssertHealthUnchanged(PickleContext ctx) {
        AssertHealthMoved(ctx, (before, now) => Mathf.Abs(now - before) <= RateTolerance, "should be unchanged");
    }

    /// <summary>Beats a pawn down without killing them, which is how the capstone's survival
    /// objective is made to fail.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    [When("{string} is beaten down")]
    public void BeatDown(PickleContext ctx, string nickname) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        HealthUtility.DamageUntilDowned(pawn, false);

        CosmereLookup.AssertThat(
            ctx,
            pawn.Downed && !pawn.Dead,
            $"'{nickname}' should be downed but alive",
            () => $"downed={pawn.Downed}, dead={pawn.Dead}, health={HealthOf(pawn):P0}");
    }

    /// <summary>Offers a pawn-scoped capstone, the same call the calling letter makes when the
    /// player accepts it.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="questDefName">The CosmereQuestDef to offer.</param>
    /// <param name="nickname">The pawn the capstone is about.</param>
    [When("the capstone {string} is offered to {string}")]
    public void OfferCapstone(PickleContext ctx, string questDefName, string nickname) {
        Pawn pawn = RequireSpawnedPawn(ctx, nickname);
        CosmereQuestDef def = CosmereLookup.RequireDef<CosmereQuestDef>(questDefName);
        CosmereQuestManager manager = RequireQuestManager(ctx);

        bool started = manager.TryStartCapstone(def, pawn.Map, pawn);

        ctx.Require(
            started,
            $"the game refused to offer '{questDefName}' to '{nickname}'. a capstone only starts from " +
            $"NotFired, and this one is {manager.BuildWorldState(pawn).StateOf(def.defName)}");
    }

    /// <summary>Accepts an offered capstone, which enables its first stage's objective.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn accepting.</param>
    /// <param name="questDefName">The CosmereQuestDef to accept.</param>
    [When("{string} accepts the capstone {string}")]
    public void AcceptCapstone(PickleContext ctx, string nickname, string questDefName) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        RimWorld.Quest quest = RequireQuest(ctx, questDefName);

        ctx.Require(
            quest.State == QuestState.NotYetAccepted,
            $"'{questDefName}' is {quest.State}, so it cannot be accepted now");

        quest.Accept(pawn);

        CosmereLookup.AssertThat(
            ctx,
            quest.State == QuestState.Ongoing,
            $"'{questDefName}' should be running once '{nickname}' accepts it",
            () => DescribeQuest(quest));
    }

    /// <summary>Asserts where a capstone's quest has got to.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="questDefName">The CosmereQuestDef to read.</param>
    /// <param name="state">A QuestState name: NotYetAccepted, Ongoing, EndedSuccess, EndedFailed.</param>
    [Then("the capstone {string} is {word}")]
    public void AssertCapstoneState(PickleContext ctx, string questDefName, string state) {
        RimWorld.Quest quest = RequireQuest(ctx, questDefName);
        QuestState wanted = ParseQuestState(state);

        CosmereLookup.AssertThat(
            ctx,
            quest.State == wanted,
            $"'{questDefName}' should be {wanted}",
            () => DescribeQuest(quest));
    }

    /// <summary>Runs the clock until a capstone's quest leaves Ongoing, or the budget is spent.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="maxTicks">How many game ticks to spend waiting.</param>
    /// <param name="questDefName">The CosmereQuestDef to wait on.</param>
    /// <returns>A task that completes when the step finishes. A failed assertion faults it.</returns>
    [When("I wait up to {int} ticks for the capstone {string} to settle", TimeoutSeconds = 120f)]
    public async Task WaitForCapstone(PickleContext ctx, int maxTicks, string questDefName) {
        ctx.Require(maxTicks > 0, $"a wait needs a positive tick budget; got {maxTicks} ticks");
        RimWorld.Quest quest = RequireQuest(ctx, questDefName);

        int deadline = Find.TickManager.TicksGame + maxTicks;
        while (quest.State == QuestState.Ongoing && Find.TickManager.TicksGame < deadline) {
            await Advance(ctx, () => quest.State != QuestState.Ongoing, deadline);
        }

        CosmereLookup.AssertThat(
            ctx,
            quest.State != QuestState.Ongoing,
            $"'{questDefName}' should have settled within {maxTicks} ticks",
            () => DescribeQuest(quest));
    }

    /// <summary>Asserts a failed capstone did not burn for this pawn, so they can be offered it again.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="questDefName">The CosmereQuestDef to read.</param>
    /// <param name="nickname">The pawn the capstone was about.</param>
    [Then("the capstone {string} is not burned for {string}")]
    public void AssertNotBurned(PickleContext ctx, string questDefName, string nickname) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        CosmereQuestDef def = CosmereLookup.RequireDef<CosmereQuestDef>(questDefName);
        QuestWorldState state = RequireQuestManager(ctx).BuildWorldState(pawn);

        CosmereLookup.AssertThat(
            ctx,
            !state.IsBurnedForPawn(def.defName, pawn.thingIDNumber),
            $"'{questDefName}' should still be open to '{nickname}'",
            () => $"it is burned for {pawn.Name?.ToStringShort}; capstone state is {state.StateOf(def.defName)}");
    }

    private static Surgebinder? FindBond(Pawn pawn, RadiantOrderDef order) {
        return pawn.genes?.GetSurgebindingGeneForOrder(order);
    }

    private static Surgebinder RequireBond(PickleContext ctx, string nickname, string orderDefName) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        RadiantOrderDef order = CosmereLookup.RequireDef<RadiantOrderDef>(orderDefName);
        Surgebinder? bond = FindBond(pawn, order);

        ctx.Require(bond != null, $"'{nickname}' carries no {orderDefName} bond. {DescribeBonds(pawn)}");

        return bond!;
    }

    private static SurgebindingAbility RequireSurge(PickleContext ctx, string nickname, string abilityDefName) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        AbilityDef def = CosmereLookup.RequireDef<AbilityDef>(abilityDefName);
        Ability? ability = pawn.abilities?.GetAbility(def);

        ctx.Require(
            ability != null,
            $"'{nickname}' does not hold the ability '{abilityDefName}'. {CosmereLookup.DescribeHeldAbilities(pawn)}. " +
            DescribeBonds(pawn));
        ctx.Require(
            ability is SurgebindingAbility,
            $"'{abilityDefName}' on '{nickname}' is a {ability!.GetType().Name}, not a surge");

        return (SurgebindingAbility)ability!;
    }

    private static RadiantTracker RequireTracker(PickleContext ctx) {
        RadiantTracker? tracker = Current.Game?.GetComponent<RadiantTracker>();
        ctx.Require(tracker != null, "no game is loaded, so there is no RadiantTracker to record against");
        return tracker!;
    }

    private static CosmereQuestManager RequireQuestManager(PickleContext ctx) {
        CosmereQuestManager? manager = Current.Game?.GetComponent<CosmereQuestManager>();
        ctx.Require(manager != null, "no game is loaded, so there is no CosmereQuestManager to ask");
        return manager!;
    }

    /// <summary>Finds the live quest a capstone def built. Matched through the reward part rather
    /// than the label, because the label is translated and the part holds the def itself.</summary>
    private static RimWorld.Quest RequireQuest(PickleContext ctx, string questDefName) {
        CosmereQuestDef def = CosmereLookup.RequireDef<CosmereQuestDef>(questDefName);
        List<RimWorld.Quest> quests = Find.QuestManager.QuestsListForReading;

        for (int i = quests.Count - 1; i >= 0; i--) {
            List<QuestPart> parts = quests[i].PartsListForReading;
            for (int j = 0; j < parts.Count; j++) {
                if (parts[j] is QuestPart_CosmereReward reward && reward.def == def) return quests[i];
            }
        }

        ctx.Require(false, $"no live quest was built from '{questDefName}'. {DescribeQuests(quests)}");
        throw new InvalidOperationException($"no live quest was built from '{questDefName}'");
    }

    private static QuestState ParseQuestState(string name) {
        if (Enum.TryParse(name, ignoreCase: true, out QuestState state)) return state;

        throw new InvalidOperationException(
            $"'{name}' is not a quest state. try one of: {string.Join(", ", Enum.GetNames(typeof(QuestState)))}");
    }

    private static float RateOf(Surgebinder bond, AbilityDef def) {
        List<DrainSource> sources = bond.Sources;
        for (int i = 0; i < sources.Count; i++) {
            if (sources[i].Def == def) return sources[i].Rate;
        }

        return 0f;
    }

    private static Pawn RequireSpawnedPawn(PickleContext ctx, string nickname) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        ctx.Require(pawn.Spawned && pawn.Map != null, $"'{nickname}' is not on a map, so they stand nowhere");
        return pawn;
    }

    private static IntVec3 FindCell(Pawn pawn, Func<IntVec3, bool> wanted) {
        int count = GenRadial.NumCellsInRadius(SearchRadius);
        for (int i = 0; i < count; i++) {
            IntVec3 cell = pawn.Position + GenRadial.RadialPattern[i];
            if (wanted(cell)) return cell;
        }

        return IntVec3.Invalid;
    }

    /// <summary>Where the windbreak goes: two east, the far edge of the range the storm's push checks.</summary>
    private static IntVec3 Windbreak(IntVec3 cell) {
        return cell + (IntVec3.East * 2);
    }

    /// <summary>Plasteel, because the storm's damage table gives it a multiplier of zero.</summary>
    private static void SpawnWall(IntVec3 cell, Map map) {
        Thing wall = ThingMaker.MakeThing(RimWorld.ThingDefOf.Wall, RimWorld.ThingDefOf.Plasteel);
        GenSpawn.Spawn(wall, cell, map, WipeMode.Vanish);
    }

    private static bool IsClearOpenGround(IntVec3 cell, Map map) {
        if (!cell.InBounds(map) || !cell.Standable(map)) return false;
        if (cell.Fogged(map) || cell.Roofed(map)) return false;

        return !StormShelterManager.IsInsideShelter(cell);
    }

    private static List<IntVec3> Room(IntVec3 middle) {
        List<IntVec3> cells = [];
        for (int x = -1; x <= 1; x++) {
            for (int z = -1; z <= 1; z++) {
                cells.Add(new IntVec3(middle.x + x, 0, middle.z + z));
            }
        }

        return cells;
    }

    /// <summary>The room must flood-fill to its own nine cells and no further, so everything around
    /// it stays unroofed, and the east face has room for the wall that seals it.</summary>
    private static bool CanHoldShelter(IntVec3 middle, Map map) {
        foreach (IntVec3 cell in Room(middle)) {
            if (!IsClearOpenGround(cell, map)) return false;
        }

        for (int x = -2; x <= 2; x++) {
            for (int z = -2; z <= 2; z++) {
                IntVec3 cell = new IntVec3(middle.x + x, 0, middle.z + z);
                if (!cell.InBounds(map) || cell.Fogged(map)) return false;

                bool isWallCell = x == 2 && z is >= -1 and <= 1;
                if (isWallCell && cell.GetEdifice(map) != null) return false;
                if (!isWallCell && cell.Roofed(map)) return false;
            }
        }

        return true;
    }

    private static void Teleport(Pawn pawn, IntVec3 cell) {
        pawn.jobs?.StopAll();
        pawn.Position = cell;
        pawn.Notify_Teleported(false);
    }

    private static float HealthOf(Pawn pawn) {
        return pawn.health?.summaryHealth?.SummaryHealthPercent ?? 0f;
    }

    private static void AssertHealthMoved(PickleContext ctx, Func<float, float, bool> holds, string wanted) {
        HealthReading reading;
        try {
            reading = ctx.Get<HealthReading>();
        } catch (InvalidOperationException) {
            ctx.Require(false, "nothing was recorded; run 'I record the health of ...' first");
            return;
        }

        float now = HealthOf(reading.Pawn);
        CosmereLookup.AssertThat(
            ctx,
            holds(reading.Value, now),
            $"'{reading.Name}' health {wanted} since it was recorded",
            () => $"it went from {reading.Value:P1} to {now:P1} over " +
                $"{Find.TickManager.TicksGame - reading.Tick} ticks. {DescribeSpot(reading.Pawn)}");
    }

    private static string DescribeCaste(Pawn pawn) {
        string xenotype = pawn.genes?.Xenotype?.defName ?? "(none)";

        return $"{pawn.LabelShort} is xenotype {xenotype}: darkeyes={CasteUtility.IsDarkeyes(pawn)}, " +
            $"lighteyes={CasteUtility.IsLighteyes(pawn)}. {DescribeBonds(pawn)}";
    }

    private static string DescribeBond(Surgebinder bond) {
        string spren = bond.bondedSpren?.Name?.ToStringShort ?? bond.godsprenName;

        return $"{bond.radiantOrderDef.defName}: stored ideal {bond.CurrentIdeal}, " +
            $"displayed {bond.CurrentIdealDisplay}, stormlight {bond.Value:0.#} of {bond.Max:0.#}, " +
            $"spren {(spren.Length == 0 ? "(none)" : spren)}";
    }

    private static string DescribeBonds(Pawn pawn) {
        List<Verse.Gene> genes = pawn.genes?.GenesListForReading ?? [];
        List<string> bonds = [];
        for (int i = 0; i < genes.Count; i++) {
            if (genes[i] is Surgebinder bond) bonds.Add(DescribeBond(bond));
        }

        if (bonds.Count > 0) return "bonds held: " + string.Join("; ", bonds);

        bool honor = ShardUtility.AreAnyEnabled(ShardDefOf.Honor);
        return $"the pawn holds no Nahel bond at all (Honor enabled={honor})";
    }

    private static string DescribeSurges(Surgebinder bond) {
        List<DrainSource> sources = bond.Sources;
        if (sources.Count == 0) return $"no surge is charging this bond. {DescribeBond(bond)}";

        List<string> charging = [];
        for (int i = 0; i < sources.Count; i++) {
            charging.Add($"{sources[i].Def.defName} at {sources[i].Rate:0.###}");
        }

        return $"charging: {string.Join(", ", charging)}. {DescribeBond(bond)}";
    }

    private static string DescribeSpot(Pawn pawn) {
        if (!pawn.Spawned || pawn.Map == null) return $"{pawn.LabelShort} is not on a map";

        return $"{pawn.LabelShort} at {pawn.Position}: roofed={pawn.Position.Roofed(pawn.Map)}, " +
            $"sheltered={StormShelterManager.IsInsideShelter(pawn.Position)}, " +
            $"windbroken={pawn.IsBehindSolidThing(IntVec3.East, 2)}, " +
            $"stormWillMove={pawn.ShouldBeMovedByStorm()}, health={HealthOf(pawn):P1}, downed={pawn.Downed}";
    }

    private static string DescribeQuest(RimWorld.Quest quest) {
        return $"quest '{quest.name}' is {quest.State}, accepted={quest.EverAccepted}, " +
            $"{quest.TicksSinceAppeared} ticks since it appeared";
    }

    private static string DescribeQuests(List<RimWorld.Quest> quests) {
        if (quests.Count == 0) return "no quests are live at all";

        return "live quests: " + string.Join(", ", quests.Select(q => $"{q.name} ({q.State})"));
    }

    // Fast mode leaves the game paused and drives ticks by hand, the way the built-in wait step does.
    private static async Task Advance(PickleContext ctx, Func<bool> holds, int deadline) {
        if (PickleRunMode.Current != PickleRunMode.Mode.Fast) {
            await ctx.WaitTicks(60);
            return;
        }

        for (int i = 0; i < 60 && !holds() && Find.TickManager.TicksGame < deadline; i++) {
            Find.TickManager.DoSingleTick();
        }

        await ctx.WaitFrames(1);
    }

    /// <summary>One pawn's summary health at the moment a step recorded it.</summary>
    private sealed class HealthReading {
        public HealthReading(Pawn pawn) {
            Pawn = pawn;
            Name = pawn.Name?.ToStringShort ?? pawn.LabelShort;
            Value = HealthOf(pawn);
            Tick = Find.TickManager?.TicksGame ?? 0;
        }

        public Pawn Pawn { get; }

        public string Name { get; }

        public float Value { get; }

        public int Tick { get; }
    }
}
