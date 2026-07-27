using Verse;

namespace Cosmere.Core.ScenarioPart.Action;

public class AddGeneAction : ProgressionAction {
    public string gene = string.Empty;
    public string pawnName = string.Empty;
    public bool xenogene = true;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = comp.FindPawnByName(pawnName);
        if (pawn == null) {
            Logger.Warning($"ScenarioProgression: Pawn '{pawnName}' not found for AddGene");
            return;
        }

        GeneDef? geneDef = DefDatabase<GeneDef>.GetNamedSilentFail(gene);
        if (geneDef == null) {
            Logger.Warning($"ScenarioProgression: Gene '{gene}' not found");
            return;
        }

        if (pawn.genes != null && !pawn.genes.HasActiveGene(geneDef)) {
            pawn.genes.AddGene(geneDef, xenogene);
        }
    }
}
