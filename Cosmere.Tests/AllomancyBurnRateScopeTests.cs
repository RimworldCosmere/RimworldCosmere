using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The allomancy dock draws a rate per ability. Reading it off the gene's BurnRate reports the
///     metal's whole drain on every row, so Steel burning two abilities shows each at double.
/// </summary>
[TestClass]
public class AllomancyBurnRateScopeTests {
    private static string Section {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereCore", "CosmereCore"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate CosmereCore above the test output directory.");

            return File.ReadAllText(Path.Combine(
                dir!.FullName,
                "CosmereCore",
                "CosmereCore",
                "System",
                "Scadrial",
                "UI",
                "AllomancyDockSection.cs"
            ));
        }
    }

    private static string CodeOnly(string source) {
        string[] lines = source.Split('\n');
        List<string> kept = [];

        for (int i = 0; i < lines.Length; i++) {
            string trimmed = lines[i].TrimStart();
            if (trimmed.StartsWith("//") || trimmed.StartsWith("///") || trimmed.StartsWith("*")) continue;

            kept.Add(lines[i]);
        }

        return string.Join("\n", kept);
    }

    [TestMethod]
    public void RateIsNotTheMetalsTotal() {
        Assert.IsFalse(
            CodeOnly(Section).Contains("gene.BurnRate"),
            "gene.BurnRate sums every burning ability of the metal - a per-ability row must not use it."
        );
    }

    [TestMethod]
    public void RateIsScopedToOneAbility() {
        string code = CodeOnly(Section);

        Assert.IsTrue(
            code.Contains("BurnReservePercentPerSecond(Pawn pawn, InvestitureCell cell, InvestitureAbility ability)"),
            "The rate readout has to take the ability whose drain it is reporting."
        );
        Assert.IsFalse(
            code.Contains("BurnReservePercentPerSecond(pawn, cell)"),
            "Every call site has to pass its own ability, not just the metal cell."
        );
    }

    /// <summary>
    ///     Scoping the rate to an ability left the remainder silent: a koloss hold drains through an
    ///     ability whose status reads idle, so the reserve fell with no row, no rate and an idle tile.
    /// </summary>
    [TestMethod]
    public void DrainNoActiveAbilityClaimsStillShows() {
        string code = CodeOnly(Section);

        Assert.IsTrue(
            code.Contains("UnclaimedDrainCount(pawn, rows[r].Cell)"),
            "The pinned strip has to reserve a row for every drain no burning ability accounts for."
        );
        Assert.IsTrue(
            code.Contains("DrawUnclaimedRows(rect, y, pawn, cell)"),
            "Reserving the height is not enough - the unclaimed rows have to be drawn."
        );
        Assert.IsTrue(
            code.Contains("cell.IsActive || UnclaimedDrainCount(pawn, cell) > 0"),
            "A metal being drained must not read Idle just because no ability reports active."
        );
    }

    [TestMethod]
    public void AbilityRowShowsARateItHasNoActiveFlagFor() {
        Assert.IsFalse(
            CodeOnly(Section).Contains("aside: ability.IsActive"),
            "The rate aside is gated on there being a rate, not on the ability's active flag."
        );
    }
}
