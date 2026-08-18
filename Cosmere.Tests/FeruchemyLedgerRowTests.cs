using Cosmere.Core.UI.Dock;
using Cosmere.System.Scadrial.Feruchemy;
using Cosmere.System.Scadrial.UI.Feruchemy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     How many extra rows the duralumin ledger strip adds, exercised through the primitive
///     overload so no live Feruchemist gene is required.
/// </summary>
[TestClass]
public class FeruchemyLedgerRowTests {
    private const float Tolerance = 1e-3f;

    [TestMethod]
    public void NonDuraluminGetsNoExtraHeight() {
        Assert.AreEqual(0f, FeruchemyLedgerRow.HeightFor(false, DuraluminLedger.Residence), Tolerance);
        Assert.AreEqual(0f, FeruchemyLedgerRow.HeightFor(false, DuraluminLedger.Shard), Tolerance);
    }

    [TestMethod]
    public void NonShardTieGetsOneRow() {
        Assert.AreEqual(DockDropdownRow.Height, FeruchemyLedgerRow.HeightFor(true, DuraluminLedger.Residence), Tolerance);
        Assert.AreEqual(DockDropdownRow.Height, FeruchemyLedgerRow.HeightFor(true, DuraluminLedger.Bonds), Tolerance);
    }

    [TestMethod]
    public void ShardTieGetsTwoRowsPlusTheGapBetweenThem() {
        Assert.AreEqual(
            DockDropdownRow.Height * 2f + 6f,
            FeruchemyLedgerRow.HeightFor(true, DuraluminLedger.Shard),
            Tolerance
        );
    }
}
