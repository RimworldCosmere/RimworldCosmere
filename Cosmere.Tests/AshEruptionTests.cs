using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Cosmere.System.Scadrial.Comp.Map;
using Cosmere.System.Scadrial.Grid;
using Cosmere.System.Scadrial.Util;
using Cosmere.System.Scadrial.World;
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

    /// <summary>Beats of one kind a run of this length actually fires, counted rather than divided.</summary>
    private static int BeatsIn(int durationTicks, int count) {
        int fired = 0;

        for (int tick = 0; tick <= durationTicks; tick++) {
            if (AshEruption.DueOn(tick, durationTicks, count)) fired++;
        }

        return fired;
    }

    /// <summary>Throws and tremors together, per game hour, for an eruption rolled this many days.</summary>
    private static float BeatsPerHour(float days) {
        int duration = (int)(days * TicksPerDay);
        int beats = BeatsIn(duration, AshEruption.ThrowsPerVent) + BeatsIn(duration, AshEruption.TremorCount);

        return beats / (days * 24f);
    }

    /// <summary>
    ///     What the player actually feels: how often anything at all happens. Stated at both ends of
    ///     the rolled duration, because the two disagree and a rate pinned at one of them says nothing
    ///     about the other.
    /// </summary>
    [TestMethod]
    public void SomethingHappensAboutEveryGameHourAcrossTheWholeRolledDuration() {
        (float min, float max) = DurationDays();

        Assert.AreEqual(
            1.42f, BeatsPerHour(min), 0.01f, $"the shortest eruption now beats {BeatsPerHour(min):0.00} times an hour."
        );

        Assert.AreEqual(
            0.71f, BeatsPerHour(max), 0.01f, $"the longest eruption now beats {BeatsPerHour(max):0.00} times an hour."
        );

        Assert.IsTrue(
            BeatsPerHour(max) > 0.5f,
            "the longest eruption now goes over two game hours between beats, which is the cadence the player read as "
            + "nothing happening."
        );
    }

    /// <summary>
    ///     The two beats have to stay tellable apart. Any shared factor and the rarer one lands inside
    ///     the commoner one every time, stacking both shakes and costing the tremor its own identity.
    /// </summary>
    [TestMethod]
    public void ATremorNeverLandsInsideAThrowExceptOnTheOpeningTick() {
        Assert.AreEqual(
            1,
            Gcd(AshEruption.ThrowsPerVent, AshEruption.TremorCount),
            $"{AshEruption.ThrowsPerVent} throws and {AshEruption.TremorCount} tremors share a factor."
        );

        int[] durations = [60000, 75000, 90000, 105000, 120000];

        foreach (int duration in durations) {
            for (int tick = 1; tick <= duration; tick++) {
                bool throwing = AshEruption.DueOn(tick, duration, AshEruption.ThrowsPerVent);
                if (!throwing) continue;

                Assert.IsFalse(
                    AshEruption.DueOn(tick, duration, AshEruption.TremorCount),
                    $"a {duration} tick run put a tremor and a throw on tick {tick}."
                );
            }
        }
    }

    private static int Gcd(int a, int b) {
        while (b != 0) {
            (a, b) = (b, a % b);
        }

        return a;
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

    /// <summary>Hit points one tremor takes off something standing this many cells out from a mouth.</summary>
    private static int TremorAt(int cells) {
        return AshEruption.TremorDamage(cells, AshEruption.TremorRadiusCells, AshEruption.TremorPeakDamage);
    }

    /// <summary>What a whole eruption's worth of tremors takes off something that far out.</summary>
    private static int EruptionTremorsAt(int cells) {
        return TremorAt(cells) * AshEruption.TremorCount;
    }

    /// <summary>
    ///     The shipped falloff, ring by ring, rather than at the two ends a curve of any shape would
    ///     agree on. A square falloff matches at the mouth and at the edge and nowhere between.
    /// </summary>
    [TestMethod]
    public void ATremorGivesUpTheSameHitPointsEveryRingOut() {
        int step = (int)(AshEruption.TremorPeakDamage / AshEruption.TremorRadiusCells);

        Assert.AreEqual(5, step, "the falloff no longer lands on whole hit points a ring, so the table below drifts.");

        for (int ring = 0; ring <= (int)AshEruption.TremorRadiusCells; ring++) {
            Assert.AreEqual(
                AshEruption.TremorPeakDamage - step * ring,
                TremorAt(ring),
                $"ring {ring} took something other than the straight falloff the constants describe."
            );
        }
    }

    /// <summary>A wall of wood: 300 hit points on the def, times the 0.65 wood carries as stuff.</summary>
    private const int WoodenWallHitPoints = 195;

    /// <summary>A shelf of wood: 100 on the def, times the same 0.65.</summary>
    private const int WoodenShelfHitPoints = 65;

    /// <summary>A battery is built from a costList rather than stuff, so it keeps the def's 100.</summary>
    private const int BatteryHitPoints = 100;

    /// <summary>
    ///     One tremor has to move the health bar or the player reads the eruption as scenery. The old
    ///     peak of 18 put a twentieth of a wooden wall out of reach at this distance; this is the pin
    ///     that failed on it.
    /// </summary>
    [TestMethod]
    public void OneTremorTakesABiteOutOfAWoodenWallTheHealthBarWillShow() {
        Assert.IsTrue(
            TremorAt(4) * 20 >= WoodenWallHitPoints,
            $"one tremor four cells out takes {TremorAt(4)} off a {WoodenWallHitPoints} point wooden wall, under the "
            + "twentieth that shows as damage."
        );
    }

    /// <summary>
    ///     A whole eruption ruins a wooden wall built just outside the vent's clearing without felling
    ///     it, and fells one built inside. The letter's warning about building near a mouth, in numbers.
    /// </summary>
    [TestMethod]
    public void AnEruptionLeavesAWoodenWallStandingOutsideTheVentsClearing() {
        Assert.AreEqual(180, EruptionTremorsAt(4), "the tremor budget four cells out moved without this moving.");

        Assert.IsTrue(
            EruptionTremorsAt(4) < WoodenWallHitPoints,
            $"an eruption now takes {EruptionTremorsAt(4)} off a {WoodenWallHitPoints} point wooden wall four cells "
            + "out, which fells it. A tremor is meant to be a warning worth heeding, not a demolition."
        );

        Assert.IsTrue(
            EruptionTremorsAt(3) > WoodenWallHitPoints,
            "a wooden wall inside the vent's own clearing now survives an eruption, so the ground near a mouth costs "
            + "the player nothing."
        );
    }

    /// <summary>
    ///     The same tremors against two things that are not walls. Stated at two distances that
    ///     disagree, because a budget checked at one point cannot tell a threat from a flattening.
    /// </summary>
    [TestMethod]
    public void TheSameTremorsFlattenAShelfAndABatteryBuiltAgainstTheClearing() {
        Assert.IsTrue(
            EruptionTremorsAt(4) > BatteryHitPoints,
            $"a battery four cells from a mouth now survives an eruption on {BatteryHitPoints - EruptionTremorsAt(4)} "
            + "hit points."
        );

        Assert.IsTrue(
            EruptionTremorsAt(6) > WoodenShelfHitPoints,
            "a wooden shelf six cells from a mouth now survives an eruption, so nothing outside the clearing is ever "
            + "at risk."
        );

        Assert.IsTrue(
            EruptionTremorsAt(6) < BatteryHitPoints,
            $"an eruption now takes {EruptionTremorsAt(6)} off a battery six cells out, which fells it. The reach is "
            + "meant to thin to a nuisance well before it runs out."
        );
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

    /// <summary>Ticks a stripe waits between visits, so also the tracker's real sampling rate.</summary>
    private const int Stripes = 64;

    private const int TicksPerDay = 60000;
    private const int TicksPerHour = 2500;

    /// <summary>
    ///     Millimetres the eruption puts down over and above the Final Empire, walked the way the
    ///     tracker walks it: ease once per stripe visit, then deposit at whatever severity that left.
    /// </summary>
    private static float ExtraMillimetres(float durationDays, float exposure) {
        float full = AshDepthTracker.BaseRateMmPerDay * exposure;
        int duration = (int)(durationDays * TicksPerDay);

        // Long enough for the ease to walk the whole spike back off, plus one visit of slack.
        int span = duration + (int)(AshEruption.PeakStep / AshDepthMath.SeverityEasePerDay * TicksPerDay) + Stripes;

        float severity = AshEruption.SpikeSeverity(AshPressure.Default);
        float target = severity;
        float erupting = 0f;
        float standing = 0f;

        for (int tick = 0; tick <= span; tick += Stripes) {
            if (tick > duration) target = AshPressure.Default;

            severity = AshDepthMath.EaseSeverity(
                severity, target, AshDepthMath.SeverityEasePerDay, Stripes / (float)TicksPerDay
            );

            erupting += AshDepthMath.FallRateMmPerHour(severity, full) * (Stripes / (float)TicksPerHour);
            standing += AshDepthMath.FallRateMmPerHour(AshPressure.Default, full) * (Stripes / (float)TicksPerHour);
        }

        return erupting - standing;
    }

    /// <summary>The rolled range the incident def hands vanilla as the condition's duration.</summary>
    private static (float min, float max) DurationDays() {
        string[] parts = Field(DefNamed("IncidentDef", IncidentDefName), "durationDays").Split('~');
        Assert.AreEqual(2, parts.Length, "durationDays is not a range.");

        return (
            float.Parse(parts[0], CultureInfo.InvariantCulture), float.Parse(parts[1], CultureInfo.InvariantCulture)
        );
    }

    /// <summary>
    ///     The one figure the eruption is tuned on that a player reads as depth rather than as a
    ///     dial. Held as a range because durationDays is rolled, and pinned because PeakStep, the
    ///     ease cap and the def's duration all move it and all live in different files.
    /// </summary>
    [TestMethod]
    public void TheEruptionLeavesBetween900AndAMetreAndAThirdOfExtraAshOnTheWorstTile() {
        (float min, float max) = DurationDays();

        float shortest = ExtraMillimetres(min, AshmountExposure.MaxMultiplier);
        float longest = ExtraMillimetres(max, AshmountExposure.MaxMultiplier);

        Assert.AreEqual(916f, shortest, 5f, $"the shortest eruption now leaves {shortest:0} mm rather than 916.");
        Assert.AreEqual(1311f, longest, 5f, $"the longest eruption now leaves {longest:0} mm rather than 1311.");
    }

    /// <summary>
    ///     Every roll buries what is standing on the ground, which is the point. Nothing stops the
    ///     longest roll turning the ground itself to ash terrain either, so state both.
    /// </summary>
    [TestMethod]
    public void EvenTheShortestEruptionBuriesTheWorstTile() {
        (float min, float max) = DurationDays();

        Assert.IsTrue(
            ExtraMillimetres(min, AshmountExposure.MaxMultiplier) > AshDepthMath.BuriedMm,
            $"the shortest eruption no longer clears the {AshDepthMath.BuriedMm} mm that hides and unhauls items."
        );

        Assert.IsTrue(
            ExtraMillimetres(max, AshmountExposure.MaxMultiplier) > AshDepthMath.TerrainSwapMm,
            $"the longest eruption no longer clears the {AshDepthMath.TerrainSwapMm} mm that turns ground to ash."
        );
    }

    /// <summary>
    ///     Exposure is a flat multiplier on the fall rate, so an ordinary tile takes a third of
    ///     what the heartland does. A tile that only gets a nuisance dusting is the intended floor.
    /// </summary>
    [TestMethod]
    public void AnUnexposedTileTakesAThirdOfWhatTheHeartlandTakes() {
        (float min, float max) = DurationDays();
        float midpoint = (min + max) / 2f;

        float open = ExtraMillimetres(midpoint, 1f);
        float heartland = ExtraMillimetres(midpoint, AshmountExposure.MaxMultiplier);

        Assert.AreEqual(371f, open, 5f, $"an ordinary tile now takes {open:0} mm rather than 371.");
        Assert.AreEqual(AshmountExposure.MaxMultiplier, heartland / open, 0.01f);
        Assert.IsTrue(open < AshDepthMath.BuriedMm, "an eruption now buries an ordinary tile as well.");
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
    ///     The incident def sells the eruption as roughly fifty days of what the vents give anyway,
    ///     and the condition's letter promises more than a season's worth. That is three numbers in
    ///     three files, which is exactly the shape of claim that quietly stops being true.
    /// </summary>
    [TestMethod]
    public void TheEruptionPaysTheFiftyDaysOfMetalTheDefClaims() {
        const float seasonDays = 15f;
        float throwsPerDay = float.Parse(Field(VentComp(), "throwsPerDay"), CultureInfo.InvariantCulture);
        float days = AshEruption.ThrowsPerVent / throwsPerDay;

        Assert.AreEqual(50f, days, 0.001f, "the eruption no longer pays the fifty days of throws the incident def claims.");

        Assert.IsTrue(
            days > seasonDays,
            $"an eruption showers {days:0} days of vent metal, and the condition's letter promises more than the "
            + $"{seasonDays:0} days a season gives."
        );
    }

    /// <summary>Vanilla clamps every request to this, times the player's own shake intensity setting.</summary>
    private const float VanillaMaxShake = 0.2f;

    /// <summary>
    ///     Both magnitudes sit inside what vanilla already spends on a building falling over, and the
    ///     tremor reads as the bigger of the two. This fires three dozen times a run.
    /// </summary>
    [TestMethod]
    public void TheShakeStaysInsideWhatVanillaSpendsOnABuildingFallingOver() {
        Assert.AreEqual(0.07f, AshEruption.ThrowShake, 0.0001f, "the throw shake moved off vanilla's one-cell collapse.");
        Assert.AreEqual(0.15f, AshEruption.TremorShake, 0.0001f, "the tremor shake moved off vanilla's collapse curve.");

        Assert.IsTrue(AshEruption.ThrowShake > 0f, "a throw asks the camera for nothing at all.");

        Assert.IsTrue(
            AshEruption.ThrowShake < AshEruption.TremorShake,
            "a throw now shakes the screen at least as hard as the ground moving, so the two stop being tellable apart."
        );

        Assert.IsTrue(
            AshEruption.TremorShake < VanillaMaxShake,
            $"a tremor asks for {AshEruption.TremorShake}, which saturates vanilla's own {VanillaMaxShake} ceiling."
        );
    }

    /// <summary>
    ///     The shake is global to the camera and the eruption is not. Vanilla's own callers all check
    ///     the map is the one on screen, and without it a caravan map rattles the colony being watched.
    /// </summary>
    [TestMethod]
    public void TheShakeOnlyReachesTheMapThePlayerIsLookingAt() {
        string source = ConditionSource;

        Assert.IsTrue(
            Regex.IsMatch(source, @"Find\.CameraDriver\.shaker\.DoShake\("),
            "the eruption asks for no camera shake at all, so a throw and a tremor both land in silence."
        );

        Assert.IsTrue(
            Regex.IsMatch(source, @"Find\.CurrentMap"),
            "the shake is not gated on the map the player is looking at."
        );
    }

    /// <summary>
    ///     One request a beat, not one a vent. Six vents on a mount-adjacent tile would stack six and
    ///     clamp to a full-strength jolt, which is the shake players mod out.
    /// </summary>
    [TestMethod]
    public void TheShakeIsAskedForOncePerBeatAndNotOncePerVent() {
        Assert.AreEqual(
            1,
            Regex.Matches(ConditionSource, @"shaker\.DoShake\s*\(").Count,
            "the eruption asks the camera to shake from more than one place, so the count per beat is no longer one."
        );
    }

    /// <summary>
    ///     The shake is the throw's own cue, so it has to wait on the throw landing. Every candidate
    ///     cell inside the throw radius can be buried, and a jolt with no lump explains nothing.
    /// </summary>
    [TestMethod]
    public void TheThrowShakeWaitsForALumpToActuallyLand() {
        string source = ConditionSource;

        Assert.IsTrue(
            Regex.IsMatch(source, @"private static bool ThrowFromEveryVent"),
            "ThrowFromEveryVent no longer reports whether a lump landed, so the throw shake cannot wait on one."
        );

        Assert.IsTrue(
            Regex.IsMatch(source, @"ThrowsPerVent\)\s*&&\s*ThrowFromEveryVent"),
            "the throw beat shakes the screen whether or not a lump landed."
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
