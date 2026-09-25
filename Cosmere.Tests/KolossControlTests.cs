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
public partial class KolossControlTests {
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

    private static string Core(params string[] parts) {
        return File.ReadAllText(Path.Combine([RepoRoot, "CosmereCore", "CosmereCore", .. parts]));
    }

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
            if (trimmed.StartsWith("//") || trimmed.StartsWith("///") || trimmed.StartsWith('*')) continue;

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

        Assert.Contains("growth.Severity", source, "A koloss resists by how far it has grown.");
        Assert.Contains("ChronologicalYears", source, "A kandra resists by how long it has existed.");
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
            BareGetStrength.IsMatch(push),
            "A bare GetStrength here is the reach 0.0 bug."
        );

        // status.power holds flaring; nextStatus.power holds the cast default and is null before a target is picked.
        Assert.IsTrue(
            PowerFromStatusOrNext.IsMatch(push),
            "Flaring lives on the live status and the cast default lives on nextStatus."
        );
        Assert.Contains(
            "Math.Max(power, 1)",
            push,
            "And neither is set at all during targeting, which reported reach 0.0."
        );
        Assert.IsFalse(
            DuraluminMention.IsMatch(seize),
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
        Assert.Contains("CS_KolossBind_TooWeak", control);
        Assert.DoesNotContain(
            "CapacityOf",
            control,
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
                HoldRadiusMention.IsMatch(source),
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
        // CodeOnly - the file's comments mention the old wrong defName, which a plain grep reads as still using it.
        string roster = CodeOnly("System", "Scadrial", "Comp", "Game", "KolossRoster.cs");

        Assert.Contains("DrainSource", roster, "Holding costs metal, the same way any burn does.");
        Assert.Contains(
            "SortByDescending(b => b.boundAtTick)",
            roster,
            "Newest first, so a force built over a campaign outlives a greedy seizure."
        );

        // guessing "Cosmere_Scadrial_Gene_Allomancy_Zinc" matched nothing, billing found no gene, dropped bonds in ~4s.
        Assert.Contains(
            "GetAllomanticGeneForMetal",
            roster,
            "Never guess a gene defName; ask the metal for it."
        );
        Assert.IsFalse(
            HandWrittenGeneDefName.IsMatch(roster),
            "A hand-written gene defName that matches nothing fails silently and reads as balance."
        );
        Assert.Contains("Scribe_References", roster, "A hold that drops on reload is worse than none.");
    }

    /// <summary>
    ///     Drafting gates on IsColonistPlayerControlled, which needs the pawn in the player
    ///     faction. Without the transfer a held koloss named its holder and still could not be
    ///     given one order, which is the entire point of holding it.
    /// </summary>
    [TestMethod]
    public void AHeldKolossBelongsToWhoeverHoldsIt() {
        string roster = Core("System", "Scadrial", "Comp", "Game", "KolossRoster.cs");

        Assert.Contains("koloss.SetFaction(holder.Faction)", roster, "Holding one makes it yours.");

        // not on release - losing the hold starts a grace window where the koloss is still nominally the colony's.
        Assert.DoesNotContain("SetFaction(null)", roster, "Release must not strip the faction.");

        // slave, not colonist - it takes orders because someone holds its mind; Ideology gates status, not the hold.
        Assert.Contains("GuestStatus.Slave", roster, "A held koloss works for the colony.");
        Assert.Contains("ModsConfig.IdeologyActive", roster, "Slavery is gated on the DLC.");
    }

    /// <summary>
    ///     Each bond remembers the metal that took it, so a Soother holding two on brass and one on
    ///     zinc runs out of one without losing the other two.
    /// </summary>
    [TestMethod]
    public void RunningOutOfOneMetalOnlyDropsWhatThatMetalHeld() {
        string roster = Core("System", "Scadrial", "Comp", "Game", "KolossRoster.cs");

        Assert.Contains("Scribe_Values.Look(ref metal", roster, "The metal has to survive a reload.");
        Assert.Contains("GeneFor(holder, bond)", roster, "Each bond bills its own metal.");

        // BurnTickInterval already drains its sources; billing by hand double-charged and showed Idle at 0.00%/s.
        Assert.Contains("UpdateDrainSource", roster, "The hold has to be a visible burn.");
        Assert.DoesNotContain(
            "RemoveFromReserve",
            roster,
            "The gene charges its own sources; taking it by hand charges twice and shows nothing."
        );
        Assert.Contains("HoldFraction", roster, "Holding costs a fraction of the seizure.");
    }

    /// <summary>
    ///     The roster was a save file and nothing else. A koloss could name its owner while the
    ///     Allomancer had no way to see what they were carrying or what it cost them.
    /// </summary>
    [TestMethod]
    public void TheHolderCanSeeWhatTheyAreCarrying() {
        string gene = Core("System", "Scadrial", "Gene", "Allomancer.cs");

        Assert.Contains("HeldOnMetal", gene, "Each metal answers for its own holds.");
        Assert.Contains("CS_KolossRoster_Label", gene, "The count belongs on a gizmo.");
        Assert.Contains("Dialog_KolossRoster", gene, "The gizmo opens the roster.");

        string window = Core("System", "Scadrial", "UI", "Dialog_KolossRoster.cs");

        // every interactive rect owes hover feedback, a description, and a sound within one frame, or it reads as dead.
        foreach (string owed in new[] { "DrawHighlightIfMouseover", "MouseoverSounds.DoRegion", "TipRegion" }) {
            Assert.Contains(owed, window, $"Interactive rects need {owed}.");
        }

        // GameFont.Large does not exist, and a scroll view that ignores the scrollbar clips.
        Assert.DoesNotContain("GameFont.Large", window, "There are only Tiny, Small and Medium.");
        Assert.Contains("ScrollbarWidth", window, "A scroll view has to deduct the scrollbar.");
        Assert.Contains("Event.current.shift", window, "Shift extends the selection.");
    }

    /// <summary>
    ///     The grace window exists so a holder running dry does not turn a koloss murderous in the
    ///     same tick the player notices. It also has to check InMentalState first, because
    ///     transitionSilently skips the recover-from-previous transition.
    /// </summary>
    [TestMethod]
    public void ALooseKolossGetsAGraceWindowBeforeItTurns() {
        string gene = Core("System", "Scadrial", "Gene", "EasilyInfluenced.cs");

        Assert.Contains("KolossControl.GraceTicks", gene);
        Assert.Contains("KolossControl.KolossFaction", gene, "Already theirs means nothing left to count.");

        // the window is a setting now, not a constant - how forgiving a lost hold should be varies by colony.
        string control = Core("System", "Scadrial", "Util", "KolossControl.cs");
        Assert.Contains("Mod.kolossGraceSeconds", control, "Grace comes from settings.");
        Assert.Contains("Mathf.Max(0,", control, "A negative window would turn it instantly.");
    }

    /// <summary>
    ///     Releasing one koloss must not touch the others, or turning zinc off entirely is the only
    ///     way to let a single one go.
    /// </summary>
    [TestMethod]
    public void ReleasingOneKolossLeavesTheRestHeld() {
        string gene = Core("System", "Scadrial", "Gene", "EasilyInfluenced.cs");

        Assert.Contains("CS_KolossRelease_Label", gene, "The player needs a per-pawn release.");
        Assert.Contains("KolossControl.Release(pawn)", gene, "Release takes one pawn, not a holder.");
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

            List<string> comps = [
                .. ability.Element("comps")!.Elements("li").Select(li => li.Attribute("Class")!.Value),
            ];

            Assert.Contains(c => c.EndsWith("SeizeKolossProperties"), comps, "Either metal holds one.");

            // CastAllomanticAbilityAtTarget throws via MoveThing and runs no ability comp - the iron/steel driver.
            Assert.AreEqual(
                "CastAbilityOnThing",
                ability.Element("jobDef")?.Value,
                $"{metal} target must not run the physical push driver."
            );
            Assert.Contains(
                c => c.EndsWith("MetalCostProperties"),
                comps,
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

        Assert.Contains("UnbreakableStateRegistry.Guards", push, "The deliberate soothe needs the guard too.");
        Assert.Contains("state.RecoverFromState()", push, "Soothing ends the state it can reach.");
        Assert.Contains("MentalBreakDefOf.Berserk", push, "Rioting starts one.");
    }

    /// <summary>
    ///     A held koloss is a slave, so the ordinary draft-and-click loop does not reach it. The
    ///     count goes in the label because sending an army is worth being sure about.
    /// </summary>
    [TestMethod]
    public void AHolderCanPointItsKolossAtSomething() {
        string orders = Core("System", "Scadrial", "UI", "Command_SendKoloss.cs");

        Assert.Contains("Command_Target", orders, "Targeting, not a plain button.");
        Assert.Contains("CS_KolossSend_Label", orders, "The label carries the count.");
        Assert.Contains("one.InMentalState", orders, "A loose koloss takes no orders.");
        Assert.Contains("CanReach", orders, "A click nothing can reach is refused, not ignored.");
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
            Assert.Contains(knob, mod, $"{knob} is not exposed.");
        }

        // Read live rather than captured at startup, or changing one does nothing until restart.
        Assert.Contains(
            "Mod.kolossHoldFraction",
            Core("System", "Scadrial", "Comp", "Game", "KolossRoster.cs"),
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

        Assert.Contains("koloss.SetFaction(theirs)", control, "It changes sides.");
        Assert.DoesNotContain("TryStartMentalState", control, "And does not go berserk doing it.");

        // Neither happens on a faction change the way it does inside TryStartMentalState.
        Assert.Contains("TryDropCarriedThing", control, "Or it walks off carrying a colonist.");
        Assert.Contains("Drafted = false", control, "Or stays drafted to a faction it left.");

        // a deliberate release lands immediately; the grace window is only for running out of metal.
        Assert.IsTrue(
            ReleaseLapsesImmediately.IsMatch(control),
            "A deliberate release has to be immediate."
        );
    }

    [GeneratedRegex(@"GetStrength\(\)")]
    private static partial Regex BareGetStrength { get; }

    [GeneratedRegex(@"Math\.Max\(parent\.status\.power, parent\.nextStatus\?\.power")]
    private static partial Regex PowerFromStatusOrNext { get; }

    [GeneratedRegex("Duralumin|SurgeCharge")]
    private static partial Regex DuraluminMention { get; }

    [GeneratedRegex(@"DistanceTo|GetCellsAround|\bradius\b", RegexOptions.IgnoreCase)]
    private static partial Regex HoldRadiusMention { get; }

    [GeneratedRegex(@"""Cosmere_Scadrial_Gene_")]
    private static partial Regex HandWrittenGeneDefName { get; }

    [GeneratedRegex(@"Release\(Pawn koloss\) \{[^}]*Lapse\(koloss\)", RegexOptions.Singleline)]
    private static partial Regex ReleaseLapsesImmediately { get; }
}
