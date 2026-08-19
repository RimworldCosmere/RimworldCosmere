using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

[TestClass]
public class PawnWorldConnectionPresentationTests {
    [TestMethod]
    public void HomeworldButtonJoinsTheXenotypeStack() {
        string patch = Core("Core", "Patch", "UI", "CharacterCardWorldConnectionPatch.cs");

        Assert.IsTrue(patch.Contains("<DoTopStack>b__8", StringComparison.Ordinal));
        Assert.IsTrue(patch.Contains("AddHomeworldElement", StringComparison.Ordinal));
        Assert.IsTrue(patch.Contains("tmpStackElements", StringComparison.Ordinal));
    }

    [TestMethod]
    public void BioCardDoesNotDrawAConnectionSection() {
        string patch = Core("Core", "Patch", "UI", "CharacterCardWorldConnectionPatch.cs");
        string source = Core("Core", "Patch", "UI", "CharacterCardWorldConnection.cs");

        Assert.IsFalse(patch.Contains("DoLeftSection", StringComparison.Ordinal));
        Assert.IsFalse(patch.Contains("AppendBackstoryDrawer", StringComparison.Ordinal));
        Assert.IsFalse(source.Contains("DrawConnectionRow", StringComparison.Ordinal));
        Assert.IsFalse(source.Contains("CardHeightFor", StringComparison.Ordinal));
    }

    [TestMethod]
    public void CodexConnectionHasShardAndPlanetTabs() {
        string source = Core("Core", "UI", "Codex", "ConnectionSubtab.cs");
        string state = Core("Core", "UI", "Codex", "CodexState.cs");
        string language = File.ReadAllText(Path.Combine(RepoRoot, "CosmereCore", "Languages", "English", "Keyed", "Inspector.xml"));

        Assert.IsTrue(source.Contains("SubtabBar.Draw", StringComparison.Ordinal));
        Assert.IsTrue(source.Contains("DrawPlanets", StringComparison.Ordinal));
        Assert.IsTrue(state.Contains("ConnectionCodexPage", StringComparison.Ordinal));
        Assert.IsTrue(language.Contains("<CC_Connection_Tab_Shards>Shards</CC_Connection_Tab_Shards>", StringComparison.Ordinal));
        Assert.IsTrue(language.Contains("<CC_Connection_Tab_Planets>Planets</CC_Connection_Tab_Planets>", StringComparison.Ordinal));
    }

    [TestMethod]
    public void PawnInfoCardGetsWorldAndShardConnectionEntries() {
        string comp = Core("Core", "Comp", "Thing", "PawnConnectionStats.cs");
        string xml = File.ReadAllText(Path.Combine(RepoRoot, "CosmereCore", "Patches", "DormantConnectTracker.xml"));

        Assert.IsTrue(comp.Contains("override IEnumerable<StatDrawEntry> SpecialDisplayStats()", StringComparison.Ordinal));
        Assert.IsTrue(comp.Contains("WorldConnectionUtility.ActiveFor", StringComparison.Ordinal));
        Assert.IsTrue(comp.Contains("ConnectionUtility.BreakdownFor", StringComparison.Ordinal));
        Assert.IsTrue(xml.Contains("Cosmere.Core.Comp.Thing.PawnConnectionStats", StringComparison.Ordinal));
    }

    [TestMethod]
    public void WorldConnectionDisplaysUseTheFullConnectionScale() {
        string model = Core("Core", "ShardConnection", "WorldConnection.cs");
        string codex = Core("Core", "UI", "Codex", "ConnectionSubtab.cs");
        string bio = Core("Core", "Patch", "UI", "CharacterCardWorldConnection.cs");
        string info = Core("Core", "Comp", "Thing", "PawnConnectionStats.cs");

        Assert.IsTrue(model.Contains("ConnectionMath.ComposeWorld", StringComparison.Ordinal));
        Assert.IsFalse(model.Contains("ToConnectionScale", StringComparison.Ordinal));
        Assert.IsTrue(codex.Contains("ConnectionMath.Max", StringComparison.Ordinal));
        Assert.IsTrue(bio.Contains("ConnectionMath.Max.Named(\"MAX\")", StringComparison.Ordinal));
        Assert.IsTrue(info.Contains("ConnectionMath.Max.Named(\"MAX\")", StringComparison.Ordinal));
    }

    [TestMethod]
    public void PlanetConnectionTracksEveryIndependentSource() {
        string model = Core("Core", "ShardConnection", "WorldConnection.cs");
        string utility = Core("Core", "ShardConnection", "WorldConnectionUtility.cs");

        Assert.IsTrue(model.Contains("int investiture, int earned", StringComparison.Ordinal));
        Assert.IsTrue(model.Contains("ConnectionMath.Compose", StringComparison.Ordinal));
        Assert.IsTrue(utility.Contains("ConnectionInvestitureRegistry.StrengthFor", StringComparison.Ordinal));
        Assert.IsTrue(utility.Contains("EarnedFor(world)", StringComparison.Ordinal));
        Assert.IsTrue(utility.Contains("PawnConnectionStats", StringComparison.Ordinal));
        Assert.IsTrue(utility.Contains("public static void Grant", StringComparison.Ordinal));
    }

    [TestMethod]
    public void PlanetConnectionUiShowsAnAdditiveBreakdown() {
        string codex = Core("Core", "UI", "Codex", "ConnectionSubtab.cs");
        string bio = Core("Core", "Patch", "UI", "CharacterCardWorldConnection.cs");
        string info = Core("Core", "Comp", "Thing", "PawnConnectionStats.cs");
        string language = File.ReadAllText(
            Path.Combine(RepoRoot, "CosmereCore", "Languages", "English", "Keyed", "Inspector.xml")
        );

        Assert.IsTrue(codex.Contains("connection.Investiture", StringComparison.Ordinal));
        Assert.IsTrue(codex.Contains("connection.Earned", StringComparison.Ordinal));
        Assert.IsTrue(bio.Contains("entry.Investiture.Named(\"INVESTITURE\")", StringComparison.Ordinal));
        Assert.IsTrue(info.Contains("entry.Earned.Named(\"EARNED\")", StringComparison.Ordinal));
        Assert.IsTrue(language.Contains("<CC_PlanetConnection_Investiture>", StringComparison.Ordinal));
        Assert.IsTrue(language.Contains("<CC_PlanetConnection_Earned>", StringComparison.Ordinal));
        Assert.IsFalse(language.Contains("Ancestry and residence overlap", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ProductionCodeUsesWorldConnectionNames() {
        string core = Path.Combine(RepoRoot, "CosmereCore", "CosmereCore");
        string[] files = Directory.GetFiles(core, "*.cs", SearchOption.AllDirectories);
        string retiredTerm = string.Concat("Residen", "cy");

        for (int i = 0; i < files.Length; i++) {
            if (files[i].Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)) continue;
            Assert.IsFalse(Path.GetFileName(files[i]).Contains(retiredTerm, StringComparison.Ordinal), files[i]);
            Assert.IsFalse(File.ReadAllText(files[i]).Contains(retiredTerm, StringComparison.Ordinal), files[i]);
        }
    }

    private static string Core(params string[] parts) {
        string[] path = new string[parts.Length + 3];
        path[0] = RepoRoot;
        path[1] = "CosmereCore";
        path[2] = "CosmereCore";
        Array.Copy(parts, 0, path, 3, parts.Length);
        return File.ReadAllText(Path.Combine(path));
    }

    private static string RepoRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate the repo root above the test output directory.");
            return dir!.FullName;
        }
    }
}
