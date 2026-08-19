using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

[TestClass]
public class FeruchemyDockSectionTests {
    [TestMethod]
    public void CompoundToggleHeightFollowsTheDrawPredicate() {
        string dock = Source("UI", "FeruchemyDockSection.cs");
        string dial = Source("UI", "Feruchemy", "FeruchemyDialWidget.cs");

        Assert.IsTrue(
            dial.Contains("internal static bool ShowsCompoundToggle(Feruchemist gene)", StringComparison.Ordinal)
        );
        Assert.IsTrue(dial.Contains("=> gene.TargetIsInternalOnly;", StringComparison.Ordinal));
        Assert.IsTrue(dial.Contains("bool eligible = ShowsCompoundToggle(gene);", StringComparison.Ordinal));
        Assert.IsFalse(dial.Contains("bool eligible = gene.TargetIsInternalOnly;", StringComparison.Ordinal));
        Assert.IsTrue(
            dock.Contains(
                "FeruchemyDialWidget.ShowsCompoundToggle(gene) ? StripButtonHeight : 0f",
                StringComparison.Ordinal
            )
        );
    }

    private static string Source(params string[] parts) {
        return File.ReadAllText(
            Path.Combine([RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", .. parts])
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
