using System;
using System.IO;
using Cosmere.Core.UI.Dock;
using Cosmere.System.Scadrial.Feruchemy;
using Cosmere.System.Scadrial.UI.Feruchemy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     How many extra rows the duralumin ledger strip adds, exercised through the primitive
///     overload so no live Feruchemist gene is required.
/// </summary>
[TestClass]
public class FeruchemyLedgerRowTests {
    private const float Tolerance = 1e-3f;

    [TestMethod]
    public void NonDuraluminGetsNoExtraHeight() {
        Assert.AreEqual(0f, FeruchemyLedgerRow.HeightFor(false, DuraluminLedger.Residence), Tolerance);
        Assert.AreEqual(0f, FeruchemyLedgerRow.HeightFor(false, DuraluminLedger.Shard), Tolerance);
    }

    [TestMethod]
    public void NonShardTieGetsOneRow() {
        Assert.AreEqual(DockDropdownRow.Height, FeruchemyLedgerRow.HeightFor(true, DuraluminLedger.Residence), Tolerance);
    }

    [TestMethod]
    public void PersonalBondsAreNotASelectableLedger() {
        string row = File.ReadAllText(
            Path.Combine(
                RepoRoot,
                "CosmereCore",
                "CosmereCore",
                "System",
                "Scadrial",
                "UI",
                "Feruchemy",
                "FeruchemyLedgerRow.cs"
            )
        );
        string gene = File.ReadAllText(
            Path.Combine(
                RepoRoot,
                "CosmereCore",
                "CosmereCore",
                "System",
                "Scadrial",
                "Gene",
                "Feruchemist.cs"
            )
        );
        string language = File.ReadAllText(
            Path.Combine(RepoRoot, "CosmereScadrial", "Languages", "English", "Keyed", "Feruchemy.xml")
        );
        string feruchemy = Path.Combine(RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "Feruchemy");

        Assert.IsFalse(row.Contains("LedgerOption(gene, DuraluminLedger.Bonds)", StringComparison.Ordinal));
        Assert.IsFalse(gene.Contains("cachedLedger = new BondLedger()", StringComparison.Ordinal));
        Assert.IsTrue(
            gene.Contains(
                "if (targetLedger != DuraluminLedger.Residence && targetLedger != DuraluminLedger.Shard)",
                StringComparison.Ordinal
            )
        );
        Assert.IsFalse(File.Exists(Path.Combine(feruchemy, "BondDistribution.cs")));
        Assert.IsFalse(File.Exists(Path.Combine(feruchemy, "Ledger", "BondLedger.cs")));
        Assert.IsFalse(language.Contains("CS_Duralumin_Ledger_Bonds", StringComparison.Ordinal));
        Assert.IsFalse(language.Contains("Personal bonds", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShardTieGetsTwoRowsPlusTheGapBetweenThem() {
        Assert.AreEqual(
            DockDropdownRow.Height * 2f + 6f,
            FeruchemyLedgerRow.HeightFor(true, DuraluminLedger.Shard),
            Tolerance
        );
    }

    [TestMethod]
    public void AResolvedEmptyLedgerExplainsWhyItCannotMove() {
        string gene = File.ReadAllText(
            Path.Combine(
                RepoRoot,
                "CosmereCore",
                "CosmereCore",
                "System",
                "Scadrial",
                "Gene",
                "Feruchemist.cs"
            )
        );
        string language = File.ReadAllText(
            Path.Combine(RepoRoot, "CosmereScadrial", "Languages", "English", "Keyed", "Feruchemy.xml")
        );

        Assert.IsTrue(gene.Contains("RefreshConnectionBudget();", StringComparison.Ordinal));
        Assert.IsFalse(
            gene.Contains("if (!StoresConnection || SelectedLedger != null) return null;", StringComparison.Ordinal),
            "A resolved ledger must reach its zero-budget check."
        );
        Assert.IsTrue(
            gene.Contains("connectionStorable > 0f || connectionTappable > 0f", StringComparison.Ordinal),
            "A resolved ledger must stay blocked only when neither direction can move."
        );
        Assert.IsTrue(gene.Contains("CS_Duralumin_LedgerEmpty", StringComparison.Ordinal));
        Assert.IsTrue(gene.Contains("tie.Named(\"TIE\")", StringComparison.Ordinal));
        Assert.IsTrue(gene.Contains("DuraluminLedger.Residence => \"CS_Duralumin_Ledger_Residence\"", StringComparison.Ordinal));
        Assert.IsTrue(gene.Contains("GetNamedSilentFail(targetShardDefName)?.LabelCap", StringComparison.Ordinal));
        Assert.IsTrue(language.Contains("<CS_Duralumin_LedgerEmpty>{TIE}:", StringComparison.Ordinal));
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
