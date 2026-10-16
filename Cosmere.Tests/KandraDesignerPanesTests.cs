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
public partial class KandraDesignerPanesTests {
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

    [GeneratedRegex(@"InitialWindowSize\s*=>\s*new Vector2\(\s*Spacing\.Get\(65\)\s*,\s*Spacing\.Get\(39\)\s*\)")]
    private static partial Regex DesignerWindowSize { get; }

    [GeneratedRegex(@"DrawColumnPlates\(")]
    private static partial Regex ColumnPlatesCall { get; }

    [GeneratedRegex(@"Widgets\.DrawMenuSection|ContractedBy")]
    private static partial Regex SecondPlate { get; }

    [GeneratedRegex(@"DrawGenderOption\(")]
    private static partial Regex GenderOptionCall { get; }

    [GeneratedRegex(@"forms\.BeginReshape\(([^)]*)\)")]
    private static partial Regex BeginReshapeCall { get; }

    [GeneratedRegex(@"stones\.Count")]
    private static partial Regex StonesCountRead { get; }

    /// <summary>The preview is the thing being judged, so flipping a tab may not shift it a pixel.</summary>
    [TestMethod]
    public void ThePreviewPlateIsLaidOutBeforeTheTabStripDecidesAnything() {
        string designer = Body("private void DrawDesigner");

        int plate = designer.IndexOf("DrawPreviewPlate(", StringComparison.Ordinal);
        int strip = designer.IndexOf("DrawTabStrip(", StringComparison.Ordinal);

        Assert.IsGreaterThanOrEqualTo(0, plate, "The designer no longer draws the preview plate.");
        Assert.IsGreaterThan(
            plate,
            strip,
            "The tab strip is laid out before the plate, so the plate moves per tab."
        );

        // The window was sized for this layout. Growing it is how the designer stops fitting on screen.
        Assert.IsTrue(
            DesignerWindowSize.IsMatch(DialogSource),
            "The designer window is no longer Spacing.Get(65) by Spacing.Get(39)."
        );
    }

    /// <summary>Three panes, one shared two-column shape. A pane that lays out its own plate drifts.</summary>
    [TestMethod]
    public void EveryPaneGoesThroughTheSharedTwoColumnHelperRatherThanItsOwnPlates() {
        string designer = Body("private void DrawDesigner");

        Assert.HasCount(
            3,
            ColumnPlatesCall.Matches(designer),
            "The designer no longer lays all three panes out with the shared helper."
        );

        foreach (string pane in new[] { "DrawBuildColumn", "DrawPalette", "DrawHairGrid", "DrawHairDyes", "DrawEyeColours" }) {
            Assert.Contains(pane, designer, "The designer never reaches " + pane + ", so that pane never draws.");

            Assert.IsFalse(
                SecondPlate.IsMatch(Body("private void " + pane)),
                pane + " draws a second plate inside the one DrawColumnPlates already drew."
            );
        }
    }

    /// <summary>The gender toggle moved into the Body tab. Left on the plate it is a control per tab.</summary>
    [TestMethod]
    public void TheGenderToggleLivesInTheBodyTabAndNotOnThePlate() {
        Assert.DoesNotContain(
            "DrawGenderOption(",
            Body("private void DrawPreviewPlate"),
            StringComparison.Ordinal,
            "The gender toggle is still on the preview plate, so it sits above the tab that owns it."
        );

        string build = Body("private void DrawBuildColumn");

        Assert.HasCount(
            2,
            GenderOptionCall.Matches(build),
            "The Body tab does not offer both genders."
        );

        foreach (string key in new[] { "CS_Kandra_BuildHeader", "CS_Kandra_BuildNote" }) {
            Assert.Contains(key, build, "The Body tab is missing " + key + ".");
        }
    }

    /// <summary>
    ///     Opening the designer and accepting without touching anything has to change nothing. Miss
    ///     either half and a kandra loses the eyes it already had the moment it is reshaped.
    /// </summary>
    [TestMethod]
    public void TheEyeColourIsSeededFromTheTrueBodyAndHandedBackToTheReshape() {
        Assert.Contains(
            "designEyeColour = body?.eyeColourName",
            Body("private void BeginDesign"),
            "The designer opens with no eye colour, so accepting it blanks the eyes the kandra had."
        );

        Match commit = BeginReshapeCall.Match(DialogSource);
        Assert.IsTrue(commit.Success, "The designer no longer commits through BeginReshape.");

        Assert.Contains(
            "designEyeColour",
            commit.Groups[1].Value,
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

        Assert.Contains("string? eyeColour", portrait, "DrawPortrait takes no eye colour override.");
        Assert.Contains("hairColour, eyeColour", portrait, "The override never reaches DrawTrueBody.");

        string trueBody = Body("private void DrawTrueBody");

        Assert.Contains(
            "eyeColour ?? form.eyeColourName",
            trueBody,
            "The preview ignores the picked colour, or ignores the stored one when nothing is picked."
        );

        // The tint moved into DrawIris, which draws the cutout and the bloom the way a pawn does.
        Assert.Contains(
            "DrawIris(",
            trueBody,
            "The preview tints the iris itself again rather than drawing it the way the pawn does."
        );

        // A kandra's hair hangs over its sockets, so eyes drawn last cover the fringe.
        Assert.IsLessThan(
            trueBody.IndexOf("HairTexture", StringComparison.Ordinal),
            trueBody.IndexOf("IrisRight", StringComparison.Ordinal),
            "The irises draw over the hair."
        );
    }

    /// <summary>A wide iris was cut on 2026-09-19. Any count written here goes stale the same way.</summary>
    [TestMethod]
    public void TheEyeColourListReadsTheTableRatherThanCountingItself() {
        string colours = Body("private void DrawEyeColours");

        Assert.Contains(
            "KandraAppearance.AllEyeColours",
            colours,
            "The eye list does not come from the palette table."
        );

        Assert.IsGreaterThanOrEqualTo(
            2,
            StonesCountRead.Matches(colours).Count,
            "The eye list does not take both its scroll height and its loop bound from the table, so "
            + "retuning the table leaves rows unreachable or scrolls past nothing."
        );

        Assert.Contains(
            "CS_Kandra_RightEyeHeader",
            colours,
            "The second eye's list is not in this pane. Ka put both lists on the left on 2026-09-20, "
            + "so a player picks the two stones side by side instead of across the window."
        );

        Assert.Contains(
            "odd ?",
            colours,
            "The pane's scroll height ignores the second list, which appears and disappears with the "
            + "odd eyes toggle, so the bar is wrong whenever it is on."
        );
    }

    private static string Body(string signature) {
        string source = DialogSource;
        int start = source.IndexOf(signature, StringComparison.Ordinal);
        Assert.IsGreaterThanOrEqualTo(0, start, "Dialog_KandraForms no longer declares " + signature + ".");

        int end = source.IndexOf("\n    }", start, StringComparison.Ordinal);
        Assert.IsGreaterThan(start, end, "Could not read the body of " + signature + ".");

        return source[start..end];
    }
}
