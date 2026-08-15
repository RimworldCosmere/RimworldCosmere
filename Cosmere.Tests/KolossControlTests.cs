using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     A koloss does what the last voice in its head told it to, and turns on the world otherwise.
/// </summary>
/// <remarks>
///     Control is two acts, not one. Seizing is a contest against the creature's own resistance and
///     can fail; holding is a bill the roster collects every interval. Distance is not part of
///     either - an earlier design leashed a koloss to a radius around its holder and had to disable
///     hauling to stop the thing snapping on its first job.
/// </remarks>
[TestClass]
public class KolossControlTests {
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

    private static string Core(params string[] parts) => File.ReadAllText(Path.Combine(
        [RepoRoot, "CosmereCore", "CosmereCore", .. parts]
    ));

    /// <summary>
    ///     The same file with every comment gone, for assertions about what the code does rather
    ///     than what it says. These files explain the radius they replaced and the duralumin they
    ///     deliberately do not special-case, and a plain grep reads those explanations as the
    ///     thing they warn about.
    /// </summary>
    private static string CodeOnly(params string[] parts) {
        string[] lines = Core(parts).Split('\n');
        List<string> kept = [];

        for (int i = 0; i < lines.Length; i++) {
            string trimmed = lines[i].TrimStart();
            if (trimmed.StartsWith("//") || trimmed.StartsWith("///") || trimmed.StartsWith("*")) continue;

            kept.Add(lines[i]);
        }

        return string.Join("\n", kept);
    }

    /// <summary>
    ///     Inheriting from Berserk drags in its maxTicksBeforeRecovery of 60000, which would end the
    ///     bloodlust after a day whatever minTicksBeforeRecovery said. Aggro is what makes it go for
    ///     the nearest thing: AttackTargetFinder skips its ranged scoring path for aggro states.
    /// </summary>
    [TestMethod]
    public void BloodlustIsPermanentAndGoesForWhateverIsNearest() {
        XElement state = XDocument.Load(Path.Combine(
                RepoRoot, "CosmereScadrial", "Defs", "Races", "KolossMentalStates.xml"
            )).Descendants("MentalStateDef")
            .First(d => d.Element("defName")?.Value == "Cosmere_Scadrial_MentalState_KolossBloodlust");

        Assert.AreEqual(
            "BaseMentalState",
            state.Attribute("ParentName")?.Value,
            "Inheriting from Berserk brings its 60000 tick recovery ceiling with it."
        );
        Assert.AreEqual("Aggro", state.Element("category")?.Value);
        Assert.AreEqual("MentalState_Berserk", state.Element("stateClass")?.Value);
        Assert.AreEqual(99999999, int.Parse(state.Element("minTicksBeforeRecovery")!.Value));
    }

    /// <summary>
    ///     ThinkNode_ConditionalMentalState.Satisfied compares by reference, so a custom state
    ///     matches none of vanilla's branches. Without the patch a koloss enters bloodlust and then
    ///     stands there with no job giver, which reads as the state doing nothing at all.
    /// </summary>
    [TestMethod]
    public void BloodlustHasAThinkTreeBranchOfItsOwn() {
        string patch = File.ReadAllText(Path.Combine(
            RepoRoot, "CosmereScadrial", "Patches", "KolossBloodlustThinkTree.xml"
        ));

        Assert.IsTrue(patch.Contains("MentalStateCritical"));
        Assert.IsTrue(patch.Contains("Cosmere_Scadrial_MentalState_KolossBloodlust"));
        Assert.IsTrue(patch.Contains("JobGiver_Berserk"));
    }

    /// <summary>
    ///     Resistance is age read two ways. A koloss that has grown twenty years is harder to take
    ///     than one spiked this morning, and a first generation kandra has nine hundred years of
    ///     being itself to argue with.
    /// </summary>
    [TestMethod]
    public void ResistanceRisesWithAge() {
        string source = Core("System", "Scadrial", "Util", "EmotionalResistance.cs");

        Assert.IsTrue(source.Contains("growth.Severity"), "A koloss resists by how far it has grown.");
        Assert.IsTrue(source.Contains("ChronologicalYears"), "A kandra resists by how long it has existed.");
        Assert.IsTrue(source.Contains("KolossCeiling") && source.Contains("KandraCeiling"));
    }

    /// <summary>
    ///     Nothing here knows what duralumin is, and nothing needs to. GetStrength already reads the
    ///     duralumin reserve directly when the burn is powered that way, so a surge multiplies a
    ///     seizure without this code being told.
    /// </summary>
    [TestMethod]
    public void ReachComesFromTheAbilityRatherThanBeingRecomputed() {
        string seize = CodeOnly("System", "Scadrial", "Allomancy", "Comp", "Ability", "SeizeKoloss.cs");

        Assert.IsTrue(seize.Contains("parent.GetStrength()"), "Reach must come off the ability.");
        Assert.IsFalse(
            Regex.IsMatch(seize, @"Duralumin|SurgeCharge"),
            "Duralumin already lands through GetStrength; special-casing it here would double it."
        );
    }

