using System;
using System.Threading.Tasks;
using Cosmere.Pickle.Lookup;
using RimWorks.Pickle;
using RimWorks.Pickle.Runtime;
using RimWorld;
using Verse;
using Highstorm = Cosmere.System.Roshar.GameCondition.Highstorm;

namespace Cosmere.Pickle.Steps;

/// <summary>Game conditions on the current map: the highstorm, and every other Cosmere
/// effect that blankets a map for a while.</summary>
[PickleSteps]
public class GameConditionSteps {
    private const int TicksPerYield = 60;

    private const float WaitTimeoutSeconds = 120f;

    /// <summary>Starts a condition on the current map for a number of ticks.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="defName">The condition def to start.</param>
    /// <param name="ticks">How long the condition runs.</param>
    /// <remarks>The two calls <c>IncidentWorker_Highstorm</c> makes. The incident also sends a
    /// letter and forces the weather, so fire it when a scenario needs those too.</remarks>
    [When("game condition {string} starts for {int} ticks")]
    public void StartCondition(PickleContext ctx, string defName, int ticks) {
        Map map = RequireMap(ctx);
        GameConditionDef def = CosmereLookup.RequireDef<GameConditionDef>(defName);
        ctx.Require(ticks > 0, $"a condition needs a positive duration; got {ticks} ticks");

        RimWorld.GameCondition condition = GameConditionMaker.MakeCondition(def, ticks);
        map.gameConditionManager.RegisterCondition(condition);
    }

    /// <summary>Ends a running condition before its duration is up.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="defName">The condition def to end.</param>
    [When("game condition {string} ends")]
    public void EndCondition(PickleContext ctx, string defName) {
        Map map = RequireMap(ctx);
        GameConditionDef def = CosmereLookup.RequireDef<GameConditionDef>(defName);

        RimWorld.GameCondition condition = map.gameConditionManager.GetActiveCondition(def);
        ctx.Require(
            condition != null,
            $"game condition '{defName}' is not running, so it cannot end; {DescribeActive(map)}");

        condition!.End();
    }

    /// <summary>Asserts a condition is running on the current map.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="defName">The condition def expected.</param>
    [Then("game condition {string} is active")]
    public void AssertActive(PickleContext ctx, string defName) {
        Map map = RequireMap(ctx);
        GameConditionDef def = CosmereLookup.RequireDef<GameConditionDef>(defName);

        CosmereLookup.AssertThat(
            ctx,
            map.gameConditionManager.ConditionIsActive(def),
            $"game condition '{defName}' should be running",
            () => DescribeActive(map));
    }

    /// <summary>Asserts a condition is over, or never started.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="defName">The condition def that should not be running.</param>
    [Then("game condition {string} is not active")]
    public void AssertNotActive(PickleContext ctx, string defName) {
        Map map = RequireMap(ctx);
        GameConditionDef def = CosmereLookup.RequireDef<GameConditionDef>(defName);

        CosmereLookup.AssertThat(
            ctx,
            !map.gameConditionManager.ConditionIsActive(def),
            $"game condition '{defName}' should be over",
            () => DescribeCondition(map.gameConditionManager.GetActiveCondition(def)));
    }

    /// <summary>Runs the clock until a condition ends, or the tick budget is spent.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="maxTicks">How many game ticks to spend waiting.</param>
    /// <param name="defName">The condition def to wait on.</param>
    /// <returns>A task that completes when the step finishes. A failed assertion faults it.</returns>
    [When("I wait up to {int} ticks for game condition {string} to end", TimeoutSeconds = WaitTimeoutSeconds)]
    public async Task WaitForEnd(PickleContext ctx, int maxTicks, string defName) {
        Map map = RequireMap(ctx);
        GameConditionDef def = CosmereLookup.RequireDef<GameConditionDef>(defName);

        bool ended = await AdvanceUntil(ctx, () => !map.gameConditionManager.ConditionIsActive(def), maxTicks);

        CosmereLookup.AssertThat(
            ctx,
            ended,
            $"game condition '{defName}' should have ended within {maxTicks} ticks",
            () => DescribeCondition(map.gameConditionManager.GetActiveCondition(def)));
    }

