using System;
using System.Xml;
using Cosmere.Core.Patch;
using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart.Parts;

public class ScenPart_FactionRelations : ScenPart {
    public List<FactionRelationEntry> relations = [];

    /// <summary>
    ///     Goodwill between two factions that are not the player. Without this everyone the
    ///     scenario creates starts neutral to everyone else, so the skaa and the empire sit out
    ///     the Collapse being polite to each other.
    /// </summary>
    public List<FactionPairEntry> between = [];

    public override void PostGameStart() {
        base.PostGameStart();

        Faction player = Faction.OfPlayer;
        if (player == null) {
            Logger.Warning("ScenPart_FactionRelations: Faction.OfPlayer is null");
            return;
        }

        for (int i = 0; i < relations.Count; i++) {
            FactionRelationEntry entry = relations[i];

            Faction? other = ResolveOrCreate(entry.faction);
            if (other == null) continue;

            int delta = entry.goodwill - player.GoodwillWith(other);
            if (delta != 0) player.TryAffectGoodwillWith(other, delta, false, false);
        }

        for (int i = 0; i < between.Count; i++) {
            FactionPairEntry pair = between[i];

            Faction? a = ResolveOrCreate(pair.a);
            Faction? b = ResolveOrCreate(pair.b);
            if (a == null || b == null || a == b) continue;

            int delta = pair.goodwill - a.GoodwillWith(b);
            if (delta == 0) continue;

            // SetRelationDirect refuses a pair that both use goodwill, so relation kind derives from delta.
            a.TryAffectGoodwillWith(b, delta, false, false);
        }
    }

    /// <summary>
    ///     The faction for a def, creating it if world generation did not roll one. A scenario
    ///     naming a faction is what guarantees it exists.
    /// </summary>
    private static Faction? ResolveOrCreate(string defName) {
        FactionDef? def = DefDatabase<FactionDef>.GetNamedSilentFail(defName);
        if (def == null) {
            Logger.Warning($"ScenPart_FactionRelations: FactionDef '{defName}' not found");
            return null;
        }

        Faction? faction = Find.FactionManager.FirstFactionOfDef(def);
        if (faction != null) return faction;

        try {
            FactionGeneratorPatch.CreateScripted(def);
        } catch (Exception ex) {
            Logger.Warning($"ScenPart_FactionRelations: Failed to create faction '{defName}': {ex}");
            return null;
        }

        return Find.FactionManager.FirstFactionOfDef(def);
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
        Scribe_Collections.Look(ref between, "between", LookMode.Deep);
    }
}

/// <summary>Goodwill between two factions, neither of which is the player.</summary>
public class FactionPairEntry : IExposable {
    public string a = string.Empty;
    public string b = string.Empty;
    public int goodwill;

    public void ExposeData() {
        Scribe_Values.Look(ref a, "a", string.Empty);
        Scribe_Values.Look(ref b, "b", string.Empty);
        Scribe_Values.Look(ref goodwill, "goodwill");
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
