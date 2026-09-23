using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Guards the TenSoon reveal in Well of Ascension: that OreSeur is on the roster, that the
///     rename fires at the right point, and that it stays quiet when he never showed up.
/// </summary>
[TestClass]
public class OreSeurRevealTests {
    private static string RepoRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir);
            return dir.FullName;
        }
    }

    private static XDocument Scenario =>
        XDocument.Load(Path.Combine(RepoRoot, "CosmereScadrial", "Defs", "Scenarios", "WellOfAscension.xml"));

    private static XDocument Progression =>
        XDocument.Load(Path.Combine(RepoRoot, "CosmereScadrial", "Defs", "ScenarioProgression", "WellOfAscension.xml"));

    [TestMethod]
    public void OreSeurIsOnTheRoster() {
        List<string> names = Scenario.Descendants("firstName").Select(n => n.Value).ToList();
        Assert.IsTrue(names.Contains("OreSeur"), "OreSeur should start with Vin.");
    }

    /// <summary>
    ///     A kandra is not born and does not age. The biological age is the body he is wearing;
    ///     the chronological one is how long he has actually been around.
    /// </summary>
    [TestMethod]
    public void OreSeurIsAKandraAndOlderThanHeLooks() {
        XElement pawn = Scenario.Descendants("li")
            .First(li => li.Element("firstName")?.Value == "OreSeur");

        Assert.AreEqual("Cosmere_Scadrial_Xenotype_Kandra", pawn.Element("xenotype")?.Value);

        int age = int.Parse(pawn.Element("age")!.Value);
        int chronological = int.Parse(pawn.Element("chronologicalAge")!.Value);
        Assert.IsTrue(chronological > age, "A kandra should be far older than the body it wears.");
    }

    /// <summary>
    ///     Adding a pawn without raising the count leaves the builder short, and a zero or stale
    ///     total is what broke map generation last time.
    /// </summary>
    [TestMethod]
    public void TheBuilderCountMatchesTheRoster() {
        int roster = Scenario.Descendants("firstName").Count();

        XElement config = Scenario.Descendants("li")
            .First(li => (string?)li.Attribute("Class") == "ScenPart_ConfigPage_ConfigureStartingPawns_Xenotypes");

        Assert.AreEqual(roster, int.Parse(config.Element("pawnChoiceCount")!.Value));

        int total = config.Element("xenotypeCounts")!.Elements("li")
            .Sum(li => int.Parse(li.Element("count")!.Value));
        Assert.AreEqual(roster, total);
    }

    private static XElement RenameEvent {
        get {
            XElement? found = Progression.Descendants("li")
                .FirstOrDefault(li => li.Element("actions")?.Elements("li")
                    .Any(a => (string?)a.Attribute("Class") == "Cosmere.Core.ScenarioPart.Action.RenamePawnAction") == true);

            Assert.IsNotNull(found, "The TenSoon rename event is missing.");
            return found;
        }
    }

    [TestMethod]
    public void TheRenameTurnsOreSeurIntoTenSoon() {
        XElement action = RenameEvent.Element("actions")!.Elements("li")
            .First(a => (string?)a.Attribute("Class") == "Cosmere.Core.ScenarioPart.Action.RenamePawnAction");

        Assert.AreEqual("OreSeur", action.Element("pawnName")?.Value);
        Assert.AreEqual("TenSoon", action.Element("newFirstName")?.Value);
    }

    /// <summary>
    ///     A player who never recruited him, or who lost him, should not get a letter about a
    ///     pawn who is not there.
    /// </summary>
    [TestMethod]
    public void TheRenameOnlyFiresIfHeIsStillAlive() {
        XElement? alive = RenameEvent.Element("triggers")?.Elements("li")
            .FirstOrDefault(t => (string?)t.Attribute("Class") == "Cosmere.Core.ScenarioPart.Trigger.PawnAliveTrigger");

        Assert.IsNotNull(alive, "The rename should be gated on him being alive.");
        Assert.AreEqual("OreSeur", alive.Element("pawnName")?.Value);
        Assert.AreEqual("true", alive.Element("alive")?.Value);
    }

    /// <summary>
    ///     The reveal belongs late, after the koloss reach the walls and before the Well itself.
    ///     Firing it on day two would give away the book in the first week.
    /// </summary>
    [TestMethod]
    public void TheRevealLandsBetweenTheAssaultAndTheWell() {
        int day = int.Parse(
            RenameEvent.Element("triggers")!.Elements("li")
                .First(t => (string?)t.Attribute("Class") == "Cosmere.Core.ScenarioPart.Trigger.DaysPassedTrigger")
                .Element("days")!.Value
        );

        Assert.IsTrue(day > 60, $"The reveal fires on day {day}, before the koloss assault.");
        Assert.IsTrue(day < 80, $"The reveal fires on day {day}, at or after the Well.");
    }
}
