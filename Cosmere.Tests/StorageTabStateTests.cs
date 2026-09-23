using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     IMGUI has one global text state, so a tab that sets the anchor or the color and walks away
///     leaves every vanilla window drawn after it wearing the change.
/// </summary>
[TestClass]
public class StorageTabStateTests {
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

    private static string Core(params string[] parts) => File.ReadAllText(Path.Combine(
        [RepoRoot, "CosmereCore", "CosmereCore", .. parts]
    ));

    /// <summary>
    ///     The row label needs a left anchor and the gear tab's label color, but it has to hand
    ///     both back. TextBlock restores on dispose; a bare assignment never does.
    /// </summary>
    [TestMethod]
    public void TheStorageTabHandsBackTheTextStateItBorrows() {
        string tab = Core("Core", "Tab", "ITab_StorageWithInventory.cs");

        Assert.IsFalse(tab.Contains("Text.Anchor ="), "A bare anchor assignment bleeds into vanilla.");
        Assert.IsTrue(
            tab.Contains("using (new TextBlock(TextAnchor.MiddleLeft, ITab_Pawn_Gear.ThingLabelColor))"),
            "The label row borrows anchor and color through a TextBlock."
        );
    }
}
