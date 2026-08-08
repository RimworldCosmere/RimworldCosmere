using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Guards the kandra: the six genes their xenotype names, the Blessings that hold them
///     together, and the mistwraith they drop to when the spikes come out.
/// </summary>
/// <remarks>
///     None of this can be exercised against a live game from here, so every check reads the
///     defs and the source off disk. That catches the failures that actually happened while
///     building it - a def referenced by a name nothing defines, and a gene whose PostAdd
///     re-entered the xenotype it was being added by.
/// </remarks>
[TestClass]
public class KandraTests {
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

    private static readonly string[] BlessingDefNames = [
        "Cosmere_Scadrial_Hediff_BlessingOfPresence",
        "Cosmere_Scadrial_Hediff_BlessingOfPotency",
        "Cosmere_Scadrial_Hediff_BlessingOfStability",
        "Cosmere_Scadrial_Hediff_BlessingOfAwareness",
    ];

    private static IEnumerable<XElement> DefsOfType(string typeName) {
        string defs = Path.Combine(RepoRoot, "CosmereScadrial", "Defs");
        foreach (string file in Directory.GetFiles(defs, "*.xml", SearchOption.AllDirectories)) {
            XDocument doc;
            try {
                doc = XDocument.Load(file);
            } catch (Exception) {
                continue;
            }

            if (doc.Root == null) continue;
            foreach (XElement element in doc.Root.Elements(typeName)) yield return element;
        }
    }

    private static HashSet<string> DefNamesOfType(string typeName) {
        HashSet<string> names = [];
        foreach (XElement element in DefsOfType(typeName)) {
            string? name = element.Element("defName")?.Value;
            if (!string.IsNullOrWhiteSpace(name)) names.Add(name!);
        }

        return names;
    }

