using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>Four sites where a guard asked a question it already knew the answer to.</summary>
/// <remarks>All four are Pawn-, WorldGrid- or PawnGenerator-bound, so the source is the only surface here.</remarks>
[TestClass]
public partial class ScadrialDeadConditionTests {
    [GeneratedRegex(@"nearbyPawns\s*\.\s*Contains")]
    private static partial Regex NearbyPawnsContains { get; }

    [GeneratedRegex(@"StillInReach\(\s*pawn\s*,\s*radius\s*\)")]
    private static partial Regex StillInReachCall { get; }

    [GeneratedRegex(@"self\.DistanceTo\(\s*other\.Position\s*\)\s*<=\s*radius")]
    private static partial Regex StillInReachRadius { get; }

    [GeneratedRegex(@"Find\.WorldGrid\.GetTileNeighbors\(")]
    private static partial Regex GetTileNeighborsCall { get; }

    [GeneratedRegex(@"neighbors\s*\.\s*Clear\s*\(\s*\)")]
    private static partial Regex NeighborsClear { get; }

    [GeneratedRegex(@"Find\.WorldObjects\.AnyMapParentAt\(\s*tile\s*\)")]
    private static partial Regex AnyMapParentAtCall { get; }

    [GeneratedRegex(@"Find\.WorldObjects\.MapParents")]
    private static partial Regex MapParentsSet { get; }

    [GeneratedRegex(@"const\s+int\s+Band\s*=\s*\d+\s*;")]
    private static partial Regex BandConstant { get; }

    [GeneratedRegex(@"Grow\(\s*one\s*,\s*i\s*/\s*\(float\)\s*\(\s*Band\s*-\s*1\s*\)\s*\)")]
    private static partial Regex BandSpreadCall { get; }

    [GeneratedRegex(@"Band\s*[=!]=\s*1\b")]
    private static partial Regex BandComparedToOne { get; }

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
            NearbyPawnsContains.IsMatch(source),
            "The leave-check is back on the cell sweep, which sees one pawn per cell."
        );
        Assert.IsTrue(
            StillInReachCall.IsMatch(source),
            "The aura leave-check must measure the distance itself."
        );
        Assert.IsTrue(
            StillInReachRadius.IsMatch(source),
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
            GetTileNeighborsCall.IsMatch(source),
            "The ring walk no longer reads neighbours, so this guard is pointed at nothing."
        );
        Assert.IsFalse(
            NeighborsClear.IsMatch(source),
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
            AnyMapParentAtCall.IsMatch(source),
            "Siting must still refuse a tile something else already settled."
        );
        Assert.IsFalse(
            MapParentsSet.IsMatch(source),
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
            BandConstant.IsMatch(source),
            "Band is expected to stay a compile-time constant."
        );
        Assert.IsTrue(
            BandSpreadCall.IsMatch(source),
            "The spread must hold at any Band without a branch for a size Band cannot be."
        );
        Assert.IsFalse(
            BandComparedToOne.IsMatch(source),
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

            Assert.IsGreaterThan(0f, along, $"koloss {i} would be removed on the next tick at severity {along}.");
            Assert.IsLessThanOrEqualTo(1f, along, $"koloss {i} is past full growth at severity {along}.");
            Assert.IsGreaterThan(previous, along, $"koloss {i} is not larger than the one before it.");
            previous = along;
        }
    }
}
