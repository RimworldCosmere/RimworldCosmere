using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The army the scenario already promised, and the several ways RimWorld quietly refuses to
///     field it.
/// </summary>
[TestClass]
public class KolossArmyTests {
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

    private static XDocument Defs(params string[] parts) => XDocument.Load(Path.Combine(
        [RepoRoot, "CosmereScadrial", "Defs", .. parts]
    ));

    private static List<XElement> KolossMakers => Defs("Factions", "GreatHouses.xml")
        .Descendants("li")
        .Where(li => li.Element("options")?.Elements()
            .Any(o => o.Name.LocalName.StartsWith("Cosmere_Scadrial_PawnKind_Koloss")) == true)
        .ToList();

    /// <summary>
    ///     The beat is called koloss_march and the opening text promises koloss from the east, and
    ///     it sent two armies of men and nothing else.
    /// </summary>
    [TestMethod]
    public void TheKolossMarchBringsKoloss() {
        XElement beat = Defs("ScenarioProgression", "WellOfAscension.xml").Descendants("li")
            .First(li => li.Element("key")?.Value == "koloss_march");

        List<XElement> raids = beat.Element("actions")!.Elements("li")
            .Where(li => li.Attribute("Class")?.Value.EndsWith("RaidAction") == true)
            .ToList();

        Assert.AreEqual(3, raids.Count, "Straff from the north, Cett from the south, koloss from the east.");

        XElement east = raids.First(r => r.Element("edge")?.Value == "East");
        Assert.AreEqual("Cosmere_Scadrial_Faction_HouseLekal", east.Element("faction")?.Value);
    }

    /// <summary>
    ///     ChoosePawnGenOptionsByPoints weights each option through
    ///     PawnWeightFactorByMostExpensivePawnCostFractionCurve, which returns 0.01 below a fifth of
    ///     the priciest kind in the list. A 300-power koloss sharing a list with a 60-power pirate
    ///     erases the pirate at a hundred to one.
    /// </summary>
    [TestMethod]
    public void KolossNeverShareAGroupMakerWithCheaperKinds() {
        Assert.AreEqual(2, KolossMakers.Count, "One for Ruin, one for Lekal.");

        foreach (XElement maker in KolossMakers) {
            List<string> kinds = maker.Element("options")!.Elements().Select(o => o.Name.LocalName).ToList();

            Assert.IsTrue(
                kinds.All(k => k.StartsWith("Cosmere_Scadrial_PawnKind_Koloss")),
                "A koloss maker holds koloss and nothing else."
            );
        }
    }

    /// <summary>
    ///     commonality defaults to 100, so a maker set to 1 beside a default one fires once in a
    ///     hundred and one - which reads as the feature not working rather than as a weight.
    /// </summary>
    [TestMethod]
    public void AKolossRaidIsAsLikelyAsAnyOther() {
        foreach (XElement maker in KolossMakers) {
            Assert.AreEqual(
                100,
                int.Parse(maker.Element("commonality")!.Value),
                "Stated, and equal to the default every other maker takes."
            );
        }
    }

    /// <summary>Koloss do not operate mortars.</summary>
    [TestMethod]
    public void KolossDoNotLaySiege() {
        foreach (XElement maker in KolossMakers) {
            CollectionAssert.Contains(
                maker.Element("disallowedStrategies")!.Elements("li").Select(li => li.Value).ToList(),
                "Siege"
            );
        }
    }

    /// <summary>
    ///     The NPC base caps one pawn at 150 even at 1300 raid points, so an overgrown koloss could
    ///     never be selected and the maker silently returned mercenaries instead.
    /// </summary>
    [TestMethod]
    public void ARaidCanAffordTheBiggestKoloss() {
        int dearest = Defs("Races", "PawnKinds.xml").Descendants("PawnKindDef")
            .Where(d => d.Element("defName")?.Value.StartsWith("Cosmere_Scadrial_PawnKind_Koloss") == true)
            .Select(d => int.Parse(d.Element("combatPower")?.Value ?? "0"))
            .Max();

        foreach (string faction in new[] { "Cosmere_Scadrial_Faction_Ruin", "Cosmere_Scadrial_Faction_HouseLekal" }) {
            XElement def = Defs("Factions", "GreatHouses.xml").Descendants("FactionDef")
                .First(d => d.Element("defName")?.Value == faction);

            XElement curve = def.Element("maxPawnCostPerTotalPointsCurve")!;
            string ceiling = curve.Element("points")!.Elements("li")
                .Select(li => li.Value)
                .First(v => v.Contains("1300"));

            int cap = int.Parse(ceiling.Trim('(', ')').Split(',')[1].Trim());
            Assert.IsTrue(cap > dearest, $"{faction} caps a pawn at {cap}, under the {dearest} koloss.");
        }
    }

