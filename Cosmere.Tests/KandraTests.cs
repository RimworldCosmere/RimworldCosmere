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
        Assert.IsTrue(body.Contains("SpikeCount(pawn)", StringComparison.Ordinal));
        Assert.IsTrue(body.Contains("SpikesPerBlessing", StringComparison.Ordinal));
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
    ///     A kandra that dies mid-disguise used to keep the borrowed name on its corpse, which
    ///     left the person it ate on the colony's dead list forever.
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
}
