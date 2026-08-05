using System;
using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart.Action;

/// <summary>
///     Sends an army at the colony. For a story beat whose whole point is that the threat
///     outside the walls finally comes through them.
///     <para>
///         Points scale off what the storyteller thinks the colony can take, rather than a fixed
///         number, so a beat written for a mid-game siege does not flatten a colony that got
///         there early or tickle one that got there late. A floor keeps it from being nothing.
///     </para>
/// </summary>
public class RaidAction : ProgressionAction {
    public string faction = string.Empty;

    /// <summary>Never send fewer points than this, however soft the colony looks.</summary>
    public float minPoints = 800f;

    /// <summary>Multiplier on the storyteller's current threat points.</summary>
    public float pointsFactor = 1f;

    /// <summary>Shown in the letter's effects list. Straff's army reads better than "a raid".</summary>
    public string? armyNameKey;

    /// <summary>
    ///     How they turn up. Defaults to walking in from the map edge, because an army that has
    ///     been camped outside the walls for a month does not arrive by drop pod. Vanilla only
    ///     picks a mode when none is set, so this is respected rather than rerolled.
    /// </summary>
    public string arrivalMode = "EdgeWalkIn";

    /// <summary>
    ///     Which map edge they walk in from - North, South, East or West. Left empty they pick
    ///     their own, which puts two armies that are meant to be converging on the same side.
    /// </summary>
    public string edge = string.Empty;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        FactionDef? def = DefDatabase<FactionDef>.GetNamedSilentFail(faction);
        if (def == null) {
            Logger.Warning($"ScenarioProgression: FactionDef '{faction}' not found for Raid");
            return;
        }

        // Create it rather than give up. A scenario's relations block normally guarantees the
        // faction exists, but arcs are reachable from starts that never declared one, and a
        // story beat that says an army arrives must not quietly send nobody.
        Faction? attacker = Find.FactionManager.FirstFactionOfDef(def);
        if (attacker == null) {
            try {
                FactionGenerator.CreateFactionAndAddToManager(def);
            } catch (Exception ex) {
                Logger.Warning($"ScenarioProgression: could not create faction '{faction}': {ex}");
                return;
            }

            attacker = Find.FactionManager.FirstFactionOfDef(def);
            if (attacker == null) {
                Logger.Warning($"ScenarioProgression: still no faction for '{faction}', no raid sent.");
                return;
            }

            Logger.Important($"ScenarioProgression: created faction '{faction}' so its raid could land.");
        }

        Map? map = Find.CurrentMap;
        if (map == null) return;

        IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
        parms.forced = true;
        parms.faction = attacker;

        float points = StorytellerUtility.DefaultThreatPointsNow(map) * pointsFactor;
        parms.points = points < minPoints ? minPoints : points;

        if (edge.Length > 0) {
            parms.spawnRotation = edge switch {
                "North" => Rot4.North,
                "South" => Rot4.South,
                "East" => Rot4.East,
                "West" => Rot4.West,
                _ => Rot4.Random,
            };
        }

        if (arrivalMode.Length > 0) {
            PawnsArrivalModeDef? mode = DefDatabase<PawnsArrivalModeDef>.GetNamedSilentFail(arrivalMode);
            if (mode == null) {
                Logger.Warning($"ScenarioProgression: arrival mode '{arrivalMode}' not found, letting vanilla pick.");
            } else {
                parms.raidArrivalMode = mode;
            }
        }

        // A story beat that says an army arrives has to produce one. Goodwill is set hostile
        // first because RaidEnemy refuses a faction that is not.
        if (!attacker.HostileTo(Faction.OfPlayer)) {
            attacker.TryAffectGoodwillWith(Faction.OfPlayer, -200, false, false);
        }

        if (!IncidentDefOf.RaidEnemy.Worker.TryExecute(parms)) {
            Logger.Warning($"ScenarioProgression: RaidEnemy refused to fire for '{faction}'.");
            return;
        }

        Logger.Important($"ScenarioProgression: sent {parms.points:F0} points of {attacker.Name} at the colony.");
    }

    public override string? Describe() {
        FactionDef? def = DefDatabase<FactionDef>.GetNamedSilentFail(faction);
        if (def == null) return null;

        string army = armyNameKey != null && armyNameKey.Length > 0
            ? armyNameKey.Translate().Resolve()
            : def.label;

        return "CC_Progression_Effect_Raid".Translate(army.Named("ARMY")).Resolve();
    }
}
