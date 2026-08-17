using System;
using System.Collections.Generic;
using Cosmere.Core.Def;
using Cosmere.System.Scadrial.Feruchemy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

internal sealed class FakeMetalmindSource : IMetalmindSource {
    private float stored;

    private readonly bool refuses;

    public FakeMetalmindSource(
        float maxAmount,
        string sourceId = "fake",
        bool implanted = false,
        bool refusesTransfers = false
    ) {
        MaxAmount = maxAmount;
        SourceId = sourceId;
        IsImplanted = implanted;
        refuses = refusesTransfers;
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
    public float AddStored(float amount, DuraluminLedger? ledger = null) {
        if (refuses) return 0f;

        float before = stored;
        stored = Math.Min(MaxAmount, stored + amount);

        return stored - before;
    }

    public float ConsumeStored(float amount, DuraluminLedger? ledger = null) {
        if (refuses) return 0f;

        float before = stored;
        stored = Math.Max(0f, stored - amount);

        return before - stored;
    }

    public float AddCompounded(float amount) {
        return 0f;
    }

    public float ConsumeCompounded(float amount, DuraluminLedger? ledger = null) {
        return 0f;
    }

    public float StoredFor(DuraluminLedger ledger) {
        return 0f;
    }
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

    // A metalmind can refuse a transfer it looked able to take - Metalmind.AddStored bails on
    // ValidateOwner, so a band another pawn owns has room and still moves nothing.
    [TestMethod]
    public void AMetalmindThatRefusesTheTransferReportsNothingMoved() {
        FakeMetalmindSource foreign = new FakeMetalmindSource(10f, "foreign", refusesTransfers: true);

        Assert.AreEqual(0f, Store([foreign], 8f));
        Assert.AreEqual(0f, foreign.StoredAmount);
    }

    // And it must not eat the remainder on its way past, or the metalmind behind it never
    // sees the charge the refusing one declined.
    [TestMethod]
    public void ARefusingMetalmindDoesNotSwallowTheRemainder() {
        FakeMetalmindSource foreign = new FakeMetalmindSource(10f, "foreign", refusesTransfers: true);
        FakeMetalmindSource own = new FakeMetalmindSource(10f, "own");

        Assert.AreEqual(8f, Store([foreign, own], 8f));
        Assert.AreEqual(0f, foreign.StoredAmount);
        Assert.AreEqual(8f, own.StoredAmount);
    }
}
