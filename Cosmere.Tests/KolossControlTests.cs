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

        string push = CodeOnly("System", "Scadrial", "Allomancy", "Comp", "Ability", "EmotionalPush.cs");

        Assert.IsFalse(
            Regex.IsMatch(push, @"GetStrength\(\)"),
            "A bare GetStrength here is the reach 0.0 bug."
        );

        // SetNextStatus only runs from QueueCastingJob, which is the confirm and not the hover, so
        // nextStatus is null while the player is still picking a target. Without a floor the
        // readout says 0.0 and teaches the player the ability is broken.
        // BurnToggle puts flaring on the live status as power 2, and QueueCastingJob then writes
        // power 1 into nextStatus, throwing it away. Reading either alone lost the flare.
        Assert.IsTrue(
            Regex.IsMatch(push, @"Math\.Max\(parent\.status\.power, parent\.nextStatus\?\.power"),
            "Flaring lives on the live status and the cast default lives on nextStatus."
        );
        Assert.IsTrue(
            push.Contains("Math.Max(power, 1)"),
            "And neither is set at all during targeting, which reported reach 0.0."
        );
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
        Assert.IsFalse(
            control.Contains("CapacityOf"),
            "Nothing caps how many are held any more; the metal running out is the only limit."
        );
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
    ///     Metal is the only limit. Newest goes first, so a force built over a campaign survives a
    ///     bad moment and the greedy seizure is the one that fails.
    /// </summary>
    [TestMethod]
    public void AnArmyIsLimitedByMetalAlone() {
        // CodeOnly, because the file explains the wrong defName it used to use and a plain grep
        // reads that explanation as the thing it warns about.
        string roster = CodeOnly("System", "Scadrial", "Comp", "Game", "KolossRoster.cs");

        Assert.IsTrue(roster.Contains("DrainSource"), "Holding costs metal, the same way any burn does.");
        Assert.IsTrue(
            roster.Contains("SortByDescending(b => b.boundAtTick)"),
            "Newest first, so a force built over a campaign outlives a greedy seizure."
        );

        // The gene is called MistingZinc. Guessing at "Cosmere_Scadrial_Gene_Allomancy_Zinc"
        // matched nothing, so billing found no gene and dropped every bond on the next tick - a
        // hold lasted about four seconds. GetAllomanticGeneForMetal knows the real names.
        Assert.IsTrue(
            roster.Contains("GetAllomanticGeneForMetal"),
            "Never guess a gene defName; ask the metal for it."
        );
        Assert.IsFalse(
            Regex.IsMatch(roster, @"""Cosmere_Scadrial_Gene_"),
            "A hand-written gene defName that matches nothing fails silently and reads as balance."
        );
        Assert.IsTrue(roster.Contains("Scribe_References"), "A hold that drops on reload is worse than none.");
    }

    /// <summary>
    ///     Drafting gates on IsColonistPlayerControlled, which needs the pawn in the player
    ///     faction. Without the transfer a held koloss named its holder and still could not be
    ///     given one order, which is the entire point of holding it.
    /// </summary>
    [TestMethod]
    public void AHeldKolossBelongsToWhoeverHoldsIt() {
        string roster = Core("System", "Scadrial", "Comp", "Game", "KolossRoster.cs");

        Assert.IsTrue(roster.Contains("koloss.SetFaction(holder.Faction)"), "Holding one makes it yours.");

        // Not on release. Losing the hold starts a grace window during which the koloss is still
        // nominally the colony's - which is what the player selects, and what the warning sits on.
        Assert.IsFalse(roster.Contains("SetFaction(null)"), "Release must not strip the faction.");

        // Slave rather than colonist: it takes orders because somebody is standing on its mind,
        // which is what the status already means. Ideology owns slavery, so a colony without it
        // still gets a working hold, just a plain faction member.
        Assert.IsTrue(roster.Contains("GuestStatus.Slave"), "A held koloss works for the colony.");
        Assert.IsTrue(roster.Contains("ModsConfig.IdeologyActive"), "Slavery is gated on the DLC.");
    }

    /// <summary>
    ///     Each bond remembers the metal that took it, so a Soother holding two on brass and one on
    ///     zinc runs out of one without losing the other two.
    /// </summary>
    [TestMethod]
    public void RunningOutOfOneMetalOnlyDropsWhatThatMetalHeld() {
        string roster = Core("System", "Scadrial", "Comp", "Game", "KolossRoster.cs");

        Assert.IsTrue(roster.Contains("Scribe_Values.Look(ref metal"), "The metal has to survive a reload.");
        Assert.IsTrue(roster.Contains("GeneFor(holder, bond)"), "Each bond bills its own metal.");

        // Allomancer.BurnTickInterval already charges the sum of its drain sources and wipes them
        // all when it cannot pay. Taking the metal by hand as well charged twice and never showed
        // a rate - the gene read Idle at 0.00%/s while the reserve quietly fell.
        Assert.IsTrue(roster.Contains("UpdateDrainSource"), "The hold has to be a visible burn.");
        Assert.IsFalse(
            roster.Contains("RemoveFromReserve"),
            "The gene charges its own sources; taking it by hand charges twice and shows nothing."
        );
        Assert.IsTrue(roster.Contains("HoldFraction"), "Holding costs a fraction of the seizure.");
    }

    /// <summary>
    ///     The roster was a save file and nothing else. A koloss could name its owner while the
    ///     Allomancer had no way to see what they were carrying or what it cost them.
    /// </summary>
    [TestMethod]
    public void TheHolderCanSeeWhatTheyAreCarrying() {
        string gene = Core("System", "Scadrial", "Gene", "Allomancer.cs");

        Assert.IsTrue(gene.Contains("HeldOnMetal"), "Each metal answers for its own holds.");
        Assert.IsTrue(gene.Contains("CS_KolossRoster_Label"), "The count belongs on a gizmo.");
        Assert.IsTrue(gene.Contains("Dialog_KolossRoster"), "The gizmo opens the roster.");

        string window = Core("System", "Scadrial", "UI", "Dialog_KolossRoster.cs");

        // Every interactive rect owes the player three things within one frame of hover: that it
        // is clickable, what it does, and a sound. Missing any of them reads as a dead panel.
        foreach (string owed in new[] { "DrawHighlightIfMouseover", "MouseoverSounds.DoRegion", "TipRegion" }) {
            Assert.IsTrue(window.Contains(owed), $"Interactive rects need {owed}.");
        }

        // GameFont.Large does not exist, and a scroll view that ignores the scrollbar clips.
        Assert.IsFalse(window.Contains("GameFont.Large"), "There are only Tiny, Small and Medium.");
        Assert.IsTrue(window.Contains("ScrollbarWidth"), "A scroll view has to deduct the scrollbar.");
        Assert.IsTrue(window.Contains("Event.current.shift"), "Shift extends the selection.");
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
        Assert.IsTrue(gene.Contains("KolossControl.KolossFaction"), "Already theirs means nothing left to count.");

        // The window is a setting now, not a constant - how forgiving a lost hold should be is
        // the kind of thing one colony wants tense and another wants survivable.
        string control = Core("System", "Scadrial", "Util", "KolossControl.cs");
        Assert.IsTrue(control.Contains("Mod.kolossGraceSeconds"), "Grace comes from settings.");
        Assert.IsTrue(control.Contains("Mathf.Max(0,"), "A negative window would turn it instantly.");
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

    /// <summary>
    ///     A targeted riot or soothe is one push, not a channel.
    /// </summary>
    /// <remarks>
    ///     AbstractToggleTargetBurnAbility runs Cosmere_Scadrial_Job_MaintainAllomanticTarget,
    ///     which calls MaintainProximityTo and cuts the burn rate to zero past the verb's range.
    ///     That walks the caster to the target and pins them there, which is a leash - on the one
    ///     mechanic built to have none.
    /// </remarks>
    [TestMethod]
    public void ATargetedPushIsOneActRatherThanAChannel() {
        foreach (string metal in new[] { "Zinc", "Brass" }) {
            XElement ability = XDocument.Load(Path.Combine(
                    RepoRoot, "CosmereScadrial", "Defs", "Allomancy", metal, "Abilities.xml"
                )).Descendants()
                .First(d => d.Element("defName")?.Value == $"Cosmere_Scadrial_Ability_{metal}Target");

            Assert.AreEqual(
                "AbstractTargetBurnAbility",
                ability.Attribute("ParentName")?.Value,
                $"{metal} target must not inherit the maintain-proximity job."
            );

            List<string> comps = ability.Element("comps")!.Elements("li")
                .Select(li => li.Attribute("Class")!.Value).ToList();

            Assert.IsTrue(comps.Any(c => c.EndsWith("SeizeKolossProperties")), "Either metal holds one.");

            // CastAllomanticAbilityAtTarget is the iron and steel driver. It calls MoveThing and
            // physically throws the target, and it never runs an ability comp - so rioting a pawn
            // launched them across the map and took hold of nothing.
            Assert.AreEqual(
                "CastAbilityOnThing",
                ability.Element("jobDef")?.Value,
                $"{metal} target must not run the physical push driver."
            );
            Assert.IsTrue(
                comps.Any(c => c.EndsWith("MetalCostProperties")),
                "Vanilla's job does not spend the metal, so the cost has to be a comp."
            );
        }
    }

    /// <summary>
    ///     Rioting pushes somebody over; soothing pulls them back. A koloss in bloodlust is neither
    ///     - it is loose, and the registry keeps a deliberate soothe off it the same way it keeps
    ///     the passive one off.
    /// </summary>
    [TestMethod]
    public void SoothingCannotReachALooseKoloss() {
        string push = Core("System", "Scadrial", "Allomancy", "Comp", "Ability", "EmotionalPush.cs");

        Assert.IsTrue(push.Contains("UnbreakableStateRegistry.Guards"), "The deliberate soothe needs the guard too.");
        Assert.IsTrue(push.Contains("state.RecoverFromState()"), "Soothing ends the state it can reach.");
        Assert.IsTrue(push.Contains("MentalBreakDefOf.Berserk"), "Rioting starts one.");
    }

    /// <summary>
    ///     Losing a koloss is the moment the player most needs telling what happened, and the
    ///     moment every other control on the pawn disappears - Pawn.GetGizmos gates the draft
    ///     button on IsColonistPlayerControlled, which a koloss in bloodlust is not.
    /// </summary>
    /// <remarks>
    ///     Gene gizmos are emitted OUTSIDE that guard, which is what lets this one survive - and
    ///     is also why it has to guard itself, or it shows up on raid koloss too.
    /// </remarks>
    [TestMethod]
    public void ALooseKolossStillTellsYouWhatHappened() {
        string gene = Core("System", "Scadrial", "Gene", "EasilyInfluenced.cs");

        Assert.IsTrue(gene.Contains("CS_KolossLoose_Label"), "The loose state needs its own gizmo.");
        Assert.IsTrue(
            gene.Contains("pawn.Faction is not { IsPlayer: true }"),
            "Gene gizmos skip the colonist guard, so this one has to bring its own."
        );
    }

    /// <summary>
    ///     A held koloss is a slave, so the ordinary draft-and-click loop does not reach it. The
    ///     count goes in the label because sending an army is worth being sure about.
    /// </summary>
    [TestMethod]
    public void AHolderCanPointItsKolossAtSomething() {
        string orders = Core("System", "Scadrial", "UI", "Command_SendKoloss.cs");

        Assert.IsTrue(orders.Contains("Command_Target"), "Targeting, not a plain button.");
        Assert.IsTrue(orders.Contains("CS_KolossSend_Label"), "The label carries the count.");
        Assert.IsTrue(orders.Contains("one.InMentalState"), "A loose koloss takes no orders.");
        Assert.IsTrue(orders.Contains("CanReach"), "A click nothing can reach is refused, not ignored.");
    }

    /// <summary>
    ///     Every number Phase 2 hard-coded is now the player's to move. The radius settings the
    ///     spec asked for do not exist, because the hold stopped caring about distance.
    /// </summary>
    [TestMethod]
    public void TheNumbersThatDecideAnArmyAreSettings() {
        string mod = File.ReadAllText(Path.Combine(
            RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "Mod.cs"
        ));

        foreach (string knob in new[] {
            "kolossGraceSeconds", "kolossHoldFraction", "kolossResistance",
        }) {
            Assert.IsTrue(mod.Contains(knob), $"{knob} is not exposed.");
        }

        // Read live rather than captured at startup, or changing one does nothing until restart.
        Assert.IsTrue(
            Core("System", "Scadrial", "Comp", "Game", "KolossRoster.cs")
                .Contains("Mod.kolossHoldFraction"),
            "A const would ignore the setting."
        );
    }

    /// <summary>
    ///     A koloss nobody holds changes sides rather than going berserk.
    /// </summary>
    /// <remarks>
    ///     A berserk state sent it at the nearest thing, so a band of loose koloss tore each other
    ///     apart in the field. Koloss do not fight koloss - they march with them. A permanently
    ///     hostile faction gets everything the mental state was for and lets them behave like an
    ///     army instead of a riot.
    /// </remarks>
    [TestMethod]
    public void ALostKolossJoinsTheOthersRatherThanTurningOnThem() {
        string control = CodeOnly("System", "Scadrial", "Util", "KolossControl.cs");

        Assert.IsTrue(control.Contains("koloss.SetFaction(theirs)"), "It changes sides.");
        Assert.IsFalse(control.Contains("TryStartMentalState"), "And does not go berserk doing it.");

        // Neither happens on a faction change the way it does inside TryStartMentalState.
        Assert.IsTrue(control.Contains("TryDropCarriedThing"), "Or it walks off carrying a colonist.");
        Assert.IsTrue(control.Contains("Drafted = false"), "Or stays drafted to a faction it left.");
    }
}
