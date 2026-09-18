using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     A wrong formless texture path draws nothing and logs nothing, and vetoing Body or Head
///     instead of their apparel children makes a formless kandra invisible rather than naked.
/// </summary>
[TestClass]
public class KandraTrueBodyArtTests {
    private static readonly string[] Directions = ["south", "east", "north"];
    private static readonly string[] Sexes = ["Kandra_Male", "Kandra_Female"];

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

    private static string TextureRoot => Path.Combine(
        RepoRoot,
        "CosmereScadrial",
        "Assets",
        "Textures",
        "Things",
        "Pawn",
        "Humanlike"
    );

    [TestMethod]
    public void EveryFormlessTextureTheRenderPatchesAskForExists() {
        foreach (string folder in new[] { "Bodies", "Heads" }) {
            foreach (string sex in Sexes) {
                foreach (string dir in Directions) {
                    string path = Path.Combine(TextureRoot, folder, $"{sex}_{dir}.png");
                    Assert.IsTrue(
                        File.Exists(path),
                        $"KandraAppearance builds a path to {folder}/{sex}, but {sex}_{dir}.png is missing."
                    );
                }
            }
        }
    }

    [TestMethod]
    public void TheSuffixesKandraAppearanceBuildsMatchTheFilesOnDisk() {
        string source = File.ReadAllText(AppearanceSourcePath);

        foreach (string sex in Sexes) {
            StringAssert.Contains(
                source,
                $"\"{sex}\"",
                $"KandraAppearance no longer builds {sex}, so the textures named for it are dead."
            );
        }
    }

    [TestMethod]
    public void EveryTrueBodyMaterialHasAParseableHexAndAPositiveWeight() {
        string source = File.ReadAllText(AppearanceSourcePath);
        int start = source.IndexOf("Materials = [", StringComparison.Ordinal);
        Assert.IsTrue(start >= 0, "KandraAppearance no longer declares a Materials table.");

        string table = source[start..source.IndexOf("];", start, StringComparison.Ordinal)];
        MatchCollection rows = Regex.Matches(table, @"\(""([^""]+)"", ""([^""]+)"", ([0-9.]+)f\)");

        Assert.IsTrue(rows.Count >= 10, $"Only {rows.Count} true-body materials; the table looks cut.");

        foreach (Match row in rows) {
            Assert.IsTrue(
                Regex.IsMatch(row.Groups[2].Value, "^[0-9a-f]{6}$"),
                $"{row.Groups[1].Value} has hex '{row.Groups[2].Value}', which ColorUtility will reject."
            );
            Assert.IsTrue(
                double.Parse(row.Groups[3].Value, CultureInfo.InvariantCulture) > 0,
                $"{row.Groups[1].Value} has a weight of zero, so it can never be rolled."
            );
        }
    }

    [TestMethod]
    public void NoTrueBodyMaterialLandsInsideTheHumanSkinGamut() {
        string source = File.ReadAllText(AppearanceSourcePath);
        int start = source.IndexOf("Materials = [", StringComparison.Ordinal);
        string table = source[start..source.IndexOf("];", start, StringComparison.Ordinal)];

        foreach (Match row in Regex.Matches(table, @"\(""([^""]+)"", ""([0-9a-f]{6})"", ")) {
            (float hue, float saturation) = HueSaturation(row.Groups[2].Value);

            // RimWorld's whole skin gradient sits at hue 18-24 with saturation up to 0.55.
            Assert.IsFalse(
                hue is >= 10f and <= 45f && saturation < 0.55f,
                $"{row.Groups[1].Value} (#{row.Groups[2].Value}) is hue {hue:0} at {saturation:0.00} "
                + "saturation, which reads as a naked person rather than a kandra."
            );
        }
    }

    private static (float hue, float saturation) HueSaturation(string hex) {
        float r = Convert.ToInt32(hex[..2], 16) / 255f;
        float g = Convert.ToInt32(hex[2..4], 16) / 255f;
        float b = Convert.ToInt32(hex[4..], 16) / 255f;

        float max = Math.Max(r, Math.Max(g, b));
        float min = Math.Min(r, Math.Min(g, b));
        float span = max - min;

        if (span == 0f) return (0f, 0f);

        float hue;
        if (max == r) {
            hue = 60f * (((g - b) / span) % 6f);
        } else if (max == g) {
            hue = 60f * (((b - r) / span) + 2f);
        } else {
            hue = 60f * (((r - g) / span) + 4f);
        }

        return (hue < 0f ? hue + 360f : hue, max == 0f ? 0f : span / max);
    }

    [TestMethod]
    public void RenderingDoesNotWaitOnTheFormlessHediff() {
        string source = File.ReadAllText(AppearanceSourcePath);
        int start = source.IndexOf("public static bool IsFormless", StringComparison.Ordinal);
        Assert.IsTrue(start >= 0, "KandraAppearance no longer has IsFormless.");

        string body = source[start..source.IndexOf("\n    }", start, StringComparison.Ordinal)];

        Assert.IsFalse(
            body.Contains("Hediff_Formless", StringComparison.Ordinal),
            "IsFormless reads the hediff again. The hediff lands on a rare tick, so a kandra "
            + "spends up to 250 ticks rendering as whoever it last ate."
        );
        Assert.IsTrue(
            body.Contains("HasActiveGene", StringComparison.Ordinal)
            && body.Contains("crafted", StringComparison.Ordinal),
            "IsFormless must read the gene and the form, which are both correct the instant they change."
        );
    }

