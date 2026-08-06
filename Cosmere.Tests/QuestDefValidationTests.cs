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
    ///     A FactionDef only makes a faction eligible for world generation, not certain -
    ///     canMakeRandomly is a roll. A raid whose faction is absent sends nobody. So every
    ///     faction raided by an arc a scenario can actually reach must be nailed down: named in
    ///     that scenario's relations block, or created by a CreateFactionAction along the way.
    ///     <para>
    ///         Reachability follows the handoff graph. The Alloy of Law cannot reach the Final
    ///         Empire's beats and has no business guaranteeing the factions they raid with.
    ///     </para>
    /// </summary>
    [TestMethod]
    public void EveryRaidFactionIsGuaranteedByTheScenarios() {
        if (!Directory.Exists(ScenarioProgressionDirectory)) return;

        Dictionary<string, HashSet<string>> raidsIn = new Dictionary<string, HashSet<string>>();
        Dictionary<string, HashSet<string>> createsIn = new Dictionary<string, HashSet<string>>();
        Dictionary<string, HashSet<string>> handsOffTo = new Dictionary<string, HashSet<string>>();

        foreach (string path in Directory.GetFiles(ScenarioProgressionDirectory, "*.xml")) {
            XElement? root = XDocument.Load(path).Root;
            if (root == null) continue;

            foreach (XElement def in root.Elements()) {
                string name = def.Element("defName")?.Value ?? string.Empty;
                if (name.Length == 0) continue;

                raidsIn[name] = new HashSet<string>();
                createsIn[name] = new HashSet<string>();
                handsOffTo[name] = new HashSet<string>();

                foreach (XElement action in def.Descendants("li")) {
                    string? cls = (string?)action.Attribute("Class");
                    if (cls == null) continue;

                    if (cls.EndsWith("RaidAction", StringComparison.Ordinal)) {
                        string? f = action.Element("faction")?.Value;
                        if (f != null) raidsIn[name].Add(f);
                    } else if (cls.EndsWith("CreateFactionAction", StringComparison.Ordinal)) {
                        string? f = action.Element("faction")?.Value;
                        if (f != null) createsIn[name].Add(f);
                    } else if (cls.EndsWith("HandOffProgressionAction", StringComparison.Ordinal)) {
                        string? p2 = action.Element("progression")?.Value;
                        if (p2 != null) handsOffTo[name].Add(p2);
                    }
                }
            }
        }

        string scenarioDir = Path.Combine(RepoRoot, "CosmereScadrial", "Defs", "Scenarios");
        if (!Directory.Exists(scenarioDir)) return;

        foreach (string path in Directory.GetFiles(scenarioDir, "*.xml")) {
            XElement? root = XDocument.Load(path).Root;
            if (root == null) continue;

            string? startArc = null;
            foreach (XElement part in root.Descendants("li")) {
                string? cls = (string?)part.Attribute("Class");
                if (cls != null && cls.EndsWith("ScenPart_ScenarioProgression", StringComparison.Ordinal)) {
                    startArc = part.Element("progression")?.Value;
                    break;
                }
            }

            // No progression means no arc, so no raid can ever fire from this scenario.
            if (startArc == null || !raidsIn.ContainsKey(startArc)) continue;

            // Must be the faction ScenPart's relations, not a named pawn's - those share a
            // tag name and the pawn block comes first in the file.
            XElement? relations = null;
            foreach (XElement part in root.Descendants("li")) {
                string? cls = (string?)part.Attribute("Class");
                if (cls != null && cls.EndsWith("ScenPart_FactionRelations", StringComparison.Ordinal)) {
                    relations = part.Element("relations");
                    break;
                }
            }

            HashSet<string> guaranteed = new HashSet<string>();
            if (relations != null) {
                foreach (XElement entry in relations.Elements()) guaranteed.Add(entry.Name.LocalName);
            }

            // Walk the handoff graph from where this scenario starts.
            HashSet<string> reachable = new HashSet<string>();
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(startArc);
            while (queue.Count > 0) {
                string arc = queue.Dequeue();
                if (!reachable.Add(arc) || !handsOffTo.ContainsKey(arc)) continue;
                foreach (string next in handsOffTo[arc]) queue.Enqueue(next);
            }

            foreach (string arc in reachable) {
                foreach (string faction in raidsIn[arc]) {
                    bool created = false;
                    foreach (string a in reachable) {
                        if (createsIn[a].Contains(faction)) {
                            created = true;
                            break;
                        }
                    }

                    Assert.IsTrue(
                        created || guaranteed.Contains(faction),
                        $"{Path.GetFileName(path)}: reaches '{arc}', which raids with '{faction}', " +
                        "but this scenario neither names it in faction relations nor creates it " +
                        "with a CreateFactionAction on the way, so the raid would send nobody."
                    );
                }
            }
        }
    }

    /// <summary>
    ///     DaysPassedTrigger reads absolute game days unless told otherwise, and CheckProgression
    ///     fires every event whose triggers are met in a single pass. An arc reached by a handoff
    ///     part-way through a campaign has already passed all its thresholds, so without
    ///     sinceArcStart its whole run of events dumps in one tick.
    /// </summary>
    [TestMethod]
    public void HandedOffArcsCountDaysFromTheirOwnStart() {
        if (!Directory.Exists(ScenarioProgressionDirectory)) return;

        HashSet<string> handoffTargets = new HashSet<string>();
        Dictionary<string, (string file, XElement def)> byName =
            new Dictionary<string, (string, XElement)>();

        foreach (string path in Directory.GetFiles(ScenarioProgressionDirectory, "*.xml")) {
            XElement? root = XDocument.Load(path).Root;
            if (root == null) continue;

            foreach (XElement def in root.Elements()) {
                XElement? name = def.Element("defName");
                if (name != null) byName[name.Value] = (Path.GetFileName(path), def);
            }

            foreach (XElement action in root.Descendants("li")) {
                string? cls = (string?)action.Attribute("Class");
                if (cls == null || !cls.EndsWith("HandOffProgressionAction", StringComparison.Ordinal)) continue;

                XElement? target = action.Element("progression");
                if (target != null) handoffTargets.Add(target.Value);
            }
        }

        foreach (string target in handoffTargets) {
            Assert.IsTrue(
                byName.ContainsKey(target),
                $"A HandOffProgressionAction names progression '{target}', which we do not ship."
            );

            (string file, XElement def) = byName[target];
            foreach (XElement trigger in def.Descendants("li")) {
                string? cls = (string?)trigger.Attribute("Class");
                if (cls == null || !cls.EndsWith("DaysPassedTrigger", StringComparison.Ordinal)) continue;

                Assert.AreEqual(
                    "true",
                    trigger.Element("sinceArcStart")?.Value.ToLowerInvariant(),
                    $"{file}: '{target}' is reached by a handoff, so its DaysPassedTrigger for " +
                    $"day {trigger.Element("days")?.Value} must set sinceArcStart. Absolute days " +
                    "would already be behind the campaign and the whole arc fires at once."
                );
            }
        }
    }

    /// <summary>
    ///     A progression choice that names a key with no entry behind it renders the raw key
    ///     string as the dialog's title and buttons. Nothing errors, and the branch it sits on
    ///     may be sixty in-game days from where anyone would look.
    /// </summary>
    [TestMethod]
    public void EveryProgressionChoiceKeyResolves() {
        HashSet<string> keys = KnownTranslationKeys();
        string[] keyFields = ["titleKey", "textKey", "acceptKey", "declineKey"];

        if (!Directory.Exists(ScenarioProgressionDirectory)) return;

        foreach (string path in Directory.GetFiles(ScenarioProgressionDirectory, "*.xml")) {
            XElement? root = XDocument.Load(path).Root;
            if (root == null) continue;

            string file = Path.GetFileName(path);
            foreach (XElement action in root.Descendants("li")) {
                string? cls = (string?)action.Attribute("Class");
                if (cls == null) continue;
                if (!cls.EndsWith("ChoiceAction", StringComparison.Ordinal)
                    && !cls.EndsWith("EndGameAction", StringComparison.Ordinal)) {
                    continue;
                }

                foreach (string field in keyFields) {
                    XElement? key = action.Element(field);
                    if (key == null || key.Value.Length == 0) continue;

                    Assert.IsTrue(
                        keys.Contains(key.Value),
                        $"{file}: {cls} names {field} '{key.Value}', which has no entry in any " +
                        "Languages/English/Keyed folder."
                    );
                }
            }
        }
    }

    /// <summary>
    ///     A TriggerIncidentAction resolves its incident by name at runtime and only warns when
    ///     the lookup misses, so a typo turns the beat into a letter that never arrives.
    /// </summary>
    [TestMethod]
    public void EveryTriggeredIncidentIsOneWeShip() {
        HashSet<string> incidents = new HashSet<string>();
        foreach ((string _, XElement def) in DefsOfType("IncidentDef")) {
            XElement? name = def.Element("defName");
            if (name != null) incidents.Add(name.Value);
        }

        if (!Directory.Exists(ScenarioProgressionDirectory)) return;

        foreach (string path in Directory.GetFiles(ScenarioProgressionDirectory, "*.xml")) {
            XElement? root = XDocument.Load(path).Root;
            if (root == null) continue;

            string file = Path.GetFileName(path);
            foreach (XElement action in root.Descendants("li")) {
                string? cls = (string?)action.Attribute("Class");
                if (cls == null || !cls.EndsWith("TriggerIncidentAction", StringComparison.Ordinal)) continue;

                XElement? incident = action.Element("incident");
                if (incident == null) continue;

                Assert.IsTrue(
                    incidents.Contains(incident.Value),
                    $"{file}: TriggerIncidentAction names incident '{incident.Value}', which is " +
                    "not an IncidentDef we ship."
                );
            }
        }
    }

    /// <summary>
    ///     RemoveFactionAction looks its factions up by name and only warns on a miss, so a typo
    ///     silently leaves a house in a world the arc has just told the player it left.
    /// </summary>
    [TestMethod]
    public void EveryRetiredFactionIsOneWeShip() {
        HashSet<string> factions = new HashSet<string>();
        foreach ((string _, XElement def) in DefsOfType("FactionDef")) {
            XElement? name = def.Element("defName");
            if (name != null) factions.Add(name.Value);
        }

        if (!Directory.Exists(ScenarioProgressionDirectory)) return;

        foreach (string path in Directory.GetFiles(ScenarioProgressionDirectory, "*.xml")) {
            XElement? root = XDocument.Load(path).Root;
            if (root == null) continue;

            string file = Path.GetFileName(path);
            foreach (XElement action in root.Descendants("li")) {
                string? cls = (string?)action.Attribute("Class");
                if (cls == null || !cls.EndsWith("RemoveFactionAction", StringComparison.Ordinal)) continue;

                XElement? list = action.Element("factions");
                Assert.IsNotNull(list, $"{file}: RemoveFactionAction has no <factions> list, so it removes nothing.");

                foreach (XElement entry in list.Elements("li")) {
                    Assert.IsTrue(
                        factions.Contains(entry.Value),
                        $"{file}: RemoveFactionAction names faction '{entry.Value}', which is not " +
                        "a FactionDef we ship."
                    );
                }
            }
        }
    }

    /// <summary>
    ///     A mine-out objective reads what is standing on the site's map. A site that is not
    ///     persistent tears its map down when the last colonist leaves and regenerates the ore
    ///     on the next visit, so the count can never fall - the quest just never completes, with
    ///     no error to say why.
    /// </summary>
    [TestMethod]
    public void EveryMineOutObjectiveSitsOnAPersistentSite() {
        foreach ((string file, XElement def) in QuestDefs()) {
            string name = RequireDefName(file, def);

            bool minesOut = false;
            foreach (XElement objective in def.Descendants("objective")) {
                string? cls = (string?)objective.Attribute("Class");
                if (cls != "Cosmere.Core.Quest.Objective.MineOutSiteObjective") continue;

                minesOut = true;

                XElement? fraction = objective.Element("fraction");
                if (fraction == null) continue;

                Assert.IsTrue(
                    float.TryParse(fraction.Value, out float parsed) && parsed > 0f && parsed <= 1f,
                    $"{name}: MineOutSiteObjective fraction '{fraction.Value}' must be above 0 and at most 1."
                );
            }

            if (!minesOut) continue;

            bool persistent = false;
            foreach (XElement objective in def.Descendants("objective")) {
                string? cls = (string?)objective.Attribute("Class");
                if (cls != "Cosmere.Core.Quest.Objective.TravelToSiteObjective") continue;
                if (objective.Element("persistent")?.Value.ToLowerInvariant() == "true") persistent = true;
            }

            Assert.IsTrue(
                persistent,
                $"{name}: has a MineOutSiteObjective but its TravelToSiteObjective is not persistent, " +
                "so the site regenerates its ore on every visit and the stage can never complete."
            );
        }
    }

    /// <summary>
    ///     MapParent.MapGeneratorDef falls back to Encounter when def.mapGenerator is null, so a
    ///     misspelt worldObject or mapGenerator does not error - the site just quietly generates
    ///     with ancient ruins scattered over it again.
    /// </summary>
    [TestMethod]
    public void EveryWorldObjectOverrideResolves() {
        Dictionary<string, XElement> worldObjects = new Dictionary<string, XElement>();
        foreach ((string _, XElement def) in DefsOfType("WorldObjectDef")) {
            XElement? name = def.Element("defName");
            if (name != null) worldObjects[name.Value] = def;
        }

        HashSet<string> generators = new HashSet<string>();
        foreach ((string _, XElement def) in DefsOfType("MapGeneratorDef")) {
            XElement? name = def.Element("defName");
            if (name != null) generators.Add(name.Value);
        }

        foreach ((string file, XElement def) in QuestDefs()) {
            string name = RequireDefName(file, def);

            foreach (XElement objective in def.Descendants("objective")) {
                XElement? worldObject = objective.Element("worldObject");
                if (worldObject == null) continue;

                Assert.IsTrue(
                    worldObjects.TryGetValue(worldObject.Value, out XElement? shipped),
                    $"{name}: worldObject '{worldObject.Value}' is not a WorldObjectDef we ship."
                );

                XElement? generator = shipped!.Element("mapGenerator");
                Assert.IsNotNull(
                    generator,
                    $"{name}: worldObject '{worldObject.Value}' sets no mapGenerator, so the site " +
                    "would fall back to Encounter and there was no point overriding it."
                );

                Assert.IsTrue(
                    generators.Contains(generator!.Value),
                    $"{name}: worldObject '{worldObject.Value}' names mapGenerator " +
                    $"'{generator.Value}', which is not a MapGeneratorDef we ship."
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
                    if (ours.Contains(entry.Value)) continue;

                    // A vanilla base-building part walls its base into a rect BESIDE the site's
                    // rect of interest, so it can never guard the objective itself. That is
                    // allowed only when the quest also ships a garrison part, which does.
                    bool guarded = false;
                    foreach (XElement other in extras.Elements("li")) {
                        if (ours.Contains(other.Value) && other.Value.Contains("Garrison")) guarded = true;
                    }

                    Assert.IsTrue(
                        guarded,
                        $"{name}: extraSiteParts names '{entry.Value}', a vanilla base-building part, " +
                        "with no garrison part alongside it. It would build a fort next to the " +
                        "objective and leave the objective itself undefended."
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

    /// <summary>
    ///     A hand-off naming a progression that does not exist stops the Scadrial timeline dead
    ///     at that arc, and nothing complains - the campaign just quietly runs out of story.
    /// </summary>
    [TestMethod]
    public void EveryProgressionHandOffNamesARealArc() {
        HashSet<string> arcs = new HashSet<string>();
        foreach ((string _, XElement def) in DefsOfType("Cosmere.Core.ScenarioPart.ScenarioProgressionDef")) {
            XElement? name = def.Element("defName");
            if (name != null) arcs.Add(name.Value);
        }

        Assert.IsTrue(arcs.Count > 0, "No ScenarioProgressionDefs were found at all.");

        foreach ((string file, XElement def) in DefsOfType("Cosmere.Core.ScenarioPart.ScenarioProgressionDef")) {
            string name = RequireDefName(file, def);

            foreach (XElement action in def.Descendants("li")) {
                if (!HasClass(action, "HandOffProgressionAction")) continue;

                XElement? target = action.Element("progression");
                Assert.IsNotNull(target, $"{name}: a HandOffProgressionAction names no progression.");
                Assert.IsTrue(
                    arcs.Contains(target!.Value),
                    $"{name}: hands off to '{target.Value}', which is not a ScenarioProgressionDef. " +
                    "The timeline would stop here."
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
