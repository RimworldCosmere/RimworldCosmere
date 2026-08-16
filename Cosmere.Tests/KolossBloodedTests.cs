using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Cosmere.System.Scadrial.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     What a koloss leaves behind, once Harmony lets it leave anything.
/// </summary>
[TestClass]
public class KolossBloodedTests {
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

    /// <summary>
    ///     Before the Catacendre a koloss fathers nothing. Keyed to the era rather than the Shard,
    ///     because a sandbox Pre-Catacendre start with Harmony toggled on is still a world of
    ///     Rashek's koloss.
    /// </summary>
    [TestMethod]
    public void AKolossFathersNothingBeforeTheCatacendre() {
        Assert.IsFalse(KolossFertility.CanBreed("Cosmere_Scadrial_Era_PreCatacendre"));
        Assert.IsTrue(KolossFertility.CanBreed("Cosmere_Scadrial_Era_PostCatacendre"));
        Assert.IsTrue(KolossFertility.CanBreed("Cosmere_Scadrial_Era_LostMetal"));

        // No era at all is not the Catacendre having happened.
        Assert.IsFalse(KolossFertility.CanBreed(null));
    }

    /// <summary>
    ///     SetXenotypeDirect is four field assignments and adds no genes. Vanilla gets away with it
    ///     because ApplyBirthOutcome passes forcedEndogenes into the generation request first, and
    ///     this patch runs at the return, long after - so the child was a baseliner wearing a name.
    /// </summary>
    [TestMethod]
    public void AKolossChildIsActuallyGivenTheGenes() {
        string patch = File.ReadAllText(Path.Combine(
            RepoRoot,
            "CosmereCore",
            "CosmereCore",
            "System",
            "Scadrial",
            "Patch",
            "World",
            "KolossBirthPatch.cs"
        ));

        Assert.IsTrue(patch.Contains("child.genes.SetXenotype(blooded)"), "SetXenotype adds the genes.");
        Assert.IsFalse(patch.Contains("SetXenotypeDirect("), "SetXenotypeDirect adds none of them.");
        Assert.IsTrue(patch.Contains("KolossFertility.CanBreedNow()"), "And only after the Catacendre.");
    }

    /// <summary>
    ///     SetXenotype adds genes with AddGene(gene, !xenotype.inheritable), so an uninheritable
    ///     xenotype hands out xenogenes. GetInheritedGenes walks endogenes only, which would leave
    ///     the grandchildren baseliners again.
    /// </summary>
    [TestMethod]
    public void KolossBloodedPassesToTheGenerationAfter() {
        XElement blooded = XDocument.Load(Path.Combine(
                RepoRoot, "CosmereScadrial", "Defs", "Races", "XenoTypes.xml"
            )).Descendants("XenotypeDef")
            .First(d => d.Element("defName")?.Value == "Cosmere_Scadrial_Xenotype_KolossBlooded");

        Assert.AreEqual(
            "true",
            blooded.Element("inheritable")?.Value,
            "Uninheritable means xenogenes, and xenogenes stop at the child."
        );
    }

    /// <summary>
    ///     Their blood spread in the generations after Harmony - spread, not took over. A weighted
    ///     pick rather than the even RandomElement the branch used, so they read as a thinning of
    ///     the line rather than a people.
    /// </summary>
    [TestMethod]
    public void KolossBloodedTurnUpRarelyAfterTheCatacendre() {
        string patch = File.ReadAllText(Path.Combine(
            RepoRoot,
            "CosmereCore",
            "CosmereCore",
            "System",
            "Scadrial",
            "Patch",
            "Gene",
            "ScadrialXenotypePatch.cs"
        ));

        Assert.IsTrue(patch.Contains("Cosmere_Scadrial_Xenotype_KolossBlooded"), "They have to exist at all.");
        Assert.IsTrue(patch.Contains("RandomElementByWeight"), "An even pick would make them a third of Scadrial.");
        Assert.IsTrue(patch.Contains("KolossBlooded, 5f"), "Rare, not absent.");
    }
}
