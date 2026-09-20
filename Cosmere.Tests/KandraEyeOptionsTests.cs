using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Guards the Eyes tab's right column: the two sentinels that turn a control off, the table
///     the iris rows are read from, and the height the scroll view is given.
/// </summary>
[TestClass]
public class KandraEyeOptionsTests {
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

    private static string EyesSource => File.ReadAllText(
        Path.Combine(
            RepoRoot,
            "CosmereCore",
            "CosmereCore",
            "System",
            "Scadrial",
            "Kandra",
            "Dialog_KandraForms.Eyes.cs"
        )
    );

    private static string GeneralKeyedPath => Path.Combine(
        RepoRoot,
        "CosmereScadrial",
        "Languages",
        "English",
        "Keyed",
        "General.xml"
    );

    /// <summary>
    ///     Null means the player changed nothing, so unticking either box has to send the sentinel.
    ///     Sending null leaves the old value and the control silently does nothing.
    /// </summary>
    [TestMethod]
    public void UntickingAControlSendsItsSentinelRatherThanNull() {
        string source = EyesSource;

        StringAssert.Contains(
            source,
            "KandraAppearance.EyeColourNone",
            "Odd eyes turn off with something other than the none sentinel, so they cannot be turned off."
        );

        StringAssert.Contains(
            source,
            "KandraAppearance.EyeLightOff",
            "The light turns off with something other than the off sentinel, so it cannot be put out."
        );

        foreach (string field in new[] { "designEyeColourTwo", "designEyeLight" }) {
            Assert.IsFalse(
                Regex.IsMatch(source, field + @"\s*=\s*null"),
                field + " is set to null, which reads as 'the player changed nothing'."
            );
        }
    }

    /// <summary>A wide iris was cut on 2026-09-19. A count written here goes stale the same way.</summary>
    [TestMethod]
    public void TheIrisAndLightRowsComeFromTheTablesRatherThanACountWrittenHere() {
        string source = EyesSource;

        foreach (string table in new[] {
                     "KandraAppearance.AllIrisSizes", "KandraAppearance.AllEyeLights",
                     "KandraAppearance.AllEyeColours",
                 }) {
            StringAssert.Contains(source, table, "The right column does not read " + table + ".");
        }

        Assert.IsFalse(
            Regex.IsMatch(source, @"(irises|lights|stones)\.Count\s*[=!]=\s*\d"),
            "The right column compares a palette against a fixed count, so retuning a table breaks it."
        );
    }

    /// <summary>
    ///     Ticking a box changes how tall the column is. A fixed height makes the bar jump, and a
    ///     height smaller than the content hides the last rows entirely.
    /// </summary>
    [TestMethod]
    public void TheScrollHeightIsMeasuredBeforeTheViewIsOpened() {
        string source = EyesSource;

        int measured = source.IndexOf("float content =", StringComparison.Ordinal);
        int opened = source.IndexOf("Widgets.BeginScrollView(", StringComparison.Ordinal);

        Assert.IsTrue(measured >= 0, "The column no longer measures its own content.");
        Assert.IsTrue(opened > measured, "The scroll view is opened before the content is measured.");

        foreach (string section in new[] { "odd ?", "lit ?" }) {
            StringAssert.Contains(
                source,
                section,
                "The measured height ignores a section that appears and disappears."
            );
        }
    }

    /// <summary>Every colour here comes from BaseWindow or the palette. One written in drifts alone.</summary>
    [TestMethod]
    public void TheRightColumnInventsNoColourOfItsOwn() {
        foreach (Match match in Regex.Matches(EyesSource, @"new Color\s*\(|#[0-9a-fA-F]{6}")) {
            Assert.Fail(
                "Dialog_KandraForms.Eyes writes its own colour at offset " + match.Index
                + ". Every colour here comes from BaseWindow or KandraAppearance."
            );
        }
    }

    /// <summary>The column is drawn by hand, so the click, the hover and the sound are all asked for.</summary>
    [TestMethod]
    public void EveryRowAnswersTheMouseAndEveryLabelComesFromAKeyThatExists() {
        string source = EyesSource;

        foreach (string call in new[] {
                     "Widgets.ButtonInvisible", "Widgets.DrawHighlightIfMouseover", "MouseoverSounds.DoRegion",
                 }) {
            StringAssert.Contains(source, call, "The right column dropped " + call + ".");
        }

        XElement root = XDocument.Load(GeneralKeyedPath).Root!;

        foreach (Match used in Regex.Matches(source, "\"(CS_[A-Za-z0-9_]+)\"")) {
            Assert.IsNotNull(
                root.Element(used.Groups[1].Value),
                used.Groups[1].Value + " is translated by the right column but missing, so it prints raw."
            );
        }
    }
}
