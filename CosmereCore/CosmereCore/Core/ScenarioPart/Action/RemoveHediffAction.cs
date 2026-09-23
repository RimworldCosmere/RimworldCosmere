using Verse;

namespace Cosmere.Core.ScenarioPart.Action;

/// <summary>Lifts a hediff back off someone. The other half of AddHediffAction.</summary>
public class RemoveHediffAction : ProgressionAction {
    public string hediff = string.Empty;
    public string pawnName = string.Empty;
    public bool optional;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        HediffDef? def = DefDatabase<HediffDef>.GetNamedSilentFail(hediff);
        if (def == null) {
            Log.Warn($"ScenarioProgression: HediffDef '{hediff}' not found for RemoveHediff");
            return;
        }

        Pawn? pawn = comp.FindPawnByName(pawnName);
        if (pawn == null) {
            if (!optional) Log.Warn($"ScenarioProgression: Pawn '{pawnName}' not found for RemoveHediff");
            return;
        }

        Verse.Hediff? found = pawn.health.hediffSet.GetFirstHediffOfDef(def);
        if (found != null) pawn.health.RemoveHediff(found);
    }
}
