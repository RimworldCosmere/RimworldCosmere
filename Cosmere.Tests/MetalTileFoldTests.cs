using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     A metal tile opens into its ability strip on click, and nothing on the closed tile says so.
/// </summary>
/// <remarks>
///     The affordance is a word in the corner, passed in by the caller, so the tile stays ignorant of
///     what it opens into. No Unity or Verse loads here, so this guards the source instead.
/// </remarks>
[TestClass]
public class MetalTileFoldTests {
    private static string TileSource => UISource("MetalTile.cs");

    private static string UISource(string file) {
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
            "UI",
            file
        ));
    }

    [TestMethod]
    public void AllomancyTilePassesAFoldLabel() {
        string source = UISource("AllomancyDockSection.cs");
        Assert.Contains("foldLabel:", source, "The allomancy tile must pass a fold label.");
        Assert.Contains("CC_Dock_Fold_Hide", source);
        Assert.Contains("CC_Dock_Fold_Show", source);
    }

    [TestMethod]
    public void FeruchemyTilePassesAFoldLabel() {
        string source = UISource("FeruchemyDockSection.cs");
        Assert.Contains("CC_Dock_Fold_Hide", source);
        Assert.Contains("CC_Dock_Fold_Show", source);
    }

    [TestMethod]
    public void FoldLabelDoesNotRideTheBandStack() {
        Assert.DoesNotContain(
            "foldY",
            TileSource,
            "The fold word must not get its own row under the note - the band stack owns that space."
        );
        Assert.Contains(
            "Rect foldRect = new Rect(rect.xMax - 5f - foldWidth, textY, foldWidth, tinyH);",
            TileSource,
            "The fold word shares the note row and takes the right edge."
        );
        Assert.Contains(
            "Rect noteRect = new Rect(rect.xMax - 5f - foldWidth - noteWidth, textY, noteWidth, tinyH);",
            TileSource,
            "The note must step left of the fold word, or a compounded readout draws under it."
        );
    }

    [TestMethod]
    public void FoldLabelIsDrawnTinyAndRightAligned() {
        Assert.Contains("TextAnchor.MiddleRight, DockPalette.GroupLabel", TileSource);
    }
}
