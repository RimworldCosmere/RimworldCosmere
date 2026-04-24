using System;
using System.Xml;
using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart;

public class ScenPart_FactionRelations : ScenPart {
    public List<FactionRelationEntry> relations = [];

    public override void PostGameStart() {
        base.PostGameStart();
        Logger.Info($"ScenPart_FactionRelations.PostGameStart entered. relations.Count={relations.Count}");

        Faction player = Faction.OfPlayer;
        if (player == null) {
            Logger.Warning("ScenPart_FactionRelations: Faction.OfPlayer is null");
            return;
        }

        Logger.Info($"ScenPart_FactionRelations: player faction = {player.Name} ({player.def.defName})");

        for (int i = 0; i < relations.Count; i++) {
            FactionRelationEntry entry = relations[i];
            Logger.Info($"ScenPart_FactionRelations: processing entry [{entry.faction}] = {entry.goodwill}");

            FactionDef? def = DefDatabase<FactionDef>.GetNamedSilentFail(entry.faction);
            if (def == null) {
                Logger.Warning($"ScenPart_FactionRelations: FactionDef '{entry.faction}' not found");
                continue;
            }

            Faction? other = Find.FactionManager.FirstFactionOfDef(def);
            if (other == null) {
                Logger.Info($"ScenPart_FactionRelations: '{entry.faction}' not in world, creating instance");
                try {
                    FactionGenerator.CreateFactionAndAddToManager(def);
                } catch (Exception ex) {
                    Logger.Warning(
                        $"ScenPart_FactionRelations: Failed to create faction '{entry.faction}': {ex.Message}"
                    );
                    continue;
                }

                other = Find.FactionManager.FirstFactionOfDef(def);
                if (other == null) {
                    Logger.Warning(
                        $"ScenPart_FactionRelations: Still no faction instance for '{entry.faction}' after creation attempt"
                    );
                    continue;
                }
            }

            int current = player.GoodwillWith(other);
            int delta = entry.goodwill - current;
            Logger.Info(
                $"ScenPart_FactionRelations: {other.Name} current={current}, target={entry.goodwill}, delta={delta}"
            );

            if (delta != 0) {
                bool canChange = player.CanChangeGoodwillFor(other, delta);
                Logger.Info($"ScenPart_FactionRelations: CanChangeGoodwillFor({other.Name}) = {canChange}");
                bool changed = player.TryAffectGoodwillWith(other, delta, false, false);
                int after = player.GoodwillWith(other);
                Logger.Info(
                    $"ScenPart_FactionRelations: TryAffectGoodwillWith returned {changed}, goodwill now {after}"
                );
            }
        }
    }

    public override string Summary(Scenario scen) {
        if (relations.Count == 0) return "";
        List<string> parts = [];
        for (int i = 0; i < relations.Count; i++) {
            FactionRelationEntry entry = relations[i];
            parts.Add($"{entry.faction}: {entry.goodwill:+#;-#;0}");
        }

        return "Starting relations: " + string.Join(", ", parts);
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Collections.Look(ref relations, "relations", LookMode.Deep);
    }
}

public class FactionRelationEntry : IExposable {
    public string faction = "";
    public int goodwill;

    public void ExposeData() {
        Scribe_Values.Look(ref faction, "faction", "");
        Scribe_Values.Look(ref goodwill, "goodwill");
    }

    public void LoadDataFromXmlCustom(XmlNode xmlNode) {
        faction = xmlNode.Name;
        goodwill = ParseHelper.FromString<int>(xmlNode.InnerText);
    }
}