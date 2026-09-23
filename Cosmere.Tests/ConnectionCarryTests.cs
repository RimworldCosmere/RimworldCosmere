using Cosmere.System.Scadrial.Feruchemy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The bank that turns a duralumin dial's fractional per-second rate into the whole-point asks
///     the Connection ledgers are able to move.
/// </summary>
[TestClass]
public class ConnectionCarryTests {
    private const float Tolerance = 1e-3f;

    private const float Plenty = 10000f;

    // The dial's fastest setting is 1 charge a second and a point is 9, so nothing moves on tick one.
    [TestMethod]
    public void ASecondOfStoringIsNotYetWorthAPoint() {
        float carry = 0f;

        Assert.AreEqual(0f, ConnectionCarry.Bank(ref carry, 1f, Plenty, true));
        Assert.AreEqual(1f, carry, Tolerance);
    }

    [TestMethod]
    public void TheBankPaysOutAWholePointAndKeepsTheRemainder() {
        float carry = 8f;

        Assert.AreEqual(9f, ConnectionCarry.Bank(ref carry, 2f, Plenty, true), Tolerance);
        Assert.AreEqual(1f, carry, Tolerance);
    }

    [TestMethod]
    public void TappingPaysOutAPositiveChargeFromANegativeBank() {
        float carry = -8f;

        Assert.AreEqual(9f, ConnectionCarry.Bank(ref carry, 2f, Plenty, false), Tolerance);
        Assert.AreEqual(-1f, carry, Tolerance);
    }

    // The whole reason the debit happens after the clamp: a budget of 0 must not eat the bank.
    [TestMethod]
    public void ABudgetThatBlocksTheAskLeavesTheIntentBanked() {
        float carry = 8f;

        Assert.AreEqual(0f, ConnectionCarry.Bank(ref carry, 2f, 0f, true), Tolerance);
        Assert.AreEqual(10f, carry, Tolerance);
    }

    // 18 charge is banked but only 9 fits, so the second point stays owed rather than vanishing.
    [TestMethod]
    public void ABudgetThatCoversPartOfTheAskKeepsTheRest() {
        float carry = 17f;

        Assert.AreEqual(9f, ConnectionCarry.Bank(ref carry, 1f, 9f, true), Tolerance);
        Assert.AreEqual(9f, carry, Tolerance);
    }

    [TestMethod]
    public void ANegativeBudgetMovesNothingAndKeepsTheBank() {
        float carry = 20f;

        Assert.AreEqual(0f, ConnectionCarry.Bank(ref carry, 1f, -20f, true), Tolerance);
        Assert.AreEqual(21f, carry, Tolerance);
    }

    // Banked storing intent must not part-pay a tap the player has only just turned to.
    [TestMethod]
    public void TurningTheDialAroundWalksTheBankBackThroughZero() {
        float carry = 8f;

        Assert.AreEqual(0f, ConnectionCarry.Bank(ref carry, 4f, Plenty, false), Tolerance);
        Assert.AreEqual(4f, carry, Tolerance);
        Assert.AreEqual(0f, ConnectionCarry.Bank(ref carry, 4f, Plenty, false), Tolerance);
        Assert.AreEqual(0f, carry, Tolerance);
        Assert.AreEqual(0f, ConnectionCarry.Bank(ref carry, 4f, Plenty, false), Tolerance);
        Assert.AreEqual(-4f, carry, Tolerance);
    }

    [TestMethod]
    public void AStoringBankNeverPaysOutOnATapAndTheOtherWayAround() {
        float storing = 20f;
        Assert.AreEqual(0f, ConnectionCarry.Bank(ref storing, 0f, Plenty, false), Tolerance);

        float tapping = -20f;
        Assert.AreEqual(0f, ConnectionCarry.Bank(ref tapping, 0f, Plenty, true), Tolerance);
    }

    // Nine seconds at the dial's stop is one point, and none of it goes missing on the way.
    [TestMethod]
    public void NineSecondsOfStoringPaysOutExactlyOnePoint() {
        float carry = 0f;
        float total = 0f;
        for (int i = 0; i < 9; i++) total += ConnectionCarry.Bank(ref carry, 1f, Plenty, true);

        Assert.AreEqual(9f, total, Tolerance);
        Assert.AreEqual(0f, carry, Tolerance);
    }
}
