using System;
using System.Collections.Generic;
using System.Linq;
using Cosmere.Core.Gene;
using Cosmere.Core.Investiture;
using Cosmere.Pickle.Lookup;
using RimWorks.Pickle;
using Verse;
using InvestitureHolder = Cosmere.Core.Comp.Thing.InvestitureHolder;

namespace Cosmere.Pickle.Steps;

/// <summary>Investiture held on a pawn or a thing: setting it, reading it back, and watching it
/// move. Shard agnostic, because every shardworld spends off the same comp.</summary>
[PickleSteps]
public class InvestitureSteps {
    /// <summary>Sets the Investiture a pawn holds, failing when the pawn's cap refuses the value.</summary>
    /// <param name="ctx">The scenario's context, for assertions, requirements, and waits.</param>
    /// <param name="nickname">The pawn's nickname.</param>
    /// <param name="value">The Investiture to set.</param>
    [Given("{string} investiture is set to {float}")]
    public void SetPawnInvestiture(PickleContext ctx, string nickname, float value) {
        SetInvestiture(ctx, RequirePawnHolder(ctx, nickname), DescribePawn(nickname), value);
    }

    /// <summary>Asserts the Investiture a pawn holds, within a tolerance.</summary>
    /// <param name="ctx">The scenario's context, for assertions, requirements, and waits.</param>
    /// <param name="nickname">The pawn's nickname.</param>
    /// <param name="expected">The Investiture expected.</param>
    [Then("{string} investiture is {float}")]
    public void AssertPawnInvestiture(PickleContext ctx, string nickname, float expected) {
        InvestitureHolder holder = RequirePawnHolder(ctx, nickname);
        AssertBound(
            ctx,
            holder,
            DescribePawn(nickname),
            CosmereLookup.IsNear(holder.currentInvestitureSelf, expected),
            $"should be {expected:0.###}");
    }

    /// <summary>Asserts a pawn's Investiture is above a bound.</summary>
    /// <param name="ctx">The scenario's context, for assertions, requirements, and waits.</param>
    /// <param name="nickname">The pawn's nickname.</param>
    /// <param name="bound">The lower bound, exclusive.</param>
    [Then("{string} investiture is above {float}")]
    public void AssertPawnInvestitureAbove(PickleContext ctx, string nickname, float bound) {
        InvestitureHolder holder = RequirePawnHolder(ctx, nickname);
        AssertBound(
            ctx,
            holder,
            DescribePawn(nickname),
            holder.currentInvestitureSelf > bound,
            $"should be above {bound:0.###}");
    }

    /// <summary>Asserts a pawn's Investiture is below a bound.</summary>
    /// <param name="ctx">The scenario's context, for assertions, requirements, and waits.</param>
    /// <param name="nickname">The pawn's nickname.</param>
    /// <param name="bound">The upper bound, exclusive.</param>
    [Then("{string} investiture is below {float}")]
    public void AssertPawnInvestitureBelow(PickleContext ctx, string nickname, float bound) {
        InvestitureHolder holder = RequirePawnHolder(ctx, nickname);
        AssertBound(
            ctx,
            holder,
            DescribePawn(nickname),
            holder.currentInvestitureSelf < bound,
            $"should be below {bound:0.###}");
    }

    /// <summary>Asserts how fast a pawn loses Investiture, within a tolerance.</summary>
    /// <param name="ctx">The scenario's context, for assertions, requirements, and waits.</param>
    /// <param name="nickname">The pawn's nickname.</param>
    /// <param name="expected">The loss per second expected.</param>
    [Then("{string} drains {float} investiture per second")]
    public void AssertPawnDrainRate(PickleContext ctx, string nickname, float expected) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        InvestitureHolder holder = RequireHolder(ctx, pawn, DescribePawn(nickname));

        // A Mistborn carries one Allomancer gene per metal, so the first one is not the total.
        List<Invested> genes = [];
        List<Gene>? all = pawn.genes?.GenesListForReading;
        for (int i = 0; all != null && i < all.Count; i++) {
            if (all[i] is Invested invested) genes.Add(invested);
        }

        float actual = 0f;
        for (int i = 0; i < genes.Count; i++) {
            actual += genes[i].DrainPerSecond;
        }

        if (genes.Count == 0) actual = PassiveDrainPerSecond(holder);

        CosmereLookup.AssertThat(
            ctx,
            CosmereLookup.IsNear(actual, expected),
            $"{DescribePawn(nickname)} should drain {expected:0.####} investiture per second",
            () => $"it drains {actual:0.####}/sec, summed over " +
                $"{(genes.Count == 0 ? "the holder's own decay" : $"{genes.Count} invested gene(s): " + string.Join(", ", genes.Select(g => g.def.defName)))}. {CosmereLookup.DescribeHolder(holder)}");
    }

