using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     A quadrant heading in the allomancy dock folds its own metals away.
/// </summary>
/// <remarks>
///     No Unity or Verse loads here, so this guards the source instead.
/// </remarks>
[TestClass]
public class MetalGroupFoldTests {
    private static string UISource(string file) {
        DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
            dir = dir.Parent;
        }

        Assert.IsNotNull(dir, "Could not locate CosmereScadrial/Defs above the test output directory.");

        return File.ReadAllText(Path.Combine(dir.FullName, "CosmereCore", "CosmereCore", "System", "Scadrial", "UI", file));
    }

    private static string DockSource(string file) {
        DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
            dir = dir.Parent;
        }

        Assert.IsNotNull(dir, "Could not locate CosmereScadrial/Defs above the test output directory.");

        return File.ReadAllText(Path.Combine(dir.FullName, "CosmereCore", "CosmereCore", "Core", "UI", "Dock", file));
    }

    [TestMethod]
    public void FoldStateLivesOnTheSharedBase() {
        string source = DockSource("DockSectionBase.cs");
        Assert.Contains("collapsedGroups", source, "Both dock sections must share one fold store.");
        Assert.Contains("IsGroupCollapsed", source);
        Assert.Contains("ToggleGroupFold", source);
    }

    [TestMethod]
    public void AllomancyKeepsPerGroupFoldState() {
        string source = UISource("AllomancyDockSection.cs");
        Assert.Contains("IsCollapsed", source, "Fold state must reach both the height pass and the draw pass.");
        Assert.Contains("ToggleGroup", source);
    }

    [TestMethod]
    public void FoldingAGroupClosesAMetalOpenInsideIt() {
        string source = UISource("AllomancyDockSection.cs");
        Assert.Contains("if (expandedMetal == id) expandedMetal = null;", source);
        Assert.Contains("if (pendingMetal == id) pendingMetal = null;", source);
    }

    [TestMethod]
    public void FoldedGroupSkipsItsTilesAndSaysWhatIsHidden() {
        string source = UISource("MetallicArtsTable.cs");
        Assert.Contains("CC_Dock_Group_Folded", source, "A folded group must summarise its metal count.");
        Assert.Contains("Widgets.ButtonInvisible(headerRect)", source, "The heading must be the fold control.");
        Assert.Contains("if (folded) {", source, "Height and draw must both skip a folded group's tiles.");
    }

    [TestMethod]
    public void FoldedGroupKeyExists() {
        DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
            dir = dir.Parent;
        }

        Assert.IsNotNull(dir);
        string xml = File.ReadAllText(Path.Combine(dir.FullName, "CosmereCore", "Languages", "English", "Keyed", "Dock.xml"));
        Assert.Contains("<CC_Dock_Group_Folded>", xml);
    }
}
