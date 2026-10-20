using Cosmere.System.Scadrial.Feruchemy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Conservation in the only form a Verse-free test host can run it: what the metalmind
///     took has to be exactly what comes back out of the ledger, once.
/// </summary>
[TestClass]
public class ChargeLedgerTests {
    [TestMethod]
    public void DrainReturnsWhatWentIn() {
        ChargeLedger ledger = new ChargeLedger();

        ledger.Stored(3f);
        ledger.Stored(5f);

        Assert.AreEqual(8f, ledger.Drain());
    }

    [TestMethod]
    public void TappingReadsBackNegative() {
        ChargeLedger ledger = new ChargeLedger();

        ledger.Tapped(4f);

        Assert.AreEqual(-4f, ledger.Drain());
    }

    // The guard against two readers applying the same movement twice.
    [TestMethod]
    public void DrainingTwiceReturnsNothingTheSecondTime() {
        ChargeLedger ledger = new ChargeLedger();

        ledger.Stored(6f);

        Assert.AreEqual(6f, ledger.Drain());
        Assert.AreEqual(0f, ledger.Drain());
    }

    [TestMethod]
    public void StoringThenTappingTheSameAmountLeavesNothingToApply() {
        ChargeLedger ledger = new ChargeLedger();

        ledger.Stored(7f);
        ledger.Tapped(7f);

        Assert.AreEqual(0f, ledger.Drain());
    }

    [TestMethod]
    public void AnUntouchedLedgerHasNothingToGive() {
        Assert.AreEqual(0f, new ChargeLedger().Drain());
    }
}
