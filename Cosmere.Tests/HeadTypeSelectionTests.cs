using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Guards the bug that put Sazed in the Pre-Catacendre scenario with no head.
/// </summary>
/// <remarks>
///     <c>Skull</c> and <c>Stump</c> are ordinary HeadTypeDefs carrying <c>randomChosen=false</c>,
///     <c>selectionWeight=0</c> and <c>gender=None</c>. A filter written as "gender matches, or the
///     head has no gender" therefore accepts both of them, and Sazed drew the graphic vanilla uses
///     for a head that has been cut off. Nothing was wrong with his body - the scenario handed him
///     the wrong face.
///     <para>
///         These assert on source text because a HeadTypeDef cannot be built without Verse loaded.
///         What they actually guard is the rule: nobody picks a head straight out of the database.
///     </para>
/// </remarks>
[TestClass]
public partial class HeadTypeSelectionTests {
    private static string RepoRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate CosmereScadrial/Defs above the test output directory.");

            return dir.FullName;
        }
    }

    [GeneratedRegex(@"selectionWeight\s*<=\s*0f")]
    private static partial Regex ZeroWeightGuard { get; }

    [GeneratedRegex(@"!\s*head\.randomChosen")]
    private static partial Regex RandomChosenGuard { get; }

    [GeneratedRegex(@"DefDatabase<HeadTypeDef>\s*\.\s*AllDefs")]
    private static partial Regex DatabaseReach { get; }

    private static string Source(params string[] parts) {
        return File.ReadAllText(Path.Combine([RepoRoot, "CosmereCore", "CosmereCore", .. parts]));
    }

    /// <summary>
    ///     The three things vanilla checks before it will consider a head, all in one place.
    /// </summary>
    [TestMethod]
    public void TheHeadPickerRefusesHeadsTheGeneratorWouldNever() {
        string util = Source("Core", "Util", "HeadTypeUtility.cs");

        Assert.Contains("randomChosen", util, "Stump and Skull are excluded by randomChosen.");
        Assert.Contains("selectionWeight", util, "A zero weight means never offer it.");
        Assert.Contains("requiredGenes", util, "Gene-gated heads belong to the pawns with the gene.");
    }

    /// <summary>
    ///     A weight of zero has to lose. Reading the field is not the same as honouring it, so this
    ///     checks the guard rather than the mention.
    /// </summary>
    [TestMethod]
    public void AZeroWeightHeadIsNeverOffered() {
        string util = Source("Core", "Util", "HeadTypeUtility.cs");

        Assert.IsTrue(
            ZeroWeightGuard.IsMatch(util),
            "Available must drop a zero-weight head, not merely read the field."
        );
        Assert.IsTrue(
            RandomChosenGuard.IsMatch(util),
            "Available must drop a head the generator would not choose."
        );
    }

    /// <summary>
    ///     Both places that used to reach into the database directly. The scenario applier is where
    ///     Sazed lost his head; the spren builder took index 0, which is whatever loaded first and
    ///     would have become a skull the moment another mod added a head above ours.
    /// </summary>
    [TestMethod]
    public void NobodyPicksAHeadOutOfTheDatabaseThemselves() {
        foreach (string[] where in new[] {
            new[] { "Core", "ScenarioPart", "Parts", "ScenPart_NamedPawns.cs" },
            ["System", "Roshar", "Extension", "Pawn_GeneTrackerExtension.cs"],
        }) {
            string source = Source(where);

            Assert.IsFalse(
                DatabaseReach.IsMatch(source),
                $"{where[^1]} must go through HeadTypeUtility, which knows what a valid head is."
            );
            Assert.Contains(
                "HeadTypeUtility",
                source,
                $"{where[^1]} still needs to pick a head somehow."
            );
        }
    }
}
