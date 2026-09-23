using System;
using System.Collections.Generic;
using Cosmere.Core.Patch;
using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart.Action;

/// <summary>
///     Brings a faction into the world at the moment the story says it exists, rather than
///     having it sitting on the world map since day one waiting to become relevant.
///     <para>
///         Prefers an existing pawn as its leader. Someone who walked out of the colony earlier
///         in the campaign is passed to the world rather than deleted, so a beat that turns them
///         into the enemy can hand back the same person instead of a stranger wearing the name.
///     </para>
/// </summary>
public class CreateFactionAction : ProgressionAction {
    public string faction = string.Empty;

    /// <summary>Where the new faction stands with the player. Hostile by default.</summary>
    public int goodwill = -100;

    /// <summary>Who leads it. Searched for on the maps, in caravans, then among world pawns.</summary>
    public string leaderPawnName = string.Empty;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        FactionDef? def = DefDatabase<FactionDef>.GetNamedSilentFail(faction);
        if (def == null) {
            Log.Warn($"ScenarioProgression: FactionDef '{faction}' not found for CreateFaction");
            return;
        }

        Faction? made = Find.FactionManager.FirstFactionOfDef(def);
        if (made == null) {
            try {
                FactionGeneratorPatch.CreateScripted(def);
            } catch (Exception ex) {
                Log.Warn($"ScenarioProgression: could not create faction '{faction}': {ex}");
                return;
            }

            made = Find.FactionManager.FirstFactionOfDef(def);
            if (made == null) {
                Log.Warn($"ScenarioProgression: faction '{faction}' still absent after creation.");
                return;
            }
        }

        Faction? player = Faction.OfPlayer;
        if (player != null && made != player) {
            int delta = goodwill - player.GoodwillWith(made);
            if (delta != 0) player.TryAffectGoodwillWith(made, delta, false, false);
        }

        InstallLeader(made, comp);
        Log.Info($"ScenarioProgression: '{faction}' now exists, led by {made.leader?.Name?.ToStringShort ?? "nobody"}.");
    }

    private void InstallLeader(Faction made, GameComponent_ScenarioProgression comp) {
        if (leaderPawnName.Length == 0) return;

        Pawn? leader = comp.FindPawnByName(leaderPawnName) ?? FindWorldPawn(leaderPawnName);
        if (leader == null || leader.Dead) {
            // Nobody by that name left. Vanilla's generated leader is better than none at all.
            if (made.leader == null) made.TryGenerateNewLeader();
            return;
        }

        if (leader.Faction != made) leader.SetFaction(made);
        made.leader = leader;
    }

    private static Pawn? FindWorldPawn(string firstName) {
        List<Pawn> pawns = Find.WorldPawns.AllPawnsAlive;
        for (int i = 0; i < pawns.Count; i++) {
            Pawn pawn = pawns[i];
            if (pawn.Name is NameTriple triple && triple.First == firstName) return pawn;
            if (pawn.Name is NameSingle single && single.Name.StartsWith(firstName)) return pawn;
        }

        return null;
    }

    public override string? Describe() {
        FactionDef? def = DefDatabase<FactionDef>.GetNamedSilentFail(faction);
        return def == null
            ? null
            : "CC_Progression_Effect_Faction".Translate(def.label.Named("FACTION")).Resolve();
    }
}