    [TestMethod]
    public void OreSeurIsAWolfhoundInEveryScenarioHeAppearsIn() {
        string[] scenarios = ["FinalEmpire.xml", "PreCatacendre.xml", "WellOfAscension.xml"];

        foreach (string file in scenarios) {
            XDocument doc = XDocument.Load(
                Path.Combine(RepoRoot, "CosmereScadrial", "Defs", "Scenarios", file)
            );

            XElement? oreSeur = doc.Descendants("li")
                .FirstOrDefault(li => li.Element("firstName")?.Value == "OreSeur");
            Assert.IsNotNull(oreSeur, $"OreSeur is not on the roster in {file}.");

            Assert.AreEqual(
                "Cosmere_Scadrial_Wolfhound",
                oreSeur.Element("kandraTrueAnimal")?.Value,
                $"The wolfhound is not OreSeur's true body in {file}, so he arrives as a person there."
            );

            Assert.IsTrue(
                oreSeur.Element("kandraKnownFaces")?.Elements("li").Any(li => li.Value == "Lord Renoux")
                == true,
                $"OreSeur has no Lord Renoux face in {file}."
            );
        }
    }

    [TestMethod]
    public void TheRememberedTrueBodyIsMarkedAsCrafted() {
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

        int start = comp.IndexOf("public void RememberTrueBody", StringComparison.Ordinal);
        Assert.IsTrue(start >= 0, "CompKandraForms no longer remembers a true body.");

        string body = comp[start..comp.IndexOf("\n    }", start, StringComparison.Ordinal)];

        // Unmarked, the snapshot is just a rolled human face and the grey art never draws.
        StringAssert.Contains(
            body,
            "crafted = true",
            "The remembered true body is not marked crafted, so returning to it shows a person."
        );
    }

    [TestMethod]
    public void EveryWolfhoundLifeStageDeclaresACoatColour() {
        XDocument doc = XDocument.Load(
            Path.Combine(
                RepoRoot,
                "CosmereScadrial",
                "Defs",
                "Things",
                "Pawn",
                "Animal",
                "Wolfhound.xml"
            )
        );

        XElement[] stages = doc.Descendants("lifeStages").Elements("li").ToArray();
        Assert.AreNotEqual(0, stages.Length, "The wolfhound has no life stages.");

        // The borrowed wolf art is greyscale at 0.84 luminance. Untinted it renders near-white.
        foreach (XElement stage in stages) {
            Assert.IsNotNull(
                stage.Element("bodyGraphicData")?.Element("color"),
                "A wolfhound life stage has no colour, so it multiplies by white and glows."
            );
        }
    }

    [TestMethod]
    public void TheWolfhoundIsSmallEnoughForAKandraToWear() {
        XDocument doc = XDocument.Load(
            Path.Combine(
                RepoRoot,
                "CosmereScadrial",
                "Defs",
                "Things",
                "Pawn",
                "Animal",
                "Wolfhound.xml"
            )
        );

        XElement? adult = doc.Descendants("lifeStages").Elements("li").LastOrDefault();
        string? size = adult?.Element("bodyGraphicData")?.Element("drawSize")?.Value;
        Assert.IsNotNull(size, "The wolfhound's adult life stage has no drawSize.");

        // KandraShapeEligibility.MaxDrawSize. Over it and Wear() silently refuses the form.
        Assert.IsTrue(
            float.Parse(size, CultureInfo.InvariantCulture) <= 2f,
            $"The wolfhound draws at {size}, over the 2.0 a kandra is allowed to wear."
        );
    }

    [TestMethod]
    public void StowingGearOnAnUnspawnedPawnDoesNotGoThroughTheMap() {
        string source = File.ReadAllText(
            Path.Combine(
                RepoRoot,
                "CosmereCore",
                "CosmereCore",
                "System",
                "Scadrial",
                "Kandra",
                "KandraShapeshift.cs"
            )
        );

        int start = source.IndexOf("private static void StowGear", StringComparison.Ordinal);
        Assert.IsTrue(start >= 0, "KandraShapeshift no longer has StowGear.");

        string body = source[start..source.IndexOf("\n    }", start, StringComparison.Ordinal)];

        // TryDrop routes through MapHeld. A scenario pawn has none, so it logs one error a garment.
        Assert.IsTrue(
            body.Contains("!pawn.Spawned", StringComparison.Ordinal),
            "StowGear no longer checks Spawned, so dressing a scenario kandra errors once per garment."
        );
    }

    [TestMethod]
    public void FormlessHidesApparelAndTattoosRatherThanTheBodyAndHead() {
        XDocument doc = XDocument.Load(
            Path.Combine(RepoRoot, "CosmereScadrial", "Patches", "KandraHumanlikeRenderTree.xml")
        );

        string[] targets = doc.Descendants("Operation")
            .Where(
                op => op.Descendants("li")
                    .Any(li => li.Value.EndsWith("HideWhileFormless", StringComparison.Ordinal))
            )
            .Select(op => op.Element("xpath")?.Value ?? string.Empty)
            .ToArray();

        Assert.AreEqual(4, targets.Length, "Expected the formless veto on exactly four nodes.");

        foreach (string want in new[] { "ApparelBody", "ApparelHead", "Body tattoo", "Head tattoo" }) {
            Assert.IsTrue(
                targets.Any(t => t.Contains(want, StringComparison.Ordinal)),
                $"The formless veto is not on {want}."
            );
        }

        foreach (string target in targets) {
            Assert.IsTrue(
                target.Contains("/children/li[", StringComparison.Ordinal),
                $"The formless veto landed on a root node ({target}); that hides the kandra entirely."
            );
        }
    }
}
