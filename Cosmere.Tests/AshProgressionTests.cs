using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using Cosmere.System.Scadrial.Grid;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Replays the ash beats out of the four arc XMLs in the order a campaign hits them. Catches
///     a beat authored out of order or a step that walks the severity back, neither of which the
///     game complains about - it just quietly stops raining ash halfway through Hero of Ages.
/// </summary>
[TestClass]
public class AshProgressionTests {
    private const string SetIntensity = "Cosmere.System.Scadrial.ScenarioPart.Action.SetAshfallIntensityAction";
    private const string EndAshfall = "Cosmere.System.Scadrial.ScenarioPart.Action.EndAshfallAction";
    private const string DaysTrigger = "Cosmere.Core.ScenarioPart.Trigger.DaysPassedTrigger";

    /// <summary>The order a campaign walks the arcs. Each one restarts its own day clock.</summary>
    private static readonly string[] ArcOrder = [
        "FinalEmpire", "WellOfAscension", "HeroOfAges", "PostCatacendre",
    ];

    private static string RepoRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null &&
                   !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs", "ScenarioProgression"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate CosmereScadrial/Defs/ScenarioProgression above the test output.");
            return dir!.FullName;
        }
    }

    private sealed record Beat(string Arc, string Key, int Day, bool Ends, float Severity);

    [TestMethod]
    public void SeverityNeverWalksBackAcrossTheCampaign() {
        List<Beat> beats = Campaign();
        Assert.IsTrue(beats.Count >= 9, $"only {beats.Count} ash beats authored; the four arcs should carry nine");

        float previous = -1f;
        for (int i = 0; i < beats.Count; i++) {
            Beat beat = beats[i];
            if (beat.Ends) {
                Assert.AreEqual(
                    beats.Count - 1, i, "EndAshfallAction has to be the last beat, or a later step restarts the fall"
                );

                continue;
            }

            Assert.IsTrue(
                beat.Severity >= previous,
                $"{beat.Arc}/{beat.Key} drops severity from {previous} to {beat.Severity}"
            );
            previous = beat.Severity;
        }
    }

    [TestMethod]
    public void EveryArcOpensOnItsOwnBeatAndTheRampEndsAtFull() {
        List<Beat> beats = Campaign();

        for (int a = 0; a < ArcOrder.Length; a++) {
            string arc = ArcOrder[a];
            bool opens = false;
            for (int i = 0; i < beats.Count; i++) {
                if (beats[i].Arc == arc && beats[i].Day == 0) opens = true;
            }

            Assert.IsTrue(opens, $"{arc} has no ash beat at arc start, so a handoff into it keeps the old severity");
        }

        Assert.AreEqual(0.15f, beats[0].Severity, 0.0001f, "the Final Empire has to open on the tracker's baseline");

        float peak = 0f;
        for (int i = 0; i < beats.Count; i++) {
            if (!beats[i].Ends && beats[i].Severity > peak) peak = beats[i].Severity;
        }

        Assert.AreEqual(1f, peak, 0.0001f, "the ramp never reaches full weight");
    }

    [TestMethod]
    public void TheCatacendreLeavesTheTargetAtExactlyZero() {
        List<Beat> beats = Campaign();
        Beat last = beats[beats.Count - 1];

        Assert.IsTrue(last.Ends, "the campaign does not end on EndAshfallAction");
        Assert.AreEqual("PostCatacendre", last.Arc);
        Assert.AreEqual(0f, Replay(beats), 0f, "post-Catacendre the target must be exactly zero, not merely small");
    }

    /// <summary>
    ///     Walks the real easing a day at a time across the whole campaign. Beats that land while
    ///     the curve is still climbing towards the previous one still have to leave it climbing.
    /// </summary>
    [TestMethod]
    public void TheEasedCurveClimbsAndThenDrainsToNothing() {
        List<Beat> beats = Campaign();

        // each arc gets the same span, past its last authored beat, so the curve arrives before the next arc.
        const int ArcSpanDays = 95;
        List<(int day, Beat beat)> timeline = new List<(int, Beat)>();
        for (int i = 0; i < beats.Count; i++) {
            timeline.Add((Array.IndexOf(ArcOrder, beats[i].Arc) * ArcSpanDays + beats[i].Day, beats[i]));
        }

        float severity = 0f;
        float target = 0f;
        float highest = 0f;
        bool ended = false;
        int next = 0;

        for (int day = 0; day <= ArcOrder.Length * ArcSpanDays; day++) {
            while (next < timeline.Count && timeline[next].day <= day) {
                Beat beat = timeline[next].beat;
                target = beat.Ends ? 0f : beat.Severity;
                ended |= beat.Ends;
                next++;
            }

            severity = AshDepthMath.EaseSeverity(severity, target, AshDepthMath.SeverityEasePerDay, 1f);

            if (!ended) {
                Assert.IsTrue(
                    severity >= highest - 0.0001f, $"day {day}: severity dipped from {highest} to {severity}"
                );
            }

            if (severity > highest) highest = severity;
        }

        Assert.AreEqual(1f, highest, 0.0001f, $"the curve peaked at {highest} rather than full weight");
        Assert.AreEqual(0f, severity, 0.0001f, "the drain never reaches zero");
    }

    private static float Replay(List<Beat> beats) {
        float target = 0f;
        for (int i = 0; i < beats.Count; i++) {
            target = beats[i].Ends ? 0f : beats[i].Severity;
        }

        return target;
    }

    /// <summary>Every ash beat in the four arcs, arc order first and day order within an arc.</summary>
    private static List<Beat> Campaign() {
        List<Beat> beats = new List<Beat>();

        for (int a = 0; a < ArcOrder.Length; a++) {
            List<Beat> arc = BeatsIn(ArcOrder[a]);
            arc.Sort((x, y) => x.Day.CompareTo(y.Day));
            beats.AddRange(arc);
        }

        return beats;
    }

    private static List<Beat> BeatsIn(string arc) {
        string path = Path.Combine(RepoRoot, "CosmereScadrial", "Defs", "ScenarioProgression", arc + ".xml");
        Assert.IsTrue(File.Exists(path), $"{arc}.xml is missing");

        List<Beat> beats = new List<Beat>();
        XElement? root = XDocument.Load(path).Root;
        Assert.IsNotNull(root, $"{arc}.xml has no root element");

        foreach (XElement def in root!.Elements()) {
            XElement? events = def.Element("events");
            if (events == null) continue;

            foreach (XElement evt in events.Elements()) {
                foreach (XElement action in evt.Element("actions")?.Elements() ?? []) {
                    string? cls = action.Attribute("Class")?.Value;
                    if (cls != SetIntensity && cls != EndAshfall) continue;

                    string key = evt.Element("key")?.Value ?? "(no key)";
                    beats.Add(new Beat(arc, key, DayOf(arc, key, evt), cls == EndAshfall, SeverityOf(arc, key, action)));
                }
            }
        }

        return beats;
    }

    /// <summary>
    ///     sinceArcStart is required, not optional. Without it a campaign that reaches an arc on
    ///     day 90 has already passed every threshold in it and fires the whole ramp in one tick.
    /// </summary>
    private static int DayOf(string arc, string key, XElement evt) {
        foreach (XElement trigger in evt.Element("triggers")?.Elements() ?? []) {
            if (trigger.Attribute("Class")?.Value != DaysTrigger) continue;

            Assert.AreEqual(
                "true",
                trigger.Element("sinceArcStart")?.Value,
                $"{arc}/{key}: ash beats must count from arc start"
            );

            return int.Parse(trigger.Element("days")?.Value ?? "0");
        }

        Assert.Fail($"{arc}/{key}: ash beat has no DaysPassedTrigger, so it fires on the first check of any save");
        return 0;
    }

    private static float SeverityOf(string arc, string key, XElement action) {
        string? raw = action.Element("severity")?.Value;
        if (raw == null) return 0f;

        float severity = float.Parse(raw, CultureInfo.InvariantCulture);
        Assert.IsTrue(severity is >= 0f and <= 1f, $"{arc}/{key}: severity {severity} is outside 0 to 1");
        return severity;
    }
}