    /// <summary>Runs the clock until the highstorm's intensity passes a threshold.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="maxTicks">How many game ticks to spend waiting.</param>
    /// <param name="intensity">The intensity the storm must reach.</param>
    /// <returns>A task that completes when the step finishes. A failed assertion faults it.</returns>
    /// <remarks>Intensity climbs and falls across a storm's life and only moves every 30 ticks, so
    /// the dangerous part of one cannot be asserted on the tick after it starts.</remarks>
    [When("I wait up to {int} ticks for the highstorm to reach intensity {float}", TimeoutSeconds = WaitTimeoutSeconds)]
    public async Task WaitForIntensity(PickleContext ctx, int maxTicks, float intensity) {
        Map map = RequireMap(ctx);
        Highstorm storm = RequireHighstorm(ctx, map);

        bool reached = await AdvanceUntil(ctx, () => storm.CurrentIntensity >= intensity, maxTicks);

        CosmereLookup.AssertThat(
            ctx,
            reached,
            $"the highstorm should have reached intensity {intensity:F2} within {maxTicks} ticks",
            () => DescribeStorm(map, storm));
    }

    /// <summary>Asserts the highstorm is in the part of its life that drives pawns to shelter.</summary>
    /// <param name="ctx">The scenario's context.</param>
    [Then("the highstorm is dangerous")]
    public void AssertDangerous(PickleContext ctx) {
        Map map = RequireMap(ctx);
        Highstorm storm = RequireHighstorm(ctx, map);

        CosmereLookup.AssertThat(
            ctx,
            storm.IsDangerousPhase,
            "the highstorm should be in its dangerous phase",
            () => DescribeStorm(map, storm));
    }

    private static Map RequireMap(PickleContext ctx) {
        Map? map = Find.CurrentMap;
        ctx.Require(map != null, "no current map is loaded; a game condition needs one");
        return map!;
    }

    private static Highstorm RequireHighstorm(PickleContext ctx, Map map) {
        Highstorm? storm = map.gameConditionManager.GetActiveCondition<Highstorm>();
        ctx.Require(storm != null, $"no highstorm is running; {DescribeActive(map)}");
        return storm!;
    }

    // Fast mode leaves the game paused and drives ticks by hand, the way the built-in wait step does.
    private static async Task<bool> AdvanceUntil(PickleContext ctx, Func<bool> holds, int maxTicks) {
        ctx.Require(maxTicks > 0, $"a wait needs a positive tick budget; got {maxTicks} ticks");
        int deadline = Find.TickManager.TicksGame + maxTicks;

        while (!holds() && Find.TickManager.TicksGame < deadline) {
            if (PickleRunMode.Current != PickleRunMode.Mode.Fast) {
                await ctx.WaitTicks(TicksPerYield);
                continue;
            }

            for (int i = 0; i < TicksPerYield && !holds() && Find.TickManager.TicksGame < deadline; i++) {
                Find.TickManager.DoSingleTick();
            }

            await ctx.WaitFrames(1);
        }

        return holds();
    }

    private static string DescribeActive(Map map) {
        List<RimWorld.GameCondition> conditions = map.gameConditionManager.ActiveConditions;
        if (conditions.Count == 0) {
            return "no conditions are running on this map";
        }

        return "running: " + string.Join(", ", conditions.Select(DescribeCondition));
    }

    private static string DescribeCondition(RimWorld.GameCondition? condition) {
        if (condition == null) {
            return "(not running)";
        }

        string life = condition.Permanent
            ? "permanent"
            : $"{condition.TicksPassed} of {condition.Duration} ticks, {condition.TicksLeft} left";

        return $"{condition.def.defName} ({life})";
    }

    private static string DescribeStorm(Map map, Highstorm storm) {
        string running = map.gameConditionManager.ActiveConditions.Contains(storm)
            ? DescribeCondition(storm)
            : $"{DescribeCondition(storm)}, already over";

        return $"intensity {storm.CurrentIntensity:F2}, dangerous={storm.IsDangerousPhase}, {running}";
    }
}
