using Cosmere.Core.Quest;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Guards the capstone lifecycle. A declined capstone must return to NotFired so its
///     cooldown governs the re-offer, and Burned must never leave.
/// </summary>
[TestClass]
public class CapstoneStateMachineTests {
    [TestMethod]
    public void DeclineReturnsToNotFired() {
        Assert.AreEqual(CapstoneState.NotFired, CapstoneStateMachine.OnDeclined(CapstoneState.Offered));
    }

    [TestMethod]
    public void AcceptAndSucceedCompletes() {
        Assert.AreEqual(CapstoneState.Completed, CapstoneStateMachine.OnCompleted(CapstoneState.Offered));
    }

    [TestMethod]
    public void AcceptAndFailBurns() {
        Assert.AreEqual(CapstoneState.Burned, CapstoneStateMachine.OnFailed(CapstoneState.Offered));
    }

    [TestMethod]
    public void BurnedIsTerminal() {
        Assert.IsTrue(CapstoneStateMachine.IsTerminal(CapstoneState.Burned));
        Assert.IsFalse(CapstoneStateMachine.CanTransition(CapstoneState.Burned, CapstoneState.NotFired));
        Assert.IsFalse(CapstoneStateMachine.CanTransition(CapstoneState.Burned, CapstoneState.Offered));
        Assert.AreEqual(CapstoneState.Burned, CapstoneStateMachine.OnDeclined(CapstoneState.Burned));
    }

    [TestMethod]
    public void CompletedIsTerminal() {
        Assert.IsTrue(CapstoneStateMachine.IsTerminal(CapstoneState.Completed));
        Assert.IsFalse(CapstoneStateMachine.CanTransition(CapstoneState.Completed, CapstoneState.Offered));
    }

    [TestMethod]
    public void OnlyNotFiredMayBeOffered() {
        Assert.IsTrue(CapstoneStateMachine.CanTransition(CapstoneState.NotFired, CapstoneState.Offered));
        Assert.IsFalse(CapstoneStateMachine.CanTransition(CapstoneState.Offered, CapstoneState.Offered));
    }

    [TestMethod]
    public void NotFiredCannotJumpStraightToAnOutcome() {
        Assert.IsFalse(CapstoneStateMachine.CanTransition(CapstoneState.NotFired, CapstoneState.Completed));
        Assert.IsFalse(CapstoneStateMachine.CanTransition(CapstoneState.NotFired, CapstoneState.Burned));
    }
}
