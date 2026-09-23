using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>Guards the hair side of the kandra true-body designer: the pending pair, the palette and the room to draw it.</summary>
[TestClass]
public class KandraHairDesignerTests {
    private static readonly (string field, string what)[] PendingFields = [
        ("pendingMaterial", "the true body is made of"),
        ("pendingGender", "the true body's gender"),
        ("pendingHair", "the true body's hair"),
        ("pendingHairColour", "the true body's hair colour"),
    ];

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

    private static string CompSourcePath => Path.Combine(
        RepoRoot,
        "CosmereCore",
        "CosmereCore",
        "System",
        "Scadrial",
        "Kandra",
        "CompKandraForms.cs"
    );

    private static string AppearanceSourcePath => Path.Combine(
        RepoRoot,
        "CosmereCore",
        "CosmereCore",
        "System",
        "Scadrial",
        "Util",
        "KandraAppearance.cs"
    );

    private static string DialogSourcePath => Path.Combine(
        RepoRoot,
        "CosmereCore",
        "CosmereCore",
        "System",
        "Scadrial",
        "Kandra",
        "Dialog_KandraForms.cs"
    );

    private static string KeyedRoot => Path.Combine(RepoRoot, "CosmereScadrial", "Languages", "English", "Keyed");

    [TestMethod]
    public void AFieldOneOfTheThreeReshapeMethodsForgetsEitherNeverLandsOrRidesInTheSaveForever() {
        string source = File.ReadAllText(CompSourcePath);

        (string method, string breaks)[] methods = [
            ("BeginReshape", "the designer's choice never reaches the job"),
            ("CommitReshape", "the ten seconds pass and that field never lands"),
            ("CancelReshape", "that field rides in the save forever and reapplies on the next reshape"),
        ];

        foreach ((string method, string breaks) in methods) {
            string body = MethodBody(source, method);

            foreach ((string field, string what) in PendingFields) {
                Assert.IsTrue(
                    Regex.IsMatch(body, $@"\b{field}\b"),
                    $"{method} never mentions {field} ({what}), so {breaks}."
                );
            }
        }
    }

    [TestMethod]
    public void AHairDefScribedByValueLosesTheDefOnReload() {
        string source = File.ReadAllText(CompSourcePath);

        Assert.IsTrue(
            Regex.IsMatch(source, @"private\s+string\?\s+pendingHairColour\s*;"),
            "pendingHairColour is not a string? on CompKandraForms. Storing a Color freezes a hex "
            + "the palette may retune, and a nullable struct through Scribe_Values is its own trap."
        );

        Assert.IsTrue(
            Regex.IsMatch(source, @"Scribe_Values\.Look\(\s*ref\s+pendingHairColour\b"),
            "pendingHairColour is not scribed with Scribe_Values.Look, so a pending colour dies on save."
        );

        Assert.IsTrue(
            Regex.IsMatch(source, @"Scribe_Defs\.Look\(\s*ref\s+pendingHair\b"),
            "pendingHair is not scribed with Scribe_Defs.Look. A HairDef through Scribe_Values "
            + "silently loses the def on reload and the reshape commits bald."
        );
    }

    [TestMethod]
    public void AHairColourWithNoKeyPrintsItsOwnKeyNameAtThePlayer() {
        string source = File.ReadAllText(AppearanceSourcePath);

        Match declaration = Regex.Match(source, @"HairColour\w*\s*=\s*\[");
        Assert.IsTrue(declaration.Success, "KandraAppearance declares no hair colour table.");

        int start = declaration.Index;
        string table = source[start..source.IndexOf("];", start, StringComparison.Ordinal)];
        MatchCollection rows = Regex.Matches(table, @"\(\s*""([^""]+)""\s*,\s*""([0-9a-f]{6})""[^)]*\)");

        Assert.AreEqual(12, rows.Count, $"Expected the twelve agreed hair colours, found {rows.Count}.");

        List<string> keys = [];
        foreach (Match row in rows) {
            Match key = Regex.Match(row.Value, @"""(CS_Kandra_HairColour_\w+)""");
            Assert.IsTrue(
                key.Success,
                $"Hair colour '{row.Groups[1].Value}' carries no labelKey, so the picker shows nothing to read."
            );
            keys.Add(key.Groups[1].Value);
        }

        keys.Add("CS_Kandra_HairHeader");
        keys.Add("CS_Kandra_HairColourHeader");

        HashSet<string> defined = Directory.EnumerateFiles(KeyedRoot, "*.xml")
            .SelectMany(f => XDocument.Load(f).Root?.Elements() ?? [])
            .Select(e => e.Name.LocalName)
            .ToHashSet(StringComparer.Ordinal);

        foreach (string key in keys) {
            Assert.IsTrue(
                defined.Contains(key),
                $"{key} is referenced but not defined under Languages/English/Keyed, so the game prints the key name."
            );
        }
    }

    [TestMethod]
    public void AnEightHundredWideWindowCannotFitTheMaterialAndHairColumnsSideBySide() {
        string source = File.ReadAllText(DialogSourcePath);

        Match width = Regex.Match(
            source,
            @"initialWindowSize\s*=>\s*new Vector2\(\s*Spacing\.Get\(([0-9.]+)\)"
        );
        Assert.IsTrue(width.Success, "Dialog_KandraForms no longer sizes its window off Spacing.Get.");

        Assert.IsTrue(
            double.Parse(width.Groups[1].Value, CultureInfo.InvariantCulture) >= 65,
            $"The form window is Spacing.Get({width.Groups[1].Value}) wide. At 800 the material column "
            + "and the hair column do not both fit, which is the two-column layout Ka picked."
        );
    }

    [TestMethod]
    public void ADesignerThatWorksTheDyeBackOutOfAColourShowsNoSwatchOnceThePaletteRetunes() {
        string source = File.ReadAllText(DialogSourcePath);

        const string why = "Without this the designer reconstructs the dye name from a Color. "
            + "Retune a palette hex and every already-committed kandra keeps the old colour and "
            + "reads as no dye selected.";

        foreach (string gone in new[] { "SeedHairColour", "SameColour" }) {
            Assert.IsFalse(
                Regex.IsMatch(source, $@"\b{gone}\b"),
                $"Dialog_KandraForms still declares {gone}. {why}"
            );
        }

        Assert.IsFalse(
            Regex.IsMatch(source, @"Mathf\.Abs\([^)]*\.[rgba]\b"),
            $"Dialog_KandraForms still compares colour channels with a float epsilon. {why}"
        );
    }

    private static string MethodBody(string source, string method) {
        int start = source.IndexOf($" {method}(", StringComparison.Ordinal);
        Assert.IsTrue(start >= 0, $"CompKandraForms no longer declares {method}.");

        int end = source.IndexOf("\n    }", start, StringComparison.Ordinal);
        Assert.IsTrue(end > start, $"Could not read the body of {method}.");
        return source[start..end];
    }
}
