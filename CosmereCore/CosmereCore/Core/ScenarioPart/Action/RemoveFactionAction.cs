using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cosmere.Core.ScenarioPart.Action;

/// <summary>
///     Takes a faction out of the world for the beat where an institution stops existing rather
///     than merely losing - a noble house that does not come out the other side of a collapse.
/// </summary>
/// <remarks>
///     Deliberately not FactionManager.Remove. That tears the Faction out of allFactions while
///     the goodwill manager, play log, tales and lords all still point at it, and every one of
///     them then asks a relationless faction for a relation - 1900 lines of "returning dummy
///     relation" in a single session. Vanilla only removes temporary quest factions for exactly
///     that reason.
///     <para>
///         Instead: defeated, hidden and off the world map. Faction.hidden is a per-instance
///         bool? that QuestPart_SetFactionHidden already uses, so the row leaves the Factions tab
///         and ShouldHaveLeader goes false, while every existing reference stays valid.
///     </para>
/// </remarks>
public class RemoveFactionAction : ProgressionAction {
    public List<string> factions = [];

    public override void Execute(GameComponent_ScenarioProgression comp) {
        for (int i = 0; i < factions.Count; i++) {
            FactionDef? def = DefDatabase<FactionDef>.GetNamedSilentFail(factions[i]);
            if (def == null) {
                Logger.Warning($"ScenarioProgression: FactionDef '{factions[i]}' not found for RemoveFaction");
                continue;
            }

            Faction? faction = Find.FactionManager.FirstFactionOfDef(def);
            if (faction is not { defeated: false }) continue;

            Retire(faction, factions[i]);
        }
    }

    private static void Retire(Faction faction, string defName) {
        List<WorldObject> owned = [];
        foreach (WorldObject obj in Find.WorldObjects.AllWorldObjects) {
            if (obj.Faction == faction) owned.Add(obj);
        }

        int destroyed = 0;
        for (int i = 0; i < owned.Count; i++) {
            // A world object holding a live map is one the player may be standing on.
            if (owned[i] is MapParent { HasMap: true }) continue;
            owned[i].Destroy();
            destroyed++;
        }

        faction.defeated = true;
        faction.hidden = true;
        Logger.Important($"ScenarioProgression: '{defName}' is finished, {destroyed} settlements removed.");
    }

    public override string? Describe() {
        if (factions.Count == 0) return null;

        List<string> labels = [];
        for (int i = 0; i < factions.Count; i++) {
            FactionDef? def = DefDatabase<FactionDef>.GetNamedSilentFail(factions[i]);
            if (def != null) labels.Add(def.label);
        }

        return labels.Count == 0
            ? null
            : "CC_Progression_Effect_FactionsGone".Translate(string.Join(", ", labels).Named("FACTIONS")).Resolve();
    }
}
