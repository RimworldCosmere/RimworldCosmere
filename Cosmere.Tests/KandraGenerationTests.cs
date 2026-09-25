using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     A kandra's age is its generation read a second way, not a separate roll.
/// </summary>
/// <remarks>
///     First generation were made at the Ascension and have been walking around for about a
///     thousand years; tenth generation were made late in the Empire. Rolling age independently
///     put forty-year-old firsts and eight-hundred-year-old tenths in the same colony.
/// </remarks>
[TestClass]
public partial class KandraGenerationTests {
    [GeneratedRegex(@"OldestYears\s*=\s*1000f")]
    private static partial Regex OldestYearsConstant { get; }

    [GeneratedRegex(@"YoungestYears\s*=\s*40f")]
    private static partial Regex YoungestYearsConstant { get; }

    [GeneratedRegex(@"IsHashIntervalTick\([^)]*\)\)\s*ApplyAge\(\)")]
    private static partial Regex ApplyAgeFromTick { get; }

    private static string Source {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate CosmereScadrial/Defs above the test output directory.");

            return File.ReadAllText(Path.Combine(
                dir.FullName,
                "CosmereCore",
                "CosmereCore",
                "System",
                "Scadrial",
                "Gene",
                "KandraHeritage.cs"
            ));
        }
    }

    /// <summary>
    ///     The two anchors. Seniority already runs 1.0 to 0.0 across the ten generations, so a
    ///     lerp between these gives every generation in between.
    /// </summary>
    [TestMethod]
    public void AFirstGenerationKandraIsAboutAThousandYearsOld() {
        string source = Source;

        Assert.IsTrue(OldestYearsConstant.IsMatch(source));
        Assert.IsTrue(YoungestYearsConstant.IsMatch(source));
        Assert.Contains(
            "Mathf.Lerp(YoungestYears, OldestYears, Seniority)",
            source,
            "Age has to come off Seniority, or it is a second roll again."
        );
    }

    /// <summary>
    ///     PostAdd runs partway through pawn generation and anything written to the age tracker
    ///     there is overwritten before the pawn is finished. The quickstart koloss read a plain 24
    ///     for exactly this reason, so the kandra age is enforced from the tick instead.
    /// </summary>
    [TestMethod]
    public void AgeIsEnforcedRatherThanSetOnceDuringGeneration() {
        string source = Source;

        Assert.IsTrue(
            ApplyAgeFromTick.IsMatch(source),
            "ApplyAge must run from the tick, not only from PostAdd."
        );
        Assert.Contains(
            "AgeChronologicalTicks >= target) return",
            source,
            "Raising only, so a kandra never gets younger and an old save corrects itself."
        );
    }

    /// <summary>
    ///     A roll inside ChronologicalYears would give a different answer every tick, and the tick
    ///     enforcement would then rewrite the age forever. Jitter comes off the pawn id instead.
    /// </summary>
    [TestMethod]
    public void TwoKandraOfOneGenerationDifferWithoutRerollingEveryTick() {
        string source = Source;

        Assert.Contains("pawn.thingIDNumber", source, "Jitter must be stable per pawn.");

        int start = source.IndexOf("public float ChronologicalYears", StringComparison.Ordinal);
        int end = source.IndexOf("public override void PostAdd", StringComparison.Ordinal);
        Assert.IsTrue(start >= 0 && end > start);

        Assert.DoesNotContain(
            "Rand.",
            source[start..end],
            "A roll here changes the answer every tick and the enforcement never settles."
        );
    }
}
