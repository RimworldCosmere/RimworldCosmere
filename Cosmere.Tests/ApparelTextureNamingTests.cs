using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Apparel graphics resolve to "&lt;texPath&gt;_&lt;BodyTypeDef.defName&gt;_&lt;direction&gt;.png", and
///     those defNames are capitalised. A lowercase suffix finds nothing on a case-sensitive filesystem.
/// </summary>
[TestClass]
public class ApparelTextureNamingTests {
    private static readonly string[] BodyTypes = ["Male", "Female", "Thin", "Fat", "Hulk"];

    private static readonly string[] Mods = ["CosmereCore", "CosmereScadrial", "CosmereRoshar"];

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

    [TestMethod]
    public void BodyTypeSuffixesMatchTheirDefNameCasing() {
        List<string> wrong = [];

        foreach (string mod in Mods) {
            string textures = Path.Combine(RepoRoot, mod, "Textures", "Things", "Pawn", "Humanlike");
            if (!Directory.Exists(textures)) continue;

            foreach (string file in Directory.EnumerateFiles(textures, "*.png", SearchOption.AllDirectories)) {
                string name = Path.GetFileNameWithoutExtension(file);

                foreach (string bodyType in BodyTypes) {
                    int at = name.IndexOf("_" + bodyType, StringComparison.OrdinalIgnoreCase);
                    if (at < 0) continue;

                    string found = name.Substring(at + 1, bodyType.Length);
                    if (found == bodyType) continue;

                    wrong.Add($"{Path.GetRelativePath(RepoRoot, file)} (reads '{found}', needs '{bodyType}')");
                }
            }
        }

        Assert.IsEmpty(
            wrong,
            "These textures carry a miscased body-type suffix, so the game finds no graphic for them:\n"
            + string.Join("\n", wrong)
        );
    }

    [TestMethod]
    public void MistcloakHasEveryBodyTypeAndDirection() {
        string dir = Path.Combine(
            RepoRoot,
            "CosmereScadrial",
            "Textures",
            "Things",
            "Pawn",
            "Humanlike",
            "Apparel",
            "Mistcloak"
        );

        List<string> missing = [];

        foreach (string bodyType in BodyTypes) {
            foreach (string direction in new[] { "north", "east", "south" }) {
                string file = Path.Combine(dir, $"Mistcloak_{bodyType}_{direction}.png");
                if (File.Exists(file)) continue;

                missing.Add(Path.GetRelativePath(RepoRoot, file));
            }
        }

        Assert.IsEmpty(missing, "The mistcloak is missing these graphics:\n" + string.Join("\n", missing));
    }
}
