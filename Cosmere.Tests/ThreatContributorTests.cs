using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The shard contributors read pawns, so they cannot run here. These guard the wiring that
///     the pure threat classes cannot see.
/// </summary>
/// <remarks>
///     Two shards adding to the raid points independently is what this replaced, so the guards
///     that matter are about going through the registry and reading one rule rather than two.
/// </remarks>
[TestClass]
public class ThreatContributorTests {
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

    private static string Core(params string[] parts) => File.ReadAllText(Path.Combine(
        [RepoRoot, "CosmereCore", "CosmereCore", .. parts]
    ));

    private static string Roshar(string file) => Core("System", "Roshar", "Threat", file);

    private static string Scadrial(string file) => Core("System", "Scadrial", "Threat", file);

    [TestMethod]
    public void BothShardsRegisterAContributor() {
        StringAssert.Contains(
            Roshar("RosharThreatRegistration.cs"),
            "ThreatContributorRegistry.Register",
            "Roshar never reaches the registry, so its threat is silently zero"
        );
        StringAssert.Contains(
            Scadrial("ScadrialThreatRegistration.cs"),
            "ThreatContributorRegistry.Register",
            "Scadrial never reaches the registry, so its threat is silently zero"
        );
    }

    /// <summary>
    ///     A contributor reports a gain over a plain colonist, not a whole-pawn multiple. Returning
    ///     the multiple would add vanilla's own colonist a second time per shard.
    /// </summary>
    [TestMethod]
    public void ContributorsReturnAGainNotAMultiple() {
        StringAssert.Contains(Roshar("RosharThreatContributor.cs"), "- 1f");
        StringAssert.Contains(Scadrial("ScadrialThreatContributor.cs"), "- 1f");
    }

    /// <summary>
    ///     Plate toggles in and out of combat and a Blade is ten heartbeats from a bare hand, so
    ///     pricing what is equipped would make a colony's threat flicker.
    /// </summary>
    [TestMethod]
    public void ShardsArePricedOnTheAbilityNotOnEquipment() {
        string source = Roshar("RosharThreatContributor.cs");

        StringAssert.Contains(source, "PawnHasShardblade", "the Blade check must defer to the ability's own rule");
        StringAssert.Contains(source, "Ability_ToggleShardplate");
        Assert.IsFalse(
            source.Contains("equipment.Primary"),
            "reading the equipped weapon reintroduces the flicker the ability check avoids"
        );
    }

    /// <summary>
    ///     ImplantSpike grants the gene, so the gene is already in the metal count. Charging a
    ///     per-spike gene bonus on top billed every spike twice.
    /// </summary>
    [TestMethod]
    public void ScadrialPricesASpikeOnceAsASpike() {
        string source = Scadrial("ScadrialThreatContributor.cs");

        StringAssert.Contains(source, "ScadrialThreat.ForSpikes");
        Assert.AreEqual(
            1,
            source.Split("ForSpikes").Length - 1,
            "a spike is priced in exactly one place"
        );
    }

    /// <summary>
    ///     The rule this replaced bailed on a null trait tracker, which dropped a gene-only pawn's
    ///     spikes and kit along with its metals.
    /// </summary>
    [TestMethod]
    public void ScadrialDoesNotGateEverythingOnTheTraitTracker() {
        string source = Scadrial("ScadrialThreatContributor.cs");

        Assert.IsFalse(
            source.Contains("story?.traits == null"),
            "gating the whole pawn on the trait tracker is the bug this replaced"
        );
    }

    /// <summary>
    ///     Core owns the registry, so it must not name a shard. A direct reference would make
    ///     unloading a shard mod a compile error in Core.
    /// </summary>
    [TestMethod]
    public void CoreRegistryNamesNoShard() {
        string source = Core("Core", "Threat", "ThreatContributorRegistry.cs");

        Assert.IsFalse(source.Contains("Cosmere.System."), "Core must not import a shard");
        Assert.IsFalse(source.Contains("Roshar"));
        Assert.IsFalse(source.Contains("Scadrial"));
    }

    /// <summary>
    ///     The injection scales the per-colonist value instead of adding at the return, which is
    ///     what keeps the bonus inside adaptation, threatScale, the days ramp and the clamp.
    /// </summary>
    [TestMethod]
    public void TheHookScalesRatherThanAdds() {
        string source = Core("Core", "Threat", "CosmereThreatPoints.cs");

        StringAssert.Contains(source, "points * ThreatContributorRegistry.MultipleForPawn");
        Assert.IsFalse(source.Contains("points +"), "adding here would reintroduce the clamp escape");
    }

    /// <summary>
    ///     The hook injects where vanilla writes a colonist's own points, so the bonus rides every
    ///     factor downstream of it. An At.Return injection would land outside the 10000 clamp.
    /// </summary>
    [TestMethod]
    public void TheHookInjectsAtTheColonistPointsLocal() {
        string source = Core("Core", "Threat", "CosmereStorytellerUtilityPatch.cs");

        StringAssert.Contains(source, "LocalAccess.Store");
        StringAssert.Contains(source, "At.Local");
        StringAssert.Contains(source, "[Local] Pawn pawn");
        Assert.IsFalse(source.Contains("At.Return"), "At.Return lands outside vanilla's clamp");
    }

    /// <summary>
    ///     Roshar's own storyteller patch is gone and Scadrial's keeps only the coppercloud, so
    ///     nothing bills a pawn outside the registry any more.
    /// </summary>
    [TestMethod]
    public void NoShardPricesPawnsOutsideTheRegistry() {
        string scadrial = Core("System", "Scadrial", "Patch", "World", "ScadrialStorytellerUtilityPatches.cs");

        string retired = Path.Combine(
            RepoRoot,
            "CosmereCore",
            "CosmereCore",
            "System",
            "Roshar",
            "Patch",
            "World",
            "RosharStorytellerUtilityPatches.cs"
        );
        Assert.IsFalse(File.Exists(retired), "the Roshar storyteller patch was replaced by the Core hook");
        StringAssert.Contains(scadrial, "Coppercloud");
        Assert.IsFalse(scadrial.Contains("FreeColonists"), "Scadrial must not price pawns here any more");
    }
}
