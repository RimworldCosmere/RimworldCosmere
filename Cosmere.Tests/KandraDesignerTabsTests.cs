using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>Guards the kandra designer's tab strip: its labels, its click handling and its colours.</summary>
[TestClass]
public class KandraDesignerTabsTests {
    private static readonly string[] TabKeys = ["CS_Kandra_Tab_Body", "CS_Kandra_Tab_Hair", "CS_Kandra_Tab_Eyes"];

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

    private static string TabsSourcePath => Path.Combine(
        RepoRoot,
        "CosmereCore",
        "CosmereCore",
        "System",
        "Scadrial",
        "Kandra",
        "Dialog_KandraForms.Tabs.cs"
    );

    private static string GeneralKeyedPath => Path.Combine(
        RepoRoot,
        "CosmereScadrial",
        "Languages",
        "English",
        "Keyed",
        "General.xml"
    );

    /// <summary>A tab whose label is written in the window is a tab no translation can reach.</summary>
    [TestMethod]
    public void EveryTabTakesItsLabelFromAKeyThatIsActuallyDefined() {
        string source = File.ReadAllText(TabsSourcePath);
        XElement root = XDocument.Load(GeneralKeyedPath).Root!;

        foreach (string key in TabKeys) {
            StringAssert.Contains(source, key, "The tab strip no longer labels itself from " + key + ".");

            Assert.IsNotNull(
                root.Element(key),
                key + " is referenced by the tab strip but missing from General.xml, so the player reads the key."
            );
        }
    }

    /// <summary>
    ///     The strip is drawn by hand, so the click, the hover and the sound all have to be asked for.
    ///     A ButtonText stacked over that styling would draw a second, vanilla-looking button on top.
    /// </summary>
    [TestMethod]
    public void TheTabsClickAndAnswerTheMouseWithoutAButtonTextOverTheStyling() {
        string source = File.ReadAllText(TabsSourcePath);

        foreach (string call in new[] {
                     "Widgets.ButtonInvisible", "Widgets.DrawHighlightIfMouseover", "MouseoverSounds.DoRegion",
                 }) {
            StringAssert.Contains(source, call, "The tab strip dropped " + call + ", so a tab stops answering the mouse.");
        }

        Assert.IsFalse(
            source.Contains("Widgets.ButtonText"),
            "A ButtonText in the tab strip draws a vanilla button over the tab's own styling."
        );
    }

    /// <summary>Two plates and one gap, with the bar's width taken off before anything is laid out in it.</summary>
    [TestMethod]
    public void TheTwoColumnShapeDrawsAPlatePerColumnAndLeavesRoomForTheScrollbar() {
        string source = File.ReadAllText(TabsSourcePath);

        Assert.AreEqual(
            2,
            Regex.Matches(source, @"Widgets\.DrawMenuSection\((?:left|right)Plate\)").Count,
            "Each column needs its own menu section, or one tab's content floats on the window background."
        );

        StringAssert.Contains(
            source,
            "ScrollbarWidth",
            "The columns no longer deduct the scrollbar, so the bar sits on top of the last chip in every row."
        );
    }

    /// <summary>A colour written here dodges the material table the rest of the designer is checked against.</summary>
    [TestMethod]
    public void TheTabStripInventsNoColourOfItsOwn() {
        string source = File.ReadAllText(TabsSourcePath);

        foreach (Match match in Regex.Matches(source, @"new Color\s*\(|#[0-9a-fA-F]{6}")) {
            Assert.Fail(
                "Dialog_KandraForms.Tabs writes its own colour at offset " + match.Index
                + ". Every colour here comes from BaseWindow or KandraAppearance."
            );
        }
    }

    /// <summary>An English literal in the strip is a label that stays English in every other language.</summary>
    [TestMethod]
    public void ATabLabelledInEnglishNeverTranslates() {
        string source = File.ReadAllText(TabsSourcePath);

        foreach (string label in new[] { "Body", "Hair", "Eyes" }) {
            Assert.IsFalse(
                source.Contains("\"" + label + "\"", StringComparison.Ordinal),
                "The tab strip carries \"" + label + "\" as a bare English literal rather than a key."
            );
        }
    }
}
