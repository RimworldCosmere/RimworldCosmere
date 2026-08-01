using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Guards the gold shadow against the crash in betahub #6: Cosmere scenarios filter the
///     Ancients faction out of the world, so Faction.OfAncients is null and PawnGenerator NREs.
/// </summary>
[TestClass]
public class GoldShadowTests {
    private static string RepoRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate CosmereScadrial/Defs above the test output directory.");
            return dir!.FullName;
        }
    }

    private static string GoldShadowDefPath =>
        Path.Combine(RepoRoot, "CosmereScadrial", "Defs", "Allomancy", "Gold", "GoldShadow.xml");

    [TestMethod]
    public void IllusoryPawnPathDoesNotUseFactionOfAncients() {
        string[] guarded = [
            Path.Combine(
                RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial",
                "Allomancy", "Ability", "GoldAbility.cs"
            ),
            Path.Combine(RepoRoot, "CosmereCore", "CosmereCore", "Core", "Util", "IllusoryPawnUtility.cs"),
        ];

        List<string> offenders = new List<string>();

        foreach (string path in guarded) {
            Assert.IsTrue(File.Exists(path), $"Expected a guarded file at {path}");

            foreach (string line in File.ReadAllLines(path)) {
                string trimmed = line.TrimStart();
                if (trimmed.StartsWith("//") || trimmed.StartsWith("///")) continue;
                if (trimmed.Contains("Faction.OfAncients")) {
                    offenders.Add(Path.GetRelativePath(RepoRoot, path));
                }
            }
        }

        Assert.AreEqual(
            0,
            offenders.Count,
            "The illusory pawn path must stay faction-agnostic rather than depending on a specific " +
            $"faction existing. Offending files: {string.Join(", ", offenders)}"
        );
    }

    [TestMethod]
    public void GoldAbilityDoesNotCallPawnGenerator() {
        string path = Path.Combine(
            RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial",
            "Allomancy", "Ability", "GoldAbility.cs"
        );

        Assert.IsTrue(File.Exists(path), $"Expected GoldAbility at {path}");
        Assert.IsFalse(
            File.ReadAllText(path).Contains("PawnGenerator"),
            "The gold shadow must be built by IllusoryPawnUtility, not PawnGenerator."
        );
    }

    [TestMethod]
    public void GoldShadowRaceDeclaresItsOwnThinkTrees() {
        Assert.IsTrue(File.Exists(GoldShadowDefPath), $"Expected gold shadow defs at {GoldShadowDefPath}");

        XDocument doc = XDocument.Load(GoldShadowDefPath);
        XElement? race = null;
        foreach (XElement thingDef in doc.Root!.Elements("ThingDef")) {
            if (thingDef.Element("defName")?.Value == "Cosmere_Scadrial_Race_GoldShadow") {
                race = thingDef.Element("race");
            }
        }

        Assert.IsNotNull(race, "Cosmere_Scadrial_Race_GoldShadow is missing or has no <race> block.");

        Assert.AreEqual("Cosmere_Scadrial_ThinkTree_GoldShadow", race!.Element("thinkTreeMain")?.Value);
        Assert.AreEqual("Cosmere_Scadrial_ThinkTree_GoldShadowConstant", race.Element("thinkTreeConstant")?.Value);

        List<string> declared = new List<string>();
        foreach (XElement tree in doc.Root.Elements("ThinkTreeDef")) {
            string? defName = tree.Element("defName")?.Value;
            if (defName != null) declared.Add(defName);
        }

        CollectionAssert.Contains(declared, "Cosmere_Scadrial_ThinkTree_GoldShadow");
        CollectionAssert.Contains(declared, "Cosmere_Scadrial_ThinkTree_GoldShadowConstant");
    }

    [TestMethod]
    public void GoldShadowPawnKindHasNoDefaultFaction() {
        XDocument doc = XDocument.Load(GoldShadowDefPath);

        foreach (XElement kind in doc.Root!.Elements("PawnKindDef")) {
            Assert.IsNull(
                kind.Element("defaultFactionDef"),
                "A defaultFactionDef on the gold shadow reintroduces the missing-faction crash."
            );
        }
    }
}
