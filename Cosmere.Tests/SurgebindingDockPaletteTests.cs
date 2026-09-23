using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The Surgebinding dock section reads parchment like the rest of the dock. Only the
///     Stormlight gauge and the current oath stay blue; state is weight, colour and spacing.
/// </summary>
[TestClass]
public class SurgebindingDockPaletteTests {
    private static string Source {
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
                "Roshar",
                "UI",
                "SurgebindingDockSection.cs"
            ));
        }
    }

    [TestMethod]
    public void NoOutlinedBoxes() {
        Assert.IsFalse(
            Source.Contains("DrawBoxSolidWithOutline"),
            "Surge state is text weight and hover fill, not a bordered box."
        );
    }

    [TestMethod]
    public void NoNotchedTop() {
        Assert.IsFalse(
            Source.Contains("DrawNotchedTop"),
            "The open surge cell draws no panel, so the strip has nothing to join through a notch."
        );
    }

    [TestMethod]
    public void NoAccentWash() {
        Assert.IsFalse(Source.Contains("ActiveWash"), "Accent washes are gone from the dock.");
    }

    [TestMethod]
    public void NoSecondaryBlue() {
        Assert.IsFalse(
            Source.Contains("0.302f, 0.396f, 0.478f"),
            "The secondary blue accent was replaced by the dock's own accent."
        );
    }

    [TestMethod]
    public void AbilityIconsAreTwenty() {
        Assert.IsTrue(
            Regex.IsMatch(Source, @"AbilityIconSize\s*=\s*20f"),
            "Ability marks sit at 20px beside tiny text."
        );
    }

    [TestMethod]
    public void OathLadderIsDrawnAndMeasured() {
        Assert.IsTrue(
            Source.Contains("OathLadder.Draw("),
            "The oath ladder has to be drawn, not just written."
        );
        Assert.IsTrue(
            Source.Contains("+ OathLadderLayout.Height"),
            "The expanded body has to reserve room for the ladder or it draws over the caption."
        );
    }

    [TestMethod]
    public void BlueOnlySurvivesOnTheGaugeAndTheOath() {
        Assert.IsFalse(
            Source.Contains("Skin.AccentColor"),
            "The shard accent is blue here; labels and buttons take the dock's parchment accent."
        );
        Assert.IsFalse(
            Source.Contains("Skin.HeaderTextColor"),
            "The order tab reads parchment like every other dock header."
        );
        Assert.IsTrue(
            Source.Contains("Skin.BarFillColor"),
            "Blue still fills the Stormlight gauge and the oath ladder's current dot."
        );
    }

    [TestMethod]
    public void CeilingCaptionIsARightAlignedAside() {
        int ceiling = Source.IndexOf("CeilingCaption(gene, order)", StringComparison.Ordinal);
        Assert.IsTrue(ceiling > 0, "The ceiling sentence is still shown.");
        Assert.IsTrue(
            Source.IndexOf("TextAnchor.MiddleRight", ceiling, StringComparison.Ordinal)
            < Source.IndexOf("TextAnchor.MiddleLeft", ceiling, StringComparison.Ordinal),
            "The ceiling sentence sits right of the refill note, not as the primary caption."
        );
    }
}