    /// <summary>Sets the Investiture a thing holds, failing when the thing's cap refuses the value.</summary>
    /// <param name="ctx">The scenario's context, for assertions, requirements, and waits.</param>
    /// <param name="defName">The thing def in the cell.</param>
    /// <param name="x">The cell's x coordinate.</param>
    /// <param name="z">The cell's z coordinate.</param>
    /// <param name="value">The Investiture to set.</param>
    [Given("the {string} at \\({int}, {int}\\) investiture is set to {float}")]
    public void SetThingInvestiture(PickleContext ctx, string defName, int x, int z, float value) {
        SetInvestiture(ctx, CosmereLookup.RequireThingHolder(ctx, defName, x, z), DescribeThing(defName, x, z), value);
    }

    /// <summary>Asserts the Investiture a thing holds, within a tolerance.</summary>
    /// <param name="ctx">The scenario's context, for assertions, requirements, and waits.</param>
    /// <param name="defName">The thing def in the cell.</param>
    /// <param name="x">The cell's x coordinate.</param>
    /// <param name="z">The cell's z coordinate.</param>
    /// <param name="expected">The Investiture expected.</param>
    [Then("the {string} at \\({int}, {int}\\) investiture is {float}")]
    public void AssertThingInvestiture(PickleContext ctx, string defName, int x, int z, float expected) {
        InvestitureHolder holder = CosmereLookup.RequireThingHolder(ctx, defName, x, z);
        AssertBound(
            ctx,
            holder,
            DescribeThing(defName, x, z),
            CosmereLookup.IsNear(holder.currentInvestitureSelf, expected),
            $"should be {expected:0.###}");
    }

    /// <summary>Asserts a thing's Investiture is above a bound.</summary>
    /// <param name="ctx">The scenario's context, for assertions, requirements, and waits.</param>
    /// <param name="defName">The thing def in the cell.</param>
    /// <param name="x">The cell's x coordinate.</param>
    /// <param name="z">The cell's z coordinate.</param>
    /// <param name="bound">The lower bound, exclusive.</param>
    [Then("the {string} at \\({int}, {int}\\) investiture is above {float}")]
    public void AssertThingInvestitureAbove(PickleContext ctx, string defName, int x, int z, float bound) {
        InvestitureHolder holder = CosmereLookup.RequireThingHolder(ctx, defName, x, z);
        AssertBound(
            ctx,
            holder,
            DescribeThing(defName, x, z),
            holder.currentInvestitureSelf > bound,
            $"should be above {bound:0.###}");
    }

    /// <summary>Asserts a thing's Investiture is below a bound.</summary>
    /// <param name="ctx">The scenario's context, for assertions, requirements, and waits.</param>
    /// <param name="defName">The thing def in the cell.</param>
    /// <param name="x">The cell's x coordinate.</param>
    /// <param name="z">The cell's z coordinate.</param>
    /// <param name="bound">The upper bound, exclusive.</param>
    [Then("the {string} at \\({int}, {int}\\) investiture is below {float}")]
    public void AssertThingInvestitureBelow(PickleContext ctx, string defName, int x, int z, float bound) {
        InvestitureHolder holder = CosmereLookup.RequireThingHolder(ctx, defName, x, z);
        AssertBound(
            ctx,
            holder,
            DescribeThing(defName, x, z),
            holder.currentInvestitureSelf < bound,
            $"should be below {bound:0.###}");
    }

    /// <summary>Remembers a pawn's Investiture so a later step can say which way it moved.</summary>
    /// <param name="ctx">The scenario's context, for assertions, requirements, and waits.</param>
    /// <param name="nickname">The pawn's nickname.</param>
    [When("I record the investiture of {string}")]
    public void RecordPawnInvestiture(PickleContext ctx, string nickname) {
        Record(ctx, RequirePawnHolder(ctx, nickname), DescribePawn(nickname));
    }

    /// <summary>Remembers a thing's Investiture so a later step can say which way it moved.</summary>
    /// <param name="ctx">The scenario's context, for assertions, requirements, and waits.</param>
    /// <param name="defName">The thing def in the cell.</param>
    /// <param name="x">The cell's x coordinate.</param>
    /// <param name="z">The cell's z coordinate.</param>
    [When("I record the investiture of the {string} at \\({int}, {int}\\)")]
    public void RecordThingInvestiture(PickleContext ctx, string defName, int x, int z) {
        Record(ctx, CosmereLookup.RequireThingHolder(ctx, defName, x, z), DescribeThing(defName, x, z));
    }

