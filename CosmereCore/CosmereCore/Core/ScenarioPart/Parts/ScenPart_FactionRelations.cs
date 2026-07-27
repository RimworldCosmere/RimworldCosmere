using System;
using System.Xml;
using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart.Parts;

public class ScenPart_FactionRelations : ScenPart {
    public List<FactionRelationEntry> relations = [];

    public override void PostGameStart() {
        base.PostGameStart();

        Faction player = Faction.OfPlayer;
        if (player == null) {
            Logger.Warning("ScenPart_FactionRelations: Faction.OfPlayer is null");
            return;
        }

        for (int i = 0; i < relations.Count; i++) {
            FactionRelationEntry entry = relations[i];

            FactionDef? def = DefDatabase<FactionDef>.GetNamedSilentFail(entry.faction);
            if (def == null) {
                Logger.Warning($"ScenPart_FactionRelations: FactionDef '{entry.faction}' not found");
                continue;
            }

            Faction? other = Find.FactionManager.FirstFactionOfDef(def);
            if (other == null) {
                try {
                    FactionGenerator.CreateFactionAndAddToManager(def);
                } catch (Exception ex) {
                    Logger.Warning(
                        $"ScenPart_FactionRelations: Failed to create faction '{entry.faction}': {ex}"
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

            if (delta != 0) {
                player.TryAffectGoodwillWith(other, delta, false, false);
            }
        }
    }

    public override string Summary(Scenario scen) {
        if (relations.Count == 0) return string.Empty;
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
    public string faction = string.Empty;
    public int goodwill;

    public void ExposeData() {
        Scribe_Values.Look(ref faction, "faction", string.Empty);
        Scribe_Values.Look(ref goodwill, "goodwill");
    }

    public void LoadDataFromXmlCustom(XmlNode xmlNode) {
        faction = xmlNode.Name;
        goodwill = ParseHelper.FromString<int>(xmlNode.InnerText);
    }
}
