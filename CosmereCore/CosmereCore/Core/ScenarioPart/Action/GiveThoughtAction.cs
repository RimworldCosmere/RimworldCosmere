using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart.Action;

/// <summary>
///     Gives a mood memory to one named pawn or to the whole colony. Set permanent when the
///     beat is meant to mark a pawn for the rest of the campaign - Thought_Memory only expires
///     on its def's durationDays when that flag is false.
/// </summary>
public class GiveThoughtAction : ProgressionAction {
    /// <summary>Leave empty to give it to every free colonist instead of one pawn.</summary>
    public string pawnName = string.Empty;

    public bool permanent;
    public string thought = string.Empty;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        ThoughtDef? def = DefDatabase<ThoughtDef>.GetNamedSilentFail(thought);
        if (def == null) {
            Logger.Warning($"ScenarioProgression: ThoughtDef '{thought}' not found for GiveThought");
            return;
        }

        if (pawnName.Length > 0) {
            Pawn? pawn = comp.FindPawnByName(pawnName);
            if (pawn == null) {
                Logger.Warning($"ScenarioProgression: Pawn '{pawnName}' not found for GiveThought");
                return;
            }

            Give(pawn, def);
            return;
        }

        List<Verse.Map> maps = Find.Maps;
        for (int i = 0; i < maps.Count; i++) {
            List<Pawn> colonists = maps[i].mapPawns.FreeColonists;
            for (int j = 0; j < colonists.Count; j++) {
                Give(colonists[j], def);
            }
        }
    }

    private void Give(Pawn pawn, ThoughtDef def) {
        MemoryThoughtHandler? memories = pawn.needs?.mood?.thoughts?.memories;
        if (memories == null) return;

        Thought_Memory memory = (Thought_Memory)ThoughtMaker.MakeThought(def);
        memory.permanent = permanent;
        memories.TryGainMemory(memory);
    }

    public override string? Describe() {
        ThoughtDef? def = DefDatabase<ThoughtDef>.GetNamedSilentFail(thought);
        if (def?.stages == null || def.stages.Count == 0) return null;

        string who = pawnName.Length > 0 ? pawnName : "CC_Progression_Effect_WholeColony".Translate().Resolve();
        return "CC_Progression_Effect_Thought".Translate(
            who.Named("PAWN"),
            def.stages[0].label.Named("THOUGHT")
        ).Resolve();
    }
}
