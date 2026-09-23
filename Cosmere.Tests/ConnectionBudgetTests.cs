using Cosmere.System.Scadrial.Feruchemy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The arithmetic behind duralumin: how much Connection charge may move between a pawn and a
///     metalmind, bounded by what the pawn holds, the ledger's capacity, and what it already holds.
/// </summary>
[TestClass]
public class ConnectionBudgetTests {
    private const float Tolerance = 1e-3f;

    [TestMethod]
    public void EachLedgerReportsItsCapacity() {
        Assert.AreEqual(30, ConnectionBudget.Capacity(DuraluminLedger.Residence));
        Assert.AreEqual(40, ConnectionBudget.Capacity(DuraluminLedger.Shard));
        Assert.AreEqual(0, ConnectionBudget.Capacity(DuraluminLedger.Social));
    }

    [TestMethod]
    public void PointsAndChargeRoundTrip() {
        foreach (float points in new[] { 0f, 1f, 10f, 33.5f, 100f }) {
            float charge = ConnectionBudget.ChargeForPoints(points);
            Assert.AreEqual(points, ConnectionBudget.PointsForCharge(charge), Tolerance);
        }
    }

    [TestMethod]
    public void StorableIsBoundedByWhatThePawnHolds() {
        Assert.AreEqual(90f, ConnectionBudget.Storable(10f, 0f, DuraluminLedger.Shard), Tolerance);
    }

    // 50 points is worth 450 charge, but Residence only ever holds 30 points' worth.
    [TestMethod]
    public void StorableIsBoundedByTheLedgersCapacity() {
        Assert.AreEqual(270f, ConnectionBudget.Storable(50f, 0f, DuraluminLedger.Residence), Tolerance);
    }

    // Residence caps at 270 charge; 100 already stored leaves only 170 of room.
    [TestMethod]
    public void StorableIsBoundedByWhatTheLedgerAlreadyHolds() {
        Assert.AreEqual(170f, ConnectionBudget.Storable(50f, 100f, DuraluminLedger.Residence), Tolerance);
    }

    [TestMethod]
    public void TappableCannotExceedWhatTheLedgerRecords() {
        Assert.AreEqual(50f, ConnectionBudget.Tappable(1000f, 50f, DuraluminLedger.Shard), Tolerance);
    }

    // 2 points of headroom is 18 charge, well under the 100 the ledger holds.
    [TestMethod]
    public void TappableCannotExceedThePawnsHeadroom() {
        Assert.AreEqual(18f, ConnectionBudget.Tappable(2f, 100f, DuraluminLedger.Shard), Tolerance);
    }

    [TestMethod]
    public void NegativeInputsAndOverfullLedgersReturnZeroRatherThanNegative() {
        Assert.AreEqual(0f, ConnectionBudget.Storable(-10f, 0f, DuraluminLedger.Shard));
        Assert.AreEqual(0f, ConnectionBudget.Tappable(-10f, 50f, DuraluminLedger.Shard));
        Assert.AreEqual(
            0f,
            ConnectionBudget.Storable(50f, 1000f, DuraluminLedger.Shard),
            "An over-full ledger has no room left, not negative room."
        );
    }

    [TestMethod]
    public void SocialCanNeverTakeOrGiveCharge() {
        Assert.AreEqual(0f, ConnectionBudget.Storable(0f, 0f, DuraluminLedger.Social));
        Assert.AreEqual(0f, ConnectionBudget.Storable(1000f, 0f, DuraluminLedger.Social));
        Assert.AreEqual(0f, ConnectionBudget.Tappable(1000f, 500f, DuraluminLedger.Social));
    }

    // 90 charge is 10 points, so a pawn who moved 10 points owes nothing and is owed nothing.
    [TestMethod]
    public void NothingIsOwedWhenBothSidesMovedTheSame() {
        Assert.AreEqual(0f, ConnectionBudget.Settlement(90f, 10f), Tolerance);
    }

    // The metalmind banked 90 charge but the pawn only gave up 8 points' worth, so 18 was never paid for.
    [TestMethod]
    public void TheMetalmindGivesBackChargeThePawnDidNotPayFor() {
        Assert.AreEqual(18f, ConnectionBudget.Settlement(90f, 8f), Tolerance);
    }

    // The mirror: the pawn moved 10 points against a 45-charge move, so 45 charge of it is owed back to them.
    [TestMethod]
    public void ThePawnGetsBackConnectionTheMetalmindNeverTook() {
        Assert.AreEqual(-45f, ConnectionBudget.Settlement(45f, 10f), Tolerance);
    }

    [TestMethod]
    public void AMoveThatOnlyOneSideMadeIsOwedWhole() {
        Assert.AreEqual(9f, ConnectionBudget.Settlement(9f, 0f), Tolerance);
        Assert.AreEqual(-9f, ConnectionBudget.Settlement(0f, 1f), Tolerance);
        Assert.AreEqual(0f, ConnectionBudget.Settlement(0f, 0f), Tolerance);
    }

    [TestMethod]
    public void NonsenseInputIsNoCorrectionRatherThanABackwardsOne() {
        Assert.AreEqual(0f, ConnectionBudget.Settlement(-90f, 10f));
        Assert.AreEqual(0f, ConnectionBudget.Settlement(90f, -10f));
        Assert.AreEqual(0f, ConnectionBudget.Settlement(-90f, -10f));
    }

    // The point of the whole exercise: apply the correction and the two sides agree.
    [TestMethod]
    public void ApplyingTheCorrectionLeavesNothingOwed() {
        float owed = ConnectionBudget.Settlement(90f, 8f);
        Assert.AreEqual(0f, ConnectionBudget.Settlement(90f - owed, 8f), Tolerance);

        float owedToPawn = ConnectionBudget.Settlement(45f, 10f);
        float repaidPoints = 10f + ConnectionBudget.PointsForCharge(owedToPawn);
        Assert.AreEqual(0f, ConnectionBudget.Settlement(45f, repaidPoints), Tolerance);
    }

    [TestMethod]
    public void AnAskWorthLessThanOnePointIsWorthNothingYet() {
        Assert.AreEqual(0f, ConnectionBudget.WholeCharge(8.9f), Tolerance);
        Assert.AreEqual(0f, ConnectionBudget.WholeCharge(-8.9f), Tolerance);
        Assert.AreEqual(0f, ConnectionBudget.WholeCharge(0f), Tolerance);
    }

    // 19 charge is two whole points plus a remainder the bank keeps.
    [TestMethod]
    public void OnlyWholePointsComeOutOfTheBankAndTheSignSurvives() {
        Assert.AreEqual(18f, ConnectionBudget.WholeCharge(19f), Tolerance);
        Assert.AreEqual(-18f, ConnectionBudget.WholeCharge(-19f), Tolerance);
    }

    // What the tick asks a ledger for: a 14-charge move buys one point, and the odd 5 goes back.
    [TestMethod]
    public void APartOfAPointIsNeverAskedForAndNeverPaidFor() {
        float ask = ConnectionBudget.PointsForCharge(ConnectionBudget.WholeCharge(14f));
        Assert.AreEqual(1f, ask, Tolerance);
        Assert.AreEqual(5f, ConnectionBudget.Settlement(14f, ask), Tolerance);
    }
}
