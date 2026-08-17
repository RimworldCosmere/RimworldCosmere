using System;
using System.Collections.Generic;
using Cosmere.Core.Def;
using Cosmere.System.Scadrial.Feruchemy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

internal sealed class FakeMetalmindSource : IMetalmindSource {
    private float stored;

    public FakeMetalmindSource(float maxAmount, string sourceId = "fake", bool implanted = false) {
        MaxAmount = maxAmount;
        SourceId = sourceId;
        IsImplanted = implanted;
    }

    public float MaxAmount { get; }

    public string SourceId { get; }

    public bool IsImplanted { get; }

    public float StoredAmount => stored;

    public float CompoundedAmount => 0f;

    public bool CanStore => FreeSpace > 0f;

    public bool CanTap => stored > 0f;

    public bool CanTapCompounded => stored > 0f;

    public bool Equipped => true;

    public MetalDef? Metal => null;

    public bool CanStoreCompounded => false;

    public float TotalStored => stored;

    public float FreeSpace => MaxAmount - stored;

    public bool IsBurnedOut => MaxAmount <= 0f;

    public string SourceLabel => SourceId;

    // The clamp the carry exists to work around: asking for more than there is room for
    // silently loses the difference, so the caller has to be told what actually landed.
    public void AddStored(float amount) {
        stored = Math.Min(MaxAmount, stored + amount);
    }

    public void ConsumeStored(float amount) {
        stored = Math.Max(0f, stored - amount);
    }

    public void AddCompounded(float amount) { }

    public void ConsumeCompounded(float amount) { }
}

/// <summary>
///     Guards what a transfer reports back. Returning a bool threw away the size of the
///     move, which is the number the Investiture mirror is built on.
/// </summary>
[TestClass]
public class MetalmindDistributionTests {
    private static float Store(List<IMetalmindSource> sources, float amount, string target = MetalmindDistribution.TargetAll) {
        return MetalmindDistribution.Carry(
            sources,
            target,
            amount,
            static m => m.CanStore,
            static (m, a) => m.AddStored(a),
            static m => m.FreeSpace
        );
    }

    [TestMethod]
    public void AMetalmindWithLessRoomThanAskedForReportsWhatItTook() {
        FakeMetalmindSource band = new FakeMetalmindSource(3f);

        Assert.AreEqual(3f, Store([band], 8f));
        Assert.AreEqual(3f, band.StoredAmount);
    }

    [TestMethod]
    public void TheRemainderCarriesToTheNextMetalmind() {
        FakeMetalmindSource small = new FakeMetalmindSource(3f, "small");
        FakeMetalmindSource large = new FakeMetalmindSource(10f, "large");

        Assert.AreEqual(8f, Store([small, large], 8f));
        Assert.AreEqual(3f, small.StoredAmount);
        Assert.AreEqual(5f, large.StoredAmount);
    }

    [TestMethod]
    public void AFullMetalmindTakesNothing() {
        FakeMetalmindSource band = new FakeMetalmindSource(0f);

        Assert.AreEqual(0f, Store([band], 8f));
    }

    [TestMethod]
    public void NothingIsAskedForAndNothingMoves() {
        FakeMetalmindSource band = new FakeMetalmindSource(10f);

        Assert.AreEqual(0f, Store([band], 0f));
        Assert.AreEqual(0f, Store([band], -4f));
        Assert.AreEqual(0f, band.StoredAmount);
    }

    [TestMethod]
    public void TheTargetDecidesWhichMetalmindsAreReached() {
        FakeMetalmindSource worn = new FakeMetalmindSource(10f, "worn");
        FakeMetalmindSource implant = new FakeMetalmindSource(10f, "implant", true);
        List<IMetalmindSource> both = [worn, implant];

        Assert.AreEqual(4f, Store(both, 4f, MetalmindDistribution.TargetInternal));
        Assert.AreEqual(4f, implant.StoredAmount);
        Assert.AreEqual(0f, worn.StoredAmount);

        Assert.AreEqual(4f, Store(both, 4f, "worn"));
        Assert.AreEqual(4f, worn.StoredAmount);
    }

    [TestMethod]
    public void ATargetThatNoLongerResolvesMovesNothing() {
        FakeMetalmindSource band = new FakeMetalmindSource(10f, "band");

        Assert.AreEqual(0f, Store([band], 4f, "thing:burned-out"));
    }
}
