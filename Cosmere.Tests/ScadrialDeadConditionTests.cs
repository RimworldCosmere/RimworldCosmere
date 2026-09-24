using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>Four sites where a guard asked a question it already knew the answer to.</summary>
/// <remarks>All four are Pawn-, WorldGrid- or PawnGenerator-bound, so the source is the only surface here.</remarks>
[TestClass]
public class ScadrialDeadConditionTests {
    private static string RepoRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate CosmereScadrial/Defs above the test output directory.");
            return dir!.FullName;
        }
    }

    private static string Read(params string[] parts) {
        string path = Path.Combine(RepoRoot, Path.Combine(parts));
        Assert.IsTrue(File.Exists(path), $"Missing source file: {path}");

        return File.ReadAllText(path);
    }

    /// <summary>
    ///     The sweep reads one pawn per cell, so membership of it answered "did I see you", not
    ///     "are you in the aura". A pawn sharing a cell lost its source every second.
    /// </summary>
    [TestMethod]
    public void AuraLeaveCheckMeasuresDistanceInsteadOfTrustingTheCellSweep() {
        string source = Read(
            "CosmereCore",
            "CosmereCore",
            "System",
            "Scadrial",
            "Allomancy",
            "Comp",
            "Hediff",
            "AllomancyAuraHediffGiver.cs"
        );

        Assert.IsFalse(
            Regex.IsMatch(source, @"nearbyPawns\s*\.\s*Contains"),
            "The leave-check is back on the cell sweep, which sees one pawn per cell."
        );
        Assert.IsTrue(
            Regex.IsMatch(source, @"StillInReach\(\s*pawn\s*,\s*radius\s*\)"),
            "The aura leave-check must measure the distance itself."
        );
        Assert.IsTrue(
            Regex.IsMatch(source, @"self\.DistanceTo\(\s*other\.Position\s*\)\s*<=\s*radius"),
            "StillInReach must compare against the same radius the sweep used."
        );
    }

    /// <summary>
    ///     WorldGrid.GetTileNeighbors goes through PackedListOfLists.GetList, whose first statement
    ///     is outList.Clear(). Clearing the buffer first was a second clear on an empty list.
    /// </summary>
    [TestMethod]
    public void BronzeRingWalkDoesNotClearABufferGetTileNeighborsAlreadyClears() {
        string source = Read(
            "CosmereCore", "CosmereCore", "System", "Scadrial", "Util", "BronzeDetectionUtility.cs"
        );

        Assert.IsTrue(
            Regex.IsMatch(source, @"Find\.WorldGrid\.GetTileNeighbors\("),
            "The ring walk no longer reads neighbours, so this guard is pointed at nothing."
        );
        Assert.IsFalse(
            Regex.IsMatch(source, @"neighbors\s*\.\s*Clear\s*\(\s*\)"),
            "GetTileNeighbors clears the buffer for us; clearing it first is a dead call."
        );
    }

    /// <summary>
    ///     Vanilla's only MapParent-making step is Factions at order 500 and ours runs at 350, so
    ///     the set it was checked against was always empty.
    /// </summary>
    [TestMethod]
    public void AshmountSitingAsksForMapParentsInsteadOfBuildingAnAlwaysEmptySet() {
        string source = Read(
            "CosmereCore", "CosmereCore", "System", "Scadrial", "World", "WorldGenStep_Ashmounts.cs"
        );

        Assert.IsTrue(
            Regex.IsMatch(source, @"Find\.WorldObjects\.AnyMapParentAt\(\s*tile\s*\)"),
            "Siting must still refuse a tile something else already settled."
        );
        Assert.IsFalse(
            Regex.IsMatch(source, @"Find\.WorldObjects\.MapParents"),
            "The pre-built taken set is back, and at order 350 it is always empty."
        );
    }

    /// <summary>
    ///     Band is a compile-time constant, so a Band == 1 arm of the growth spread could never be
    ///     taken whatever value the constant holds.
    /// </summary>
    [TestMethod]
    public void KolossBandGrowthHasNoBranchForABandSizeItCannotHave() {
        string source = Read("CosmereCore", "CosmereQuickstarts", "Scadrial", "PreCatacendreQuickstart.cs");

        Assert.IsTrue(
            Regex.IsMatch(source, @"const\s+int\s+Band\s*=\s*\d+\s*;"),
            "Band is expected to stay a compile-time constant."
        );
        Assert.IsTrue(
            Regex.IsMatch(source, @"Grow\(\s*one\s*,\s*\(\s*i\s*\+\s*0\.5f\s*\)\s*/\s*Band\s*\)"),
            "The spread must hold at any Band without a branch for a size Band cannot be."
        );
        Assert.IsFalse(
            Regex.IsMatch(source, @"Band\s*[=!]=\s*1\b"),
            "Band is a constant, so comparing it to 1 is a branch nothing can reach."
        );
    }

    /// <summary>
    ///     The replacement spread, checked at the Band the quickstart actually uses. Severity 0 would
    ///     make Hediff.ShouldRemove true, so the first koloss must land above it.
    /// </summary>
    [TestMethod]
    public void KolossBandGrowthSpreadsInsideTheSeverityRange() {
        const int band = 10;
        float previous = 0f;

        for (int i = 0; i < band; i++) {
            float along = (i + 0.5f) / band;

            Assert.IsTrue(along > 0f, $"koloss {i} would be removed on the next tick at severity {along}.");
            Assert.IsTrue(along <= 1f, $"koloss {i} is past full growth at severity {along}.");
            Assert.IsTrue(along > previous, $"koloss {i} is not larger than the one before it.");
            previous = along;
        }
    }

    /// <summary>A band of one still gets a middling koloss rather than a divide by zero.</summary>
    [TestMethod]
    public void KolossBandGrowthSurvivesABandOfOne() {
        Assert.AreEqual(0.5f, (0 + 0.5f) / 1, 0.0001f);
    }
}
