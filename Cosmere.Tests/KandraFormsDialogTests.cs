using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     A missing key prints its own name on screen, and a colour literal in a dialog is how the
///     dock palette drifts apart one window at a time.
/// </summary>
[TestClass]
public partial class KandraFormsDialogTests {
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

    [TestMethod]
    public void EveryKeyTheDialogTranslatesIsDefined() {
        HashSet<string> defined = [];
        string keyed = Path.Combine(RepoRoot, "CosmereScadrial", "Languages", "English", "Keyed");

        foreach (string file in Directory.GetFiles(keyed, "*.xml")) {
            foreach (XElement entry in XDocument.Load(file).Root!.Elements()) defined.Add(entry.Name.LocalName);
        }

        foreach (Match used in TranslateKeyPattern.Matches(DialogSource)) {
            Assert.Contains(
                used.Groups[1].Value,
                defined,
                $"{used.Groups[1].Value} is translated by the dialog but defined nowhere, so it prints raw."
            );
        }
    }

    [TestMethod]
    public void TheDialogUsesTheSharedWindowChromeRatherThanItsOwn() {
        string source = DialogSource;

        Assert.Contains("BaseWindow", source, "The dialog must share BaseWindow's chrome.");
        Assert.Contains("CTAButtonText", source, "Buttons must be the ones the radiant dialogs use.");
        Assert.Contains("DrawMenuSection", source, "Panels must be vanilla menu sections.");

        // PanelRaised is a mouseover wash on small rows. As a surface fill it reads as mud.
        Assert.IsFalse(
            source.Contains("DockPalette", StringComparison.Ordinal),
            "The dock palette is sized for hover tints on tiles, not for a window's surfaces."
        );
    }

    [TestMethod]
    public void TheCallToActionStaysRareEnoughToMeanSomething() {
        string source = DialogSource;

        int cta = CtaButtonPattern.Matches(source).Count;
        int plain = PlainButtonPattern.Matches(source).Count;

        // One CTA per committing state: revert, reshape, adopt. No more than that.
        Assert.IsLessThanOrEqualTo(
            3,
            cta,
            $"{cta} CTA buttons against three committing states. The atlas is for the action that commits, not for every control."
        );
        Assert.IsGreaterThanOrEqualTo(
            cta,
            plain,
            $"{plain} plain buttons against {cta} CTAs. The loud one should be the exception."
        );
    }

    [TestMethod]
    public void PromotingATrueFormAsksBeforeItActs() {
        string source = DialogSource;

        Assert.Contains("AdoptTrueBody", source, "The dialog no longer adopts a true form.");
        Assert.Contains("DrawConfirm", source, "The confirmation step is gone, so the swap happens on one click.");

        Assert.Contains(
            "CS_Kandra_AdoptKeep",
            source,
            "The confirmation has no way to back out."
        );

        int buttons = source.IndexOf("private void DrawRailButtons", StringComparison.Ordinal);
        string rail = source[buttons..source.IndexOf("\n    }", buttons, StringComparison.Ordinal)];

        // Changing the body underneath is independent of whatever face is on top of it.
        Assert.IsFalse(
            rail.Contains("CS_Kandra_ChangeTrue_Wearing", StringComparison.Ordinal),
            "Change true form is gated on wearing a disguise again. The two are unrelated."
        );

        int card = source.IndexOf("private void DrawCard", StringComparison.Ordinal);
        string draw = source[card..source.IndexOf("\n    }", card, StringComparison.Ordinal)];

        Assert.Contains(
            "if (confirming != null) return;",
            draw,
            "Cards stay clickable behind the confirmation, so a stray click wears a form instead."
        );
    }

    [TestMethod]
    public void TheOldTrueBodyStaysWearable() {
        string comp = File.ReadAllText(
            Path.Combine(
                RepoRoot,
                "CosmereCore",
                "CosmereCore",
                "System",
                "Scadrial",
                "Kandra",
                "CompKandraForms.cs"
            )
        );

        int start = comp.IndexOf("public void AdoptTrueBody", StringComparison.Ordinal);
        Assert.IsGreaterThanOrEqualTo(0, start, "CompKandraForms no longer adopts a true body.");

        string body = comp[start..comp.IndexOf("\n    }", start, StringComparison.Ordinal)];

        // Changing which shape is home does not destroy the old one. It is still a face it owns.
        Assert.Contains(
            "known.Add(trueBody)",
            body,
            "The old true body is dropped instead of returning to the repertoire."
        );
    }

