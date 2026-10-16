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
        Assert.AreEqual("Residence", ConnectionKey.For(DuraluminLedger.Residence, "Ruin").Name);
        Assert.AreEqual("Social", ConnectionKey.For(DuraluminLedger.Social, "Ruin").Name);
    }

    [TestMethod]
    public void TwoShardsNeverShareAKey() {
        Assert.AreEqual("Shard:Ruin", ConnectionKey.For(DuraluminLedger.Shard, "Ruin").Name);
        Assert.AreNotEqual(
            ConnectionKey.For(DuraluminLedger.Shard, "Ruin").Name,
            ConnectionKey.For(DuraluminLedger.Shard, "Preservation").Name
        );
    }

    [TestMethod]
    public void AShardKeyIsNeverConfusedWithAnotherLedger() {
        Assert.AreNotEqual(
            ConnectionKey.For(DuraluminLedger.Shard, "Ruin").Name,
            ConnectionKey.For(DuraluminLedger.Residence, "Ruin").Name
        );
    }

    // A key nobody built names nothing, so it can never collide with one that does.
    [TestMethod]
    public void ADefaultKeyNamesNothing() {
        Assert.AreEqual(string.Empty, default(ConnectionKey).Name);
    }

    [TestMethod]
    public void NamingNoShardFallsBackToThePlainName() {
        Assert.AreEqual("Shard", ConnectionKey.For(DuraluminLedger.Shard, null).Name);
        Assert.AreEqual("Shard", ConnectionKey.For(DuraluminLedger.Shard, string.Empty).Name);
    }
}
