using Cosmere.System.Scadrial.Feruchemy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The name a duralumind files charge under. Shard charge carries the Shard's own defName, so a
///     tie to one Shard can never be handed back as a tie to another.
/// </summary>
[TestClass]
public class ConnectionKeyTests {
    [TestMethod]
    public void TheLedgersWithOneStoreKeepTheirPlainName() {
        Assert.AreEqual("Residence", ConnectionKey.For(DuraluminLedger.Residence, "Ruin"));
        Assert.AreEqual("Bonds", ConnectionKey.For(DuraluminLedger.Bonds, "Ruin"));
        Assert.AreEqual("Social", ConnectionKey.For(DuraluminLedger.Social, "Ruin"));
    }

    [TestMethod]
    public void TwoShardsNeverShareAKey() {
        Assert.AreEqual("Shard:Ruin", ConnectionKey.For(DuraluminLedger.Shard, "Ruin"));
        Assert.AreNotEqual(
            ConnectionKey.For(DuraluminLedger.Shard, "Ruin"),
            ConnectionKey.For(DuraluminLedger.Shard, "Preservation")
        );
    }

    [TestMethod]
    public void AShardKeyIsNeverConfusedWithAnotherLedger() {
        Assert.AreNotEqual(
            ConnectionKey.For(DuraluminLedger.Shard, "Ruin"),
            ConnectionKey.For(DuraluminLedger.Residence, "Ruin")
        );
    }

    [TestMethod]
    public void NamingNoShardFallsBackToThePlainName() {
        Assert.AreEqual("Shard", ConnectionKey.For(DuraluminLedger.Shard, null));
        Assert.AreEqual("Shard", ConnectionKey.For(DuraluminLedger.Shard, string.Empty));
    }
}
