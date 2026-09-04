using System;
using Cosmere.Core.Patch;
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

    /// <summary>
    ///     Allies arriving to help rather than an army arriving to kill you. Uses vanilla's
    ///     friendly raid, and never touches goodwill - a faction sending help is not a faction
    ///     you want turned hostile on the way in.
    /// </summary>
    public bool friendly;

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
            Log.Warn($"ScenarioProgression: FactionDef '{faction}' not found for Raid");
            return;
        }

        // Some starts never declare this faction in relations; a beat promising a raid must not send nobody.
        Faction? attacker = Find.FactionManager.FirstFactionOfDef(def);
        if (attacker == null) {
            try {
                FactionGeneratorPatch.CreateScripted(def);
            } catch (Exception ex) {
                Log.Warn($"ScenarioProgression: could not create faction '{faction}': {ex}");
                return;
            }

            attacker = Find.FactionManager.FirstFactionOfDef(def);
            if (attacker == null) {
                Log.Warn($"ScenarioProgression: still no faction for '{faction}', no raid sent.");
                return;
            }

            Log.Info($"ScenarioProgression: created faction '{faction}' so its raid could land.");
        }

        Map? map = Find.CurrentMap;
        if (map == null) return;

        // Take the category off the incident itself: friendly raids aren't a threat category with a DefOf constant.
        IncidentDef incident = friendly ? IncidentDefOf.RaidFriendly : IncidentDefOf.RaidEnemy;

        IncidentParms parms = StorytellerUtility.DefaultParmsNow(incident.category, map);
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
                Log.Warn($"ScenarioProgression: arrival mode '{arrivalMode}' not found, letting vanilla pick.");
            } else {
                parms.raidArrivalMode = mode;
            }
        }

        // Enemy raids refuse to fire for a non-hostile faction, so force hostility first or the beat produces nothing.
        if (!friendly && !attacker.HostileTo(Faction.OfPlayer)) {
            attacker.TryAffectGoodwillWith(Faction.OfPlayer, -200, false, false);
        }

        if (!incident.Worker.TryExecute(parms)) {
            Log.Warn($"ScenarioProgression: {incident.defName} refused to fire for '{faction}'.");
            return;
        }

        Log.Info($"ScenarioProgression: sent {parms.points:F0} points of {attacker.Name} at the colony.");
    }

    public override string? Describe() {
        FactionDef? def = DefDatabase<FactionDef>.GetNamedSilentFail(faction);
        if (def == null) return null;

        string army = armyNameKey != null && armyNameKey.Length > 0
            ? armyNameKey.Translate().Resolve()
            : def.label;

        return (friendly ? "CC_Progression_Effect_Allies" : "CC_Progression_Effect_Raid")
            .Translate(army.Named("ARMY")).Resolve();
    }
}
