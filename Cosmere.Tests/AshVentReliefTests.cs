using System;
using System.IO;
using System.Text.RegularExpressions;
using Cosmere.System.Scadrial.Comp.Map;
using Cosmere.System.Scadrial.Grid;
using Cosmere.System.Scadrial.Util;
using Cosmere.System.Scadrial.World;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Sealing a vent buys the map relief, and the cap on that relief is the number the endgame
///     turns on. Pinned through the curve that consumes it, not only as a constant sitting alone.
/// </summary>
[TestClass]
public class AshVentReliefTests {
    /// <summary>The last ash beat of Hero of Ages, and where the floor has to hold.</summary>
    private const float Endgame = 1f;

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

    private static string Source(params string[] parts) {
        string path = Path.Combine(RepoRoot, Path.Combine(parts));
        Assert.IsTrue(File.Exists(path), $"{path} is missing.");

        return File.ReadAllText(path);
    }

    private static string ScadrialSource(params string[] parts) {
        string[] full = new string[parts.Length + 4];
        full[0] = "CosmereCore";
        full[1] = "CosmereCore";
        full[2] = "System";
        full[3] = "Scadrial";
        Array.Copy(parts, 0, full, 4, parts.Length);

        return Source(full);
    }

    private static string ReliefSource => ScadrialSource("Util", "AshVentRelief.cs");

    private static string TrackerSource => ScadrialSource("Comp", "Map", "AshDepthTracker.cs");

    private static string VentSource => ScadrialSource("Comp", "Thing", "CompAshVent.cs");

    private static string GridSource => ScadrialSource("Grid", "AshGrid.cs");

    /// <summary>The one number the whole task exists to pin, at the severity he named it at.</summary>
    [TestMethod]
    public void SealingEveryVentAtTheEndgameLeavesExactlyHalf() {
        float relieved = AshVentRelief.Relieve(Endgame, AshVentSiting.MaxVents);

        Assert.AreEqual(0.5f, relieved, 1e-6f, "full containment at severity 1 no longer lands on 0.5.");
        Assert.IsTrue(relieved >= 0.5f, $"full containment took the endgame map to {relieved}, under the floor.");
    }

    /// <summary>Counts past the six a tile can carry are reachable from dev actions.</summary>
    [TestMethod]
    public void NoNumberOfSealedVentsTakesTheEndgameBelowHalf() {
        for (int sealedVents = 0; sealedVents <= AshVentSiting.MaxVents * 4; sealedVents++) {
            float relieved = AshVentRelief.Relieve(Endgame, sealedVents);

            Assert.IsTrue(relieved >= 0.5f, $"{sealedVents} sealed vents took the endgame map to {relieved}.");
        }
    }

    /// <summary>
    ///     A share, not a subtraction. Taking a flat 0.5 off the Final Empire's 0.15 leaves clear
    ///     sky on the tiles carrying the most vents, which is the one outcome he ruled out.
    /// </summary>
    [TestMethod]
    public void SealingEveryVentNeverStopsTheAshfallOutright() {
        float[] arc = [AshPressure.Default, 0.35f, 0.45f, 0.55f, 0.7f, 0.8f, 0.9f, 1f];

        for (int i = 0; i < arc.Length; i++) {
            float relieved = AshVentRelief.Relieve(arc[i], AshVentSiting.MaxVents);

            Assert.IsTrue(relieved > 0f, $"severity {arc[i]} falls to nothing once every vent is sealed.");
            Assert.IsTrue(
                AshDepthMath.FallRateMmPerHour(relieved, AshDepthTracker.BaseRateMmPerDay) > 0f,
                $"severity {arc[i]} deposits nothing once every vent is sealed."
            );
        }
    }

    /// <summary>
    ///     Every roof pays. A per-vent share that saturated early would leave the last roofs on a
    ///     six-vent tile worth nothing, and a roof that buys nothing reads as a bug.
    /// </summary>
    [TestMethod]
    public void EveryRoofOnTheWorstTileStillPays() {
        for (int sealedVents = 1; sealedVents <= AshVentSiting.MaxVents; sealedVents++) {
            Assert.IsTrue(
                AshVentRelief.Fraction(sealedVents) > AshVentRelief.Fraction(sealedVents - 1),
                $"the {sealedVents}th sealed vent buys the map nothing."
            );
        }
    }

