using Cosmere.System.Scadrial.Feruchemy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Which side of a duralumin transfer hands back the difference, and with which sign. The
///     magnitude lives in ConnectionBudget; this is the routing that decides where it lands.
/// </summary>
[TestClass]
public class ConnectionSettlementTests {
    private const float Tolerance = 1e-3f;

    [TestMethod]
    public void StoringAsksTheLedgerToTakePointsAndTappingAsksItToGive() {
        Assert.AreEqual(-2f, ConnectionSettlement.Ask(18f, true), Tolerance);
        Assert.AreEqual(2f, ConnectionSettlement.Ask(18f, false), Tolerance);
    }

    // 14 charge buys one whole point; the ledger is never asked for the part of a point.
    [TestMethod]
    public void TheAskIsAlwaysAWholeNumberOfPoints() {
        Assert.AreEqual(-1f, ConnectionSettlement.Ask(14f, true), Tolerance);
        Assert.AreEqual(0f, ConnectionSettlement.Ask(8f, true), Tolerance);
        Assert.AreEqual(0f, ConnectionSettlement.Ask(-90f, true), Tolerance);
    }

    [TestMethod]
    public void SidesThatAgreeSettleNothing() {
        ConnectionSettlement stored = ConnectionSettlement.Resolve(18f, true, -2f);
        Assert.AreEqual(0f, stored.MetalmindCharge);
        Assert.AreEqual(0f, stored.PawnPoints);

        ConnectionSettlement tapped = ConnectionSettlement.Resolve(18f, false, 2f);
        Assert.AreEqual(0f, tapped.MetalmindCharge);
        Assert.AreEqual(0f, tapped.PawnPoints);
    }

    // The metalmind took 18 but the pawn only gave one point, so 9 comes back out of the metalmind.
    [TestMethod]
    public void AStoreTheLedgerUnderpaidComesBackOutOfTheMetalmind() {
        ConnectionSettlement due = ConnectionSettlement.Resolve(18f, true, -1f);

        Assert.AreEqual(-9f, due.MetalmindCharge, Tolerance);
        Assert.AreEqual(0f, due.PawnPoints);
    }

    // The mirror: the metalmind gave up 18 and the pawn only took one point's worth, so 9 goes back in.
    [TestMethod]
    public void ATapTheLedgerUnderpaidGoesBackIntoTheMetalmind() {
        ConnectionSettlement due = ConnectionSettlement.Resolve(18f, false, 1f);

        Assert.AreEqual(9f, due.MetalmindCharge, Tolerance);
        Assert.AreEqual(0f, due.PawnPoints);
    }

    [TestMethod]
    public void AStoreThatTookMoreFromThePawnThanTheMetalmindKeptIsGivenBack() {
        ConnectionSettlement due = ConnectionSettlement.Resolve(9f, true, -2f);

        Assert.AreEqual(0f, due.MetalmindCharge);
        Assert.AreEqual(1f, due.PawnPoints, Tolerance);
    }

    [TestMethod]
    public void ATapThatGaveThePawnMoreThanTheMetalmindLostIsTakenBack() {
        ConnectionSettlement due = ConnectionSettlement.Resolve(9f, false, 2f);

        Assert.AreEqual(0f, due.MetalmindCharge);
        Assert.AreEqual(-1f, due.PawnPoints, Tolerance);
    }

    // A ledger reporting movement the wrong way counts as no movement, so the metalmind unwinds it.
    [TestMethod]
    public void MovementInTheWrongDirectionIsTreatedAsNoMovement() {
        ConnectionSettlement stored = ConnectionSettlement.Resolve(18f, true, 2f);
        Assert.AreEqual(-18f, stored.MetalmindCharge, Tolerance);

        ConnectionSettlement tapped = ConnectionSettlement.Resolve(18f, false, -2f);
        Assert.AreEqual(18f, tapped.MetalmindCharge, Tolerance);
    }

    // Only one side ever settles, in every direction and at every shortfall.
    [TestMethod]
    public void TheTwoSidesAreNeverBothAsked() {
        foreach (bool storing in new[] { true, false }) {
            foreach (float points in new[] { -3f, -1f, 0f, 1f, 3f }) {
                ConnectionSettlement due = ConnectionSettlement.Resolve(18f, storing, points);
                Assert.IsTrue(
                    due.MetalmindCharge == 0f || due.PawnPoints == 0f,
                    $"storing={storing} points={points} asked both sides to settle."
                );
            }
        }
    }
}
