using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Cosmere.System.Scadrial.Grid;
using Cosmere.System.Scadrial.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     An eruption is three effects riding one clock, and every one of them is a number somebody
///     will want to move. These pin the arithmetic the shipped numbers imply, and the def wiring
///     vanilla insists on before it will send the letter at all.
/// </summary>
[TestClass]
public class AshEruptionTests {
    private const string IncidentDefName = "Cosmere_Scadrial_Incident_AshmountEruption";
    private const string ConditionDefName = "Cosmere_Scadrial_Condition_AshmountEruption";
    private const string WorkerClass = "Cosmere.System.Scadrial.Incident.Worker.IncidentWorker_AshmountEruption";
    private const string ConditionClass = "Cosmere.System.Scadrial.GameCondition.AshmountEruption";
    private const string VentDefName = "Cosmere_Scadrial_Thing_AshVent";
    private const string VentCompClass = "Cosmere.System.Scadrial.Comp.Thing.CompProperties_AshVent";

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

    private static XElement DefNamed(string element, string defName) {
        string root = Path.Combine(RepoRoot, "CosmereScadrial", "Defs");
        foreach (string path in Directory.GetFiles(root, "*.xml", SearchOption.AllDirectories)) {
            XElement? doc = XDocument.Load(path).Root;
            if (doc == null) continue;

            foreach (XElement def in doc.Elements(element)) {
                if (def.Element("defName")?.Value.Trim() == defName) return def;
            }
        }

        Assert.Fail($"No {element} named {defName} under CosmereScadrial/Defs.");

        return null!;
    }

    private static string Field(XElement def, string name) {
        string? value = def.Element(name)?.Value.Trim();
        Assert.IsFalse(string.IsNullOrEmpty(value), $"{def.Element("defName")?.Value} carries no {name}.");

        return value!;
    }

    /// <summary>The condition's own source, for the checks a Verse-bound tick cannot be run for.</summary>
    private static string ConditionSource {
        get {
            string path = Path.Combine(
                RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "GameCondition", "AshmountEruption.cs"
            );

            Assert.IsTrue(File.Exists(path), $"Expected the eruption condition at {path}");

            return File.ReadAllText(path);
        }
    }

    /// <summary>Whether any C# file under CosmereCore declares this fully qualified class.</summary>
    private static bool ClassIsDeclared(string qualified) {
        int split = qualified.LastIndexOf('.');
        string ns = qualified.Substring(0, split);
        string name = qualified.Substring(split + 1);
        string root = Path.Combine(RepoRoot, "CosmereCore", "CosmereCore");

        foreach (string path in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories)) {
            string source = File.ReadAllText(path);
            if (!source.Contains($"namespace {ns};")) continue;
            if (source.Contains($"class {name} ") || source.Contains($"class {name}:")) return true;
        }

