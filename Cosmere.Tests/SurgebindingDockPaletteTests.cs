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
public partial class SurgebindingDockPaletteTests {
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

    [GeneratedRegex(@"AbilityIconSize\s*=\s*20f")]
    private static partial Regex AbilityIconSize { get; }

    [TestMethod]
    public void NoOutlinedBoxes() {
        Assert.DoesNotContain(
            "DrawBoxSolidWithOutline",
            Source,
            "Surge state is text weight and hover fill, not a bordered box."
        );
    }

    [TestMethod]
    public void NoNotchedTop() {
        Assert.DoesNotContain(
            "DrawNotchedTop",
            Source,
            "The open surge cell draws no panel, so the strip has nothing to join through a notch."
        );
    }

    [TestMethod]
    public void NoAccentWash() {
        Assert.DoesNotContain("ActiveWash", Source, "Accent washes are gone from the dock.");
    }

    [TestMethod]
    public void NoSecondaryBlue() {
        Assert.DoesNotContain(
            "0.302f, 0.396f, 0.478f",
            Source,
            "The secondary blue accent was replaced by the dock's own accent."
        );
    }

    [TestMethod]
    public void AbilityIconsAreTwenty() {
        Assert.IsTrue(
            AbilityIconSize.IsMatch(Source),
            "Ability marks sit at 20px beside tiny text."
        );
    }

    [TestMethod]
    public void OathLadderIsDrawnAndMeasured() {
        Assert.Contains(
            "OathLadder.Draw(",
            Source,
            "The oath ladder has to be drawn, not just written."
        );
        Assert.Contains(
            "+ OathLadderLayout.Height",
            Source,
            "The expanded body has to reserve room for the ladder or it draws over the caption."
        );
    }

    [TestMethod]
    public void BlueOnlySurvivesOnTheGaugeAndTheOath() {
        Assert.DoesNotContain(
            "Skin.AccentColor",
            Source,
            "The shard accent is blue here; labels and buttons take the dock's parchment accent."
        );
        Assert.DoesNotContain(
            "Skin.HeaderTextColor",
            Source,
            "The order tab reads parchment like every other dock header."
        );
        Assert.Contains(
            "Skin.BarFillColor",
            Source,
            "Blue still fills the Stormlight gauge and the oath ladder's current dot."
        );
    }

    [TestMethod]
    public void CeilingCaptionIsARightAlignedAside() {
        int ceiling = Source.IndexOf("CeilingCaption(gene, order)", StringComparison.Ordinal);
        Assert.IsGreaterThan(0, ceiling, "The ceiling sentence is still shown.");
        Assert.IsLessThan(
            Source.IndexOf("TextAnchor.MiddleLeft", ceiling, StringComparison.Ordinal),
            Source.IndexOf("TextAnchor.MiddleRight", ceiling, StringComparison.Ordinal),
            "The ceiling sentence sits right of the refill note, not as the primary caption."
        );
    }
}
