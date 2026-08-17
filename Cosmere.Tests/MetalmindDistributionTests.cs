using System;
using System.Collections.Generic;
using Cosmere.Core.Def;
using Cosmere.System.Scadrial.Feruchemy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

internal sealed class FakeMetalmindSource : IMetalmindSource {
    private float stored;

    private readonly Dictionary<string, float> byLedger = new Dictionary<string, float>();

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
    public float AddStored(float amount, string? ledgerKey = null) {
        if (refuses) return 0f;

        float before = stored;
        stored = Math.Min(MaxAmount, stored + amount);
        float moved = stored - before;
        if (ledgerKey != null && moved != 0f) {
            byLedger.TryGetValue(ledgerKey, out float existing);
            byLedger[ledgerKey] = existing + moved;
        }

        return moved;
    }

    public float ConsumeStored(float amount, string? ledgerKey = null) {
        if (refuses) return 0f;

        float before = stored;
        stored = Math.Max(0f, stored - amount);
        float moved = before - stored;
        if (moved != 0f) {
            if (ledgerKey != null) ChargeAttribution.DrainNamed(byLedger, ledgerKey, moved);
            else ChargeAttribution.Drain(byLedger, moved);
        }

        return moved;
    }

    public float AddCompounded(float amount) {
        return 0f;
    }

    public float ConsumeCompounded(float amount, string? ledgerKey = null) {
        return 0f;
    }

    public float StoredFor(string ledgerKey) {
        return byLedger.TryGetValue(ledgerKey, out float amount) ? amount : 0f;
    }
}

/// <summary>
///     Guards what a transfer reports back. Returning a bool threw away the size of the
///     move, which is the number the Investiture mirror is built on.
/// </summary>
[TestClass]
public class MetalmindDistributionTests {
    private const float Tolerance = 1e-3f;

    private static float Store(List<IMetalmindSource> sources, float amount, string target = MetalmindDistribution.TargetAll) {
        return MetalmindDistribution.Carry(
            sources,
            target,
            null,
            amount,
            static (m, _) => m.CanStore,
            static (m, k, a) => m.AddStored(a, k),
            static (m, _) => m.FreeSpace
        );
    }

    private static float StoreFor(
        List<IMetalmindSource> sources,
        string ledgerKey,
        float amount,
        string target = MetalmindDistribution.TargetAll
    ) {
        return MetalmindDistribution.Carry(
            sources,
            target,
            ledgerKey,
            amount,
            static (m, _) => m.CanStore,
            static (m, k, a) => m.AddStored(a, k),
            static (m, _) => m.FreeSpace
        );
    }

    /// <summary>
    ///     A settlement correction must come out of the ledger that was overpaid, even when the
    ///     transfer landed on a different metalmind than the one holding another ledger's charge.
    /// </summary>
    [TestMethod]
    public void AReclaimNeverFallsThroughOntoAnotherLedgersCharge() {
        FakeMetalmindSource full = new FakeMetalmindSource(20f, "full");
        FakeMetalmindSource empty = new FakeMetalmindSource(20f, "empty");
        Assert.AreEqual(20f, StoreFor([full], "Shard:Ruin", 20f), Tolerance);

        Assert.AreEqual(18f, StoreFor([full, empty], "Residence", 18f), Tolerance);
        Assert.AreEqual(5f, MetalmindDistribution.Reclaim([full, empty], MetalmindDistribution.TargetAll, "Residence", 5f), Tolerance);

        Assert.AreEqual(20f, full.StoredFor("Shard:Ruin"), Tolerance, "the correction robbed a Shard tie to pay a residence one.");
        Assert.AreEqual(13f, empty.StoredFor("Residence"), Tolerance);
    }

    // A reclaim can never take more than the key is recorded as holding, whatever the metalmind holds.
    [TestMethod]
    public void AReclaimIsBoundedByWhatTheKeyIsRecordedAsHolding() {
        FakeMetalmindSource band = new FakeMetalmindSource(50f);
        StoreFor([band], "Shard:Ruin", 30f);
        StoreFor([band], "Residence", 10f);

        Assert.AreEqual(10f, MetalmindDistribution.Reclaim([band], MetalmindDistribution.TargetAll, "Residence", 40f), Tolerance);
        Assert.AreEqual(0f, band.StoredFor("Residence"), Tolerance);
        Assert.AreEqual(30f, band.StoredFor("Shard:Ruin"), Tolerance);
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