    /// <summary>The cap is reached by the sixth vent and by nothing short of it.</summary>
    [TestMethod]
    public void OnlyAFullySealedWorstTileReachesTheCap() {
        Assert.AreEqual(
            AshVentRelief.MaxRelief,
            AshVentRelief.Fraction(AshVentSiting.MaxVents),
            1e-6f,
            "six sealed vents no longer add up to the cap."
        );

        Assert.IsTrue(
            AshVentRelief.Fraction(AshVentSiting.MaxVents - 1) < AshVentRelief.MaxRelief,
            "five sealed vents already reach the cap, so the sixth roof is wasted."
        );
    }

    /// <summary>
    ///     One vent is what most tiles get. Its roof is worth one share, not the whole cap - equal
    ///     shares would make the cheapest tile to seal the one that needed sealing least.
    /// </summary>
    [TestMethod]
    public void AOneVentTileBuysOneShareAndNotTheCap() {
        Assert.AreEqual(AshVentRelief.PerVent, AshVentRelief.Fraction(AshVentSiting.MinVents), 1e-6f);

        Assert.IsTrue(
            AshVentRelief.Fraction(AshVentSiting.MinVents) < AshVentRelief.MaxRelief,
            "one sealed vent buys the whole cap, so the tile with the least ash is the cheapest to relieve."
        );

        Assert.AreEqual(
            1f - AshVentRelief.PerVent,
            AshVentRelief.Relieve(Endgame, AshVentSiting.MinVents),
            1e-6f,
            "a lone sealed vent no longer takes one share off the endgame."
        );
    }

    /// <summary>An open map pays full price, or the relief is not relief at all.</summary>
    [TestMethod]
    public void AnUnsealedMapKeepsItsWholeSeverity() {
        Assert.AreEqual(0f, AshVentRelief.Fraction(0), 1e-6f);
        Assert.AreEqual(0f, AshVentRelief.Fraction(-3), 1e-6f);
        Assert.AreEqual(Endgame, AshVentRelief.Relieve(Endgame, 0), 1e-6f);
        Assert.AreEqual(AshPressure.Default, AshVentRelief.Relieve(AshPressure.Default, 0), 1e-6f);
    }

    /// <summary>
    ///     Severity is a dial; millimetres are what the player reads. Stated at two severities,
    ///     because the shaping curve happens to pass a halved endgame straight through unchanged.
    /// </summary>
    [TestMethod]
    public void FullContainmentHalvesTheEndgameFallRateAndCutsTheFinalEmpireDeeper() {
        float full = AshDepthTracker.BaseRateMmPerDay * AshmountExposure.MaxMultiplier;

        float open = AshDepthMath.FallRateMmPerHour(Endgame, full);
        float held = AshDepthMath.FallRateMmPerHour(AshVentRelief.Relieve(Endgame, AshVentSiting.MaxVents), full);

        Assert.AreEqual(
            0.5f, held / open, 0.001f, $"a fully sealed endgame map falls at {held / open:0.000} of an open one."
        );

        float standingOpen = AshDepthMath.FallRateMmPerHour(AshPressure.Default, full);
        float standingHeld = AshDepthMath.FallRateMmPerHour(
            AshVentRelief.Relieve(AshPressure.Default, AshVentSiting.MaxVents), full
        );

        Assert.AreEqual(
            0.264f,
            standingHeld / standingOpen,
            0.001f,
            $"a fully sealed Final Empire map falls at {standingHeld / standingOpen:0.000} of an open one."
        );

        Assert.IsTrue(
            standingHeld / standingOpen < held / open,
            "halving severity now takes the same fraction off the fall rate wherever it is applied, which is what a "
            + "linear rate would do. The shaping curve has stopped being consulted."
        );
    }

    /// <summary>
    ///     The sweep that actually deposits has to read the relieved figure. A cap nothing consumes
    ///     is the shape the mask filtration shipped in, and it passed its own tests too.
    /// </summary>
    [TestMethod]
    public void TheDepositSweepFallsAtTheRelievedSeverity() {
        Match sweep = Regex.Match(
            TrackerSource, @"private void AccumulateStripe\(int stripe\)\s*\{(.*?)\n    \}", RegexOptions.Singleline
        );

        Assert.IsTrue(sweep.Success, "could not find AccumulateStripe on the tracker.");

        Assert.IsTrue(
            Regex.IsMatch(sweep.Groups[1].Value, @"FallRateMmPerHour\(\s*EffectiveSeverity"),
            "AccumulateStripe still deposits at the raw severity, so sealing a vent changes nothing on the ground."
        );
    }

