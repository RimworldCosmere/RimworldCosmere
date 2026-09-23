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

        int look = source.IndexOf("Scribe_Collections.Look(ref scribed", StringComparison.Ordinal);
        Assert.IsTrue(look >= 0, "The offsets are no longer scribed, so nothing anyone stores survives a save.");
        Assert.IsTrue(
            source.IndexOf("scribed ??= []", StringComparison.Ordinal) > look,
            "The null guard must follow the Look, or loading a save with no offsets throws."
        );
    }

    /// <summary>
    ///     Dropping an offset row hands a pawn back a tie whose charge is still stored elsewhere, so
    ///     this store must never prune the way ResidenceTracker does. The signs are opposite.
    /// </summary>
    [TestMethod]
    public void TheOffsetStoreNeverPrunesDepartedPawns() {
        string source = Source("ConnectionOffsets.cs");

        int expose = source.IndexOf("public override void ExposeData()", StringComparison.Ordinal);
        int get = source.IndexOf("public static float Get(", expose, StringComparison.Ordinal);
        Assert.IsTrue(expose >= 0 && get > expose, "Expected ExposeData before Get.");

        string body = source.Substring(expose, get - expose);

        Assert.IsFalse(
            source.Contains("Prune", StringComparison.Ordinal),
            "Pruning an offset refunds a banked tie. Read the note in ExposeData before adding it back."
        );
        Assert.IsFalse(
            source.Contains("mapPawns", StringComparison.Ordinal),
            "Nothing here should care whether a pawn is still on a map; leaving does not clear a debt."
        );
        Assert.IsFalse(
            body.Contains("heldByPawn.Clear()", StringComparison.Ordinal)
            || body.Contains("heldByPawn.Remove(", StringComparison.Ordinal),
            "ExposeData must not drop live rows before saving them."
        );
    }

    /// <summary>A clamped or swallowed write reports the stored movement, never the request.</summary>
    [TestMethod]
    public void AdjustOffsetReportsWhatTheStoreAccepted() {
        string source = Source("ConnectionUtility.cs");

        int adjust = source.IndexOf("public static float AdjustOffset(", StringComparison.Ordinal);
        int entity = source.IndexOf("private static Cosmere.Core.Entity.Shard? Entity(", adjust, StringComparison.Ordinal);
        Assert.IsTrue(adjust >= 0 && entity > adjust, "Expected AdjustOffset before Entity.");

        string body = source.Substring(adjust, entity - adjust);
        Assert.IsTrue(
            body.Contains("return ConnectionOffsets.Get(pawn, shard) - had;", StringComparison.Ordinal),
            "AdjustOffset must report what the store accepted after clamping or swallowing the write."
        );
    }

    /// <summary>The Shard ledger offers the spendable tie and treats the held offset as tap headroom.</summary>
    [TestMethod]
    public void ShardLedgerReadsTheSpendableTieAndHeldHeadroom() {
        string source = ShardLedgerSource();

        Assert.IsTrue(
            source.Contains(
                "return ConnectionMath.OffsetCeiling(ConnectionUtility.StrengthOf(pawn, shard));",
                StringComparison.Ordinal
            ),
            "CurrentPoints must offer the whole spendable tie."
        );
        Assert.IsTrue(
            source.Contains("return ConnectionOffsets.Get(pawn, shard);", StringComparison.Ordinal),
            "HeadroomPoints must report what the pawn can take back."
        );
    }

    /// <summary>The ledger translates pawn movement to offset movement and translates the report back.</summary>
    [TestMethod]
    public void ShardLedgerInvertsBothSidesOfOffsetMovement() {
        string source = ShardLedgerSource();

        Assert.IsTrue(
            source.Contains("return -ConnectionUtility.AdjustOffset(pawn, shard, -points);", StringComparison.Ordinal),
            "A store is negative on the pawn and positive on the offset, but Move must report the pawn sign."
        );
    }

    /// <summary>
    ///     A grant tops a pawn up to a level. Measured against the reduced reading it pays out the
    ///     set-aside portion a second time: composed 40, set aside 39, drink a 40-grant metal, read 79.
    /// </summary>
    [TestMethod]
    public void ATopUpMeasuresAgainstTheTieIncludingWhatWasSetAside() {
        string source = Source("ConnectionUtility.cs");

        int grantFromMetal = source.IndexOf("public static void GrantFromMetal(", StringComparison.Ordinal);
        Assert.IsTrue(grantFromMetal >= 0, "GrantFromMetal is gone; the top-up rule lives somewhere else now.");

        int shortfall = source.IndexOf("int shortfall =", grantFromMetal, StringComparison.Ordinal);
        Assert.IsTrue(shortfall > grantFromMetal, "GrantFromMetal no longer computes a shortfall.");

        string line = source.Substring(shortfall, source.IndexOf('\n', shortfall) - shortfall);
        Assert.IsTrue(
            line.Contains("StrengthBeforeOffset(", StringComparison.Ordinal),
            $"The shortfall must count what is held elsewhere, or setting a tie aside earns a grant twice: {line}"
        );
    }

    /// <summary>
    ///     Both readings compose the same Harmony rules over different bases. Two copies of those two
    ///     branches would drift, and the drift would only show on Ruin, Preservation and Harmony.
    /// </summary>
    [TestMethod]
    public void TheTwoReadingsShareOneHarmonyBody() {
        string source = Source("ConnectionUtility.cs");

        Assert.AreEqual(
            1,
            Occurrences(source, "ConnectionMath.HarmonyFrom("),
            "The Harmony derivation is duplicated. Parameterise the one body instead."
        );
        Assert.AreEqual(
            2,
            Occurrences(source, "StrengthFrom(pawn, shard,"),
            "StrengthOf and StrengthBeforeOffset must both delegate to the shared body."
        );
    }

    private static int Occurrences(string source, string needle) {
        int count = 0;
        for (int at = source.IndexOf(needle, StringComparison.Ordinal);
             at >= 0;
             at = source.IndexOf(needle, at + needle.Length, StringComparison.Ordinal)) {
            count++;
        }

        return count;
    }

    private static string Source(string file) {
        return File.ReadAllText(
            Path.Combine(RepoRoot, "CosmereCore", "CosmereCore", "Core", "ShardConnection", file)
        );
    }

    private static string ShardLedgerSource() {
        return File.ReadAllText(
            Path.Combine(
                RepoRoot,
                "CosmereCore",
                "CosmereCore",
                "System",
                "Scadrial",
                "Feruchemy",
                "Ledger",
                "ShardLedger.cs"
            )
        );
    }

    private static string RepoRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate the repo root above the test output directory.");
            return dir.FullName;
        }
    }
}