    /// <summary>Asserts the recorded holder has lost Investiture since it was recorded.</summary>
    /// <param name="ctx">The scenario's context, for assertions, requirements, and waits.</param>
    [Then("the recorded investiture has fallen")]
    public void AssertRecordedFell(PickleContext ctx) {
        AssertMoved(ctx, (before, now) => now < before - CosmereLookup.Tolerance(before), "should have fallen");
    }

    /// <summary>Asserts the recorded holder has gained Investiture since it was recorded.</summary>
    /// <param name="ctx">The scenario's context, for assertions, requirements, and waits.</param>
    [Then("the recorded investiture has risen")]
    public void AssertRecordedRose(PickleContext ctx) {
        AssertMoved(ctx, (before, now) => now > before + CosmereLookup.Tolerance(before), "should have risen");
    }

    /// <summary>Asserts the recorded holder's Investiture has not moved.</summary>
    /// <param name="ctx">The scenario's context, for assertions, requirements, and waits.</param>
    [Then("the recorded investiture is unchanged")]
    public void AssertRecordedUnchanged(PickleContext ctx) {
        AssertMoved(ctx, CosmereLookup.IsNear, "should be unchanged");
    }

    private static void Record(PickleContext ctx, InvestitureHolder holder, string described) {
        ctx.Set(new Reading(holder, described));
    }

    private static void AssertMoved(PickleContext ctx, Func<float, float, bool> holds, string wanted) {
        Reading reading;
        try {
            reading = ctx.Get<Reading>();
        } catch (InvalidOperationException) {
            ctx.Require(false, "nothing was recorded; run 'I record the investiture of ...' first");
            return;
        }

        float now = reading.Holder.currentInvestitureSelf;
        CosmereLookup.AssertThat(
            ctx,
            holds(reading.Value, now),
            $"{reading.Described} investiture {wanted} since it was recorded",
            () => $"it went from {reading.Value:0.###} to {now:0.###} over " +
                $"{Find.TickManager.TicksGame - reading.Tick} ticks. {CosmereLookup.DescribeHolder(reading.Holder)}");
    }

    private static void SetInvestiture(PickleContext ctx, InvestitureHolder holder, string described, float value) {
        holder.currentInvestitureSelf = value;

        ctx.Require(
            CosmereLookup.IsNear(holder.currentInvestitureSelf, value),
            $"{described} would not hold {value:0.###} investiture; it clamped to " +
            $"{holder.currentInvestitureSelf:0.###}. {CosmereLookup.DescribeHolder(holder)}");
    }

    private static void AssertBound(
        PickleContext ctx,
        InvestitureHolder holder,
        string described,
        bool condition,
        string wanted
    ) {
        CosmereLookup.AssertThat(
            ctx,
            condition,
            $"{described} investiture {wanted}",
            () => $"it is {holder.currentInvestitureSelf:0.###}. {CosmereLookup.DescribeHolder(holder)}");
    }

    private static InvestitureHolder RequirePawnHolder(PickleContext ctx, string nickname) {
        return RequireHolder(ctx, CosmereLookup.RequirePawn(ctx, nickname), DescribePawn(nickname));
    }

    private static InvestitureHolder RequireHolder(PickleContext ctx, Verse.Thing thing, string described) {
        InvestitureHolder? holder = thing.TryGetComp<InvestitureHolder>();
        ctx.Require(
            holder != null,
            $"{described} has no InvestitureHolder comp, so it cannot hold investiture at all");

        return holder!;
    }

    /// <summary>The holder's own decay as a per-second figure. It spends once a rare interval,
    /// the same rescale <see cref="Invested.DrainPerSecond"/> applies to it.</summary>
    private static float PassiveDrainPerSecond(InvestitureHolder holder) {
        return holder.drainRate * UpkeepRate.TicksPerSecond / UpkeepRate.TicksPerRareInterval;
    }

    private static string DescribePawn(string nickname) {
        return $"pawn '{nickname}'";
    }

    private static string DescribeThing(string defName, int x, int z) {
        return $"the {defName} at ({x}, {z})";
    }

    /// <summary>One holder's Investiture at the moment a step recorded it.</summary>
    private sealed class Reading {
        public Reading(InvestitureHolder holder, string described) {
            Holder = holder;
            Described = described;
            Value = holder.currentInvestitureSelf;
            Tick = Find.TickManager?.TicksGame ?? 0;
        }

        public InvestitureHolder Holder { get; }

        public string Described { get; }

        public float Value { get; }

        public int Tick { get; }
    }
}
