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
        StringAssert.Contains(source, "foldLabel:", "The allomancy tile must pass a fold label.");
        StringAssert.Contains(source, "CC_Dock_Fold_Hide");
        StringAssert.Contains(source, "CC_Dock_Fold_Show");
    }

    [TestMethod]
    public void FeruchemyTilePassesAFoldLabel() {
        string source = UISource("FeruchemyDockSection.cs");
        StringAssert.Contains(source, "CC_Dock_Fold_Hide");
        StringAssert.Contains(source, "CC_Dock_Fold_Show");
    }

    [TestMethod]
    public void FoldLabelDoesNotRideTheBandStack() {
        Assert.IsFalse(
            TileSource.Contains("foldY"),
            "The fold word must not get its own row under the note - the band stack owns that space."
        );
        StringAssert.Contains(
            TileSource,
            "Rect foldRect = new Rect(rect.xMax - 5f - foldWidth, textY, foldWidth, tinyH);",
            "The fold word shares the note row and takes the right edge."
        );
        StringAssert.Contains(
            TileSource,
            "Rect noteRect = new Rect(rect.xMax - 5f - foldWidth - noteWidth, textY, noteWidth, tinyH);",
            "The note must step left of the fold word, or a compounded readout draws under it."
        );
    }

    [TestMethod]
    public void FoldLabelIsDrawnTinyAndRightAligned() {
        StringAssert.Contains(TileSource, "TextAnchor.MiddleRight, DockPalette.GroupLabel");
    }
}
