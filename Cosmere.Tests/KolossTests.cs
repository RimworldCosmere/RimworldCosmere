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
}
