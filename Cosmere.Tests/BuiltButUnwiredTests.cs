using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>Four features that shipped fully built and were never called.</summary>
/// <remarks>The test host has no Verse or UnityEngine, so these read the source.</remarks>
[TestClass]
public class BuiltButUnwiredTests {
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

    [TestMethod]
    public void TheHighstormQueuesTheGemheartHunt() {
        string source = Core("System", "Roshar", "GameCondition", "Highstorm.cs");

        Assert.IsTrue(
            source.Contains("QueueHuntAfterHighstorm", StringComparison.Ordinal),
            "The hunt is queued off the back of a highstorm. With no caller the incident never fires."
        );
    }

    [TestMethod]
    public void MakingAKolossCanKillTheSubject() {
        string source = Core("System", "Scadrial", "Hemalurgy", "RecipeWorker", "MakeKoloss.cs");

        Assert.IsTrue(
            source.Contains("CheckSurgeryFail", StringComparison.Ordinal),
            "Without this the recipe's surgerySuccessChanceFactor and deathOnFailedSurgeryChance " +
            "are decoration and every operation succeeds."
        );
    }

    [TestMethod]
    public void TheSettingsWindowSearchesTheIndex() {
        string source = Core("Core", "Window", "SettingsWindow.cs");

        Assert.IsTrue(
            source.Contains("new SettingsSearchIndex", StringComparison.Ordinal),
            "The index was built and unit tested but nothing outside the tests ever constructed it."
        );
        Assert.IsTrue(
            source.Contains("searchIndex.Search", StringComparison.Ordinal),
            "The search field fed Widgets.TextField and nothing else."
        );
    }

    [TestMethod]
    public void TheCodexRemembersItsSystemAcrossASave() {
        string component = Core("Core", "UI", "Codex", "GameComponent_CodexMemory.cs");
        string tab = Core("Core", "Tab", "ITab_Investiture.cs");

        Assert.IsTrue(
            component.Contains("Scribe_Values.Look", StringComparison.Ordinal),
            "Nothing scribes the last system, so the codex reopens on the first one every load."
        );
        Assert.IsTrue(
            tab.Contains("GameComponent_CodexMemory.ApplyTo", StringComparison.Ordinal) &&
            tab.Contains("GameComponent_CodexMemory.RememberFrom", StringComparison.Ordinal),
            "The tab has to both read the remembered system and write the one being viewed."
        );
    }
}