    [TestMethod]
    public void TheAdoptCopyPromisesNothingTheRulesDoNotDo() {
        string keyed = File.ReadAllText(
            Path.Combine(
                RepoRoot,
                "CosmereScadrial",
                "Languages",
                "English",
                "Keyed",
                "Gizmo.xml"
            )
        );

        int start = keyed.IndexOf("<CS_Kandra_AdoptBody>", StringComparison.Ordinal);
        string copy = keyed[start..keyed.IndexOf("</CS_Kandra_AdoptBody>", start, StringComparison.Ordinal)];

        foreach (string invented in new[] { "year", "unmade", "give up", "cannot be undone" }) {
            Assert.IsFalse(
                copy.Contains(invented, StringComparison.OrdinalIgnoreCase),
                $"The adopt copy claims '{invented}'. Adopting a form costs nothing and is reversible."
            );
        }
    }

    [TestMethod]
    public void ReturningToTheTrueFormGoesThroughTheJobRatherThanSnapping() {
        string source = DialogSource;

        Assert.IsFalse(
            source.Contains("KandraShapeshift.Revert", StringComparison.Ordinal),
            "The dialog reverts directly again, which skips the ten-second job the gizmo uses."
        );

        Assert.Contains(
            "KandraChangeShape.RevertIndex",
            source,
            "Reverting must queue the same job the revert gizmo queues."
        );
    }

    [TestMethod]
    public void TheCraftedBodyPortraitDoesNotUseADressedStandIn() {
        string source = DialogSource;

        int start = source.IndexOf("private void DrawPortrait", StringComparison.Ordinal);
        Assert.IsGreaterThanOrEqualTo(0, start, "The dialog no longer draws portraits.");

        string body = source[start..source.IndexOf("\n    }", start, StringComparison.Ordinal)];

        // PortraitPawn generates a colonist, and a generated colonist arrives wearing clothes.
        Assert.Contains(
            "crafted: true",
            body,
            "The crafted body falls through to PortraitPawn, so it shows a dressed human."
        );

        int draw = source.IndexOf("private void DrawTrueBody", StringComparison.Ordinal);
        Assert.IsGreaterThanOrEqualTo(0, draw, "The dialog no longer draws the crafted body itself.");

        string trueBody = source[draw..source.IndexOf("\n    }", draw, StringComparison.Ordinal)];

        // Body and head are separate files. Drawing only the body leaves a headless torso.
        Assert.Contains("TrueBody", trueBody, "The crafted body portrait draws no body.");
        Assert.Contains("TrueHead", trueBody, "The crafted body portrait draws no head.");
        Assert.Contains(
            "headOffset",
            trueBody,
            "The head is placed by a made-up constant rather than the body type's own offset."
        );
        Assert.Contains("HairTexture", trueBody, "The crafted body portrait draws no hair.");

        // ContentFinder inside a draw call is a per-frame disk lookup. GraphicDatabase caches.
        Assert.IsFalse(
            trueBody.Contains("ContentFinder", StringComparison.Ordinal),
            "The crafted body portrait reaches ContentFinder on a draw call."
        );
    }

    [TestMethod]
    public void AShapeCarryingTheKandrasOwnNameReadsAsNoDisguise() {
        string comp = File.ReadAllText(
            Path.Combine(
                RepoRoot,
                "CosmereCore",
                "CosmereCore",
                "System",
                "Scadrial",
                "Kandra",
                "CompKandraForms.cs"
            )
        );

        int start = comp.IndexOf("public string? WornName", StringComparison.Ordinal);
        Assert.IsGreaterThanOrEqualTo(0, start, "CompKandraForms no longer exposes WornName.");

        string body = comp[start..comp.IndexOf("\n    }", start, StringComparison.Ordinal)];

        Assert.Contains(
            "worn == own",
            body,
            "WornName reports the kandra's own name back as an impersonation of itself."
        );
    }