    /// <summary>
    ///     A bind that fails silently reads as a broken button, and the player never learns that
    ///     duralumin is the answer.
    /// </summary>
    [TestMethod]
    public void AFailedSeizureNamesBothNumbers() {
        XDocument keys = XDocument.Load(Path.Combine(
            RepoRoot, "CosmereScadrial", "Languages", "English", "Keyed", "Koloss.xml"
        ));

        string weak = keys.Descendants("CS_KolossBind_TooWeak").Single().Value;
        Assert.IsTrue(weak.Contains("{REACH}") && weak.Contains("{NEEDED}"));

        string control = Core("System", "Scadrial", "Util", "KolossControl.cs");
        Assert.IsTrue(control.Contains("CS_KolossBind_TooWeak"));
        Assert.IsTrue(control.Contains("CS_KolossBind_NoCapacity"));
    }

    /// <summary>
    ///     Distance is not part of holding. The Lord Ruler's koloss stayed his across the empire,
    ///     and the radius design had to disable hauling because a haul crosses forty tiles against
    ///     a hold radius near twelve.
    /// </summary>
    [TestMethod]
    public void HoldingAKolossNeverChecksDistance() {
        foreach (string[] where in new[] {
            new[] { "System", "Scadrial", "Util", "KolossControl.cs" },
            ["System", "Scadrial", "Comp", "Game", "KolossRoster.cs"],
            ["System", "Scadrial", "Gene", "EasilyInfluenced.cs"],
        }) {
            string source = CodeOnly(where);

            Assert.IsFalse(
                Regex.IsMatch(source, @"DistanceTo|GetCellsAround|\bradius\b", RegexOptions.IgnoreCase),
                $"{where[^1]} must not reintroduce a hold radius."
            );
        }
    }

    /// <summary>
    ///     Capacity and metal are what limit an army. Newest goes first, so a force built over a
    ///     campaign survives a bad moment and the greedy seizure is the one that fails.
    /// </summary>
    [TestMethod]
    public void AnArmyIsLimitedByCapacityAndMetal() {
        string roster = Core("System", "Scadrial", "Comp", "Game", "KolossRoster.cs");

        Assert.IsTrue(roster.Contains("Cosmere_Scadrial_Stat_AllomanticPower"), "Slots come off power.");
        Assert.IsTrue(roster.Contains("CanBurn") && roster.Contains("RemoveFromReserve"), "Holding costs metal.");
        Assert.IsTrue(roster.Contains("DropNewest"), "The newest bond is the one that goes.");
        Assert.IsTrue(roster.Contains("Scribe_References"), "A hold that drops on reload is worse than none.");
    }

    /// <summary>
    ///     HandleMentalBreakRemoveFactor calls Reset, which clears whatever state a pawn is in. A
    ///     koloss in bloodlust is loose because nobody holds it, not because it is upset, so a
    ///     passing Soother would otherwise cure the mechanic and throw a green mote doing it.
    /// </summary>
    [TestMethod]
    public void APassingSootherCannotCalmALooseKoloss() {
        string handler = File.ReadAllText(Path.Combine(
            RepoRoot, "CosmereCore", "CosmereCore", "Core", "Comp", "Hediff", "CustomStatDef.cs"
        ));

        Assert.AreEqual(
            2,
            Regex.Matches(handler, @"UnbreakableStateRegistry\.Guards").Count,
            "Both the add and the remove handler need the guard."
        );

        // Core must not learn what a koloss is, so the def arrives through the registry.
        Assert.IsFalse(handler.Contains("Cosmere.System"), "Core must not import a shard.");
    }

    /// <summary>
    ///     SnapEventsPatches rolls a Snap on every successful TryStartMentalState. Without the def
    ///     guard, every koloss coming off its leash rolled somebody a one-in-sixteen chance of
    ///     becoming a Misting, which is a strange reward for losing control of an army.
    /// </summary>
    [TestMethod]
    public void AKolossLapsingDoesNotSnapAnybody() {
        string patches = Core("System", "Scadrial", "Patch", "Allomancy", "SnapEventsPatches.cs");

        Assert.IsTrue(
            patches.Contains("stateDef == MentalStateDefOf.Cosmere_Scadrial_MentalState_KolossBloodlust"),
            "The snap roll must skip the bloodlust."
        );
    }

    /// <summary>
    ///     The grace window exists so a holder running dry does not turn a koloss murderous in the
    ///     same tick the player notices. It also has to check InMentalState first, because
    ///     transitionSilently skips the recover-from-previous transition.
    /// </summary>
    [TestMethod]
    public void ALooseKolossGetsAGraceWindowBeforeItTurns() {
        string gene = Core("System", "Scadrial", "Gene", "EasilyInfluenced.cs");

        Assert.IsTrue(gene.Contains("KolossControl.GraceTicks"));
        Assert.IsTrue(gene.Contains("pawn.InMentalState"), "Starting a state on top of one gets a pawn stuck.");
        Assert.IsTrue(gene.Contains("KolossControl.Calm"), "Retaking one mid-rampage has to end the rampage.");

        string control = Core("System", "Scadrial", "Util", "KolossControl.cs");
        Assert.AreEqual(600, int.Parse(Regex.Match(control, @"GraceTicks = (\d+)").Groups[1].Value));
    }

    /// <summary>
    ///     Releasing one koloss must not touch the others, or turning zinc off entirely is the only
    ///     way to let a single one go.
    /// </summary>
    [TestMethod]
    public void ReleasingOneKolossLeavesTheRestHeld() {
        string gene = Core("System", "Scadrial", "Gene", "EasilyInfluenced.cs");

        Assert.IsTrue(gene.Contains("CS_KolossRelease_Label"), "The player needs a per-pawn release.");
        Assert.IsTrue(gene.Contains("KolossControl.Release(pawn)"), "Release takes one pawn, not a holder.");
    }
}
