using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Reads the generated Feruchemy hediffs and the hand-written Allomancy ones, and checks
///     the numbers RimWorld will happily load and then apply as nonsense.
/// </summary>
/// <remarks>
///     RimWorld scales a hediff stat factor as <c>1 - (1 - factor) * severity</c>, and nothing
///     bounds severity - a duralumin burn reaches about 87. Any factor below 1 on that path
///     crosses zero and keeps going, which is how burning pewter reached a negative melee
///     cooldown. Any factor that has to move with severity now does it through a saturating
///     curve instead.
/// </remarks>
[TestClass]
public class MetallicArtsBalanceTests {
    // Past anything like this a duralumin burn is off the end of every scale in the game.
    private const float ExtremeSeverity = 100f;

    private static string RepoRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs", "Feruchemy"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate CosmereScadrial/Defs/Feruchemy above the test output directory.");
            return dir!.FullName;
        }
    }

    private static IEnumerable<(string file, XElement def)> DefsUnder(params string[] relativeParts) {
        string dir = Path.Combine(RepoRoot, Path.Combine(relativeParts));
        if (!Directory.Exists(dir)) yield break;

        foreach (string path in Directory.GetFiles(dir, "*.xml", SearchOption.AllDirectories)) {
            XElement? root = XDocument.Load(path).Root;
            if (root == null) continue;
            foreach (XElement def in root.Elements("HediffDef")) {
                yield return (Path.GetFileName(path), def);
            }
        }
    }

    private static IEnumerable<XElement> Stages(XElement def) {
        return def.Element("stages")?.Elements("li") ?? [];
    }

    private static string Name(XElement def) {
        return (string?)def.Element("defName") ?? (string?)def.Attribute("Name") ?? "<abstract>";
    }

    // The bug: getStatForStage emitted the factor formula for offsets too, so every offset
    // carried a spare 1 and then multiplied by the stage. Storing brass asked for a room
    // above eighty degrees.
    [TestMethod]
    public void FeruchemicalTemperatureOffsetsStayInsideARangeAPawnCanLiveIn() {
        foreach ((string file, XElement def) in DefsUnder("CosmereScadrial", "Defs", "Feruchemy")) {
            foreach (XElement stage in Stages(def)) {
                XElement? offsets = stage.Element("statOffsets");
                if (offsets == null) continue;

                foreach (XElement offset in offsets.Elements()) {
                    float value = float.Parse(offset.Value);
                    Assert.IsTrue(
                        Math.Abs(value) <= 100f,
                        $"{file}/{Name(def)}: {offset.Name} offset is {value}"
                    );
                }
            }
        }
    }

    [TestMethod]
    public void NoFeruchemicalLadderSwitchesAStatOffEntirely() {
        foreach ((string file, XElement def) in DefsUnder("CosmereScadrial", "Defs", "Feruchemy")) {
            foreach (XElement stage in Stages(def)) {
                foreach (XElement factor in stage.Element("statFactors")?.Elements() ?? []) {
                    float value = float.Parse(factor.Value);
                    Assert.IsTrue(value > 0f, $"{file}/{Name(def)}: {factor.Name} factor is {value}");
                }
            }
        }
    }

    // A capacity floored where a stat is floored leaves a colonist who cannot see, hear or
    // stand up, which is a different thing from one who gave their senses away.
    [TestMethod]
    public void StoringNeverTakesACapacityBelowWhatAPawnCanFunctionAt() {
        foreach ((string file, XElement def) in DefsUnder("CosmereScadrial", "Defs", "Feruchemy")) {
            foreach (XElement stage in Stages(def)) {
                foreach (XElement mod in stage.Element("capMods")?.Elements("li") ?? []) {
                    string? postFactor = (string?)mod.Element("postFactor");
                    if (postFactor == null) continue;

                    Assert.IsTrue(
                        float.Parse(postFactor) >= 0.15f,
                        $"{file}/{Name(def)}: {(string?)mod.Element("capacity")} postFactor is {postFactor}"
                    );
                }
            }
        }
    }

    // Feruchemy is conservation. A metal whose tapping ladder pays out more than its storing
    // ladder costs is making Investiture out of nothing - gold's tap step was 0.95 against a
    // storing step of 0.045, so a bloodmaker healed twenty times over for a tenth of the price.
    [TestMethod]
    public void EveryFeruchemicalLadderMirrorsItsOppositeNumber() {
        Dictionary<string, Dictionary<string, float>> byDef = [];

        foreach ((string _, XElement def) in DefsUnder("CosmereScadrial", "Defs", "Feruchemy")) {
            string name = Name(def);
            if (!name.StartsWith("Cosmere_Scadrial_Hediff_Store") && !name.StartsWith("Cosmere_Scadrial_Hediff_Tap")) {
                continue;
            }

            if (name.Contains("Compounded")) continue;

            XElement? top = Stages(def).LastOrDefault();
            XElement? factors = top?.Element("statFactors");
            if (factors == null) continue;

            byDef[name] = factors.Elements().ToDictionary(e => e.Name.LocalName, e => float.Parse(e.Value));
        }

        foreach ((string storeName, Dictionary<string, float> store) in byDef) {
            if (!storeName.StartsWith("Cosmere_Scadrial_Hediff_Store")) continue;

            string tapName = storeName.Replace("Hediff_Store", "Hediff_Tap");
            if (!byDef.TryGetValue(tapName, out Dictionary<string, float>? tap)) continue;

            foreach ((string stat, float storeValue) in store) {
                Assert.IsTrue(tap.ContainsKey(stat), $"{tapName} is missing {stat}, which {storeName} moves");

                // The ladder is geometric, so the two sides of any rung are reciprocals and
                // multiply back to 1. That is what conservation means for a multiplier - and
                // it is the only shape that reaches a peak of ten without the storing side
                // having to pass through zero on the way down.
                float tapValue = tap[stat];

                Assert.AreEqual(
                    1f,
                    storeValue * tapValue,
                    0.01f,
                    $"{stat}: storing gives {storeValue} but tapping gives {tapValue}"
                );
            }
        }
    }

    // A ladder that climbs evenly to its peak is the point of the geometric shape: half the
    // dial should be worth noticeably less than all of it, and every rung should be an
    // improvement on the one below.
    [TestMethod]
    public void EveryLadderClimbsEvenlyToItsPeak() {
        int laddersChecked = 0;

        foreach ((string file, XElement def) in DefsUnder("CosmereScadrial", "Defs", "Feruchemy")) {
            string name = Name(def);
            if (!name.StartsWith("Cosmere_Scadrial_Hediff_Tap") || name.Contains("Compounded")) continue;

            List<XElement> stages = Stages(def).ToList();
            if (stages.Count < 2) continue;

            Dictionary<string, List<float>> byStat = [];
            foreach (XElement stage in stages) {
                foreach (XElement factor in stage.Element("statFactors")?.Elements() ?? []) {
                    if (!byStat.TryGetValue(factor.Name.LocalName, out List<float>? rungs)) {
                        byStat[factor.Name.LocalName] = rungs = [];
                    }

                    rungs.Add(float.Parse(factor.Value));
                }
            }

            foreach ((string stat, List<float> rungs) in byStat) {
                Assert.AreEqual(stages.Count, rungs.Count, $"{file}/{name}: {stat} skips a rung");

                bool climbing = rungs[^1] > rungs[0];
                for (int i = 1; i < rungs.Count; i++) {
                    bool ordered = climbing ? rungs[i] > rungs[i - 1] : rungs[i] < rungs[i - 1];
                    Assert.IsTrue(ordered, $"{file}/{name}: {stat} goes backwards at rung {i}");
                }

                // Geometric means the halfway rung is the square root of the peak, so half a
                // dial buys about a third of a tenfold ladder rather than half of it.
                float peak = rungs[^1];
                float middle = rungs[rungs.Count / 2 - 1];
                Assert.AreEqual(
                    Math.Sqrt(peak),
                    middle,
                    Math.Abs(peak) * 0.05f + 0.02f,
                    $"{file}/{name}: {stat} is not geometric - peak {peak}, halfway {middle}"
                );

                laddersChecked++;
            }
        }

        Assert.IsTrue(laddersChecked > 0, "found no feruchemical ladders at all");
    }

    // Anything that used to ride multiplyStatChangesBySeverity now rides a curve. Leaving the
    // flag on a stage that still lists a flat factor puts the old blowout straight back.
    [TestMethod]
    public void NoAllomanticStageStillMultipliesAFlatFactorBySeverity() {
        foreach ((string file, XElement def) in DefsUnder("CosmereScadrial", "Defs", "Allomancy")) {
            foreach (XElement stage in Stages(def)) {
                bool multiplies = string.Equals(
                    (string?)stage.Element("multiplyStatChangesBySeverity"),
                    "true",
                    StringComparison.OrdinalIgnoreCase
                );
                if (!multiplies) continue;

                Assert.IsNull(
                    stage.Element("statFactors"),
                    $"{file}/{Name(def)}: flat statFactors scaled by an unbounded severity"
                );
                Assert.IsNull(
                    stage.Element("statOffsets"),
                    $"{file}/{Name(def)}: flat statOffsets scaled by an unbounded severity"
                );
            }
        }
    }

    [TestMethod]
    public void EveryAllomanticSeverityCurveStaysPositiveAndSaturates() {
        int checkedCurves = 0;

        foreach ((string file, XElement def) in DefsUnder("CosmereScadrial", "Defs", "Allomancy")) {
            foreach (XElement stage in Stages(def)) {
                foreach (XElement entry in stage.Element("statFactorsBySeverity")?.Elements("li") ?? []) {
                    string stat = (string?)entry.Element("stat") ?? "?";
                    List<(float severity, float value)> points = ReadCurve(entry);

                    Assert.IsTrue(points.Count >= 2, $"{file}/{Name(def)}: {stat} curve has too few points");
                    Assert.AreEqual(1f, points[0].value, 1e-4f, $"{file}/{Name(def)}: {stat} does nothing at severity 0");
                    Assert.IsTrue(
                        points[^1].severity >= ExtremeSeverity,
                        $"{file}/{Name(def)}: {stat} curve stops at severity {points[^1].severity}, below a duralumin burn"
                    );

                    foreach ((float severity, float value) in points) {
                        Assert.IsTrue(value > 0f, $"{file}/{Name(def)}: {stat} reaches {value} at severity {severity}");
                    }

                    checkedCurves++;
                }
            }
        }

        Assert.IsTrue(checkedCurves > 0, "found no severity curves at all - the conversion did not run");
    }

    // Both bubbles shipped with the flag off and hardcoded factors, so burning, flaring and a
    // duralumin burn produced the identical bubble.
    [TestMethod]
    public void BothTimeBubblesRespondToSeverity() {
        string[] wanted = [
            "Cosmere_Scadrial_Hediff_TimeBubbleCadmium",
            "Cosmere_Scadrial_Hediff_TimeBubbleBendalloy",
        ];

        foreach (string defName in wanted) {
            XElement? def = DefsUnder("CosmereScadrial", "Defs", "Allomancy")
                .Select(pair => pair.def)
                .FirstOrDefault(d => (string?)d.Element("defName") == defName);

            Assert.IsNotNull(def, $"{defName} not found");

            XElement? stage = Stages(def!).FirstOrDefault();
            Assert.IsNotNull(stage, $"{defName} has no stage");
            Assert.IsNull((string?)stage!.Element("statFactors"), $"{defName} still carries flat factors");

            List<XElement> curves = stage.Element("statFactorsBySeverity")?.Elements("li").ToList() ?? [];
            Assert.IsTrue(curves.Count > 0, $"{defName} has no severity curves");

            foreach (XElement entry in curves) {
                List<(float severity, float value)> points = ReadCurve(entry);
                Assert.AreNotEqual(
                    points[1].value,
                    points[^1].value,
                    $"{defName}: {(string?)entry.Element("stat")} is flat across the whole severity range"
                );
            }
        }
    }

    // Upkeep should track what the burn buys. Pewter - seventeen stats including double move
    // speed - used to sit on the same default as an ability that only draws lines in the air.
    [TestMethod]
    public void PewterCostsMoreToBurnThanAPassiveSense() {
        Dictionary<string, float> upkeep = ReadUpkeep();

        Assert.IsTrue(upkeep.ContainsKey("Cosmere_Scadrial_Ability_Pewter"), "pewter ability not found");
        Assert.IsTrue(
            upkeep["Cosmere_Scadrial_Ability_Pewter"] > upkeep["Cosmere_Scadrial_Ability_SteelAura"],
            "burning pewter costs no more than burning steel for the lines"
        );
        Assert.IsTrue(
            upkeep["Cosmere_Scadrial_Ability_Atium"] > upkeep["Cosmere_Scadrial_Ability_Pewter"],
            "atium costs no more than pewter"
        );
    }

    [TestMethod]
    public void EveryBurnableAbilityDeclaresItsUpkeep() {
        foreach ((string name, float value) in ReadUpkeep()) {
            Assert.IsTrue(value > 0f, $"{name} declares a non-positive beuPerTick of {value}");
        }
    }

    private static Dictionary<string, float> ReadUpkeep() {
        Dictionary<string, float> upkeep = [];
        string dir = Path.Combine(RepoRoot, "CosmereScadrial", "Defs", "Allomancy");

        foreach (string path in Directory.GetFiles(dir, "Abilities.xml", SearchOption.AllDirectories)) {
            XElement? root = XDocument.Load(path).Root;
            if (root == null) continue;

            foreach (XElement def in root.Elements()) {
                string? name = (string?)def.Element("defName");
                string? beu = (string?)def.Element("beuPerTick");
                if (name == null || beu == null) continue;

                upkeep[name] = float.Parse(beu);
            }
        }

        return upkeep;
    }

    // SimpleCurve keeps its points in a `points` field. A bare list of <li> under the curve
    // loads for a List<CurvePoint> and not for this, and RimWorld's answer is an XML error per
    // point plus an empty curve - which evaluates to zero, so every affected stat silently
    // became a factor of 0 rather than reverting to 1.
    private static List<(float severity, float value)> ReadCurve(XElement entry) {
        List<(float, float)> points = [];
        XElement? curve = entry.Element("valueBySeverity");
        Assert.IsNotNull(curve, $"{(string?)entry.Element("stat")} has no valueBySeverity");

        XElement? wrapper = curve!.Element("points");
        Assert.IsNotNull(
            wrapper,
            $"{(string?)entry.Element("stat")}: SimpleCurve points must sit inside <points>, not directly under the curve"
        );

        foreach (XElement point in wrapper!.Elements("li")) {
            string[] parts = point.Value.Trim('(', ')', ' ').Split(',');
            points.Add((float.Parse(parts[0]), float.Parse(parts[1])));
        }

        return points;
    }
}