    [TestMethod]
    public void AnAnimalTrueBodyStillDrawsWhenNoDisguiseIsWorn() {
        string node = File.ReadAllText(
            Path.Combine(
                RepoRoot,
                "CosmereCore",
                "CosmereCore",
                "System",
                "Scadrial",
                "Kandra",
                "PawnRenderNode_KandraShape.cs"
            )
        );

        int start = node.IndexOf("public static PawnKindDef? WornKind", StringComparison.Ordinal);
        string body = node[start..node.IndexOf("\n    }", start, StringComparison.Ordinal)];

        // Revert nulls Current. Reading only Current drew a human over a kandra that is a dog.
        Assert.Contains(
            "TrueBody?.animalKind",
            body,
            "WornKind ignores an animal true body, so reverting to it renders the pawn as a person."
        );
    }

    [TestMethod]
    public void AdoptingAnEatenShapeStopsTheGreyTrueBodyRendering() {
        string appearance = File.ReadAllText(
            Path.Combine(
                RepoRoot,
                "CosmereCore",
                "CosmereCore",
                "System",
                "Scadrial",
                "Util",
                "KandraAppearance.cs"
            )
        );

        int start = appearance.IndexOf("public static bool IsFormless", StringComparison.Ordinal);
        string body = appearance[start..appearance.IndexOf("\n    }", start, StringComparison.Ordinal)];

        // The flag rides the form, so wearing the crafted body reads grey the same as being it.
        Assert.Contains(
            "crafted",
            body,
            "IsFormless ignores the crafted flag, so putting the crafted body back on draws a human."
        );

        Assert.Contains(
            "Current.crafted",
            body,
            "IsFormless only checks the true body, so wearing the crafted body renders as its stored face."
        );
    }

    [TestMethod]
    public void TheMaterialReadoutShowsTheTranslatedLabelRatherThanTheSaveKey() {
        string source = DialogSource;

        foreach (string method in new[] { "private void DrawMaterial", "private string CurrentTrueLabel" }) {
            int start = source.IndexOf(method, StringComparison.Ordinal);
            Assert.IsGreaterThanOrEqualTo(0, start, $"{method} is gone, so this guard no longer covers anything.");

            string body = source[start..source.IndexOf("\n    }", start, StringComparison.Ordinal)];

            Assert.Contains(
                "MaterialLabel(",
                body,
                $"{method} prints the material's raw save key, so the rail reads 'milky quartzite' "
                + "beside the palette's 'Milky quartzite'."
            );
        }

        int helper = source.IndexOf("private static string MaterialLabel", StringComparison.Ordinal);
        Assert.IsGreaterThanOrEqualTo(0, helper, "The dialog has no shared material label, so each caller resolves its own.");

        string resolve = source[helper..source.IndexOf("\n    }", helper, StringComparison.Ordinal)];

        Assert.Contains("FindMaterial", resolve, "The label is not resolved against the material table.");
        Assert.Contains("labelKey.Translate", resolve, "The label skips the row's key, so it prints untranslated.");
    }

    [TestMethod]
    public void TheCraftedPortraitUsesTheBodysOwnGenderRatherThanTheDisguises() {
        string source = DialogSource;

        int start = source.IndexOf("private void DrawTrueBody", StringComparison.Ordinal);
        Assert.IsGreaterThanOrEqualTo(0, start, "The dialog no longer draws the crafted body itself.");

        string body = source[start..source.IndexOf("\n    }", start, StringComparison.Ordinal)];

        // ApplyTo puts the disguise's gender on the pawn while the true body keeps the real one.
        Assert.IsFalse(
            body.Contains("?? pawn.gender", StringComparison.Ordinal),
            "DrawTrueBody falls back to the pawn's gender, so a disguised kandra sees the wrong "
            + "silhouette in every caller that passes none."
        );

        Assert.Contains(
            "?? form.gender",
            body,
            "DrawTrueBody must default to the form's own gender; three call sites pass none."
        );
    }

    [GeneratedRegex("\"(CS_[A-Za-z0-9_]+)\"")]
    private static partial Regex TranslateKeyPattern { get; }

    [GeneratedRegex(@"CTAButtonText\(")]
    private static partial Regex CtaButtonPattern { get; }

    [GeneratedRegex(@"Widgets\.ButtonText\(")]
    private static partial Regex PlainButtonPattern { get; }
}