        return false;
    }

    /// <summary>The eruption lifts whatever the arc is already pushing, it does not replace it.</summary>
    [TestMethod]
    public void TheSpikeLiftsTheStandingPressure() {
        Assert.AreEqual(
            AshPressure.Default + AshEruption.PeakStep, AshEruption.SpikeSeverity(AshPressure.Default), 0.0001f
        );

        Assert.AreEqual(0.6f + AshEruption.PeakStep, AshEruption.SpikeSeverity(0.6f), 0.0001f);
    }

    /// <summary>
    ///     Severity is a 0 to 1 dial and the tracker clamps anything it is handed. An eruption late
    ///     in Hero of Ages has nowhere left to climb, which is correct - the sky is already full.
    /// </summary>
    [TestMethod]
    public void TheSpikeStopsAtAFullSky() {
        Assert.AreEqual(1f, AshEruption.SpikeSeverity(0.9f), 0.0001f);
        Assert.AreEqual(1f, AshEruption.SpikeSeverity(1f), 0.0001f);
        Assert.AreEqual(0f, AshEruption.SpikeSeverity(-1f), 0.0001f);
    }

    /// <summary>
    ///     The step is stated as a severity, but what the player sees is millimetres. The shaping
    ///     curve is not linear, so the two drift apart the moment either is retuned alone.
    /// </summary>
    [TestMethod]
    public void TheSpikeMultipliesTheFallRateByTheStatedFigure() {
        float standing = AshDepthMath.FallRateMmPerHour(AshPressure.Default, 1f);
        float erupting = AshDepthMath.FallRateMmPerHour(AshEruption.SpikeSeverity(AshPressure.Default), 1f);
        float multiple = erupting / standing;

        Assert.IsTrue(
            multiple > 7.5f && multiple < 9f,
            $"the eruption multiplies the Final Empire fall rate by {multiple:0.00}, not the 8.2 the comment claims."
        );
    }

    /// <summary>The mountain does not clear its throat for six hours before anything happens.</summary>
    [TestMethod]
    public void TheFirstBeatLandsOnTheOpeningTick() {
        Assert.IsTrue(AshEruption.DueOn(0, 90000, AshEruption.ThrowsPerVent));
        Assert.IsTrue(AshEruption.DueOn(0, 90000, AshEruption.TremorCount));
    }

    /// <summary>
    ///     Exactly as many beats as asked for, whatever the duration. A run that divides evenly is
    ///     the case that buys a free extra beat on the closing tick if the ceiling is missing.
    /// </summary>
    [TestMethod]
    public void EveryRunGivesExactlyTheBeatsAskedFor() {
        int[] durations = [60000, 90000, 120000, 97531];
        int[] counts = [AshEruption.ThrowsPerVent, AshEruption.TremorCount, 1];

        foreach (int duration in durations) {
            foreach (int count in counts) {
                int fired = 0;

                // Inclusive: a condition is not expired until the tick AFTER its duration runs out,
                // so the closing tick really does get a GameConditionTick.
                for (int tick = 0; tick <= duration; tick++) {
                    if (AshEruption.DueOn(tick, duration, count)) fired++;
                }

                Assert.AreEqual(count, fired, $"a {duration} tick run gave {fired} of {count} beats.");
            }
        }
    }

    /// <summary>Evenly spaced, or the eruption front-loads and then goes quiet for a day.</summary>
    [TestMethod]
    public void TheBeatsAreEvenlySpaced() {
        const int duration = 90000;
        List<int> fired = [];

        for (int tick = 0; tick <= duration; tick++) {
            if (AshEruption.DueOn(tick, duration, AshEruption.TremorCount)) fired.Add(tick);
        }

        Assert.AreEqual(AshEruption.TremorCount, fired.Count);

        int gap = fired[1] - fired[0];
        for (int i = 2; i < fired.Count; i++) {
            Assert.AreEqual(gap, fired[i] - fired[i - 1], $"beat {i} sat {fired[i] - fired[i - 1]} ticks out, not {gap}.");
        }

        Assert.IsTrue(fired[^1] < duration, "the last beat landed on or past the closing tick.");
    }

    /// <summary>
    ///     A dev-set duration of nothing, or a count of nothing, has to fall through rather than
    ///     divide by zero inside a tick the game swallows the exception from.
    /// </summary>
    [TestMethod]
    public void DegenerateRunsFireNothing() {
        Assert.IsFalse(AshEruption.DueOn(0, 0, 4), "a zero length run still beat.");
        Assert.IsFalse(AshEruption.DueOn(0, -1, 4), "a negative run still beat.");
        Assert.IsFalse(AshEruption.DueOn(0, 90000, 0), "a run wanting no beats still beat.");
        Assert.IsFalse(AshEruption.DueOn(-1, 90000, 4), "a tick before the start still beat.");
    }

    /// <summary>
    ///     A run shorter than the beats it owes packs them one per tick instead of dropping them.
    ///     Only a dev tool can produce this, but silently skipping beats would hide the mistake.
    /// </summary>
    [TestMethod]
    public void ARunShorterThanItsBeatsPacksThem() {
        for (int tick = 0; tick < 6; tick++) {
            Assert.IsTrue(AshEruption.DueOn(tick, 3, 6), $"tick {tick} of a packed run was skipped.");
        }

        Assert.IsFalse(AshEruption.DueOn(6, 3, 6), "a packed run kept beating past its count.");
    }

    /// <summary>Full strength on the mouth, nothing at the edge, and nothing at all past it.</summary>
    [TestMethod]
    public void ATremorFallsOffToNothing() {
        Assert.AreEqual(18, AshEruption.TremorDamage(0f, 8f, 18));
        Assert.AreEqual(9, AshEruption.TremorDamage(4f, 8f, 18));
        Assert.AreEqual(0, AshEruption.TremorDamage(8f, 8f, 18), "the edge ring still took damage.");
        Assert.AreEqual(0, AshEruption.TremorDamage(20f, 8f, 18), "the tremor reached past its radius.");
    }

    /// <summary>Nothing here may divide by a radius of zero or hand back a negative.</summary>
    [TestMethod]
    public void ATremorWithNoReachDoesNothing() {
        Assert.AreEqual(0, AshEruption.TremorDamage(0f, 0f, 18));
        Assert.AreEqual(0, AshEruption.TremorDamage(-1f, 8f, 18));
        Assert.AreEqual(0, AshEruption.TremorDamage(1f, 8f, 0));
    }

    /// <summary>
    ///     The whole eruption's tremor budget against something built on the mouth itself. This is
    ///     meant to be a warning worth heeding and not a demolition, so the figure is stated rather
    ///     than left to fall out of two constants nobody checks together.
    /// </summary>
    [TestMethod]
    public void TheWholeEruptionTakes108OffTheMouth() {
        int perTremor = AshEruption.TremorDamage(0f, AshEruption.TremorRadiusCells, AshEruption.TremorPeakDamage);

        Assert.AreEqual(108, perTremor * AshEruption.TremorCount, "the tremor budget moved without the comment moving.");
    }

    /// <summary>
    ///     One TakeDamage can take several things off a cell at once - a dying shelf hands its
    ///     overflow to a neighbour - so the tremor has to walk a copy and not the grid's own list.
    /// </summary>
    [TestMethod]
    public void TheTremorWalksACopyOfTheCellAndNotTheGridsOwnList() {
        string source = ConditionSource;

        Assert.AreEqual(
            1,
            Regex.Matches(source, @"ThingsListAtFast\s*\(").Count,
            "the condition reads the thing grid's live list somewhere new, which this check does not cover."
        );

        Assert.IsTrue(
            Regex.IsMatch(source, @"AddRange\(\s*map\.thingGrid\.ThingsListAtFast\s*\("),
            "the tremor indexes the thing grid's own list. Destroying a shelf relocates its overflow inside the "
            + "same TakeDamage, which takes more than one entry off that list and throws on the next index."
        );
    }

    /// <summary>
    ///     The Catacendre can land inside a 1 to 2 day eruption. Start and end both check the era,
    ///     so the tick has to as well or the vents keep paying out after the Ashmounts are gone.
    /// </summary>
    [TestMethod]
    public void TheEruptionStopsItsPerTickEffectsAtTheCatacendre() {
        Match tick = Regex.Match(
            ConditionSource, @"public override void GameConditionTick\(\)\s*\{(.*?)\n    \}", RegexOptions.Singleline
        );

        Assert.IsTrue(tick.Success, "could not find GameConditionTick on the eruption condition.");

        Assert.IsTrue(
            tick.Groups[1].Value.Contains("AshEra.CanAccumulate"),
            "GameConditionTick does not check the era. An eruption in flight when the Catacendre lands keeps "
            + "throwing metal and shaking buildings, and ThrowOnce carries no gate of its own."
        );
    }

    /// <summary>The vent's own comp block, which is where the everyday throw cadence is tuned.</summary>
    private static XElement VentComp() {
        XElement vent = DefNamed("ThingDef", VentDefName);

        foreach (XElement li in vent.Element("comps")?.Elements("li") ?? []) {
            if (li.Attribute("Class")?.Value.Trim() == VentCompClass) return li;
        }

        Assert.Fail($"{VentDefName} carries no {VentCompClass}.");

        return null!;
    }

    /// <summary>
    ///     The vent sweeps its own apron clear, which is exactly the ground a player builds on. A
    ///     tremor that stopped inside that apron would never touch anything anybody put up, and one
    ///     that reached past the plume would shake ground the vent has no other claim on.
    /// </summary>
    [TestMethod]
    public void ATremorReachesPastTheApronAndStopsInsideThePlume() {
        float clearRadius = float.Parse(Field(VentComp(), "clearRadius"), CultureInfo.InvariantCulture);

        Assert.IsTrue(
            clearRadius + 1f < AshEruption.TremorRadiusCells,
            $"a tremor reaches {AshEruption.TremorRadiusCells} cells and the vent sweeps {clearRadius} clear."
        );

        Assert.IsTrue(
            AshEruption.TremorRadiusCells <= AshPlume.RadiusCells,
            $"a tremor reaches {AshEruption.TremorRadiusCells} cells, past the {AshPlume.RadiusCells} the plume covers."
        );
    }

    /// <summary>
    ///     The incident def sells the eruption as roughly eight days of what the vents give anyway.
    ///     That figure is two numbers multiplied together in different files, which is exactly the
    ///     shape of claim that quietly stops being true.
    /// </summary>
    [TestMethod]
    public void TheEruptionPaysTheEightDaysOfMetalTheDefClaims() {
        float throwsPerDay = float.Parse(Field(VentComp(), "throwsPerDay"), CultureInfo.InvariantCulture);

        Assert.AreEqual(
            8f,
            AshEruption.ThrowsPerVent / throwsPerDay,
            0.001f,
            "the eruption no longer pays the eight days of throws the incident def claims."
        );
    }

    /// <summary>
    ///     The storyteller has to be able to pick it, and the eruption has to end on its own. A
    ///     baseChance of zero is how the progression-only incidents in this mod are written, and
    ///     copying one of those would leave this def loaded and unreachable.
    /// </summary>
    [TestMethod]
    public void TheIncidentIsSomethingTheStorytellerCanPick() {
        XElement incident = DefNamed("IncidentDef", IncidentDefName);

        Assert.IsTrue(
            float.Parse(Field(incident, "baseChance"), CultureInfo.InvariantCulture) > 0f,
            "the eruption has a baseChance of zero, so no storyteller will ever fire it."
        );

        Assert.AreEqual(WorkerClass, Field(incident, "workerClass"));
        Assert.AreEqual(ConditionDefName, Field(incident, "gameCondition"));
        Assert.IsTrue(Field(incident, "durationDays").Contains('~'), "durationDays belongs on the def as a range.");
        Assert.IsTrue(float.Parse(Field(incident, "minRefireDays"), CultureInfo.InvariantCulture) > 0f);
    }

    /// <summary>
    ///     Vanilla's IncidentWorker_MakeGameCondition reads the letter LABEL off the incident and
    ///     the letter BODY off the condition. Write the body on the incident like every other
    ///     incident def in this mod and the eruption fires in complete silence.
    /// </summary>
    [TestMethod]
    public void TheLetterIsSplitTheWayVanillaReadsIt() {
        XElement incident = DefNamed("IncidentDef", IncidentDefName);
        XElement condition = DefNamed("GameConditionDef", ConditionDefName);

        Assert.IsFalse(
            string.IsNullOrEmpty(incident.Element("letterLabel")?.Value.Trim()),
            "no letterLabel on the incident, so vanilla sends nothing."
        );

        Assert.IsFalse(
            string.IsNullOrEmpty(incident.Element("letterDef")?.Value.Trim()), "no letterDef on the incident."
        );

        Assert.IsFalse(
            string.IsNullOrEmpty(condition.Element("letterText")?.Value.Trim()),
            "no letterText on the condition, so vanilla sends nothing however full the incident is."
        );

        Assert.IsNull(
            incident.Element("letterText"), "letterText on the incident is read by nothing and will mislead the next edit."
        );
    }

    /// <summary>An eruption that could go permanent would hold the ashfall spiked forever.</summary>
    [TestMethod]
    public void TheConditionIsWiredToOurClassAndAlwaysEnds() {
        XElement condition = DefNamed("GameConditionDef", ConditionDefName);

        Assert.AreEqual(ConditionClass, Field(condition, "conditionClass"));
        Assert.AreEqual("false", Field(condition, "canBePermanent"));
        Assert.IsFalse(
            string.IsNullOrEmpty(condition.Element("endMessage")?.Value.Trim()),
            "no endMessage, so the eruption stops with nothing telling the player it did."
        );
    }

    /// <summary>
    ///     A class name in XML is a string until the game loads, and a wrong one costs a red error
    ///     at startup and an incident that never runs.
    /// </summary>
    [TestMethod]
    public void BothClassNamesResolveToSomethingThatExists() {
        Assert.IsTrue(ClassIsDeclared(WorkerClass), $"{WorkerClass} is named by a def but declared nowhere.");
        Assert.IsTrue(ClassIsDeclared(ConditionClass), $"{ConditionClass} is named by a def but declared nowhere.");
    }
}
