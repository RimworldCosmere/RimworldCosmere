using System;
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

        Assert.AreEqual("0", growth.Element("initialSeverity")?.Value);
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
            util.Contains("skill.levelInt = 0;", StringComparison.Ordinal),
            "Everything that needed a mind to hold it did not survive the spikes."
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
}