    /// <summary>
    ///     RaidAgeRestrictionWorker.ShouldApplyToKind is true for anything Humanlike, so child
    ///     koloss roll at two and a half percent from day fifteen without this.
    /// </summary>
    [TestMethod]
    public void NobodyFieldsChildKoloss() {
        foreach (string faction in new[] { "Cosmere_Scadrial_Faction_Ruin", "Cosmere_Scadrial_Faction_HouseLekal" }) {
            XElement def = Defs("Factions", "GreatHouses.xml").Descendants("FactionDef")
                .First(d => d.Element("defName")?.Value == faction);

            CollectionAssert.Contains(
                def.Element("disallowedRaidAgeRestrictions")!.Elements("li").Select(li => li.Value).ToList(),
                "Children",
                $"{faction} can field child koloss."
            );
        }
    }

    /// <summary>
    ///     Ruin had no commonality curve, so from the moment a scenario created it, it raided at
    ///     full weight at every point level.
    /// </summary>
    [TestMethod]
    public void RuinDoesNotRaidAFledglingColony() {
        XElement ruin = Defs("Factions", "GreatHouses.xml").Descendants("FactionDef")
            .First(d => d.Element("defName")?.Value == "Cosmere_Scadrial_Faction_Ruin");

        List<string> points = ruin.Element("raidCommonalityFromPointsCurve")!
            .Element("points")!.Elements("li").Select(li => li.Value).ToList();

        Assert.IsTrue(points[0].Contains("(0, 0)") || points[0].Contains("(0,0)"), "Nothing at zero points.");
    }

    /// <summary>
    ///     Omitting weaponTags makes them arrive unarmed - PawnWeaponGenerator returns immediately
    ///     on an empty list. Their own tag also keeps them out of the industrial pools and gives
    ///     the sword a death drop.
    /// </summary>
    [TestMethod]
    public void AKolossArrivesCarryingSomething() {
        XElement kind = Defs("Races", "PawnKinds.xml").Descendants("PawnKindDef")
            .First(d => d.Element("defName")?.Value == "Cosmere_Scadrial_PawnKind_Koloss");

        CollectionAssert.Contains(
            kind.Element("weaponTags")!.Elements("li").Select(li => li.Value).ToList(),
            "Cosmere_Scadrial_KolossWeapon"
        );

        XElement sword = Defs("Things", "Weapons", "KolossSword.xml").Descendants("ThingDef")
            .First(d => d.Element("defName")?.Value == "Cosmere_Scadrial_Thing_KolossSword");

        CollectionAssert.Contains(
            sword.Element("weaponTags")!.Elements("li").Select(li => li.Value).ToList(),
            "Cosmere_Scadrial_KolossWeapon",
            "The tag has to match, or the kind generates unarmed."
        );
    }

    /// <summary>
    ///     A raid of newly spiked koloss reads as a raid of large men. The kind says how long
    ///     somebody has had them, because PawnKindDef has no hook for it and the growth hediff's
    ///     initialSeverity is one number for every koloss alive.
    /// </summary>
    [TestMethod]
    public void RaidKolossHaveBeenSomebodysForYears() {
        List<XElement> tiers = Defs("Races", "PawnKinds.xml").Descendants("PawnKindDef")
            .Where(d => d.Element("modExtensions") != null
                        && d.Element("defName")!.Value.StartsWith("Cosmere_Scadrial_PawnKind_Koloss"))
            .ToList();

        Assert.AreEqual(3, tiers.Count, "Young, grown, overgrown.");

        int power = 0;
        foreach (XElement tier in tiers) {
            int next = int.Parse(tier.Element("combatPower")!.Value);
            Assert.IsTrue(next > power, "An older koloss has to cost a raid more.");
            power = next;
        }
    }
}
