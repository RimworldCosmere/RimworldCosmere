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
///     An eye row with no key, or a name that collides with a body material, fails silently in
///     game rather than at load.
/// </summary>
[TestClass]
public class KandraEyePaletteTests {
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

    [TestMethod]
    public void AnEyeColourWithNoKeyPrintsItsOwnKeyNameAtThePlayer() {
        List<(string name, string key)> rows = Table("EyeColour");

        Assert.AreEqual(12, rows.Count, $"Expected the twelve agreed eye colours, found {rows.Count}.");
        AssertKeysResolve(rows, "eye colour");
    }

    [TestMethod]
    public void AnIrisSizeOrLightStrengthWithNoKeyPrintsItsOwnKeyNameAtThePlayer() {
        List<(string name, string key)> iris = Table("Iris");
        Assert.AreEqual(2, iris.Count, $"Expected two iris sizes, found {iris.Count}.");
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
        List<float> strengths = Strengths();

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
                + "adds to every channel above it, so anything past 2 clamps to white on a pale stone "
                + "and any two such rows are indistinguishable."
            );
        }
    }

    [TestMethod]
    public void AnUnlitIrisThatIsNotItsOwnStoneColourShowsThePlayerAPaletteTheyDidNotPick() {
        Assert.AreEqual(
            1f,
            Constant("UnlitDim"),
            "UnlitDim is not 1, so an unlit eye draws something other than the hex in EyeColourTable. "
            + "Unlit is the state almost every kandra is in, so that is the palette the player judges."
        );
    }

    [TestMethod]
    public void AnEyeLightThatDarkensTheIrisMakesTurningTheLightOnLookLikeTurningItOff() {
        float dim = Constant("UnlitDim");
        float lift = Constant("LightLift");

        foreach ((string name, float[] stone) in EyeColours()) {
            float[] off = Draw(stone, dim, 0f);

            foreach (float strength in Strengths().OrderBy(value => value)) {
                float[] drawn = Draw(stone, dim, (strength - 1f) * lift);

                for (int i = 0; i < off.Length; i++) {
                    Assert.IsTrue(
                        drawn[i] >= off[i],
                        $"Lighting {name} eyes at {strength} makes channel {i} darker than unlit. "
                        + "Every strength has to be at least as bright as no light at all."
                    );
                }
            }
        }
    }

    private static float[] Draw(float[] stone, float dim, float lift) {
        return stone.Select(channel => Math.Clamp((channel * dim) + lift, 0f, 1f)).ToArray();
    }

    private static float MinDelta(float[] a, float[] b) {
        return a.Zip(b, (left, right) => Math.Abs(left - right)).Min();
    }

    private static List<float> Strengths() {
        return Regex.Matches(Source("EyeLights"), @"""[^""]+""\s*,\s*([0-9.]+)f")
            .Select(match => float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture))
            .ToList();
    }

    private static List<(string name, float[] stone)> EyeColours() {
        return Regex.Matches(Source("EyeColourTable"), @"""([^""]+)""\s*,\s*""([0-9a-fA-F]{6})""")
            .Select(match => (
                match.Groups[1].Value,
                Enumerable.Range(0, 3)
                    .Select(i => int.Parse(
                        match.Groups[2].Value.Substring(i * 2, 2),
                        NumberStyles.HexNumber,
                        CultureInfo.InvariantCulture
                    ) / 255f)
                    .ToArray()
            ))
            .ToList();
    }

    private static float Constant(string name) {
        Match value = Regex.Match(
            File.ReadAllText(AppearanceSourcePath),
            $@"const\s+float\s+{name}\s*=\s*([0-9.]+)f"
        );
        Assert.IsTrue(value.Success, $"KandraAppearance declares no {name} constant, so EyeDrawColorFor changed shape.");
        return float.Parse(value.Groups[1].Value, CultureInfo.InvariantCulture);
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
