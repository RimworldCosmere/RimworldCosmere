using Verse;

namespace Cosmere.Core.ScenarioPart;

public class AddGeneAction : ProgressionAction {
    public string pawnName = "";
    public string gene = "";
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
