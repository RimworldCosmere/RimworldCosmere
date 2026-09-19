using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     What a committed reshape does with the hair colour: which name it keeps, and what happens
///     to a name the palette no longer carries.
/// </summary>
[TestClass]
public class KandraReshapeHairTests {
    private static string RepoRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not find the repo root from the test output directory.");

            return dir!.FullName;
        }
    }

    private static string KandraDir =>
        Path.Combine(RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "Kandra");

    private static string CompSource => File.ReadAllText(Path.Combine(KandraDir, "CompKandraForms.cs"));

    private static string FormSource => File.ReadAllText(Path.Combine(KandraDir, "KandraForm.cs"));

    private const string Why =
        "Without the name, retuning a palette hex leaves every already-committed kandra on the old "
        + "colour, and the designer has to work the dye back out of a Color and shows nothing selected.";

    [TestMethod]
    public void ACommittedReshapeThatKeepsOnlyTheColourForgetsWhichDyeItWas() {
        string name = HairColourNameField();
        string form = FormSource;

        Assert.IsTrue(
            Regex.IsMatch(form, $@"Scribe_Values\.Look\(\s*ref\s+{name}\b"),
            $"KandraForm.{name} is not scribed with Scribe_Values.Look, so the dye name dies on reload. {Why}"
        );

        Assert.IsTrue(
            Regex.IsMatch(form, @"public\s+Color\s+hairColour\s*;"),
            "KandraForm no longer carries a Color hairColour. KandraShapeshift.ApplyTo writes it to "
            + "pawn.story.HairColor and the portraits read it, so the name replaces nothing."
        );

        Assert.IsTrue(
            Regex.IsMatch(MethodBody(CompSource, "CommitReshape"), $@"\b{name}\s*="),
            $"CommitReshape never writes {name}, so the palette name the player picked dies at commit. {Why}"
        );
    }

    [TestMethod]
    public void AHairColourNameThePaletteDroppedLeavesTheOldColourAlone() {
        Assert.IsTrue(
            Regex.IsMatch(
                MethodBody(CompSource, "CommitReshape"),
                @"FindHairColour\(\s*pendingHairColour\s*\)\s*!=\s*null"
            ),
            "CommitReshape writes the hair colour unguarded. A name the palette dropped resolves to "
            + "nothing, and the fallback colour would land on the true body instead of the old one."
        );
    }

    [TestMethod]
    public void ARetunedPaletteHexReachesABodyAlreadyWearingTheDye() {
        string name = HairColourNameField();
        string body = MethodBody(FormSource, "ExposeData");

        Assert.IsTrue(
            Regex.IsMatch(body, $@"FindHairColour\(\s*{name}\s*\)\s*!=\s*null"),
            $"KandraForm.ExposeData resolves {name} unguarded, or not at all. A name the palette "
            + "dropped must leave the committed colour alone."
        );

        Assert.IsTrue(
            Regex.IsMatch(body, $@"hairColour\s*=\s*[\w.]*HairColorFor\(\s*{name}\s*\)"),
            $"KandraForm.ExposeData never writes hairColour back from {name}, so a retuned palette "
            + "hex never reaches a kandra already wearing it - the designer previews the new colour "
            + "while the pawn on the map keeps the old one."
        );

        Assert.IsTrue(
            body.Contains("LoadSaveMode.PostLoadInit", StringComparison.Ordinal),
            "The hair colour is resolved outside PostLoadInit, so it runs on save too."
        );
    }

    /// <summary>The field KandraForm remembers the dye by. Named freely, so it is matched by shape.</summary>
    private static string HairColourNameField() {
        Match field = Regex.Match(FormSource, @"public\s+string\?\s+(\w*[Hh]air\w*[Cc]olour\w+)\s*;");
        Assert.IsTrue(
            field.Success,
            $"KandraForm declares no string? hair colour name beside its Color hairColour. {Why}"
        );

        return field.Groups[1].Value;
    }

    private static string MethodBody(string source, string method) {
        int start = source.IndexOf($" {method}(", StringComparison.Ordinal);
        Assert.IsTrue(start >= 0, $"CompKandraForms no longer declares {method}.");

        int end = source.IndexOf("\n    }", start, StringComparison.Ordinal);
        Assert.IsTrue(end > start, $"Could not read the body of {method}.");

        return source[start..end];
    }
}
