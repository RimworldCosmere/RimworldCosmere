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
        StringAssert.Contains(source, "collapsedGroups", "Both dock sections must share one fold store.");
        StringAssert.Contains(source, "IsGroupCollapsed");
        StringAssert.Contains(source, "ToggleGroupFold");
    }

    [TestMethod]
    public void AllomancyKeepsPerGroupFoldState() {
        string source = UISource("AllomancyDockSection.cs");
        StringAssert.Contains(source, "IsCollapsed", "Fold state must reach both the height pass and the draw pass.");
        StringAssert.Contains(source, "ToggleGroup");
    }

    [TestMethod]
    public void FoldingAGroupClosesAMetalOpenInsideIt() {
        string source = UISource("AllomancyDockSection.cs");
        StringAssert.Contains(source, "if (expandedMetal == id) expandedMetal = null;");
        StringAssert.Contains(source, "if (pendingMetal == id) pendingMetal = null;");
    }

    [TestMethod]
    public void FoldedGroupSkipsItsTilesAndSaysWhatIsHidden() {
        string source = UISource("MetallicArtsTable.cs");
        StringAssert.Contains(source, "CC_Dock_Group_Folded", "A folded group must summarise its metal count.");
        StringAssert.Contains(source, "Widgets.ButtonInvisible(headerRect)", "The heading must be the fold control.");
        StringAssert.Contains(source, "if (folded) {", "Height and draw must both skip a folded group's tiles.");
    }

    [TestMethod]
    public void FoldedGroupKeyExists() {
        DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
            dir = dir.Parent;
        }

        Assert.IsNotNull(dir);
        string xml = File.ReadAllText(Path.Combine(dir.FullName, "CosmereCore", "Languages", "English", "Keyed", "Dock.xml"));
        StringAssert.Contains(xml, "<CC_Dock_Group_Folded>");
    }
}