    private static string Source(params string[] parts) {
        string path = Path.Combine([RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", .. parts]);
        Assert.IsTrue(File.Exists(path), $"Expected source file at {path}.");
        return File.ReadAllText(path);
    }

    [TestMethod]
    public void EveryBlessingHediffExists() {
        HashSet<string> hediffs = DefNamesOfType("HediffDef");
        foreach (string blessing in BlessingDefNames) {
            Assert.IsTrue(hediffs.Contains(blessing), $"{blessing} is referenced but never defined.");
        }
    }

    [TestMethod]
    public void MistwraithHediffExists() {
        Assert.IsTrue(DefNamesOfType("HediffDef").Contains("Cosmere_Scadrial_Hediff_Mistwraith"));
    }

    [TestMethod]
    public void KandraXenotypeNamesOnlyGenesThatExist() {
        XElement? kandra = DefsOfType("XenotypeDef")
            .FirstOrDefault(x => x.Element("defName")?.Value == "Cosmere_Scadrial_Xenotype_Kandra");
        Assert.IsNotNull(kandra, "The kandra xenotype is missing.");

        HashSet<string> genes = DefNamesOfType("GeneDef");
        List<string> named = kandra!.Element("genes")?.Elements("li").Select(li => li.Value).ToList() ?? [];

        Assert.AreEqual(5, named.Count, "The kandra xenotype should carry exactly its five genes.");
        foreach (string gene in named) {
            Assert.IsTrue(genes.Contains(gene), $"The kandra xenotype names {gene}, which no GeneDef defines.");
        }
    }

    /// <summary>
    ///     A kandra is not inheritable. Making it so would let two kandra in a colony have a
    ///     kandra child, and kandra do not breed - they are made out of mistwraiths one at a time.
    /// </summary>
    [TestMethod]
    public void KandraXenotypeIsNotInheritable() {
        XElement kandra = DefsOfType("XenotypeDef")
            .First(x => x.Element("defName")?.Value == "Cosmere_Scadrial_Xenotype_Kandra");

        Assert.AreEqual("false", kandra.Element("inheritable")?.Value);
    }

    [TestMethod]
    public void EveryBlessingSurgeryNamesABlessingThatExists() {
        HashSet<string> hediffs = DefNamesOfType("HediffDef");
        int found = 0;

        foreach (XElement recipe in DefsOfType("RecipeDef")) {
            XElement? extension = recipe.Element("modExtensions")?
                .Elements("li")
                .FirstOrDefault(li => (string?)li.Attribute("Class") == "Cosmere.System.Scadrial.Hemalurgy.BlessingExtension");
            if (extension == null) continue;

            found++;
            string? blessing = extension.Element("blessing")?.Value;
            Assert.IsFalse(string.IsNullOrWhiteSpace(blessing), "A Blessing surgery has an empty blessing field.");
            Assert.IsTrue(hediffs.Contains(blessing!), $"A Blessing surgery names {blessing}, which no HediffDef defines.");
        }

        Assert.AreEqual(BlessingDefNames.Length, found, "There should be one surgery per Blessing.");
    }

    /// <summary>
    ///     PostAdd runs while the xenotype is being applied. Calling Become from there sets the
    ///     xenotype again and the whole thing re-enters, so PostAdd must take the narrow path
    ///     that only adds the hediff and the spikes.
    /// </summary>
    [TestMethod]
    public void BlessingBoundPostAddDoesNotReapplyTheXenotype() {
        string source = Source("Gene", "BlessingBound.cs");
        int postAdd = source.IndexOf("public override void PostAdd()", StringComparison.Ordinal);
        int tick = source.IndexOf("public override void TickInterval(", StringComparison.Ordinal);

        Assert.IsTrue(postAdd >= 0 && tick > postAdd, "BlessingBound should have both PostAdd and TickInterval.");

        string body = source.Substring(postAdd, tick - postAdd);
        Assert.IsFalse(
            body.Contains("KandraUtility.Become", StringComparison.Ordinal),
            "PostAdd must not call Become - it reapplies the xenotype from inside gene addition."
        );
        Assert.IsTrue(body.Contains("KandraUtility.GiveBlessing", StringComparison.Ordinal));
    }

    /// <summary>
    ///     A Blessing is a pair of spikes and the spikes are what get pulled, so the state has
    ///     to be driven off how many are left rather than off the Blessing hediff vanishing.
    ///     Two is a kandra, one is half-blessed, none is a mistwraith.
    /// </summary>
    [TestMethod]
    public void SpikeCountDrivesAllThreeStates() {
        string gene = Source("Gene", "BlessingBound.cs");
        Assert.IsTrue(
            gene.Contains("KandraUtility.ReconcileSpikes", StringComparison.Ordinal),
            "BlessingBound should delegate to ReconcileSpikes."
        );

        string util = Source("Util", "KandraUtility.cs");
        int start = util.IndexOf("public static void ReconcileSpikes(", StringComparison.Ordinal);
        Assert.IsTrue(start >= 0, "ReconcileSpikes is missing.");

        string body = util[start..];
        Assert.IsTrue(body.Contains("CompleteBlessingsOn(pawn)", StringComparison.Ordinal));
        Assert.IsTrue(body.Contains("SpikeCount(pawn) <= 0", StringComparison.Ordinal));
        Assert.IsTrue(body.Contains("Cosmere_Scadrial_Hediff_HalfBlessed", StringComparison.Ordinal));
        Assert.IsTrue(body.Contains("RevertToMistwraith", StringComparison.Ordinal));
    }

    /// <summary>
    ///     Pulling a spike in surgery has to take effect immediately. Waiting for the slow gene
    ///     tick would leave the surgeon watching nothing happen.
    /// </summary>
    [TestMethod]
    public void SurgeryReconcilesSpikesImmediately() {
        string source = Source("Hemalurgy", "RecipeWorker", "RemoveSpike.cs");
        Assert.IsTrue(source.Contains("KandraUtility.ReconcileSpikes", StringComparison.Ordinal));
    }

    [TestMethod]
    public void HalfBlessedHediffExists() {
        Assert.IsTrue(DefNamesOfType("HediffDef").Contains("Cosmere_Scadrial_Hediff_HalfBlessed"));
    }

    /// <summary>
    ///     Wearing a dead Mistborn's face must not hand over her Allomancy. Shapeshifting is a
    ///     disguise; if it granted the xenotype it would be a way to farm powers off corpses.
    /// </summary>
    [TestMethod]
    public void ShapeshiftingCopiesLooksAndNotPowers() {
        string source = Source("Kandra", "KandraShapeshift.cs");
        int apply = source.IndexOf("private static void ApplyTo(", StringComparison.Ordinal);
        Assert.IsTrue(apply >= 0);

        string body = source[apply..];
        Assert.IsFalse(
            body.Contains("SetXenotype", StringComparison.Ordinal),
            "ApplyTo must never set the xenotype from a stolen form."
        );
    }

    /// <summary>
    ///     Overwriting pawn.Name left a dead kandra permanently carrying the name of the person
    ///     it ate. The pawn keeps its own name now and the worn one is added only where it is
    ///     displayed, which also keeps the colonist bar honest.
    /// </summary>
    [TestMethod]
    public void ShapeshiftingNeverOverwritesTheRealName() {
        string source = Source("Kandra", "KandraShapeshift.cs");
        Assert.IsFalse(
            source.Contains("pawn.Name =", StringComparison.Ordinal),
            "ApplyTo must not assign pawn.Name."
        );

        string comp = Source("Kandra", "CompKandraForms.cs");
        Assert.IsTrue(comp.Contains("WornName", StringComparison.Ordinal));
        Assert.IsTrue(comp.Contains("CompInspectStringExtra", StringComparison.Ordinal));
    }

    /// <summary>
    ///     Only the map label gains the second name. The colonist bar takes a different path, so
    ///     patching GetPawnLabel is what keeps the two different.
    /// </summary>
    [TestMethod]
    public void OnlyTheMapLabelShowsTheWornName() {
        string path = Path.Combine(
            RepoRoot, "CosmereCore", "CosmereCore", "Core", "Patch", "UI", "KandraMapLabelPatch.cs"
        );
        Assert.IsTrue(File.Exists(path), "The map label patch is missing.");

        string source = File.ReadAllText(path);
        Assert.IsTrue(source.Contains("GetPawnLabel", StringComparison.Ordinal));
        Assert.IsTrue(source.Contains("WornName", StringComparison.Ordinal));
    }

    /// <summary>
    ///     A kandra that dies mid-disguise should not stay in somebody else's shape.
    /// </summary>
    [TestMethod]
    public void DeathDropsTheDisguise() {
        string source = Source("Kandra", "CompKandraForms.cs");
        int killed = source.IndexOf("public override void Notify_Killed(", StringComparison.Ordinal);
        Assert.IsTrue(killed >= 0, "CompKandraForms should revert the disguise on death.");
        Assert.IsTrue(source[killed..].Contains("KandraShapeshift.Revert", StringComparison.Ordinal));
    }

    /// <summary>
    ///     Every work tag the mistwraith stage disables has to be a real WorkTags member.
    ///     A typo here loads as a red error and silently leaves the tag enabled.
    /// </summary>
    [TestMethod]
    public void MistwraithDisablesOnlyRealWorkTags() {
        HashSet<string> valid = [
            "None", "Animals", "ManualDumb", "ManualSkilled", "Violent", "Caring", "Social",
            "Commoner", "Cleaning", "Hauling", "Crafting", "Mining", "PlantWork", "Intellectual",
            "Firefighting", "Constructing", "Artistic", "Cooking", "Shooting", "AllWork",
        ];

        XElement mistwraith = DefsOfType("HediffDef")
            .First(h => h.Element("defName")?.Value == "Cosmere_Scadrial_Hediff_Mistwraith");

        List<string> tags = mistwraith.Element("stages")?
            .Elements("li")
            .SelectMany(stage => stage.Element("disabledWorkTags")?.Elements("li") ?? [])
            .Select(li => li.Value)
            .ToList() ?? [];

        Assert.IsTrue(tags.Count > 0, "The mistwraith stage should disable work.");
        foreach (string tag in tags) {
            Assert.IsTrue(valid.Contains(tag), $"'{tag}' is not a WorkTags member.");
        }
    }

    /// <summary>
    ///     A Blessing is two spikes, and the rest of hemalurgy reads spikes rather than the
    ///     Blessing hediff. Without the real pair, bronze cannot find a kandra and Ruin cannot
    ///     reach one, which is backwards for the most famously spiked people on Scadrial.
    /// </summary>
    [TestMethod]
    public void BecomingAKandraDrivesRealSpikes() {
        string source = Source("Util", "KandraUtility.cs");
        Assert.IsTrue(source.Contains("new ImplantedSpikeData", StringComparison.Ordinal));
        Assert.IsTrue(source.Contains("SpikesPerBlessing = 2", StringComparison.Ordinal));

        foreach (string blessing in BlessingDefNames) {
            Assert.IsTrue(
                source.Contains(blessing, StringComparison.Ordinal),
                $"{blessing} has no metal and steal type mapped for its spikes."
            );
        }
    }

    /// <summary>
    ///     Gene.GetGizmos returns null rather than an empty sequence. Foreaching it directly
    ///     threw a NullReferenceException every frame a kandra was selected.
    /// </summary>
    [TestMethod]
    public void BodyAbsorptionGuardsTheInheritedGizmos() {
        string source = Source("Gene", "BodyAbsorption.cs");
        Assert.IsFalse(
            source.Contains("foreach (Verse.Gizmo gizmo in base.GetGizmos())", StringComparison.Ordinal),
            "base.GetGizmos() can be null and must be null-checked before iterating."
        );
        Assert.IsTrue(source.Contains("if (inherited != null)", StringComparison.Ordinal));
    }

    /// <summary>
    ///     Kandra do not age. A lifespan multiplier only stretches out a clock that is still
    ///     running, which is a different thing and was what this used to have.
    /// </summary>
    [TestMethod]
    public void KandraDoNotAge() {
        XElement heritage = DefsOfType("GeneDef")
            .First(g => g.Element("defName")?.Value == "Cosmere_Scadrial_Gene_KandraHeritage");

        XElement? curve = heritage.Element("biologicalAgeTickFactorFromAgeCurve");
        Assert.IsNotNull(curve, "The kandra heritage gene should stop biological ageing.");

        List<string> points = curve!.Element("points")?.Elements("li").Select(li => li.Value).ToList() ?? [];
        Assert.IsTrue(points.Count > 0, "The ageing curve has no points.");
        foreach (string point in points) {
            Assert.IsTrue(
                point.TrimEnd(')', ' ').EndsWith("0", StringComparison.Ordinal),
                $"Every point on the ageing curve must be zero, but found {point}."
            );
        }

        foreach (XElement gene in DefsOfType("GeneDef")) {
            string? name = gene.Element("defName")?.Value;
            if (name == null || !name.Contains("Kandra")) continue;

            Assert.IsNull(
                gene.Element("statFactors")?.Element("LifespanFactor"),
                $"{name} still has a LifespanFactor. Ageless pawns do not need one."
            );
        }
    }

    /// <summary>
    ///     A kandra should catch nothing a sanguophage would not. Vanilla's perfect-immunity
    ///     gene is the benchmark, so anything on that list has to be on this one.
    /// </summary>
    [TestMethod]
    public void KandraAreAtLeastAsImmuneAsASanguophage() {
        string[] perfectImmunity = [
            "Flu", "Malaria", "SleepingSickness", "Plague", "WoundInfection",
            "LungRot", "GutWorms", "MuscleParasites", "OrganDecay",
        ];

        XElement heritage = DefsOfType("GeneDef")
            .First(g => g.Element("defName")?.Value == "Cosmere_Scadrial_Gene_KandraHeritage");

        HashSet<string> immune = heritage.Element("makeImmuneTo")?
            .Elements("li")
            .Select(li => li.Value)
            .ToHashSet() ?? [];

        foreach (string disease in perfectImmunity) {
            Assert.IsTrue(immune.Contains(disease), $"A kandra should be immune to {disease}.");
        }
    }

    /// <summary>
    ///     Generation is a record of when the Contract made this kandra, not a heritable trait.
    ///     It rides on the heritage gene rather than having one of its own.
    /// </summary>
    [TestMethod]
    public void GenerationIsNotItsOwnGene() {
        Assert.IsFalse(
            DefNamesOfType("GeneDef").Contains("Cosmere_Scadrial_Gene_Generational"),
            "Generation should not be a gene. It is not biology."
        );

        XElement heritage = DefsOfType("GeneDef")
            .First(g => g.Element("defName")?.Value == "Cosmere_Scadrial_Gene_KandraHeritage");
        Assert.AreEqual(
            "Cosmere.System.Scadrial.Gene.KandraHeritage",
            heritage.Element("geneClass")?.Value,
            "The heritage gene should carry the generation roll."
        );

        string source = Source("Gene", "KandraHeritage.cs");
        Assert.IsTrue(source.Contains("kandraGeneration", StringComparison.Ordinal), "Generation must still be saved.");
    }

    /// <summary>
    ///     Two genes sharing an exclusion tag switch each other off.
    /// </summary>
    /// <remarks>
    ///     Pawn_GeneTracker.CheckForOverrides compares every pair of genes and overrides one of
    ///     any pair whose defs ConflictsWith. Putting one tag on an abstract base to group a
    ///     xenotype's genes therefore disables all but one of them, silently. Both the kandra
    ///     and koloss sets shipped that way and almost nothing they did actually ran.
    /// </remarks>
    [TestMethod]
    public void NoTwoGenesInOneSetShareAnExclusionTag() {
        string genes = Path.Combine(RepoRoot, "CosmereScadrial", "Defs", "Races", "Genes");

        foreach (string file in Directory.GetFiles(genes, "*.xml")) {
            XDocument doc = XDocument.Load(file);
            if (doc.Root == null) continue;

            Dictionary<string, XElement> abstracts = doc.Root.Elements("GeneDef")
                .Where(g => g.Attribute("Name") != null)
                .ToDictionary(g => g.Attribute("Name")!.Value, g => g);

            Dictionary<string, List<string>> byTag = [];

            foreach (XElement gene in doc.Root.Elements("GeneDef")) {
                string? name = gene.Element("defName")?.Value;
                if (name == null) continue;

                List<string> tags = Tags(gene);
                XElement? walk = gene;
                while (walk?.Attribute("ParentName")?.Value is string parent
                       && abstracts.TryGetValue(parent, out XElement? next)) {
                    tags.AddRange(Tags(next));
                    walk = next;
                }

                foreach (string tag in tags.Distinct()) {
                    if (!byTag.TryGetValue(tag, out List<string>? owners)) byTag[tag] = owners = [];
                    owners.Add(name);
                }
            }

            foreach ((string tag, List<string> owners) in byTag) {
                Assert.AreEqual(
                    1,
                    owners.Count,
                    $"In {Path.GetFileName(file)}, exclusion tag '{tag}' is on {owners.Count} genes "
                    + $"({string.Join(", ", owners)}). All but one would be switched off in game."
                );
            }
        }

        static List<string> Tags(XElement gene) {
            return gene.Element("exclusionTags")?.Elements("li").Select(li => li.Value).ToList() ?? [];
        }
    }

    /// <summary>
    ///     A Blessing hediff with no spikes under it is a kandra with nothing holding it
    ///     together: no spike for a surgeon to pull, and the next spike check would drop it to a
    ///     mistwraith. PostAdd has to top the pair up rather than bail when it sees the hediff.
    /// </summary>
    [TestMethod]
    public void PostAddGivesSpikesEvenWhenTheBlessingIsAlreadyThere() {
        string source = Source("Gene", "BlessingBound.cs");
        int start = source.IndexOf("public override void PostAdd()", StringComparison.Ordinal);
        int end = source.IndexOf("public override void TickInterval(", StringComparison.Ordinal);
        Assert.IsTrue(start >= 0 && end > start);

        string body = source[start..end];
        Assert.IsFalse(
            body.Contains("if (KandraUtility.HasBlessing(pawn)) return;", StringComparison.Ordinal),
            "PostAdd must not bail on an existing Blessing; the spikes may still be missing."
        );
        Assert.IsTrue(body.Contains("KandraUtility.GiveBlessing", StringComparison.Ordinal));
    }

    /// <summary>
    ///     The spikes hediff has to be attached to a body part.
    /// </summary>
    /// <remarks>
    ///     Recipe_Surgery asks its worker for parts to operate on, and RemoveSpike answers with
    ///     the spikes hediff's Part. Adding the hediff with no part leaves that null, the recipe
    ///     reports no valid parts, and "remove hemalurgic spike" never appears in the bill list.
    ///     Nothing logs. The kandra just cannot be unmade.
    /// </remarks>
    [TestMethod]
    public void SpikesAreAttachedToTheTorso() {
        string source = Source("Util", "KandraUtility.cs");
        int start = source.IndexOf("private static void DriveSpikes(", StringComparison.Ordinal);
        Assert.IsTrue(start >= 0);

        string body = source[start..];
        Assert.IsTrue(
            body.Contains("BodyPartDefOf.Torso", StringComparison.Ordinal),
            "The spikes hediff must be placed on a real body part."
        );
        Assert.IsTrue(
            body.Contains("HediffMaker.MakeHediff", StringComparison.Ordinal),
            "Build the hediff with its part, the way the implant surgery does."
        );
        Assert.IsFalse(
            body.Contains("pawn.health.AddHediff(\n                Hemalurgy.HemalurgicDefOf", StringComparison.Ordinal),
            "Do not add the spikes hediff without a part."
        );
    }

    /// <summary>
    ///     A kandra saved before the part fix carries a part-less hediff, which would keep the
    ///     removal surgery hidden for the rest of that colony's life.
    /// </summary>
    [TestMethod]
    public void PartlessSpikeHediffsGetReseated() {
        string source = Source("Util", "KandraUtility.cs");
        Assert.IsTrue(source.Contains("stray.Part == null", StringComparison.Ordinal));
        Assert.IsTrue(
            source.Contains("salvaged", StringComparison.Ordinal),
            "Re-seating must keep the spikes that were already in it."
        );
    }

    /// <summary>
    ///     A Blessing is two spikes of one metal stealing one thing. Counting every spike in the
    ///     body would let a kandra hold its mind together on a scavenged pair off somebody else.
    /// </summary>
    [TestMethod]
    public void OnlyTheBlessingsOwnMatchedPairCounts() {
        string source = Source("Util", "KandraUtility.cs");

        int start = source.IndexOf("public static int MatchingSpikeCount(", StringComparison.Ordinal);
        Assert.IsTrue(start >= 0, "MatchingSpikeCount is missing.");

        string body = source[start..];
        Assert.IsTrue(body.Contains("metalDefName != recipe.Value.metal", StringComparison.Ordinal));
        Assert.IsTrue(body.Contains("stealType != recipe.Value.steal", StringComparison.Ordinal));

        int rec = source.IndexOf("public static void ReconcileSpikes(", StringComparison.Ordinal);
        string recBody = source[rec..];
        Assert.IsTrue(
            recBody.Contains("CompleteBlessingsOn(pawn)", StringComparison.Ordinal),
            "ReconcileSpikes must work from whole Blessings, not a raw spike count."
        );
    }

    /// <summary>
    ///     Holding somebody else's face takes a whole mind. One spike or none and the kandra
    ///     drops the shape and loses the buttons entirely.
    /// </summary>
    [TestMethod]
    public void DegradedKandraCannotTakeAForm() {
        string util = Source("Util", "KandraUtility.cs");
        int start = util.IndexOf("public static bool CanHoldAShape(", StringComparison.Ordinal);
        Assert.IsTrue(start >= 0, "CanHoldAShape is missing.");

        string body = util[start..];
        Assert.IsTrue(body.Contains("Cosmere_Scadrial_Hediff_HalfBlessed", StringComparison.Ordinal));
        Assert.IsTrue(body.Contains("Cosmere_Scadrial_Hediff_Mistwraith", StringComparison.Ordinal));

        string gene = Source("Gene", "BodyAbsorption.cs");
        Assert.IsTrue(
            gene.Contains("CanHoldAShape(pawn)) yield break", StringComparison.Ordinal),
            "The gizmos should disappear, not merely disable."
        );

        Assert.IsTrue(
            util.Contains("WearMistwraithShape", StringComparison.Ordinal),
            "Both degraded states should drop to the mistwraith shape."
        );
    }

    /// <summary>
    ///     The spikes hold a kandra's mind rather than contain it, so a fresh pair gives back the
    ///     same person. Without the snapshot, re-blessing would produce a blank one.
    /// </summary>
    [TestMethod]
    public void ReblessingRestoresSkillsAndMemories() {
        string mind = Source("Kandra", "KandraMind.cs");
        Assert.IsTrue(mind.Contains("public void Store(Pawn pawn)", StringComparison.Ordinal));
        Assert.IsTrue(mind.Contains("public void Restore(Pawn pawn)", StringComparison.Ordinal));
        Assert.IsTrue(mind.Contains("memories", StringComparison.Ordinal));
        Assert.IsTrue(
            mind.Contains("if (held) return;", StringComparison.Ordinal),
            "Losing the second spike must not overwrite the snapshot taken at the first."
        );

        string util = Source("Util", "KandraUtility.cs");
        Assert.IsTrue(util.Contains("Mind.Store(pawn)", StringComparison.Ordinal));
        Assert.IsTrue(util.Contains("Mind.Restore(pawn)", StringComparison.Ordinal));
    }

    /// <summary>
    ///     Kandra can carry more than one Blessing, and each stands on its own pair of spikes.
    /// </summary>
    /// <remarks>
    ///     Topping up to two spikes in total was right while a kandra could only have one
    ///     Blessing. With three, that is six spikes, and a new Blessing would have been driven
    ///     into a body that already had two and given nothing.
    /// </remarks>
    [TestMethod]
    public void EachBlessingStandsOnItsOwnPair() {
        string source = Source("Util", "KandraUtility.cs");

        Assert.IsTrue(source.Contains("public static List<Hediff> BlessingsOn(", StringComparison.Ordinal));
        Assert.IsTrue(source.Contains("public static List<Hediff> CompleteBlessingsOn(", StringComparison.Ordinal));

        int drive = source.IndexOf("private static void DriveSpikes(", StringComparison.Ordinal);
        string body = source[drive..];
        Assert.IsTrue(
            body.Contains("MatchingSpikeCount(pawn, blessing); i < SpikesPerBlessing", StringComparison.Ordinal),
            "DriveSpikes must top up this Blessing's pair, not the pawn's total spike count."
        );
    }

    /// <summary>
    ///     A Blessing that loses a spike stops applying, and takes its stats with it, while the
    ///     kandra's other Blessings carry on.
    /// </summary>
    [TestMethod]
    public void ABrokenBlessingIsRemovedButTheOthersSurvive() {
        string source = Source("Util", "KandraUtility.cs");
        int rec = source.IndexOf("public static void ReconcileSpikes(", StringComparison.Ordinal);
        string body = source[rec..];

        Assert.IsTrue(
            body.Contains("MatchingSpikeCount(pawn, all[i].def) < SpikesPerBlessing", StringComparison.Ordinal),
            "Each Blessing should be judged on its own pair."
        );
        Assert.IsTrue(
            body.Contains("if (whole > 0)", StringComparison.Ordinal),
            "One whole Blessing should be enough to stay a kandra."
        );
    }

    /// <summary>
    ///     Coming back from a mistwraith grey and nameless would read as a different person
    ///     walking in. The face it was wearing goes back on with the mind.
    /// </summary>
    [TestMethod]
    public void RepairingAKandraPutsItsFaceBackOn() {
        string mind = Source("Kandra", "KandraMind.cs");
        Assert.IsTrue(mind.Contains("KandraForm? shape", StringComparison.Ordinal));
        Assert.IsTrue(mind.Contains("public KandraForm? Shape => shape;", StringComparison.Ordinal));

        string util = Source("Util", "KandraUtility.cs");
        int rec = util.IndexOf("public static void ReconcileSpikes(", StringComparison.Ordinal);
        string body = util[rec..];
        Assert.IsTrue(body.Contains("KandraShapeshift.Wear(pawn, worn)", StringComparison.Ordinal));
    }

    /// <summary>
    ///     The surgery used to demand a mistwraith, which meant a working kandra could never be
    ///     given a second Blessing.
    /// </summary>
    [TestMethod]
    public void AKandraCanBeGivenAnotherBlessing() {
        string source = Source("Hemalurgy", "RecipeWorker", "GiveBlessing.cs");
        Assert.IsTrue(
            source.Contains("return KandraUtility.IsKandra(pawn);", StringComparison.Ordinal),
            "An awake kandra should be offered Blessings it does not have."
        );
        Assert.IsTrue(
            source.Contains("HasHediff(blessing) == true) return false", StringComparison.Ordinal),
            "But not the same Blessing twice."
        );
    }

    /// <summary>
    ///     Granting a second Blessing has to add that Blessing's hediff.
    /// </summary>
    /// <remarks>
    ///     The guard asked whether the pawn had any Blessing at all, so a kandra already carrying
    ///     Presence got the spikes for Potency driven in and no Potency hediff to show for it.
    /// </remarks>
    [TestMethod]
    public void GivingASecondBlessingAddsItsOwnHediff() {
        string source = Source("Util", "KandraUtility.cs");
        int start = source.IndexOf("public static void GiveBlessing(", StringComparison.Ordinal);
        Assert.IsTrue(start >= 0);

        string body = source[start..(start + 600)];
        Assert.IsFalse(
            body.Contains("BlessingOn(pawn) == null", StringComparison.Ordinal),
            "Checking for any Blessing skips the one being granted."
        );
        Assert.IsTrue(body.Contains("HasHediff(blessing)", StringComparison.Ordinal));
    }

    /// <summary>
    ///     OreSeur is Third Generation, which is a fact about him rather than a roll.
    /// </summary>
    [TestMethod]
    public void OreSeurIsThirdGeneration() {
        foreach (string file in new[] { "PreCatacendre.xml", "FinalEmpire.xml", "WellOfAscension.xml" }) {
            XDocument doc = XDocument.Load(Path.Combine(RepoRoot, "CosmereScadrial", "Defs", "Scenarios", file));
            XElement? oreSeur = doc.Descendants("li")
                .FirstOrDefault(li => li.Element("firstName")?.Value == "OreSeur");

            Assert.IsNotNull(oreSeur, $"OreSeur is missing from {file}.");
            Assert.AreEqual("3", oreSeur!.Element("kandraGeneration")?.Value, $"in {file}");
        }
    }

    /// <summary>
    ///     A quickstart exists to hand a pawn the gear and powers a test needs. Renaming somebody
    ///     to Kaladin throws away the roster the scenario just built.
    /// </summary>
    [TestMethod]
    public void QuickstartsDoNotRewriteWhoAPawnIs() {
        string root = Path.Combine(RepoRoot, "CosmereCore", "CosmereCore");

        foreach (string file in Directory.GetFiles(root, "*Quickstart*.cs", SearchOption.AllDirectories)) {
            string source = File.ReadAllText(file);

            Assert.IsFalse(
                source.Contains("pawn.Name = ", StringComparison.Ordinal),
                $"{Path.GetFileName(file)} renames an existing colonist."
            );
            Assert.IsFalse(
                source.Contains("pawn.gender = ", StringComparison.Ordinal),
                $"{Path.GetFileName(file)} changes an existing colonist's gender."
            );
        }
    }

    /// <summary>
    ///     A recipe filter must not try to name a metal through stuffCategoriesToAllow.
    /// </summary>
    /// <remarks>
    ///     ThingFilter.SetAllow(StuffCategoryDef) walks every ThingDef and allows the ones that
    ///     ARE that stuff, so listing the zinc category made zinc bars a valid stand-in for a
    ///     spike. Nothing in ThingFilter can express "a spike made of zinc"; a thing's stuff is
    ///     not filterable. The metal has to be checked in the recipe worker instead.
    /// </remarks>
    [TestMethod]
    public void BlessingSurgeriesDoNotFilterSpikesByStuffCategory() {
        foreach (XElement recipe in DefsOfType("RecipeDef")) {
            string? name = recipe.Element("defName")?.Value;
            if (name == null || !name.StartsWith("Cosmere_Scadrial_Recipe_BlessingOf", StringComparison.Ordinal)) {
                continue;
            }

            Assert.AreEqual(
                0,
                recipe.Descendants("stuffCategoriesToAllow").Count(),
                $"{name} filters by stuff category, which lets bare metal count as a spike."
            );
        }
    }

    /// <summary>
    ///     Each Blessing surgery asks for exactly one lot of spikes and one of medicine.
    /// </summary>
    /// <remarks>
    ///     Def inheritance appends list nodes rather than replacing them. Leaving a generic
    ///     ingredients block on the abstract base stacked it on top of each child's metal
    ///     specific one, and every surgery asked for four spikes and four medicine.
    /// </remarks>
    [TestMethod]
    public void ABlessingCostsTwoSpikesNotFour() {
        int seen = 0;

        foreach (XElement recipe in DefsOfType("RecipeDef")) {
            string? name = recipe.Element("defName")?.Value;
            if (name == null || !name.StartsWith("Cosmere_Scadrial_Recipe_BlessingOf", StringComparison.Ordinal)) {
                continue;
            }

            seen++;
            List<XElement> lines = recipe.Element("ingredients")!.Elements("li").ToList();

            Assert.AreEqual(2, lines.Count, $"{name} lists {lines.Count} ingredient lines, expected spikes and medicine.");
            foreach (XElement line in lines) {
                Assert.AreEqual("2", line.Element("count")?.Value, $"{name} asks for the wrong count.");
            }
        }

        Assert.AreEqual(4, seen, "A Blessing surgery is missing.");

        // And the base must contribute none, or they come back doubled.
        XElement? abstractBase = DefsOfType("RecipeDef")
            .FirstOrDefault(r => (string?)r.Attribute("Name") == "Cosmere_Scadrial_KandraBlessingBase");
        Assert.IsNotNull(abstractBase);
        Assert.IsNull(abstractBase!.Element("ingredients"), "The base must not list ingredients; they append.");
    }

    /// <summary>
    ///     Each Blessing surgery rejects spikes of the wrong metal.
    /// </summary>
    /// <remarks>
    ///     A ThingFilter cannot see a thing's stuff, and its stuff category list allows the
    ///     material as an item instead, which is how zinc bars became a valid spike. The only
    ///     hook handed the Thing is a special filter's worker, so the metal check goes there and
    ///     each recipe wears it as a specialFiltersToDisallow entry.
    /// </remarks>
    [TestMethod]
    public void EachBlessingRejectsSpikesOfTheWrongMetal() {
        Dictionary<string, string> expected = new() {
            ["Cosmere_Scadrial_Recipe_BlessingOfPresence"] = "Cosmere_Scadrial_SpecialFilter_NotSpikeCopper",
            ["Cosmere_Scadrial_Recipe_BlessingOfPotency"] = "Cosmere_Scadrial_SpecialFilter_NotSpikeIron",
            ["Cosmere_Scadrial_Recipe_BlessingOfStability"] = "Cosmere_Scadrial_SpecialFilter_NotSpikeZinc",
            ["Cosmere_Scadrial_Recipe_BlessingOfAwareness"] = "Cosmere_Scadrial_SpecialFilter_NotSpikeTin",
        };

        HashSet<string> filters = DefNamesOfType("SpecialThingFilterDef");
        int seen = 0;

        foreach (XElement recipe in DefsOfType("RecipeDef")) {
            string? name = recipe.Element("defName")?.Value;
            if (name == null || !expected.TryGetValue(name, out string? wanted)) continue;

            seen++;
            List<string> disallowed = recipe.Descendants("specialFiltersToDisallow")
                .Elements("li").Select(li => li.Value).ToList();

            CollectionAssert.Contains(disallowed, wanted, $"{name} accepts spikes of any metal.");
            Assert.IsTrue(filters.Contains(wanted), $"{wanted} is referenced but never defined.");
        }

        Assert.AreEqual(expected.Count, seen, "A Blessing surgery is missing.");
    }

    /// <summary>
    ///     The worker must only ever have an opinion about spikes. Matching anything else would
    ///     quietly drop medicine out of the same bill.
    /// </summary>
    [TestMethod]
    public void TheSpikeMetalFilterOnlyJudgesSpikes() {
        string source = Source("Hemalurgy", "SpecialThingFilterWorker_WrongSpikeMetal.cs");

        Assert.IsTrue(source.Contains("public override bool Matches(", StringComparison.Ordinal));
        Assert.IsTrue(
            source.Contains("Cosmere_Scadrial_Thing_HemalurgicSpike", StringComparison.Ordinal),
            "It has to check the thing is a spike before judging its metal."
        );
        Assert.IsTrue(
            source.Contains("t.Stuff?.defName != wanted", StringComparison.Ordinal),
            "The metal comes from the thing's stuff."
        );
    }
}
