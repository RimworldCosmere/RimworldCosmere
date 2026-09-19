using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     An eye row with no key, a name that collides with a body material, or a missing north
///     texture all fail silently in game rather than at load.
/// </summary>
[TestClass]
public class KandraEyePaletteTests {
    private static readonly string[] EyeTextures = [
        "Kandra_Eyes_Male_south.png",
        "Kandra_Eyes_Male_southm.png",
        "Kandra_Eyes_Male_east.png",
        "Kandra_Eyes_Male_eastm.png",
        "Kandra_Eyes_Male_westm.png",
        "Kandra_Eyes_Male_north.png",
        "Kandra_Eyes_Male_northm.png",
        "Kandra_Eyes_Female_south.png",
        "Kandra_Eyes_Female_southm.png",
        "Kandra_Eyes_Female_east.png",
        "Kandra_Eyes_Female_eastm.png",
        "Kandra_Eyes_Female_westm.png",
        "Kandra_Eyes_Female_north.png",
        "Kandra_Eyes_Female_northm.png",
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

    private static string AppearanceSourcePath => Path.Combine(
        RepoRoot,
        "CosmereCore",
        "CosmereCore",
        "System",
        "Scadrial",
        "Util",
        "KandraAppearance.cs"
    );

    private static string KeyedRoot => Path.Combine(RepoRoot, "CosmereScadrial", "Languages", "English", "Keyed");

    private static string EyeTextureRoot => Path.Combine(
        RepoRoot,
        "CosmereScadrial",
        "Assets",
        "Textures",
        "Things",
        "Pawn",
        "Humanlike",
        "HeadAttachments",
        "KandraEyes"
    );

    [TestMethod]
    public void AnEyeColourWithNoKeyPrintsItsOwnKeyNameAtThePlayer() {
        List<(string name, string key)> rows = Table("EyeColour");

        Assert.AreEqual(12, rows.Count, $"Expected the twelve agreed eye colours, found {rows.Count}.");
        AssertKeysResolve(rows, "eye colour");
    }

    [TestMethod]
    public void AnIrisSizeOrLightStrengthWithNoKeyPrintsItsOwnKeyNameAtThePlayer() {
        List<(string name, string key)> iris = Table("Iris");
        Assert.AreEqual(3, iris.Count, $"Expected three iris sizes, found {iris.Count}.");
        AssertKeysResolve(iris, "iris size");

        List<(string name, string key)> light = Table("EyeLight");
        Assert.AreEqual(3, light.Count, $"Expected three eye light strengths, found {light.Count}.");
        AssertKeysResolve(light, "eye light strength");
    }

    [TestMethod]
    public void AnEyeColourNamedAfterAMaterialOrADyeLeavesThePlayerUnableToTellTheControlsApart() {
        HashSet<string> eyes = Table("EyeColour").Select(row => row.name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        HashSet<string> materials = Table("Materials").Select(row => row.name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        HashSet<string> hair = Table("HairColour").Select(row => row.name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (string name in eyes) {
            Assert.IsFalse(
                materials.Contains(name) || hair.Contains(name),
                $"'{name}' names both an eye colour and a body material or hair dye. A player reading "
                + "\"amethyst eyes\" next to an amethyst body cannot tell which control they changed."
            );
        }
    }

    [TestMethod]
    public void AMissingNorthEyeTextureDrawsEyesOnTheBackOfTheHead() {
        foreach (string file in EyeTextures) {
            string path = Path.Combine(EyeTextureRoot, file);
            Assert.IsTrue(
                File.Exists(path),
                file.Contains("north", StringComparison.Ordinal)
                    ? $"{file} is missing. Graphic_Multi reuses _south rotated 180 degrees when _north "
                      + "is missing, which draws eyes on the back of the head."
                    : $"{file} is missing, so the eye node draws nothing and logs nothing."
            );
        }
    }

    [TestMethod]
    public void ADesignerKeyThatIsNeverDefinedShowsTheKeyNameOnTheTab() {
        string[] keys = [
            "CS_Kandra_Tab_Body",
            "CS_Kandra_Tab_Hair",
            "CS_Kandra_Tab_Eyes",
            "CS_Kandra_BuildHeader",
            "CS_Kandra_BuildNote",
            "CS_Kandra_EyeColourHeader",
            "CS_Kandra_IrisHeader",
            "CS_Kandra_OddEyesHeader",
            "CS_Kandra_OddEyesToggle",
            "CS_Kandra_OddEyesDesc",
            "CS_Kandra_EyeLightHeader",
            "CS_Kandra_EyeLightToggle",
            "CS_Kandra_EyeLightDesc",
        ];

        HashSet<string> defined = DefinedKeys();
        foreach (string key in keys) {
            Assert.IsTrue(
                defined.Contains(key),
                $"{key} is referenced but not defined under Languages/English/Keyed, so the game prints the key name."
            );
        }
    }

    [TestMethod]
    public void AnEyeLightStrengthOutsideTheBrightnessRangeDrawsTheSameEyeAsTheOneBesideIt() {
        string table = Source("EyeLights");
        List<float> strengths = Regex.Matches(table, @"""[^""]+""\s*,\s*([0-9.]+)f")
            .Select(match => float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture))
            .ToList();

        Assert.AreEqual(3, strengths.Count, $"Expected three eye light strengths, found {strengths.Count}.");
        Assert.AreEqual(
            3,
            strengths.Distinct().Count(),
            "Two eye light strengths are equal, so two picker rows draw an identical eye."
        );

        foreach (float strength in strengths) {
            Assert.IsTrue(
                strength is > 0f and <= 2f,
                $"Eye light strength {strength} is outside (0, 2]. EyeDrawColorFor darkens below 1 and "
                + "bleaches toward white above it, so anything past 2 is pure white and any two such "
                + "rows are indistinguishable."
            );
        }
    }

    private static string Source(string namePrefix) {
        string source = File.ReadAllText(AppearanceSourcePath);
        Match declaration = Regex.Match(source, $@"\b{namePrefix}\w*\s*=\s*\[");
        Assert.IsTrue(declaration.Success, $"KandraAppearance declares no {namePrefix} table.");
        return source[declaration.Index..source.IndexOf("];", declaration.Index, StringComparison.Ordinal)];
    }

    private static List<(string name, string key)> Table(string namePrefix) {
        string source = File.ReadAllText(AppearanceSourcePath);

        Match declaration = Regex.Match(source, $@"\b{namePrefix}\w*\s*=\s*\[");
        Assert.IsTrue(declaration.Success, $"KandraAppearance declares no {namePrefix} table.");

        string table = source[declaration.Index..source.IndexOf("];", declaration.Index, StringComparison.Ordinal)];

        List<(string name, string key)> rows = [];
        foreach (Match row in Regex.Matches(table, @"\(\s*(?:""([^""]+)""|(\w+))\s*,[^)]*\)")) {
            Match key = Regex.Match(row.Value, @"""(CS_Kandra_\w+)""");
            string name = row.Groups[1].Success ? row.Groups[1].Value : Const(source, row.Groups[2].Value);
            rows.Add((name, key.Success ? key.Groups[1].Value : string.Empty));
        }

        return rows;
    }

    private static string Const(string source, string identifier) {
        Match value = Regex.Match(source, $@"const\s+string\s+{identifier}\s*=\s*""([^""]+)""");
        Assert.IsTrue(value.Success, $"A table row names {identifier}, which is not a string constant.");
        return value.Groups[1].Value;
    }

    private static void AssertKeysResolve(List<(string name, string key)> rows, string what) {
        HashSet<string> defined = DefinedKeys();

        foreach ((string name, string key) in rows) {
            Assert.AreNotEqual(
                string.Empty,
                key,
                $"The {what} '{name}' carries no labelKey, so the picker shows nothing to read."
            );
            Assert.IsTrue(
                defined.Contains(key),
                $"{key} is referenced but not defined under Languages/English/Keyed, so the game prints the key name."
            );
        }
    }

    private static HashSet<string> DefinedKeys() {
        return Directory.EnumerateFiles(KeyedRoot, "*.xml")
            .SelectMany(file => XDocument.Load(file).Root?.Elements() ?? [])
            .Select(element => element.Name.LocalName)
            .ToHashSet(StringComparer.Ordinal);
    }
}