    /// <summary>Relief the player cannot see is relief the player will not believe.</summary>
    [TestMethod]
    public void TheVeilAndTheShaderShowTheRelievedSeverity() {
        Match update = Regex.Match(
            TrackerSource, @"public override void MapComponentUpdate\(\)\s*\{(.*?)\n    \}", RegexOptions.Singleline
        );

        Assert.IsTrue(update.Success, "could not find MapComponentUpdate on the tracker.");

        Assert.IsTrue(
            update.Groups[1].Value.Contains("EffectiveSeverity"),
            "the falling veil and the ground shader still run off the raw severity, so a sealed map looks unsealed."
        );
    }

    /// <summary>
    ///     Severity already has two writers and neither is this one. The relief is derived where it
    ///     is read, so a progression beat and a sealed vent can never overwrite each other.
    /// </summary>
    [TestMethod]
    public void ReliefNeverWritesTheArcsPressure() {
        Assert.IsFalse(ReliefSource.Contains("AshPressure"), "the relief maths reaches for the arc's pressure.");

        string vent = VentSource;
        Assert.IsFalse(vent.Contains("AshPressure"), "the vent writes the arc's game-wide pressure.");
        Assert.IsFalse(vent.Contains("SetSeverityTarget"), "the vent writes the map's severity target.");
        Assert.IsFalse(vent.Contains("SetSeverityNow"), "the vent snaps the map's severity.");
    }

    /// <summary>
    ///     The roof rule is load-bearing in the deposit sweep, the plume and the dev actions.
    ///     Containment is a second question asked on top of it, never a redefinition of it.
    /// </summary>
    [TestMethod]
    public void TheRoofRuleIsUnchanged() {
        Match rule = Regex.Match(
            GridSource, @"public bool CanHaveAsh\(IntVec3 cell\)\s*\{(.*?)\n    \}", RegexOptions.Singleline
        );

        Assert.IsTrue(rule.Success, "could not find CanHaveAsh on the grid.");

        Assert.AreEqual(
            "return !map.roofGrid.Roofed(cell);",
            rule.Groups[1].Value.Trim(),
            "CanHaveAsh was redefined. The deposit sweep, the plume and the dev actions all turn on it."
        );
    }

    /// <summary>
    ///     A vent relieves the map exactly when it stops feeding it - when the mouth is sealed into
    ///     a room small enough to hold the plume, not merely when roof tiles sit over it.
    /// </summary>
    [TestMethod]
    public void AVentCountsAsSealedOnlyWhileItFillsARoom() {
        Match contribute = Regex.Match(
            VentSource,
            @"public bool ContributeToGrid\(AshGrid grid, float dayFraction\)\s*\{(.*?)\n    \}",
            RegexOptions.Singleline
        );

        Assert.IsTrue(contribute.Success, "could not find ContributeToGrid on the vent.");

        string body = contribute.Groups[1].Value;
        int room = body.IndexOf("SealedRoom(", StringComparison.Ordinal);
        int held = body.IndexOf("contained = true;", StringComparison.Ordinal);
        int fill = body.IndexOf("FillRoom(", StringComparison.Ordinal);

        Assert.IsTrue(
            room >= 0 && held > room && fill > held,
            "the vent no longer counts itself sealed on the same branch that fills its room."
        );

        Assert.IsTrue(
            body.Contains("contained = false;"), "an opened vent never stops relieving the map it drifts over again."
        );
    }

    /// <summary>
    ///     The game loads paused, so a flag only the sweep rebuilds would show the wrong sky and
    ///     the wrong fall rate until the player unpaused.
    /// </summary>
    [TestMethod]
    public void ContainmentSurvivesASaveAndLoad() {
        Assert.IsTrue(
            VentSource.Contains("Scribe_Values.Look(ref contained, \"ashVentContained\")"),
            "the sealed flag is not scribed, so every load hands a sealed map its full severity back."
        );
    }
}
