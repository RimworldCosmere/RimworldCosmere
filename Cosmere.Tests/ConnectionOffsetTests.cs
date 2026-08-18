using System;
using System.IO;
using Cosmere.Core.ShardConnection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The bounds on holding part of a Shard tie somewhere else: how much may leave, and what a
///     clamped move reports back to whoever asked for it.
/// </summary>
[TestClass]
public class ConnectionOffsetTests {
    private const float Tolerance = 1e-3f;

    [TestMethod]
    public void TheOffsetMayNeverTakeTheLastPoint() {
        Assert.AreEqual(49f, ConnectionMath.OffsetCeiling(50), Tolerance);
        Assert.AreEqual(0f, ConnectionMath.OffsetCeiling(1), Tolerance);
    }

    [TestMethod]
    public void APawnWithNothingCanGiveNothing() {
        Assert.AreEqual(0f, ConnectionMath.OffsetCeiling(0), Tolerance);
        Assert.AreEqual(0f, ConnectionMath.OffsetCeiling(-5), Tolerance);
    }

    /// <summary>The ceiling is one short of the total at every step, not only at the two the design names.</summary>
    [TestMethod]
    public void TheCeilingTrailsTheTotalByExactlyOne() {
        for (int total = 2; total <= ConnectionMath.Max; total++) {
            Assert.AreEqual(total - 1f, ConnectionMath.OffsetCeiling(total), Tolerance, $"total {total}");
        }
    }

    [TestMethod]
    public void GivingStopsAtTheCeilingRatherThanTheAsk() {
        Assert.AreEqual(49f, ConnectionMath.ClampOffset(40f, 20f, 49f), Tolerance);
    }

    [TestMethod]
    public void TakingBackStopsAtNothingHeld() {
        Assert.AreEqual(0f, ConnectionMath.ClampOffset(5f, -20f, 49f), Tolerance);
    }

    [TestMethod]
    public void AnAskInsideTheHeadroomMovesInFull() {
        Assert.AreEqual(45f, ConnectionMath.ClampOffset(40f, 5f, 49f), Tolerance);
        Assert.AreEqual(35f, ConnectionMath.ClampOffset(40f, -5f, 49f), Tolerance);
    }

    [TestMethod]
    public void NoAskMovesNothing() {
        Assert.AreEqual(12f, ConnectionMath.ClampOffset(12f, 0f, 49f), Tolerance);
    }

    /// <summary>
    ///     The sign inversion that residence ticks shipped with: an offset already past its ceiling
    ///     must move nothing rather than lurch backwards to the ceiling.
    /// </summary>
    [TestMethod]
    public void AnOffsetPastItsCeilingMovesNothingRatherThanBackwards() {
        Assert.AreEqual(60f, ConnectionMath.ClampOffset(60f, 5f, 49f), Tolerance);
    }

    [TestMethod]
    public void AnOffsetPastItsCeilingCanStillBeTakenBack() {
        Assert.AreEqual(50f, ConnectionMath.ClampOffset(60f, -10f, 49f), Tolerance);
    }

    /// <summary>A pawn holding a single point has no headroom at all, so an ask of any size moves nothing.</summary>
    [TestMethod]
    public void ATieOfOneCannotBeGivenAway() {
        float ceiling = ConnectionMath.OffsetCeiling(1);

        Assert.AreEqual(0f, ConnectionMath.ClampOffset(0f, 40f, ceiling), Tolerance);
    }

    /// <summary>What a clamped move actually shifted, which is what the caller is owed back.</summary>
    [TestMethod]
    public void ClampedMovesReportTheirMovementAndNotTheAsk() {
        Assert.AreEqual(9f, ConnectionMath.ClampOffset(40f, 20f, 49f) - 40f, Tolerance);
        Assert.AreEqual(-5f, ConnectionMath.ClampOffset(5f, -20f, 49f) - 5f, Tolerance);
        Assert.AreEqual(0f, ConnectionMath.ClampOffset(60f, 5f, 49f) - 60f, Tolerance);
    }

    /// <summary>Everything given away comes back, so a full round trip leaves the pawn where they started.</summary>
    [TestMethod]
    public void GivingEverythingAndTakingItBackConserves() {
        const int total = 50;
        float ceiling = ConnectionMath.OffsetCeiling(total);

        float held = ConnectionMath.ClampOffset(0f, total, ceiling);
        Assert.AreEqual(49f, held, Tolerance);
        Assert.AreEqual(1, ConnectionMath.Clamp(total - (int)held), "The last point never leaves.");

        Assert.AreEqual(0f, ConnectionMath.ClampOffset(held, -held, ceiling), Tolerance);
    }

    /// <summary>
    ///     Raw is where every reader of a tie converges and it needs a live game, so the subtraction
    ///     itself can only be pinned by reading it. Drop it and a held tie stays with its owner.
    /// </summary>
    [TestMethod]
    public void TheComposedTotalHasTheHeldPortionTakenOffIt() {
        string source = Source("ConnectionUtility.cs");

        int raw = source.IndexOf("private static int Raw(", StringComparison.Ordinal);
        int composed = source.IndexOf("private static int Composed(", StringComparison.Ordinal);
        Assert.IsTrue(raw >= 0, "ConnectionUtility.Raw is gone; the tie is composed somewhere else now.");
        Assert.IsTrue(composed > raw, "Composed no longer follows Raw, so this scan reads the wrong body.");

        string body = source.Substring(raw, composed - raw);
        Assert.IsTrue(
            body.Contains("ConnectionOffsets.Get(pawn, shard)", StringComparison.Ordinal),
            "Raw must read the held offset, or nothing a pawn stores ever leaves them."
        );
        Assert.IsTrue(
            body.Contains("Composed(pawn, shard) - held", StringComparison.Ordinal),
            "The held portion comes off the composed total. Adding it would pay the pawn twice."
        );
    }

    /// <summary>Scribe_Collections.Look hands back null for an empty dictionary, so the guard is load-bearing.</summary>
    [TestMethod]
    public void TheOffsetStoreSurvivesLoadingASaveThatHeldNothing() {
        string source = Source("ConnectionOffsets.cs");

        int look = source.IndexOf("Scribe_Collections.Look(ref heldByPawn", StringComparison.Ordinal);
        Assert.IsTrue(look >= 0, "The offsets are no longer scribed, so nothing anyone stores survives a save.");
        Assert.IsTrue(
            source.IndexOf("heldByPawn ??= []", StringComparison.Ordinal) > look,
            "The null guard must follow the Look, or loading a save with no offsets throws."
        );
    }

    private static string Source(string file) {
        return File.ReadAllText(
            Path.Combine(RepoRoot, "CosmereCore", "CosmereCore", "Core", "ShardConnection", file)
        );
    }

    private static string RepoRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate the repo root above the test output directory.");
            return dir!.FullName;
        }
    }
}
