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

    private static string Core(params string[] parts) {
        return File.ReadAllText(Path.Combine([RepoRoot, "CosmereCore", "CosmereCore", .. parts]));
    }

    private static string Roshar(string file) {
        return Core("System", "Roshar", "Threat", file);
    }

    private static string Scadrial(string file) {
        return Core("System", "Scadrial", "Threat", file);
    }

    [TestMethod]
    public void BothShardsRegisterAContributor() {
        Assert.Contains(
            "ThreatContributorRegistry.Register",
            Roshar("RosharThreatRegistration.cs"),
            "Roshar never reaches the registry, so its threat is silently zero"
        );
        Assert.Contains(
            "ThreatContributorRegistry.Register",
            Scadrial("ScadrialThreatRegistration.cs"),
            "Scadrial never reaches the registry, so its threat is silently zero"
        );
    }

    /// <summary>
    ///     A contributor reports a gain over a plain colonist, not a whole-pawn multiple. Returning
    ///     the multiple would add vanilla's own colonist a second time per shard.
    /// </summary>
    [TestMethod]
    public void ContributorsReturnAGainNotAMultiple() {
        Assert.Contains("- 1f", Roshar("RosharThreatContributor.cs"));
        Assert.Contains("- 1f", Scadrial("ScadrialThreatContributor.cs"));
    }

    /// <summary>
    ///     Plate toggles in and out of combat and a Blade is ten heartbeats from a bare hand, so
    ///     pricing what is equipped would make a colony's threat flicker.
    /// </summary>
    [TestMethod]
    public void ShardsArePricedOnTheAbilityNotOnEquipment() {
        string source = Roshar("RosharThreatContributor.cs");

        Assert.Contains("PawnHasShardblade", source, "the Blade check must defer to the ability's own rule");
        Assert.Contains("Ability_ToggleShardplate", source);
        Assert.DoesNotContain(
            "equipment.Primary",
            source,
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

        Assert.Contains("ScadrialThreat.ForSpikes", source);
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

        Assert.DoesNotContain(
            "story?.traits == null",
            source,
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

        Assert.DoesNotContain("Cosmere.System.", source, "Core must not import a shard");
        Assert.DoesNotContain("Roshar", source);
        Assert.DoesNotContain("Scadrial", source);
    }

    /// <summary>
    ///     The injection scales the per-colonist value instead of adding at the return, which is
    ///     what keeps the bonus inside adaptation, threatScale, the days ramp and the clamp.
    /// </summary>
    [TestMethod]
    public void TheHookScalesRatherThanAdds() {
        string source = Core("Core", "Threat", "CosmereThreatPoints.cs");

        Assert.Contains("points * ThreatContributorRegistry.MultipleForPawn", source);
        Assert.DoesNotContain("points +", source, "adding here would reintroduce the clamp escape");
    }

    /// <summary>
    ///     The hook injects where vanilla writes a colonist's own points, so the bonus rides every
    ///     factor downstream of it. An At.Return injection would land outside the 10000 clamp.
    /// </summary>
    [TestMethod]
    public void TheHookInjectsAtTheColonistPointsLocal() {
        string source = Core("Core", "Threat", "CosmereStorytellerUtilityPatch.cs");

        Assert.Contains("LocalAccess.Store", source);
        Assert.Contains("At.Local", source);
        Assert.Contains("[Local] Pawn pawn", source);
        Assert.DoesNotContain("At.Return", source, "At.Return lands outside vanilla's clamp");
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
        Assert.Contains("Coppercloud", scadrial);
        Assert.DoesNotContain("FreeColonists", scadrial, "Scadrial must not price pawns here any more");
    }
}
