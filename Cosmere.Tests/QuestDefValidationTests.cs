using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Cosmere.Core.Quest;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Reads quest def XML directly and checks what RimWorld only complains about at load
///     time, or never: unresolved translation keys, dangling faction/era/metal references,
///     and stage orderings that QuestBuildContext only discovers when an incident fires.
/// </summary>
[TestClass]
public class QuestDefValidationTests {
    private static readonly string[] SiteDependentObjectiveTypes = [
        "ClearSiteObjective", "TimedWorkObjective", "SpawnThingObjective",
    ];

    private static string RepoRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs", "Quests"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate CosmereScadrial/Defs/Quests above the test output directory.");
            return dir!.FullName;
        }
    }

    private static string QuestsDirectory => Path.Combine(RepoRoot, "CosmereScadrial", "Defs", "Quests");

    private static List<(string file, XElement def)> QuestDefs() {
        List<(string file, XElement def)> defs = new List<(string file, XElement def)>();
        if (!Directory.Exists(QuestsDirectory)) return defs;

        foreach (string path in Directory.GetFiles(QuestsDirectory, "*.xml")) {
            XDocument doc = XDocument.Load(path);
            XElement? root = doc.Root;
            Assert.IsNotNull(root, $"{Path.GetFileName(path)}: file has no root element.");
            foreach (XElement def in root!.Elements()) {
                defs.Add((path, def));
            }
        }

        return defs;
    }

    private static string RequireDefName(string filePath, XElement def) {
        XElement? name = def.Element("defName");
        Assert.IsNotNull(name, $"{Path.GetFileName(filePath)}: a quest def is missing its defName element.");
        return name!.Value;
    }

    private static bool HasClass(XElement element, string typeSuffix) {
        string? cls = element.Attribute("Class")?.Value;
        return cls != null && cls.EndsWith("." + typeSuffix, StringComparison.Ordinal);
    }

    private static HashSet<string> DefNamesUnder(params string[] relativePathParts) {
        HashSet<string> names = new HashSet<string>();
        string dir = Path.Combine(RepoRoot, Path.Combine(relativePathParts));
        if (!Directory.Exists(dir)) return names;

        foreach (string path in Directory.GetFiles(dir, "*.xml")) {
            foreach (XElement name in XDocument.Load(path).Descendants("defName")) {
                names.Add(name.Value);
            }
        }

        return names;
    }

    private static string ScenarioProgressionDirectory => Path.Combine(RepoRoot, "CosmereScadrial", "Defs", "ScenarioProgression");

    /// <summary>Every def of one XML element name across all three mods' Defs trees.</summary>
    private static List<(string file, XElement def)> DefsOfType(string elementName) {
        List<(string file, XElement def)> defs = new List<(string file, XElement def)>();
        foreach (string mod in new[] { "CosmereCore", "CosmereScadrial", "CosmereRoshar" }) {
            string dir = Path.Combine(RepoRoot, mod, "Defs");
            if (!Directory.Exists(dir)) continue;

            foreach (string path in Directory.GetFiles(dir, "*.xml", SearchOption.AllDirectories)) {
                XElement? root = XDocument.Load(path).Root;
                if (root == null) continue;
                foreach (XElement def in root.Elements(elementName)) {
                    defs.Add((path, def));
                }
            }
        }

        return defs;
    }

    private static HashSet<string> OurSitePartDefNames() {
        HashSet<string> names = new HashSet<string>();
        foreach ((string _, XElement def) in DefsOfType("SitePartDef")) {
            XElement? name = def.Element("defName");
            if (name != null) names.Add(name.Value);
        }

        return names;
    }

    private static List<(string file, XElement def)> ScenariosForEra(string era) {
        List<(string file, XElement def)> matches = new List<(string file, XElement def)>();
        foreach ((string file, XElement def) in DefsOfType("ScenarioDef")) {
            foreach (XElement extension in def.Descendants("li")) {
                if (!HasClass(extension, "ScenarioEra")) continue;
                if (extension.Element("era")?.Value == era) {
                    matches.Add((file, def));
                    break;
                }
            }
        }

        return matches;
    }

    /// <summary>
    ///     The player faction plus everything the scenario's ScenPart_FactionRelations names -
    ///     that part creates any faction world generation did not already roll.
    /// </summary>
    private static HashSet<string> FactionsCreatedBy(XElement scenario) {
        HashSet<string> factions = new HashSet<string>();

        XElement? playerFaction = scenario.Descendants("factionDef").FirstOrDefault();
        if (playerFaction != null) factions.Add(playerFaction.Value);

        foreach (XElement part in scenario.Descendants("li")) {
            if (!HasClass(part, "ScenPart_FactionRelations")) continue;

            XElement? relations = part.Element("relations");
            if (relations == null) continue;
            foreach (XElement entry in relations.Elements()) {
                factions.Add(entry.Name.LocalName);
            }
        }

        return factions;
    }

    private static HashSet<string> CapstonesReferencedByStartQuestAction() {
        HashSet<string> referenced = new HashSet<string>();
        if (!Directory.Exists(ScenarioProgressionDirectory)) return referenced;

        foreach (string path in Directory.GetFiles(ScenarioProgressionDirectory, "*.xml")) {
            foreach (XElement action in XDocument.Load(path).Descendants("li")) {
                if (!HasClass(action, "StartQuestAction")) continue;

                XElement? questDef = action.Element("questDef");
                if (questDef != null) referenced.Add(questDef.Value);
            }
        }

        return referenced;
    }

    private static HashSet<string> KnownTranslationKeys() {
        HashSet<string> keys = new HashSet<string>();
        foreach (string mod in new[] { "CosmereCore", "CosmereScadrial" }) {
            string dir = Path.Combine(RepoRoot, mod, "Languages", "English", "Keyed");
            if (!Directory.Exists(dir)) continue;

            foreach (string path in Directory.GetFiles(dir, "*.xml")) {
                XElement? root = XDocument.Load(path).Root;
                if (root == null) continue;
                foreach (XElement entry in root.Elements()) {
                    keys.Add(entry.Name.LocalName);
                }
            }
        }

        return keys;
    }

    /// <summary>
    ///     Guards every other test in this class: if the quest directory resolves to a path
    ///     with nothing in it, every def-scanning assertion below would silently iterate zero
    ///     times and report green.
    /// </summary>
    [TestMethod]
    public void AtLeastOneQuestDefExists() {
        List<(string file, XElement def)> defs = QuestDefs();
        Assert.IsTrue(defs.Count > 0, $"No quest defs found under '{QuestsDirectory}'.");
    }

    [TestMethod]
    public void EveryLabelKeyAndTipKeyResolves() {
        HashSet<string> keys = KnownTranslationKeys();
        foreach ((string file, XElement def) in QuestDefs()) {
            string name = RequireDefName(file, def);

            foreach (XElement optionsElement in def.Descendants("options")) {
                foreach (XElement option in optionsElement.Elements("li")) {
                    XElement? labelKey = option.Element("labelKey");
                    Assert.IsNotNull(labelKey, $"{name}: a choice option has no labelKey.");
                    Assert.IsTrue(
                        keys.Contains(labelKey!.Value),
                        $"{name}: labelKey '{labelKey.Value}' has no matching entry in any " +
                        "Languages/English/Keyed folder. The player would see the raw key."
                    );

                    XElement? tipKey = option.Element("tipKey");
                    Assert.IsNotNull(tipKey, $"{name}: a choice option has no tipKey.");
                    Assert.IsTrue(
                        keys.Contains(tipKey!.Value),
                        $"{name}: tipKey '{tipKey.Value}' has no matching entry in any " +
                        "Languages/English/Keyed folder. The player would see the raw key."
                    );
                }
            }
        }
    }

    [TestMethod]
    public void EveryEraReferenceResolves() {
        HashSet<string> eras = DefNamesUnder("CosmereScadrial", "Defs", "Eras");
        foreach ((string file, XElement def) in QuestDefs()) {
            string name = RequireDefName(file, def);

            XElement? erasElement = def.Element("eras");
            if (erasElement == null) continue;

            foreach (XElement era in erasElement.Elements("li")) {
                Assert.IsTrue(
                    eras.Contains(era.Value),
                    $"{name}: era '{era.Value}' does not resolve to a known EraDef."
                );
            }
        }
    }

    /// <summary>
    ///     The Core scenarios are shard-agnostic and are what every quickstart falls back to
    ///     (AbstractQuickstart.scenario defaults to Crashlanded). If one carries no ScenarioEra,
    ///     FindActiveEra returns null, EraMatches rejects every era-gated quest, and the whole
    ///     quest system goes silently unreachable with nothing logged. Shard scenarios are
    ///     deliberately exempt - Roshar has no eras yet and should not offer Scadrial quests.
    /// </summary>
    [TestMethod]
    public void EveryCoreScenarioDeclaresAnEra() {
        string dir = Path.Combine(RepoRoot, "CosmereCore", "Defs", "Scenarios");
        Assert.IsTrue(Directory.Exists(dir), $"Core scenario directory '{dir}' does not exist.");

        string[] files = Directory.GetFiles(dir, "*.xml");
        Assert.IsTrue(files.Length > 0, $"No scenario defs found under '{dir}'.");

        foreach (string path in files) {
            XElement? root = XDocument.Load(path).Root;
            if (root == null) continue;

            foreach (XElement def in root.Elements("ScenarioDef")) {
                string name = def.Element("defName")?.Value ?? Path.GetFileName(path);

                bool hasEra = false;
                foreach (XElement li in def.Descendants("li")) {
                    if ((string?)li.Attribute("Class") == "Cosmere.Core.DefModExtension.ScenarioEra") {
                        hasEra = true;
                        break;
                    }
                }

                Assert.IsTrue(
                    hasEra,
                    $"{name}: Core scenario declares no ScenarioEra. Every era-gated quest would " +
                    "be filtered out of this scenario with no log line."
                );
            }
        }
    }

    /// <summary>
    ///     Site.Label falls back to MainSitePartDef.label, so a TravelToSiteObjective without a
    ///     siteLabelKey puts "manhunter pack" or "outpost" on the world map instead of the place
    ///     the quest is about.
    /// </summary>
    [TestMethod]
    public void EveryTravelObjectiveNamesItsSite() {
        HashSet<string> keys = KnownTranslationKeys();
        foreach ((string file, XElement def) in QuestDefs()) {
            string name = RequireDefName(file, def);

            foreach (XElement objective in def.Descendants("objective")) {
                string? cls = (string?)objective.Attribute("Class");
                if (cls != "Cosmere.Core.Quest.Objective.TravelToSiteObjective") continue;

                XElement? labelKey = objective.Element("siteLabelKey");
                Assert.IsNotNull(
                    labelKey,
                    $"{name}: a TravelToSiteObjective has no siteLabelKey, so the world map would " +
                    "show the raw SitePartDef label."
                );

                Assert.IsTrue(
                    keys.Contains(labelKey!.Value),
                    $"{name}: siteLabelKey '{labelKey.Value}' has no matching entry in any " +
                    "Languages/English/Keyed folder."
                );
            }
        }
    }

    /// <summary>
    ///     Every base-building site part - Outpost and its relatives - reads RectOfInterest and
    ///     then deliberately walls its settlement into a rect *beside* it, so the ground the
    ///     quest is about generates empty. Extra site parts have to be ones we ship, which put
    ///     their pawns on the rect instead.
    /// </summary>
    [TestMethod]
    public void EveryExtraSitePartIsOneWeShip() {
        HashSet<string> ours = OurSitePartDefNames();
        foreach ((string file, XElement def) in QuestDefs()) {
            string name = RequireDefName(file, def);

            foreach (XElement objective in def.Descendants("objective")) {
                XElement? extras = objective.Element("extraSiteParts");
                if (extras == null) continue;

                foreach (XElement entry in extras.Elements("li")) {
                    Assert.IsTrue(
                        ours.Contains(entry.Value),
                        $"{name}: extraSiteParts names '{entry.Value}', which is not a SitePartDef we " +
                        "define. Vanilla base-building parts generate a fort next to the site's rect " +
                        "of interest, not defenders on it."
                    );
                }
            }
        }
    }

    /// <summary>
    ///     GenStep_PreciousLump sets RectOfInterest to the seam's bounds in ScatterAt, at order
    ///     900. A garrison GenStep ordered before that reads an unset var and falls back to the
    ///     map centre, which is how vanilla's Outpost ends up nowhere near the resource.
    /// </summary>
    [TestMethod]
    public void EverySiteGarrisonGenStepRunsAfterThePreciousLump() {
        const int preciousLumpOrder = 900;
        int found = 0;

        foreach ((string file, XElement def) in DefsOfType("GenStepDef")) {
            XElement? genStep = def.Element("genStep");
            string? cls = (string?)genStep?.Attribute("Class");
            if (cls != "Cosmere.Core.Quest.GenStep_SiteGarrison") continue;

            found++;
            string name = RequireDefName(file, def);

            XElement? order = def.Element("order");
            Assert.IsNotNull(order, $"{name}: a GenStep_SiteGarrison def declares no order.");
            Assert.IsTrue(
                int.TryParse(order!.Value, out int value) && value > preciousLumpOrder,
                $"{name}: order '{order.Value}' must be above {preciousLumpOrder}, or RectOfInterest " +
                "is still unset when the garrison spawns."
            );

            XElement? link = def.Element("linkWithSite");
            Assert.IsNotNull(link, $"{name}: a GenStep_SiteGarrison def has no linkWithSite, so it never runs.");
        }

        Assert.IsTrue(found > 0, "No GenStepDef wires up Cosmere.Core.Quest.GenStep_SiteGarrison.");
    }

    /// <summary>
    ///     A quest's targetFaction has to exist in the world the quest can fire in, or the site
    ///     generates with no owner and the garrison has nobody to draw from. Only
    ///     ScenPart_FactionRelations creates a faction that world generation did not roll.
    /// </summary>
    [TestMethod]
    public void EveryTargetFactionExistsInEveryScenarioTheQuestCanFireIn() {
        foreach ((string file, XElement def) in QuestDefs()) {
            string name = RequireDefName(file, def);

            string? faction = def.Element("targetFaction")?.Value;
            if (faction == null) continue;

            XElement? eras = def.Element("eras");
            if (eras == null) continue;

            foreach (XElement era in eras.Elements("li")) {
                foreach ((string scenarioFile, XElement scenario) in ScenariosForEra(era.Value)) {
                    HashSet<string> present = FactionsCreatedBy(scenario);
                    if (present.Contains(faction)) continue;

                    // The player-faction and NPC-faction defs for one power differ only by an
                    // "NPC" suffix, and a scenario where you play that power has no business
                    // spawning a rival copy of yourself.
                    if (faction.EndsWith("NPC", StringComparison.Ordinal)
                        && present.Contains(faction.Substring(0, faction.Length - 3))) {
                        continue;
                    }

                    Assert.Fail(
                        $"{name}: targetFaction '{faction}' is not created by " +
                        $"{RequireDefName(scenarioFile, scenario)}, which shares era '{era.Value}'. " +
                        "Add it to that scenario's ScenPart_FactionRelations."
                    );
                }
            }
        }
    }

    /// <summary>
    ///     A stage or reward tagged for a branch that no option declares never runs, and
    ///     nothing at runtime complains - the payout just silently never arrives.
    /// </summary>
    [TestMethod]
    public void EveryAfterChoiceTagNamesARealOption() {
        foreach ((string file, XElement def) in QuestDefs()) {
            string name = RequireDefName(file, def);

            HashSet<string> optionKeys = new HashSet<string>();
            foreach (XElement objective in def.Descendants("objective")) {
                if ((string?)objective.Attribute("Class") != "Cosmere.Core.Quest.Objective.ChoiceObjective") {
                    continue;
                }

                XElement? options = objective.Element("options");
                if (options == null) continue;
                foreach (XElement option in options.Elements("li")) {
                    XElement? key = option.Element("key");
                    if (key != null) optionKeys.Add(key.Value);
                }
            }

            foreach (XElement tag in def.Descendants("afterChoice")) {
                Assert.IsTrue(
                    optionKeys.Contains(tag.Value),
                    $"{name}: afterChoice '{tag.Value}' matches no ChoiceObjective option key. " +
                    "That stage or reward would never run."
                );
            }
        }
    }

    [TestMethod]
    public void EveryStageHasADescriptionKeyThatResolves() {
        HashSet<string> keys = KnownTranslationKeys();
        foreach ((string file, XElement def) in QuestDefs()) {
            string name = RequireDefName(file, def);

            XElement? stages = def.Element("stages");
            if (stages == null) continue;

            foreach (XElement stage in stages.Elements("li")) {
                string stageKey = stage.Element("key")?.Value ?? "(unnamed)";

                XElement? descriptionKey = stage.Element("descriptionKey");
                Assert.IsNotNull(
                    descriptionKey,
                    $"{name}: stage '{stageKey}' has no descriptionKey, so the Quests tab " +
                    "would show the player nothing about what to do."
                );

                Assert.IsTrue(
                    keys.Contains(descriptionKey!.Value),
                    $"{name}: stage '{stageKey}' descriptionKey '{descriptionKey.Value}' has no " +
                    "matching entry in any Languages/English/Keyed folder. The player would see " +
                    "the raw key."
                );
            }
        }
    }

    [TestMethod]
    public void OnlyCapstonesMayWaiveTheAcceptDeadline() {
        foreach ((string file, XElement def) in QuestDefs()) {
            string name = RequireDefName(file, def);

            XElement? expire = def.Element("expireAfterDays");
            if (expire == null) continue;

            if (!int.TryParse(expire.Value, out int days)) {
                Assert.Fail($"{name}: expireAfterDays '{expire.Value}' is not an integer.");
                continue;
            }

            if (days > 0) continue;

            // Only a quest the player is asked to accept can miss an acceptance deadline.
            // Capstones and Threats are both handed to the player rather than offered.
            string? kind = def.Element("kind")?.Value;
            Assert.IsTrue(
                kind == "Capstone" || kind == "Threat",
                $"{name}: expireAfterDays {days} waives the accept deadline, but a {kind ?? "Repeatable"} " +
                "quest is offered, so it needs a window to accept it in."
            );
        }
    }

    [TestMethod]
    public void EveryFactionReferenceResolves() {
        HashSet<string> factions = DefNamesUnder("CosmereScadrial", "Defs", "Factions");
        foreach ((string file, XElement def) in QuestDefs()) {
            string name = RequireDefName(file, def);

            foreach (string field in new[] { "giverFaction", "targetFaction" }) {
                XElement? element = def.Element(field);
                if (element == null) continue;

                Assert.IsTrue(
                    factions.Contains(element.Value),
                    $"{name}: {field} '{element.Value}' does not resolve to a known FactionDef."
                );
            }
        }
    }

    /// <summary>
    ///     MetalDef.Item resolves the reward's actual item by looking up a ThingDef with the
    ///     same defName, so a metal reference needs both to exist. Only the MetalDef existing
    ///     is not enough - the reward would silently give nothing.
    /// </summary>
    [TestMethod]
    public void EveryGodMetalRewardResolvesBothMetalDefAndThingDef() {
        HashSet<string> metalDefs = DefNamesUnder("CosmereCore", "Defs", "Metals");
        HashSet<string> itemThingDefs = DefNamesUnder("CosmereCore", "Defs", "Things", "Metals", "Items");

        foreach ((string file, XElement def) in QuestDefs()) {
            string name = RequireDefName(file, def);

            foreach (XElement reward in def.Descendants("li")) {
                if (!HasClass(reward, "GodMetalReward")) continue;

                XElement? metal = reward.Element("metal");
                Assert.IsNotNull(metal, $"{name}: a GodMetalReward has no metal.");

                Assert.IsTrue(
                    metalDefs.Contains(metal!.Value),
                    $"{name}: GodMetalReward metal '{metal.Value}' has no matching MetalDef under CosmereCore/Defs/Metals."
                );
                Assert.IsTrue(
                    itemThingDefs.Contains(metal.Value),
                    $"{name}: GodMetalReward metal '{metal.Value}' has a MetalDef but no matching ThingDef " +
                    "under CosmereCore/Defs/Things/Metals/Items. MetalDef.Item looks up a ThingDef by the " +
                    "same defName, so the reward would silently give nothing."
                );
            }
        }
    }

    /// <summary>
    ///     ClearSiteObjective, TimedWorkObjective and SpawnThingObjective all read the site off
    ///     the QuestPart_ArrivedAtSite that TravelToSiteObjective creates. Getting the stage
    ///     order wrong only surfaces as QuestBuildFailure the first time the incident fires.
    /// </summary>
    [TestMethod]
    public void SiteDependentObjectivesFollowATravelToSiteObjective() {
        foreach ((string file, XElement def) in QuestDefs()) {
            string name = RequireDefName(file, def);

            XElement? stages = def.Element("stages");
            if (stages == null) continue;

            bool sawTravel = false;
            foreach (XElement stage in stages.Elements("li")) {
                XElement? objective = stage.Element("objective");
                string? cls = objective?.Attribute("Class")?.Value;
                if (cls == null) continue;

                if (cls.EndsWith(".TravelToSiteObjective", StringComparison.Ordinal)) {
                    sawTravel = true;
                    continue;
                }

                foreach (string dependent in SiteDependentObjectiveTypes) {
                    if (!cls.EndsWith("." + dependent, StringComparison.Ordinal)) continue;

                    Assert.IsTrue(
                        sawTravel,
                        $"{name}: {dependent} appears with no preceding TravelToSiteObjective stage. " +
                        "It builds by reading QuestPart_ArrivedAtSite off the quest, which only " +
                        "TravelToSiteObjective adds, so the quest would throw QuestBuildFailure when offered."
                    );
                }
            }
        }
    }

    /// <summary>No def uses RolledReward yet, so this passes vacuously today. It stays in place
    ///     so the first one that does is guarded from launch.</summary>
    [TestMethod]
    public void EveryRolledRewardBranchWeightsSumToOneHundred() {
        foreach ((string file, XElement def) in QuestDefs()) {
            string name = RequireDefName(file, def);

            foreach (XElement reward in def.Descendants("li")) {
                if (!HasClass(reward, "RolledReward")) continue;

                XElement? branches = reward.Element("branches");
                Assert.IsNotNull(branches, $"{name}: a RolledReward has no branches.");

                List<RewardTableEntry> entries = new List<RewardTableEntry>();
                foreach (XElement branch in branches!.Elements("li")) {
                    XElement? key = branch.Element("key");
                    XElement? weight = branch.Element("weight");
                    Assert.IsNotNull(key, $"{name}: a RolledReward branch has no key.");
                    Assert.IsNotNull(weight, $"{name}: RolledReward branch '{key!.Value}' has no weight.");
                    entries.Add(new RewardTableEntry { key = key.Value, weight = int.Parse(weight!.Value) });
                }

                Assert.IsTrue(
                    RewardTable.IsValid(entries),
                    $"{name}: RolledReward branch weights sum to {RewardTable.TotalWeight(entries)}, expected exactly 100."
                );
            }
        }
    }

    [TestMethod]
    public void EveryCapstoneDeclaresAnOnFailure() {
        foreach ((string file, XElement def) in QuestDefs()) {
            string name = RequireDefName(file, def);
            if (def.Element("kind")?.Value != "Capstone") continue;

            Assert.IsNotNull(
                def.Element("onFailure"),
                $"{name}: is a Capstone but declares no onFailure. Use " +
                "Cosmere.Core.Quest.Outcome.NeverBurns if it genuinely cannot fail."
            );
        }
    }

    /// <summary>
    ///     CosmereQuestEligibility.IsOfferableByStoryteller excludes Capstone quests from the
    ///     random offer pool by design - they are meant to reach the player through
    ///     StartQuestAction fired from scenario progression instead. A capstone with no
    ///     StartQuestAction anywhere is unreachable content: nothing will ever offer it.
    /// </summary>
    [TestMethod]
    public void EveryCapstoneIsReferencedByAStartQuestAction() {
        HashSet<string> referenced = CapstonesReferencedByStartQuestAction();
        foreach ((string file, XElement def) in QuestDefs()) {
            string name = RequireDefName(file, def);
            if (def.Element("kind")?.Value != "Capstone") continue;

            Assert.IsTrue(
                referenced.Contains(name),
                $"{name}: is a Capstone but no StartQuestAction under " +
                "CosmereScadrial/Defs/ScenarioProgression references it. Capstones are excluded " +
                "from the storyteller's random offer pool, so this quest can never be offered."
            );
        }
    }

    /// <summary>An inverted band means TileFinder can never find a tile and the quest can
    ///     never build. A real bug in the plan's original defs.</summary>
    [TestMethod]
    public void EveryTravelToSiteObjectiveHasANonInvertedTileBand() {
        foreach ((string file, XElement def) in QuestDefs()) {
            string name = RequireDefName(file, def);

            foreach (XElement objective in def.Descendants("objective")) {
                if (!HasClass(objective, "TravelToSiteObjective")) continue;

                XElement? minElement = objective.Element("minTiles");
                XElement? maxElement = objective.Element("maxTiles");
                int min = minElement != null ? int.Parse(minElement.Value) : 7;
                int max = maxElement != null ? int.Parse(maxElement.Value) : 27;

                Assert.IsTrue(
                    max >= min,
                    $"{name}: TravelToSiteObjective maxTiles ({max}) is below minTiles ({min}). " +
                    "TileFinder can never satisfy an inverted band, so the quest can never build."
                );
            }
        }
    }
}
