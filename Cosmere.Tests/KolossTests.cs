using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Guards what makes a koloss a koloss, on every path one can come from.
/// </summary>
[TestClass]
public class KolossTests {
    private static string RepoRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir);
            return dir!.FullName;
        }
    }

    private static string Source(params string[] parts) => File.ReadAllText(Path.Combine(
        new[] { RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial" }.Concat(parts).ToArray()
    ));

    private static XDocument Defs(params string[] parts) => XDocument.Load(Path.Combine(
        new[] { RepoRoot, "CosmereScadrial", "Defs" }.Concat(parts).ToArray()
    ));

    /// <summary>
    ///     HediffDef.initialSeverity defaults to 0.5 and Hediff.PostMake maxes the severity
    ///     against it, so without an explicit zero every koloss was born forty percent of the way
    ///     to splitting - and drawn oversized on its first frame.
    /// </summary>
    [TestMethod]
    public void AKolossStartsGrowingFromNothing() {
        XElement growth = Defs("Races", "KolossHediffs.xml").Descendants("HediffDef")
            .First(d => d.Element("defName")?.Value == "Cosmere_Scadrial_Hediff_KolossGrowth");

        // A hair above zero, not zero: Hediff.ShouldRemove is `Severity <= 0f`, so a literal zero
        // makes the hediff delete itself on the next tick and the koloss has no growth clock.
        double initial = double.Parse(growth.Element("initialSeverity")!.Value);
        Assert.IsTrue(initial > 0, "Zero makes the hediff remove itself.");
        Assert.IsTrue(initial < 0.01, "And anything much above zero starts the koloss part-grown.");
    }

    /// <summary>
    ///     lifeThreatening kills nothing on its own - it feeds TendPriority, the alert and the
    ///     caravan-banish check. Hediff.IsLethal reads lethalSeverity, which is also what puts a
    ///     percentage in the health tab so the player can watch it coming.
    /// </summary>
    [TestMethod]
    public void UncheckedGrowthEventuallyKills() {
        XElement growth = Defs("Races", "KolossHediffs.xml").Descendants("HediffDef")
            .First(d => d.Element("defName")?.Value == "Cosmere_Scadrial_Hediff_KolossGrowth");

        Assert.AreEqual("1.0", growth.Element("lethalSeverity")?.Value);
    }

    /// <summary>
    ///     Only the surgery used to start the clock, so a koloss from a raid, the dev menu or a
    ///     post-Catacendre birth was a big blue person who would live forever.
    /// </summary>
    [TestMethod]
    public void EveryKolossGrowsHoweverItWasMade() {
        XElement heritage = Defs("Races", "Genes", "Koloss.xml").Descendants("GeneDef")
            .First(d => d.Element("defName")?.Value == "Cosmere_Scadrial_Gene_KolossHeritage");

        Assert.AreEqual("Cosmere.System.Scadrial.Gene.KolossHeritage", heritage.Element("geneClass")?.Value);

        string gene = File.ReadAllText(Path.Combine(
            RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "Gene", "KolossHeritage.cs"
        ));
        Assert.IsTrue(gene.Contains("public override void PostAdd()", StringComparison.Ordinal));
        Assert.IsTrue(
            gene.Contains("base.PostAdd();", StringComparison.Ordinal),
            "Gene.PostAdd is not empty; it dirties the graphics for any gene that defines them."
        );
    }

    /// <summary>
    ///     Become's own message says whoever they were did not come back, and the koloss childhood
    ///     backstory says it does not remember its name. The code used to keep the name, the
    ///     ideoligion, the traits and every relationship, so a colonist's husband could be made
    ///     into a koloss and stay her husband.
    /// </summary>
    [TestMethod]
    public void NothingOfThePersonSurvivesBeingMade() {
        string util = Source("Util", "KolossUtility.cs");

        Assert.IsTrue(util.Contains("pawn.Name = new NameSingle", StringComparison.Ordinal));
        Assert.IsTrue(util.Contains("pawn.ideo?.SetIdeo(null)", StringComparison.Ordinal));
        Assert.IsTrue(util.Contains("RemoveTrait", StringComparison.Ordinal));
        Assert.IsTrue(util.Contains("ClearAllRelations", StringComparison.Ordinal));
    }

    /// <summary>
    ///     SkillRecord.Level returns 0 for a skill the pawn is currently incapable of and adds
    ///     trait and gene aptitude on top of what is stored, so subtracting through it takes two
    ///     off a number that was never there.
    /// </summary>
    [TestMethod]
    public void MakingAKolossReadsTheRealSkillLevels() {
        string util = Source("Util", "KolossUtility.cs");

        Assert.IsTrue(util.Contains("skill.levelInt", StringComparison.Ordinal));
        Assert.IsFalse(
            util.Contains("skill.Level -", StringComparison.Ordinal),
            "The property is lossy in both directions; the backing field is not."
        );
    }

    /// <summary>
    ///     The gene's own description says four spikes driven in at once, but nothing recorded
    ///     them - so a koloss from a raid or the dev menu had nothing in its body to pull out, and
    ///     nothing for Ruin to speak through.
    /// </summary>
    [TestMethod]
    public void AKolossCarriesTheFourSpikesThatMadeIt() {
        XElement bound = Defs("Races", "Genes", "Koloss.xml").Descendants("GeneDef")
            .First(d => d.Element("defName")?.Value == "Cosmere_Scadrial_Gene_SpikeBound");

        Assert.AreEqual("Cosmere.System.Scadrial.Gene.SpikeBound", bound.Element("geneClass")?.Value);

        string gene = File.ReadAllText(Path.Combine(
            RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "Gene", "SpikeBound.cs"
        ));

        Assert.IsTrue(gene.Contains("public const int SpikeCount = 4;", StringComparison.Ordinal));
        Assert.IsTrue(
            gene.Contains("if (KolossUtility.SpikeCount(pawn) > 0) return;", StringComparison.Ordinal),
            "SetXenotype calls AddGene per gene and nothing dedupes, so a second Become would drive four more in."
        );
        Assert.IsTrue(
            gene.Contains("corePart", StringComparison.Ordinal),
            "Only 6 of 49 vanilla BodyDefs have a part called Torso."
        );
    }

    /// <summary>
    ///     A koloss is a new thing built out of a person, not that person enlarged. Generating one
    ///     also sidesteps the riskiest edit in the plan: RimWorld has no ClearEndogenes, so
    ///     rewriting the subject meant a hand-rolled reverse-index loop over RemoveGene firing
    ///     trait, passion and graphics side effects per gene, on a live colonist.
    /// </summary>
    [TestMethod]
    public void MakingAKolossGeneratesAPawnRatherThanRewritingOne() {
        string util = Source("Util", "KolossUtility.cs");

        Assert.IsTrue(util.Contains("PawnGenerator.GeneratePawn", StringComparison.Ordinal));
        Assert.IsTrue(
            util.Contains("forcedXenotype: koloss", StringComparison.Ordinal),
            "The genes come from the xenotype rather than being hand-applied."
        );
        Assert.IsTrue(
            util.Contains("fixedGender: subject.gender", StringComparison.Ordinal),
            "Gender is the one thing that carries - a koloss is built out of a body."
        );
    }

    /// <summary>
    ///     The flesh went into the thing standing over it, so there is no corpse to bury and no
    ///     spikes to take back out of one.
    /// </summary>
    [TestMethod]
    public void NothingIsLeftOfTheSubject() {
        string util = Source("Util", "KolossUtility.cs");

        Assert.IsTrue(util.Contains("subject.Destroy(DestroyMode.Vanish)", StringComparison.Ordinal));

        int destroy = util.IndexOf("subject.Destroy(", StringComparison.Ordinal);
        int spawn = util.IndexOf("GenSpawn.Spawn(made", StringComparison.Ordinal);
        Assert.IsTrue(
            destroy < spawn,
            "The subject leaves before the koloss arrives, or two things stand on one tile."
        );
    }

    /// <summary>
    ///     A fighter makes a better koloss than a clerk does, which is the only reason to care who
    ///     goes on the table. Nothing that needed a mind survives.
    /// </summary>
    [TestMethod]
    public void TheSubjectsBuildIsWhatCarries() {
        string util = Source("Util", "KolossUtility.cs");

        Assert.IsTrue(util.Contains("theirMelee.levelInt", StringComparison.Ordinal));
        Assert.IsTrue(
            util.Contains("Clumsy.Contains(skill.def) ? 1 : 0", StringComparison.Ordinal),
            "A koloss can stir a pot and hold a bandage on, terribly; everything else is gone."
        );
    }

    /// <summary>
    ///     The surgery is always available; the ritual is the same act performed in front of a
    ///     congregation, which is how the Steel Ministry did it. Making a koloss must not require
    ///     Ideology, so every ritual def is gated and the hemalurgy bill is not.
    /// </summary>
    [TestMethod]
    public void TheRitualIsIdeologyOnlyAndTheSurgeryIsNot() {
        XDocument rituals = XDocument.Load(Path.Combine(
            RepoRoot, "CosmereCore", "Defs", "Ideology", "Rituals", "Scadrial_Rituals.xml"
        ));

        foreach (string def in new[] {
            "Cosmere_RitualOutcome_MakeKoloss",
            "Cosmere_RitualPattern_MakeKoloss",
            "Cosmere_Ritual_MakeKoloss",
        }) {
            XElement node = rituals.Descendants()
                .First(e => e.Element("defName")?.Value == def);
            Assert.AreEqual(
                "Ludeon.RimWorld.Ideology",
                (string?)node.Attribute("MayRequire"),
                $"{def} must be gated, or a colony without Ideology fails to load."
            );
        }

        // The hemalurgy path carries no such gate.
        string recipes = File.ReadAllText(Path.Combine(
            RepoRoot, "CosmereScadrial", "Defs", "Hemalurgy", "KolossRecipes.xml"
        ));
        Assert.IsFalse(recipes.Contains("Ludeon.RimWorld.Ideology", StringComparison.Ordinal));
    }

    /// <summary>
    ///     The sacrifice behaviour kills before the outcome fires, so the ritual path hands a
    ///     corpse to the same code the surgery hands a living pawn. Destroying the pawn without
    ///     its corpse leaves the body on the floor with nothing in it.
    /// </summary>
    [TestMethod]
    public void TheRitualPathConsumesTheBodyToo() {
        string util = Source("Util", "KolossUtility.cs");

        Assert.IsTrue(util.Contains("public static Pawn? MakeFrom(", StringComparison.Ordinal));
        Assert.IsTrue(util.Contains("corpse.Destroy(DestroyMode.Vanish)", StringComparison.Ordinal));

        string worker = Source("Ritual", "RitualOutcomeEffectWorker_MakeKoloss.cs");
        Assert.IsTrue(worker.Contains("FirstAssignedPawn(\"prisoner\")", StringComparison.Ordinal));
        Assert.IsTrue(worker.Contains("KolossUtility.MakeFrom", StringComparison.Ordinal));
    }

    /// <summary>
    ///     The four that make a koloss are added by SpikeBound during generation. Anything the
    ///     subject was already carrying - a Misting made by hemalurgy, a kandra, an Inquisitor
    ///     part-way through - is physically in the body being used, so it comes across too.
    /// </summary>
    [TestMethod]
    public void WhateverWasAlreadyDrivenInComesAcross() {
        string util = Source("Util", "KolossUtility.cs");

        Assert.IsTrue(util.Contains("CarrySpikes(subject, made)", StringComparison.Ordinal));
        Assert.IsTrue(util.Contains("AddToUnifiedHediff(made, carried[i], core)", StringComparison.Ordinal));
        Assert.IsTrue(
            util.Contains("UpdateRuinsInfluence(made)", StringComparison.Ordinal),
            "Ruin speaks through the total, so it is recomputed after the carry."
        );
    }

    /// <summary>
    ///     Four, everywhere. The recipe asks for four, the gene and xenotype prose both say four,
    ///     and canon is four - so a change to five is a change to all of them at once, not a
    ///     number edited in one place.
    /// </summary>
    [TestMethod]
    public void FourSpikesMakeAKolossEverywhereItIsWritten() {
        // MakeKoloss inherits its ingredients from the abstract base above it, so the count lives
        // there rather than on the recipe itself.
        XElement spikes = Defs("Hemalurgy", "KolossRecipes.xml").Descendants("li")
            .First(li => li.Descendants("thingDefs").Any(t =>
                t.Elements("li").Any(x => x.Value == "Cosmere_Scadrial_Thing_HemalurgicSpike")));

        Assert.AreEqual("4", spikes.Element("count")?.Value);

        string gene = File.ReadAllText(Path.Combine(
            RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "Gene", "SpikeBound.cs"
        ));
        Assert.IsTrue(gene.Contains("SpikeCount = 4", StringComparison.Ordinal));
    }

    /// <summary>
    ///     RitualOutcomeEffectWorker_FromQuality picks an outcome by quality and RimWorld rejects
    ///     the def at load if the chances do not total exactly one. Bare floats do not parse into
    ///     the list at all, which reads as a total of zero.
    /// </summary>
    [TestMethod]
    public void TheRitualOutcomeChancesAddUp() {
        XDocument rituals = XDocument.Load(Path.Combine(
            RepoRoot, "CosmereCore", "Defs", "Ideology", "Rituals", "Scadrial_Rituals.xml"
        ));
        XElement outcome = rituals.Descendants()
            .First(e => e.Element("defName")?.Value == "Cosmere_RitualOutcome_MakeKoloss");

        double total = outcome.Descendants("chance").Sum(c => double.Parse(c.Value));
        Assert.AreEqual(1.0, total, 0.0001);

        Assert.IsFalse(
            outcome.Descendants("effecter").Any(),
            "ExecutionFlames is not a real EffecterDef; an invented one fails cross-reference at load."
        );
    }

    /// <summary>
    ///     RimWorld rejects a humanlike PawnKindDef without a resistance range at load time, not
    ///     when one is first generated.
    /// </summary>
    [TestMethod]
    public void TheKolossKindCanBeGenerated() {
        XElement kind = Defs("Races", "PawnKinds.xml").Descendants("PawnKindDef")
            .First(d => d.Element("defName")?.Value == "Cosmere_Scadrial_PawnKind_Koloss");

        Assert.IsNotNull(kind.Element("initialResistanceRange"));
        Assert.IsNotNull(kind.Element("initialWillRange"));
    }

    /// <summary>
    ///     Made, not born. RimWorld picks the life stage off the biological age, so the body stays
    ///     adult and the chronological age carries "came into existence today".
    /// </summary>
    [TestMethod]
    public void AKolossIsNewButNotAChild() {
        string util = Source("Util", "KolossUtility.cs");

        Assert.IsTrue(util.Contains("Rand.Range(MinAdultAge, MaxAdultAge)", StringComparison.Ordinal));
        Assert.IsTrue(util.Contains("fixedChronologicalAge: 0f", StringComparison.Ordinal));
    }

    /// <summary>
    ///     Gene PostAdd runs during generation, before the health tracker is worth writing to, so
    ///     the growth hediff silently never landed and the koloss had no growth row at all.
    /// </summary>
    [TestMethod]
    public void TheGrowthRowActuallyAppears() {
        string util = Source("Util", "KolossUtility.cs");

        Assert.IsTrue(util.Contains("StartGrowing(made)", StringComparison.Ordinal));
        Assert.IsTrue(util.Contains("made.health.AddHediff(growth)", StringComparison.Ordinal));
    }

    /// <summary>
    ///     One name, no family. Whatever it was called belonged to somebody who is not here.
    /// </summary>
    [TestMethod]
    public void AKolossHasOneNameAndNoFamily() {
        string util = Source("Util", "KolossUtility.cs");

        Assert.IsTrue(util.Contains("made.Name = new NameSingle", StringComparison.Ordinal));
        Assert.IsTrue(
            util.Contains("PawnBioAndNameGenerator.GeneratePawnName", StringComparison.Ordinal),
            "A rolled name, not the literal word Koloss."
        );
    }

    /// <summary>
    ///     It can stamp out a fire, stir a pot, hammer a thing flat and hold a bandage on. What it
    ///     cannot do is judge, talk, create, or ever get better at any of it.
    /// </summary>
    [TestMethod]
    public void AKolossCanStillBePointedAtWork() {
        XElement gene = Defs("Races", "Genes", "Koloss.xml").Descendants("GeneDef")
            .First(d => d.Element("defName")?.Value == "Cosmere_Scadrial_Gene_EasilyInfluenced");

        List<string> off = gene.Element("disabledWorkTags")!.Elements("li").Select(li => li.Value).ToList();

        foreach (string allowed in new[] {
            "Firefighting", "Cooking", "Crafting", "Caring", "Social", "Animals", "Mining", "Construction",
        }) {
            CollectionAssert.DoesNotContain(off, allowed, $"A koloss should still be able to do {allowed}.");
        }

        // Shooting is the one it can never be taught. Judgement and art it never had.
        foreach (string denied in new[] { "Intellectual", "Artistic", "PlantWork", "Shooting" }) {
            CollectionAssert.Contains(off, denied);
        }

        Assert.AreEqual("0.02", gene.Element("statFactors")?.Element("GlobalLearningFactor")?.Value);
    }

    /// <summary>
    ///     Shapeshifting belongs to kandra and to nothing else, so a koloss should never see a row
    ///     for it on its character card.
    /// </summary>
    [TestMethod]
    public void OnlyAKandraSeesShapeshifting() {
        XElement skill = XDocument.Load(Path.Combine(RepoRoot, "CosmereScadrial", "Defs", "Skills.xml"))
            .Descendants("SkillDef")
            .First(d => d.Element("defName")?.Value == "Cosmere_Scadrial_Skill_Shapeshift");

        Assert.AreEqual(
            "Cosmere_Scadrial_Gene_BodyAbsorption",
            skill.Descendants("requiresGene").FirstOrDefault()?.Value
        );
    }

    /// <summary>It does not remember being anything else, so both backstories say the same word.</summary>
    [TestMethod]
    public void BothBackstoriesJustSayKoloss() {
        XDocument stories = XDocument.Load(Path.Combine(
            RepoRoot, "CosmereScadrial", "Defs", "Backstories", "Koloss.xml"
        ));

        foreach (string def in new[] {
            "Cosmere_Scadrial_Backstory_Koloss_Childhood",
            "Cosmere_Scadrial_Backstory_Koloss_Adulthood",
        }) {
            XElement story = stories.Descendants().First(e => e.Element("defName")?.Value == def);
            Assert.AreEqual("Koloss", story.Element("title")?.Value);
            Assert.AreEqual("Koloss", story.Element("titleShort")?.Value);
        }
    }

    /// <summary>
    ///     Iron takes human strength, which is what a koloss is made out of. Any other metal
    ///     steals the wrong thing, and an uncharged spike is a lump of metal - a bill that accepts
    ///     those lets a player make a koloss out of four iron bars.
    /// </summary>
    [TestMethod]
    public void OnlyChargedIronSpikesMakeAKoloss() {
        XElement spikes = Defs("Hemalurgy", "KolossRecipes.xml").Descendants("li")
            .First(li => li.Descendants("thingDefs").Any(t =>
                t.Elements("li").Any(x => x.Value == "Cosmere_Scadrial_Thing_HemalurgicSpike")));

        // specialFiltersToDisallow, matching the kandra recipe that already works.
        // disallowedSpecialFilters is not the field, and using it silently excluded everything.
        List<string> disallowed = spikes.Descendants("specialFiltersToDisallow")
            .Elements("li").Select(li => li.Value).ToList();

        CollectionAssert.Contains(disallowed, "Cosmere_Scadrial_SpecialFilter_NotSpikeIron");
        CollectionAssert.Contains(disallowed, "Cosmere_Scadrial_SpecialFilter_SpikeUncharged");
        Assert.AreEqual("4", spikes.Element("count")?.Value);
    }

    /// <summary>
    ///     A ThingFilter cannot see what a thing is made of or what is in it - stuffCategories
    ///     allows the material itself as an item. The only hook handed the actual Thing is a
    ///     special filter's worker.
    /// </summary>
    [TestMethod]
    public void TheChargeCheckLooksAtTheRealThing() {
        string worker = File.ReadAllText(Path.Combine(
            RepoRoot,
            "CosmereCore",
            "CosmereCore",
            "System",
            "Scadrial",
            "Hemalurgy",
            "SpecialThingFilterWorker_UnchargedSpike.cs"
        ));

        Assert.IsTrue(worker.Contains("spike is not { isCharged: true }", StringComparison.Ordinal));
        Assert.IsTrue(
            worker.Contains("if (!CanEverMatch(t.def)) return false;", StringComparison.Ordinal),
            "Matching anything but a spike would quietly drop the medicine out of the same bill."
        );
    }

    /// <summary>
    ///     Iron takes human strength, and a koloss is four of those. Any other metal steals
    ///     something a koloss has no use for, and the recipe description has to say the same thing
    ///     the code does.
    /// </summary>
    [TestMethod]
    public void AllFourSpikesAreIron() {
        string gene = File.ReadAllText(Path.Combine(
            RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "Gene", "SpikeBound.cs"
        ));

        Assert.IsTrue(gene.Contains("Metal = \"Iron\"", StringComparison.Ordinal));
        Assert.IsFalse(
            gene.Contains("Steel", StringComparison.Ordinal) || gene.Contains("Pewter", StringComparison.Ordinal),
            "Four iron, not one of each."
        );

        string recipes = File.ReadAllText(Path.Combine(
            RepoRoot, "CosmereScadrial", "Defs", "Hemalurgy", "KolossRecipes.xml"
        ));
        Assert.IsFalse(
            recipes.Contains("iron, steel, tin and pewter", StringComparison.Ordinal),
            "The description has to agree with the ingredients."
        );
    }

    /// <summary>
    ///     Generation hands out whatever traits it likes, which produced koloss who were delicate
    ///     and pyromaniac. A koloss is enormous and hard to look at, and has no personality left to
    ///     roll one from.
    /// </summary>
    [TestMethod]
    public void EveryKolossIsTheSameKindOfUgly() {
        string util = Source("Util", "KolossUtility.cs");

        Assert.IsTrue(util.Contains("Disfigure(made)", StringComparison.Ordinal));
        Assert.IsTrue(util.Contains("made.story.traits.RemoveTrait(rolled[i])", StringComparison.Ordinal));
        Assert.IsTrue(
            util.Contains("new Trait(beauty, -2, true)", StringComparison.Ordinal),
            "Degree -2 on Beauty is staggeringly ugly."
        );
    }

    /// <summary>
    ///     fixedChronologicalAge on the generation request does not survive, so the tracker is
    ///     written directly. Without it the Bio tab reads "age 20" rather than "age 20 (0)",
    ///     because AgeNumberString only shows the second number when the two differ.
    /// </summary>
    [TestMethod]
    public void ItsAgeSaysHowLongItHasBeenAKoloss() {
        string util = Source("Util", "KolossUtility.cs");

        Assert.IsTrue(util.Contains("made.ageTracker.AgeChronologicalTicks = 0", StringComparison.Ordinal));
    }

    /// <summary>
    ///     A backstory's workDisables cannot be undone by a gene, so Social had to come out of
    ///     both koloss backstories as well as the gene.
    /// </summary>
    [TestMethod]
    public void NothingElseSecretlyDisablesKolossWork() {
        XDocument stories = XDocument.Load(Path.Combine(
            RepoRoot, "CosmereScadrial", "Defs", "Backstories", "Koloss.xml"
        ));

        foreach (string def in new[] {
            "Cosmere_Scadrial_Backstory_Koloss_Childhood",
            "Cosmere_Scadrial_Backstory_Koloss_Adulthood",
        }) {
            XElement story = stories.Descendants().First(e => e.Element("defName")?.Value == def);
            List<string> off = story.Element("workDisables")?.Elements("li").Select(li => li.Value).ToList() ?? [];

            CollectionAssert.DoesNotContain(off, "Social");
            CollectionAssert.DoesNotContain(off, "Firefighting");
        }
    }

    /// <summary>Enough of everything to be pointed at it, and never enough to be good.</summary>
    [TestMethod]
    public void AKolossIsClumsyAtSevenThings() {
        string util = Source("Util", "KolossUtility.cs");
        int start = util.IndexOf("Clumsy = [", StringComparison.Ordinal);
        int end = util.IndexOf("];", start, StringComparison.Ordinal);
        string block = util[start..end];

        foreach (string skill in new[] {
            "Cooking", "Crafting", "Medicine", "Social", "Animals", "Mining", "Construction",
        }) {
            Assert.IsTrue(block.Contains(skill, StringComparison.Ordinal), $"{skill} should start at 1.");
        }

        Assert.IsFalse(block.Contains("Shooting", StringComparison.Ordinal), "Shooting is disabled outright.");
    }

    /// <summary>
    ///     A koloss is not uncomfortable - being comfortable was never something it knew about. The
    ///     thoughts are nullified rather than offset so they never appear in the mood tab, because
    ///     a list of complaints it does not have is noise on every koloss you own.
    /// </summary>
    [TestMethod]
    public void AKolossDoesNotNoticeHowItLives() {
        XDocument patch = XDocument.Load(Path.Combine(
            RepoRoot, "CosmereScadrial", "Patches", "KolossSimpleMind.xml"
        ));
        string xpath = patch.Descendants("xpath").First().Value;

        foreach (string thought in new[] {
            "SleptOutside", "SleptOnGround", "AteWithoutTable", "ApparelDamaged",
            "EnvironmentCold", "EnvironmentHot", "NeedComfort", "Naked",
        }) {
            Assert.IsTrue(
                xpath.Contains($"defName=\"{thought}\"", StringComparison.Ordinal),
                $"A koloss should not care about {thought}."
            );
        }

        Assert.IsTrue(
            patch.Descendants("nullifyingGenes").Elements("li")
                .Any(li => li.Value == "Cosmere_Scadrial_Gene_KolossHeritage"),
            "Keyed to the gene every koloss has."
        );
    }

    /// <summary>
    ///     There is very little in there for a bad day to land on. Keyed to the growth hediff
    ///     because ThoughtWorker_Hediff is the vanilla worker for a permanent situational thought,
    ///     and every koloss carries that hediff from the moment it is made.
    /// </summary>
    [TestMethod]
    public void AKolossIsContentByDefault() {
        XElement thought = Defs("Races", "KolossHediffs.xml").Descendants("ThoughtDef")
            .First(d => d.Element("defName")?.Value == "Cosmere_Scadrial_Thought_SimpleMinded");

        Assert.AreEqual("ThoughtWorker_Hediff", thought.Element("workerClass")?.Value);
        Assert.AreEqual("Cosmere_Scadrial_Hediff_KolossGrowth", thought.Element("hediff")?.Value);
        Assert.AreEqual("15", thought.Descendants("baseMoodEffect").First().Value);
    }

    /// <summary>
    ///     Nullifying the thoughts stopped a koloss complaining; removing the needs stops it having
    ///     the opinion at all. CurStage decides, and a koloss is in one of the four from its first
    ///     tick, so every stage carries the list.
    /// </summary>
    [TestMethod]
    public void AKolossHasNoOpinionAboutWhereItIs() {
        XElement growth = Defs("Races", "KolossHediffs.xml").Descendants("HediffDef")
            .First(d => d.Element("defName")?.Value == "Cosmere_Scadrial_Hediff_KolossGrowth");

        List<XElement> stages = growth.Element("stages")!.Elements("li").ToList();
        Assert.AreEqual(4, stages.Count);

        foreach (XElement stage in stages) {
            List<string> gone = stage.Element("disablesNeeds")?.Elements("li")
                .Select(li => li.Value).ToList() ?? [];

            foreach (string need in new[] { "Beauty", "Comfort", "RoomSize", "Outdoors" }) {
                CollectionAssert.Contains(
                    gone,
                    need,
                    $"Stage '{stage.Element("label")?.Value}' still leaves it caring about {need}."
                );
            }
        }
    }

    /// <summary>
    ///     Swinging at rock is the same motion as swinging at a person. A koloss is built for one
    ///     of those and is just as strong at the other, so mining gets half the melee buff - and
    ///     only the speed, because a koloss is no more careful about ore than about anything else.
    /// </summary>
    [TestMethod]
    public void AKolossMinesAtHalfWhatItFightsAt() {
        XDocument genes = Defs("Races", "Genes", "Koloss.xml");

        foreach ((string def, double melee, double mining) in new[] {
            ("Cosmere_Scadrial_Gene_KolossHeritage", 2.5, 1.75),
            ("Cosmere_Scadrial_Gene_KolossBlooded", 1.25, 1.125),
        }) {
            XElement factors = genes.Descendants("GeneDef")
                .First(d => d.Element("defName")?.Value == def)
                .Element("statFactors")!;

            Assert.AreEqual(melee, double.Parse(factors.Element("MeleeDamageFactor")!.Value), 0.001);
            Assert.AreEqual(mining, double.Parse(factors.Element("MiningSpeed")!.Value), 0.001);

            // Half the buff, not half the factor: a 2.5 factor is +150%, so half is +75%.
            Assert.AreEqual((melee - 1d) / 2d, mining - 1d, 0.001);

            Assert.IsNull(factors.Element("MiningYield"), "A koloss is not more careful, only faster.");
        }
    }

    /// <summary>
    ///     A koloss is a pack animal that fights. Four times a person was already a lot; the number
    ///     that reads as "send the koloss" rather than "send two colonists" is a good deal more.
    /// </summary>
    [TestMethod]
    public void AKolossCarriesFarMoreThanAPerson() {
        XElement factors = Defs("Races", "Genes", "Koloss.xml").Descendants("GeneDef")
            .First(d => d.Element("defName")?.Value == "Cosmere_Scadrial_Gene_KolossHeritage")
            .Element("statFactors")!;

        Assert.IsTrue(
            double.Parse(factors.Element("CarryingCapacity")!.Value) >= 8d,
            "Hauling is one of the two things a koloss is for."
        );
    }

    /// <summary>
    ///     The mood side of weather was already nullified. This is the other half: hypothermia and
    ///     heatstroke read the stat, not the thought, so a koloss that does not mind the cold still
    ///     froze to death without this.
    /// </summary>
    [TestMethod]
    public void AKolossSurvivesWeatherThatWouldKillAColonist() {
        XElement offsets = Defs("Races", "Genes", "Koloss.xml").Descendants("GeneDef")
            .First(d => d.Element("defName")?.Value == "Cosmere_Scadrial_Gene_KolossHeritage")
            .Element("statOffsets")!;

        Assert.IsTrue(double.Parse(offsets.Element("ComfyTemperatureMin")!.Value) <= -40d);
        Assert.IsTrue(double.Parse(offsets.Element("ComfyTemperatureMax")!.Value) >= 30d);
    }

    /// <summary>
    ///     Ash falls on Scadrial whether anyone wants it or not, so a koloss army feeds itself. The
    ///     gating is entirely in the thought - anyone can swallow ash, and only a koloss can do it
    ///     without it ruining their week.
    /// </summary>
    [TestMethod]
    public void OnlyAKolossEatsAshWithoutMinding() {
        XElement food = XDocument.Load(Path.Combine(
                RepoRoot, "CosmereScadrial", "Defs", "Things", "Ash.xml"
            )).Descendants("ThingDef")
            .First(d => d.Element("defName")?.Value == "Cosmere_Scadrial_Thing_Ash")
            .Element("ingestible")!;

        Assert.AreEqual(
            "DesperateOnly",
            food.Element("preferability")?.Value,
            "Ash must never win against an actual meal."
        );
        Assert.IsTrue(double.Parse(food.Element("nutrition")!.Value) <= 0.1d);

        XElement taste = Defs("Races", "KolossHediffs.xml").Descendants("ThoughtDef")
            .First(d => d.Element("defName")?.Value == food.Element("tasteThought")!.Value);

        Assert.IsTrue(double.Parse(taste.Descendants("baseMoodEffect").First().Value) < 0d);
        CollectionAssert.Contains(
            taste.Element("nullifyingGenes")!.Elements("li").Select(li => li.Value).ToList(),
            "Cosmere_Scadrial_Gene_KolossHeritage"
        );
    }
}
