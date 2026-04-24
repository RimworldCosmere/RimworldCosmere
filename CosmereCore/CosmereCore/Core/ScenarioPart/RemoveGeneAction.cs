using Verse;

namespace Cosmere.Core.ScenarioPart;

public class RemoveGeneAction : ProgressionAction {
    public string gene = "";
    public string pawnName = "";

    public override void Execute(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = comp.FindPawnByName(pawnName);
        if (pawn == null) {
            Logger.Warning($"ScenarioProgression: Pawn '{pawnName}' not found for RemoveGene");
            return;
        }

        GeneDef? geneDef = DefDatabase<GeneDef>.GetNamedSilentFail(gene);
        if (geneDef == null) {
            Logger.Warning($"ScenarioProgression: Gene '{gene}' not found");
            return;
        }

        if (pawn.genes == null) return;

        Verse.Gene? activeGene = pawn.genes.GetGene(geneDef);
        if (activeGene != null) {
            pawn.genes.RemoveGene(activeGene);
        }
    }
}