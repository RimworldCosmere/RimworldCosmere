using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Absorption separates what leaves the source from what reaches the pawn. Multiplying the
///     wrong one either drains spheres twice as fast or does nothing at all.
/// </summary>
[TestClass]
public class InvestitureAbsorptionTests {
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

    private static string Holder => File.ReadAllText(Path.Combine(
        RepoRoot, "CosmereCore", "CosmereCore", "Core", "Comp", "Thing", "InvestitureHolder.cs"
    ));

    /// <summary>
    ///     The source loses the raw draw and the pawn receives it multiplied. Swapping these makes
    ///     an efficiency boon into a drain penalty.
    /// </summary>
    [TestMethod]
    public void TheSourceLosesTheDrawAndThePawnReceivesItMultiplied() {
        string src = Holder;

        Assert.IsTrue(
            src.Contains("currentInvestitureSelf += drawn * efficiency / parent.stackCount", StringComparison.Ordinal),
            "The pawn receives the multiplied amount."
        );
        Assert.IsTrue(
            src.Contains("thingInvestiture.currentInvestitureSelf -= drawn / thing.stackCount", StringComparison.Ordinal),
            "The source loses the raw draw, never the multiplied one."
        );
    }

    /// <summary>
    ///     Dividing the free space by the multiplier is what stops a boosted pawn wasting half a
    ///     sphere when topping up.
    /// </summary>
    [TestMethod]
    public void TheDrawIsCappedBySpaceDividedByEfficiency() {
        Assert.IsTrue(
            Holder.Contains(
                "(maxInvestitureSelfStack - currentInvestitureSelfStack) / efficiency",
                StringComparison.Ordinal
            ),
            "Capping by raw space discards the overflow instead of drawing less."
        );
    }

    /// <summary>
    ///     Only pawns carry the stat, and a zero would divide the draw to infinity.
    /// </summary>
    [TestMethod]
    public void NonPawnsTradeOneForOneAndTheMultiplierCannotReachZero() {
        string src = Holder;

        Assert.IsTrue(src.Contains("if (parent is not Pawn pawn) return 1f;", StringComparison.Ordinal));
        Assert.IsTrue(
            src.Contains("Mathf.Max(0.01f, pawn.GetStatValue(StatDefOf.Cosmere_InvestitureAbsorption))", StringComparison.Ordinal),
            "A zero multiplier would divide the cap by zero."
        );
    }

    /// <summary>
    ///     The stat has to exist and default to 1, or every absorption in the game changes.
    /// </summary>
    [TestMethod]
    public void TheStatExistsAndDefaultsToNoChange() {
        XElement stat = XDocument
            .Load(Path.Combine(RepoRoot, "CosmereCore", "Defs", "Stats.xml"))
            .Root!
            .Elements("StatDef")
            .Single(e => (string?)e.Element("defName") == "Cosmere_InvestitureAbsorption");

        Assert.AreEqual("1", (string?)stat.Element("defaultBaseValue"), "Default must be a no-op.");
        Assert.AreEqual("0.01", (string?)stat.Element("minValue"), "Zero would divide the draw cap by zero.");
    }

    /// <summary>
    ///     The boons that promise Stormlight capacity now deliver efficiency instead, tiered 1-3.
    /// </summary>
    [TestMethod]
    public void TheFourBoonsCarryTheirTierMultiplier() {
        XDocument hediffs = XDocument.Load(Path.Combine(
            RepoRoot, "CosmereRoshar", "Defs", "Nightwatcher", "HediffDefs.xml"
        ));

        (string defName, string expected)[] wanted = [
            ("Cosmere_Roshar_Hediff_NW_BoonPassive_Touched", "1.15"),
            ("Cosmere_Roshar_Hediff_NW_BoonPassive_Investiture", "1.5"),
            ("Cosmere_Roshar_Hediff_NW_BoonPassive_Lifelight", "2.0"),
            ("Cosmere_Roshar_Hediff_NW_BoonPassive_AwokenMind", "2.0"),
        ];

        foreach ((string defName, string expected) in wanted) {
            XElement def = hediffs.Root!
                .Elements("HediffDef")
                .Single(e => (string?)e.Element("defName") == defName);

            string? factor = def
                .Descendants("statFactors")
                .Elements("Cosmere_InvestitureAbsorption")
                .Select(e => e.Value)
                .SingleOrDefault();

            Assert.AreEqual(expected, factor, $"{defName} carries the wrong multiplier.");
        }
    }

    /// <summary>
    ///     investitureBonus raised a ceiling that Surgebinder reassigned on every ideal change, so
    ///     it never survived. Nothing should reintroduce it.
    /// </summary>
    [TestMethod]
    public void TheBrokenCeilingBonusIsGone() {
        string[] roots = [
            Path.Combine(RepoRoot, "CosmereCore"),
            Path.Combine(RepoRoot, "CosmereRoshar"),
        ];

        foreach (string root in roots) {
            foreach (string file in Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)) {
                if (!file.EndsWith(".cs", StringComparison.Ordinal) && !file.EndsWith(".xml", StringComparison.Ordinal)) continue;
                if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")) continue;

                Assert.IsFalse(
                    File.ReadAllText(file).Contains("investitureBonus", StringComparison.Ordinal),
                    $"{Path.GetRelativePath(RepoRoot, file)} still references investitureBonus."
                );
            }
        }
    }
}
