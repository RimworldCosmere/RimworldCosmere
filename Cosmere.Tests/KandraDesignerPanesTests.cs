using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Guards the kandra designer once it runs through tabs: the plate that must not move, the
///     controls that moved off it, and the eye colour that has to survive a round trip through the
///     window without the player touching it.
/// </summary>
[TestClass]
public class KandraDesignerPanesTests {
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

    private static string DialogSource => File.ReadAllText(
        Path.Combine(
            RepoRoot,
            "CosmereCore",
            "CosmereCore",
            "System",
            "Scadrial",
            "Kandra",
            "Dialog_KandraForms.cs"
        )
    );

    /// <summary>The preview is the thing being judged, so flipping a tab may not shift it a pixel.</summary>
    [TestMethod]
    public void ThePreviewPlateIsLaidOutBeforeTheTabStripDecidesAnything() {
        string designer = Body("private void DrawDesigner");

        int plate = designer.IndexOf("DrawPreviewPlate(", StringComparison.Ordinal);
        int strip = designer.IndexOf("DrawTabStrip(", StringComparison.Ordinal);

        Assert.IsTrue(plate >= 0, "The designer no longer draws the preview plate.");
        Assert.IsTrue(strip > plate, "The tab strip is laid out before the plate, so the plate moves per tab.");

        // The window was sized for this layout. Growing it is how the designer stops fitting on screen.
        Match size = Regex.Match(
            DialogSource,
            @"initialWindowSize\s*=>\s*new Vector2\(\s*Spacing\.Get\(65\)\s*,\s*Spacing\.Get\(39\)\s*\)"
        );

        Assert.IsTrue(size.Success, "The designer window is no longer Spacing.Get(65) by Spacing.Get(39).");
    }

    /// <summary>Three panes, one shared two-column shape. A pane that lays out its own plate drifts.</summary>
    [TestMethod]
    public void EveryPaneGoesThroughTheSharedTwoColumnHelperRatherThanItsOwnPlates() {
        string designer = Body("private void DrawDesigner");

        Assert.AreEqual(
            3,
            Regex.Matches(designer, @"DrawColumnPlates\(").Count,
            "The designer no longer lays all three panes out with the shared helper."
        );

        foreach (string pane in new[] { "DrawBuildColumn", "DrawPalette", "DrawHairGrid", "DrawHairDyes", "DrawEyeColours" }) {
            StringAssert.Contains(designer, pane, "The designer never reaches " + pane + ", so that pane never draws.");

            Assert.IsFalse(
                Regex.IsMatch(Body("private void " + pane), @"Widgets\.DrawMenuSection|ContractedBy"),
                pane + " draws a second plate inside the one DrawColumnPlates already drew."
            );
        }
    }

    /// <summary>The gender toggle moved into the Body tab. Left on the plate it is a control per tab.</summary>
    [TestMethod]
    public void TheGenderToggleLivesInTheBodyTabAndNotOnThePlate() {
        Assert.IsFalse(
            Body("private void DrawPreviewPlate").Contains("DrawGenderOption(", StringComparison.Ordinal),
            "The gender toggle is still on the preview plate, so it sits above the tab that owns it."
        );

        string build = Body("private void DrawBuildColumn");

        Assert.AreEqual(
            2,
            Regex.Matches(build, @"DrawGenderOption\(").Count,
            "The Body tab does not offer both genders."
        );

        foreach (string key in new[] { "CS_Kandra_BuildHeader", "CS_Kandra_BuildNote" }) {
            StringAssert.Contains(build, key, "The Body tab is missing " + key + ".");
        }
    }

    /// <summary>
    ///     Opening the designer and accepting without touching anything has to change nothing. Miss
    ///     either half and a kandra loses the eyes it already had the moment it is reshaped.
    /// </summary>
    [TestMethod]
    public void TheEyeColourIsSeededFromTheTrueBodyAndHandedBackToTheReshape() {
        StringAssert.Contains(
            Body("private void BeginDesign"),
            "designEyeColour = body?.eyeColourName",
            "The designer opens with no eye colour, so accepting it blanks the eyes the kandra had."
        );

        Match commit = Regex.Match(DialogSource, @"forms\.BeginReshape\(([^)]*)\)");
        Assert.IsTrue(commit.Success, "The designer no longer commits through BeginReshape.");

        StringAssert.Contains(
            commit.Groups[1].Value,
            "designEyeColour",
            "The picked eye colour never reaches BeginReshape, so the eyes come out unchanged."
        );
    }

    /// <summary>
    ///     PortraitPawn is a plain colonist with no CompKandraForms, so the eye render nodes find no
    ///     form and draw nothing. The preview has to be handed the colour the same way as the dye.
    /// </summary>
    [TestMethod]
    public void ThePreviewTakesTheEyeColourAsAnOverrideRatherThanReadingItOffAPawn() {
        string portrait = Body("private void DrawPortrait");

        StringAssert.Contains(portrait, "string? eyeColour", "DrawPortrait takes no eye colour override.");
        StringAssert.Contains(portrait, "hairColour, eyeColour", "The override never reaches DrawTrueBody.");

        string trueBody = Body("private void DrawTrueBody");

        StringAssert.Contains(
            trueBody,
            "eyeColour ?? form.eyeColourName",
            "The preview ignores the picked colour, or ignores the stored one when nothing is picked."
        );

        StringAssert.Contains(
            trueBody,
            "EyeDrawColorFor",
            "The preview tints the iris with something other than the palette's own draw colour."
        );

        // A kandra's hair hangs over its sockets, so eyes drawn last cover the fringe.
        Assert.IsTrue(
            trueBody.IndexOf("IrisRight", StringComparison.Ordinal)
            < trueBody.IndexOf("HairTexture", StringComparison.Ordinal),
            "The irises draw over the hair."
        );
    }

    /// <summary>A wide iris was cut on 2026-09-19. Any count written here goes stale the same way.</summary>
    [TestMethod]
    public void TheEyeColourListReadsTheTableRatherThanCountingItself() {
        string colours = Body("private void DrawEyeColours");

        StringAssert.Contains(
            colours,
            "KandraAppearance.AllEyeColours",
            "The eye list does not come from the palette table."
        );

        Assert.AreEqual(
            2,
            Regex.Matches(colours, @"stones\.Count").Count,
            "The eye list does not take both its scroll height and its loop bound from the table, so "
            + "retuning the table leaves rows unreachable or scrolls past nothing."
        );
    }

    private static string Body(string signature) {
        string source = DialogSource;
        int start = source.IndexOf(signature, StringComparison.Ordinal);
        Assert.IsTrue(start >= 0, "Dialog_KandraForms no longer declares " + signature + ".");

        int end = source.IndexOf("\n    }", start, StringComparison.Ordinal);
        Assert.IsTrue(end > start, "Could not read the body of " + signature + ".");

        return source[start..end];
    }
}
