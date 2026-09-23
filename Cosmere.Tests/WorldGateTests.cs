using System;
using System.IO;
using Cosmere.Core.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>Guards W23: a scenario incident from one world must not fire on another.</summary>
[TestClass]
public class WorldGateTests {
    [TestMethod]
    public void TwoKnownWorldsThatDisagreeFail() {
        Assert.IsFalse(WorldGate.Matches("Cosmere_World_Roshar", "Cosmere_World_Scadrial", false));
    }

    [TestMethod]
    public void TheSameWorldPasses() {
        Assert.IsTrue(WorldGate.Matches("Cosmere_World_Roshar", "Cosmere_World_Roshar", false));
    }

    [TestMethod]
    public void ContentNamingNoWorldBelongsToEveryWorld() {
        Assert.IsTrue(WorldGate.Matches(null, "Cosmere_World_Scadrial", false));
        Assert.IsTrue(WorldGate.Matches(string.Empty, "Cosmere_World_Scadrial", false));
    }

    [TestMethod]
    public void ACrossWorldSaveReachesEveryWorld() {
        Assert.IsTrue(WorldGate.Matches("Cosmere_World_Roshar", "Cosmere_World_Scadrial", true));
    }

    [TestMethod]
    public void ASaveWithNoWorldYetTurnsNothingAway() {
        Assert.IsTrue(WorldGate.Matches("Cosmere_World_Roshar", null, false));
        Assert.IsTrue(WorldGate.Matches("Cosmere_World_Roshar", string.Empty, false));
    }

    [TestMethod]
    public void TheScenarioIncidentActionRunsTheGate() {
        DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
            dir = dir.Parent;
        }

        Assert.IsNotNull(dir, "Could not locate CosmereScadrial/Defs above the test output directory.");
        string source = File.ReadAllText(
            Path.Combine(
                dir.FullName,
                "CosmereCore",
                "CosmereCore",
                "Core",
                "ScenarioPart",
                "Action",
                "TriggerIncidentAction.cs"
            )
        );

        Assert.IsTrue(
            source.Contains("WorldGate.Matches", StringComparison.Ordinal),
            "Without the gate the action fires any named incident, so a Scadrial save can roll a Roshar one."
        );
    }
}
